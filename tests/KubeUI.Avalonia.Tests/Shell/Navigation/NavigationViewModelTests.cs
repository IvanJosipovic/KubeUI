using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Controls;
using Dock.Model.Core;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Resources.List.ViewModels;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Tests.Features.Clusters.Workspace;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Kubernetes;
using KubeUI.Testing;
using Moq;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Shell.Navigation;

public class NavigationViewModelTests : AvaloniaTestBase
{
    private readonly List<IDisposable> _disposables = [];

    public override void Dispose()
    {
        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }

        base.Dispose();
    }

    private ClusterWorkspaceViewModel CreateWorkspace(TestCluster runtime)
    {
        var workspace = runtime.CreateWorkspace();
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

    private static async Task<T?> WaitForValueAsync<T>(Func<T?> getValue, int timeoutMs = 3000) where T : class
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

        while (DateTime.UtcNow < deadline)
        {
            Dispatcher.UIThread.RunJobs();

            var value = getValue();
            if (value != null)
            {
                return value;
            }

            await Task.Delay(25);
        }

        Dispatcher.UIThread.RunJobs();
        return getValue();
    }

    private static async Task WaitForAsync(Func<bool> predicate, int timeoutMs = 3000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

        while (DateTime.UtcNow < deadline)
        {
            Dispatcher.UIThread.RunJobs();
            if (predicate())
            {
                return;
            }

            await Task.Delay(25);
        }

        Dispatcher.UIThread.RunJobs();
        predicate().ShouldBeTrue();
    }

    private static async Task<int?> WaitForCountAsync(IObservable<int>? count, int timeoutMs = 3000)
    {
        if (count == null)
        {
            return null;
        }

        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

        while (DateTime.UtcNow < deadline)
        {
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

            await Task.Delay(25);
        }

        Dispatcher.UIThread.RunJobs();
        return null;
    }

    private static ResourceNavigationLink? FindResourceLink(ClusterNavigationNode root, string name)
    {
        return FindResourceLink(root.NavigationItems, name);
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

    private static Type? GetCustomResourceType(TestClusterRuntime runtime, V1CustomResourceDefinition crd)
    {
        var version = crd.Spec?.Versions?.FirstOrDefault(x => x.Served && x.Storage)?.Name;
        return version == null ? null : runtime.ModelCache.GetResourceType(crd.Spec.Group, version, crd.Spec.Names.Kind);
    }

    private static async Task AddGeneratedCustomResourceAsync(ClusterWorkspaceViewModel cluster, Type resourceType, V1CustomResourceDefinition crd, string @namespace, string name)
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

        var addOrUpdateMethod = typeof(ClusterWorkspaceViewModel)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .First(x => x.Name == nameof(ClusterWorkspaceViewModel.AddOrUpdateResource) && x.IsGenericMethodDefinition && x.GetParameters().Length == 1)
            .MakeGenericMethod(resourceType);

        await (Task)addOrUpdateMethod.Invoke(cluster, [resource])!;
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaFact]
    public async Task resource_navigation_items_populate_only_after_connect_completes()
    {
        var runtime = new TestCluster
        {
            Connected = false,
            Status = ClusterStatus.None,
        };

        var releaseConnect = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

        runtime.ConnectBehavior = async () =>
        {
            runtime.Status = ClusterStatus.Connecting;
            await releaseConnect.Task;
            runtime.Connected = true;
            runtime.Status = ClusterStatus.Connected;
        };

        var workspace = CreateWorkspace(runtime);
        await workspace.SeedResource<V1Namespace>();
        await workspace.EnsureWorkspaceStateInitializedAsync();
        Dispatcher.UIThread.RunJobs();
        workspace.AddResourceConfigForTest(new FakeResourceConfig(typeof(V1CustomResourceDefinition), "Definitions"));

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);

        clusterNode.NavigationItems.Count.ShouldBe(0);

        await vm.TreeViewSelectionChangedAsync(clusterNode);

        await WaitForAsync(() => clusterNode.Cluster.Status == ClusterStatus.Connecting);
        clusterNode.NavigationItems.Count.ShouldBe(0);
        clusterNode.IsExpanded.ShouldBeFalse();

        releaseConnect.TrySetResult(null);

        await WaitForAsync(() => clusterNode.NavigationItems.Count > 0);
        clusterNode.IsExpanded.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task selecting_cluster_node_does_not_crash_when_connect_fails()
    {
        var runtime = new TestCluster
        {
            Connected = false,
            Status = ClusterStatus.None,
            ConnectBehavior = () => throw new InvalidOperationException("connect failed"),
        };

        var workspace = CreateWorkspace(runtime);
        await workspace.EnsureWorkspaceStateInitializedAsync();
        Dispatcher.UIThread.RunJobs();

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);

        await vm.TreeViewSelectionChangedAsync(clusterNode);

        await WaitForAsync(() => clusterNode.Cluster.Status == ClusterStatus.Errored);
        clusterNode.NavigationItems.Count.ShouldBe(0);
    }

    [AvaloniaFact]
    public async Task selecting_cluster_node_opens_cluster_error_document_when_connect_fails()
    {
        var runtime = new TestCluster
        {
            Connected = false,
            Status = ClusterStatus.None,
            ConnectBehavior = () => throw new InvalidOperationException("connect failed"),
        };

        var workspace = CreateWorkspace(runtime);
        await workspace.EnsureWorkspaceStateInitializedAsync();
        Dispatcher.UIThread.RunJobs();
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
        errorDocument.Error.ShouldBe("connect failed");
    }

    [AvaloniaFact]
    public async Task selecting_cluster_node_without_namespace_list_permission_opens_settings_and_prompt()
    {
        var runtime = new TestCluster
        {
            Name = "settings-reuse-" + Guid.NewGuid().ToString("N"),
            Connected = false,
            Status = ClusterStatus.None,
            ListNamespaces = false,
        };
        runtime.ConnectBehavior = () =>
        {
            runtime.Status = ClusterStatus.Errored;
            runtime.RequiresNamespaceSelectionPrompt = true;
            return Task.CompletedTask;
        };

        var workspace = CreateWorkspace(runtime);
        await workspace.EnsureWorkspaceStateInitializedAsync();
        Dispatcher.UIThread.RunJobs();
        workspace.AddResourceConfigForTest(new FakeResourceConfig(typeof(V1CustomResourceDefinition), "Definitions"));
        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var documents = vm.Factory.GetDockable<IDocumentDock>("Documents");
        documents.ShouldNotBeNull();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        await vm.TreeViewSelectionChangedAsync(clusterNode);

        await WaitForAsync(() =>
            documents.VisibleDockables?.OfType<ClusterSettingsViewModel>().Any(x => x.Id == nameof(ClusterSettingsViewModel) + workspace.Name) == true);

        var settingsDocument = documents.VisibleDockables!
            .OfType<ClusterSettingsViewModel>()
            .Single(x => x.Id == nameof(ClusterSettingsViewModel) + workspace.Name);

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
        var runtime = new TestCluster
        {
            Connected = false,
            Status = ClusterStatus.None,
            ListNamespaces = false,
        };
        runtime.ConnectBehavior = () =>
        {
            runtime.Status = ClusterStatus.Errored;
            runtime.RequiresNamespaceSelectionPrompt = true;
            return Task.CompletedTask;
        };

        var workspace = CreateWorkspace(runtime);
        await workspace.EnsureWorkspaceStateInitializedAsync();
        Dispatcher.UIThread.RunJobs();
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
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
        };

        var workspace = CreateWorkspace(runtime);
        await workspace.EnsureWorkspaceStateInitializedAsync();
        Dispatcher.UIThread.RunJobs();

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        clusterNode.NavigationItems.Count.ShouldBeGreaterThan(0);
        clusterNode.ConnectionMenuHeader.ShouldBe(Assets.Resources.NavigationView_ContextMenu_Disconnect);

        await vm.ToggleClusterConnectionCommand.ExecuteAsync(clusterNode);
        Dispatcher.UIThread.RunJobs();

        await WaitForAsync(() => !workspace.Connected && workspace.Status == ClusterStatus.None);
        clusterNode.NavigationItems.Count.ShouldBe(0);
        clusterNode.ConnectionMenuHeader.ShouldBe(Assets.Resources.NavigationView_ContextMenu_Connect);
    }

    [AvaloniaFact]
    public async Task selecting_cluster_node_with_namespace_fallback_does_not_open_settings_or_prompt()
    {
        var runtime = new TestCluster
        {
            Connected = false,
            Status = ClusterStatus.None,
            ListNamespaces = false,
        };
        var connectCompleted = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        runtime.ConnectBehavior = () =>
        {
            runtime.Connected = true;
            runtime.Status = ClusterStatus.Connected;
            runtime.RequiresNamespaceSelectionPrompt = false;
            connectCompleted.TrySetResult(null);
            return Task.CompletedTask;
        };

        var workspace = CreateWorkspace(runtime);
        var settingsService = TestApp.CurrentServices?.GetRequiredService<ISettingsService>()
            ?? throw new InvalidOperationException("Test services are not initialized.");
        settingsService.Settings.GetClusterSettings(workspace).Namespaces!.Add("my-app");

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var documents = vm.Factory.GetDockable<IDocumentDock>("Documents");
        documents.ShouldNotBeNull();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        await vm.TreeViewSelectionChangedAsync(clusterNode);
        await connectCompleted.Task.WaitAsync(TimeSpan.FromSeconds(5));
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
        var runtime = new TestCluster
        {
            Connected = false,
            Status = ClusterStatus.None,
            ListNamespaces = false,
            DefaultPermissionAllowed = false,
        };

        runtime.SetPermission<V1Pod>(Verb.List, true, "my-app");
        runtime.SetPermission<V1Pod>(Verb.Watch, true, "my-app");
        runtime.SetPermission<V1Deployment>(Verb.List, true, "my-app");
        runtime.SetPermission<V1Deployment>(Verb.Watch, true, "my-app");

        runtime.ConnectBehavior = async () =>
        {
            runtime.Connected = true;
            runtime.Status = ClusterStatus.Connected;
            runtime.RequiresNamespaceSelectionPrompt = false;

            await Task.CompletedTask;
        };

        var workspace = CreateWorkspace(runtime);
        var settingsService = TestApp.CurrentServices?.GetRequiredService<ISettingsService>()
            ?? throw new InvalidOperationException("Test services are not initialized.");
        settingsService.Settings.GetClusterSettings(workspace).Namespaces!.Add("my-app");

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
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
        };

        var workspace = CreateWorkspace(runtime);
        await workspace.EnsureWorkspaceStateInitializedAsync();
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

        await vm.OpenResourceNavigationCommand.ExecuteAsync(podsLink);
        await vm.OpenResourceNavigationCommand.ExecuteAsync(podsLink);
        Dispatcher.UIThread.RunJobs();

        documents.VisibleDockables!
            .OfType<ResourceListViewModel<V1Pod>>()
            .Count()
            .ShouldBe(1);

        await vm.OpenResourceNavigationInNewTabCommand.ExecuteAsync(podsLink);
        Dispatcher.UIThread.RunJobs();

        await WaitForAsync(() =>
            documents.VisibleDockables?.OfType<ResourceListViewModel<V1Pod>>().Count() == 2);

        var podDocuments = documents.VisibleDockables!
            .OfType<ResourceListViewModel<V1Pod>>()
            .OrderBy(x => x.Id, StringComparer.Ordinal)
            .ToList();

        podDocuments.Count.ShouldBe(2);
        podDocuments[0].Id.ShouldBe($"{workspace.Name}-{GroupApiVersionKind.From<V1Pod>()}");
        podDocuments[1].Id.ShouldBe($"{workspace.Name}-{GroupApiVersionKind.From<V1Pod>()}#2");
    }

    [AvaloniaFact]
    public async Task selecting_cluster_node_with_settings_only_namespace_fallback_shows_namespaced_resources_in_navigation()
    {
        var runtime = new TestCluster
        {
            Connected = false,
            Status = ClusterStatus.None,
            ListNamespaces = false,
            DefaultPermissionAllowed = false,
        };

        runtime.SetPermission<V1Pod>(Verb.List, true, "my-app");
        runtime.SetPermission<V1Pod>(Verb.Watch, true, "my-app");
        runtime.SetPermission<V1Deployment>(Verb.List, true, "my-app");
        runtime.SetPermission<V1Deployment>(Verb.Watch, true, "my-app");

        runtime.ConnectBehavior = () =>
        {
            runtime.Connected = true;
            runtime.Status = ClusterStatus.Connected;
            runtime.RequiresNamespaceSelectionPrompt = false;
            return Task.CompletedTask;
        };

        var workspace = CreateWorkspace(runtime);
        var settingsService = TestApp.CurrentServices?.GetRequiredService<ISettingsService>()
            ?? throw new InvalidOperationException("Test services are not initialized.");
        settingsService.Settings.GetClusterSettings(workspace).Namespaces!.Add("my-app");

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
    public async Task namespaced_resource_link_uses_namespace_permissions_even_when_cached_config_flag_is_false()
    {
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
            ListNamespaces = false,
            DefaultPermissionAllowed = false,
        };

        runtime.SetPermission<TestPermissionResourceAlpha>(Verb.List, true, "my-app");
        runtime.SetPermission<TestPermissionResourceAlpha>(Verb.Watch, true, "my-app");

        var workspace = CreateWorkspace(runtime);
        var settingsService = TestApp.CurrentServices?.GetRequiredService<ISettingsService>()
            ?? throw new InvalidOperationException("Test services are not initialized.");
        settingsService.Settings.GetClusterSettings(workspace).Namespaces!.Add("my-app");
        await workspace.EnsureWorkspaceStateInitializedAsync();
        Dispatcher.UIThread.RunJobs();

        workspace.AddResourceConfigForTest(new FakeResourceConfig(
            typeof(TestPermissionResourceAlpha),
            "Alpha Permission Resource",
            canListAndWatch: false));

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var link = await WaitForValueAsync(() => FindResourceLink(clusterNode, "Alpha Permission Resource"));

        link.ShouldNotBeNull();
    }

    [AvaloniaFact]
    public async Task namespaced_resource_link_stays_hidden_until_settings_namespace_is_materialized_into_runtime_namespaces()
    {
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
            ListNamespaces = false,
            DefaultPermissionAllowed = false,
        };

        runtime.SetPermission<TestPermissionResourceAlpha>(Verb.List, true, "my-app");
        runtime.SetPermission<TestPermissionResourceAlpha>(Verb.Watch, true, "my-app");

        var workspace = CreateWorkspace(runtime);
        var settingsService = TestApp.CurrentServices?.GetRequiredService<ISettingsService>()
            ?? throw new InvalidOperationException("Test services are not initialized.");
        settingsService.Settings.GetClusterSettings(workspace).Namespaces!.Add("my-app");
        workspace.AddResourceConfigForTest(new FakeResourceConfig(typeof(TestPermissionResourceAlpha), "Alpha Permission Resource"));

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);

        FindResourceLink(clusterNode, "Alpha Permission Resource").ShouldBeNull();

        await workspace.EnsureWorkspaceStateInitializedAsync();
        Dispatcher.UIThread.RunJobs();

        var link = await WaitForValueAsync(() => FindResourceLink(clusterNode, "Alpha Permission Resource"));
        link.ShouldNotBeNull();
    }

    [AvaloniaFact]
    public async Task selecting_pods_in_limited_access_cluster_opens_populated_resource_list()
    {
        var runtime = new TestCluster
        {
            Connected = false,
            Status = ClusterStatus.None,
            ListNamespaces = false,
            DefaultPermissionAllowed = false,
        };

        runtime.SetPermission<V1Pod>(Verb.List, true, "my-app");
        runtime.SetPermission<V1Pod>(Verb.Watch, true, "my-app");
        runtime.SetPermission<V1Pod>(Verb.Get, true, "my-app");
        runtime.SetPermission<V1Pod>(Verb.Get, true, "my-app", "log");
        runtime.SetPermission<V1Pod>(Verb.Create, true, "my-app", "exec");
        runtime.SetPermission<V1Pod>(Verb.Create, true, "my-app", "portforward");

        runtime.ConnectBehavior = async () =>
        {
            await runtime.AddOrUpdateResource(new V1Pod
            {
                Metadata = new()
                {
                    Name = "pod-1",
                    NamespaceProperty = "my-app"
                },
                Spec = new()
                {
                    Containers =
                    [
                        new V1Container
                        {
                            Name = "app"
                        }
                    ]
                },
                Status = new()
                {
                    Conditions =
                    [
                        new V1PodCondition
                        {
                            Type = "Ready",
                            Status = "True"
                        }
                    ]
                }
            });

            runtime.Connected = true;
            runtime.Status = ClusterStatus.Connected;
            runtime.RequiresNamespaceSelectionPrompt = false;
        };

        var workspace = CreateWorkspace(runtime);
        var settingsService = TestApp.CurrentServices?.GetRequiredService<ISettingsService>()
            ?? throw new InvalidOperationException("Test services are not initialized.");
        settingsService.Settings.GetClusterSettings(workspace).Namespaces!.Add("my-app");

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var documents = vm.Factory.GetDockable<IDocumentDock>("Documents");
        documents.ShouldNotBeNull();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        await vm.TreeViewSelectionChangedAsync(clusterNode);

        await WaitForAsync(() => workspace.Namespaces.Any(x => x.Name() == "my-app"));
        await WaitForAsync(() => workspace.GetResourceConfigs().Any(x => x.Type == typeof(V1Pod) && x.PermissionsLoaded));

        var podConfig = workspace.GetResourceConfig<V1Pod>();
        podConfig.CanListAndWatch.ShouldBeTrue();

        await WaitForAsync(() => FindResourceLink(clusterNode, "Pods") != null);

        var podsLink = FindResourceLink(clusterNode, "Pods");
        podsLink.ShouldNotBeNull();

        await vm.TreeViewSelectionChangedAsync(podsLink);

        await WaitForAsync(() =>
            documents.VisibleDockables?.OfType<ResourceListViewModel<V1Pod>>().Any(x => x.Id == $"{workspace.Name}-{GroupApiVersionKind.From<V1Pod>()}") == true);

        var podsDocument = documents.VisibleDockables!
            .OfType<ResourceListViewModel<V1Pod>>()
            .Single(x => x.Id == $"{workspace.Name}-{GroupApiVersionKind.From<V1Pod>()}");

        podsDocument.LoadError.ShouldBeNull();
        await WaitForAsync(() => podsDocument.ItemCount > 0);
        podsDocument.ItemCount.ShouldBe(1);
    }

    [AvaloniaFact]
    public async Task delayed_namespace_population_after_connect_still_shows_namespaced_resources_in_navigation()
    {
        var runtime = new TestCluster
        {
            Connected = false,
            Status = ClusterStatus.None,
            ListNamespaces = false,
            DefaultPermissionAllowed = false,
        };

        runtime.SetPermission<V1Pod>(Verb.List, true, "my-app");
        runtime.SetPermission<V1Pod>(Verb.Watch, true, "my-app");
        runtime.SetPermission<V1Deployment>(Verb.List, true, "my-app");
        runtime.SetPermission<V1Deployment>(Verb.Watch, true, "my-app");

        runtime.ConnectBehavior = async () =>
        {
            runtime.Connected = true;
            runtime.Status = ClusterStatus.Connected;

            await Task.Delay(50);

            await runtime.AddOrUpdateResource(new V1Namespace
            {
                Metadata = new() { Name = "my-app" }
            });
        };

        var workspace = CreateWorkspace(runtime);
        var settingsService = TestApp.CurrentServices?.GetRequiredService<ISettingsService>()
            ?? throw new InvalidOperationException("Test services are not initialized.");
        settingsService.Settings.GetClusterSettings(workspace).Namespaces!.Add("my-app");

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        await vm.TreeViewSelectionChangedAsync(clusterNode);

        await WaitForAsync(() =>
            FindResourceLink(clusterNode, "Pods") != null
            && FindResourceLink(clusterNode, "Deployments") != null,
            timeoutMs: 1000);
    }

    [AvaloniaFact]
    public async Task cluster_node_expands_after_successful_connect()
    {
        var runtime = new TestCluster
        {
            Connected = false,
            Status = ClusterStatus.None,
        };

        var workspace = CreateWorkspace(runtime);
        await workspace.EnsureWorkspaceStateInitializedAsync();
        Dispatcher.UIThread.RunJobs();

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        clusterNode.IsExpanded.ShouldBeFalse();

        await vm.TreeViewSelectionChangedAsync(clusterNode);

        await WaitForAsync(() => clusterNode.Cluster.Connected && clusterNode.IsExpanded);
    }

    [AvaloniaFact]
    public async Task selecting_already_connected_cluster_initializes_navigation_on_demand()
    {
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
        };

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
        var runtime = new TestCluster
        {
            Connected = false,
            Status = ClusterStatus.None,
        };

        var permissionRefreshRelease = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var workspace = CreateWorkspace(runtime);
        workspace.AddResourceConfigForTest(new SlowPermissionResourceConfig(typeof(TestPermissionResourceGamma), "Gamma Permission Resource", permissionRefreshRelease.Task));

        runtime.ConnectBehavior = () =>
        {
            runtime.Status = ClusterStatus.Connected;
            runtime.Connected = true;
            return Task.CompletedTask;
        };

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);

        await vm.TreeViewSelectionChangedAsync(clusterNode);

        await Task.Delay(100);
        Dispatcher.UIThread.RunJobs();

        FindResourceLink(clusterNode, "Gamma Permission Resource").ShouldBeNull();

        permissionRefreshRelease.TrySetResult(null);

        await WaitForAsync(() => FindResourceLink(clusterNode, "Gamma Permission Resource") != null);
    }

    [AvaloniaFact]
    public async Task connect_path_publishes_ready_resources_without_waiting_for_unrelated_slow_permission_refresh()
    {
        var runtime = new TestCluster
        {
            Connected = false,
            Status = ClusterStatus.None,
        };

        var slowPermissionRelease = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var workspace = CreateWorkspace(runtime);
        workspace.AddResourceConfigForTest(new DeferredPermissionResourceConfig(typeof(TestPermissionResourceAlpha), "Alpha Permission Resource"));
        workspace.AddResourceConfigForTest(new SlowPermissionResourceConfig(typeof(TestPermissionResourceGamma), "Gamma Permission Resource", slowPermissionRelease.Task));

        runtime.ConnectBehavior = () =>
        {
            runtime.Status = ClusterStatus.Connected;
            runtime.Connected = true;
            return Task.CompletedTask;
        };

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
    public async Task selecting_cluster_node_does_not_block_on_slow_synchronous_connect_startup()
    {
        var runtime = new TestCluster
        {
            Connected = false,
            Status = ClusterStatus.None,
        };

        var releaseConnect = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

        runtime.ConnectBehavior = async () =>
        {
            Thread.Sleep(300);
            runtime.Status = ClusterStatus.Connecting;
            await releaseConnect.Task;
            runtime.Connected = true;
            runtime.Status = ClusterStatus.Connected;
        };

        var workspace = CreateWorkspace(runtime);
        await workspace.EnsureWorkspaceStateInitializedAsync();
        Dispatcher.UIThread.RunJobs();

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);

        var stopwatch = Stopwatch.StartNew();
        await vm.TreeViewSelectionChangedAsync(clusterNode);
        stopwatch.Stop();

        stopwatch.Elapsed.ShouldBeLessThan(TimeSpan.FromMilliseconds(150));
        clusterNode.IsExpanded.ShouldBeFalse();

        releaseConnect.TrySetResult(null);

        await WaitForAsync(() => clusterNode.IsExpanded);
        clusterNode.Cluster.Connected.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task navigation_items_rebuild_after_resource_config_batch_publish()
    {
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
        };

        var workspace = CreateWorkspace(runtime);
        await workspace.EnsureWorkspaceStateInitializedAsync();
        Dispatcher.UIThread.RunJobs();

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var originalRoot = clusterNode.NavigationItems.Single(x => x.Name == "Custom Resource Definitions");
        var subtreeChanges = 0;
        originalRoot.NavigationItems.CollectionChanged += (_, _) => subtreeChanges++;

        workspace.AddResourceConfigForTest(new FakeCustomResourceConfig(typeof(TestCustomResourceAlpha), "Alpha Resources"));
        await Task.Delay(250);
        Dispatcher.UIThread.RunJobs();

        var updatedRoot = clusterNode.NavigationItems.Single(x => x.Name == "Custom Resource Definitions");
        ReferenceEquals(originalRoot, updatedRoot).ShouldBeTrue();
        subtreeChanges.ShouldBeGreaterThan(0);
    }

    [AvaloniaFact]
    public async Task resource_navigation_items_appear_incrementally_as_permissions_complete()
    {
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
        };

        var workspace = CreateWorkspace(runtime);
        await workspace.EnsureWorkspaceStateInitializedAsync();
        Dispatcher.UIThread.RunJobs();

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var alphaConfig = new FakeResourceConfig(typeof(TestPermissionResourceAlpha), "Alpha Permission Resource");
        var betaConfig = new FakeResourceConfig(typeof(TestPermissionResourceBeta), "Beta Permission Resource");

        workspace.AddResourceConfigForTest(alphaConfig);

        var betaConfigAdded = Task.Run(async () =>
        {
            await Task.Delay(50);
            workspace.AddResourceConfigForTest(betaConfig);
        });

        var alphaLink = await WaitForValueAsync(
            () => FindResourceLink(clusterNode, alphaConfig.Name),
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
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
        };

        var workspace = CreateWorkspace(runtime);
        await workspace.EnsureWorkspaceStateInitializedAsync();
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
    public async Task port_forwarders_is_under_network_category_not_top_level()
    {
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
        };
        var workspace = CreateWorkspace(runtime);
        await workspace.EnsureWorkspaceStateInitializedAsync();
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
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
            CanCreatePodPortForward = false,
        };

        var workspace = CreateWorkspace(runtime);
        await workspace.EnsureWorkspaceStateInitializedAsync();
        Dispatcher.UIThread.RunJobs();

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var networkCategory = clusterNode.NavigationItems.Single(x => x.Name == "Network");

        var portForwardersInNetwork = networkCategory.NavigationItems
            .OfType<NavigationLink>()
            .SingleOrDefault(x => x.ViewModelKey == NavigationTargets.PortForwarders);

        portForwardersInNetwork.ShouldBeNull();
    }

    [AvaloniaFact]
    public async Task initial_navigation_build_does_not_check_port_forward_until_pod_permissions_are_loaded()
    {
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
            ThrowOnMissingPortForwardReview = true,
        };

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
        runtime.PortForwardPermissionChecks.ShouldBe(0);
    }

    [AvaloniaFact]
    public async Task resource_navigation_updates_incrementally_and_port_forward_waits_for_pod_permissions()
    {
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
            DefaultPermissionAllowed = false,
            ThrowOnMissingPortForwardReview = true,
        };

        var workspace = CreateWorkspace(runtime);
        await workspace.EnsureWorkspaceStateInitializedAsync();
        Dispatcher.UIThread.RunJobs();

        var podRelease = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var fastRefreshCompleted = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

        workspace.AddResourceConfigForTest(new BlockingPodPermissionResourceConfig(runtime, podRelease.Task));
        workspace.AddResourceConfigForTest(new ImmediatePermissionResourceConfig(typeof(TestPermissionResourceAlpha), "Alpha Permission Resource", fastRefreshCompleted));

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        runtime.ResetPortForwardPermissionChecks();

        await runtime.AddOrUpdateResource(new V1Namespace
        {
            Metadata = new V1ObjectMeta { Name = "my-app" }
        });

        await Task.Delay(100);
        Dispatcher.UIThread.RunJobs();

        await WaitForAsync(() => fastRefreshCompleted.Task.IsCompleted);
        FindResourceLink(clusterNode, "Alpha Permission Resource").ShouldNotBeNull();
        FindNavigationLink(clusterNode.NavigationItems, NavigationTargets.PortForwarders).ShouldBeNull();
        runtime.PortForwardPermissionChecks.ShouldBe(0);

        runtime.ThrowOnMissingPortForwardReview = false;
        podRelease.TrySetResult(null);

        await WaitForAsync(() => FindNavigationLink(clusterNode.NavigationItems, NavigationTargets.PortForwarders) != null);
    }

    [AvaloniaFact]
    public async Task connect_preloads_pod_default_and_custom_permissions()
    {
        var runtime = new TestCluster
        {
            Connected = false,
            Status = ClusterStatus.None,
            ListNamespaces = false,
        };

        runtime.SetPermission<V1Namespace>(Verb.List, true);
        runtime.SetPermission<V1Namespace>(Verb.Watch, true);
        runtime.SetPermission<V1Pod>(Verb.List, true, "my-app");
        runtime.SetPermission<V1Pod>(Verb.Watch, true, "my-app");
        runtime.SetPermission<V1Pod>(Verb.List, false);
        runtime.SetPermission<V1Pod>(Verb.Watch, false);
        runtime.SetPermission<V1Pod>(Verb.Get, false);
        runtime.SetPermission<V1Pod>(Verb.Delete, false);
        runtime.SetPermission<V1Pod>(Verb.Patch, false);
        runtime.SetPermission<V1Pod>(Verb.Update, false);
        runtime.SetPermission<V1Pod>(Verb.Create, false, subresource: "exec");
        runtime.SetPermission<V1Pod>(Verb.Get, false, subresource: "log");
        runtime.SetPermission<V1Pod>(Verb.Create, false, subresource: "portforward");
        runtime.SetPermission<V1Pod>(Verb.Get, false, "my-app", "log");
        runtime.SetPermission<V1Pod>(Verb.Create, false, "my-app", "exec");
        runtime.SetPermission<V1Pod>(Verb.Create, true, "my-app", "portforward");

        await runtime.AddOrUpdateResource(new V1Namespace
        {
            Metadata = new V1ObjectMeta { Name = "my-app" }
        });

        var workspace = CreateWorkspace(runtime);

        await workspace.Connect();
        Dispatcher.UIThread.RunJobs();

        runtime.CanI<V1Pod>(Verb.Create, "my-app", "portforward").ShouldBeTrue();
        runtime.CanI<V1Pod>(Verb.Create, subresource: "portforward").ShouldBeFalse();
        runtime.CanI<V1Pod>(Verb.Get, "my-app", "log").ShouldBeFalse();
        runtime.CanI<V1Pod>(Verb.Create, "my-app", "exec").ShouldBeFalse();
    }

    [AvaloniaFact]
    public async Task custom_resource_definitions_link_is_sorted_to_bottom()
    {
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
        };
        var workspace = CreateWorkspace(runtime);
        await workspace.EnsureWorkspaceStateInitializedAsync();
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
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
        };
        var workspace = CreateWorkspace(runtime);
        await workspace.EnsureWorkspaceStateInitializedAsync();
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
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
        };
        var workspace = CreateWorkspace(runtime);
        await workspace.EnsureWorkspaceStateInitializedAsync();
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
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
        };
        var workspace = CreateWorkspace(runtime);
        await workspace.EnsureWorkspaceStateInitializedAsync();
        Dispatcher.UIThread.RunJobs();
        workspace.AddResourceConfigForTest(new FakeCustomResourceConfig(typeof(TestCustomResourceAlpha), "Alpha Resources"));

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var crdRoot = clusterNode.NavigationItems.Single(x => x.Name == "Custom Resource Definitions");
        crdRoot.IsExpanded = true;

        workspace.AddResourceConfigForTest(new FakeCustomResourceConfig(typeof(TestCustomResourceBeta), "Beta Resources"));
        await Task.Delay(250);
        Dispatcher.UIThread.RunJobs();

        var rebuiltRoot = clusterNode.NavigationItems.Single(x => x.Name == "Custom Resource Definitions");
        rebuiltRoot.IsExpanded.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task resource_navigation_links_keep_counts_after_rebuild()
    {
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
        };

        var workspace = CreateWorkspace(runtime);
        await workspace.SeedResource<V1Namespace>();
        await workspace.EnsureWorkspaceStateInitializedAsync();
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
        await Task.Delay(250);
        Dispatcher.UIThread.RunJobs();

        var rebuiltResourceLink = clusterNode.NavigationItems
            .OfType<ResourceNavigationLink>()
            .FirstOrDefault(x => x.ControlType == typeof(V1Namespace));

        rebuiltResourceLink.ShouldNotBeNull();
        (rebuiltResourceLink.Count is not null).ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task updated_crd_reloads_open_resource_list_document_to_the_new_generated_type()
    {
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
        };

        var workspace = CreateWorkspace(runtime);
        await workspace.EnsureWorkspaceStateInitializedAsync();
        Dispatcher.UIThread.RunJobs();

        var originalCrd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");
        await runtime.AddOrUpdateResource(originalCrd);

        var originalType = await WaitForValueAsync(() => GetCustomResourceType(runtime, originalCrd));
        originalType.ShouldNotBeNull();
        await AddGeneratedCustomResourceAsync(workspace, originalType, originalCrd, "default", "old-item");

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var originalLink = await WaitForValueAsync(() => FindResourceLink(clusterNode, "Tests"));
        originalLink.ShouldNotBeNull();

        await vm.OpenResourceNavigationCommand.ExecuteAsync(originalLink);
        Dispatcher.UIThread.RunJobs();

        var documents = vm.Factory.GetDockable<IDocumentDock>("Documents");
        documents.ShouldNotBeNull();

        await WaitForAsync(() =>
            documents.VisibleDockables?.OfType<IResourceListViewModel>().Any(x => ReferenceEquals(x.Cluster, workspace) && x.Kind == GroupApiVersionKind.From(originalType)) == true);

        var originalDocument = documents.VisibleDockables!
            .OfType<IResourceListViewModel>()
            .Single(x => ReferenceEquals(x.Cluster, workspace) && x.Kind == GroupApiVersionKind.From(originalType));

        var updatedCrd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "otherString");
        await runtime.AddOrUpdateResource(updatedCrd);

        var updatedType = await WaitForValueAsync(() => GetCustomResourceType(runtime, updatedCrd));
        updatedType.ShouldNotBeNull();
        updatedType.ShouldNotBe(originalType);
        await AddGeneratedCustomResourceAsync(workspace, updatedType, updatedCrd, "default", "new-item");

        await WaitForAsync(() =>
            documents.VisibleDockables?.OfType<IResourceListViewModel>().Any(x =>
                ReferenceEquals(x.Cluster, workspace)
                && x.Kind == GroupApiVersionKind.From(updatedType)
                && x.ResourceConfig.Type == updatedType
                && x.ItemCount == 1) == true);

        documents.VisibleDockables!
            .OfType<IResourceListViewModel>()
            .Any(x => ReferenceEquals(x, originalDocument))
            .ShouldBeFalse();

        var reloadedDocument = documents.VisibleDockables!
            .OfType<IResourceListViewModel>()
            .Single(x => ReferenceEquals(x.Cluster, workspace) && x.Kind == GroupApiVersionKind.From(updatedType));

        reloadedDocument.ResourceConfig.Type.ShouldBe(updatedType);
        reloadedDocument.ItemCount.ShouldBe(1);
    }

    [AvaloniaFact]
    public async Task stale_crd_navigation_link_opens_the_current_generated_resource_type()
    {
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
        };

        var workspace = CreateWorkspace(runtime);
        await workspace.EnsureWorkspaceStateInitializedAsync();
        Dispatcher.UIThread.RunJobs();

        var originalCrd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "someString");
        await runtime.AddOrUpdateResource(originalCrd);

        var originalType = await WaitForValueAsync(() => GetCustomResourceType(runtime, originalCrd));
        originalType.ShouldNotBeNull();

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var staleLink = await WaitForValueAsync(() => FindResourceLink(clusterNode, "Tests"));
        staleLink.ShouldNotBeNull();
        staleLink.ControlType.ShouldBe(originalType);

        var updatedCrd = ClusterWorkspaceTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "tests", "otherString");
        await runtime.AddOrUpdateResource(updatedCrd);

        var updatedType = await WaitForValueAsync(() => GetCustomResourceType(runtime, updatedCrd));
        updatedType.ShouldNotBeNull();
        updatedType.ShouldNotBe(originalType);
        await AddGeneratedCustomResourceAsync(workspace, updatedType, updatedCrd, "default", "new-item");

        await vm.OpenResourceNavigationCommand.ExecuteAsync(staleLink);
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
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
        };

        var workspace = CreateWorkspace(runtime);
        await workspace.EnsureWorkspaceStateInitializedAsync();
        Dispatcher.UIThread.RunJobs();

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var podsLink = FindResourceLink(clusterNode, "Pods");

        podsLink.ShouldNotBeNull();
        (podsLink.Count is not null).ShouldBeTrue();
        var countTask = WaitForCountAsync(podsLink.Count, timeoutMs: 10000);
        await vm.TreeViewSelectionChangedAsync(podsLink);
        var count = await countTask;
        count.ShouldBe(0);
    }

    [AvaloniaFact]
    public async Task first_click_on_resource_navigation_link_shows_count()
    {
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
        };

        await runtime.AddOrUpdateResource(new V1Namespace
        {
            Metadata = new() { Name = "default" }
        });

        var workspace = CreateWorkspace(runtime);
        await workspace.EnsureWorkspaceStateInitializedAsync();
        Dispatcher.UIThread.RunJobs();

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var podsLink = await WaitForValueAsync(() => FindResourceLink(clusterNode, "Pods"));

        podsLink.ShouldNotBeNull();
        (podsLink.Count is not null).ShouldBeTrue();
        var countTask = WaitForCountAsync(podsLink.Count, timeoutMs: 10000);
        await vm.TreeViewSelectionChangedAsync(podsLink);
        var count = await countTask;
        count.ShouldBe(0);
    }

    [AvaloniaFact]
    public async Task selecting_seeded_resource_navigation_link_shows_source_cache_count()
    {
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
        };

        await runtime.AddOrUpdateResource(new V1Namespace
        {
            Metadata = new() { Name = "default" }
        });

        var workspace = CreateWorkspace(runtime);
        await workspace.EnsureWorkspaceStateInitializedAsync();
        Dispatcher.UIThread.RunJobs();

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var podsLink = FindResourceLink(clusterNode, "Pods");

        podsLink.ShouldNotBeNull();
        (podsLink.Count is not null).ShouldBeTrue();
        var countTask = WaitForCountAsync(podsLink.Count, timeoutMs: 10000);
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
        count.ShouldBe(0);
    }

    [AvaloniaFact]
    public async Task crd_delta_does_not_rebuild_unrelated_resource_nodes()
    {
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
        };

        await runtime.AddOrUpdateResource(new V1Namespace
        {
            Metadata = new() { Name = "default" }
        });

        var workspace = CreateWorkspace(runtime);
        await workspace.EnsureWorkspaceStateInitializedAsync();
        Dispatcher.UIThread.RunJobs();

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var namespaceLink = clusterNode.NavigationItems
            .OfType<ResourceNavigationLink>()
            .Single(x => x.ControlType == typeof(V1Namespace));

        workspace.AddResourceConfigForTest(new FakeCustomResourceConfig(typeof(TestCustomResourceAlpha), "Alpha Resources"));
        await Task.Delay(250);
        Dispatcher.UIThread.RunJobs();

        var rebuiltNamespaceLink = clusterNode.NavigationItems
            .OfType<ResourceNavigationLink>()
            .Single(x => x.ControlType == typeof(V1Namespace));

        ReferenceEquals(namespaceLink, rebuiltNamespaceLink).ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task namespace_addition_does_not_replace_namespace_navigation_link()
    {
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
        };

        await runtime.AddOrUpdateResource(new V1Namespace
        {
            Metadata = new() { Name = "default" }
        });

        var workspace = CreateWorkspace(runtime);
        await workspace.EnsureWorkspaceStateInitializedAsync();
        Dispatcher.UIThread.RunJobs();

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var namespaceLink = clusterNode.NavigationItems
            .OfType<ResourceNavigationLink>()
            .Single(x => x.ControlType == typeof(V1Namespace));
        var initialNamespaceCount = workspace.Namespaces.Count;

        await runtime.AddOrUpdateResource(new V1Namespace
        {
            Metadata = new V1ObjectMeta
            {
                Name = "team-b"
            }
        });

        await WaitForAsync(() => workspace.Namespaces.Count == initialNamespaceCount + 1);

        var updatedNamespaceLink = clusterNode.NavigationItems
            .OfType<ResourceNavigationLink>()
            .Single(x => x.ControlType == typeof(V1Namespace));

        ReferenceEquals(namespaceLink, updatedNamespaceLink).ShouldBeTrue();
        (updatedNamespaceLink.Count is not null).ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task custom_resource_definition_added_after_navigation_build_adds_custom_resource_entry()
    {
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
        };

        var workspace = CreateWorkspace(runtime);
        await workspace.EnsureWorkspaceStateInitializedAsync();
        Dispatcher.UIThread.RunJobs();
        workspace.AddResourceConfigForTest(new FakeResourceConfig(typeof(V1CustomResourceDefinition), "Definitions"));

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        workspace.AddResourceConfigForTest(new FakeCustomResourceConfig(typeof(TestCustomResourceKubeUi), "Tests"));
        await Task.Delay(250);
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
    public async Task custom_resource_definition_update_updates_existing_navigation_entry_without_replacing_group()
    {
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
        };

        var workspace = CreateWorkspace(runtime);
        await workspace.EnsureWorkspaceStateInitializedAsync();
        Dispatcher.UIThread.RunJobs();
        workspace.AddResourceConfigForTest(new FakeResourceConfig(typeof(V1CustomResourceDefinition), "Definitions"));
        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        workspace.AddResourceConfigForTest(new FakeCustomResourceConfig(typeof(TestCustomResourceKubeUi), "Tests"));
        await Task.Delay(250);
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

        workspace.AddResourceConfigForTest(new FakeCustomResourceConfig(typeof(TestCustomResourceKubeUi), "Renamed Tests"));
        await Task.Delay(250);
        Dispatcher.UIThread.RunJobs();

        var updatedRootGroup = await WaitForValueAsync(
            () => crdRoot.NavigationItems.OfType<NavigationItem>().SingleOrDefault(x => x.Name == "kubeui.com"),
            timeoutMs: 10000);
        updatedRootGroup.ShouldNotBeNull();
        ReferenceEquals(originalRootGroup, updatedRootGroup).ShouldBeTrue();

        var updatedLink = await WaitForValueAsync(
            () => updatedRootGroup.NavigationItems.OfType<ResourceNavigationLink>().SingleOrDefault(x => x.Name == "Renamed Tests"),
            timeoutMs: 10000);
        updatedLink.ShouldNotBeNull();
        ReferenceEquals(originalLink, updatedLink).ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task custom_resource_definition_delete_removes_navigation_entry_without_rebuilding_remaining_groups()
    {
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
        };

        var workspace = CreateWorkspace(runtime);
        await workspace.EnsureWorkspaceStateInitializedAsync();
        Dispatcher.UIThread.RunJobs();
        workspace.AddResourceConfigForTest(new FakeResourceConfig(typeof(V1CustomResourceDefinition), "Definitions"));
        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var crdA = NavigationTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "Tests", "someString");
        var crdB = NavigationTestCustomResourceDefinitionFactory.Create("others.kubeui.com", "Others", "otherString");

        await runtime.AddOrUpdateResource(crdA);
        await runtime.AddOrUpdateResource(crdB);

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var crdRoot = await WaitForValueAsync(
            () => clusterNode.NavigationItems.SingleOrDefault(x => x.Name == "Custom Resource Definitions"),
            timeoutMs: 10000);
        crdRoot.ShouldNotBeNull();

        var survivingGroup = await WaitForValueAsync(
            () => crdRoot.NavigationItems.OfType<NavigationItem>().SingleOrDefault(x => x.Name == "kubeui.com"),
            timeoutMs: 10000);
        survivingGroup.ShouldNotBeNull();
        var originalSurvivingGroup = survivingGroup;

        await runtime.DeleteResource(crdA);
        await WaitForValueAsync(
            () => survivingGroup.NavigationItems.OfType<ResourceNavigationLink>().SingleOrDefault(x => x.Name == "Others"),
            timeoutMs: 10000);

        crdRoot.NavigationItems
            .OfType<NavigationItem>()
            .SelectMany(x => x.NavigationItems.OfType<ResourceNavigationLink>())
            .Any(x => x.Name == "Tests")
            .ShouldBeFalse();
    }

    [AvaloniaFact]
    public async Task custom_resource_definition_delete_prunes_empty_group_branch_without_replacing_root()
    {
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
        };

        var workspace = CreateWorkspace(runtime);
        await workspace.EnsureWorkspaceStateInitializedAsync();
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
    public IStyle ListStyle() => new global::Avalonia.Styling.Style();
    public IEnumerable<(Verb verb, string? subresource)> Permissions() => [];
    public Task UpdatePermissions() => Task.CompletedTask;
    public Type Type { get; }

    public IRelayCommand NewResourceCommand => throw new NotImplementedException();

    public IRelayCommand<IList> ViewCommand => throw new NotImplementedException();

    public void Initialize(ClusterWorkspaceViewModel cluster) { }
}

internal class FakeResourceConfig : IResourceConfig
{
    public FakeResourceConfig(Type resourceType, string name, bool canListAndWatch = true)
    {
        Type = resourceType;
        Name = name;
        CanListAndWatch = canListAndWatch;
    }

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
    public IStyle ListStyle() => new global::Avalonia.Styling.Style();
    public IEnumerable<(Verb verb, string? subresource)> Permissions() => [];
    public Task UpdatePermissions() => Task.CompletedTask;
    public Type Type { get; }

    public IRelayCommand NewResourceCommand => throw new NotImplementedException();

    public IRelayCommand<IList> ViewCommand => throw new NotImplementedException();

    public void Initialize(ClusterWorkspaceViewModel cluster) { }
}

internal sealed class DeferredPermissionResourceConfig : IResourceConfig
{
    public DeferredPermissionResourceConfig(Type resourceType, string name)
    {
        Type = resourceType;
        Name = name;
    }

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
    public IStyle ListStyle() => new global::Avalonia.Styling.Style();
    public IEnumerable<(Verb verb, string? subresource)> Permissions() => [];
    public Type Type { get; }

    public Task UpdatePermissions()
    {
        CanListAndWatch = true;
        PermissionsLoaded = true;
        return Task.CompletedTask;
    }

    public IRelayCommand NewResourceCommand => throw new NotImplementedException();

    public IRelayCommand<IList> ViewCommand => throw new NotImplementedException();

    public void Initialize(ClusterWorkspaceViewModel cluster) { }
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
    public IStyle ListStyle() => new global::Avalonia.Styling.Style();
    public IEnumerable<(Verb verb, string? subresource)> Permissions() => [];
    public Type Type { get; }

    public async Task UpdatePermissions()
    {
        await _permissionRefreshTask;
        CanListAndWatch = true;
        PermissionsLoaded = true;
    }

    public IRelayCommand NewResourceCommand => throw new NotImplementedException();

    public IRelayCommand<IList> ViewCommand => throw new NotImplementedException();

    public void Initialize(ClusterWorkspaceViewModel cluster) { }
}
