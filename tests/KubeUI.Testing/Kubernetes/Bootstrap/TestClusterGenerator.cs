using System.Reflection;
using k8s;
using k8s.KubeConfigModels;
using k8s.Models;
using KubeUI.Kubernetes.Serialization;
using KubeUI.Testing.Kubernetes.Scenarios;
using Microsoft.Extensions.DependencyInjection;

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
            KubernetesBackend.Live => await CreateLiveAsync(config, cancellationToken).ConfigureAwait(false),
            _ => throw new ArgumentOutOfRangeException(nameof(config), config.Type, "Unknown test cluster type."),
        };

        lock (_clusters)
        {
            _clusters.Add(cluster);
        }

        return cluster;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Reliability",
        "CA2000",
        Justification = "Client ownership transfers to the returned TestCluster.")]
    private async Task<TestCluster> CreateLiveAsync(TestClusterConfig config, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(config.Name))
        {
            throw new ArgumentException("A live kubeconfig cluster requires Name.", nameof(config));
        }

        K8SConfiguration kubeConfig;
        try
        {
            kubeConfig = KubernetesClientConfiguration.LoadKubeConfig(
                kubeconfigPath: null,
                useRelativePaths: true);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or k8s.Exceptions.KubeConfigException)
        {
            throw new InvalidOperationException(
                $"Unable to load the default host kubeconfig for live cluster '{config.Name}'.",
                exception);
        }

        KubernetesClientConfiguration clientConfig;
        try
        {
            clientConfig = LiveKubernetesConfiguration.CreateClientConfiguration(
                kubeConfig,
                config.Name);
        }
        catch (Exception exception) when (exception is InvalidOperationException or ArgumentException or k8s.Exceptions.KubeConfigException)
        {
            throw new InvalidOperationException(
                $"Unable to create a Kubernetes client for live cluster '{config.Name}'.",
                exception);
        }

        clientConfig.FirstMessageHandlerSetup = config.FirstMessageHandlerSetup;
        var client = new k8s.Kubernetes(clientConfig);
        TestCluster? testCluster = null;
        try
        {
            testCluster = await CreateTestClusterAsync(
                client,
                kubeConfig,
                clientConfig,
                config,
                cancellationToken,
                clientFactory: configuration => new k8s.Kubernetes(configuration)).ConfigureAwait(false);
            await testCluster.Cluster.Connect().WaitAsync(cancellationToken).ConfigureAwait(false);
            return testCluster;
        }
        catch
        {
            if (testCluster is not null)
            {
                await testCluster.DisposeAsync().ConfigureAwait(false);
            }
            else
            {
                client.Dispose();
            }

            throw;
        }
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

        if (_ownsServices && _services is IAsyncDisposable disposable)
            await disposable.DisposeAsync().ConfigureAwait(false);
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

        RegisterKubernetesModels(api, _services.GetRequiredService<KubernetesModelCatalog>());
        api.Register<V1ConfigMap>();
        api.Register<V1Pod>();
        api.Register<V1CustomResourceDefinition>();
        api.Register<V1Service>();
        api.Register<V1ServiceAccount>();
        api.Register<V1PersistentVolume>();
        api.Register<V1PersistentVolumeClaim>();
        api.Register<V1StorageClass>();
        api.Register<V1Deployment>();
        api.Register<V1ReplicaSet>();
        api.Register<V1StatefulSet>();
        api.Register<V1DaemonSet>();
        api.Register<V1Job>();
        api.Register<V1CronJob>();
        api.Register<V1EndpointSlice>();
        api.Register<Corev1Event>();
        api.Register<V1Role>();
        api.Register<V1RoleBinding>();
        api.Register<V1ClusterRole>();
        api.Register<V1ClusterRoleBinding>();

        foreach (IKubernetesObject resource in config.InitialResources)
        {
            api.Add(resource);
        }

        if (!string.IsNullOrWhiteSpace(config.InitialYaml))
        {
            api.AddYaml(config.InitialYaml, _services.GetRequiredService<KubernetesModelCatalog>().GetYamlTypeMap());
        }

        var kubeConfig = CreateFakeKubeConfig(config.Name ?? "fake");
        var clientConfig = KubernetesClientConfiguration.BuildConfigFromConfigObject(
            kubeConfig,
            kubeConfig.CurrentContext,
            masterUrl: "http://fake-kubernetes");
        var handlers = CreateHttpHandlers(config, out _);
        var client = CreateClient(clientConfig, api, handlers);
        var adminClient = CreateClient(
            clientConfig,
            api.CreateClient("system:admin", useRoleBasedAuthorization: false),
            []);
        var cluster = await CreateTestClusterAsync(client, kubeConfig, clientConfig, config, cancellationToken, _ =>
        {
            api.Shutdown();
            return Task.CompletedTask;
        }, api, adminClient: adminClient).ConfigureAwait(false);
        cluster.Cluster.KubernetesClientFactory = configuration =>
        {
            var reconnectHandlers = CreateHttpHandlers(config, out _);
            return CreateClient(configuration, api, reconnectHandlers);
        };
        return cluster;
    }

    private static void RegisterKubernetesModels(FakeKubernetesHttpApi api, KubernetesModelCatalog catalog)
    {
        foreach (var yamlKey in catalog.GetYamlTypeMap().Keys)
        {
            var segments = yamlKey.Split('/');
            var group = segments.Length == 3 ? segments[0] : string.Empty;
            var version = segments[^2];
            var kind = segments[^1];
            if (IsBuiltInApiGroup(group)
                && catalog.TryGetResourceKind(group, version, kind, out var resourceKind))
            {
                api.Register(resourceKind);
            }
        }
    }

    private static bool IsBuiltInApiGroup(string group)
        => group is ""
            or "admissionregistration.k8s.io"
            or "apiextensions.k8s.io"
            or "apiregistration.k8s.io"
            or "apps"
            or "authentication.k8s.io"
            or "authorization.k8s.io"
            or "autoscaling"
            or "batch"
            or "certificates.k8s.io"
            or "coordination.k8s.io"
            or "discovery.k8s.io"
            or "events.k8s.io"
            or "flowcontrol.apiserver.k8s.io"
            or "networking.k8s.io"
            or "node.k8s.io"
            or "policy"
            or "rbac.authorization.k8s.io"
            or "resource.k8s.io"
            or "scheduling.k8s.io"
            or "storage.k8s.io";

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
        var adminClient = CreateClient(clientConfig, terminalHandler: null, handlers);
        var client = adminClient;
        try
        {
            await ApplyResourcesAsync(adminClient, config.InitialResources, cancellationToken).ConfigureAwait(false);
            await ApplyInitialYamlAsync(adminClient, config.InitialYaml, _services.GetRequiredService<KubernetesModelCatalog>().GetYamlTypeMap(), cancellationToken).ConfigureAwait(false);
            ApplyImpersonation(kubeConfig, config.AuthenticatedUser);
            clientConfig = KubernetesClientConfiguration.BuildConfigFromConfigObject(kubeConfig, config.Name, masterUrl: null);
            clientConfig.FirstMessageHandlerSetup = config.FirstMessageHandlerSetup;
            handlers = CreateHttpHandlers(config, out _);
            client = CreateClient(clientConfig, terminalHandler: null, handlers);
            return await CreateTestClusterAsync(
                client,
                kubeConfig,
                clientConfig,
                config,
                cancellationToken,
                clientFactory: configuration => CreateClient(
                    configuration,
                    terminalHandler: null,
                    CreateHttpHandlers(config, out _)),
                adminClient: adminClient).ConfigureAwait(false);
        }
        catch
        {
            if (!ReferenceEquals(client, adminClient))
            {
                client.Dispose();
            }

            adminClient.Dispose();
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
            var handlers = CreateHttpHandlers(
                config,
                out _,
                includeConfiguredHandlers: false);
            var adminClient = CreateClient(clientConfig, terminalHandler: null, handlers);
            var client = adminClient;
            try
            {
                await ApplyResourcesAsync(adminClient, config.InitialResources, cancellationToken).ConfigureAwait(false);
                await ApplyInitialYamlAsync(adminClient, config.InitialYaml, _services.GetRequiredService<KubernetesModelCatalog>().GetYamlTypeMap(), cancellationToken).ConfigureAwait(false);
                if (!string.Equals(config.AuthenticatedUser, "system:admin", StringComparison.Ordinal))
                {
                    var (namespaceName, serviceAccountName) = ParseServiceAccount(config.AuthenticatedUser);
                    var tokenResponse = await adminClient.CoreV1.CreateNamespacedServiceAccountTokenAsync(
                        new Authenticationv1TokenRequest
                        {
                            ApiVersion = "authentication.k8s.io/v1",
                            Kind = "TokenRequest",
                            Spec = new V1TokenRequestSpec
                            {
                                ExpirationSeconds = 3600,
                            },
                        },
                        serviceAccountName,
                        namespaceName,
                        cancellationToken: cancellationToken).ConfigureAwait(false);
                    SetServiceAccountToken(kubeConfig, tokenResponse.Status?.Token
                        ?? throw new InvalidOperationException("Kind returned an empty service-account token."));
                }

                clientConfig = KubernetesClientConfiguration.BuildConfigFromConfigObject(kubeConfig, null, null);
                clientConfig.FirstMessageHandlerSetup = config.FirstMessageHandlerSetup;
                client = CreateClient(
                    clientConfig,
                    terminalHandler: null,
                    CreateHttpHandlers(config, out _, includeHandlerFactory: false));

                return await CreateTestClusterAsync(
                    client,
                    kubeConfig,
                    clientConfig,
                    config,
                    cancellationToken,
                    token => CleanupKindAsync(name, kubeConfigPath, token),
                    clientFactory: configuration => CreateClient(
                        configuration,
                        terminalHandler: null,
                        CreateHttpHandlers(config, out _, includeHandlerFactory: false)),
                    adminClient: adminClient).ConfigureAwait(false);
            }
            catch
            {
                if (!ReferenceEquals(client, adminClient))
                {
                    client.Dispose();
                }

                adminClient.Dispose();
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
        Func<KubernetesClientConfiguration, IKubernetes>? clientFactory = null,
        IKubernetes? adminClient = null)
    {
        _ = config;
        _ = cancellationToken;
        var cluster = _services.GetRequiredService<KubeUI.Kubernetes.Cluster>();
        cluster.Name = clientConfiguration.CurrentContext ?? "test";
        cluster.KubeConfig = kubeConfig;
        cluster.KubeConfigPath = string.Empty;
        cluster.KubernetesClientFactory = clientFactory ?? (_ => client);

        var finalCleanup = cleanup;
        if (cleanup is not null)
            finalCleanup = cleanup;

        return new TestCluster(client, kubeConfig, clientConfiguration, cluster, _services, finalCleanup, fakeApi, adminClient);
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
        bool includeConfiguredHandlers = true,
        bool includeHandlerFactory = true)
    {
        conditionHandler = new TestConditionHandler(config.EffectiveResponseLatency, config.ThrowOnConnect);
        List<DelegatingHandler> handlers = [conditionHandler];
        if (includeConfiguredHandlers)
        {
            handlers.AddRange(includeHandlerFactory
                ? config.HttpHandlerFactory?.Invoke() ?? config.HttpHandlers
                : config.HttpHandlers);
        }

        return handlers;
    }

    private async Task ApplyInitialYamlAsync(
        IKubernetes client,
        string? yaml,
        IDictionary<string, Type> typeMap,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(yaml))
        {
            return;
        }

        await ApplyResourcesAsync(
            client,
            _services.GetRequiredService<IKubernetesYamlSerializer>().LoadAllFromString(yaml, key => typeMap[key]).Cast<IKubernetesObject>(),
            cancellationToken).ConfigureAwait(false);
    }

    private static (string Namespace, string Name) ParseServiceAccount(string authenticatedUser)
    {
        const string prefix = "system:serviceaccount:";
        if (!authenticatedUser.StartsWith(prefix, StringComparison.Ordinal))
        {
            throw new ArgumentException("Kind test users must be system:admin or a service account.", nameof(authenticatedUser));
        }

        var parts = authenticatedUser[prefix.Length..].Split(':', 2);
        return parts.Length == 2 && parts.All(static part => !string.IsNullOrWhiteSpace(part))
            ? (parts[0], parts[1])
            : throw new ArgumentException("Kind service-account users must include namespace and name.", nameof(authenticatedUser));
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

    private static void SetServiceAccountToken(K8SConfiguration kubeConfig, string token)
    {
        var currentContext = kubeConfig.CurrentContext
            ?? throw new InvalidOperationException("The kubeconfig has no current context.");
        var context = kubeConfig.Contexts.First(item => item.Name == currentContext);
        var user = kubeConfig.Users.First(item => item.Name == context.ContextDetails.User);
        user.UserCredentials.Impersonate = null;
        user.UserCredentials.Token = token;
        user.UserCredentials.ClientCertificateData = null;
        user.UserCredentials.ClientKeyData = null;
        user.UserCredentials.ClientCertificate = null;
        user.UserCredentials.ClientKey = null;
        user.UserCredentials.UserName = null;
        user.UserCredentials.Password = null;
    }

    private static async Task CleanupKindAsync(string name, string kubeConfigPath, CancellationToken cancellationToken)
    {
        using var cleanupCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cleanupCancellation.CancelAfter(TimeSpan.FromSeconds(30));

        try
        {
            await Kind.DeleteCluster(name, kubeConfigPath, cleanupCancellation.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
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

    private static K8SConfiguration CreateFakeKubeConfig(string contextName)
        => new()
        {
            ApiVersion = "v1",
            Kind = "Config",
            CurrentContext = contextName,
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
            [new Context { Name = contextName, ContextDetails = new ContextDetails { Cluster = "fake-cluster", User = "fake-user" } }],
        };
}
