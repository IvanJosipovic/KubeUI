using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Dock.Model.Controls;
using Dock.Model.Core;
using FluentIcons.Common;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Kubernetes;
using KubeUI.Avalonia.Resources;

namespace KubeUI.Avalonia.ViewModels;

public sealed partial class NavigationViewModel : ViewModelBase, IDisposable
{
    private static readonly TimeSpan ResourceNavigationRebuildDelay = TimeSpan.FromMilliseconds(200);
    private const int ClusterWorkspaceOrder = -500;
    private const int VisualizationOrder = -490;
    private const int ClusterSettingsOrder = -480;
    private const int LoadYamlOrder = -470;
    private const int LoadFolderOrder = -460;
    private const int PortForwardersOrder = -450;
    private const int NetworkCategoryOrder = 10;
    private const int CategoryOrderOffset = 1000;
    private const int CustomResourceDefinitionsCategoryOrder = 13 + CategoryOrderOffset;
    private const string NetworkCategoryName = "Network";
    private const string CustomResourceDefinitionsCategoryName = "Custom Resource Definitions";

    private static readonly Dictionary<string, int> CategoryOrderOverrides = new(StringComparer.Ordinal)
    {
        ["Workloads"] = 8,
        ["Configuration"] = 9,
        ["Network"] = 10,
        ["Storage"] = 11,
        ["Access Control"] = 12,
        ["Custom Resource Definitions"] = 13,
    };

    private readonly ILogger<NavigationViewModel> _logger;
    private readonly Dictionary<ClusterWorkspaceViewModel, ClusterNavigationNode> _clusterNodes = [];
    private readonly Dictionary<ClusterWorkspaceViewModel, PendingClusterNavigationUpdate> _clusterRebuildDelays = [];
    private INotificationManager _notificationManager => Application.Current.GetRequiredService<INotificationManager>();

    [ObservableProperty]
    public partial ClusterWorkspaceCatalog ClusterCatalog { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<ClusterNavigationNode> Clusters { get; set; } = [];

    public NavigationViewModel()
    {
        ClusterCatalog = Application.Current.GetRequiredService<ClusterWorkspaceCatalog>();
        Title = Assets.Resources.NavigationViewModel_Title;
        Id = nameof(NavigationViewModel);
        _logger = Application.Current.GetRequiredService<ILogger<NavigationViewModel>>();

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
        Clusters.Clear();
    }

    [RelayCommand]
    private Task HandleSelectionChangedAsync(SelectionChangedEventArgs? e)
    {
        var selectedItem = e?.AddedItems.Count > 0 ? e.AddedItems[0] : null;
        return TreeViewSelectionChangedAsync(selectedItem);
    }

    public async Task TreeViewSelectionChangedAsync(object? item)
    {
        if (item is ClusterNavigationNode clusterNode)
        {
            HandleClusterSelection(clusterNode);
        }
        else if (item is ResourceNavigationLink resourceNavLink)
        {
            SelectResourceNavigationLink(resourceNavLink);
        }
        else if (item is NavigationLink navLink)
        {
            await SelectNavigationLink(navLink);
        }

        if (item is NavigationItem nav && nav.NavigationItems.Count > 0 && item is not ClusterNavigationNode)
        {
            nav.IsExpanded = !nav.IsExpanded;
        }
    }

    private void HandleClusterSelection(ClusterNavigationNode clusterNode)
    {
        var cluster = clusterNode.Cluster;

        if (cluster.Connected)
        {
            _ = EnsureClusterWorkspaceReadyAsync(clusterNode);
            return;
        }

        if (cluster.Status == ClusterStatus.Connecting)
        {
            return;
        }

        _ = ConnectClusterAsync(clusterNode);
    }

    private async Task EnsureClusterWorkspaceReadyAsync(ClusterNavigationNode clusterNode)
    {
        await clusterNode.Cluster.EnsureWorkspaceStateInitializedAsync().ConfigureAwait(false);

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            clusterNode.IsExpanded = !clusterNode.IsExpanded;
            RebuildClusterNavigation(clusterNode);
        });
    }

    private async Task ConnectClusterAsync(ClusterNavigationNode clusterNode)
    {
        var cluster = clusterNode.Cluster;

        try
        {
            await Task.Run(cluster.Connect).ConfigureAwait(false);

            if (cluster.Status == ClusterStatus.Connected)
            {
                await cluster.EnsureWorkspaceStateInitializedAsync().ConfigureAwait(false);
            }

            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (cluster.RequiresNamespaceSelectionPrompt)
                {
                    await ShowMissingNamespacePermissionPromptAsync(cluster);
                }
                else if (cluster.Status == ClusterStatus.Errored)
                {
                    ShowClusterError(cluster.LastError);
                }

                RebuildClusterNavigation(clusterNode);
            });
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                cluster.LastError = ex.Message;
                cluster.Status = ClusterStatus.Errored;
                ShowClusterError(ex.Message);
                RebuildClusterNavigation(clusterNode);
            });
        }
    }

    private async Task ShowMissingNamespacePermissionPromptAsync(ClusterWorkspaceViewModel cluster)
    {
        var settingsVm = Application.Current.GetRequiredService<ClusterSettingsViewModel>();
        settingsVm.Initialize(cluster);
        Factory.AddToDocuments(settingsVm);

        var dialogService = Application.Current.GetRequiredService<IDialogService>();
        var settings = new ContentDialogSettings
        {
            Title = Assets.Resources.Cluster_Missing_Namespace_Permission_Title,
            Content = Assets.Resources.Cluster_Missing_Namespace_Permission_Content,
            PrimaryButtonText = Assets.Resources.Cluster_Missing_Namespace_Permission_Primary,
            DefaultButton = ContentDialogButton.Primary
        };

        await dialogService.ShowContentDialogAsync(this, settings);
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

        var vm = Application.Current.GetRequiredService<ClusterErrorViewModel>();
        vm.Id = id;
        vm.Error = error;
        Factory.AddToDocuments(vm);
    }

    private void OnClusterCatalogCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ReloadClusters();
    }

    private void ReloadClusters()
    {
        foreach (var cluster in _clusterNodes.Keys.ToList())
        {
            UnsubscribeCluster(cluster);
        }

        _clusterNodes.Clear();
        Clusters.Clear();

        foreach (var cluster in ClusterCatalog.Clusters)
        {
            SubscribeCluster(cluster);

            var node = new ClusterNavigationNode
            {
                Cluster = cluster,
            };

            _clusterNodes.Add(cluster, node);
            Clusters.Add(node);
            RebuildClusterNavigation(node);
        }
    }

    private void SubscribeCluster(ClusterWorkspaceViewModel cluster)
    {
        cluster.ResourcePermissionsChanged += OnClusterResourcePermissionsChanged;
        cluster.CustomResourceDefinitionsChanged += OnClusterCustomResourceDefinitionsChanged;

        if (cluster is INotifyPropertyChanged propertyChanged)
        {
            propertyChanged.PropertyChanged += OnClusterPropertyChanged;
        }
    }

    private void UnsubscribeCluster(ClusterWorkspaceViewModel cluster)
    {
        cluster.ResourcePermissionsChanged -= OnClusterResourcePermissionsChanged;
        cluster.CustomResourceDefinitionsChanged -= OnClusterCustomResourceDefinitionsChanged;

        if (cluster is INotifyPropertyChanged propertyChanged)
        {
            propertyChanged.PropertyChanged -= OnClusterPropertyChanged;
        }

        if (_clusterRebuildDelays.Remove(cluster, out var pendingUpdate))
        {
            pendingUpdate.CancellationTokenSource.Cancel();
            pendingUpdate.CancellationTokenSource.Dispose();
        }

    }

    private void OnClusterPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not ClusterWorkspaceViewModel cluster)
        {
            return;
        }

        if (e.PropertyName == nameof(ClusterWorkspaceViewModel.Connected))
        {
            if (cluster.Connected)
            {
                cluster.IsExpanded = true;
            }

            Dispatcher.UIThread.Post(() => RebuildClusterNavigation(cluster));
            return;
        }

        if (e.PropertyName == nameof(ClusterWorkspaceViewModel.Status))
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (cluster.Status == ClusterStatus.Connected)
                {
                    cluster.IsExpanded = true;
                }

                RebuildClusterNavigation(cluster);
            });
        }
    }

    private void OnClusterResourcePermissionsChanged(ClusterWorkspaceViewModel cluster)
    {
        Dispatcher.UIThread.Post(() => RebuildClusterNavigation(cluster));
    }

    private void OnClusterCustomResourceDefinitionsChanged(ClusterWorkspaceViewModel cluster, IReadOnlyList<PendingCustomResourceConfig> addedConfigs, IReadOnlyList<GroupApiVersionKind> removedKinds)
    {
        ScheduleClusterNavigationUpdate(cluster, rebuildCustomResourceDefinitionsOnly: true, ResourceNavigationRebuildDelay, addedConfigs, removedKinds);
    }

    private void ScheduleClusterNavigationUpdate(ClusterWorkspaceViewModel cluster, bool rebuildCustomResourceDefinitionsOnly, TimeSpan delay, IReadOnlyList<PendingCustomResourceConfig>? addedConfigs = null, IReadOnlyList<GroupApiVersionKind>? removedKinds = null)
    {
        if (_clusterRebuildDelays.Remove(cluster, out var existing))
        {
            existing.CancellationTokenSource.Cancel();
            existing.CancellationTokenSource.Dispose();
        }

        var pendingUpdate = new PendingClusterNavigationUpdate(new CancellationTokenSource());
        pendingUpdate.Merge(rebuildCustomResourceDefinitionsOnly, addedConfigs, removedKinds);
        _clusterRebuildDelays[cluster] = pendingUpdate;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(delay, pendingUpdate.CancellationTokenSource.Token).ConfigureAwait(false);
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (pendingUpdate.RebuildWholeNavigation)
                    {
                        RebuildClusterNavigation(cluster);
                    }
                    else
                    {
                        ApplyCustomResourceDefinitionChanges(cluster, pendingUpdate.AddedConfigs.Values, pendingUpdate.RemovedKinds);
                    }
                });
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                if (_clusterRebuildDelays.TryGetValue(cluster, out var current) && ReferenceEquals(current, pendingUpdate))
                {
                    _clusterRebuildDelays.Remove(cluster);
                }
                pendingUpdate.CancellationTokenSource.Dispose();
            }
        });
    }

    private void RebuildClusterNavigation(ClusterWorkspaceViewModel cluster)
    {
        if (_clusterNodes.TryGetValue(cluster, out var node))
        {
            RebuildClusterNavigation(node);
        }
    }

    private void RebuildClusterNavigation(ClusterNavigationNode node)
    {
        var desiredItems = BuildNavigationItems(node.Cluster).ToList();
        ReconcileNavigationItems(node.NavigationItems, desiredItems);
    }

    private IEnumerable<NavigationItem> BuildNavigationItems(ClusterWorkspaceViewModel cluster)
    {
        if (!ShouldPopulateClusterNavigation(cluster))
        {
            return [];
        }

        var items = new List<NavigationItem>
        {
            CreateNavigationLink(cluster, NavigationTargets.ClusterWorkspace, Assets.Resources.ClusterViewModel_Title, ClusterWorkspaceOrder),
            CreateNavigationLink(cluster, NavigationTargets.Visualization, Assets.Resources.VisualizationViewModel_Title, VisualizationOrder),
            CreateNavigationLink(cluster, NavigationTargets.ClusterSettings, Assets.Resources.ClusterSettingsViewModel_Title, ClusterSettingsOrder),
            CreateNavigationLink(cluster, "load-yaml", Assets.Resources.NavigationViewModel_LoadYaml, LoadYamlOrder),
            CreateNavigationLink(cluster, "load-folder", Assets.Resources.NavigationViewModel_LoadFolder, LoadFolderOrder)
        };

        var categories = new Dictionary<string, NavigationItem>(StringComparer.Ordinal);
        var customResourceConfigs = new List<IResourceConfig>();
        ResourceNavigationLink? definitionsLink = null;

        foreach (var resourceConfig in cluster.GetResourceConfigs().OrderBy(config => config.Order).ThenBy(config => config.Name, StringComparer.Ordinal))
        {
            if (!CanListAndWatchResource(cluster, resourceConfig))
            {
                continue;
            }

            if (resourceConfig.IsCustomResource)
            {
                customResourceConfigs.Add(resourceConfig);
                continue;
            }

            var link = CreateResourceNavigationLink(cluster, resourceConfig);

            if (resourceConfig.Type == typeof(V1CustomResourceDefinition))
            {
                definitionsLink = link;
                definitionsLink.Name = "Definitions";
                definitionsLink.Order = -1;
                continue;
            }

            if (string.IsNullOrWhiteSpace(resourceConfig.Category))
            {
                items.Add(link);
                continue;
            }

            if (!categories.TryGetValue(resourceConfig.Category, out var category))
            {
                category = EnsureCategory(items, categories, cluster, resourceConfig.Category, resourceConfig.Order);
            }

            category.NavigationItems.Add(link);
        }

        if (CanShowPortForwarders(cluster))
        {
            var networkCategory = EnsureCategory(items, categories, cluster, NetworkCategoryName, NetworkCategoryOrder);
            networkCategory.NavigationItems.Add(CreateNavigationLink(cluster, NavigationTargets.PortForwarders, Assets.Resources.PortForwarderListViewModel_Title, PortForwardersOrder));
        }

        var customResourceDefinitions = BuildCustomResourceDefinitionsNavigationItem(cluster, definitionsLink, customResourceConfigs);
        if (customResourceDefinitions != null)
        {
            items.Add(customResourceDefinitions);
        }

        return items;
    }

    private bool CanShowPortForwarders(ClusterWorkspaceViewModel cluster)
    {
        try
        {
            return cluster.CanIAnyNamespace(typeof(V1Pod), Verb.Create, "portforward");
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Unable to evaluate port forward permissions for cluster {Cluster}", cluster.Name);
            return false;
        }
    }

    private static NavigationItem EnsureCategory(List<NavigationItem> items, Dictionary<string, NavigationItem> categories, ClusterWorkspaceViewModel cluster, string categoryName, int order)
    {
        if (categories.TryGetValue(categoryName, out var category))
        {
            return category;
        }

        category = new NavigationItem
        {
            Id = $"{cluster.Name}-category-{categoryName}",
            Name = categoryName,
            // Keep uncategorized resources (e.g. Namespace/Node/Event) above category groups,
            // while preserving alpha's deterministic category order.
            Order = ResolveCategoryOrder(categoryName, order),
        };

        categories.Add(categoryName, category);
        items.Add(category);
        return category;
    }

    private static int ResolveCategoryOrder(string categoryName, int defaultOrder)
    {
        if (CategoryOrderOverrides.TryGetValue(categoryName, out var fixedOrder))
        {
            return fixedOrder + CategoryOrderOffset;
        }

        return defaultOrder + CategoryOrderOffset;
    }

    private void ApplyCustomResourceDefinitionChanges(ClusterWorkspaceViewModel cluster, IEnumerable<PendingCustomResourceConfig> addedConfigs, IEnumerable<GroupApiVersionKind> removedKinds)
    {
        if (!_clusterNodes.TryGetValue(cluster, out var node))
        {
            return;
        }

        var root = GetOrCreateCustomResourceDefinitionsRoot(node, cluster);
        if (root == null)
        {
            return;
        }

        foreach (var removedKind in removedKinds)
        {
            RemoveCustomResourceDefinition(root, cluster, removedKind);
        }

        foreach (var addedConfig in addedConfigs)
        {
            AddOrUpdateCustomResourceDefinition(root, cluster, addedConfig.ResourceConfig);
        }
    }

    private static NavigationItem? BuildCustomResourceDefinitionsNavigationItem(ClusterWorkspaceViewModel cluster, ResourceNavigationLink? definitionsLink = null, IEnumerable<IResourceConfig>? customResourceConfigs = null)
    {
        var configs = customResourceConfigs as IList<IResourceConfig>
            ?? customResourceConfigs?.ToList()
            ?? cluster.GetResourceConfigs()
                .Where(config => config.IsCustomResource)
                .OrderBy(config => config.Order)
                .ThenBy(config => config.Name, StringComparer.Ordinal)
                .ToList();
        if (definitionsLink == null)
        {
            var resourceConfig = cluster.GetResourceConfigs()
                .OrderBy(config => config.Order)
                .ThenBy(config => config.Name, StringComparer.Ordinal)
                .FirstOrDefault(x => x.Type == typeof(V1CustomResourceDefinition));

            if (resourceConfig == null)
            {
                return null;
            }

            definitionsLink = CreateResourceNavigationLink(cluster, resourceConfig);
            definitionsLink.Name = "Definitions";
            definitionsLink.Order = -1;
        }

        var root = new NavigationItem
        {
            Id = $"{cluster.Name}-custom-resource-definitions",
            Name = CustomResourceDefinitionsCategoryName,
            Order = CustomResourceDefinitionsCategoryOrder,
        };

        root.NavigationItems.Add(definitionsLink);

        for (var i = 0; i < configs.Count; i++)
        {
            AddOrUpdateCustomResourceDefinition(root, cluster, configs[i]);
        }

        return root;
    }

    private static void AddOrUpdateCustomResourceDefinition(NavigationItem root, ClusterWorkspaceViewModel cluster, IResourceConfig config)
    {
        var currentList = root.NavigationItems;
        var pathParts = ConstructCustomResourceGroupPath(config.Kind.Group);
        var path = new List<string>(pathParts.Count);

        for (var i = 0; i < pathParts.Count; i++)
        {
            var part = pathParts[i];
            path.Add(part);
            var groupId = $"{cluster.Name}-crd-group-{string.Join("/", path)}";
            var groupNode = FindNavigationItem(currentList, groupId);

            if (groupNode == null)
            {
                groupNode = new NavigationItem
                {
                    Id = groupId,
                    Name = part,
                    Order = 0,
                };

                currentList.Add(groupNode);
            }

            currentList = groupNode.NavigationItems;
        }

        var resourceId = $"{cluster.Name}-{config.Kind}";
        var existing = FindNavigationItem(currentList, resourceId);
        if (existing is ResourceNavigationLink resourceLink)
        {
            resourceLink.Name = config.Name;
            resourceLink.ControlType = config.Type;
            resourceLink.Order = config.Order;
            resourceLink.Count ??= TryGetResourceCount(cluster, config.Type);
            currentList.Remove(resourceLink);
            currentList.Add(resourceLink);
            return;
        }

        currentList.Add(CreateResourceNavigationLink(cluster, config));
    }

    private static void RemoveCustomResourceDefinition(NavigationItem root, ClusterWorkspaceViewModel cluster, GroupApiVersionKind kind)
    {
        var currentList = root.NavigationItems;
        var pathParts = ConstructCustomResourceGroupPath(kind.Group);
        var pathIds = new List<string>(pathParts.Count);
        var groups = new List<NavigationItem>(pathParts.Count);

        for (var i = 0; i < pathParts.Count; i++)
        {
            pathIds.Add(pathParts[i]);
            var groupId = $"{cluster.Name}-crd-group-{string.Join("/", pathIds)}";
            var groupNode = FindNavigationItem(currentList, groupId);
            if (groupNode == null)
            {
                return;
            }

            groups.Add(groupNode);
            currentList = groupNode.NavigationItems;
        }

        var resourceId = $"{cluster.Name}-{kind}";
        var existing = FindNavigationItem(currentList, resourceId);
        if (existing != null)
        {
            currentList.Remove(existing);
        }

        for (var i = groups.Count - 1; i >= 0; i--)
        {
            var group = groups[i];
            if (group.NavigationItems.Count > 0)
            {
                break;
            }

            var parentItems = i == 0 ? root.NavigationItems : groups[i - 1].NavigationItems;
            parentItems.Remove(group);
        }
    }

    private static NavigationItem? GetOrCreateCustomResourceDefinitionsRoot(ClusterNavigationNode node, ClusterWorkspaceViewModel cluster)
    {
        var rootId = $"{cluster.Name}-custom-resource-definitions";
        var existingRoot = node.NavigationItems.FirstOrDefault(x => x.Id == rootId);
        if (existingRoot != null)
        {
            return existingRoot;
        }

        var root = BuildCustomResourceDefinitionsNavigationItem(cluster, customResourceConfigs: []);
        if (root != null)
        {
            node.NavigationItems.Add(root);
        }

        return root;
    }

    private static NavigationItem? FindNavigationItem(IEnumerable<NavigationItem> items, string id)
    {
        foreach (var item in items)
        {
            if (item.Id == id)
            {
                return item;
            }
        }

        return null;
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

        var fqdnList = new List<string>(levels.Length - 1)
        {
            $"{levels[^2]}.{levels[^1]}"
        };

        for (var i = levels.Length - 3; i >= 0; i--)
        {
            fqdnList.Add(string.Join('.', levels, i, levels.Length - i));
        }

        return fqdnList;
    }

    private static void CaptureExpandedState(IEnumerable<NavigationItem> items, Dictionary<string, bool> expandedState)
    {
        foreach (var item in items)
        {
            expandedState[item.Id] = item.IsExpanded;
            CaptureExpandedState(item.NavigationItems, expandedState);
        }
    }

    private static void ReconcileNavigationItems(ObservableCollection<NavigationItem> currentItems, IReadOnlyList<NavigationItem> desiredItems)
    {
        var desiredById = desiredItems.ToDictionary(item => item.Id, StringComparer.Ordinal);

        for (var i = currentItems.Count - 1; i >= 0; i--)
        {
            var current = currentItems[i];
            if (!desiredById.ContainsKey(current.Id))
            {
                currentItems.RemoveAt(i);
            }
        }

        foreach (var desired in desiredItems)
        {
            var current = FindNavigationItem(currentItems, desired.Id);
            if (current == null)
            {
                currentItems.Add(desired);
                continue;
            }

            if (!CanReuseNavigationItem(current, desired))
            {
                currentItems.Remove(current);
                ApplyExpandedState(desired, CaptureExpandedState(current));
                currentItems.Add(desired);
                continue;
            }

            UpdateNavigationItem(current, desired);
        }
    }

    private static Dictionary<string, bool> CaptureExpandedState(NavigationItem item)
    {
        var expandedState = new Dictionary<string, bool>(StringComparer.Ordinal);
        expandedState[item.Id] = item.IsExpanded;
        CaptureExpandedState(item.NavigationItems, expandedState);
        return expandedState;
    }

    private static bool CanReuseNavigationItem(NavigationItem current, NavigationItem desired)
    {
        return current.GetType() == desired.GetType();
    }

    private static void UpdateNavigationItem(NavigationItem current, NavigationItem desired)
    {
        current.Id = desired.Id;
        current.Name = desired.Name;
        current.Order = desired.Order;
        current.SvgIcon = desired.SvgIcon;
        current.StyleIcon = desired.StyleIcon;
        current.FluentIcon = desired.FluentIcon;

        if (current is NavigationLink currentLink && desired is NavigationLink desiredLink)
        {
            currentLink.Cluster = desiredLink.Cluster;
            currentLink.ControlType = desiredLink.ControlType;
            currentLink.ViewModelKey = desiredLink.ViewModelKey;
        }

        if (current is ResourceNavigationLink currentResourceLink && desired is ResourceNavigationLink desiredResourceLink)
        {
            currentResourceLink.Count ??= desiredResourceLink.Count;
        }

        ReconcileNavigationItems(current.NavigationItems, desired.NavigationItems.ToList());
    }

    private static void ApplyExpandedState(NavigationItem item, IReadOnlyDictionary<string, bool> expandedState)
    {
        if (expandedState.TryGetValue(item.Id, out var isExpanded))
        {
            item.IsExpanded = isExpanded;
        }

        foreach (var child in item.NavigationItems)
        {
            ApplyExpandedState(child, expandedState);
        }
    }

    private static bool ShouldPopulateClusterNavigation(ClusterWorkspaceViewModel cluster)
    {
        return cluster.Status == ClusterStatus.Connected;
    }

    private static IObservable<int> TryGetResourceCount(ClusterWorkspaceViewModel cluster, Type resourceType)
    {
        try
        {
            var kind = GroupApiVersionKind.From(resourceType);
            return cluster.Objects.ContainsKey(kind) ? cluster.GetResourceCount(resourceType) : Observable.Empty<int>();
        }
        catch
        {
            return Observable.Empty<int>();
        }
    }

    private bool CanListAndWatchResource(ClusterWorkspaceViewModel cluster, IResourceConfig resourceConfig)
    {
        if (!resourceConfig.PermissionsLoaded)
        {
            return false;
        }

        if (cluster.CanI(resourceConfig.Type, Verb.List)
            && cluster.CanI(resourceConfig.Type, Verb.Watch))
        {
            return true;
        }

        if (!resourceConfig.IsNamespaced)
        {
            return false;
        }

        foreach (var @namespace in cluster.Namespaces)
        {
            var namespaceName = @namespace.Name();
            if (string.IsNullOrWhiteSpace(namespaceName))
            {
                continue;
            }

            if (cluster.CanI(resourceConfig.Type, Verb.List, namespaceName)
                && cluster.CanI(resourceConfig.Type, Verb.Watch, namespaceName))
            {
                return true;
            }
        }

        return false;
    }

    private static NavigationLink CreateNavigationLink(ClusterWorkspaceViewModel cluster, string id, string name, int order)
    {
        return new NavigationLink
        {
            Cluster = cluster,
            Id = $"{cluster.Name}-{id}",
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

    private static ResourceNavigationLink CreateResourceNavigationLink(ClusterWorkspaceViewModel cluster, IResourceConfig resourceConfig)
    {
        return new ResourceNavigationLink
        {
            Cluster = cluster,
            Id = $"{cluster.Name}-{resourceConfig.Kind}",
            Name = resourceConfig.Name,
            ControlType = resourceConfig.Type,
            Order = resourceConfig.Order,
            // Throttle rapid updates to the resource count to reduce UI churn.
            // Sample emits the latest value at most once per 100ms and ensures
            // updates are observed on the Avalonia UI scheduler. Also avoid
            // emitting duplicate consecutive values.
            Count = TryGetResourceCount(cluster, resourceConfig.Type)
                .Sample(TimeSpan.FromMilliseconds(100), AvaloniaScheduler.Instance)
                .DistinctUntilChanged(),
        };
    }

    private void SelectResourceNavigationLink(ResourceNavigationLink nav)
    {
        if (nav.ControlType == null)
        {
            _logger.LogError("Unable to resolve resource navigation target for {Name}", nav.Name);
            return;
        }

        var expectedId = $"{nav.Cluster.Name}-{GroupApiVersionKind.From(nav.ControlType)}";

        if (Factory.FindDockableById(expectedId) is IDockable existingDocument)
        {
            ActivateDocument(existingDocument);
            if (existingDocument is IResourceListViewModel existingResourceList)
            {
                nav.Count = ObserveResourceListItemCount(existingResourceList);
            }

            return;
        }

        var resourceListType = typeof(ResourceListViewModel<>).MakeGenericType(nav.ControlType);
        var vm = Application.Current.GetRequiredService(resourceListType) as IDockable;

        if (vm == null)
        {
            _logger.LogError("Unable to resolve resource list view model for {Name}", nav.Name);
            return;
        }

        if (vm is IInitializeCluster init)
        {
            init.Initialize(nav.Cluster);
        }

        if (vm is IResourceListViewModel resourceList)
        {
            nav.Count = ObserveResourceListItemCount(resourceList);
        }

        Factory.AddToDocuments(vm);
    }

    private async Task SelectNavigationLink(NavigationLink link)
    {
        if (link.ViewModelKey == "load-yaml")
        {
            var files = await TopLevelAccessor.GetRequired().StorageProvider.OpenFilePickerAsync(new()
            {
                Title = Assets.Resources.NavigationViewModel_LoadYaml,
                AllowMultiple = true,
                FileTypeFilter = [new("Yaml") { Patterns = ["*.yaml", ".yml"] }]
            });

            foreach (var file in files)
            {
                try
                {
                    var stream = await file.OpenReadAsync();
                    await link.Cluster.ImportYaml(stream);
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
                Title = Assets.Resources.NavigationViewModel_LoadFolder,
                AllowMultiple = false
            });

            foreach (var folder in folders)
            {
                try
                {
                    var path = folder.TryGetLocalPath();
                    if (string.IsNullOrWhiteSpace(path))
                    {
                        continue;
                    }

                    await link.Cluster.ImportFolder(path);
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

            var vm = Application.Current.GetRequiredService(vmType) as IDockable;

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

    private bool TryActivateExistingDocument(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return false;
        }

        var existing = Factory.FindDockableById(id);

        if (existing == null)
        {
            return false;
        }

        ActivateDocument(existing);
        return true;
    }

    private void ActivateDocument(IDockable dockable)
    {
        var documents = Factory.GetDockable<IDocumentDock>("Documents")!;
        Factory.SetActiveDockable(dockable);
        Factory.SetFocusedDockable(documents, dockable);
    }

    private static IObservable<int> ObserveResourceListItemCount(IResourceListViewModel resourceList)
    {
        return Observable.Create<int>(observer =>
        {
            observer.OnNext(resourceList.ItemCount);

            if (resourceList is not INotifyPropertyChanged propertyChanged)
            {
                observer.OnCompleted();
                return Disposable.Empty;
            }

            PropertyChangedEventHandler handler = (_, args) =>
            {
                if (string.IsNullOrEmpty(args.PropertyName) || args.PropertyName == nameof(IResourceListViewModel.ItemCount))
                {
                    observer.OnNext(resourceList.ItemCount);
                }
            };

            propertyChanged.PropertyChanged += handler;
            return Disposable.Create(() => propertyChanged.PropertyChanged -= handler);
        }).DistinctUntilChanged();
    }
}

internal sealed class PendingClusterNavigationUpdate
{
    public PendingClusterNavigationUpdate(CancellationTokenSource cancellationTokenSource)
    {
        CancellationTokenSource = cancellationTokenSource;
    }

    public CancellationTokenSource CancellationTokenSource { get; }

    public bool RebuildWholeNavigation { get; private set; }

    public Dictionary<GroupApiVersionKind, PendingCustomResourceConfig> AddedConfigs { get; } = [];

    public HashSet<GroupApiVersionKind> RemovedKinds { get; } = [];

    public void Merge(bool rebuildCustomResourceDefinitionsOnly, IReadOnlyList<PendingCustomResourceConfig>? addedConfigs, IReadOnlyList<GroupApiVersionKind>? removedKinds)
    {
        if (!rebuildCustomResourceDefinitionsOnly)
        {
            RebuildWholeNavigation = true;
            AddedConfigs.Clear();
            RemovedKinds.Clear();
            return;
        }

        if (RebuildWholeNavigation)
        {
            return;
        }

        if (removedKinds != null)
        {
            foreach (var removedKind in removedKinds)
            {
                AddedConfigs.Remove(removedKind);
                RemovedKinds.Add(removedKind);
            }
        }

        if (addedConfigs == null)
        {
            return;
        }

        foreach (var addedConfig in addedConfigs)
        {
            RemovedKinds.Remove(addedConfig.Kind);
            AddedConfigs[addedConfig.Kind] = addedConfig;
        }
    }
}



