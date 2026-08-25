using System.Collections;
using System.Diagnostics;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Kubernetes;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Clusters.Workspace;

public class ClusterWorkspaceTests
{
    [AvaloniaFact]
    public async Task creating_workspace_does_not_initialize_resource_configs_until_requested()
    {
        var workspace = await Application.Current.CreateClusterAsync(connect: false);

        workspace.GetResourceConfigs().ShouldBeEmpty();
    }

    [AvaloniaFact]
    public async Task connected_workspace_does_not_register_crd_config_template_as_built_in()
    {
        var workspace = await Application.Current.CreateClusterAsync();

        workspace.GetResourceConfigs().ShouldNotContain(config => config is CRDResourceConfig);
    }

    [AvaloniaFact]
    public async Task connected_workspace_registers_resource_config_models_in_catalog()
    {
        var workspace = await Application.Current.CreateClusterAsync();

        workspace.Runtime.ModelCatalog
            .TryGetResourceKind("v1", "Pod", out var resourceKind)
            .ShouldBeTrue();
        resourceKind.ShouldBe(GroupApiVersionKind.From<V1Pod>());
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task resource_config_can_be_looked_up_by_resource_kind(KubernetesBackend backend)
    {
        var workspace = await Application.Current.CreateClusterAsync(
            config => config.Type = backend);

        var expected = workspace.GetResourceConfig(GroupApiVersionKind.From<V1Pod>());
        workspace.GetResourceConfig(GroupApiVersionKind.From<V1Pod>()).ShouldBeSameAs(expected);
    }

    [AvaloniaFact]
    public async Task changing_runtime_status_to_connecting_updates_cluster_color()
    {
        var workspace = await Application.Current.CreateClusterAsync(connect: false);

        workspace.Runtime.Status = ClusterStatus.Connecting;
        await TestApplicationExtensions.WaitForUiAsync();

        workspace.ClusterColor.ShouldBe(Brushes.Orange);
    }

    [AvaloniaFact]
    public async Task namespace_updates_from_background_threads_update_bound_controls()
    {
        var workspace = await Application.Current.CreateClusterAsync();
        var listBox = new ListBox
        {
            ItemsSource = workspace.Runtime.Namespaces
        };
        using var window = Application.Current.CreateTestWindow(content: listBox);
        window.Show();
        await TestApplicationExtensions.WaitForUiAsync();

        var expectedItemCount = workspace.Runtime.Namespaces.Count + 1;
        await Task.Run(() => workspace.Runtime.GetResourceSourceCache<V1Namespace>().AddOrUpdate(new V1Namespace
        {
            Metadata = new V1ObjectMeta
            {
                Name = $"background-{Guid.NewGuid():N}",
                Uid = $"background-{Guid.NewGuid():N}"
            }
        }), TestContext.Current.CancellationToken);
        await TestApplicationExtensions.WaitForUiAsync();

        listBox.ItemCount.ShouldBe(expectedItemCount);
    }

    [AvaloniaFact]
    public async Task reconnect_replaces_namespace_collection_and_updates_bound_controls()
    {
        var workspace = await Application.Current.CreateClusterAsync();
        var listBox = new ListBox();
        using var window = Application.Current.CreateTestWindow(content: listBox);
        listBox.ItemsSource = workspace.Runtime.Namespaces;
        window.Show();
        await TestApplicationExtensions.WaitForUiAsync();

        var previousNamespaces = workspace.Runtime.Namespaces;
        await workspace.Disconnect();
        await workspace.Connect();
        await TestApplicationExtensions.WaitForUiAsync();

        workspace.Runtime.Namespaces.ShouldNotBeSameAs(previousNamespaces);
        listBox.ItemsSource = workspace.Runtime.Namespaces;
        var expectedItemCount = workspace.Runtime.Namespaces.Count + 1;

        await Task.Run(() => workspace.Runtime.GetResourceSourceCache<V1Namespace>().AddOrUpdate(new V1Namespace
        {
            Metadata = new V1ObjectMeta
            {
                Name = $"reconnected-{Guid.NewGuid():N}",
                Uid = $"reconnected-{Guid.NewGuid():N}"
            }
        }), TestContext.Current.CancellationToken);
        await TestApplicationExtensions.WaitForUiAsync();

        listBox.ItemCount.ShouldBe(expectedItemCount);
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task added_crd_adds_resource_config_and_gvk_catalog_entry(KubernetesBackend backend)
    {
        var workspace = await Application.Current.CreateClusterAsync(
            config => config.Type = backend);
        await workspace.Runtime.SeedResource<V1CustomResourceDefinition>(true);
        var crd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");

        await workspace.Runtime.CreateAsync(crd, TestContext.Current.CancellationToken);

        var resourceKind = await WaitForRegisteredCustomResourceKind(workspace.Runtime, crd);

        var resourceConfig = await TestWait.UntilValueAsync(
            () => GetCustomResourceConfig(workspace, crd),
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        resourceConfig.ShouldNotBeNull();
        resourceConfig.Kind.ShouldBe(resourceKind);
        resourceConfig.IsCustomResource.ShouldBeTrue();
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task resource_config_processed_event_observes_registered_crd_config(KubernetesBackend backend)
    {
        var workspace = await Application.Current.CreateClusterAsync(
            config => config.Type = backend);
        await workspace.Runtime.SeedResource<V1CustomResourceDefinition>(true);
        IResourceConfig? processedConfig = null;
        workspace.ResourceConfigProcessed += (_, resourceConfig) => processedConfig = workspace.GetResourceConfig(resourceConfig.Kind);
        var crd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");

        await workspace.Runtime.CreateAsync(crd, TestContext.Current.CancellationToken);

        var resourceConfig = await TestWait.UntilValueAsync(
            () => GetCustomResourceConfig(workspace, crd),
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        processedConfig.ShouldBeSameAs(resourceConfig);
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task resource_config_processed_event_is_published_after_crd_registration(KubernetesBackend backend)
    {
        var workspace = await Application.Current.CreateClusterAsync(
            config => config.Type = backend);
        await workspace.Runtime.SeedResource<V1CustomResourceDefinition>(true);
        List<IResourceConfig> observedConfigs = [];
        workspace.ResourceConfigProcessed += (_, resourceConfig) =>
            observedConfigs.Add(workspace.GetResourceConfig(resourceConfig.Kind));
        var crd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("events.kubeui.com", "events", "value");

        await workspace.Runtime.CreateAsync(crd, TestContext.Current.CancellationToken);

        var resourceConfig = await TestWait.UntilValueAsync(
            () => GetCustomResourceConfig(workspace, crd),
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        await TestWait.UntilAsync(
            () => observedConfigs.Count > 0,
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        observedConfigs.ShouldHaveSingleItem().ShouldBeSameAs(resourceConfig);
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task updated_crd_updates_resource_config_gvk_entry_and_seeded_informer(KubernetesBackend backend)
    {
        var workspace = await Application.Current.CreateClusterAsync(
            config => config.Type = backend);
        await workspace.Runtime.SeedResource<V1CustomResourceDefinition>(true);
        var originalCrd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");

        await workspace.Runtime.CreateAsync(originalCrd, TestContext.Current.CancellationToken);

        var originalKind = await WaitForRegisteredCustomResourceKind(workspace.Runtime, originalCrd);
        await workspace.Runtime.Permissions.UpdateCanI(GetCustomResourceKind(originalCrd), Verb.List);
        await workspace.Runtime.Permissions.UpdateCanI(GetCustomResourceKind(originalCrd), Verb.Watch);
        await SeedResourceAsync(workspace.Runtime, originalKind);

        var originalContainer = GetSeededContainer(workspace.Runtime, originalKind);
        originalContainer.ShouldNotBeNull();
        GetInformers(originalContainer).Count.ShouldBe(1);

        var updatedCrd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "otherString");
        updatedCrd.Metadata.Uid = workspace.Runtime.GetResource<V1CustomResourceDefinition>(null, originalCrd.Name()).ShouldNotBeNull().Metadata.Uid;
        await workspace.Runtime.ReplaceAsync(updatedCrd, TestContext.Current.CancellationToken);

        var updatedKind = await WaitForRegisteredCustomResourceKind(workspace.Runtime, updatedCrd);
        updatedKind.ShouldBe(originalKind);

        var updatedResourceConfig = await TestWait.UntilValueAsync(
            () =>
            {
                var resourceConfig = GetCustomResourceConfig(workspace, updatedCrd);
                return resourceConfig?.Kind == updatedKind ? resourceConfig : null;
            },
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        updatedResourceConfig.ShouldNotBeNull();
        updatedResourceConfig.Kind.ShouldBe(updatedKind);

        var updatedContainer = await TestWait.UntilValueAsync(
            () => GetSeededContainer(workspace.Runtime, updatedKind),
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        updatedContainer.ShouldNotBeNull();
        updatedContainer.ShouldBeSameAs(originalContainer);
        GetInformers(originalContainer).Count.ShouldBe(1);
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task metadata_only_crd_update_rebuilds_json_config_without_reseeding_informer(KubernetesBackend backend)
    {
        var workspace = await Application.Current.CreateClusterAsync(
            config => config.Type = backend);
        await workspace.Runtime.SeedResource<V1CustomResourceDefinition>(true);
        var originalCrd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");

        await workspace.Runtime.CreateAsync(originalCrd, TestContext.Current.CancellationToken);

        var originalKind = await WaitForRegisteredCustomResourceKind(workspace.Runtime, originalCrd);
        await workspace.Runtime.Permissions.UpdateCanI(GetCustomResourceKind(originalCrd), Verb.List);
        await workspace.Runtime.Permissions.UpdateCanI(GetCustomResourceKind(originalCrd), Verb.Watch);
        await SeedResourceAsync(workspace.Runtime, originalKind);

        var originalContainer = GetSeededContainer(workspace.Runtime, originalKind);
        originalContainer.ShouldNotBeNull();
        GetInformers(originalContainer).Count.ShouldBe(1);

        var originalResourceConfig = await TestWait.UntilValueAsync(
            () => GetCustomResourceConfig(workspace, originalCrd),
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        originalResourceConfig.ShouldNotBeNull();

        var updatedCrd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");
        updatedCrd.Metadata.Uid = workspace.Runtime.GetResource<V1CustomResourceDefinition>(null, originalCrd.Name()).ShouldNotBeNull().Metadata.Uid;
        updatedCrd.Metadata.Annotations = new Dictionary<string, string>
        {
            ["metadata-only"] = "true"
        };

        await workspace.Runtime.ReplaceAsync(updatedCrd, TestContext.Current.CancellationToken);

        var updatedResourceConfig = await TestWait.UntilValueAsync(
            () =>
            {
                var resourceConfig = GetCustomResourceConfig(workspace, updatedCrd);
                return resourceConfig is not null && !ReferenceEquals(resourceConfig, originalResourceConfig)
                    ? resourceConfig
                    : null;
            },
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        updatedResourceConfig.ShouldNotBeNull();
        GetInformers(originalContainer).Count.ShouldBe(1);
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task seeding_resource_raises_resource_seeded_event(KubernetesBackend backend)
    {
        var workspace = await Application.Current.CreateClusterAsync(
            config => config.Type = backend);
        GroupApiVersionKind? seededKind = null;
        workspace.Runtime.ResourceSeeded += (_, resourceKind) => seededKind = resourceKind;

        await workspace.Runtime.Permissions.UpdateCanI<V1Pod>(Verb.List);
        await workspace.Runtime.Permissions.UpdateCanI<V1Pod>(Verb.Watch);
        await workspace.Runtime.SeedResource<V1Pod>();

        seededKind.ShouldBe(GroupApiVersionKind.From<V1Pod>());
    }

    [AvaloniaFact]
    public async Task denied_resource_seed_does_not_raise_resource_seeded_event()
    {
        var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.AuthenticatedUser = KubernetesRbac.ServiceAccountUser;
            config.InitialYaml = KubernetesTestData.LimitedAccessWithNamespacePermissions;
        });
        var seeded = false;
        workspace.Runtime.ResourceSeeded += (_, _) => seeded = true;

        await workspace.Runtime.SeedResource<Corev1Event>();

        seeded.ShouldBeFalse();
        var container = GetSeededContainer(workspace.Runtime, GroupApiVersionKind.From<Corev1Event>());
        container.ShouldNotBeNull();
        GetInformers(container).ShouldBeEmpty();
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task initializing_workspace_seeds_custom_resource_definitions_when_allowed(KubernetesBackend backend)
    {
        var workspace = await Application.Current.CreateClusterAsync(
            config => config.Type = backend);

        var kind = GroupApiVersionKind.From<V1CustomResourceDefinition>();
        workspace.Runtime.Objects.TryGetValue(kind, out var container).ShouldBeTrue();
        container.ShouldBeOfType<ContainerClass<V1CustomResourceDefinition>>()
            .Informers.Count.ShouldBe(1);
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task deleted_crd_removes_resource_config_gvk_entry_and_seeded_informer(KubernetesBackend backend)
    {
        var workspace = await Application.Current.CreateClusterAsync(
            config => config.Type = backend);
        await workspace.Runtime.SeedResource<V1CustomResourceDefinition>(true);
        var crd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");

        await workspace.Runtime.CreateAsync(crd, TestContext.Current.CancellationToken);

        var resourceKind = await WaitForRegisteredCustomResourceKind(workspace.Runtime, crd);
        await workspace.Runtime.Permissions.UpdateCanI(GetCustomResourceKind(crd), Verb.List);
        await workspace.Runtime.Permissions.UpdateCanI(GetCustomResourceKind(crd), Verb.Watch);
        await SeedResourceAsync(workspace.Runtime, resourceKind);

        var seededContainer = GetSeededContainer(workspace.Runtime, resourceKind);
        seededContainer.ShouldNotBeNull();
        GetInformers(seededContainer).Count.ShouldBe(1);

        crd.Metadata.Uid = workspace.Runtime.GetResource<V1CustomResourceDefinition>(null, crd.Name()).ShouldNotBeNull().Metadata.Uid;
        await workspace.Runtime.DeleteAsync(crd, TestContext.Current.CancellationToken);

        await TestWait.UntilAsync(
            () => GetCustomResourceConfig(workspace, crd) == null,
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        await TestWait.UntilAsync(
            () => !IsCustomResourceRegistered(workspace.Runtime, crd),
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
        var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.AuthenticatedUser = KubernetesRbac.ServiceAccountUser;
            config.InitialResources = new[]
                    {
                        new V1Namespace { Metadata = new V1ObjectMeta { Name = "team-a" } },
                        new V1Namespace { Metadata = new V1ObjectMeta { Name = "team-b" } },
                    }
                        .Concat(KubernetesRbac.ClusterWide(
                            new RbacRule("namespaces", "list"),
                            new RbacRule("namespaces", "watch")))
                        .Concat(KubernetesRbac.InNamespace("team-a",
                            new RbacRule("pods", "list"),
                            new RbacRule("pods", "watch")))
                        .Concat(KubernetesRbac.InNamespace("team-b",
                            new RbacRule("pods", "list"),
                            new RbacRule("pods", "watch")))
                        .ToArray();
        }, connect: false);
        workspace.GetResourceConfigs().ShouldBeEmpty();
        await workspace.Connect();
        await workspace.Runtime.SeedResource<V1Namespace>(true);

        await TestWait.UntilAsync(
            () => workspace.Runtime.Namespaces.Select(x => x.Name()).Contains("team-a")
                && workspace.Runtime.Namespaces.Select(x => x.Name()).Contains("team-b"),
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        await workspace.Runtime.Permissions.UpdateCanI<V1Pod>(Verb.List, "team-a");
        await workspace.Runtime.Permissions.UpdateCanI<V1Pod>(Verb.Watch, "team-a");
        await workspace.Runtime.Permissions.UpdateCanI<V1Pod>(Verb.List, "team-b");
        await workspace.Runtime.Permissions.UpdateCanI<V1Pod>(Verb.Watch, "team-b");

        await workspace.Runtime.SeedResource<V1Pod>();

        workspace.Runtime.GetResource<V1Namespace>(null, "team-a").ShouldNotBeNull();

        var podContainer = GetSeededContainer(workspace.Runtime, GroupApiVersionKind.From<V1Pod>());
        podContainer.ShouldNotBeNull();
        GetInformers(podContainer).Count.ShouldBe(2);
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task disconnect_disposes_seeded_informers_and_registrations(KubernetesBackend backend)
    {
        var workspace = await Application.Current.CreateClusterAsync(
            config => config.Type = backend);

        await workspace.Runtime.Permissions.UpdateCanI<V1Pod>(Verb.List);
        await workspace.Runtime.Permissions.UpdateCanI<V1Pod>(Verb.Watch);
        await workspace.Runtime.SeedResource<V1Pod>();

        var container = GetSeededContainer(workspace.Runtime, GroupApiVersionKind.From<V1Pod>());
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
        workspace.Runtime.Objects.ShouldBeEmpty();
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task disconnect_allows_resource_types_to_be_seeded_again(KubernetesBackend backend)
    {
        var workspace = await Application.Current.CreateClusterAsync(
            config => config.Type = backend);

        await workspace.Runtime.Permissions.UpdateCanI<V1Pod>(Verb.List);
        await workspace.Runtime.Permissions.UpdateCanI<V1Pod>(Verb.Watch);
        await workspace.Runtime.SeedResource<V1Pod>();

        var initialContainer = GetSeededContainer(workspace.Runtime, GroupApiVersionKind.From<V1Pod>());
        initialContainer.ShouldNotBeNull();
        GetInformers(initialContainer).Count.ShouldBe(1);

        await workspace.Disconnect();

        workspace.Runtime.Objects.ShouldBeEmpty();

        await workspace.Connect();
        await workspace.Runtime.SeedResource<V1Pod>();

        var reseededContainer = GetSeededContainer(workspace.Runtime, GroupApiVersionKind.From<V1Pod>());
        reseededContainer.ShouldNotBeNull();
        GetInformers(reseededContainer).Count.ShouldBe(1);
        GetInformerRegistrations(reseededContainer).Count.ShouldBe(1);
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task disconnect_removes_dynamic_crd_gvk_catalog_entries(KubernetesBackend backend)
    {
        var workspace = await Application.Current.CreateClusterAsync(
            config => config.Type = backend);
        await workspace.Runtime.SeedResource<V1CustomResourceDefinition>(true);
        var crd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");

        await workspace.Runtime.CreateAsync(crd, TestContext.Current.CancellationToken);

        var resourceKind = await WaitForRegisteredCustomResourceKind(workspace.Runtime, crd);

        await workspace.Disconnect();

        await TestWait.UntilAsync(
            () => !IsCustomResourceRegistered(workspace.Runtime, crd),
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
    }

    [AvaloniaFact]
    public async Task connect_returns_before_synchronous_connection_work_completes()
    {
        var workspace = await Application.Current.CreateClusterAsync(
            config => config.ResponseLatency = TimeSpan.FromMilliseconds(200),
            connect: false);

        var stopwatch = Stopwatch.StartNew();
        var connectTask = workspace.Connect();
        stopwatch.Stop();

        stopwatch.Elapsed.ShouldBeLessThan(TimeSpan.FromMilliseconds(150));

        await connectTask;
        workspace.Runtime.Connected.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task concurrent_connect_calls_share_one_in_flight_connection()
    {
        var workspace = await Application.Current.CreateClusterAsync(connect: false);

        var firstConnect = workspace.Connect();
        var secondConnect = workspace.Connect();

        secondConnect.ShouldBeSameAs(firstConnect);
        await Task.WhenAll(firstConnect, secondConnect);
        workspace.Runtime.Connected.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task connect_skips_workspace_initialization_when_runtime_remains_disconnected()
    {
        var workspace = await Application.Current.CreateClusterAsync(
            config => config.ThrowOnConnect = true,
            connect: false);
        workspace.Runtime.Status = ClusterStatus.Errored;

        await workspace.Connect();

        workspace.Runtime.Connected.ShouldBeFalse();
        workspace.GetResourceConfigs().ShouldNotBeEmpty();
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task added_crd_does_not_refresh_authorization_index_for_generic_resource(KubernetesBackend backend)
    {
        var workspace = await Application.Current.CreateClusterAsync(
            config => config.Type = backend);

        var crd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");
        await workspace.Runtime.AddOrUpdateResource(crd);

        var resourceKind = await WaitForRegisteredCustomResourceKind(workspace.Runtime, crd);

        await TestWait.UntilAsync(
            () => GetCustomResourceConfig(workspace, crd) != null,
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
    }

    private static async Task<GroupApiVersionKind> WaitForRegisteredCustomResourceKind(
        IClusterRuntime runtime,
        V1CustomResourceDefinition crd)
    {
        var kind = GetCustomResourceKind(crd);
        await TestWait.UntilAsync(
            () => runtime.ModelCatalog.IsCustomResource(kind),
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        return kind;
    }

    private static bool IsCustomResourceRegistered(
        IClusterRuntime runtime,
        V1CustomResourceDefinition crd)
    {
        return crd.TryGetResourceKind(out var kind)
            && runtime.ModelCatalog.IsCustomResource(kind);
    }

    private static GroupApiVersionKind GetCustomResourceKind(V1CustomResourceDefinition crd)
    {
        var version = crd.Spec.Versions.First(x => x.Served && x.Storage).Name;
        return new GroupApiVersionKind(crd.Spec.Group, version, crd.Spec.Names.Kind, crd.Spec.Names.Plural);
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

    private static Task SeedResourceAsync(IClusterRuntime runtime, GroupApiVersionKind resourceKind)
    {
        return runtime.SeedResource(resourceKind, true);
    }

    private static object? GetSeededContainer(IClusterRuntime runtime, GroupApiVersionKind resourceKind)
    {
        return runtime.Objects.TryGetValue(resourceKind, out var container)
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

    public Task SeedResource(bool waitForReady = false) => Task.CompletedTask;

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

    public Task SeedResource(bool waitForReady = false) => Task.CompletedTask;

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
