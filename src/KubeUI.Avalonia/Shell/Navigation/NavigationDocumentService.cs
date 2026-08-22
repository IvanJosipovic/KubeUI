using Dock.Model.Controls;
using Dock.Model.Core;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Features.Resources.List;
using KubeUI.Avalonia.Infrastructure.Docking;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Resources;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Shell.Navigation;

public interface IResourceNavigationService
{
    void Open(ResourceNavigationLink navigation, bool forceNewTab = false);
    Task<bool> OpenResourceListAsync(string? clusterName, string apiVersion, string kind);
}

internal sealed class NavigationDocumentService : IResourceNavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NavigationDocumentService> _logger;
    private readonly ClusterWorkspaceCatalog _clusterCatalog;
    private readonly Func<IFactory> _factory;

    public NavigationDocumentService(
        IServiceProvider serviceProvider,
        ILogger<NavigationDocumentService> logger,
        ClusterWorkspaceCatalog clusterCatalog,
        Func<IFactory> factory)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _clusterCatalog = clusterCatalog;
        _factory = factory;
    }

    public async Task<bool> OpenResourceListAsync(string? clusterName, string apiVersion, string kind)
    {
        if (string.IsNullOrWhiteSpace(apiVersion) || string.IsNullOrWhiteSpace(kind))
            return false;

        var workspace = string.IsNullOrWhiteSpace(clusterName)
            ? _clusterCatalog.GetDefault()
            : _clusterCatalog.GetCluster(clusterName);
        if (workspace is null)
            return false;

        await workspace.Connect().ConfigureAwait(false);
        if (!workspace.Runtime.Connected)
            return false;

        var parts = apiVersion.Split('/', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var group = parts.Length == 2 ? parts[0] : string.Empty;
        var version = parts.Length == 2 ? parts[1] : apiVersion;
        var config = workspace.GetResourceConfigs().FirstOrDefault(item =>
            string.Equals(item.Kind.Group, group, StringComparison.Ordinal)
            && string.Equals(item.Kind.ApiVersion, version, StringComparison.Ordinal)
            && string.Equals(item.Kind.Kind, kind, StringComparison.Ordinal));
        if (config is null)
            return false;

        await Dispatcher.UIThread.InvokeAsync(() => Open(new ResourceNavigationLink
        {
            Cluster = workspace,
            Name = config.Name,
            ResourceKind = config.Kind
        }));
        return true;
    }

    public void Open(ResourceNavigationLink navigation, bool forceNewTab = false)
    {
        var config = ResolveConfig(navigation);
        if (config == null)
        {
            _logger.LogError("Unable to resolve resource navigation target for {Name}", navigation.Name);
            return;
        }

        var existing = !forceNewTab ? FindExisting(navigation.Cluster, config.Kind) : null;
        if (existing != null)
        {
            Activate(existing);
            return;
        }

        var document = Create(navigation.Cluster, config.Kind);
        if (document == null)
        {
            _logger.LogError("Unable to resolve resource list view model for {Name}", navigation.Name);
            return;
        }

        if (forceNewTab)
        {
            document.Id = CreateUniqueId($"{navigation.Cluster.Runtime.Name}-{config.Kind}");
        }

        _factory().AddToDocuments(document);
    }

    private IResourceConfig? ResolveConfig(ResourceNavigationLink navigation)
    {
        return navigation.ResourceKind is { } kind
            ? navigation.Cluster.GetResourceConfig(kind)
            : null;
    }

    private IDockable? Create(ClusterWorkspace cluster, GroupApiVersionKind kind)
    {
        if (!cluster.Runtime.ModelCatalog.TryGetResourceType(kind, out var resourceType))
        {
            return null;
        }

        var type = typeof(ResourceListViewModel<>).MakeGenericType(resourceType);
        if (_serviceProvider.GetRequiredService(type) is not IDockable document)
        {
            return null;
        }

        if (document is IResourceListViewModel resourceList)
        {
            resourceList.InitializeResource(cluster, kind);
        }
        else if (document is IInitializeCluster initialize)
        {
            initialize.Initialize(cluster);
        }

        return document;
    }

    private IDockable? FindExisting(ClusterWorkspace cluster, GroupApiVersionKind kind)
    {
        return _factory().GetDockable<IDocumentDock>("Documents")?.VisibleDockables?
            .OfType<IResourceListViewModel>()
            .FirstOrDefault(list => ReferenceEquals(list.Cluster, cluster) && list.Kind == kind) as IDockable;
    }

    private void Activate(IDockable document)
    {
        var factory = _factory();
        var documents = factory.GetDockable<IDocumentDock>("Documents")!;
        factory.SetActiveDockable(document);
        factory.SetFocusedDockable(documents, document);
    }

    private string CreateUniqueId(string baseId)
    {
        var ids = _factory().GetDockable<IDocumentDock>("Documents")?.VisibleDockables?
            .Select(static dockable => dockable.Id)
            .Where(static id => !string.IsNullOrWhiteSpace(id))
            .ToHashSet(StringComparer.Ordinal)
            ?? [];

        if (!ids.Contains(baseId))
        {
            return baseId;
        }

        var suffix = 2;
        while (ids.Contains($"{baseId}#{suffix}"))
        {
            suffix++;
        }

        return $"{baseId}#{suffix}";
    }
}
