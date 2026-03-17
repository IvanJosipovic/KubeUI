using System.Collections.Specialized;
using System.ComponentModel;
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
    private const int ClusterWorkspaceOrder = -500;
    private const int VisualizationOrder = -490;
    private const int ClusterSettingsOrder = -480;
    private const int LoadYamlOrder = -470;
    private const int LoadFolderOrder = -460;
    private const int PortForwardersOrder = -450;

    private readonly ILogger<NavigationViewModel> _logger;
    private readonly Dictionary<ClusterWorkspaceViewModel, ClusterNavigationNode> _clusterNodes = [];
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
            collection.CollectionChanged += ClusterCatalog_CollectionChanged;
        }

        ReloadClusters();
    }

    public void Dispose()
    {
        if (ClusterCatalog.Clusters is INotifyCollectionChanged collection)
        {
            collection.CollectionChanged -= ClusterCatalog_CollectionChanged;
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
            clusterNode.IsExpanded = !clusterNode.IsExpanded;

            if (!clusterNode.Cluster.Connected)
            {
                await clusterNode.Cluster.Connect();
            }

            RebuildClusterNavigation(clusterNode);
        }
        else if (item is ResourceNavigationLink resourceNavLink)
        {
            SelectResourceNavigationLink(resourceNavLink);
        }
        else if (item is NavigationLink navLink)
        {
            await SelectNavigationLink(navLink);
        }

        if (item is NavigationItem nav && nav.NavigationItems.Count > 0)
        {
            nav.IsExpanded = !nav.IsExpanded;
        }
    }

    private void ClusterCatalog_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
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

            RebuildClusterNavigation(node);
            _clusterNodes.Add(cluster, node);
            Clusters.Add(node);
        }
    }

    private void SubscribeCluster(ClusterWorkspaceViewModel cluster)
    {
        cluster.OnChange += Cluster_OnChange;

        if (cluster is INotifyPropertyChanged propertyChanged)
        {
            propertyChanged.PropertyChanged += Cluster_PropertyChanged;
        }
    }

    private void UnsubscribeCluster(ClusterWorkspaceViewModel cluster)
    {
        cluster.OnChange -= Cluster_OnChange;

        if (cluster is INotifyPropertyChanged propertyChanged)
        {
            propertyChanged.PropertyChanged -= Cluster_PropertyChanged;
        }
    }

    private void Cluster_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not ClusterWorkspaceViewModel cluster)
        {
            return;
        }

        if (e.PropertyName is nameof(ClusterWorkspaceViewModel.Connected) or nameof(ClusterWorkspaceViewModel.Status))
        {
            Dispatcher.UIThread.Post(() => RebuildClusterNavigation(cluster));
        }
    }

    private void Cluster_OnChange(WatchEventType eventType, GroupApiVersionKind kind, IKubernetesObject<V1ObjectMeta> item)
    {
        if (item is not V1CustomResourceDefinition)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            foreach (var node in _clusterNodes.Values)
            {
                RebuildClusterNavigation(node);
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
        node.NavigationItems.Clear();

        foreach (var item in BuildNavigationItems(node.Cluster))
        {
            node.NavigationItems.Add(item);
        }
    }

    private IEnumerable<NavigationItem> BuildNavigationItems(ClusterWorkspaceViewModel cluster)
    {
        var items = new List<NavigationItem>
        {
            CreateNavigationLink(cluster, NavigationTargets.ClusterWorkspace, Assets.Resources.ClusterViewModel_Title, ClusterWorkspaceOrder),
            CreateNavigationLink(cluster, NavigationTargets.Visualization, Assets.Resources.VisualizationViewModel_Title, VisualizationOrder),
            CreateNavigationLink(cluster, NavigationTargets.ClusterSettings, Assets.Resources.ClusterSettingsViewModel_Title, ClusterSettingsOrder),
            CreateNavigationLink(cluster, "load-yaml", Assets.Resources.NavigationViewModel_LoadYaml, LoadYamlOrder),
            CreateNavigationLink(cluster, "load-folder", Assets.Resources.NavigationViewModel_LoadFolder, LoadFolderOrder),
            CreateNavigationLink(cluster, NavigationTargets.PortForwarders, Assets.Resources.PortForwarderListViewModel_Title, PortForwardersOrder)
        };

        var categories = new Dictionary<string, NavigationItem>(StringComparer.Ordinal);

        foreach (var resourceConfig in cluster.GetResourceConfigs().OrderBy(config => config.Order).ThenBy(config => config.Name, StringComparer.Ordinal))
        {
            var link = CreateResourceNavigationLink(cluster, resourceConfig);

            if (string.IsNullOrWhiteSpace(resourceConfig.Category))
            {
                items.Add(link);
                continue;
            }

            if (!categories.TryGetValue(resourceConfig.Category, out var category))
            {
                category = new NavigationItem
                {
                    Id = $"{cluster.Name}-category-{resourceConfig.Category}",
                    Name = resourceConfig.Category,
                    Order = resourceConfig.Order,
                };

                categories.Add(resourceConfig.Category, category);
                items.Add(category);
            }

            category.NavigationItems.Add(link);
        }

        return items;
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
            FluentIcon = id switch
            {
                NavigationTargets.ClusterWorkspace => Icon.Desktop,
                NavigationTargets.Visualization => Icon.DataUsage,
                NavigationTargets.ClusterSettings => Icon.Settings,
                NavigationTargets.PortForwarders => Icon.CloudFlow,
                "load-yaml" => Icon.Code,
                "load-folder" => Icon.FolderOpen,
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

