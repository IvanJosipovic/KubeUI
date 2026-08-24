using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using Dock.Model.Controls;
using Dock.Model.Core;
using FluentAvalonia.UI.Controls;
using FluentIcons.Common;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
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
using KubeUI.Avalonia.Services.Icons;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Shell.Navigation;

public sealed partial class NavigationViewModel : ViewModelBase, IDisposable
{
    private readonly ILogger<NavigationViewModel> _logger;
    private readonly INotificationManager _notificationManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly IDialogService _dialogService;
    public new IFactory Factory => _serviceProvider.GetRequiredService<IFactory>();
    private readonly IResourceNavigationService _documentService;
    private readonly IResourceIconService _iconService;
    private readonly IPlatformServices _platformServices;
    private readonly NavigationResourceSynchronizer _resourceNavigation;
    private readonly NavigationClusterCatalogSynchronizer _clusterNavigation;
    private readonly NavigationSelectionHandler _selection;

    [ObservableProperty]
    public partial ClusterWorkspaceCatalog ClusterCatalog { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<ClusterNavigationNode> Clusters { get; set; } = [];

    public NavigationViewModel(
        ILogger<NavigationViewModel> logger,
        INotificationManager notificationManager,
        IDialogService dialogService,
        ClusterWorkspaceCatalog clusterCatalog,
        IServiceProvider serviceProvider,
        IResourceNavigationService documentService,
        IResourceIconService iconService,
        IPlatformServices platformServices)
    {
        _logger = logger;
        _notificationManager = notificationManager;
        _serviceProvider = serviceProvider;
        _iconService = iconService;
        _platformServices = platformServices;
        _dialogService = dialogService;
        ClusterCatalog = clusterCatalog;
        _documentService = documentService;
        _resourceNavigation = new NavigationResourceSynchronizer(
            _iconService,
            OpenResourceNavigationCommand,
            OpenResourceNavigationInNewTabCommand,
            _logger);
        _selection = new NavigationSelectionHandler(
            _logger,
            _notificationManager,
            _serviceProvider,
            _platformServices,
            dockable => Factory.AddToDocuments(dockable));
        Title = Assets.Resources.NavigationView_Title!;
        Id = nameof(NavigationViewModel);

        _clusterNavigation = new NavigationClusterCatalogSynchronizer(
            ClusterCatalog,
            Clusters,
            SubscribeCluster,
            UnsubscribeCluster,
            (cluster, config) => ApplyResourceConfigNavigation(cluster, config),
            ToggleClusterConnectionCommand,
            OpenClusterSettingsCommand,
            _serviceProvider.GetRequiredService<ILogger<NavigationClusterCatalogSynchronizer>>());
        _clusterNavigation.Reload();
    }

    public void Dispose()
    {
        _clusterNavigation.Dispose();
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
            await HandleClusterSelectionAsync(clusterNode).ConfigureAwait(false);
        }
        else if (item is ResourceNavigationLink resourceNavLink)
        {
            _documentService.Open(resourceNavLink);
        }
        else if (item is NavigationLink navLink)
        {
            await _selection.SelectAsync(navLink).ConfigureAwait(true);
        }

        if (item is NavigationItem nav && nav.NavigationItems.Count > 0 && item is not ClusterNavigationNode)
        {
            nav.IsExpanded = !nav.IsExpanded;
        }
    }

    private async Task HandleClusterSelectionAsync(ClusterNavigationNode clusterNode)
    {
        var cluster = clusterNode.Cluster;

        if (cluster.Runtime.Connected)
        {
            foreach (var resourceConfig in cluster.GetResourceConfigs())
            {
                ApplyResourceConfigNavigation(cluster, resourceConfig);
            }

            clusterNode.IsExpanded = !clusterNode.IsExpanded;
            return;
        }

        await ConnectIfIdleAsync(clusterNode).ConfigureAwait(false);
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

    private Task ConnectIfIdleAsync(ClusterNavigationNode clusterNode)
    {
        if (clusterNode.Cluster.Runtime.Status == ClusterStatus.Connecting)
        {
            return Task.CompletedTask;
        }

        _ = ConnectAndExpandAsync(clusterNode);
        return Task.CompletedTask;
    }

    private async Task ConnectAndExpandAsync(ClusterNavigationNode clusterNode)
    {
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

        return _selection.SelectAsync(CreateNavigationLink(
            clusterNode.Cluster,
            NavigationTargets.ClusterSettings,
            Assets.Resources.ClusterSettingsView_Title!));
    }

    private static NavigationLink CreateNavigationLink(ClusterWorkspace cluster, string id, string name, int order = 0) => new()
    {
        Cluster = cluster,
        Id = $"{cluster.Runtime.Name}-{id}",
        Name = name,
        ViewModelKey = id,
        Order = order,
        FluentIcon = id switch
        {
            NavigationTargets.ClusterWorkspace => Icon.Desktop,
            NavigationTargets.Visualization => Icon.DataUsage,
            NavigationTargets.ClusterSettings => Icon.Settings,
            NavigationTargets.PortForwarders => Icon.CloudFlow,
            NavigationTargets.LoadYaml => Icon.ArrowUpload,
            NavigationTargets.LoadFolder => Icon.FolderAdd,
            _ => null,
        }
    };

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
        if (sender is not IClusterRuntime runtime
            || (e.PropertyName != nameof(IClusterRuntime.Status)
                && e.PropertyName != nameof(IClusterRuntime.Connected)))
        {
            return;
        }

        _logger.LogDebug(
            "Navigation runtime state event received for {ClusterName}: {PropertyName}; Connected={Connected}; Status={Status}",
            runtime.Name,
            e.PropertyName,
            runtime.Connected,
            runtime.Status);

        Dispatcher.UIThread.Post(() =>
        {
            if (!_clusterNavigation.TryGetWorkspace(runtime, out var cluster))
            {
                _logger.LogDebug("Navigation runtime state event ignored for unknown runtime {ClusterName}", runtime.Name);
                return;
            }

            if (!_clusterNavigation.TryGetNode(cluster, out var node))
            {
                _logger.LogDebug("Navigation runtime state event ignored because node is missing for {ClusterName}", runtime.Name);
                return;
            }

            if (e.PropertyName == nameof(IClusterRuntime.Connected))
            {
                _logger.LogDebug(
                    "Navigation connection state applying for {ClusterName}: Connected={Connected}; ExistingItems={ExistingItems}",
                    runtime.Name,
                    runtime.Connected,
                    node.NavigationItems.Count);
                node.UpdateConnectionNavigation(runtime.Connected);
            }

            if (runtime.Connected
                && runtime.Status == ClusterStatus.Connected)
            {
                var resourceConfigs = cluster.GetResourceConfigs().ToArray();
                foreach (var resourceConfig in resourceConfigs)
                {
                    ApplyResourceConfigNavigation(cluster, resourceConfig);
                }

                _logger.LogDebug(
                    "Navigation resource state replayed for {ClusterName}; ResourceConfigs={ResourceConfigs}; Items={Items}",
                    runtime.Name,
                    resourceConfigs.Length,
                    node.NavigationItems.Count);
            }

            if (runtime.Status == ClusterStatus.Errored)
            {
                ShowClusterError(runtime.LastError);
            }
        });
    }

    private void OnNamespaceSelectionRequired(IClusterRuntime runtime)
    {
        if (_clusterNavigation.TryGetWorkspace(runtime, out var cluster))
        {
            Dispatcher.UIThread.Post(() => _ = ShowMissingNamespacePermissionPromptAsync(cluster));
        }
    }

    private void OnClusterResourceConfigProcessed(ClusterWorkspace cluster, IResourceConfig resourceConfig)
    {
        _logger.LogDebug(
            "Navigation resource config event received for {ClusterName}: {ResourceKind}; PermissionsLoaded={PermissionsLoaded}; CanListAndWatch={CanListAndWatch}; Connected={Connected}; Status={Status}",
            cluster.Runtime.Name,
            resourceConfig.Kind,
            resourceConfig.PermissionsLoaded,
            resourceConfig.CanListAndWatch,
            cluster.Runtime.Connected,
            cluster.Runtime.Status);

        if (Dispatcher.UIThread.CheckAccess())
        {
            ApplyResourceConfigNavigation(cluster, resourceConfig);
            return;
        }

        Dispatcher.UIThread.Post(() => ApplyResourceConfigNavigation(cluster, resourceConfig));
    }

    private void ApplyResourceConfigNavigation(
        ClusterWorkspace cluster,
        IResourceConfig resourceConfig,
        IReadOnlyCollection<IResourceConfig>? resourceConfigs = null)
    {
        _resourceNavigation.Apply(cluster, resourceConfig, resourceConfigs, _clusterNavigation.Nodes);
        if (_clusterNavigation.TryGetNode(cluster, out var node))
        {
            _logger.LogDebug(
                "Navigation resource config applied for {ClusterName}: {ResourceKind}; Items={Items}",
                cluster.Runtime.Name,
                resourceConfig.Kind,
                node.NavigationItems.Count);
        }
    }

    private void OnClusterCustomResourceDefinitionRemoved(ClusterWorkspace cluster, GroupApiVersionKind removedKind)
    {
        _logger.LogDebug(
            "Navigation custom resource definition removal received for {ClusterName}: {ResourceKind}",
            cluster.Runtime.Name,
            removedKind);

        Dispatcher.UIThread.Post(() =>
        {
            if (!_clusterNavigation.TryGetNode(cluster, out var node))
            {
                return;
            }

            _resourceNavigation.RemoveCustomResourceDefinition(node, removedKind);
            foreach (var resourceConfig in cluster.GetResourceConfigs())
            {
                ApplyResourceConfigNavigation(cluster, resourceConfig);
            }

            _logger.LogDebug(
                "Navigation custom resource definition removal applied for {ClusterName}: {ResourceKind}; Items={Items}",
                cluster.Runtime.Name,
                removedKind,
                node.NavigationItems.Count);
        });
    }

    private void OnClusterResourceSeeded(IClusterRuntime runtime, GroupApiVersionKind kind)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (!_clusterNavigation.TryGetWorkspace(runtime, out var cluster))
            {
                return;
            }

            _resourceNavigation.AttachResourceCount(cluster, kind, _clusterNavigation.Nodes);
        });
    }

}
