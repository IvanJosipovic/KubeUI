using System.Reflection;
using k8s;
using k8s.KubeConfigModels;
using k8s.Models;
using Microsoft.Extensions.DependencyInjection;
using KubeUI.Testing.Kubernetes.Infrastructure;
using KubeUI.Testing.Kubernetes.Scenarios;
using KubeUI.Testing.Kubernetes.Transport;

namespace KubeUI.Testing.Kubernetes.Bootstrap;

public sealed class TestClusterGenerator
{
    private readonly IServiceProvider _services;
    private readonly bool _ownsServices;
    private readonly List<TestCluster> _clusters = [];

    public TestClusterGenerator()
    {
        _services = KubernetesTestServiceProvider.Build(new KubernetesTestSettingsStore());
        _ownsServices = true;
    }

    public TestClusterGenerator(IServiceProvider services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public async Task<TestCluster> CreateAsync(
        TestClusterConfig config,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(config);
        cancellationToken.ThrowIfCancellationRequested();

        var cluster = config.Type switch
        {
            KubernetesBackend.Fake => await CreateFakeAsync(config, cancellationToken).ConfigureAwait(false),
            KubernetesBackend.Kind => config.KubeConfig is null
                ? await CreateKindAsync(config, cancellationToken).ConfigureAwait(false)
                : await CreateNamedAsync(config, cancellationToken).ConfigureAwait(false),
            _ => throw new ArgumentOutOfRangeException(nameof(config), config.Type, "Unknown test cluster type."),
        };

        lock (_clusters)
        {
            _clusters.Add(cluster);
        }

        return cluster;
    }

    public async ValueTask ResetAsync()
    {
        TestCluster[] clusters;
        lock (_clusters)
        {
            clusters = [.. _clusters];
            _clusters.Clear();
        }

        for (var index = clusters.Length - 1; index >= 0; index--)
        {
            await clusters[index].DisposeAsync().ConfigureAwait(false);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Reliability",
        "CA2000",
        Justification = "Client and fake API ownership transfers to the returned TestCluster.")]
    private async Task<TestCluster> CreateFakeAsync(TestClusterConfig config, CancellationToken cancellationToken)
    {
        var api = new FakeKubernetesHttpApi()
        {
            UseRoleBasedAuthorization = config.AuthenticatedUser.StartsWith("system:serviceaccount:", StringComparison.Ordinal),
            AuthenticatedUser = config.AuthenticatedUser,
        };

        foreach (IKubernetesObject resource in config.InitialResources)
        {
            api.Add(resource);
        }

        if (!string.IsNullOrWhiteSpace(config.InitialYaml))
        {
            api.AddYaml(config.InitialYaml);
        }

        var kubeConfig = CreateFakeKubeConfig();
        var clientConfig = KubernetesClientConfiguration.BuildConfigFromConfigObject(
            kubeConfig,
            kubeConfig.CurrentContext,
            masterUrl: "http://fake-kubernetes");
        var handlers = CreateHttpHandlers(config, out _);
        var client = CreateClient(clientConfig, api, handlers);
        var cluster = await CreateTestClusterAsync(client, kubeConfig, clientConfig, config, cancellationToken, _ =>
        {
            api.Shutdown();
            return Task.CompletedTask;
        }, api).ConfigureAwait(false);
        cluster.Cluster.KubernetesClientFactory = configuration =>
        {
            var reconnectHandlers = CreateHttpHandlers(config, out _);
            return CreateClient(configuration, api, reconnectHandlers);
        };
        return cluster;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Reliability",
        "CA2000",
        Justification = "Client ownership transfers to the returned TestCluster.")]
    private async Task<TestCluster> CreateNamedAsync(TestClusterConfig config, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(config.Name))
        {
            throw new ArgumentException("A named kubeconfig cluster requires Name.", nameof(config));
        }

        var kubeConfig = config.KubeConfig
            ?? throw new ArgumentException("A named kubeconfig cluster requires KubeConfig.", nameof(config));
        var clientConfig = KubernetesClientConfiguration.BuildConfigFromConfigObject(
            kubeConfig,
            config.Name,
            masterUrl: null);
        clientConfig.FirstMessageHandlerSetup = config.FirstMessageHandlerSetup;
        var handlers = CreateHttpHandlers(config, out _);
        var client = CreateClient(clientConfig, terminalHandler: null, handlers);
        try
        {
            await ApplyResourcesAsync(client, config.InitialResources, cancellationToken).ConfigureAwait(false);
            await ApplyInitialYamlAsync(client, config.InitialYaml, cancellationToken).ConfigureAwait(false);
            client.Dispose();
            ApplyImpersonation(kubeConfig, config.AuthenticatedUser);
            clientConfig = KubernetesClientConfiguration.BuildConfigFromConfigObject(kubeConfig, config.Name, masterUrl: null);
            clientConfig.FirstMessageHandlerSetup = config.FirstMessageHandlerSetup;
            handlers = CreateHttpHandlers(config, out _);
            client = CreateClient(clientConfig, terminalHandler: null, handlers);
            return await CreateTestClusterAsync(client, kubeConfig, clientConfig, config, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            client.Dispose();
            throw;
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Reliability",
        "CA2000",
        Justification = "Client ownership transfers to the returned TestCluster.")]
    private async Task<TestCluster> CreateKindAsync(TestClusterConfig config, CancellationToken cancellationToken)
    {
        var name = config.Name ?? $"kubeui-test-{Guid.NewGuid():N}";
        var kubeConfigPath = Path.Combine(Path.GetTempPath(), $"{name}-{Guid.NewGuid():N}.yaml");

        try
        {
            await Kind.DownloadClient(cancellationToken).ConfigureAwait(false);
            await Kind.CreateCluster(name, kubeConfigPath: kubeConfigPath, cancellationToken: cancellationToken).ConfigureAwait(false);

            var kubeConfig = await Kind.GetK8SConfiguration(name, cancellationToken).ConfigureAwait(false);
            var clientConfig = KubernetesClientConfiguration.BuildConfigFromConfigObject(kubeConfig, null, null);
            clientConfig.FirstMessageHandlerSetup = config.FirstMessageHandlerSetup;
            var handlers = CreateHttpHandlers(config, out _, includeConfiguredHandlers: false);
            var client = CreateClient(clientConfig, terminalHandler: null, handlers);
            try
            {
                await ApplyResourcesAsync(client, config.InitialResources, cancellationToken).ConfigureAwait(false);
                await ApplyInitialYamlAsync(client, config.InitialYaml, cancellationToken).ConfigureAwait(false);
                client.Dispose();
                ApplyImpersonation(kubeConfig, config.AuthenticatedUser);
                clientConfig = KubernetesClientConfiguration.BuildConfigFromConfigObject(kubeConfig, null, null);
                clientConfig.FirstMessageHandlerSetup = config.FirstMessageHandlerSetup;
                handlers = CreateHttpHandlers(config, out _);
                client = CreateClient(clientConfig, terminalHandler: null, handlers);
                return await CreateTestClusterAsync(
                    client,
                    kubeConfig,
                    clientConfig,
                    config,
                    cancellationToken,
                    token => CleanupKindAsync(name, kubeConfigPath, token)).ConfigureAwait(false);
            }
            catch
            {
                client.Dispose();
                throw;
            }
        }
        catch
        {
            await CleanupKindAsync(name, kubeConfigPath, CancellationToken.None).ConfigureAwait(false);
            throw;
        }
    }

    private async Task<TestCluster> CreateTestClusterAsync(
        IKubernetes client,
        K8SConfiguration kubeConfig,
        KubernetesClientConfiguration clientConfiguration,
        TestClusterConfig config,
        CancellationToken cancellationToken,
        Func<CancellationToken, Task>? cleanup = null,
        FakeKubernetesHttpApi? fakeApi = null)
    {
        _ = config;
        _ = cancellationToken;
        var cluster = _services.GetRequiredService<KubeUI.Kubernetes.Cluster>();
        cluster.Name = clientConfiguration.CurrentContext ?? "test";
        cluster.KubeConfig = kubeConfig;
        cluster.KubeConfigPath = string.Empty;
        cluster.KubernetesClientFactory = _ => client;

            var finalCleanup = cleanup;
            if (_ownsServices)
            {
                finalCleanup = async token =>
                {
                    if (cleanup is not null)
                    {
                        await cleanup(token).ConfigureAwait(false);
                    }

                    if (_services is IAsyncDisposable disposable)
                    {
                        await disposable.DisposeAsync().ConfigureAwait(false);
                    }
                };
            }

            return new TestCluster(client, kubeConfig, clientConfiguration, cluster, _services, finalCleanup, fakeApi);
    }

    private static IKubernetes CreateClient(
        KubernetesClientConfiguration configuration,
        DelegatingHandler? terminalHandler,
        IReadOnlyCollection<DelegatingHandler> handlers)
    {
        if (terminalHandler is null)
        {
            return new k8s.Kubernetes(configuration, handlers.ToArray());
        }

        var pipeline = terminalHandler;
        for (var index = handlers.Count - 1; index >= 0; index--)
        {
            var handler = handlers.ElementAt(index);
            if (handler.InnerHandler is not null)
            {
                throw new InvalidOperationException("A test HTTP handler must not already have an InnerHandler.");
            }

            handler.InnerHandler = pipeline;
            pipeline = handler;
        }

        return pipeline is null
            ? new k8s.Kubernetes(configuration)
            : new k8s.Kubernetes(configuration, pipeline);
    }

    private static IReadOnlyCollection<DelegatingHandler> CreateHttpHandlers(
        TestClusterConfig config,
        out TestConditionHandler conditionHandler,
        bool includeConfiguredHandlers = true)
    {
        conditionHandler = new TestConditionHandler(config.EffectiveResponseLatency, config.ThrowOnConnect);
        List<DelegatingHandler> handlers = [conditionHandler];
        if (includeConfiguredHandlers)
        {
            handlers.AddRange(config.HttpHandlers);
        }

        return handlers;
    }

    private static async Task ApplyInitialYamlAsync(
        IKubernetes client,
        string? yaml,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(yaml))
        {
            return;
        }

        await ApplyResourcesAsync(
            client,
            KubeUI.Kubernetes.Serialization.KubernetesYaml.LoadAllFromString(yaml).Cast<IKubernetesObject>(),
            cancellationToken).ConfigureAwait(false);
    }

    private static void ApplyImpersonation(K8SConfiguration kubeConfig, string authenticatedUser)
    {
        if (string.Equals(authenticatedUser, "system:admin", StringComparison.Ordinal))
        {
            return;
        }

        var currentContext = kubeConfig.CurrentContext
            ?? throw new InvalidOperationException("The kubeconfig has no current context.");
        var context = kubeConfig.Contexts.First(item => item.Name == currentContext);
        var user = kubeConfig.Users.First(item => item.Name == context.ContextDetails.User);
        user.UserCredentials.Impersonate = authenticatedUser;
    }

    private static async Task CleanupKindAsync(string name, string kubeConfigPath, CancellationToken cancellationToken)
    {
        try
        {
            await Kind.DeleteCluster(name, kubeConfigPath, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            try
            {
                File.Delete(kubeConfigPath);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }

    private static async Task ApplyResourcesAsync(
        IKubernetes client,
        IEnumerable<IKubernetesObject> resources,
        CancellationToken cancellationToken)
    {
        var createMethod = typeof(TestClusterGenerator)
            .GetMethod(nameof(CreateResourceAsync), BindingFlags.Static | BindingFlags.NonPublic)!;

        foreach (var resource in resources)
        {
            var createTask = (Task)createMethod
                .MakeGenericMethod(resource.GetType())
                .Invoke(null, [client, resource, cancellationToken])!;
            await createTask.ConfigureAwait(false);
        }
    }

    private static async Task CreateResourceAsync<T>(
        IKubernetes client,
        T resource,
        CancellationToken cancellationToken)
        where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        using var genericClient = client.GetGenericClient<T>();
        if (string.IsNullOrEmpty(resource.Namespace()))
        {
            await genericClient.CreateAsync(resource).WaitAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        await genericClient.CreateNamespacedAsync(resource, resource.Namespace(), cancellationToken).ConfigureAwait(false);
    }

    private static K8SConfiguration CreateFakeKubeConfig()
        => new()
        {
            ApiVersion = "v1",
            Kind = "Config",
            CurrentContext = "fake",
            Clusters =
            [
                new k8s.KubeConfigModels.Cluster
                {
                    Name = "fake-cluster",
                    ClusterEndpoint = new ClusterEndpoint { Server = "http://fake-kubernetes" },
                },
            ],
            Users =
            [new User { Name = "fake-user", UserCredentials = new UserCredentials { Token = "fake-token" } }],
            Contexts =
            [new Context { Name = "fake", ContextDetails = new ContextDetails { Cluster = "fake-cluster", User = "fake-user" } }],
        };
}
