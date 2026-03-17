using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Dock.Model.Controls;
using Dock.Model.Core;
using FluentIcons.Common;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Client;
using KubeUI.Resources;

namespace KubeUI.ViewModels;

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
    private readonly Dictionary<ClusterWorkspaceViewModel, CancellationTokenSource> _clusterRebuildDelays = [];
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
    }

    public async void TreeView_SelectionChanged(object? item)
    {
        if (item is ClusterNavigationNode clusterNode)
        {
            await HandleClusterSelectionAsync(clusterNode);
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

    private async Task HandleClusterSelectionAsync(ClusterNavigationNode clusterNode)
    {
        var cluster = clusterNode.Cluster;

        if (cluster.Connected)
        {
            clusterNode.IsExpanded = !clusterNode.IsExpanded;
            RebuildClusterNavigation(clusterNode);
            return;
        }

        try
        {
            await cluster.Connect();
        }
        catch (Exception ex)
        {
            Utilities.HandleException(_logger, _notificationManager, ex, "Error connecting to cluster", sendNotification: true);
            RebuildClusterNavigation(clusterNode);
            return;
        }

        clusterNode.IsExpanded = true;
        RebuildClusterNavigation(clusterNode);
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
        cluster.ResourceConfigsChanged += OnClusterResourceConfigsChanged;

        if (cluster is INotifyPropertyChanged propertyChanged)
        {
            propertyChanged.PropertyChanged += OnClusterPropertyChanged;
        }
    }

    private void UnsubscribeCluster(ClusterWorkspaceViewModel cluster)
    {
        cluster.ResourceConfigsChanged -= OnClusterResourceConfigsChanged;

        if (cluster is INotifyPropertyChanged propertyChanged)
        {
            propertyChanged.PropertyChanged -= OnClusterPropertyChanged;
        }

        if (_clusterRebuildDelays.Remove(cluster, out var cancellationTokenSource))
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
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
            Dispatcher.UIThread.Post(() => RebuildClusterNavigation(cluster));
        }
    }

    private void OnClusterResourceConfigsChanged(ClusterWorkspaceViewModel cluster)
    {
        ScheduleClusterNavigationRebuild(cluster, ResourceNavigationRebuildDelay);
    }

    private void ScheduleClusterNavigationRebuild(ClusterWorkspaceViewModel cluster, TimeSpan delay)
    {
        if (_clusterRebuildDelays.Remove(cluster, out var existing))
        {
            existing.Cancel();
            existing.Dispose();
        }

        var cancellationTokenSource = new CancellationTokenSource();
        _clusterRebuildDelays[cluster] = cancellationTokenSource;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(delay, cancellationTokenSource.Token).ConfigureAwait(false);
                await Dispatcher.UIThread.InvokeAsync(() => RebuildClusterNavigation(cluster));
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                if (_clusterRebuildDelays.TryGetValue(cluster, out var current) && ReferenceEquals(current, cancellationTokenSource))
                {
                    _clusterRebuildDelays.Remove(cluster);
                }

                cancellationTokenSource.Dispose();
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
        var expandedState = new Dictionary<string, bool>(StringComparer.Ordinal);
        CaptureExpandedState(node.NavigationItems, expandedState);

        node.NavigationItems.Clear();

        foreach (var item in BuildNavigationItems(node.Cluster))
        {
            ApplyExpandedState(item, expandedState);
            node.NavigationItems.Add(item);
        }
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
                definitionsLink.Count ??= TryGetResourceCount(cluster, typeof(V1CustomResourceDefinition));
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

        PopulateCustomResourceDefinitions(definitionsLink, customResourceConfigs, items, cluster);

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

    private static void PopulateCustomResourceDefinitions(ResourceNavigationLink? definitionsLink, IEnumerable<IResourceConfig> customResourceConfigs, List<NavigationItem> items, ClusterWorkspaceViewModel cluster)
    {
        if (definitionsLink == null)
        {
            return;
        }

        var configs = customResourceConfigs as IList<IResourceConfig> ?? customResourceConfigs.ToList();

        var root = new NavigationItem
        {
            Id = $"{cluster.Name}-custom-resource-definitions",
            Name = CustomResourceDefinitionsCategoryName,
            Order = CustomResourceDefinitionsCategoryOrder,
        };

        root.NavigationItems.Add(definitionsLink);

        foreach (var config in configs.OrderBy(config => config.Kind.Group, StringComparer.Ordinal).ThenBy(config => config.Name, StringComparer.Ordinal))
        {
            var currentList = root.NavigationItems;
            var pathParts = ConstructCustomResourceGroupPath(config.Kind.Group);
            var path = new List<string>(pathParts.Count);

            foreach (var part in pathParts)
            {
                path.Add(part);
                var pathKey = string.Join("/", path);
                var groupId = $"{cluster.Name}-crd-group-{pathKey}";
                var groupNode = currentList.FirstOrDefault(x => x.Id == groupId);

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

            currentList.Add(CreateResourceNavigationLink(cluster, config));
        }

        items.Add(root);
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
        return cluster.Status is ClusterStatus.Connecting or ClusterStatus.Connected;
    }

    private static IObservable<int>? TryGetResourceCount(ClusterWorkspaceViewModel cluster, Type resourceType)
    {
        try
        {
            var kind = GroupApiVersionKind.From(resourceType);
            return cluster.Objects.ContainsKey(kind) ? cluster.GetResourceCount(resourceType) : null;
        }
        catch
        {
            return null;
        }
    }

    private bool CanListAndWatchResource(ClusterWorkspaceViewModel cluster, IResourceConfig resourceConfig)
    {
        return resourceConfig.CanListAndWatch;
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
        };
    }

    private void SelectResourceNavigationLink(ResourceNavigationLink nav)
    {
        var expectedId = $"{nav.Cluster.Name}-{GroupApiVersionKind.From(nav.ControlType)}";

        if (TryActivateExistingDocument(expectedId))
        {
            nav.Count ??= nav.Cluster.GetResourceCount(nav.ControlType);
            return;
        }

        var resourceListType = typeof(ResourceListViewModel<>).MakeGenericType(nav.ControlType);
        var vm = Application.Current.GetRequiredService(resourceListType) as IDockable;

        if (vm is IInitializeCluster init)
        {
            init.Initialize(nav.Cluster);
        }

        nav.Count ??= nav.Cluster.GetResourceCount(nav.ControlType);

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
                    await link.Cluster.ImportFolder(folder.TryGetLocalPath());
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

        var documents = Factory.GetDockable<IDocumentDock>("Documents");
        Factory.SetActiveDockable(existing);
        Factory.SetFocusedDockable(documents, existing);
        return true;
    }
}

