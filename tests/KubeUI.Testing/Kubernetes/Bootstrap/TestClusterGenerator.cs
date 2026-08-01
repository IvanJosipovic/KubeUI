using System.Reflection;
using k8s;
using k8s.KubeConfigModels;
using k8s.Models;
using Microsoft.Extensions.DependencyInjection;
using KubeUI.Testing.Kubernetes.Infrastructure;
using KubeUI.Testing.Kubernetes.Scenarios;
using KubeUI.Testing.Kubernetes.Transport;

namespace KubeUI.Testing.Kubernetes.Bootstrap;

public sealed class TestClusterGenerator : IAsyncDisposable
{
    private readonly IServiceProvider _services;
    private readonly bool _ownsServices;
    private readonly List<TestCluster> _clusters = [];
    private int _disposed;

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

        TestCluster cluster = config.Type switch
        {
            KubernetesBackend.Fake => await CreateFakeAsync(config, cancellationToken).ConfigureAwait(false),
            KubernetesBackend.Kind => config.KubeConfig is null
                ? await CreateKindAsync(config, cancellationToken).ConfigureAwait(false)
                : await CreateNamedAsync(config, cancellationToken).ConfigureAwait(false),
            _ => throw new ArgumentOutOfRangeException(nameof(config), config.Type, "Unknown test cluster type."),
        };

        if (Interlocked.CompareExchange(ref _disposed, 0, 0) != 0)
        {
            await cluster.DisposeAsync().ConfigureAwait(false);
            throw new ObjectDisposedException(nameof(TestClusterGenerator));
        }

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

        for (int index = clusters.Length - 1; index >= 0; index--)
        {
            await clusters[index].DisposeAsync().ConfigureAwait(false);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        await ResetAsync().ConfigureAwait(false);

        if (_ownsServices && _services is IAsyncDisposable disposable)
        {
            await disposable.DisposeAsync().ConfigureAwait(false);
        }
    }

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

        K8SConfiguration kubeConfig = CreateFakeKubeConfig();
        KubernetesClientConfiguration clientConfig = KubernetesClientConfiguration.BuildConfigFromConfigObject(
            kubeConfig,
            kubeConfig.CurrentContext,
            masterUrl: "http://fake-kubernetes");
        IReadOnlyCollection<DelegatingHandler> handlers = CreateHttpHandlers(config, out TestConditionHandler conditionHandler);
        IKubernetes client = CreateClient(clientConfig, api, handlers);
        TestCluster cluster = await CreateTestClusterAsync(client, kubeConfig, clientConfig, config, cancellationToken, _ =>
        {
            api.Shutdown();
            return Task.CompletedTask;
        }, api, conditionHandler).ConfigureAwait(false);
        cluster.Cluster.KubernetesClientFactory = configuration =>
        {
            IReadOnlyCollection<DelegatingHandler> reconnectHandlers = CreateHttpHandlers(config, out _);
            return CreateClient(configuration, api, reconnectHandlers);
        };
        return cluster;
    }

    private async Task<TestCluster> CreateNamedAsync(TestClusterConfig config, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(config.Name))
        {
            throw new ArgumentException("A named kubeconfig cluster requires Name.", nameof(config));
        }

        K8SConfiguration kubeConfig = config.KubeConfig
            ?? throw new ArgumentException("A named kubeconfig cluster requires KubeConfig.", nameof(config));
        KubernetesClientConfiguration clientConfig = KubernetesClientConfiguration.BuildConfigFromConfigObject(
            kubeConfig,
            config.Name,
            masterUrl: null);
        clientConfig.FirstMessageHandlerSetup = config.FirstMessageHandlerSetup;
        IReadOnlyCollection<DelegatingHandler> handlers = CreateHttpHandlers(config, out TestConditionHandler conditionHandler);
        IKubernetes client = CreateClient(clientConfig, terminalHandler: null, handlers);
        try
        {
            await ApplyResourcesAsync(client, config.InitialResources, cancellationToken).ConfigureAwait(false);
            await ApplyInitialYamlAsync(client, config.InitialYaml, cancellationToken).ConfigureAwait(false);
            client.Dispose();
            ApplyImpersonation(kubeConfig, config.AuthenticatedUser);
            clientConfig = KubernetesClientConfiguration.BuildConfigFromConfigObject(kubeConfig, config.Name, masterUrl: null);
            clientConfig.FirstMessageHandlerSetup = config.FirstMessageHandlerSetup;
            handlers = CreateHttpHandlers(config, out conditionHandler);
            client = CreateClient(clientConfig, terminalHandler: null, handlers);
            return await CreateTestClusterAsync(client, kubeConfig, clientConfig, config, cancellationToken, conditionHandler: conditionHandler).ConfigureAwait(false);
        }
        catch
        {
            client.Dispose();
            throw;
        }
    }

    private async Task<TestCluster> CreateKindAsync(TestClusterConfig config, CancellationToken cancellationToken)
    {
        string name = config.Name ?? $"kubeui-test-{Guid.NewGuid():N}";
        string kubeConfigPath = Path.Combine(Path.GetTempPath(), $"{name}-{Guid.NewGuid():N}.yaml");

        await Kind.DownloadClient(cancellationToken).ConfigureAwait(false);
        await Kind.CreateCluster(name, kubeConfigPath: kubeConfigPath, cancellationToken: cancellationToken).ConfigureAwait(false);

        try
        {
            K8SConfiguration kubeConfig = await Kind.GetK8SConfiguration(name, cancellationToken).ConfigureAwait(false);
            KubernetesClientConfiguration clientConfig = KubernetesClientConfiguration.BuildConfigFromConfigObject(kubeConfig, null, null);
            clientConfig.FirstMessageHandlerSetup = config.FirstMessageHandlerSetup;
            IReadOnlyCollection<DelegatingHandler> handlers = CreateHttpHandlers(config, out TestConditionHandler conditionHandler, includeConfiguredHandlers: false);
            IKubernetes client = CreateClient(clientConfig, terminalHandler: null, handlers);
            try
            {
                await ApplyResourcesAsync(client, config.InitialResources, cancellationToken).ConfigureAwait(false);
                await ApplyInitialYamlAsync(client, config.InitialYaml, cancellationToken).ConfigureAwait(false);
                client.Dispose();
                ApplyImpersonation(kubeConfig, config.AuthenticatedUser);
                clientConfig = KubernetesClientConfiguration.BuildConfigFromConfigObject(kubeConfig, null, null);
                clientConfig.FirstMessageHandlerSetup = config.FirstMessageHandlerSetup;
                handlers = CreateHttpHandlers(config, out conditionHandler);
                client = CreateClient(clientConfig, terminalHandler: null, handlers);
                return await CreateTestClusterAsync(
                    client,
                    kubeConfig,
                    clientConfig,
                    config,
                    cancellationToken,
                    token => CleanupKindAsync(name, kubeConfigPath, token),
                    conditionHandler: conditionHandler).ConfigureAwait(false);
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
        FakeKubernetesHttpApi? fakeApi = null,
        TestConditionHandler? conditionHandler = null)
    {
        var cluster = _services.GetRequiredService<KubeUI.Kubernetes.Cluster>();
        cluster.Name = clientConfiguration.CurrentContext ?? "test";
        cluster.KubeConfig = kubeConfig;
        cluster.KubeConfigPath = string.Empty;
        cluster.KubernetesClientFactory = _ => client;
        conditionHandler?.Enable();

            Func<CancellationToken, Task>? finalCleanup = cleanup;
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

        DelegatingHandler? pipeline = terminalHandler;
        for (int index = handlers.Count - 1; index >= 0; index--)
        {
            DelegatingHandler handler = handlers.ElementAt(index);
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

        string currentContext = kubeConfig.CurrentContext
            ?? throw new InvalidOperationException("The kubeconfig has no current context.");
        Context context = kubeConfig.Contexts.First(item => item.Name == currentContext);
        User user = kubeConfig.Users.First(item => item.Name == context.ContextDetails.User);
        user.UserCredentials.Impersonate = authenticatedUser;
    }

    private static async Task CleanupKindAsync(string name, string kubeConfigPath, CancellationToken cancellationToken)
    {
        try
        {
            await Kind.DeleteCluster(name, cancellationToken).ConfigureAwait(false);
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
        MethodInfo createMethod = typeof(TestClusterGenerator)
            .GetMethod(nameof(CreateResourceAsync), BindingFlags.Static | BindingFlags.NonPublic)!;

        foreach (IKubernetesObject resource in resources)
        {
            Task createTask = (Task)createMethod
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
