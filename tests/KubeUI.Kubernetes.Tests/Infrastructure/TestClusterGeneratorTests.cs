using k8s;
using k8s.KubeConfigModels;
using k8s.Models;
using Shouldly;

namespace KubeUI.Kubernetes.Tests.Infrastructure;

public sealed class TestClusterGeneratorTests
{
    [Fact]
    public void FactoryMethodsPreserveOptionalClusterSettings()
    {
        V1Namespace resource = new() { Metadata = new V1ObjectMeta { Name = "seeded" } };
        using RecordingHandler handler = new();
        Action<SocketsHttpHandler> setup = socket => socket.PooledConnectionLifetime = TimeSpan.FromMinutes(2);

        TestClusterConfig fake = TestClusterConfig.Fake(
            [resource],
            [handler],
            TimeSpan.FromSeconds(3),
            throwOnConnect: true,
            firstMessageHandlerSetup: setup);

        fake.Resources.ShouldContain(resource);
        fake.HttpHandlers.ShouldContain(handler);
        fake.ResponseLatency.ShouldBe(TimeSpan.FromSeconds(3));
        fake.ThrowOnConnect.ShouldBeTrue();
        fake.FirstMessageHandlerSetup.ShouldBeSameAs(setup);
    }

    [Theory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task CreatesRealClientAndSeedsDifferentResourceTypes(KubernetesBackend backend)
    {
        var config = CreateConfig(backend,
        [
            new V1Namespace { Metadata = new V1ObjectMeta { Name = "seeded" } },
            new V1Pod
            {
                Metadata = new V1ObjectMeta { Name = "pod", NamespaceProperty = "seeded" },
                Spec = new V1PodSpec { Containers = [new V1Container { Name = "main", Image = "busybox" }] },
            },
        ]);

        await using var cluster = await new TestClusterGenerator().CreateAsync(config, TestContext.Current.CancellationToken);

        cluster.Client.ShouldBeOfType<k8s.Kubernetes>();
        if (backend == KubernetesBackend.Fake)
        {
            cluster.KubeConfig.CurrentContext.ShouldBe("fake");
        }
        else
        {
            cluster.KubeConfig.CurrentContext.ShouldStartWith("kind-");
        }
        var namespaces = await cluster.Client.CoreV1.ListNamespaceAsync(cancellationToken: TestContext.Current.CancellationToken);
        var pods = await cluster.Client.CoreV1.ListNamespacedPodAsync("seeded", cancellationToken: TestContext.Current.CancellationToken);

        namespaces.Items.ShouldContain(item => item.Name() == "seeded");
        pods.Items.ShouldContain(item => item.Name() == "pod");
    }

    [Theory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task AppendsHttpHandlersBeforeTheTransport(KubernetesBackend backend)
    {
        using RecordingHandler handler = new();
        var config = CreateConfig(backend, httpHandlers: [handler]);

        await using var cluster = await new TestClusterGenerator().CreateAsync(config, TestContext.Current.CancellationToken);
        await cluster.Client.CoreV1.ListNamespaceAsync(cancellationToken: TestContext.Current.CancellationToken);

        handler.RequestCount.ShouldBeGreaterThan(0);
        handler.LastRequestUri.ShouldNotBeNull();
        handler.LastRequestUri!.AbsolutePath.ShouldEndWith("/api/v1/namespaces");
    }

    [Fact]
    public async Task FakeResponseLatencyCanBeCancelled()
    {
        TestClusterConfig config = new()
        {
            ResponseLatency = TimeSpan.FromMinutes(1),
        };
        await using var cluster = await new TestClusterGenerator().CreateAsync(config, TestContext.Current.CancellationToken);
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(
            () => cluster.Client.CoreV1.ListNamespaceAsync(cancellationToken: cancellation.Token));
    }

    [Fact]
    public async Task FakeCanThrowWhenTheClientConnects()
    {
        await using var cluster = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig { ThrowOnConnect = true },
            TestContext.Current.CancellationToken);

        await Should.ThrowAsync<HttpRequestException>(
            () => cluster.Client.CoreV1.ListNamespaceAsync(cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task NamedKubeConfigSelectsTheRequestedContext()
    {
        K8SConfiguration kubeConfig = new()
        {
            ApiVersion = "v1",
            Kind = "Config",
            CurrentContext = "default",
            Clusters =
            [
                new k8s.KubeConfigModels.Cluster
                {
                    Name = "default-cluster",
                    ClusterEndpoint = new ClusterEndpoint { Server = "https://default.example.test" },
                },
                new k8s.KubeConfigModels.Cluster
                {
                    Name = "named-cluster",
                    ClusterEndpoint = new ClusterEndpoint { Server = "https://named.example.test" },
                },
            ],
            Users =
            [
                new User { Name = "default-user", UserCredentials = new UserCredentials { Token = "default" } },
                new User { Name = "named-user", UserCredentials = new UserCredentials { Token = "named" } },
            ],
            Contexts =
            [
                new Context { Name = "default", ContextDetails = new ContextDetails { Cluster = "default-cluster", User = "default-user" } },
                new Context { Name = "named", ContextDetails = new ContextDetails { Cluster = "named-cluster", User = "named-user" } },
            ],
        };

        await using var cluster = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig
            {
                Type = KubernetesBackend.Kind,
                Name = "named",
                KubeConfig = kubeConfig,
            },
            TestContext.Current.CancellationToken);

        cluster.Client.ShouldBeOfType<k8s.Kubernetes>();
        cluster.ClientConfiguration.CurrentContext.ShouldBe("named");
    }

    [Fact]
    public async Task NamedKubeConfigRequiresAClusterName()
    {
        await Should.ThrowAsync<ArgumentException>(
            () => new TestClusterGenerator().CreateAsync(
                new TestClusterConfig { Type = KubernetesBackend.Kind, KubeConfig = new K8SConfiguration() },
                TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CancellationBeforeGenerationIsHonored()
    {
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(
            () => new TestClusterGenerator().CreateAsync(new TestClusterConfig(), cancellation.Token));
    }

    [Theory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task DisposingGeneratedClusterDisposesAdditionalHandler(KubernetesBackend backend)
    {
        using RecordingHandler handler = new();
        var cluster = await new TestClusterGenerator().CreateAsync(
            CreateConfig(backend, httpHandlers: [handler]),
            TestContext.Current.CancellationToken);
        await cluster.Cluster.Connect();

        await cluster.DisposeAsync();

        handler.WasDisposed.ShouldBeTrue();
    }

    private static TestClusterConfig CreateConfig(
        KubernetesBackend backend,
        IEnumerable<IKubernetesObject>? resources = null,
        IEnumerable<DelegatingHandler>? httpHandlers = null)
        => backend switch
        {
            KubernetesBackend.Fake => TestClusterConfig.Fake(resources, httpHandlers),
            KubernetesBackend.Kind => new TestClusterConfig
            {
                Type = KubernetesBackend.Kind,
                Resources = resources?.ToArray() ?? [],
                HttpHandlers = httpHandlers?.ToArray() ?? [],
            },
            _ => throw new ArgumentOutOfRangeException(nameof(backend), backend, null),
        };

    private sealed class RecordingHandler : DelegatingHandler
    {
        public int RequestCount { get; private set; }

        public Uri? LastRequestUri { get; private set; }

        public bool WasDisposed { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestCount++;
            LastRequestUri = request.RequestUri;
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        protected override void Dispose(bool disposing)
        {
            WasDisposed = true;
            base.Dispose(disposing);
        }
    }
}
