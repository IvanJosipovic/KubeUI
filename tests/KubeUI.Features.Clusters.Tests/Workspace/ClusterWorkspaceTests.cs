using System.Collections;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.App.Infrastructure.Platform;
using KubeUI.Features.Resources;
using KubeUI.Testing.Kubernetes.Transport;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace KubeUI.Features.Clusters.Tests.Workspace;

public class ClusterWorkspaceTests
{
    [AvaloniaFact]
    public async Task creating_workspace_does_not_initialize_resource_configs_until_requested()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);

        workspace.GetResourceConfigs().ShouldBeEmpty();
    }

    [AvaloniaFact]
    public async Task connected_workspace_does_not_register_crd_config_template_as_built_in()
    {
        using var workspace = await Application.Current.CreateClusterAsync();

        workspace.GetResourceConfigs().ShouldNotContain(config => config is CustomResourceConfig);
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task discovery_registers_custom_resource_without_crd_list_or_watch_access(KubernetesBackend backend)
    {
        var services = Application.Current.GetTestServices();
        var crd = TestCustomResourceDefinitionFactory.Create("widgets.kubeui.com", "widgets", "value");
        var expectedKind = GetCustomResourceKind(crd);
        var config = CreateRestrictedClusterConfig(backend,
        [
            crd,
            .. KubernetesRbac.InNamespace(
                KubernetesRbac.ServiceAccountNamespace,
                new RbacRule("widgets", "list", "kubeui.com"),
                new RbacRule("widgets", "watch", "kubeui.com")),
        ]);
        await using var testCluster = await services.GetRequiredService<TestClusterGenerator>()
            .CreateAsync(config, TestContext.Current.CancellationToken);
        using var workspace = CreateWorkspace(testCluster, services);

        await workspace.Connect();

        workspace.Runtime.Connected.ShouldBeTrue();
        workspace.Runtime.LastError.ShouldBeNull();
        workspace.Runtime.ApiDiscoverySnapshot.ShouldNotBeNull()
            .Resources.ShouldContain(resource => resource.Kind == expectedKind);
        workspace.Runtime.ModelCatalog.IsCustomResource(expectedKind).ShouldBeTrue();

        var resourceConfig = await TestWait.UntilValueAsync(
            () => workspace.GetResourceConfigs().OfType<CustomResourceConfig>()
                .FirstOrDefault(candidate => candidate.Kind == expectedKind),
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        resourceConfig.ShouldNotBeNull();
        resourceConfig.CanListAndWatch.ShouldBeTrue();
        workspace.Runtime.Permissions.CanI<V1CustomResourceDefinition>(Verb.List).ShouldBeFalse();
        workspace.Runtime.Permissions.CanI<V1CustomResourceDefinition>(Verb.Watch).ShouldBeFalse();

        await resourceConfig.SeedResource(false, TestContext.Current.CancellationToken);

        var container = GetSeededContainer(workspace.Runtime, expectedKind);
        container.ShouldNotBeNull();
        GetInformers(container).Count.ShouldBe(1);
        var requestPaths = testCluster.FakeApi!.RequestUris.Select(static uri => uri?.AbsolutePath).ToArray();
        requestPaths.ShouldContain("/apis/kubeui.com/v1beta1/namespaces/my-app/widgets");
        requestPaths.ShouldNotContain("/apis/kubeui.com/v1beta1/widgets");
    }

    [AvaloniaFact]
    public async Task connected_workspace_registers_resource_config_models_in_catalog()
    {
        using var workspace = await Application.Current.CreateClusterAsync();

        workspace.Runtime.ModelCatalog
            .TryGetResourceKind("v1", "Pod", out var resourceKind)
            .ShouldBeTrue();
        resourceKind.ShouldBe(GroupApiVersionKind.From<V1Pod>());
        workspace.Runtime.ModelCatalog.TryGetResourceSeeder(resourceKind, out var seedResource).ShouldBeTrue();

        await seedResource(false, TestContext.Current.CancellationToken);

        await TestWait.UntilAsync(
            () => workspace.Runtime.Objects.TryGetValue(resourceKind, out var container)
                && container is IResourceContainer { IsSeeded: true },
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken);
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task resource_config_can_be_looked_up_by_resource_kind(KubernetesBackend backend)
    {
        using var workspace = await Application.Current.CreateClusterAsync(
            config => config.Type = backend);

        var expected = workspace.GetResourceConfig(GroupApiVersionKind.From<V1Pod>());
        workspace.GetResourceConfig(GroupApiVersionKind.From<V1Pod>()).ShouldBeSameAs(expected);
    }

    [AvaloniaFact]
    public async Task changing_runtime_status_to_connecting_updates_cluster_color()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);

        workspace.Runtime.Status = ClusterStatus.Connecting;
        await TestApplicationExtensions.WaitForUiAsync();

        workspace.ClusterColor.ShouldBe(Brushes.Orange);
    }

    [AvaloniaFact]
    public async Task namespace_updates_from_background_threads_update_bound_controls()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
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
        using var workspace = await Application.Current.CreateClusterAsync();
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
        using var workspace = await Application.Current.CreateClusterAsync(
            config => config.Type = backend);
        await workspace.Runtime.SeedResource<V1CustomResourceDefinition>(true);
        var crd = TestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");

        await workspace.Runtime.CreateAsync(crd, TestContext.Current.CancellationToken);

        var resourceKind = await WaitForRegisteredCustomResourceKindAsync(workspace.Runtime, crd);

        var resourceConfig = await TestWait.UntilValueAsync(
            () => GetCustomResourceConfig(workspace, crd),
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        resourceConfig.ShouldNotBeNull();
        resourceConfig.ShouldBeOfType<CustomResourceConfig>().Origin.ShouldBe(ResourceOrigin.ApiDiscovery);
        resourceConfig.Kind.ShouldBe(resourceKind);
        resourceConfig.IsCustomResource.ShouldBeTrue();
        resourceConfig.CustomResourceDefinition.ShouldBeNull();

        await resourceConfig.SeedResource(false, TestContext.Current.CancellationToken);
        await TestWait.UntilAsync(
            () => workspace.Runtime.Objects.TryGetValue(resourceKind, out var container)
                && container is IResourceContainer { IsSeeded: true },
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task custom_resource_config_is_dropped_on_disconnect_and_reregistered_when_the_crd_informer_lists_again(KubernetesBackend backend)
    {
        using var workspace = await Application.Current.CreateClusterAsync(config => config.Type = backend);
        await workspace.Runtime.SeedResource<V1CustomResourceDefinition>(true);
        var crd = TestCustomResourceDefinitionFactory.Create("replays.kubeui.com", "replays", "value");
        await workspace.Runtime.CreateAsync(crd, TestContext.Current.CancellationToken);

        var originalConfig = await TestWait.UntilValueAsync(
            () => GetCustomResourceConfig(workspace, crd),
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        originalConfig.ShouldNotBeNull();

        await workspace.Disconnect();

        workspace.GetResourceConfigs().ShouldNotContain(config => config.Kind == originalConfig.Kind);

        await workspace.Connect();
        await workspace.Runtime.SeedResource<V1CustomResourceDefinition>(true);

        var registeredConfig = await TestWait.UntilValueAsync(
            () => GetCustomResourceConfig(workspace, crd),
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        registeredConfig.ShouldNotBeNull();
        registeredConfig.Kind.ShouldBe(originalConfig.Kind);
        registeredConfig.ShouldNotBeSameAs(originalConfig);
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task resource_config_processed_event_observes_registered_crd_config(KubernetesBackend backend)
    {
        using var workspace = await Application.Current.CreateClusterAsync(
            config => config.Type = backend);
        await workspace.Runtime.SeedResource<V1CustomResourceDefinition>(true);
        TaskCompletionSource<IClusterResourceConfig> processedConfig =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        workspace.ResourceConfigProcessed += (_, resourceConfig) =>
        {
            if (resourceConfig.IsCustomResource)
            {
                processedConfig.TrySetResult(workspace.GetResourceConfig(resourceConfig.Kind));
            }
        };
        var crd = TestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");

        await workspace.Runtime.CreateAsync(crd, TestContext.Current.CancellationToken);

        var resourceConfig = await TestWait.UntilValueAsync(
            () => GetCustomResourceConfig(workspace, crd),
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        var observedConfig = await processedConfig.Task.WaitAsync(TestContext.Current.CancellationToken);
        observedConfig.ShouldBeSameAs(resourceConfig);
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task resource_config_processed_event_is_published_after_crd_registration(KubernetesBackend backend)
    {
        using var workspace = await Application.Current.CreateClusterAsync(
            config => config.Type = backend);
        await workspace.Runtime.SeedResource<V1CustomResourceDefinition>(true);
        System.Collections.Concurrent.ConcurrentQueue<IClusterResourceConfig> observedConfigs = new();
        workspace.ResourceConfigProcessed += (_, resourceConfig) =>
        {
            if (resourceConfig.IsCustomResource)
            {
                observedConfigs.Enqueue(workspace.GetResourceConfig(resourceConfig.Kind));
            }
        };
        var crd = TestCustomResourceDefinitionFactory.Create("events.kubeui.com", "events", "value");

        await workspace.Runtime.CreateAsync(crd, TestContext.Current.CancellationToken);

        var resourceConfig = await TestWait.UntilValueAsync(
            () => GetCustomResourceConfig(workspace, crd),
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        await TestWait.UntilAsync(
            () => !observedConfigs.IsEmpty,
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        observedConfigs.ToArray().ShouldHaveSingleItem().ShouldBeSameAs(resourceConfig);
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task crd_schema_update_keeps_discovery_backed_config_and_seeded_informer(KubernetesBackend backend)
    {
        using var workspace = await Application.Current.CreateClusterAsync(
            config => config.Type = backend);
        await workspace.Runtime.SeedResource<V1CustomResourceDefinition>(true);
        var originalCrd = TestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");
        originalCrd.Metadata.Generation = 1;

        await workspace.Runtime.CreateAsync(originalCrd, TestContext.Current.CancellationToken);

        var originalKind = await WaitForRegisteredCustomResourceKindAsync(workspace.Runtime, originalCrd);
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

        var updatedCrd = TestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "otherString");
        var currentCrd = workspace.Runtime.GetResource<V1CustomResourceDefinition>(null, originalCrd.Name()).ShouldNotBeNull();
        updatedCrd.Metadata.Uid = currentCrd.Metadata.Uid;
        updatedCrd.Metadata.ResourceVersion = currentCrd.Metadata.ResourceVersion;
        updatedCrd.Metadata.Generation = 2;
        await workspace.Runtime.ReplaceAsync(updatedCrd, TestContext.Current.CancellationToken);

        var updatedKind = await WaitForRegisteredCustomResourceKindAsync(workspace.Runtime, updatedCrd);
        updatedKind.ShouldBe(originalKind);

        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);
        var updatedResourceConfig = GetCustomResourceConfig(workspace, updatedCrd);
        updatedResourceConfig.ShouldBeSameAs(originalResourceConfig);
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
    public async Task metadata_only_crd_update_does_not_rebuild_resource_config(KubernetesBackend backend)
    {
        using var workspace = await Application.Current.CreateClusterAsync(
            config => config.Type = backend);
        await workspace.Runtime.SeedResource<V1CustomResourceDefinition>(true);
        var originalCrd = TestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");
        originalCrd.Metadata.Generation = 1;

        await workspace.Runtime.CreateAsync(originalCrd, TestContext.Current.CancellationToken);

        var originalKind = await WaitForRegisteredCustomResourceKindAsync(workspace.Runtime, originalCrd);
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

        var processedConfigs = 0;
        workspace.ResourceConfigProcessed += (_, resourceConfig) =>
        {
            if (resourceConfig.IsCustomResource)
            {
                Interlocked.Increment(ref processedConfigs);
            }
        };

        var updatedCrd = TestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");
        updatedCrd.Metadata.Uid = workspace.Runtime.GetResource<V1CustomResourceDefinition>(null, originalCrd.Name()).ShouldNotBeNull().Metadata.Uid;
        updatedCrd.Metadata.Generation = 1;
        updatedCrd.Metadata.Annotations = new Dictionary<string, string>
        {
            ["metadata-only"] = "true"
        };

        await workspace.Runtime.ReplaceAsync(updatedCrd, TestContext.Current.CancellationToken);
        await TestWait.UntilAsync(
            () => workspace.Runtime.GetResource<V1CustomResourceDefinition>(null, updatedCrd.Name())
                ?.Metadata?.Annotations?.ContainsKey("metadata-only") == true,
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

        GetCustomResourceConfig(workspace, updatedCrd).ShouldBeSameAs(originalResourceConfig);
        processedConfigs.ShouldBe(0);
        GetInformers(originalContainer).Count.ShouldBe(1);
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task seeding_resource_raises_resource_seeded_event(KubernetesBackend backend)
    {
        using var workspace = await Application.Current.CreateClusterAsync(
            config => config.Type = backend);
        GroupApiVersionKind? seededKind = null;
        workspace.Runtime.ResourceSeeded += (_, resourceKind) => seededKind = resourceKind;

        await workspace.Runtime.Permissions.UpdateCanI<V1Pod>(Verb.List);
        await workspace.Runtime.Permissions.UpdateCanI<V1Pod>(Verb.Watch);
        await workspace.Runtime.SeedResource<V1Pod>();

        seededKind.ShouldBe(GroupApiVersionKind.From<V1Pod>());
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task denied_resource_seed_does_not_raise_resource_seeded_event(KubernetesBackend backend)
    {
        using var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.Type = backend;
            config.AuthenticatedUser = KubernetesRbac.ServiceAccountUser;
            config.InitialYaml = KubernetesTestData.LimitedAccessWithNamespacePermissions;
        });
        var seeded = false;
        workspace.Runtime.ResourceSeeded += (_, _) => seeded = true;

        await workspace.Runtime.SeedResource<V1ClusterRole>();

        seeded.ShouldBeFalse();
        var container = GetSeededContainer(workspace.Runtime, GroupApiVersionKind.From<V1ClusterRole>());
        container.ShouldNotBeNull();
        GetInformers(container).ShouldBeEmpty();
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task initializing_workspace_seeds_custom_resource_definitions_when_allowed(KubernetesBackend backend)
    {
        using var workspace = await Application.Current.CreateClusterAsync(
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
        using var workspace = await Application.Current.CreateClusterAsync(
            config => config.Type = backend);
        await workspace.Runtime.SeedResource<V1CustomResourceDefinition>(true);
        var crd = TestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");

        await workspace.Runtime.CreateAsync(crd, TestContext.Current.CancellationToken);

        var resourceKind = await WaitForRegisteredCustomResourceKindAsync(workspace.Runtime, crd);
        await workspace.Runtime.Permissions.UpdateCanI(GetCustomResourceKind(crd), Verb.List);
        await workspace.Runtime.Permissions.UpdateCanI(GetCustomResourceKind(crd), Verb.Watch);
        await SeedResourceAsync(workspace.Runtime, resourceKind);

        var seededContainer = GetSeededContainer(workspace.Runtime, resourceKind);
        seededContainer.ShouldNotBeNull();
        GetInformers(seededContainer).Count.ShouldBe(1);

        var currentCrd = await TestWait.UntilValueAsync(
            () => workspace.Runtime.GetResource<V1CustomResourceDefinition>(null, crd.Name()),
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        crd.Metadata.Uid = currentCrd.ShouldNotBeNull().Metadata.Uid;
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
    public async Task deleted_crd_with_invalid_spec_still_removes_resource_config()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        await workspace.Runtime.SeedResource<V1CustomResourceDefinition>(true);
        var crd = TestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");
        crd.Metadata.Generation = 1;

        await workspace.Runtime.CreateAsync(crd, TestContext.Current.CancellationToken);

        var resourceKind = await WaitForRegisteredCustomResourceKindAsync(workspace.Runtime, crd);
        var resourceConfig = await TestWait.UntilValueAsync(
            () => GetCustomResourceConfig(workspace, crd),
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        resourceConfig.ShouldNotBeNull();

        GroupApiVersionKind? removedKind = null;
        workspace.CustomResourceRemoved += (_, kind) => removedKind = kind;

        var invalidCrd = TestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");
        var currentCrd = workspace.Runtime.GetResource<V1CustomResourceDefinition>(null, crd.Name()).ShouldNotBeNull();
        invalidCrd.Metadata.Uid = currentCrd.Metadata.Uid;
        invalidCrd.Metadata.Generation = 2;
        invalidCrd.Spec.Versions = [];
        await workspace.Runtime.ReplaceAsync(invalidCrd, TestContext.Current.CancellationToken);

        await TestWait.UntilAsync(
            () => GetCustomResourceConfig(workspace, crd) == null && removedKind == resourceKind,
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        removedKind.ShouldBe(resourceKind);
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task seeding_namespaced_resource_uses_known_namespaces_without_eager_resource_config_initialization(KubernetesBackend backend)
    {
        using var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.Type = backend;
            config.AuthenticatedUser = KubernetesRbac.ServiceAccountUser;
            config.InitialResources = new IKubernetesObject<V1ObjectMeta>[]
                    {
                        new V1Namespace { Metadata = new V1ObjectMeta { Name = "my-app" } },
                        new V1ServiceAccount
                        {
                            Metadata = new V1ObjectMeta
                            {
                                Name = KubernetesRbac.ServiceAccountName,
                                NamespaceProperty = KubernetesRbac.ServiceAccountNamespace,
                            },
                        },
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
        using var workspace = await Application.Current.CreateClusterAsync(
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
        using var workspace = await Application.Current.CreateClusterAsync(
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
        using var workspace = await Application.Current.CreateClusterAsync(
            config => config.Type = backend);
        await workspace.Runtime.SeedResource<V1CustomResourceDefinition>(true);
        var crd = TestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");

        await workspace.Runtime.CreateAsync(crd, TestContext.Current.CancellationToken);

        var resourceKind = await WaitForRegisteredCustomResourceKindAsync(workspace.Runtime, crd);

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
        using var workspace = await Application.Current.CreateClusterAsync(
            config => config.ResponseLatency = TimeSpan.FromMilliseconds(200),
            connect: false);

        var connectTask = workspace.Connect();

        // With a 200 ms response latency the connect cannot have completed yet, so the
        // call returning proves it did not block on the connection.
        connectTask.IsCompleted.ShouldBeFalse();

        await connectTask;
        workspace.Runtime.Connected.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task concurrent_connect_calls_share_one_in_flight_connection()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);

        var firstConnect = workspace.Connect();
        var secondConnect = workspace.Connect();

        secondConnect.ShouldBeSameAs(firstConnect);
        await Task.WhenAll(firstConnect, secondConnect);
        workspace.Runtime.Connected.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task connect_skips_workspace_initialization_when_runtime_remains_disconnected()
    {
        using var workspace = await Application.Current.CreateClusterAsync(
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
        using var workspace = await Application.Current.CreateClusterAsync(
            config => config.Type = backend);

        var crd = TestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");
        await workspace.Runtime.AddOrUpdateResource(crd);

        var resourceKind = await WaitForRegisteredCustomResourceKindAsync(workspace.Runtime, crd);

        await TestWait.UntilAsync(
            () => GetCustomResourceConfig(workspace, crd) != null,
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task manual_permission_refresh_reloads_rules_and_reevaluates_resource_configs(KubernetesBackend backend)
    {
        var services = Application.Current.GetTestServices();
        await using var testCluster = await services.GetRequiredService<TestClusterGenerator>().CreateAsync(
            CreateRestrictedClusterConfig(backend),
            TestContext.Current.CancellationToken);
        using var workspace = CreateWorkspace(testCluster, services);
        await workspace.Connect();

        var podKind = GroupApiVersionKind.From<V1Pod>();
        var podConfig = workspace.GetResourceConfig<V1Pod>(podKind);
        podConfig.CanListAndWatch.ShouldBeFalse();
        var processed = 0;
        workspace.ResourceConfigProcessed += (_, config) =>
        {
            if (config.Kind == podKind)
            {
                processed++;
            }
        };

        await GrantPodListAndWatchAsync(testCluster, "default", TestContext.Current.CancellationToken);
        await workspace.RefreshPermissionsAsync();

        podConfig.CanListAndWatch.ShouldBeTrue();
        processed.ShouldBeGreaterThan(0);

        await RevokePodListAndWatchAsync(testCluster, "default", TestContext.Current.CancellationToken);
        await workspace.RefreshPermissionsAsync();

        podConfig.CanListAndWatch.ShouldBeFalse();
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task namespace_set_change_refreshes_permissions_without_polling(KubernetesBackend backend)
    {
        var services = Application.Current.GetTestServices();
        await using var testCluster = await services.GetRequiredService<TestClusterGenerator>().CreateAsync(
            CreateRestrictedClusterConfig(backend),
            TestContext.Current.CancellationToken);
        using var workspace = CreateWorkspace(testCluster, services);
        await workspace.Connect();
        await TestWait.UntilAsync(
            () => workspace.Runtime.Namespaces.Count > 0,
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        var podConfig = workspace.GetResourceConfig<V1Pod>(GroupApiVersionKind.From<V1Pod>());
        await GrantPodListAndWatchAsync(testCluster, "default", TestContext.Current.CancellationToken);
        podConfig.CanListAndWatch.ShouldBeFalse();

        await testCluster.CreateAsAdminAsync(
            new V1Namespace { Metadata = new V1ObjectMeta { Name = "permission-team" } },
            TestContext.Current.CancellationToken);

        await TestWait.UntilAsync(
            () => podConfig.CanListAndWatch,
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task rbac_watch_refreshes_permissions_when_a_role_changes(KubernetesBackend backend)
    {
        var services = Application.Current.GetTestServices();
        await using var testCluster = await services.GetRequiredService<TestClusterGenerator>().CreateAsync(
            CreateRestrictedClusterConfig(
                backend,
                [
                    .. KubernetesRbac.InNamespace(
                        "default",
                        new RbacRule("roles", "list", "rbac.authorization.k8s.io"),
                        new RbacRule("roles", "watch", "rbac.authorization.k8s.io"),
                        new RbacRule("rolebindings", "list", "rbac.authorization.k8s.io"),
                        new RbacRule("rolebindings", "watch", "rbac.authorization.k8s.io")),
                    KubernetesRbac.NamespacedRole("default", "kubeui-test-rbac-watch"),
                    KubernetesRbac.NamespacedRoleBinding("default", "kubeui-test-rbac-watch", "kubeui-test-rbac-watch"),
                ]),
            TestContext.Current.CancellationToken);
        using var workspace = CreateWorkspace(testCluster, services);
        await workspace.Connect();

        var roleKind = GroupApiVersionKind.From<V1Role>();
        var roleContainer = GetSeededContainer(workspace.Runtime, roleKind);
        roleContainer.ShouldNotBeNull();
        GetInformers(roleContainer).ShouldNotBeEmpty();
        await workspace.Runtime.SeedResource<V1Role>(
            waitForReady: true,
            TestContext.Current.CancellationToken);

        var podConfig = workspace.GetResourceConfig<V1Pod>(GroupApiVersionKind.From<V1Pod>());
        await TestWait.UntilAsync(
            () => workspace.Runtime.GetResourceList<V1Role>().Any(role => role.Name() == "kubeui-test-rbac-watch"),
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        podConfig.CanListAndWatch.ShouldBeFalse();

        await testCluster.ReplaceAsAdminAsync(
            KubernetesRbac.NamespacedRole(
                "default",
                "kubeui-test-rbac-watch",
                new RbacRule("pods", "list"),
                new RbacRule("pods", "watch")),
            TestContext.Current.CancellationToken);
        await TestWait.UntilAsync(
            () => podConfig.CanListAndWatch,
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        await testCluster.ReplaceAsAdminAsync(
            KubernetesRbac.NamespacedRole("default", "kubeui-test-rbac-watch"),
            TestContext.Current.CancellationToken);
        await TestWait.UntilAsync(
            () => !podConfig.CanListAndWatch,
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task workspace_does_not_watch_rbac_without_list_and_watch_permission(KubernetesBackend backend)
    {
        using var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.Type = backend;
            config.AuthenticatedUser = KubernetesRbac.ServiceAccountUser;
            config.InitialResources = new IKubernetesObject<V1ObjectMeta>[]
                {
                    new V1Namespace { Metadata = new V1ObjectMeta { Name = "my-app" } },
                    new V1ServiceAccount
                    {
                        Metadata = new V1ObjectMeta
                        {
                            Name = KubernetesRbac.ServiceAccountName,
                            NamespaceProperty = KubernetesRbac.ServiceAccountNamespace,
                        },
                    },
                }
                .Concat(KubernetesRbac.ClusterWide(
                    new RbacRule("namespaces", "list"),
                    new RbacRule("namespaces", "watch")))
                .ToArray();
        });

        GetSeededContainer(workspace.Runtime, GroupApiVersionKind.From<V1Role>()).ShouldBeNull();
        GetSeededContainer(workspace.Runtime, GroupApiVersionKind.From<V1RoleBinding>()).ShouldBeNull();
        GetSeededContainer(workspace.Runtime, GroupApiVersionKind.From<V1ClusterRole>()).ShouldBeNull();
        GetSeededContainer(workspace.Runtime, GroupApiVersionKind.From<V1ClusterRoleBinding>()).ShouldBeNull();
    }

    private static ClusterWorkspace CreateWorkspace(
        TestCluster testCluster,
        IServiceProvider services)
    {
        return new ClusterWorkspace(
            testCluster.Cluster,
            services,
            services.GetRequiredService<ILogger<ClusterWorkspace>>(),
            services.GetRequiredService<Instrumentation>());
    }

    private const string PodListWatchRoleName = "kubeui-test-pod-list-watch";

    private static TestClusterConfig CreateRestrictedClusterConfig(
        KubernetesBackend backend,
        IEnumerable<IKubernetesObject<V1ObjectMeta>>? additionalResources = null)
    {
        List<IKubernetesObject<V1ObjectMeta>> resources =
        [
            new V1Namespace { Metadata = new V1ObjectMeta { Name = "my-app" } },
            new V1ServiceAccount
            {
                Metadata = new V1ObjectMeta
                {
                    Name = KubernetesRbac.ServiceAccountName,
                    NamespaceProperty = KubernetesRbac.ServiceAccountNamespace,
                },
            },
        ];
        resources.AddRange(KubernetesRbac.ClusterWide(
            new RbacRule("namespaces", "list"),
            new RbacRule("namespaces", "watch")));
        if (additionalResources is not null)
        {
            resources.AddRange(additionalResources);
        }

        return new TestClusterConfig
        {
            Type = backend,
            AuthenticatedUser = KubernetesRbac.ServiceAccountUser,
            InitialResources = resources,
        };
    }

    private static async Task GrantPodListAndWatchAsync(
        TestCluster testCluster,
        string @namespace,
        CancellationToken cancellationToken)
    {
        await testCluster.CreateAsAdminAsync(
            KubernetesRbac.NamespacedRole(
                @namespace,
                PodListWatchRoleName,
                new RbacRule("pods", "list"),
                new RbacRule("pods", "watch")),
            cancellationToken);
        await testCluster.CreateAsAdminAsync(
            KubernetesRbac.NamespacedRoleBinding(@namespace, PodListWatchRoleName, PodListWatchRoleName),
            cancellationToken);
    }

    private static Task RevokePodListAndWatchAsync(
        TestCluster testCluster,
        string @namespace,
        CancellationToken cancellationToken)
    {
        return testCluster.DeleteAsAdminAsync(
            KubernetesRbac.NamespacedRoleBinding(@namespace, PodListWatchRoleName, PodListWatchRoleName),
            cancellationToken);
    }

    private static async Task<GroupApiVersionKind> WaitForRegisteredCustomResourceKindAsync(
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

    private static IClusterResourceConfig? GetCustomResourceConfig(ClusterWorkspace workspace, V1CustomResourceDefinition crd)
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
