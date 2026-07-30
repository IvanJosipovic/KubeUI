using System.Collections;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reflection;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Controls;
using FluentAvalonia.UI.Controls;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Features.Resources.Visualization;
using KubeUI.Avalonia.Shell.Navigation;
using KubeUI.Avalonia.Tests.Features.Clusters.Workspace;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Testing;
using KubeUI.Kubernetes.Resources.Relationships;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Shell.Navigation;

public class NavigationViewModelTests : AvaloniaTestBase
{
    private readonly List<IDisposable> _disposables = [];

    public override void Dispose()
    {
        for (var index = _disposables.Count - 1; index >= 0; index--)
        {
            _disposables[index].Dispose();
        }

        base.Dispose();
    }

    private ClusterWorkspace CreateWorkspace(IClusterRuntime runtime)
    {
        var workspace = ActivatorUtilities.CreateInstance<ClusterWorkspace>(
            TestApp.CurrentServices ?? throw new InvalidOperationException("Test services are not initialized."),
            runtime);
        _disposables.Add(workspace);
        return workspace;
    }

    private NavigationViewModel CreateViewModel()
    {
        var vm = TestApp.CurrentServices?.GetRequiredService<NavigationViewModel>()
            ?? throw new InvalidOperationException("Test services are not initialized.");
        _disposables.Add(vm);
        return vm;
    }

    private static async Task<T?> WaitForValueAsync<T>(Func<T?> getValue, int timeoutMs = 3000, CancellationToken cancellationToken = default) where T : class
    {
        cancellationToken = cancellationToken == default ? TestContext.Current.CancellationToken : cancellationToken;
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Dispatcher.UIThread.RunJobs();

            var value = getValue();
            if (value != null)
            {
                return value;
            }

            await WaitForNextPollAsync(cancellationToken);
        }

        Dispatcher.UIThread.RunJobs();
        return getValue();
    }

    private static async Task WaitForAsync(Func<bool> predicate, int timeoutMs = 3000, CancellationToken cancellationToken = default)
    {
        cancellationToken = cancellationToken == default ? TestContext.Current.CancellationToken : cancellationToken;
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Dispatcher.UIThread.RunJobs();
            if (predicate())
            {
                return;
            }

            await WaitForNextPollAsync(cancellationToken);
        }

        Dispatcher.UIThread.RunJobs();
        predicate().ShouldBeTrue();
    }

    private static async Task<int?> WaitForCountAsync(IObservable<int>? count, int timeoutMs = 3000, CancellationToken cancellationToken = default)
    {
        cancellationToken = cancellationToken == default ? TestContext.Current.CancellationToken : cancellationToken;
        if (count == null)
        {
            return null;
        }

        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Dispatcher.UIThread.RunJobs();

            var nextValue = await count
                .Take(1)
                .Timeout(TimeSpan.FromMilliseconds(150))
                .Catch<int, TimeoutException>(_ => Observable.Empty<int>())
                .DefaultIfEmpty(int.MinValue);

            if (nextValue != int.MinValue)
            {
                return nextValue;
            }

            await WaitForNextPollAsync(cancellationToken);
        }

        Dispatcher.UIThread.RunJobs();
        return null;
    }

    [AvaloniaFact]
    public async Task cluster_catalog_changes_update_navigation_nodes()
    {
        using var vm = CreateViewModel();
        await using var firstScope = await KubernetesTestWorkspaceScope.CreateAsync(TestApp.CurrentServices!);
        await using var replacementScope = await KubernetesTestWorkspaceScope.CreateAsync(TestApp.CurrentServices!);
        var firstWorkspace = firstScope.Workspace;
        var replacementWorkspace = replacementScope.Workspace;
        firstWorkspace.Runtime.Name = "catalog-first";
        replacementWorkspace.Runtime.Name = "catalog-replacement";

        vm.ClusterCatalog.Clusters.Add(firstWorkspace);
        vm.Clusters.ShouldContain(node => ReferenceEquals(node.Cluster, firstWorkspace));

        vm.ClusterCatalog.Clusters.Remove(firstWorkspace);
        vm.Clusters.ShouldNotContain(node => ReferenceEquals(node.Cluster, firstWorkspace));

        vm.ClusterCatalog.Clusters.Add(firstWorkspace);
        var firstIndex = vm.ClusterCatalog.Clusters.IndexOf(firstWorkspace);
        firstIndex.ShouldBeGreaterThanOrEqualTo(0);
        vm.ClusterCatalog.Clusters[firstIndex] = replacementWorkspace;

        vm.Clusters.ShouldNotContain(node => ReferenceEquals(node.Cluster, firstWorkspace));
        vm.Clusters.ShouldContain(node => ReferenceEquals(node.Cluster, replacementWorkspace));

        vm.ClusterCatalog.Clusters.Clear();
        vm.Clusters.ShouldBeEmpty();
    }

    private static ResourceNavigationLink? FindResourceLink(ClusterNavigationNode root, string name)
    {
        return FindResourceLink(root.NavigationItems, name);
    }

    private static ResourceNavigationLink? FindResourceLink(ClusterNavigationNode root, Type resourceType)
    {
        return FindResourceLink(root.NavigationItems, resourceType);
    }

    private static ResourceNavigationLink? FindResourceLink(IEnumerable<NavigationItem> items, string name)
    {
        foreach (var child in items)
        {
            if (child is ResourceNavigationLink resourceLink && string.Equals(resourceLink.Name, name, StringComparison.Ordinal))
            {
                return resourceLink;
            }

            var nested = FindResourceLink(child.NavigationItems, name);
            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }

    private static ResourceNavigationLink? FindResourceLink(IEnumerable<NavigationItem> items, Type resourceType)
    {
        foreach (var child in items)
        {
            if (child is ResourceNavigationLink resourceLink && resourceLink.ControlType == resourceType)
            {
                return resourceLink;
            }

            var nested = FindResourceLink(child.NavigationItems, resourceType);
            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }

    private static async Task<int?> WaitForObservedCountAsync(IObservable<int>? count, int expected, int timeoutMs = 3000, CancellationToken cancellationToken = default)
    {
        cancellationToken = cancellationToken == default ? TestContext.Current.CancellationToken : cancellationToken;
        if (count == null)
        {
            return null;
        }

        var latest = int.MinValue;
        using var subscription = count.Subscribe(value => latest = value);
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Dispatcher.UIThread.RunJobs();

            if (latest == expected)
            {
                return latest;
            }

            await WaitForNextPollAsync(cancellationToken);
        }

        Dispatcher.UIThread.RunJobs();
        return latest == int.MinValue ? null : latest;
    }

    private static async Task WaitForNextPollAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(25));
        await timer.WaitForNextTickAsync(cancellationToken);
    }

    private static NavigationLink? FindNavigationLink(IEnumerable<NavigationItem> items, string viewModelKey)
    {
        foreach (var child in items)
        {
            if (child is NavigationLink navigationLink && string.Equals(navigationLink.ViewModelKey, viewModelKey, StringComparison.Ordinal))
            {
                return navigationLink;
            }

            var nested = FindNavigationLink(child.NavigationItems, viewModelKey);
            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }

    private static ResourceNavigationLink? FindNavigationLinkByName(
        IEnumerable<NavigationItem> items,
        string name)
    {
        foreach (var item in items)
        {
            if (item is ResourceNavigationLink link && link.Name == name)
            {
                return link;
            }

            var nested = FindNavigationLinkByName(item.NavigationItems, name);
            if (nested is not null)
            {
                return nested;
            }
        }

        return null;
    }

    private static Type? GetCustomResourceType(IClusterRuntime runtime, V1CustomResourceDefinition crd)
    {
        var version = crd.Spec?.Versions?.FirstOrDefault(x => x.Served && x.Storage)?.Name;
        return version == null ? null : runtime.ModelCache.GetResourceType(crd.Spec.Group, version, crd.Spec.Names.Kind);
    }

    private static async Task AddGeneratedCustomResourceAsync(ClusterWorkspace cluster, Type resourceType, V1CustomResourceDefinition crd, string @namespace, string name)
    {
        var resource = Activator.CreateInstance(resourceType).ShouldNotBeNull();
        var metadata = new V1ObjectMeta
        {
            NamespaceProperty = @namespace,
            Name = name,
            CreationTimestamp = DateTime.UtcNow,
        };

        resourceType.GetProperty(nameof(IKubernetesObject<V1ObjectMeta>.Metadata))?.SetValue(resource, metadata);
        resourceType.GetProperty(nameof(IKubernetesObject.ApiVersion))?.SetValue(resource, $"{crd.Spec.Group}/{crd.Spec.Versions.First(x => x.Served && x.Storage).Name}");
        resourceType.GetProperty(nameof(IKubernetesObject.Kind))?.SetValue(resource, crd.Spec.Names.Kind);

        var addOrUpdateMethod = typeof(IClusterRuntime)
            .GetMethods()
            .First(x => x.Name == nameof(IClusterRuntime.AddOrUpdateResource) && x.IsGenericMethodDefinition && x.GetParameters().Length == 1)
            .MakeGenericMethod(resourceType);

        await (Task)addOrUpdateMethod.Invoke(cluster.Runtime, [resource])!;
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaFact]
    public async Task resource_navigation_items_populate_only_after_connect_completes()
    {
        await using var runtimeScope = KubernetesScenarioClusterScope.CreateDisconnected();
        var runtime = runtimeScope.Cluster;

        var workspace = CreateWorkspace(runtime);
        workspace.AddResourceConfigForTest(new FakeResourceConfig(typeof(V1CustomResourceDefinition), "Definitions"));

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);

        clusterNode.NavigationItems.Count.ShouldBe(0);

        await workspace.Connect();

        await WaitForAsync(() => clusterNode.NavigationItems.Count > 0);
        clusterNode.IsExpanded.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task selecting_cluster_node_does_not_crash_when_connect_fails()
    {
        await using var runtimeScope = KubernetesScenarioClusterScope.CreateDisconnected(harness =>
            harness.FailConnection = true);
        var runtime = runtimeScope.Cluster;

        var workspace = CreateWorkspace(runtime);

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);

        await vm.TreeViewSelectionChangedAsync(clusterNode);

        await WaitForAsync(() => clusterNode.Cluster.Runtime.Status == ClusterStatus.Errored);
        clusterNode.NavigationItems.Count.ShouldBe(0);
    }

    [AvaloniaFact]
    public async Task selecting_cluster_node_does_not_block_on_slow_client_connection()
    {
        await using var runtimeScope = KubernetesScenarioClusterScope.CreateDisconnected(harness =>
            harness.ResponseDelay = TimeSpan.FromMilliseconds(300));
        var runtime = runtimeScope.Cluster;

        var workspace = CreateWorkspace(runtime);
        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var stopwatch = Stopwatch.StartNew();

        await vm.TreeViewSelectionChangedAsync(clusterNode);

        stopwatch.Stop();
        stopwatch.Elapsed.ShouldBeLessThan(TimeSpan.FromMilliseconds(150));
        clusterNode.IsExpanded.ShouldBeFalse();

        await WaitForAsync(() => runtime.Status is ClusterStatus.Connected or ClusterStatus.Errored, timeoutMs: 5000);
        await TestWait.NextPollAsync(TimeSpan.FromMilliseconds(25), TestContext.Current.CancellationToken);
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaFact]
    public async Task selecting_cluster_node_opens_cluster_error_document_when_connect_fails()
    {
        await using var runtimeScope = KubernetesScenarioClusterScope.CreateDisconnected(harness =>
            harness.FailConnection = true);
        var runtime = runtimeScope.Cluster;

        var workspace = CreateWorkspace(runtime);
        workspace.AddResourceConfigForTest(new FakeResourceConfig(typeof(V1CustomResourceDefinition), "Definitions"));
        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var documents = vm.Factory.GetDockable<IDocumentDock>("Documents");
        documents.ShouldNotBeNull();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);

        await vm.TreeViewSelectionChangedAsync(clusterNode);

        await WaitForAsync(() =>
            documents.VisibleDockables?.OfType<ClusterErrorViewModel>().Any(x => x.Id == "cluster-error") == true);

        var errorDocument = documents.VisibleDockables?
            .OfType<ClusterErrorViewModel>()
            .SingleOrDefault(x => x.Id == "cluster-error");

        errorDocument.ShouldNotBeNull();
        errorDocument.Error.ShouldContain("simulated connection failure");
    }

    [AvaloniaFact]
    public async Task selecting_cluster_node_without_namespace_list_permission_opens_settings_and_prompt()
    {
        await using var runtimeScope = KubernetesScenarioClusterScope.CreateDisconnected(harness =>
        {
            harness.DefaultPermissionAllowed = false;
            harness.SetPermission<V1Namespace>(Verb.List, false);
            harness.SetPermission<V1Namespace>(Verb.Watch, false);
        });
        var runtime = runtimeScope.Cluster;

        var workspace = CreateWorkspace(runtime);
        workspace.AddResourceConfigForTest(new FakeResourceConfig(typeof(V1CustomResourceDefinition), "Definitions"));
        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var documents = vm.Factory.GetDockable<IDocumentDock>("Documents");
        documents.ShouldNotBeNull();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        await vm.TreeViewSelectionChangedAsync(clusterNode);
        await workspace.Connect();
        ((Cluster)runtime).ListNamespaces.ShouldBeFalse();

        await WaitForAsync(() => runtime.Status == ClusterStatus.Errored);
        runtime.LastError.ShouldContain("cannot list namespaces");
        await WaitForAsync(() =>
            documents.VisibleDockables?.OfType<ClusterSettingsViewModel>().Any(x => x.Id == nameof(ClusterSettingsViewModel) + workspace.Runtime.Name) == true);

        var settingsDocument = documents.VisibleDockables!
            .OfType<ClusterSettingsViewModel>()
            .Single(x => x.Id == nameof(ClusterSettingsViewModel) + workspace.Runtime.Name);

        settingsDocument.Cluster.ShouldBe(workspace);
        runtime.Status.ShouldBe(ClusterStatus.Errored);

        TestApp.LastContentDialogSettings.ShouldNotBeNull();
        TestApp.LastContentDialogSettings.Title.ShouldBe(Assets.Resources.Cluster_Missing_Namespace_Permission_Title);
        TestApp.LastContentDialogSettings.Content.ShouldBe(Assets.Resources.Cluster_Missing_Namespace_Permission_Content);
        TestApp.LastContentDialogSettings.PrimaryButtonText.ShouldBe(Assets.Resources.Cluster_Missing_Namespace_Permission_Primary);
        TestApp.LastContentDialogSettings.DefaultButton.ShouldBe(FAContentDialogButton.Primary);
    }

    [AvaloniaFact]
    public async Task selecting_cluster_node_without_namespace_list_permission_reuses_existing_settings_document()
    {
        await using var runtimeScope = KubernetesScenarioClusterScope.CreateDisconnected(harness =>
            harness.DefaultPermissionAllowed = false);
        var runtime = runtimeScope.Cluster;

        var workspace = CreateWorkspace(runtime);
        workspace.AddResourceConfigForTest(new FakeResourceConfig(typeof(V1CustomResourceDefinition), "Definitions"));
        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var existingSettings = TestApp.CurrentServices?.GetRequiredService<ClusterSettingsViewModel>()
            ?? throw new InvalidOperationException("Test services are not initialized.");
        existingSettings.Initialize(workspace);
        vm.Factory.AddToDocuments(existingSettings);
        Dispatcher.UIThread.RunJobs();

        var documents = vm.Factory.GetDockable<IDocumentDock>("Documents");
        documents.ShouldNotBeNull();

        await WaitForAsync(() =>
            documents.VisibleDockables?.OfType<ClusterSettingsViewModel>().Count(x => x.Id == existingSettings.Id) == 1,
            10000);

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        await vm.TreeViewSelectionChangedAsync(clusterNode);

        await WaitForAsync(() =>
        {
            var visibleSettings = documents.VisibleDockables?.OfType<ClusterSettingsViewModel>().ToList();
            return visibleSettings?.Count(x => x.Id == existingSettings.Id) == 1;
        }, 10000);

        var visibleDockables = documents.VisibleDockables!.OfType<ClusterSettingsViewModel>().ToList();
        var matchingDockables = visibleDockables.Where(x => x.Id == existingSettings.Id).ToList();
        matchingDockables.Count.ShouldBe(1, $"visibleIds={string.Join(",", visibleDockables.Select(x => x.Id))}");
    }

    [AvaloniaFact]
    public async Task cluster_context_menu_disconnect_clears_navigation_and_updates_menu()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync();
        var runtime = runtimeScope.Cluster;

        var workspace = CreateWorkspace(runtime);
        await workspace.Connect();
        Dispatcher.UIThread.RunJobs();

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        clusterNode.NavigationItems.Count.ShouldBeGreaterThan(0);
        clusterNode.ConnectionMenuHeader.ShouldBe(Assets.Resources.NavigationView_ContextMenu_Disconnect);

        await vm.ToggleClusterConnectionCommand.ExecuteAsync(clusterNode).WaitAsync(TestContext.Current.CancellationToken);
        Dispatcher.UIThread.RunJobs();

        await WaitForAsync(() => !workspace.Runtime.Connected && workspace.Runtime.Status == ClusterStatus.None);
        clusterNode.NavigationItems.Count.ShouldBe(0);
        clusterNode.ConnectionMenuHeader.ShouldBe(Assets.Resources.NavigationView_ContextMenu_Connect);
    }

    [AvaloniaFact]
    public async Task selecting_cluster_node_with_namespace_fallback_does_not_open_settings_or_prompt()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync();
        var runtime = await runtimeScope.Harness.CreateLimitedAccessClusterAsync(includeNamespaceFallback: true);

        var workspace = CreateWorkspace(runtime);
        await workspace.Connect();
        var settingsService = TestApp.CurrentServices?.GetRequiredService<ISettingsService>()
            ?? throw new InvalidOperationException("Test services are not initialized.");
        settingsService.Settings.GetClusterSettings(workspace.Runtime).Namespaces!.Add("my-app");

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var documents = vm.Factory.GetDockable<IDocumentDock>("Documents");
        documents.ShouldNotBeNull();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        await vm.TreeViewSelectionChangedAsync(clusterNode);
        Dispatcher.UIThread.RunJobs();

        runtime.LastError.ShouldBeNull();
        runtime.Status.ShouldBe(ClusterStatus.Connected);
        documents.VisibleDockables!
            .OfType<ClusterSettingsViewModel>()
            .Any(x => x.Cluster == workspace)
            .ShouldBeFalse();
        TestApp.LastContentDialogSettings.ShouldBeNull();
    }

    [AvaloniaFact]
    public async Task selecting_cluster_node_with_namespace_fallback_shows_namespaced_resources_in_navigation()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync();
        var runtime = await runtimeScope.Harness.CreateLimitedAccessClusterAsync(includeNamespaceFallback: true);
        var workspace = CreateWorkspace(runtime);
        await workspace.Connect();
        var settingsService = TestApp.CurrentServices?.GetRequiredService<ISettingsService>()
            ?? throw new InvalidOperationException("Test services are not initialized.");
        settingsService.Settings.GetClusterSettings(workspace.Runtime).Namespaces!.Add("my-app");

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        await vm.TreeViewSelectionChangedAsync(clusterNode);

        await WaitForAsync(() =>
            FindResourceLink(clusterNode, "Pods") != null
            && FindResourceLink(clusterNode, "Deployments") != null);

        var podsLink = FindResourceLink(clusterNode, "Pods");
        podsLink.ShouldNotBeNull();
        var deploymentsLink = FindResourceLink(clusterNode, "Deployments");
        deploymentsLink.ShouldNotBeNull();
    }

    [AvaloniaFact]
    public async Task resource_context_menu_open_new_tab_creates_distinct_document_id()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync();
        var runtime = runtimeScope.Cluster;

        var workspace = CreateWorkspace(runtime);
        await workspace.Connect();
        Dispatcher.UIThread.RunJobs();

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var documents = vm.Factory.GetDockable<IDocumentDock>("Documents");
        documents.ShouldNotBeNull();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var podsLink = await WaitForValueAsync(() => FindResourceLink(clusterNode, "Pods"));
        podsLink.ShouldNotBeNull();
        podsLink.OpenCommand.ShouldNotBeNull();
        podsLink.OpenInNewTabCommand.ShouldNotBeNull();

        await vm.OpenResourceNavigationCommand.ExecuteAsync(podsLink).WaitAsync(TestContext.Current.CancellationToken);
        await vm.OpenResourceNavigationCommand.ExecuteAsync(podsLink).WaitAsync(TestContext.Current.CancellationToken);
        Dispatcher.UIThread.RunJobs();

        documents.VisibleDockables!
            .OfType<ResourceListViewModel<V1Pod>>()
            .Count()
            .ShouldBe(1);

        await vm.OpenResourceNavigationInNewTabCommand.ExecuteAsync(podsLink).WaitAsync(TestContext.Current.CancellationToken);
        Dispatcher.UIThread.RunJobs();

        await WaitForAsync(() =>
            documents.VisibleDockables?.OfType<ResourceListViewModel<V1Pod>>().Count() == 2);

        var podDocuments = documents.VisibleDockables!
            .OfType<ResourceListViewModel<V1Pod>>()
            .OrderBy(x => x.Id, StringComparer.Ordinal)
            .ToList();

        podDocuments.Count.ShouldBe(2);
        podDocuments[0].Id.ShouldBe($"{workspace.Runtime.Name}-{GroupApiVersionKind.From<V1Pod>()}");
        podDocuments[1].Id.ShouldBe($"{workspace.Runtime.Name}-{GroupApiVersionKind.From<V1Pod>()}#2");
    }

    [AvaloniaFact]
    public async Task selecting_cluster_node_with_settings_only_namespace_fallback_shows_namespaced_resources_in_navigation()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync();
        var runtime = await runtimeScope.Harness.CreateLimitedAccessClusterAsync(includeNamespaceFallback: false);
        var workspace = CreateWorkspace(runtime);
        var settingsService = TestApp.CurrentServices?.GetRequiredService<ISettingsService>()
            ?? throw new InvalidOperationException("Test services are not initialized.");
        settingsService.Settings.GetClusterSettings(workspace.Runtime).Namespaces!.Add("my-app");
        await workspace.Connect();

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        await vm.TreeViewSelectionChangedAsync(clusterNode);

        await WaitForAsync(() =>
            FindResourceLink(clusterNode, "Pods") != null
            && FindResourceLink(clusterNode, "Deployments") != null);

        runtime.Namespaces.Select(x => x.Name()).ShouldContain("my-app");
        FindResourceLink(clusterNode, "Pods").ShouldNotBeNull();
        FindResourceLink(clusterNode, "Deployments").ShouldNotBeNull();
    }

    [AvaloniaFact]
    public async Task namespaced_resource_link_stays_hidden_when_cached_config_flag_is_false()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync(harness =>
            harness.AddInitialResource(new V1Namespace { Metadata = new V1ObjectMeta { Name = "my-app" } }));
        var runtime = runtimeScope.Cluster;

        var workspace = CreateWorkspace(runtime);
        var settingsService = TestApp.CurrentServices?.GetRequiredService<ISettingsService>()
            ?? throw new InvalidOperationException("Test services are not initialized.");
        settingsService.Settings.GetClusterSettings(workspace.Runtime).Namespaces!.Add("my-app");
        await workspace.Connect();
        Dispatcher.UIThread.RunJobs();

        workspace.AddResourceConfigForTest(new FakeResourceConfig(
            typeof(TestPermissionResourceAlpha),
            "Alpha Permission Resource",
            canListAndWatch: false));

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        FindResourceLink(clusterNode, "Alpha Permission Resource").ShouldBeNull();
    }

    [AvaloniaFact]
    public async Task configured_namespaced_resource_link_is_visible_without_namespace_listing_access()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync(harness =>
            harness.AddInitialResource(new V1Namespace { Metadata = new V1ObjectMeta { Name = "my-app" } }));
        var runtime = runtimeScope.Cluster;

        var workspace = CreateWorkspace(runtime);
        var settingsService = TestApp.CurrentServices?.GetRequiredService<ISettingsService>()
            ?? throw new InvalidOperationException("Test services are not initialized.");
        settingsService.Settings.GetClusterSettings(workspace.Runtime).Namespaces!.Add("my-app");
        workspace.AddResourceConfigForTest(new FakeResourceConfig(typeof(TestPermissionResourceAlpha), "Alpha Permission Resource"));

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);

        FindResourceLink(clusterNode, "Alpha Permission Resource").ShouldNotBeNull();

        FindResourceLink(clusterNode, "Alpha Permission Resource").ShouldNotBeNull();
    }

    [AvaloniaFact]
    public async Task selecting_pods_in_limited_access_cluster_opens_populated_resource_list()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync();
        var runtime = await runtimeScope.Harness.CreateLimitedAccessClusterAsync(includeNamespaceFallback: false);
        var workspace = CreateWorkspace(runtime);
        var settingsService = TestApp.CurrentServices?.GetRequiredService<ISettingsService>()
            ?? throw new InvalidOperationException("Test services are not initialized.");
        settingsService.Settings.GetClusterSettings(workspace.Runtime).Namespaces!.Add("my-app");
        await workspace.Connect();

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var documents = vm.Factory.GetDockable<IDocumentDock>("Documents");
        documents.ShouldNotBeNull();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        await vm.TreeViewSelectionChangedAsync(clusterNode);

        await WaitForAsync(() => workspace.Runtime.Namespaces.Any(x => x.Name() == "my-app"));
        await WaitForAsync(() => workspace.GetResourceConfigs().Any(x => x.Type == typeof(V1Pod) && x.PermissionsLoaded));

        var podConfig = workspace.GetResourceConfig<V1Pod>();
        podConfig.CanListAndWatch.ShouldBeTrue();

        await WaitForAsync(() => FindResourceLink(clusterNode, "Pods") != null);

        var podsLink = FindResourceLink(clusterNode, "Pods");
        podsLink.ShouldNotBeNull();

        await vm.TreeViewSelectionChangedAsync(podsLink);

        await WaitForAsync(() =>
            documents.VisibleDockables?.OfType<ResourceListViewModel<V1Pod>>().Any(x => x.Id == $"{workspace.Runtime.Name}-{GroupApiVersionKind.From<V1Pod>()}") == true);

        var podsDocument = documents.VisibleDockables!
            .OfType<ResourceListViewModel<V1Pod>>()
            .Single(x => x.Id == $"{workspace.Runtime.Name}-{GroupApiVersionKind.From<V1Pod>()}");

        podsDocument.LoadError.ShouldBeNull();
        await runtime.AddOrUpdateResource(new V1Pod
        {
            Metadata = new V1ObjectMeta { Name = "pod-1", NamespaceProperty = "my-app" },
            Spec = new V1PodSpec
            {
                Containers = [new V1Container { Name = "app", Image = "busybox" }],
            },
            Status = new V1PodStatus
            {
                Phase = "Running",
                Conditions = [new V1PodCondition { Type = "Ready", Status = "True" }],
            },
        });
        await TestWait.UntilAsync(
            () => runtime.GetResource<V1Pod>("my-app", "pod-1") != null,
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);
        runtime.GetResource<V1Pod>("my-app", "pod-1").ShouldNotBeNull();
        await WaitForAsync(() => podsDocument.ItemCount > 0);
        podsDocument.ItemCount.ShouldBe(1);
    }

    [AvaloniaFact]
    public async Task cluster_node_expands_after_successful_connect()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync();
        var runtime = runtimeScope.Cluster;

        var workspace = CreateWorkspace(runtime);
        await workspace.Connect();
        Dispatcher.UIThread.RunJobs();

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        clusterNode.IsExpanded.ShouldBeFalse();

        await vm.TreeViewSelectionChangedAsync(clusterNode);

        await WaitForAsync(() => clusterNode.Cluster.Runtime.Connected && clusterNode.IsExpanded);
    }

    [AvaloniaFact]
    public async Task selecting_disconnected_cluster_initializes_navigation_on_demand()
    {
        await using var runtimeScope = KubernetesScenarioClusterScope.CreateDisconnected();
        var runtime = runtimeScope.Cluster;

        var workspace = CreateWorkspace(runtime);
        workspace.GetResourceConfigs().ShouldBeEmpty();

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);

        await vm.TreeViewSelectionChangedAsync(clusterNode);

        await WaitForAsync(() => workspace.GetResourceConfigs().Any());
        workspace.GetResourceConfigs().ShouldNotBeEmpty();
    }

    [AvaloniaFact]
    public async Task cluster_navigation_waits_for_permission_refresh_before_showing_resources()
    {
        await using var runtimeScope = KubernetesScenarioClusterScope.CreateDisconnected();
        var runtime = runtimeScope.Cluster;

        var permissionRefreshRelease = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var workspace = CreateWorkspace(runtime);
        workspace.AddResourceConfigForTest(new SlowPermissionResourceConfig(typeof(TestPermissionResourceGamma), "Gamma Permission Resource", permissionRefreshRelease.Task));

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);

        await vm.TreeViewSelectionChangedAsync(clusterNode);

        FindResourceLink(clusterNode, "Gamma Permission Resource").ShouldBeNull();

        permissionRefreshRelease.TrySetResult(null);

        await WaitForAsync(() => FindResourceLink(clusterNode, "Gamma Permission Resource") != null);
    }

    [AvaloniaFact]
    public async Task connect_path_publishes_ready_resources_without_waiting_for_unrelated_slow_permission_refresh()
    {
        await using var runtimeScope = KubernetesScenarioClusterScope.CreateDisconnected();
        var runtime = runtimeScope.Cluster;

        var slowPermissionRelease = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var workspace = CreateWorkspace(runtime);
        workspace.AddResourceConfigForTest(new DeferredPermissionResourceConfig(typeof(TestPermissionResourceAlpha), "Alpha Permission Resource"));
        workspace.AddResourceConfigForTest(new SlowPermissionResourceConfig(typeof(TestPermissionResourceGamma), "Gamma Permission Resource", slowPermissionRelease.Task));

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        await vm.TreeViewSelectionChangedAsync(clusterNode);

        await WaitForAsync(() => FindResourceLink(clusterNode, "Alpha Permission Resource") != null, timeoutMs: 250);
        FindResourceLink(clusterNode, "Gamma Permission Resource").ShouldBeNull();

        slowPermissionRelease.TrySetResult(null);

        await WaitForAsync(() => FindResourceLink(clusterNode, "Gamma Permission Resource") != null);
    }

    [AvaloniaFact]
    public async Task navigation_items_rebuild_after_resource_config_batch_publish()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync();
        var runtime = runtimeScope.Cluster;

        var workspace = CreateWorkspace(runtime);
        await workspace.Connect();
        Dispatcher.UIThread.RunJobs();

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var originalRoot = clusterNode.NavigationItems.Single(x => x.Name == "Custom Resource Definitions");
        var subtreeChanges = 0;
        originalRoot.NavigationItems.CollectionChanged += (_, _) => subtreeChanges++;

        workspace.AddResourceConfigForTest(new FakeCustomResourceConfig(typeof(TestCustomResourceAlpha), "Alpha Resources"));
        await WaitForAsync(() => subtreeChanges > 0);

        var updatedRoot = clusterNode.NavigationItems.Single(x => x.Name == "Custom Resource Definitions");
        ReferenceEquals(originalRoot, updatedRoot).ShouldBeTrue();
        subtreeChanges.ShouldBeGreaterThan(0);
    }

    [AvaloniaFact]
    public async Task resource_config_burst_preserves_existing_navigation_nodes()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync();
        var runtime = runtimeScope.Cluster;
        var workspace = CreateWorkspace(runtime);
        await workspace.Connect();
        Dispatcher.UIThread.RunJobs();

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var originalNamespaceLink = clusterNode.NavigationItems
            .OfType<ResourceNavigationLink>()
            .Single(x => x.ControlType == typeof(V1Namespace));
        var alphaConfig = new FakeResourceConfig(typeof(TestPermissionResourceAlpha), "Alpha Permission Resource");
        var betaConfig = new FakeResourceConfig(typeof(TestPermissionResourceBeta), "Beta Permission Resource");

        workspace.AddResourceConfigForTest(alphaConfig);
        workspace.AddResourceConfigForTest(betaConfig);

        await WaitForAsync(() =>
            FindResourceLink(clusterNode, alphaConfig.Name) != null
            && FindResourceLink(clusterNode, betaConfig.Name) != null);

        clusterNode.NavigationItems
            .OfType<ResourceNavigationLink>()
            .Single(x => x.ControlType == typeof(V1Namespace))
            .ShouldBeSameAs(originalNamespaceLink);
    }

    [AvaloniaFact]
    public async Task resource_navigation_items_appear_incrementally_as_permissions_complete()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync();
        var runtime = runtimeScope.Cluster;

        var workspace = CreateWorkspace(runtime);
        await workspace.Connect();
        Dispatcher.UIThread.RunJobs();

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var alphaConfig = new FakeResourceConfig(typeof(TestPermissionResourceAlpha), "Alpha Permission Resource");
        var betaConfig = new FakeResourceConfig(typeof(TestPermissionResourceBeta), "Beta Permission Resource");

        workspace.AddResourceConfigForTest(alphaConfig);

        var alphaObserved = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var betaConfigAdded = Task.Run(async () =>
        {
            await alphaObserved.Task;
            workspace.AddResourceConfigForTest(betaConfig);
        });

        var alphaLink = await WaitForValueAsync(
            () =>
            {
                var link = FindResourceLink(clusterNode, alphaConfig.Name);
                if (link != null)
                {
                    alphaObserved.TrySetResult();
                }

                return link;
            },
            timeoutMs: 150);

        alphaLink.ShouldNotBeNull();
        FindResourceLink(clusterNode, betaConfig.Name).ShouldBeNull();
        await betaConfigAdded;

        var betaLink = await WaitForValueAsync(
            () => FindResourceLink(clusterNode, betaConfig.Name),
            timeoutMs: 1000);

        betaLink.ShouldNotBeNull();
    }

    [AvaloniaFact]
    public async Task permission_driven_resource_add_keeps_existing_navigation_nodes()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync();
        var runtime = runtimeScope.Cluster;

        var workspace = CreateWorkspace(runtime);
        await workspace.Connect();
        Dispatcher.UIThread.RunJobs();

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var originalNamespaceLink = clusterNode.NavigationItems
            .OfType<ResourceNavigationLink>()
            .Single(x => x.ControlType == typeof(V1Namespace));

        workspace.AddResourceConfigForTest(new FakeResourceConfig(typeof(TestPermissionResourceAlpha), "Alpha Permission Resource"));
        await WaitForAsync(
            () => FindResourceLink(clusterNode, "Alpha Permission Resource") != null,
            timeoutMs: 1000);

        var updatedNamespaceLink = clusterNode.NavigationItems
            .OfType<ResourceNavigationLink>()
            .Single(x => x.ControlType == typeof(V1Namespace));

        ReferenceEquals(originalNamespaceLink, updatedNamespaceLink).ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task renaming_connected_cluster_preserves_resource_navigation()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync();
        var runtime = runtimeScope.Cluster;
        runtime.Name = "old-name";
        var workspace = CreateWorkspace(runtime);
        await workspace.Connect();
        Dispatcher.UIThread.RunJobs();

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        workspace.AddResourceConfigForTest(new FakeResourceConfig(
            typeof(TestPermissionResourceAlpha),
            "Alpha Permission Resource"));

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var resourceLink = await WaitForValueAsync(
            () => FindResourceLink(clusterNode, "Alpha Permission Resource"),
            timeoutMs: 1000);
        resourceLink.ShouldNotBeNull();

        runtime.Name = "new-name";

        FindResourceLink(clusterNode, "Alpha Permission Resource").ShouldBeSameAs(resourceLink);
        resourceLink!.Id.ShouldStartWith("new-name-");
        clusterNode.NavigationItems.ShouldNotBeEmpty();
    }

    [AvaloniaFact]
    public async Task resource_config_navigation_is_applied_immediately_on_ui_thread()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync();
        var runtime = runtimeScope.Cluster;
        var workspace = CreateWorkspace(runtime);
        await workspace.Connect();
        Dispatcher.UIThread.RunJobs();

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();
        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);

        workspace.AddResourceConfigForTest(new FakeResourceConfig(
            typeof(TestPermissionResourceAlpha),
            "Alpha Permission Resource"));

        FindResourceLink(clusterNode, "Alpha Permission Resource").ShouldNotBeNull();
    }

    [AvaloniaFact]
    public async Task port_forwarders_is_under_network_category_not_top_level()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync();
        var runtime = runtimeScope.Cluster;
        var workspace = CreateWorkspace(runtime);
        await workspace.Connect();
        Dispatcher.UIThread.RunJobs();

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);

        var networkCategory = clusterNode.NavigationItems.Single(x => x.Name == "Network");
        var portForwardersInNetwork = networkCategory.NavigationItems
            .OfType<NavigationLink>()
            .SingleOrDefault(x => x.ViewModelKey == NavigationTargets.PortForwarders);

        portForwardersInNetwork.ShouldNotBeNull();

        var topLevelPortForwarders = clusterNode.NavigationItems
            .OfType<NavigationLink>()
            .SingleOrDefault(x => x.ViewModelKey == NavigationTargets.PortForwarders);

        topLevelPortForwarders.ShouldBeNull();
    }

    [AvaloniaFact]
    public async Task port_forwarders_is_hidden_when_pod_portforward_is_not_allowed()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync(harness =>
        {
            harness.DefaultPermissionAllowed = false;
            harness.SetPermission<V1Pod>(Verb.Create, false, subresource: "portforward");
        });
        var runtime = runtimeScope.Cluster;

        var workspace = CreateWorkspace(runtime);
        await workspace.Connect();
        await TestWait.UntilAsync(
            () => !runtime.Permissions.CanIAnyNamespace<V1Pod>(Verb.Create, "portforward"),
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);
        Dispatcher.UIThread.RunJobs();

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var networkCategory = clusterNode.NavigationItems.SingleOrDefault(x => x.Name == "Network");
        if (networkCategory is not null)
        {
            networkCategory.NavigationItems
                .OfType<NavigationLink>()
                .SingleOrDefault(x => x.ViewModelKey == NavigationTargets.PortForwarders)
                .ShouldBeNull();
        }

        FindNavigationLink(clusterNode.NavigationItems, NavigationTargets.PortForwarders).ShouldBeNull();
    }

    [AvaloniaFact]
    public async Task initial_navigation_build_does_not_check_port_forward_until_pod_permissions_are_loaded()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync();
        var runtime = runtimeScope.Cluster;
        var harness = runtimeScope.Harness;
        var authorizationRequestsBeforeNavigation = harness.AuthorizationRequestCount;

        var workspace = CreateWorkspace(runtime);
        workspace.AddResourceConfigForTest(new FakeResourceConfig(typeof(V1Pod), "Pods")
        {
            PermissionsLoaded = false,
            CanListAndWatch = false,
        });

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        FindNavigationLink(clusterNode.NavigationItems, NavigationTargets.PortForwarders).ShouldBeNull();
        harness.AuthorizationRequestCount.ShouldBe(authorizationRequestsBeforeNavigation);
    }

    [AvaloniaFact]
    public async Task resource_navigation_updates_incrementally_and_port_forward_waits_for_pod_permissions()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync(harness =>
        {
            harness.DefaultPermissionAllowed = false;
            harness.SetPermission<V1Pod>(Verb.Create, true, subresource: "portforward");
        });
        var runtime = runtimeScope.Cluster;

        var workspace = CreateWorkspace(runtime);
        var podRelease = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var fastRefreshCompleted = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var podConfig = new BlockingPodPermissionResourceConfig(podRelease.Task);
        var fastConfig = new ImmediatePermissionResourceConfig(
            typeof(TestPermissionResourceAlpha),
            "Alpha Permission Resource",
            fastRefreshCompleted);

        workspace.AddResourceConfigForTest(podConfig);
        var podPermissionTask = podConfig.EvaluateListWatchAccessAsync();
        await fastConfig.EvaluateListWatchAccessAsync();
        workspace.AddResourceConfigForTest(fastConfig);

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);

        await WaitForAsync(() => fastRefreshCompleted.Task.IsCompleted);
        FindResourceLink(clusterNode, "Alpha Permission Resource").ShouldNotBeNull();
        FindNavigationLink(clusterNode.NavigationItems, NavigationTargets.PortForwarders).ShouldBeNull();

        podRelease.TrySetResult(null);

        await podPermissionTask;
        workspace.AddResourceConfigForTest(podConfig);
        await WaitForAsync(() => FindNavigationLink(clusterNode.NavigationItems, NavigationTargets.PortForwarders) != null);
    }

    [AvaloniaFact]
    public async Task connect_preloads_pod_default_and_custom_permissions()
    {
        await using var runtimeScope = KubernetesScenarioClusterScope.CreateDisconnected(harness =>
        {
            harness.DefaultPermissionAllowed = false;
            harness.AddInitialResource(new V1Namespace { Metadata = new V1ObjectMeta { Name = "my-app" } });
            harness.SetPermission<V1Namespace>(Verb.List, true);
            harness.SetPermission<V1Namespace>(Verb.Watch, true);
            harness.SetPermission<V1Pod>(Verb.List, true, "my-app");
            harness.SetPermission<V1Pod>(Verb.Watch, true, "my-app");
            harness.SetPermission<V1Pod>(Verb.Create, true, "my-app", "portforward");
        });
        var runtime = runtimeScope.Cluster;

        var workspace = CreateWorkspace(runtime);

        await workspace.Connect();
        Dispatcher.UIThread.RunJobs();

        runtime.Permissions.CanI<V1Pod>(Verb.Create, "my-app", "portforward").ShouldBeTrue();
        runtime.Permissions.CanI<V1Pod>(Verb.Create, subresource: "portforward").ShouldBeFalse();
        runtime.Permissions.CanI<V1Pod>(Verb.Get, "my-app", "log").ShouldBeFalse();
        runtime.Permissions.CanI<V1Pod>(Verb.Create, "my-app", "exec").ShouldBeFalse();
    }

    [AvaloniaFact]
    public async Task custom_resource_definitions_link_is_sorted_to_bottom()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync();
        var runtime = runtimeScope.Cluster;
        var workspace = CreateWorkspace(runtime);
        await workspace.Connect();
        Dispatcher.UIThread.RunJobs();

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        clusterNode.NavigationItems.Last().Name.ShouldBe("Custom Resource Definitions");
    }

    [AvaloniaFact]
    public async Task category_nav_items_follow_alpha_ordering()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync();
        var runtime = runtimeScope.Cluster;
        var workspace = CreateWorkspace(runtime);
        await workspace.Connect();
        Dispatcher.UIThread.RunJobs();

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var categoriesByName = clusterNode.NavigationItems
            .ToDictionary(x => x.Name, x => x.Order, StringComparer.Ordinal);

        categoriesByName["Workloads"].ShouldBeLessThan(categoriesByName["Configuration"]);
        categoriesByName["Configuration"].ShouldBeLessThan(categoriesByName["Network"]);
        categoriesByName["Network"].ShouldBeLessThan(categoriesByName["Storage"]);
        categoriesByName["Storage"].ShouldBeLessThan(categoriesByName["Access Control"]);
    }

    [AvaloniaFact]
    public async Task custom_resource_items_grouped_under_crd_link()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync();
        var runtime = runtimeScope.Cluster;
        var workspace = CreateWorkspace(runtime);
        await workspace.Connect();
        Dispatcher.UIThread.RunJobs();

        workspace.AddResourceConfigForTest(new FakeCustomResourceConfig(typeof(TestCustomResourceAlpha), "Alpha Resources"));
        workspace.AddResourceConfigForTest(new FakeCustomResourceConfig(typeof(TestCustomResourceBeta), "Beta Resources"));
        workspace.AddResourceConfigForTest(new FakeCustomResourceConfig(typeof(TestCustomResourceNested), "Nested Resources"));

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var crdRoot = clusterNode.NavigationItems
            .Single(x => x.Name == "Custom Resource Definitions");

        var rootNames = crdRoot.NavigationItems
            .OfType<NavigationItem>()
            .Select(x => x.Name)
            .ToList();

        rootNames.ShouldContain("Definitions");
        rootNames.ShouldContain("kubeui.com");

        var definitionsLink = crdRoot.NavigationItems
            .OfType<ResourceNavigationLink>()
            .Single(x => x.ControlType == typeof(V1CustomResourceDefinition));

        definitionsLink.Name.ShouldBe("Definitions");

        var rootGroup = crdRoot.NavigationItems
            .OfType<NavigationItem>()
            .Single(x => x.Name == "kubeui.com");

        var alphaGroup = rootGroup.NavigationItems
            .OfType<NavigationItem>()
            .Single(x => x.Name == "alpha.kubeui.com");

        alphaGroup.NavigationItems
            .OfType<ResourceNavigationLink>()
            .Single()
            .ControlType
            .ShouldBe(typeof(TestCustomResourceAlpha));

        var testGroup = rootGroup.NavigationItems
            .OfType<NavigationItem>()
            .Single(x => x.Name == "test.kubeui.com");

        var nestedGroup = testGroup.NavigationItems
            .OfType<NavigationItem>()
            .Single(x => x.Name == "mygroup.test.kubeui.com");

        nestedGroup.NavigationItems
            .OfType<ResourceNavigationLink>()
            .Single()
            .ControlType
            .ShouldBe(typeof(TestCustomResourceNested));

        clusterNode.NavigationItems
            .OfType<ResourceNavigationLink>()
            .Any(x => x.ControlType == typeof(TestCustomResourceAlpha))
            .ShouldBeFalse();
    }

    [AvaloniaFact]
    public async Task custom_resource_definitions_root_preserves_expansion_on_rebuild()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync();
        var runtime = runtimeScope.Cluster;
        var workspace = CreateWorkspace(runtime);
        await workspace.Connect();
        Dispatcher.UIThread.RunJobs();
        workspace.AddResourceConfigForTest(new FakeCustomResourceConfig(typeof(TestCustomResourceAlpha), "Alpha Resources"));

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var crdRoot = clusterNode.NavigationItems.Single(x => x.Name == "Custom Resource Definitions");
        crdRoot.IsExpanded = true;

        workspace.AddResourceConfigForTest(new FakeCustomResourceConfig(typeof(TestCustomResourceBeta), "Beta Resources"));
        await WaitForAsync(() => clusterNode.NavigationItems.Single(x => x.Name == "Custom Resource Definitions").IsExpanded);

        var rebuiltRoot = clusterNode.NavigationItems.Single(x => x.Name == "Custom Resource Definitions");
        rebuiltRoot.IsExpanded.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task resource_navigation_links_keep_counts_after_rebuild()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync();
        var runtime = runtimeScope.Cluster;

        var workspace = CreateWorkspace(runtime);
        await workspace.SeedResource<V1Namespace>();
        await workspace.Connect();
        Dispatcher.UIThread.RunJobs();

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var resourceLink = clusterNode.NavigationItems
            .OfType<ResourceNavigationLink>()
            .FirstOrDefault(x => x.ControlType == typeof(V1Namespace));

        resourceLink.ShouldNotBeNull();
        (resourceLink.Count is not null).ShouldBeTrue();

        workspace.AddResourceConfigForTest(new FakeCustomResourceConfig(typeof(TestCustomResourceAlpha), "Alpha Resources"));
        await WaitForAsync(() => clusterNode.NavigationItems
            .OfType<ResourceNavigationLink>()
            .Any(x => x.ControlType == typeof(V1Namespace) && x.Count is not null));

        var rebuiltResourceLink = clusterNode.NavigationItems
            .OfType<ResourceNavigationLink>()
            .FirstOrDefault(x => x.ControlType == typeof(V1Namespace));

        rebuiltResourceLink.ShouldNotBeNull();
        (rebuiltResourceLink.Count is not null).ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task stale_crd_navigation_link_opens_the_current_generated_resource_type()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync();
        var runtime = runtimeScope.Cluster;

        var workspace = CreateWorkspace(runtime);
        await workspace.Connect();
        await runtime.SeedResource<V1CustomResourceDefinition>(true);
        Dispatcher.UIThread.RunJobs();

        var originalCrd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");
        await runtime.AddOrUpdateResource(originalCrd);

        var originalType = await WaitForValueAsync(() => GetCustomResourceType(runtime, originalCrd));
        originalType.ShouldNotBeNull();
        await ((KubeUI.Kubernetes.Cluster)runtime).UpdateCanI(originalType, Verb.List);
        await ((KubeUI.Kubernetes.Cluster)runtime).UpdateCanI(originalType, Verb.Watch);
        await ((KubeUI.Kubernetes.Cluster)runtime).UpdatePermissionsAllNamespaceAsync(originalType, Verb.List);
        await ((KubeUI.Kubernetes.Cluster)runtime).UpdatePermissionsAllNamespaceAsync(originalType, Verb.Watch);

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var staleLink = await WaitForValueAsync(() => FindResourceLink(clusterNode, "Tests"));
        staleLink.ShouldNotBeNull();
        staleLink.ControlType.ShouldBe(originalType);

        var updatedCrd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "otherString");
        updatedCrd.Metadata.Uid = runtime.GetResource<V1CustomResourceDefinition>(null, originalCrd.Name()).ShouldNotBeNull().Metadata.Uid;
        await runtime.AddOrUpdateResource(updatedCrd);

        var updatedType = await WaitForValueAsync(() =>
        {
            var type = GetCustomResourceType(runtime, updatedCrd);
            return type != null && type != originalType ? type : null;
        });
        updatedType.ShouldNotBeNull();
        updatedType.ShouldNotBe(originalType);
        await ((KubeUI.Kubernetes.Cluster)runtime).UpdateCanI(updatedType, Verb.List);
        await ((KubeUI.Kubernetes.Cluster)runtime).UpdateCanI(updatedType, Verb.Watch);
        await ((KubeUI.Kubernetes.Cluster)runtime).UpdatePermissionsAllNamespaceAsync(updatedType, Verb.List);
        await ((KubeUI.Kubernetes.Cluster)runtime).UpdatePermissionsAllNamespaceAsync(updatedType, Verb.Watch);
        await AddGeneratedCustomResourceAsync(workspace, updatedType, updatedCrd, "default", "new-item");

        await vm.OpenResourceNavigationCommand.ExecuteAsync(staleLink).WaitAsync(TestContext.Current.CancellationToken);
        Dispatcher.UIThread.RunJobs();

        var documents = vm.Factory.GetDockable<IDocumentDock>("Documents");
        documents.ShouldNotBeNull();

        await WaitForAsync(() =>
            documents.VisibleDockables?.OfType<IResourceListViewModel>().Any(x =>
                ReferenceEquals(x.Cluster, workspace)
                && x.Kind == GroupApiVersionKind.From(updatedType)
                && x.ResourceConfig.Type == updatedType
                && x.ItemCount == 1) == true);

        var openedDocument = documents.VisibleDockables!
            .OfType<IResourceListViewModel>()
            .Single(x => ReferenceEquals(x.Cluster, workspace) && x.Kind == GroupApiVersionKind.From(updatedType));

        openedDocument.ResourceConfig.Type.ShouldBe(updatedType);
        openedDocument.ItemCount.ShouldBe(1);
    }

    [AvaloniaFact]
    public async Task selecting_unseeded_resource_navigation_link_keeps_count_blank()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync();
        var runtime = runtimeScope.Cluster;

        var workspace = CreateWorkspace(runtime);
        await workspace.Connect();
        Dispatcher.UIThread.RunJobs();

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var podsLink = FindResourceLink(clusterNode, "Pods");

        podsLink.ShouldNotBeNull();
        podsLink.Count.ShouldBeNull();
        await vm.TreeViewSelectionChangedAsync(podsLink);

        await WaitForAsync(() => podsLink.Count is not null, timeoutMs: 10000);
        var countTask = WaitForCountAsync(podsLink.Count!, timeoutMs: 10000);
        var count = await countTask;
        count.ShouldBe(0);
    }

    [AvaloniaFact]
    public async Task first_click_on_resource_navigation_link_shows_count()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync();
        var runtime = runtimeScope.Cluster;

        await runtime.AddOrUpdateResource(new V1Namespace
        {
            Metadata = new() { Name = "default" }
        });

        var workspace = CreateWorkspace(runtime);
        await workspace.Connect();
        Dispatcher.UIThread.RunJobs();

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var podsLink = await WaitForValueAsync(() => FindResourceLink(clusterNode, "Pods"));

        podsLink.ShouldNotBeNull();
        podsLink.Count.ShouldBeNull();
        await vm.TreeViewSelectionChangedAsync(podsLink);
        await WaitForAsync(() => podsLink.Count is not null, timeoutMs: 10000);
        var countTask = WaitForCountAsync(podsLink.Count!, timeoutMs: 10000);
        var count = await countTask;
        count.ShouldBe(0);
    }

    [AvaloniaFact]
    public async Task selecting_seeded_resource_navigation_link_shows_source_cache_count()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync();
        var runtime = runtimeScope.Cluster;

        await runtime.AddOrUpdateResource(new V1Namespace
        {
            Metadata = new() { Name = "default" }
        });

        var workspace = CreateWorkspace(runtime);
        await workspace.Connect();
        Dispatcher.UIThread.RunJobs();

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var podsLink = FindResourceLink(clusterNode, "Pods");

        podsLink.ShouldNotBeNull();
        podsLink.Count.ShouldBeNull();
        await runtime.SeedResource<V1Pod>();
        await WaitForAsync(() => podsLink.Count is not null, timeoutMs: 10000);
        var countTask = WaitForObservedCountAsync(podsLink.Count!, expected: 1, timeoutMs: 10000);
        await runtime.AddOrUpdateResource(new V1Pod
        {
            Metadata = new()
            {
                Name = "pod-one",
                NamespaceProperty = "default"
            }
        });

        await vm.TreeViewSelectionChangedAsync(podsLink);

        var count = await countTask;
        count.ShouldBe(1);
    }

    [AvaloniaFact]
    public async Task visualization_seeded_resource_attaches_navigation_count()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync();
        var runtime = runtimeScope.Cluster;
        await runtime.AddOrUpdateResource(new V1Namespace { Metadata = new() { Name = "default" } });

        var workspace = CreateWorkspace(runtime);
        await workspace.Connect();
        var navigation = CreateViewModel();
        navigation.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = navigation.Clusters.Single(x => x.Cluster == workspace);
        var podsLink = await WaitForValueAsync(() => FindResourceLink(clusterNode, typeof(V1Pod)));
        podsLink.ShouldNotBeNull();
        podsLink.Count.ShouldBeNull();

        using var visualization = new VisualizationViewModel(new ResourceRelationshipBuilder());
        visualization.Initialize(workspace);
        await runtime.SeedResource<V1Pod>();

        await WaitForAsync(() => podsLink.Count is not null, timeoutMs: 10000);
        var count = await WaitForObservedCountAsync(podsLink.Count, expected: 0, timeoutMs: 10000);

        count.ShouldBe(0);
    }

    [AvaloniaFact]
    public async Task resource_navigation_count_updates_when_events_arrive_after_initial_zero()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync();
        var runtime = runtimeScope.Cluster;

        var workspace = CreateWorkspace(runtime);
        await workspace.Connect();
        Dispatcher.UIThread.RunJobs();

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var eventsLink = await WaitForValueAsync(() => FindResourceLink(clusterNode, typeof(Corev1Event)));

        eventsLink.ShouldNotBeNull();
        (eventsLink.Count is not null).ShouldBeTrue();

        var directCountTask = WaitForObservedCountAsync(runtime.GetResourceCount<Corev1Event>(), expected: 1, timeoutMs: 10000);
        var zeroCount = await WaitForObservedCountAsync(eventsLink.Count, expected: 0, timeoutMs: 10000);
        zeroCount.ShouldBe(0);

        var countTask = WaitForObservedCountAsync(eventsLink.Count, expected: 1, timeoutMs: 10000);

        await runtime.AddOrUpdateResource(new Corev1Event
        {
            Metadata = new()
            {
                Name = "event-after-zero",
                NamespaceProperty = "default"
            },
            InvolvedObject = new()
            {
                Name = "pod-one",
                NamespaceProperty = "default",
                Kind = "Pod"
            },
            LastTimestamp = DateTime.UtcNow,
            Count = 1
        });

        var directCount = await directCountTask;
        directCount.ShouldBe(1);

        var count = await countTask;
        count.ShouldBe(1);
    }

    [AvaloniaFact]
    public async Task resource_navigation_count_updates_while_resource_events_continue()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync();
        var runtime = runtimeScope.Cluster;

        var workspace = CreateWorkspace(runtime);
        await workspace.Connect();
        Dispatcher.UIThread.RunJobs();

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var eventsLink = await WaitForValueAsync(() => FindResourceLink(clusterNode, typeof(Corev1Event)));
        eventsLink.ShouldNotBeNull();
        if (eventsLink.Count is null)
        {
            throw new InvalidOperationException("The Events navigation count was not attached.");
        }

        var positiveCount = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var subscription = eventsLink.Count.Subscribe(count =>
        {
            if (count > 0)
            {
                positiveCount.TrySetResult(count);
            }
        });

        for (var i = 0; i < 20; i++)
        {
            await runtime.AddOrUpdateResource(new Corev1Event
            {
                Metadata = new()
                {
                    Name = $"event-{i}",
                    NamespaceProperty = "default",
                },
                InvolvedObject = new()
                {
                    Name = "pod-one",
                    NamespaceProperty = "default",
                    Kind = "Pod",
                },
                LastTimestamp = DateTime.UtcNow,
                Count = 1,
            });

            await TestWait.NextPollAsync(TimeSpan.FromMilliseconds(20), TestContext.Current.CancellationToken);
            Dispatcher.UIThread.RunJobs();

            if (positiveCount.Task.IsCompleted)
            {
                break;
            }
        }

        positiveCount.Task.IsCompleted.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task resource_navigation_count_updates_from_real_runtime()
    {
        await using var scope = await KubernetesScenarioClusterScope.CreateAsync();
        var runtime = scope.Cluster;

        var workspace = CreateWorkspace(runtime);
        await workspace.Connect();
        Dispatcher.UIThread.RunJobs();

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var eventsLink = await WaitForValueAsync(() => FindResourceLink(clusterNode, typeof(Corev1Event)));
        eventsLink.ShouldNotBeNull();
        if (eventsLink.Count is null)
        {
            throw new InvalidOperationException("The Events navigation count was not attached.");
        }

        var countTask = WaitForObservedCountAsync(eventsLink.Count, expected: 1, timeoutMs: 10000);
        await runtime.AddOrUpdateResource(new Corev1Event
        {
            Metadata = new()
            {
                Name = "event-from-decorated-runtime",
                NamespaceProperty = "default",
            },
            InvolvedObject = new()
            {
                Name = "pod-one",
                NamespaceProperty = "default",
                Kind = "Pod",
            },
            LastTimestamp = DateTime.UtcNow,
            Count = 1,
        });

        (await countTask).ShouldBe(1);
    }

    [AvaloniaFact]
    public async Task resource_navigation_count_updates_for_events_after_connecting_from_navigation()
    {
        await using var runtimeScope = KubernetesScenarioClusterScope.CreateDisconnected();
        var runtime = runtimeScope.Cluster;

        var workspace = CreateWorkspace(runtime);
        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        await vm.TreeViewSelectionChangedAsync(clusterNode);
        await WaitForAsync(() => workspace.GetResourceConfigs().Any(x => x.Type == typeof(Corev1Event) && x.PermissionsLoaded));
        var eventsLink = await WaitForValueAsync(
            () => FindResourceLink(clusterNode, typeof(Corev1Event))?.Count is not null
                ? FindResourceLink(clusterNode, typeof(Corev1Event))
                : null,
            timeoutMs: 10000);
        eventsLink.ShouldNotBeNull();

        var zeroCount = await WaitForObservedCountAsync(eventsLink.Count, expected: 0, timeoutMs: 10000);
        zeroCount.ShouldBe(0);

        var countTask = WaitForObservedCountAsync(eventsLink.Count, expected: 1, timeoutMs: 10000);

        await runtime.AddOrUpdateResource(new Corev1Event
        {
            Metadata = new()
            {
                Name = "event-after-connect",
                NamespaceProperty = "default"
            },
            InvolvedObject = new()
            {
                Name = "pod-one",
                NamespaceProperty = "default",
                Kind = "Pod"
            },
            LastTimestamp = DateTime.UtcNow,
            Count = 1
        });

        var count = await countTask;
        count.ShouldBe(1);
    }

    [AvaloniaFact]
    public async Task resource_navigation_count_is_preserved_until_resource_is_seeded()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync();
        var runtime = runtimeScope.Cluster;

        var workspace = CreateWorkspace(runtime);

        var current = new ResourceNavigationLink
        {
            Cluster = workspace,
            Id = "cluster-events",
            Name = "Events",
            ControlType = typeof(Corev1Event),
            Order = 1,
            Count = Observable.Return(1),
            OpenCommand = new RelayCommand(() => { }),
            OpenInNewTabCommand = new RelayCommand(() => { }),
        };

        var desiredCount = Observable.Return(2);
        var desired = new ResourceNavigationLink
        {
            Cluster = workspace,
            Id = "cluster-events",
            Name = "Events",
            ControlType = typeof(Corev1Event),
            Order = 1,
            Count = desiredCount,
            OpenCommand = new RelayCommand(() => { }),
            OpenInNewTabCommand = new RelayCommand(() => { }),
        };

        var method = typeof(NavigationViewModel).GetMethod("UpdateNavigationItem", BindingFlags.Static | BindingFlags.NonPublic);
        method.ShouldNotBeNull();

        var originalCount = current.Count;
        method.Invoke(null, [current, desired]);

        current.Count.ShouldBeSameAs(originalCount);

        var count = await WaitForCountAsync(current.Count, timeoutMs: 1000);
        count.ShouldBe(1);
    }

    [AvaloniaFact]
    public async Task event_navigation_count_recovers_when_event_seed_happened_before_namespace_permission()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync(harness =>
        {
            harness.DefaultPermissionAllowed = false;
            harness.SetPermission<Corev1Event>(Verb.List, false);
            harness.SetPermission<Corev1Event>(Verb.Watch, false);
        });
        var runtime = runtimeScope.Cluster;
        var harness = runtimeScope.Harness;

        await runtime.SeedResource<Corev1Event>();
        runtime.Objects[GroupApiVersionKind.From<Corev1Event>()].ShouldBeOfType<ContainerClass<Corev1Event>>()
            .Informers.Count.ShouldBe(0);

        await runtime.AddOrUpdateResource(new V1Namespace
        {
            Metadata = new() { Name = "default" }
        });

        harness.SetPermission<Corev1Event>(Verb.List, true, "default");
        harness.SetPermission<Corev1Event>(Verb.Watch, true, "default");

        var workspace = CreateWorkspace(runtime);
        await workspace.Connect();
        Dispatcher.UIThread.RunJobs();

        runtime.Objects[GroupApiVersionKind.From<Corev1Event>()].ShouldBeOfType<ContainerClass<Corev1Event>>()
            .Informers.Count.ShouldBe(1);

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var eventsLink = await WaitForValueAsync(() => FindResourceLink(clusterNode, typeof(Corev1Event)));

        eventsLink.ShouldNotBeNull();
        (eventsLink.Count is not null).ShouldBeTrue();

        var countTask = WaitForObservedCountAsync(eventsLink.Count, expected: 1, timeoutMs: 10000);

        await runtime.AddOrUpdateResource(new Corev1Event
        {
            Metadata = new()
            {
                Name = "event-after-retry",
                NamespaceProperty = "default"
            },
            InvolvedObject = new()
            {
                Name = "pod-one",
                NamespaceProperty = "default",
                Kind = "Pod"
            },
            LastTimestamp = DateTime.UtcNow,
            Count = 1
        });

        var count = await countTask;
        count.ShouldBe(1);
    }

    [AvaloniaFact]
    public async Task crd_delta_does_not_rebuild_unrelated_resource_nodes()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync();
        var runtime = runtimeScope.Cluster;

        await runtime.AddOrUpdateResource(new V1Namespace
        {
            Metadata = new() { Name = "default" }
        });

        var workspace = CreateWorkspace(runtime);
        await workspace.Connect();
        Dispatcher.UIThread.RunJobs();

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var namespaceLink = clusterNode.NavigationItems
            .OfType<ResourceNavigationLink>()
            .Single(x => x.ControlType == typeof(V1Namespace));

        workspace.AddResourceConfigForTest(new FakeCustomResourceConfig(typeof(TestCustomResourceAlpha), "Alpha Resources"));
        await WaitForAsync(() => clusterNode.NavigationItems
            .OfType<ResourceNavigationLink>()
            .Any(x => x.ControlType == typeof(V1Namespace)));

        var rebuiltNamespaceLink = clusterNode.NavigationItems
            .OfType<ResourceNavigationLink>()
            .Single(x => x.ControlType == typeof(V1Namespace));

        ReferenceEquals(namespaceLink, rebuiltNamespaceLink).ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task namespace_addition_does_not_replace_namespace_navigation_link()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync();
        var runtime = runtimeScope.Cluster;

        await runtime.AddOrUpdateResource(new V1Namespace
        {
            Metadata = new() { Name = "default" }
        });

        var workspace = CreateWorkspace(runtime);
        await workspace.Connect();
        Dispatcher.UIThread.RunJobs();

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var namespaceLink = clusterNode.NavigationItems
            .OfType<ResourceNavigationLink>()
            .Single(x => x.ControlType == typeof(V1Namespace));
        var initialNamespaceCount = workspace.Runtime.Namespaces.Count;

        await runtime.AddOrUpdateResource(new V1Namespace
        {
            Metadata = new V1ObjectMeta
            {
                Name = "team-b"
            }
        });

        await WaitForAsync(() => workspace.Runtime.Namespaces.Count == initialNamespaceCount + 1);

        var updatedNamespaceLink = clusterNode.NavigationItems
            .OfType<ResourceNavigationLink>()
            .Single(x => x.ControlType == typeof(V1Namespace));

        ReferenceEquals(namespaceLink, updatedNamespaceLink).ShouldBeTrue();
        (updatedNamespaceLink.Count is not null).ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task custom_resource_definition_added_after_navigation_build_adds_custom_resource_entry()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync();
        var runtime = runtimeScope.Cluster;

        var workspace = CreateWorkspace(runtime);
        await workspace.Connect();
        Dispatcher.UIThread.RunJobs();
        workspace.AddResourceConfigForTest(new FakeResourceConfig(typeof(V1CustomResourceDefinition), "Definitions"));

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        workspace.AddResourceConfigForTest(new FakeCustomResourceConfig(typeof(TestCustomResourceKubeUi), "Tests"));
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var testsLink = await WaitForValueAsync(
            () => clusterNode.NavigationItems
                .OfType<NavigationItem>()
                .Where(x => x.Name == "Custom Resource Definitions")
                .SelectMany(x => x.NavigationItems.OfType<NavigationItem>())
                .Where(x => x.Name == "kubeui.com")
                .SelectMany(x => x.NavigationItems.OfType<ResourceNavigationLink>())
                .SingleOrDefault(x => x.Name == "Tests"),
            timeoutMs: 10000);

        testsLink.ShouldNotBeNull();
        testsLink.ControlType
            .ShouldNotBe(typeof(V1CustomResourceDefinition));
    }

    [AvaloniaFact]
    public async Task coalesced_custom_resource_updates_add_each_navigation_entry()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync();
        var runtime = runtimeScope.Cluster;

        var workspace = CreateWorkspace(runtime);
        await workspace.Connect();
        Dispatcher.UIThread.RunJobs();
        workspace.AddResourceConfigForTest(new FakeResourceConfig(typeof(V1CustomResourceDefinition), "Definitions"));

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        workspace.AddResourceConfigForTest(new FakeCustomResourceConfig(typeof(TestCustomResourceAlpha), "Alpha"));
        workspace.AddResourceConfigForTest(new FakeCustomResourceConfig(typeof(TestCustomResourceBeta), "Beta"));

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var alphaLink = await WaitForValueAsync(
            () => FindResourceLink(clusterNode, typeof(TestCustomResourceAlpha)),
            timeoutMs: 10000);
        var betaLink = await WaitForValueAsync(
            () => FindResourceLink(clusterNode, typeof(TestCustomResourceBeta)),
            timeoutMs: 10000);

        alphaLink.ShouldNotBeNull();
        betaLink.ShouldNotBeNull();
    }

    [AvaloniaFact]
    public async Task custom_resource_definition_update_updates_existing_navigation_entry_without_replacing_group()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync();
        var runtime = runtimeScope.Cluster;

        var workspace = CreateWorkspace(runtime);
        await workspace.Connect();
        Dispatcher.UIThread.RunJobs();
        workspace.AddResourceConfigForTest(new FakeResourceConfig(typeof(V1CustomResourceDefinition), "Definitions"));
        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        workspace.AddResourceConfigForTest(new FakeCustomResourceConfig(typeof(TestCustomResourceKubeUi), "Tests"));
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var crdRoot = await WaitForValueAsync(
            () => clusterNode.NavigationItems.SingleOrDefault(x => x.Name == "Custom Resource Definitions"),
            timeoutMs: 10000);
        crdRoot.ShouldNotBeNull();

        var rootGroup = await WaitForValueAsync(
            () => crdRoot.NavigationItems.OfType<NavigationItem>().SingleOrDefault(x => x.Name == "kubeui.com"),
            timeoutMs: 10000);
        rootGroup.ShouldNotBeNull();
        var originalRootGroup = rootGroup;

        var originalLink = await WaitForValueAsync(
            () => rootGroup.NavigationItems.OfType<ResourceNavigationLink>().SingleOrDefault(x => x.Name == "Tests"),
            timeoutMs: 10000);
        originalLink.ShouldNotBeNull();
        var originalCount = originalLink.Count;

        workspace.AddResourceConfigForTest(new FakeCustomResourceConfig(typeof(TestCustomResourceKubeUi), "Tests"));
        await WaitForAsync(() => crdRoot.NavigationItems.OfType<NavigationItem>().Any(x => x.Name == "kubeui.com"));

        var updatedRootGroup = await WaitForValueAsync(
            () => crdRoot.NavigationItems.OfType<NavigationItem>().SingleOrDefault(x => x.Name == "kubeui.com"),
            timeoutMs: 10000);
        updatedRootGroup.ShouldNotBeNull();
        ReferenceEquals(originalRootGroup, updatedRootGroup).ShouldBeTrue();

        var updatedLink = await WaitForValueAsync(
            () => updatedRootGroup.NavigationItems.OfType<ResourceNavigationLink>().SingleOrDefault(x => x.Name == "Tests"),
            timeoutMs: 10000);
        updatedLink.ShouldNotBeNull();
        ReferenceEquals(originalLink, updatedLink).ShouldBeTrue();
        ReferenceEquals(originalCount, updatedLink.Count).ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task custom_resource_definition_delete_removes_navigation_entry_without_rebuilding_remaining_groups()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync();
        var runtime = runtimeScope.Cluster;

        var workspace = CreateWorkspace(runtime);
        await workspace.Connect();
        Dispatcher.UIThread.RunJobs();
        workspace.AddResourceConfigForTest(new FakeResourceConfig(typeof(V1CustomResourceDefinition), "Definitions"));
        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var crdA = NavigationTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "Tests", "someString");
        var crdB = NavigationTestCustomResourceDefinitionFactory.Create("others.kubeui.com", "Others", "otherString");

        await runtime.AddOrUpdateResource(crdA);
        await runtime.AddOrUpdateResource(crdB);

        await TestWait.UntilAsync(
            () => runtime.GetResourceList<V1CustomResourceDefinition>().Count >= 2,
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var crdRoot = await WaitForValueAsync(
            () => clusterNode.NavigationItems.SingleOrDefault(x => x.Name == "Custom Resource Definitions"),
            timeoutMs: 10000);
        crdRoot.ShouldNotBeNull();

        var survivingGroup = await WaitForValueAsync(
            () => crdRoot.NavigationItems.OfType<NavigationItem>().SingleOrDefault(x => x.Name == "kubeui.com"),
            timeoutMs: 10000);
        survivingGroup.ShouldNotBeNull();
        await WaitForValueAsync(
            () => FindNavigationLinkByName(crdRoot.NavigationItems, "Tests"),
            timeoutMs: 10000);
        await WaitForValueAsync(
            () => FindNavigationLinkByName(survivingGroup.NavigationItems, "Others"),
            timeoutMs: 10000);
        await runtime.DeleteResource(crdA);
        await TestWait.UntilAsync(
            () => runtime.GetResource<V1CustomResourceDefinition>(null, crdA.Name()!) == null,
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);
        await WaitForAsync(
            () => FindNavigationLinkByName(crdRoot.NavigationItems, "Tests") is null,
            timeoutMs: 10000);

        await WaitForValueAsync(
            () => FindNavigationLinkByName(survivingGroup.NavigationItems, "Others"),
            timeoutMs: 10000);
    }

    [AvaloniaFact]
    public async Task custom_resource_definition_delete_prunes_empty_group_branch_without_replacing_root()
    {
        await using var runtimeScope = await KubernetesScenarioClusterScope.CreateAsync();
        var runtime = runtimeScope.Cluster;

        var workspace = CreateWorkspace(runtime);
        await workspace.Connect();
        Dispatcher.UIThread.RunJobs();
        workspace.AddResourceConfigForTest(new FakeResourceConfig(typeof(V1CustomResourceDefinition), "Definitions"));
        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var crd = NavigationTestCustomResourceDefinitionFactory.Create(
            name: "widgets.alpha.kubeui.com",
            plural: "Widgets",
            schemaProperty: "someString",
            group: "alpha.kubeui.com");

        await runtime.AddOrUpdateResource(crd);

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var crdRoot = await WaitForValueAsync(
            () => clusterNode.NavigationItems.SingleOrDefault(x => x.Name == "Custom Resource Definitions"),
            timeoutMs: 10000);
        crdRoot.ShouldNotBeNull();
        var originalRoot = crdRoot;

        var groupBranch = await WaitForValueAsync(
            () => crdRoot.NavigationItems.OfType<NavigationItem>().SingleOrDefault(x => x.Name == "kubeui.com"),
            timeoutMs: 10000);
        groupBranch.ShouldNotBeNull();

        await runtime.DeleteResource(crd);

        await WaitForAsync(
            () => !crdRoot.NavigationItems
                .OfType<NavigationItem>()
                .Any(x => x.Name == "kubeui.com"),
            timeoutMs: 10000);

        var updatedRoot = clusterNode.NavigationItems.Single(x => x.Name == "Custom Resource Definitions");
        ReferenceEquals(originalRoot, updatedRoot).ShouldBeTrue();
        updatedRoot.NavigationItems
            .OfType<ResourceNavigationLink>()
            .Single(x => x.ControlType == typeof(V1CustomResourceDefinition));
        updatedRoot.NavigationItems
            .Where(x => x is not ResourceNavigationLink)
            .Any()
            .ShouldBeFalse();
    }
}

internal static class NavigationTestCustomResourceDefinitionFactory
{
    public static V1CustomResourceDefinition Create(string name, string plural, string schemaProperty, string group = "kubeui.com")
    {
        return new V1CustomResourceDefinition
        {
            Metadata = new()
            {
                Name = name
            },
            Spec = new()
            {
                Group = group,
                Scope = "Namespaced",
                Names = new()
                {
                    Plural = plural.ToLowerInvariant().Replace(' ', '-'),
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

[KubernetesEntity(Group = "alpha.kubeui.com", ApiVersion = "v1", Kind = "TestCustomResourceAlpha")]
internal class TestCustomResourceAlpha : IKubernetesObject<V1ObjectMeta>
{
    public string ApiVersion { get; set; } = "alpha.kubeui.com/v1";
    public string Kind { get; set; } = "TestCustomResourceAlpha";
    public V1ObjectMeta Metadata { get; set; } = new();
}

[KubernetesEntity(Group = "beta.kubeui.com", ApiVersion = "v1", Kind = "TestCustomResourceBeta")]
internal class TestCustomResourceBeta : IKubernetesObject<V1ObjectMeta>
{
    public string ApiVersion { get; set; } = "beta.kubeui.com/v1";
    public string Kind { get; set; } = "TestCustomResourceBeta";
    public V1ObjectMeta Metadata { get; set; } = new();
}

[KubernetesEntity(Group = "kubeui.com", ApiVersion = "v1", Kind = "TestCustomResourceKubeUi")]
internal class TestCustomResourceKubeUi : IKubernetesObject<V1ObjectMeta>
{
    public string ApiVersion { get; set; } = "kubeui.com/v1";
    public string Kind { get; set; } = "TestCustomResourceKubeUi";
    public V1ObjectMeta Metadata { get; set; } = new();
}

[KubernetesEntity(Group = "mygroup.test.kubeui.com", ApiVersion = "v1", Kind = "TestCustomResourceNested")]
internal class TestCustomResourceNested : IKubernetesObject<V1ObjectMeta>
{
    public string ApiVersion { get; set; } = "mygroup.test.kubeui.com/v1";
    public string Kind { get; set; } = "TestCustomResourceNested";
    public V1ObjectMeta Metadata { get; set; } = new();
}

internal class FakeCustomResourceConfig : IResourceConfig
{
    public FakeCustomResourceConfig(Type resourceType, string name, bool canListAndWatch = true)
    {
        Type = resourceType;
        Name = name;
        CanListAndWatch = canListAndWatch;
    }

    public ClusterWorkspace? Cluster { get; private set; }
    public bool IsNamespaced => true;
    public bool CanListAndWatch { get; set; }
    public bool PermissionsLoaded { get; set; } = true;
    public bool ShowNewResource => true;
    public bool IsCustomResource => true;
    public GroupApiVersionKind Kind => GroupApiVersionKind.From(Type);
    public IList<IResourceListColumn> Columns() => Array.Empty<IResourceListColumn>();
    public IEnumerable<MenuItemViewModel> GetDefaultMenuItems(IEnumerable? selectedItems) => Array.Empty<MenuItemViewModel>();
    public IEnumerable<MenuItemViewModel> GetCustomMenuItems(IEnumerable? selectedItems) => Array.Empty<MenuItemViewModel>();
    public int Order { get; set; }
    public string Name { get; }
    public string? Category => null;
    public Style[] ListStyle() => [];
    public IEnumerable<(Verb verb, string? subresource)> Permissions() => [];
    public Task EvaluateListWatchAccessAsync() => Task.CompletedTask;
    public Type Type { get; }

    public IRelayCommand NewResourceCommand => throw new NotImplementedException();

    public IRelayCommand<IList> ViewCommand => throw new NotImplementedException();
    public IAsyncRelayCommand<IList> DeleteCommand => throw new NotImplementedException();

    public void Initialize(ClusterWorkspace cluster)
    {
        Cluster = cluster;
    }
}

internal class FakeResourceConfig : IResourceConfig
{
    public FakeResourceConfig(Type resourceType, string name, bool canListAndWatch = true)
    {
        Type = resourceType;
        Name = name;
        CanListAndWatch = canListAndWatch;
    }

    public ClusterWorkspace? Cluster { get; private set; }
    public bool IsNamespaced => true;
    public bool CanListAndWatch { get; set; }
    public bool PermissionsLoaded { get; set; } = true;
    public bool ShowNewResource => true;
    public bool IsCustomResource => false;
    public GroupApiVersionKind Kind => GroupApiVersionKind.From(Type);
    public IList<IResourceListColumn> Columns() => Array.Empty<IResourceListColumn>();
    public IEnumerable<MenuItemViewModel> GetDefaultMenuItems(IEnumerable? selectedItems) => Array.Empty<MenuItemViewModel>();
    public IEnumerable<MenuItemViewModel> GetCustomMenuItems(IEnumerable? selectedItems) => Array.Empty<MenuItemViewModel>();
    public int Order { get; set; }
    public string Name { get; }
    public string? Category => null;
    public Style[] ListStyle() => [];
    public IEnumerable<(Verb verb, string? subresource)> Permissions() => [];
    public Task EvaluateListWatchAccessAsync() => Task.CompletedTask;
    public Type Type { get; }

    public IRelayCommand NewResourceCommand => throw new NotImplementedException();

    public IRelayCommand<IList> ViewCommand => throw new NotImplementedException();
    public IAsyncRelayCommand<IList> DeleteCommand => throw new NotImplementedException();

    public void Initialize(ClusterWorkspace cluster)
    {
        Cluster = cluster;
    }
}

internal sealed class DeferredPermissionResourceConfig : IResourceConfig
{
    public DeferredPermissionResourceConfig(Type resourceType, string name)
    {
        Type = resourceType;
        Name = name;
    }

    public ClusterWorkspace? Cluster { get; private set; }
    public bool IsNamespaced => true;
    public bool CanListAndWatch { get; set; }
    public bool PermissionsLoaded { get; set; }
    public bool ShowNewResource => true;
    public bool IsCustomResource => false;
    public GroupApiVersionKind Kind => GroupApiVersionKind.From(Type);
    public IList<IResourceListColumn> Columns() => Array.Empty<IResourceListColumn>();
    public IEnumerable<MenuItemViewModel> GetDefaultMenuItems(IEnumerable? selectedItems) => Array.Empty<MenuItemViewModel>();
    public IEnumerable<MenuItemViewModel> GetCustomMenuItems(IEnumerable? selectedItems) => Array.Empty<MenuItemViewModel>();
    public int Order { get; set; }
    public string Name { get; }
    public string? Category => null;
    public Style[] ListStyle() => [];
    public IEnumerable<(Verb verb, string? subresource)> Permissions() => [];
    public Type Type { get; }

    public Task EvaluateListWatchAccessAsync()
    {
        CanListAndWatch = true;
        PermissionsLoaded = true;
        return Task.CompletedTask;
    }

    public IRelayCommand NewResourceCommand => throw new NotImplementedException();

    public IRelayCommand<IList> ViewCommand => throw new NotImplementedException();
    public IAsyncRelayCommand<IList> DeleteCommand => throw new NotImplementedException();

    public void Initialize(ClusterWorkspace cluster)
    {
        Cluster = cluster;
    }
}

[KubernetesEntity(Group = "permissions.alpha.kubeui.com", ApiVersion = "v1", Kind = "TestPermissionResourceAlpha")]
internal class TestPermissionResourceAlpha : IKubernetesObject<V1ObjectMeta>
{
    public string ApiVersion { get; set; } = "permissions.alpha.kubeui.com/v1";
    public string Kind { get; set; } = "TestPermissionResourceAlpha";
    public V1ObjectMeta Metadata { get; set; } = new();
}

[KubernetesEntity(Group = "permissions.beta.kubeui.com", ApiVersion = "v1", Kind = "TestPermissionResourceBeta")]
internal class TestPermissionResourceBeta : IKubernetesObject<V1ObjectMeta>
{
    public string ApiVersion { get; set; } = "permissions.beta.kubeui.com/v1";
    public string Kind { get; set; } = "TestPermissionResourceBeta";
    public V1ObjectMeta Metadata { get; set; } = new();
}

[KubernetesEntity(Group = "permissions.gamma.kubeui.com", ApiVersion = "v1", Kind = "TestPermissionResourceGamma")]
internal class TestPermissionResourceGamma : IKubernetesObject<V1ObjectMeta>
{
    public string ApiVersion { get; set; } = "permissions.gamma.kubeui.com/v1";
    public string Kind { get; set; } = "TestPermissionResourceGamma";
    public V1ObjectMeta Metadata { get; set; } = new();
}

internal sealed class SlowPermissionResourceConfig : IResourceConfig
{
    private readonly Task _permissionRefreshTask;

    public SlowPermissionResourceConfig(Type resourceType, string name, Task permissionRefreshTask)
    {
        Type = resourceType;
        Name = name;
        _permissionRefreshTask = permissionRefreshTask;
    }

    public ClusterWorkspace? Cluster { get; private set; }
    public bool IsNamespaced => true;
    public bool CanListAndWatch { get; set; }
    public bool PermissionsLoaded { get; set; }
    public bool ShowNewResource => true;
    public bool IsCustomResource => false;
    public GroupApiVersionKind Kind => GroupApiVersionKind.From(Type);
    public IList<IResourceListColumn> Columns() => Array.Empty<IResourceListColumn>();
    public IEnumerable<MenuItemViewModel> GetDefaultMenuItems(IEnumerable? selectedItems) => Array.Empty<MenuItemViewModel>();
    public IEnumerable<MenuItemViewModel> GetCustomMenuItems(IEnumerable? selectedItems) => Array.Empty<MenuItemViewModel>();
    public int Order { get; set; }
    public string Name { get; }
    public string? Category => null;
    public Style[] ListStyle() => [];
    public IEnumerable<(Verb verb, string? subresource)> Permissions() => [];
    public Type Type { get; }

    public async Task EvaluateListWatchAccessAsync()
    {
        await _permissionRefreshTask;
        CanListAndWatch = true;
        PermissionsLoaded = true;
    }

    public IRelayCommand NewResourceCommand => throw new NotImplementedException();

    public IRelayCommand<IList> ViewCommand => throw new NotImplementedException();
    public IAsyncRelayCommand<IList> DeleteCommand => throw new NotImplementedException();

    public void Initialize(ClusterWorkspace cluster)
    {
        Cluster = cluster;
    }
}
