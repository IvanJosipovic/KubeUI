using System.Collections;
using System.Diagnostics;
using System.Reflection;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Tests.Infra;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Clusters.Workspace;

public class ClusterWorkspaceTests
{
    [AvaloniaFact]
    public async Task creating_workspace_does_not_initialize_resource_configs_until_requested()
    {
        IServiceProvider services = Application.Current.GetTestServices();
        TestClusterConfig config = services.GetRequiredService<TestClusterConfig>();
        config.StartDisconnected = true;
        ClusterWorkspace workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();

        workspace.GetResourceConfigs().ShouldBeEmpty();
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task resource_config_can_be_looked_up_by_resource_type(KubernetesBackend backend)
    {
        IServiceProvider services = Application.Current.GetTestServices();
        TestClusterConfig config = services.GetRequiredService<TestClusterConfig>();
        config.Type = backend;
        ClusterWorkspace workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();

        await workspace.Connect();

        IResourceConfig expected = workspace.GetResourceConfig<V1Pod>();
        workspace.GetResourceConfig(typeof(V1Pod)).ShouldBeSameAs(expected);
    }

    [AvaloniaFact]
    public async Task changing_runtime_status_to_connecting_updates_cluster_color()
    {
        IServiceProvider services = Application.Current.GetTestServices();
        TestClusterConfig config = services.GetRequiredService<TestClusterConfig>();
        config.StartDisconnected = true;
        ClusterWorkspace workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        IClusterRuntime runtime = workspace.Runtime;

        runtime.Status = ClusterStatus.Connecting;
        Dispatcher.UIThread.RunJobs();

        workspace.ClusterColor.ShouldBe(Brushes.Orange);
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task added_crd_adds_resource_config_and_model_cache_entry(KubernetesBackend backend)
    {
        IServiceProvider services = Application.Current.GetTestServices();
        TestClusterConfig config = services.GetRequiredService<TestClusterConfig>();
        config.Type = backend;
        ClusterWorkspace workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        IClusterRuntime runtime = workspace.Runtime;
        await workspace.Connect();
        await runtime.SeedResource<V1CustomResourceDefinition>(true);
        var crd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");

        await runtime.CreateAsync(crd, TestContext.Current.CancellationToken);

        var resourceType = await TestWait.UntilValueAsync(
            () => GetCustomResourceType(runtime, crd),
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        resourceType.ShouldNotBeNull();

        var resourceConfig = await TestWait.UntilValueAsync(
            () => GetCustomResourceConfig(workspace, crd),
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        resourceConfig.ShouldNotBeNull();
        resourceConfig.Type.ShouldBe(resourceType);
        resourceConfig.IsCustomResource.ShouldBeTrue();
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task resource_config_processed_event_observes_registered_crd_config(KubernetesBackend backend)
    {
        IServiceProvider services = Application.Current.GetTestServices();
        TestClusterConfig config = services.GetRequiredService<TestClusterConfig>();
        config.Type = backend;
        ClusterWorkspace workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        IClusterRuntime runtime = workspace.Runtime;
        await workspace.Connect();
        await runtime.SeedResource<V1CustomResourceDefinition>(true);
        IResourceConfig? processedConfig = null;
        workspace.ResourceConfigProcessed += (_, resourceConfig) => processedConfig = workspace.GetResourceConfig(resourceConfig.Type);
        var crd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");

        await runtime.CreateAsync(crd, TestContext.Current.CancellationToken);

        var resourceConfig = await TestWait.UntilValueAsync(
            () => GetCustomResourceConfig(workspace, crd),
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        processedConfig.ShouldBeSameAs(resourceConfig);
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task updated_crd_replaces_resource_config_model_cache_entry_and_seeded_informer(KubernetesBackend backend)
    {
        IServiceProvider services = Application.Current.GetTestServices();
        TestClusterConfig config = services.GetRequiredService<TestClusterConfig>();
        config.Type = backend;
        ClusterWorkspace workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        IClusterRuntime runtime = workspace.Runtime;
        await workspace.Connect();
        await runtime.SeedResource<V1CustomResourceDefinition>(true);
        var originalCrd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");

        await runtime.CreateAsync(originalCrd, TestContext.Current.CancellationToken);

        var originalType = await TestWait.UntilValueAsync(
            () => GetCustomResourceType(runtime, originalCrd),
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        originalType.ShouldNotBeNull();
        await runtime.Permissions.UpdateCanI(originalType, Verb.List);
        await runtime.Permissions.UpdateCanI(originalType, Verb.Watch);
        await SeedResourceAsync(runtime, originalType);

        var originalContainer = GetSeededContainer(runtime, originalType);
        originalContainer.ShouldNotBeNull();
        GetInformers(originalContainer).Count.ShouldBe(1);

        var updatedCrd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "otherString");
        updatedCrd.Metadata.Uid = runtime.GetResource<V1CustomResourceDefinition>(null, originalCrd.Name()).ShouldNotBeNull().Metadata.Uid;
        await runtime.ReplaceAsync(updatedCrd, TestContext.Current.CancellationToken);

        var updatedType = await TestWait.UntilValueAsync(
            () =>
            {
                var type = GetCustomResourceType(runtime, updatedCrd);
                return type != null && type != originalType ? type : null;
            },
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        updatedType.ShouldNotBeNull();
        updatedType.ShouldNotBe(originalType);

        var updatedResourceConfig = await TestWait.UntilValueAsync(
            () =>
            {
                var resourceConfig = GetCustomResourceConfig(workspace, updatedCrd);
                return resourceConfig?.Type == updatedType ? resourceConfig : null;
            },
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        updatedResourceConfig.ShouldNotBeNull();
        updatedResourceConfig.Type.ShouldBe(updatedType);

        var updatedContainer = await TestWait.UntilValueAsync(
            () => GetSeededContainer(runtime, updatedType),
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        updatedContainer.ShouldNotBeNull();
        updatedContainer.ShouldNotBeSameAs(originalContainer);
        GetInformers(originalContainer).Count.ShouldBe(0);
        GetInformers(updatedContainer).Count.ShouldBe(1);
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task metadata_only_crd_update_does_not_rebuild_resource_config_or_reseed_informer(KubernetesBackend backend)
    {
        IServiceProvider services = Application.Current.GetTestServices();
        TestClusterConfig config = services.GetRequiredService<TestClusterConfig>();
        config.Type = backend;
        ClusterWorkspace workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        IClusterRuntime runtime = workspace.Runtime;
        await workspace.Connect();
        await runtime.SeedResource<V1CustomResourceDefinition>(true);
        var originalCrd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");

        await runtime.CreateAsync(originalCrd, TestContext.Current.CancellationToken);

        var originalType = await TestWait.UntilValueAsync(
            () => GetCustomResourceType(runtime, originalCrd),
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        originalType.ShouldNotBeNull();
        await runtime.Permissions.UpdateCanI(originalType, Verb.List);
        await runtime.Permissions.UpdateCanI(originalType, Verb.Watch);
        await SeedResourceAsync(runtime, originalType);

        var originalContainer = GetSeededContainer(runtime, originalType);
        originalContainer.ShouldNotBeNull();
        GetInformers(originalContainer).Count.ShouldBe(1);

        var originalResourceConfig = await TestWait.UntilValueAsync(
            () => GetCustomResourceConfig(workspace, originalCrd),
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        originalResourceConfig.ShouldNotBeNull();

        var updatedCrd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");
        updatedCrd.Metadata.Uid = runtime.GetResource<V1CustomResourceDefinition>(null, originalCrd.Name()).ShouldNotBeNull().Metadata.Uid;
        updatedCrd.Metadata.Annotations = new Dictionary<string, string>
        {
            ["metadata-only"] = "true"
        };

        await runtime.ReplaceAsync(updatedCrd, TestContext.Current.CancellationToken);

        await TestWait.UntilAsync(
            () => GetCustomResourceConfig(workspace, updatedCrd) is not null,
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        var updatedResourceConfig = GetCustomResourceConfig(workspace, updatedCrd);
        updatedResourceConfig.ShouldBeSameAs(originalResourceConfig);
        GetInformers(originalContainer).Count.ShouldBe(1);
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task seeding_resource_raises_resource_seeded_event(KubernetesBackend backend)
    {
        IServiceProvider services = Application.Current.GetTestServices();
        TestClusterConfig config = services.GetRequiredService<TestClusterConfig>();
        config.Type = backend;
        ClusterWorkspace workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        IClusterRuntime runtime = workspace.Runtime;
        GroupApiVersionKind? seededKind = null;
        runtime.ResourceSeeded += (_, resourceKind) => seededKind = resourceKind;

        await runtime.Permissions.UpdateCanI<V1Pod>(Verb.List);
        await runtime.Permissions.UpdateCanI<V1Pod>(Verb.Watch);
        await workspace.Runtime.SeedResource<V1Pod>();

        seededKind.ShouldBe(GroupApiVersionKind.From<V1Pod>());
    }

    [AvaloniaFact]
    public async Task denied_resource_seed_does_not_raise_resource_seeded_event()
    {
        IServiceProvider services = Application.Current.GetTestServices();
        TestClusterConfig config = services.GetRequiredService<TestClusterConfig>();
        config.AuthenticatedUser = KubernetesRbac.ServiceAccountUser;
        config.InitialYaml = KubernetesTestData.LimitedAccessWithNamespacePermissions;
        ClusterWorkspace workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        IClusterRuntime runtime = workspace.Runtime;
        var seeded = false;
        runtime.ResourceSeeded += (_, _) => seeded = true;

        await workspace.Runtime.SeedResource<Corev1Event>();

        seeded.ShouldBeFalse();
        var container = GetSeededContainer(runtime, typeof(Corev1Event));
        container.ShouldNotBeNull();
        GetInformers(container).ShouldBeEmpty();
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task initializing_workspace_seeds_custom_resource_definitions_when_allowed(KubernetesBackend backend)
    {
        IServiceProvider services = Application.Current.GetTestServices();
        TestClusterConfig config = services.GetRequiredService<TestClusterConfig>();
        config.Type = backend;
        ClusterWorkspace workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        IClusterRuntime runtime = workspace.Runtime;

        await workspace.Connect();

        var kind = GroupApiVersionKind.From<V1CustomResourceDefinition>();
        runtime.Objects.TryGetValue(kind, out var container).ShouldBeTrue();
        container.ShouldBeOfType<ContainerClass<V1CustomResourceDefinition>>()
            .Informers.Count.ShouldBe(1);
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task deleted_crd_removes_resource_config_model_cache_entry_and_seeded_informer(KubernetesBackend backend)
    {
        IServiceProvider services = Application.Current.GetTestServices();
        TestClusterConfig config = services.GetRequiredService<TestClusterConfig>();
        config.Type = backend;
        ClusterWorkspace workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        IClusterRuntime runtime = workspace.Runtime;
        await workspace.Connect();
        await runtime.SeedResource<V1CustomResourceDefinition>(true);
        var crd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");

        await runtime.CreateAsync(crd, TestContext.Current.CancellationToken);

        var resourceType = await TestWait.UntilValueAsync(
            () => GetCustomResourceType(runtime, crd),
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        resourceType.ShouldNotBeNull();
        await runtime.Permissions.UpdateCanI(resourceType, Verb.List);
        await runtime.Permissions.UpdateCanI(resourceType, Verb.Watch);
        await SeedResourceAsync(runtime, resourceType);

        var seededContainer = GetSeededContainer(runtime, resourceType);
        seededContainer.ShouldNotBeNull();
        GetInformers(seededContainer).Count.ShouldBe(1);

        crd.Metadata.Uid = runtime.GetResource<V1CustomResourceDefinition>(null, crd.Name()).ShouldNotBeNull().Metadata.Uid;
        await runtime.DeleteAsync(crd, TestContext.Current.CancellationToken);

        await TestWait.UntilAsync(
            () => GetCustomResourceConfig(workspace, crd) == null,
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        await TestWait.UntilAsync(
            () => GetCustomResourceType(runtime, crd) == null,
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        await TestWait.UntilAsync(
            () => GetInformers(seededContainer).Count == 0,
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
    }

    [AvaloniaFact]
    public async Task seeding_namespaced_resource_uses_known_namespaces_without_eager_resource_config_initialization()
    {
        IServiceProvider services = Application.Current.GetTestServices();
        TestClusterConfig config = services.GetRequiredService<TestClusterConfig>();
        config.AuthenticatedUser = KubernetesRbac.ServiceAccountUser;
        config.InitialResources = new[]
                {
                    new V1Namespace { Metadata = new V1ObjectMeta { Name = "team-a" } },
                    new V1Namespace { Metadata = new V1ObjectMeta { Name = "team-b" } },
                }
                    .Concat(KubernetesRbac.InNamespace("team-a",
                        new RbacRule("pods", "list"),
                        new RbacRule("pods", "watch")))
                    .Concat(KubernetesRbac.InNamespace("team-b",
                        new RbacRule("pods", "list"),
                        new RbacRule("pods", "watch")))
                    .ToArray();
        ClusterWorkspace workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        IClusterRuntime runtime = workspace.Runtime;
        var cluster = runtime;
        await runtime.SeedResource<V1Namespace>(true);
        await runtime.AddOrUpdateResource(new V1Namespace
        {
            Metadata = new V1ObjectMeta { Name = "team-a" }
        });
        await runtime.AddOrUpdateResource(new V1Namespace
        {
            Metadata = new V1ObjectMeta { Name = "team-b" }
        });

        await TestWait.UntilAsync(
            () => runtime.Namespaces.Select(x => x.Name()).Contains("team-a")
                && runtime.Namespaces.Select(x => x.Name()).Contains("team-b"),
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        workspace.GetResourceConfigs().ShouldBeEmpty();

        await cluster.Permissions.UpdateCanI<V1Pod>(Verb.List, "team-a");
        await cluster.Permissions.UpdateCanI<V1Pod>(Verb.Watch, "team-a");
        await cluster.Permissions.UpdateCanI<V1Pod>(Verb.List, "team-b");
        await cluster.Permissions.UpdateCanI<V1Pod>(Verb.Watch, "team-b");

        await workspace.Runtime.SeedResource<V1Pod>();

        runtime.GetResource<V1Namespace>(null, "team-a").ShouldNotBeNull();
        workspace.GetResourceConfigs().ShouldBeEmpty();

        var podContainer = GetSeededContainer(runtime, typeof(V1Pod));
        podContainer.ShouldNotBeNull();
        GetInformers(podContainer).Count.ShouldBe(2);
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task disconnect_disposes_seeded_informers_and_registrations(KubernetesBackend backend)
    {
        IServiceProvider services = Application.Current.GetTestServices();
        TestClusterConfig config = services.GetRequiredService<TestClusterConfig>();
        config.Type = backend;
        ClusterWorkspace workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        IClusterRuntime runtime = workspace.Runtime;

        await runtime.Permissions.UpdateCanI<V1Pod>(Verb.List);
        await runtime.Permissions.UpdateCanI<V1Pod>(Verb.Watch);
        await workspace.Runtime.SeedResource<V1Pod>();

        var container = GetSeededContainer(runtime, typeof(V1Pod));
        container.ShouldNotBeNull();

        var informers = GetInformers(container);
        informers.Count.ShouldBe(1);
        var registrations = GetInformerRegistrations(container);
        registrations.Count.ShouldBe(1);

        await workspace.Disconnect();

        await TestWait.UntilAsync(
            () => informers.Count == 0 && registrations.Count == 0,
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        runtime.Objects.ShouldBeEmpty();
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task disconnect_allows_resource_types_to_be_seeded_again(KubernetesBackend backend)
    {
        IServiceProvider services = Application.Current.GetTestServices();
        TestClusterConfig config = services.GetRequiredService<TestClusterConfig>();
        config.Type = backend;
        ClusterWorkspace workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        IClusterRuntime runtime = workspace.Runtime;

        await runtime.Permissions.UpdateCanI<V1Pod>(Verb.List);
        await runtime.Permissions.UpdateCanI<V1Pod>(Verb.Watch);
        await workspace.Runtime.SeedResource<V1Pod>();

        var initialContainer = GetSeededContainer(runtime, typeof(V1Pod));
        initialContainer.ShouldNotBeNull();
        GetInformers(initialContainer).Count.ShouldBe(1);

        await workspace.Disconnect();

        runtime.Objects.ShouldBeEmpty();

        await workspace.Connect();
        await workspace.Runtime.SeedResource<V1Pod>();

        var reseededContainer = GetSeededContainer(runtime, typeof(V1Pod));
        reseededContainer.ShouldNotBeNull();
        GetInformers(reseededContainer).Count.ShouldBe(1);
        GetInformerRegistrations(reseededContainer).Count.ShouldBe(1);
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task disconnect_removes_dynamic_crd_model_cache_entries(KubernetesBackend backend)
    {
        IServiceProvider services = Application.Current.GetTestServices();
        TestClusterConfig config = services.GetRequiredService<TestClusterConfig>();
        config.Type = backend;
        ClusterWorkspace workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        IClusterRuntime runtime = workspace.Runtime;
        await workspace.Connect();
        await runtime.SeedResource<V1CustomResourceDefinition>(true);
        var crd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");

        await runtime.CreateAsync(crd, TestContext.Current.CancellationToken);

        var resourceType = await TestWait.UntilValueAsync(
            () => GetCustomResourceType(runtime, crd),
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        resourceType.ShouldNotBeNull();

        await workspace.Disconnect();

        await TestWait.UntilAsync(
            () => GetCustomResourceType(runtime, crd) == null,
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
    }

    [AvaloniaFact]
    public async Task connect_returns_before_synchronous_connection_work_completes()
    {
        IServiceProvider services = Application.Current.GetTestServices();
        TestClusterConfig config = services.GetRequiredService<TestClusterConfig>();
        config.ResponseLatency = TimeSpan.FromMilliseconds(200);
        ClusterWorkspace workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        IClusterRuntime runtime = workspace.Runtime;

        var stopwatch = Stopwatch.StartNew();
        var connectTask = workspace.Connect();
        stopwatch.Stop();

        stopwatch.Elapsed.ShouldBeLessThan(TimeSpan.FromMilliseconds(150));

        await connectTask;
        runtime.Connected.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task concurrent_connect_calls_share_one_in_flight_connection()
    {
        IServiceProvider services = Application.Current.GetTestServices();
        TestClusterConfig config = services.GetRequiredService<TestClusterConfig>();
        config.StartDisconnected = true;
        var workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();

        var firstConnect = workspace.Connect();
        var secondConnect = workspace.Connect();

        secondConnect.ShouldBeSameAs(firstConnect);
        await Task.WhenAll(firstConnect, secondConnect);
        workspace.Runtime.Connected.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task connect_skips_workspace_initialization_when_runtime_remains_disconnected()
    {
        IServiceProvider services = Application.Current.GetTestServices();
        TestClusterConfig config = services.GetRequiredService<TestClusterConfig>();
        config.ThrowOnConnect = true;
        ClusterWorkspace workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        IClusterRuntime runtime = workspace.Runtime;
        runtime.Status = ClusterStatus.Errored;

        await workspace.Connect();

        runtime.Connected.ShouldBeFalse();
        workspace.GetResourceConfigs().ShouldNotBeEmpty();
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task added_crd_does_not_refresh_authorization_index_for_generated_resource(KubernetesBackend backend)
    {
        IServiceProvider services = Application.Current.GetTestServices();
        TestClusterConfig config = services.GetRequiredService<TestClusterConfig>();
        config.Type = backend;
        ClusterWorkspace workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        var runtime = workspace.Runtime;

        await workspace.Connect();

        var crd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");
        await runtime.AddOrUpdateResource(crd);

        var resourceType = await TestWait.UntilValueAsync(
            () => GetCustomResourceType(runtime, crd),
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        resourceType.ShouldNotBeNull();

        await TestWait.UntilAsync(
            () => GetCustomResourceConfig(workspace, crd) != null,
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
    }

    private static Type? GetCustomResourceType(IClusterRuntime runtime, V1CustomResourceDefinition crd)
    {
        var version = crd.Spec?.Versions?.FirstOrDefault(x => x.Served && x.Storage)?.Name;
        return version == null ? null : runtime.ModelCache.GetResourceType(crd.Spec.Group, version, crd.Spec.Names.Kind);
    }

    private static IResourceConfig? GetCustomResourceConfig(ClusterWorkspace workspace, V1CustomResourceDefinition crd)
    {
        var version = crd.Spec?.Versions?.FirstOrDefault(x => x.Served && x.Storage)?.Name;
        if (version == null)
        {
            return null;
        }

        return workspace.GetResourceConfigs().FirstOrDefault(x =>
            x.IsCustomResource
            && string.Equals(x.Kind.Group, crd.Spec.Group, StringComparison.Ordinal)
            && string.Equals(x.Kind.ApiVersion, version, StringComparison.Ordinal)
            && string.Equals(x.Kind.Kind, crd.Spec.Names.Kind, StringComparison.Ordinal));
    }

    private static async Task SeedResourceAsync(IClusterRuntime runtime, Type resourceType)
    {
        var method = runtime.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .First(x => x.Name == nameof(IClusterRuntime.SeedResource) && x.IsGenericMethodDefinition && x.GetParameters().Length == 1)
            .MakeGenericMethod(resourceType);

        await (Task)method.Invoke(runtime, [true])!;
    }

    private static object? GetSeededContainer(IClusterRuntime runtime, Type resourceType)
    {
        return runtime.Objects.TryGetValue(GroupApiVersionKind.From(resourceType), out var container)
            ? container
            : null;
    }

    private static IList<IResourceInformer> GetInformers(object container)
    {
        return (IList<IResourceInformer>)(container.GetType().GetProperty("Informers")?.GetValue(container)
            ?? throw new InvalidOperationException("Container does not expose Informers."));
    }

    private static IList<IResourceInformerRegistration> GetInformerRegistrations(object container)
    {
        return (IList<IResourceInformerRegistration>)(container.GetType().GetProperty("InformerRegistrations")?.GetValue(container)
            ?? throw new InvalidOperationException("Container does not expose InformerRegistrations."));
    }
}

internal sealed class BlockingPodPermissionResourceConfig : IResourceConfig
{
    private readonly Task _releaseTask;

    public BlockingPodPermissionResourceConfig(Task releaseTask)
    {
        _releaseTask = releaseTask;
    }

    public ClusterWorkspace? Cluster { get; private set; }
    public bool IsNamespaced => true;
    public bool CanListAndWatch { get; private set; }
    public bool PermissionsLoaded { get; private set; }
    public bool ShowNewResource => true;
    public bool IsCustomResource => false;
    public GroupApiVersionKind Kind => GroupApiVersionKind.From(Type);
    public IList<IResourceListColumn> Columns() => [];
    public IEnumerable<MenuItemViewModel> GetDefaultMenuItems(IEnumerable? selectedItems) => [];
    public IEnumerable<MenuItemViewModel> GetCustomMenuItems(IEnumerable? selectedItems) => [];
    public int Order => 0;
    public string Name => "Pods";
    public string? Category => "Workloads";
    public Style[] ListStyle() => [];
    public Type Type { get; } = typeof(V1Pod);
    public IRelayCommand NewResourceCommand => throw new NotImplementedException();
    public IRelayCommand<IList> ViewCommand => throw new NotImplementedException();
    public IAsyncRelayCommand<IList> DeleteCommand => throw new NotImplementedException();
    public IEnumerable<(Verb verb, string? subresource)> Permissions() => [(Verb.List, null), (Verb.Watch, null), (Verb.Create, "portforward")];

    public async Task EvaluateListWatchAccessAsync()
    {
        await _releaseTask.ConfigureAwait(false);
        CanListAndWatch = true;
        PermissionsLoaded = true;
    }

    public void Initialize(ClusterWorkspace cluster)
    {
        Cluster = cluster;
    }
}

internal sealed class ImmediatePermissionResourceConfig : IResourceConfig
{
    private readonly TaskCompletionSource<object?> _completion;

    public ImmediatePermissionResourceConfig(Type type, string name, TaskCompletionSource<object?> completion)
    {
        Type = type;
        Name = name;
        _completion = completion;
    }

    public ClusterWorkspace? Cluster { get; private set; }
    public bool IsNamespaced => true;
    public bool CanListAndWatch { get; private set; }
    public bool PermissionsLoaded { get; private set; }
    public bool ShowNewResource => true;
    public bool IsCustomResource => false;
    public GroupApiVersionKind Kind => GroupApiVersionKind.From(Type);
    public IList<IResourceListColumn> Columns() => [];
    public IEnumerable<MenuItemViewModel> GetDefaultMenuItems(IEnumerable? selectedItems) => [];
    public IEnumerable<MenuItemViewModel> GetCustomMenuItems(IEnumerable? selectedItems) => [];
    public int Order => 0;
    public string Name { get; }
    public string? Category => null;
    public Style[] ListStyle() => [];
    public Type Type { get; }
    public IRelayCommand NewResourceCommand => throw new NotImplementedException();
    public IRelayCommand<IList> ViewCommand => throw new NotImplementedException();
    public IAsyncRelayCommand<IList> DeleteCommand => throw new NotImplementedException();
    public IEnumerable<(Verb verb, string? subresource)> Permissions() => [(Verb.List, null), (Verb.Watch, null)];

    public Task EvaluateListWatchAccessAsync()
    {
        CanListAndWatch = true;
        PermissionsLoaded = true;
        _completion.TrySetResult(null);
        return Task.CompletedTask;
    }

    public void Initialize(ClusterWorkspace cluster)
    {
        Cluster = cluster;
    }
}

internal static class ClusterWorkspaceTestCustomResourceDefinitionFactory
{
    public static V1CustomResourceDefinition Create(string name, string plural, string schemaProperty)
    {
        return new V1CustomResourceDefinition
        {
            Metadata = new()
            {
                Name = name
            },
            Spec = new()
            {
                Group = "kubeui.com",
                Scope = "Namespaced",
                Names = new()
                {
                    Plural = plural,
                    Singular = "test",
                    Kind = "Test",
                    ListKind = "TestList"
                },
                Versions =
                [
                    new()
                    {
                        Name = "v1beta1",
                        Served = true,
                        Storage = true,
                        Schema = new()
                        {
                            OpenAPIV3Schema = new()
                            {
                                Type = "object",
                                Properties = new Dictionary<string, V1JSONSchemaProps>
                                {
                                    ["apiVersion"] = new() { Type = "string" },
                                    ["kind"] = new() { Type = "string" },
                                    ["metadata"] = new() { Type = "object" },
                                    ["spec"] = new()
                                    {
                                        Type = "object",
                                        Properties = new Dictionary<string, V1JSONSchemaProps>
                                        {
                                            [schemaProperty] = new() { Type = "string" }
                                        }
                                    }
                                }
                            }
                        }
                    }
                ]
            }
        };
    }
}
