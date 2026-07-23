using System.Collections.Specialized;
using System.Reactive.Linq;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using Dock.Model.Controls;
using Dock.Model.Core;
using FluentAvalonia.UI.Controls;
using FluentIcons.Common;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Clusters.Error;
using KubeUI.Avalonia.Features.Clusters.Overview;
using KubeUI.Avalonia.Features.Clusters.Settings;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Features.Resources.Visualization;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.Docking;
using KubeUI.Avalonia.Infrastructure.Platform;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Infrastructure.Threading;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Shell.Navigation;

public sealed partial class NavigationViewModel : ViewModelBase, IDisposable
{
    private readonly ILogger<NavigationViewModel> _logger;
    private readonly INotificationManager _notificationManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly IDialogService _dialogService;
    public new IFactory Factory => _serviceProvider.GetRequiredService<IFactory>();
    private readonly Dictionary<ClusterWorkspace, ClusterNavigationNode> _clusterNodes = [];
    private readonly Dictionary<IClusterRuntime, ClusterWorkspace> _workspacesByRuntime = [];
    private readonly object _pendingResourceNavigationUpdatesLock = new();
    private readonly Dictionary<ClusterWorkspace, HashSet<IResourceConfig>> _pendingResourceNavigationUpdates = [];
    private readonly HashSet<ClusterWorkspace> _scheduledResourceNavigationUpdates = [];
    private readonly NavigationDocumentService _documentService;

    private sealed record ResourceNavigationUpdateBatch(
        IResourceConfig[] ProcessedResourceConfigs,
        IResourceConfig[] ResourceConfigSnapshot);

    [ObservableProperty]
    public partial ClusterWorkspaceCatalog ClusterCatalog { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<ClusterNavigationNode> Clusters { get; set; } = [];

    public NavigationViewModel(
        ILogger<NavigationViewModel> logger,
        INotificationManager notificationManager,
        IDialogService dialogService,
        ClusterWorkspaceCatalog clusterCatalog,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _notificationManager = notificationManager;
        _serviceProvider = serviceProvider;
        _dialogService = dialogService;
        ClusterCatalog = clusterCatalog;
        _documentService = new NavigationDocumentService(
            serviceProvider,
            serviceProvider.GetRequiredService<ILogger<NavigationDocumentService>>(),
            () => Factory);
        Title = Assets.Resources.NavigationView_Title!;
        Id = nameof(NavigationViewModel);

        if (ClusterCatalog.Clusters is INotifyCollectionChanged collection)
        {
            collection.CollectionChanged += OnClusterCatalogCollectionChanged;
        }

        ReloadClusters();
    }

    public void Dispose()
    {
        if (ClusterCatalog.Clusters is INotifyCollectionChanged collection)
        {
            collection.CollectionChanged -= OnClusterCatalogCollectionChanged;
        }

        foreach (var cluster in _clusterNodes.Keys.ToList())
        {
            UnsubscribeCluster(cluster);
        }

        _clusterNodes.Clear();
        _workspacesByRuntime.Clear();
        lock (_pendingResourceNavigationUpdatesLock)
        {
            _pendingResourceNavigationUpdates.Clear();
            _scheduledResourceNavigationUpdates.Clear();
        }
        Clusters.Clear();
    }

    [RelayCommand]
    private Task HandleSelectionChangedAsync(SelectionChangedEventArgs? e)
    {
        var selectedItem = e?.AddedItems.Count > 0 ? e.AddedItems[0] : null;
        return TreeViewSelectionChangedAsync(selectedItem);
    }

    internal async Task TreeViewSelectionChangedAsync(object? item)
    {
        if (item is ClusterNavigationNode clusterNode)
        {
            HandleClusterSelection(clusterNode);
        }
        else if (item is ResourceNavigationLink resourceNavLink)
        {
            _documentService.Open(resourceNavLink);
        }
        else if (item is NavigationLink navLink)
        {
            await SelectNavigationLink(navLink).ConfigureAwait(true);
        }

        if (item is NavigationItem nav && nav.NavigationItems.Count > 0 && item is not ClusterNavigationNode)
        {
            nav.IsExpanded = !nav.IsExpanded;
        }
    }

    private void HandleClusterSelection(ClusterNavigationNode clusterNode)
    {
        var cluster = clusterNode.Cluster;

        if (cluster.Runtime.Connected)
        {
            clusterNode.IsExpanded = !clusterNode.IsExpanded;
            return;
        }

        Task.Run(() => ConnectIfIdleAsync(clusterNode));
    }

    [RelayCommand]
    private async Task ToggleClusterConnectionAsync(ClusterNavigationNode? clusterNode)
    {
        if (clusterNode == null)
        {
            return;
        }

        var cluster = clusterNode.Cluster;
        if (cluster.Runtime.Connected)
        {
            await cluster.Disconnect().ConfigureAwait(false);
            return;
        }

        await ConnectIfIdleAsync(clusterNode).ConfigureAwait(false);
    }

    private async Task ConnectIfIdleAsync(ClusterNavigationNode clusterNode)
    {
        if (clusterNode.Cluster.Runtime.Status == ClusterStatus.Connecting)
        {
            return;
        }

        await clusterNode.Cluster.Connect().ConfigureAwait(false);

        if (clusterNode.Cluster.Runtime.Connected)
        {
            Dispatcher.UIThread.Post(() => clusterNode.IsExpanded = true);
        }
    }

    [RelayCommand]
    private Task OpenClusterSettingsAsync(ClusterNavigationNode? clusterNode)
    {
        if (clusterNode == null)
        {
            return Task.CompletedTask;
        }

        return SelectNavigationLink(CreateNavigationLink(
            clusterNode.Cluster,
            NavigationTargets.ClusterSettings,
            Assets.Resources.ClusterSettingsView_Title!));
    }

    [RelayCommand]
    private Task OpenResourceNavigationAsync(ResourceNavigationLink? resourceNavLink)
    {
        if (resourceNavLink == null)
        {
            return Task.CompletedTask;
        }

        _documentService.Open(resourceNavLink);
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task OpenResourceNavigationInNewTabAsync(ResourceNavigationLink? resourceNavLink)
    {
        if (resourceNavLink == null)
        {
            return Task.CompletedTask;
        }

        _documentService.Open(resourceNavLink, forceNewTab: true);
        return Task.CompletedTask;
    }

    private async Task ShowMissingNamespacePermissionPromptAsync(ClusterWorkspace cluster)
    {
        var settingsVm = _serviceProvider.GetRequiredService<ClusterSettingsViewModel>();
        settingsVm.Initialize(cluster);
        Factory.AddToDocuments(settingsVm);

        var settings = new ContentDialogSettings
        {
            Title = Assets.Resources.Cluster_Missing_Namespace_Permission_Title,
            Content = Assets.Resources.Cluster_Missing_Namespace_Permission_Content,
            PrimaryButtonText = Assets.Resources.Cluster_Missing_Namespace_Permission_Primary,
            DefaultButton = FAContentDialogButton.Primary
        };

        await _dialogService.ShowContentDialogAsync(this, settings);
    }

    private void ShowClusterError(string? error)
    {
        const string id = "cluster-error";

        if (Factory.FindDockableById(id) is ClusterErrorViewModel existing)
        {
            existing.Error = error;
            Factory.SetActiveDockable(existing);
            if (Factory.GetDockable<IDocumentDock>("Documents") is { } documents)
            {
                Factory.SetFocusedDockable(documents, existing);
            }

            return;
        }

        var vm = _serviceProvider.GetRequiredService<ClusterErrorViewModel>();
        vm.Id = id;
        vm.Error = error;
        Factory.AddToDocuments(vm);
    }

    private void OnClusterCatalogCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (ClusterWorkspace cluster in e.NewItems!)
                {
                    AddClusterNode(cluster);
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (ClusterWorkspace cluster in e.OldItems!)
                {
                    RemoveClusterNode(cluster);
                }
                break;
            case NotifyCollectionChangedAction.Replace:
                foreach (ClusterWorkspace cluster in e.OldItems!)
                {
                    RemoveClusterNode(cluster);
                }

                foreach (ClusterWorkspace cluster in e.NewItems!)
                {
                    AddClusterNode(cluster);
                }
                break;
            case NotifyCollectionChangedAction.Reset:
                ReloadClusters();
                break;
        }
    }

    private void ReloadClusters()
    {
        lock (_pendingResourceNavigationUpdatesLock)
        {
            _pendingResourceNavigationUpdates.Clear();
            _scheduledResourceNavigationUpdates.Clear();
        }

        foreach (var cluster in _clusterNodes.Keys.ToList())
        {
            UnsubscribeCluster(cluster);
        }

        _clusterNodes.Clear();
        Clusters.Clear();

        foreach (var cluster in ClusterCatalog.Clusters)
        {
            AddClusterNode(cluster);
        }
    }

    private void AddClusterNode(ClusterWorkspace cluster)
    {
        if (_clusterNodes.ContainsKey(cluster))
        {
            return;
        }

        SubscribeCluster(cluster);
        _workspacesByRuntime[cluster.Runtime] = cluster;

        var node = new ClusterNavigationNode(cluster)
        {
            ToggleConnectionCommand = ToggleClusterConnectionCommand,
            OpenSettingsCommand = OpenClusterSettingsCommand,
        };

        _clusterNodes.Add(cluster, node);
        Clusters.Add(node);

        foreach (var resourceConfig in cluster.GetResourceConfigs())
        {
            ApplyResourceConfigNavigation(cluster, resourceConfig);
        }
    }

    private void RemoveClusterNode(ClusterWorkspace cluster)
    {
        lock (_pendingResourceNavigationUpdatesLock)
        {
            _pendingResourceNavigationUpdates.Remove(cluster);
            _scheduledResourceNavigationUpdates.Remove(cluster);
        }

        if (!_clusterNodes.Remove(cluster, out var node))
        {
            return;
        }

        UnsubscribeCluster(cluster);
        _workspacesByRuntime.Remove(cluster.Runtime);
        node.Dispose();
        Clusters.Remove(node);
    }

    private void SubscribeCluster(ClusterWorkspace cluster)
    {
        cluster.ResourceConfigProcessed += OnClusterResourceConfigProcessed;
        cluster.Runtime.ResourceSeeded += OnClusterResourceSeeded;
        if (cluster.Runtime is INotifyPropertyChanged runtime)
        {
            runtime.PropertyChanged += OnClusterRuntimePropertyChanged;
        }
        cluster.Runtime.NamespaceSelectionRequired += OnNamespaceSelectionRequired;
        cluster.CustomResourceDefinitionRemoved += OnClusterCustomResourceDefinitionRemoved;
    }

    private void UnsubscribeCluster(ClusterWorkspace cluster)
    {
        cluster.ResourceConfigProcessed -= OnClusterResourceConfigProcessed;
        cluster.Runtime.ResourceSeeded -= OnClusterResourceSeeded;
        if (cluster.Runtime is INotifyPropertyChanged runtime)
        {
            runtime.PropertyChanged -= OnClusterRuntimePropertyChanged;
        }
        cluster.Runtime.NamespaceSelectionRequired -= OnNamespaceSelectionRequired;
        cluster.CustomResourceDefinitionRemoved -= OnClusterCustomResourceDefinitionRemoved;
    }

    private void OnClusterRuntimePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not IClusterRuntime runtime || e.PropertyName != nameof(IClusterRuntime.Status))
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (runtime.Status == ClusterStatus.Errored)
            {
                ShowClusterError(runtime.LastError);
            }
        });
    }

    private void OnNamespaceSelectionRequired(IClusterRuntime runtime)
    {
        if (_workspacesByRuntime.TryGetValue(runtime, out var cluster))
        {
            Dispatcher.UIThread.Post(() => _ = ShowMissingNamespacePermissionPromptAsync(cluster));
        }
    }

    private void OnClusterResourceConfigProcessed(ClusterWorkspace cluster, IResourceConfig resourceConfig)
    {
        var scheduleUpdate = false;
        lock (_pendingResourceNavigationUpdatesLock)
        {
            if (!_pendingResourceNavigationUpdates.TryGetValue(cluster, out var resourceConfigs))
            {
                resourceConfigs = [];
                _pendingResourceNavigationUpdates.Add(cluster, resourceConfigs);
            }

            resourceConfigs.Add(resourceConfig);
            scheduleUpdate = _scheduledResourceNavigationUpdates.Add(cluster);
        }

        if (scheduleUpdate)
        {
            _ = PrepareAndApplyQueuedResourceNavigationUpdatesAsync(cluster);
        }
    }

    private Task PrepareAndApplyQueuedResourceNavigationUpdatesAsync(ClusterWorkspace cluster)
    {
        return Task.Run(() =>
        {
            IResourceConfig[] resourceConfigs;
            lock (_pendingResourceNavigationUpdatesLock)
            {
                _scheduledResourceNavigationUpdates.Remove(cluster);
                if (!_pendingResourceNavigationUpdates.Remove(cluster, out var pendingResourceConfigs))
                {
                    return;
                }

                resourceConfigs = pendingResourceConfigs.ToArray();
            }

            var resourceConfigSnapshot = resourceConfigs.Any(static config =>
                    config.Type == typeof(V1CustomResourceDefinition) || config.IsCustomResource)
                ? cluster.GetResourceConfigs().ToArray()
                : [];
            var batch = new ResourceNavigationUpdateBatch(resourceConfigs, resourceConfigSnapshot);
            Dispatcher.UIThread.Post(
                () => ApplyResourceNavigationUpdateBatch(cluster, batch),
                DispatcherPriority.Background);
        });
    }

    private void ApplyResourceNavigationUpdateBatch(ClusterWorkspace cluster, ResourceNavigationUpdateBatch batch)
    {
        if (!_clusterNodes.ContainsKey(cluster)
            || cluster.Runtime.Status != ClusterStatus.Connected)
        {
            return;
        }

        foreach (var resourceConfig in batch.ProcessedResourceConfigs)
        {
            ApplyResourceConfigNavigation(cluster, resourceConfig, batch.ResourceConfigSnapshot);
        }
    }

    private void OnClusterResourceSeeded(IClusterRuntime runtime, GroupApiVersionKind kind)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (!_workspacesByRuntime.TryGetValue(runtime, out var cluster))
            {
                return;
            }

            AttachResourceCount(cluster, kind);
        });
    }

    private void AttachResourceCount(ClusterWorkspace cluster, GroupApiVersionKind kind)
    {
        if (!_clusterNodes.TryGetValue(cluster, out var node))
        {
            return;
        }

        var link = FindResourceNavigationLink(node.NavigationItems, kind);
        if (link != null
            && cluster.Runtime.Objects.TryGetValue(kind, out var container)
            && container is IResourceContainer { IsSeeded: true })
        {
            link.Count ??= CreateResourceCountStream(cluster, link.ControlType);
        }
    }

    private static ResourceNavigationLink? FindResourceNavigationLink(IEnumerable<NavigationItem> items, GroupApiVersionKind kind)
    {
        foreach (var item in items)
        {
            if (item is ResourceNavigationLink link
                && link.ControlType != null
                && GroupApiVersionKind.From(link.ControlType) == kind)
            {
                return link;
            }

            var nested = FindResourceNavigationLink(item.NavigationItems, kind);
            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }

    private void OnClusterCustomResourceDefinitionRemoved(ClusterWorkspace cluster, GroupApiVersionKind removedKind)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (!_clusterNodes.TryGetValue(cluster, out var node))
            {
                return;
            }

            RemoveNavigationItem(node.NavigationItems, $"{cluster.Runtime.Name}-{removedKind}");
            RemoveEmptyCategories(node.NavigationItems, cluster);
        });
    }

    private void ApplyResourceConfigNavigation(
        ClusterWorkspace cluster,
        IResourceConfig resourceConfig,
        IReadOnlyCollection<IResourceConfig>? resourceConfigs = null)
    {
        if (!_clusterNodes.TryGetValue(cluster, out var node)
            || cluster.Runtime.Status != ClusterStatus.Connected)
        {
            return;
        }

        if (resourceConfig.Type == typeof(V1CustomResourceDefinition) || resourceConfig.IsCustomResource)
        {
            UpdateCustomResourceNavigation(node, cluster, resourceConfig, resourceConfigs);
        }
        else
        {
            UpdateStandardResourceNavigation(node, cluster, resourceConfig);
        }

        if (resourceConfig.Type == typeof(V1Pod))
        {
            UpdatePortForwardersNavigation(node);
        }

        AttachResourceCount(cluster, resourceConfig.Kind);
    }

    private void UpdateStandardResourceNavigation(ClusterNavigationNode node, ClusterWorkspace cluster, IResourceConfig config)
    {
        var resourceId = $"{cluster.Runtime.Name}-{config.Kind}";
        if (!config.PermissionsLoaded || !config.CanListAndWatch)
        {
            RemoveNavigationItem(node.NavigationItems, resourceId);
            RemoveEmptyCategories(node.NavigationItems, cluster);
            return;
        }

        var target = node.NavigationItems;
        if (!string.IsNullOrWhiteSpace(config.Category))
        {
            target = EnsureNavigationCategory(node.NavigationItems, cluster, config.Category, config.Order).NavigationItems;
        }

        var existingParent = FindNavigationParentCollection(node.NavigationItems, resourceId);
        var existing = existingParent?.FirstOrDefault(item => item.Id == resourceId);
        var desired = CreateResourceNavigationLink(cluster, config);

        if (existing is null)
        {
            target.Add(desired);
        }
        else if (ReferenceEquals(existingParent, target))
        {
            UpdateNavigationItem(existing, desired);
        }
        else
        {
            existingParent!.Remove(existing);
            target.Add(existing);
            UpdateNavigationItem(existing, desired);
            RemoveEmptyCategories(node.NavigationItems, cluster);
        }
    }

    private void UpdateCustomResourceNavigation(
        ClusterNavigationNode node,
        ClusterWorkspace cluster,
        IResourceConfig changedConfig,
        IReadOnlyCollection<IResourceConfig>? resourceConfigs = null)
    {
        var configs = resourceConfigs ?? cluster.GetResourceConfigs().ToArray();
        var definitions = configs
            .FirstOrDefault(config => config.Type == typeof(V1CustomResourceDefinition));
        var rootId = $"{cluster.Runtime.Name}-custom-resource-definitions";
        var root = node.NavigationItems.FirstOrDefault(item => item.Id == rootId);

        if (definitions is not { PermissionsLoaded: true, CanListAndWatch: true })
        {
            if (root != null)
            {
                node.NavigationItems.Remove(root);
            }

            return;
        }

        if (root == null)
        {
            root = new NavigationItem
            {
                Id = rootId,
                Name = ResourceCategories.CustomResourceDefinitions,
                Order = ResourceCategories.CustomResourceDefinitionsNavigationOrder,
            };
            node.NavigationItems.Add(root);

            UpdateCustomResourceLink(root, cluster, definitions);

            foreach (var config in configs
                         .Where(config => config.IsCustomResource && config.PermissionsLoaded && config.CanListAndWatch)
                         .OrderBy(config => config.Order)
                         .ThenBy(config => config.Name, StringComparer.Ordinal))
            {
                UpdateCustomResourceLink(root, cluster, config);
            }
        }
        else if (changedConfig.Type == typeof(V1CustomResourceDefinition))
        {
            UpdateCustomResourceLink(root, cluster, definitions);
        }
        else if (changedConfig.IsCustomResource)
        {
            if (changedConfig.PermissionsLoaded && changedConfig.CanListAndWatch)
            {
                UpdateCustomResourceLink(root, cluster, changedConfig);
            }
            else
            {
                RemoveNavigationItem(root.NavigationItems, $"{cluster.Runtime.Name}-{changedConfig.Kind}");
                RemoveEmptyCategories(root.NavigationItems, cluster);
            }
        }

    }

    private void UpdatePortForwardersNavigation(ClusterNavigationNode node)
    {
        var id = $"{node.Cluster.Runtime.Name}-{NavigationTargets.PortForwarders}";
        RemoveNavigationItem(node.NavigationItems, id);

        var podConfig = node.Cluster.GetResourceConfigs().FirstOrDefault(config => config.Type == typeof(V1Pod));
        if (podConfig is not { PermissionsLoaded: true, CanListAndWatch: true }
            || !CanCreatePortForward(node.Cluster))
        {
            RemoveEmptyCategories(node.NavigationItems, node.Cluster);
            return;
        }

        EnsureNavigationCategory(node.NavigationItems, node.Cluster, ResourceCategories.Network, 10)
            .NavigationItems
            .Add(CreateNavigationLink(node.Cluster, NavigationTargets.PortForwarders, Assets.Resources.PortForwarderListView_Title, -450));
    }

    private bool CanCreatePortForward(ClusterWorkspace cluster)
    {
        try
        {
            return cluster.Runtime.Permissions.CanIAnyNamespace(typeof(V1Pod), Verb.Create, "portforward");
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Unable to evaluate port forward permissions for cluster {Cluster}", cluster.Runtime.Name);
            return false;
        }
    }

    private static bool RemoveNavigationItem(IEnumerable<NavigationItem> items, string id)
    {
        foreach (var item in items.ToList())
        {
            if (item.Id == id)
            {
                if (items is ICollection<NavigationItem> collection)
                {
                    collection.Remove(item);
                    return true;
                }
            }

            if (RemoveNavigationItem(item.NavigationItems, id))
            {
                return true;
            }
        }

        return false;
    }

    private static void RemoveEmptyCategories(ObservableCollection<NavigationItem> items, ClusterWorkspace cluster)
    {
        for (var i = items.Count - 1; i >= 0; i--)
        {
            var item = items[i];
            RemoveEmptyCategories(item.NavigationItems, cluster);
            if (item.NavigationItems.Count == 0
                && (item.Id.StartsWith($"{cluster.Runtime.Name}-category-", StringComparison.Ordinal)
                    || item.Id.StartsWith($"{cluster.Runtime.Name}-crd-group-", StringComparison.Ordinal)))
            {
                items.RemoveAt(i);
            }
        }
    }

    private static ObservableCollection<NavigationItem>? FindNavigationParentCollection(ObservableCollection<NavigationItem> items, string id)
    {
        if (items.Any(item => item.Id == id))
        {
            return items;
        }

        foreach (var item in items)
        {
            var parent = FindNavigationParentCollection(item.NavigationItems, id);
            if (parent != null)
            {
                return parent;
            }
        }

        return null;
    }

    private static void UpdateNavigationItem(NavigationItem current, NavigationItem desired)
    {
        current.Name = desired.Name;
        current.Order = desired.Order;
        current.SvgIcon = desired.SvgIcon;
        current.StyleIcon = desired.StyleIcon;
        current.FluentIcon = desired.FluentIcon;

        if (current is ResourceNavigationLink currentResource
            && desired is ResourceNavigationLink desiredResource)
        {
            currentResource.Cluster = desiredResource.Cluster;
            currentResource.ControlType = desiredResource.ControlType;
            currentResource.OpenCommand = desiredResource.OpenCommand;
            currentResource.OpenInNewTabCommand = desiredResource.OpenInNewTabCommand;
        }
    }

    private NavigationItem EnsureNavigationCategory(ObservableCollection<NavigationItem> items, ClusterWorkspace cluster, string name, int order)
    {
        var id = $"{cluster.Runtime.Name}-category-{name}";
        var existing = items.FirstOrDefault(item => item.Id == id);
        if (existing != null)
        {
            return existing;
        }

        var category = new NavigationItem
        {
            Id = id,
            Name = name,
            Order = ResourceCategories.GetOrder(name, order),
        };
        items.Add(category);
        return category;
    }

    private void UpdateCustomResourceLink(NavigationItem root, ClusterWorkspace cluster, IResourceConfig config)
    {
        var resourceId = $"{cluster.Runtime.Name}-{config.Kind}";
        var target = root.NavigationItems;
        if (config.Type != typeof(V1CustomResourceDefinition))
        {
            var path = ConstructCustomResourceGroupPath(config.Kind.Group);
            var parts = new List<string>(path.Count);

            foreach (var part in path)
            {
                parts.Add(part);
                var id = $"{cluster.Runtime.Name}-crd-group-{string.Join("/", parts)}";
                var group = target.FirstOrDefault(item => item.Id == id);
                if (group == null)
                {
                    group = new NavigationItem { Id = id, Name = part, Order = 0 };
                    target.Add(group);
                }

                target = group.NavigationItems;
            }
        }

        var existingParent = FindNavigationParentCollection(root.NavigationItems, resourceId);
        var existing = existingParent?.FirstOrDefault(item => item.Id == resourceId);
        var desired = CreateResourceNavigationLink(cluster, config);
        if (config.Type == typeof(V1CustomResourceDefinition))
        {
            desired.Name = "Definitions";
            desired.Order = -1;
        }

        if (existing == null)
        {
            target.Add(desired);
        }
        else if (!ReferenceEquals(existingParent, target))
        {
            existingParent!.Remove(existing);
            target.Add(existing);
            UpdateNavigationItem(existing, desired);
            RemoveEmptyCategories(root.NavigationItems, cluster);
        }
        else
        {
            UpdateNavigationItem(existing, desired);
        }
    }

    private ResourceNavigationLink CreateResourceNavigationLink(ClusterWorkspace cluster, IResourceConfig config)
    {
        return new ResourceNavigationLink
        {
            Cluster = cluster,
            Id = $"{cluster.Runtime.Name}-{config.Kind}",
            Name = config.Name,
            ControlType = config.Type,
            Order = config.Order,
            OpenCommand = OpenResourceNavigationCommand,
            OpenInNewTabCommand = OpenResourceNavigationInNewTabCommand,
        };
    }

    private static IReadOnlyList<string> ConstructCustomResourceGroupPath(string? group)
    {
        if (string.IsNullOrWhiteSpace(group))
        {
            return ["core"];
        }

        var levels = group.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (levels.Length <= 2)
        {
            return [group];
        }

        var path = new List<string>(levels.Length - 1) { $"{levels[^2]}.{levels[^1]}" };
        for (var i = levels.Length - 3; i >= 0; i--)
        {
            path.Add(string.Join('.', levels, i, levels.Length - i));
        }

        return path;
    }

    private static IObservable<int> CreateResourceCountStream(ClusterWorkspace cluster, Type resourceType)
    {
        return cluster.Runtime.GetResourceCount(resourceType)
            .DistinctUntilChanged()
            .Publish(counts => counts.Take(1).Merge(counts.Skip(1).Sample(TimeSpan.FromMilliseconds(100), AvaloniaScheduler.Instance)))
            .ObserveOn(AvaloniaScheduler.Instance)
            .Replay(1)
            .RefCount();
    }

    private static NavigationLink CreateNavigationLink(ClusterWorkspace cluster, string id, string name, int order = 0)
    {
        return new NavigationLink
        {
            Cluster = cluster,
            Id = $"{cluster.Runtime.Name}-{id}",
            Name = name,
            ViewModelKey = id,
            Order = order,
            SvgIcon = null,
            FluentIcon = id switch
            {
                NavigationTargets.ClusterWorkspace => Icon.Desktop,
                NavigationTargets.Visualization => Icon.DataUsage,
                NavigationTargets.ClusterSettings => Icon.Settings,
                NavigationTargets.PortForwarders => Icon.CloudFlow,
                "load-yaml" => Icon.ArrowUpload,
                "load-folder" => Icon.FolderAdd,
                _ => null,
            }
        };
    }

    private async Task SelectNavigationLink(NavigationLink link)
    {
        if (link.ViewModelKey == "load-yaml")
        {
            var files = await TopLevelAccessor.GetRequired().StorageProvider.OpenFilePickerAsync(new()
            {
                Title = Assets.Resources.NavigationView_LoadYaml,
                AllowMultiple = true,
                FileTypeFilter = [new("Yaml") { Patterns = ["*.yaml", ".yml"] }]
            }).ConfigureAwait(false);

            foreach (var file in files)
            {
                try
                {
                    var stream = await file.OpenReadAsync().ConfigureAwait(false);
                    await link.Cluster.Runtime.ImportYaml(stream).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Utilities.HandleException(_logger, _notificationManager, ex, "Error loading yaml file", sendNotification: true);
                }
            }
        }
        else if (link.ViewModelKey == "load-folder")
        {
            var folders = await TopLevelAccessor.GetRequired().StorageProvider.OpenFolderPickerAsync(new()
            {
                Title = Assets.Resources.NavigationView_LoadFolder,
                AllowMultiple = false
            }).ConfigureAwait(false);

            foreach (var folder in folders)
            {
                try
                {
                    var path = folder.TryGetLocalPath();
                    if (string.IsNullOrWhiteSpace(path))
                    {
                        continue;
                    }

                    await link.Cluster.Runtime.ImportFolder(path).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Utilities.HandleException(_logger, _notificationManager, ex, "Error loading yaml from folder", sendNotification: true);
                }
            }
        }
        else
        {
            var vmType = link.ViewModelKey switch
            {
                NavigationTargets.ClusterSettings => typeof(ClusterSettingsViewModel),
                NavigationTargets.ClusterWorkspace => typeof(ClusterViewModel),
                NavigationTargets.PortForwarders => typeof(PortForwarderListViewModel),
                NavigationTargets.Visualization => typeof(VisualizationViewModel),
                _ => link.ControlType
            };

            if (vmType == null)
            {
                _logger.LogError("Unable to resolve navigation target for {Name}", link.Name);
                return;
            }

            var vm = _serviceProvider.GetRequiredService(vmType) as IDockable;

            if (vm == null)
            {
                _logger.LogError("Unable to resolve navigation target dockable for {Name}", link.Name);
                return;
            }

            if (vm is IInitializeCluster init)
            {
                init.Initialize(link.Cluster);
            }

            Factory.AddToDocuments(vm);
        }
    }
}
