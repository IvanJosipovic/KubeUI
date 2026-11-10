using Dock.Model.Controls;
using Dock.Model.Core;
using KubernetesClient.Informer.Client;
using KubeUI.App.Infrastructure.Docking;
using KubeUI.App.Infrastructure.Presentation;
using KubeUI.Features.Clusters.Workspace;
using KubeUI.Features.Resources;
using KubeUI.Features.Resources.List;

namespace KubeUI.App.Shell.Navigation;

public interface IResourceNavigationService
{
    void Open(NavigationLink navigation, bool forceNewTab = false);
    Task<bool> OpenResourceListAsync(string? clusterName, string apiVersion, string kind);
}

internal sealed class NavigationDocumentService : IResourceNavigationService
{
    private readonly IResourceListDocumentFactory _resourceListDocumentFactory;
    private readonly ILogger<NavigationDocumentService> _logger;
    private readonly ClusterWorkspaceCatalog _clusterCatalog;
    private readonly Func<IFactory> _factory;

    public NavigationDocumentService(
        IResourceListDocumentFactory resourceListDocumentFactory,
        ILogger<NavigationDocumentService> logger,
        ClusterWorkspaceCatalog clusterCatalog,
        Func<IFactory> factory)
    {
        _resourceListDocumentFactory = resourceListDocumentFactory;
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
        var config = workspace.GetResourceConfigs().OfType<IResourceConfig>().FirstOrDefault(item =>
            string.Equals(item.Kind.Group, group, StringComparison.Ordinal)
            && string.Equals(item.Kind.ApiVersion, version, StringComparison.Ordinal)
            && string.Equals(item.Kind.Kind, kind, StringComparison.Ordinal));
        if (config is null)
            return false;

        await Dispatcher.UIThread.InvokeAsync(() => Open(new NavigationLink
        {
            Cluster = workspace,
            Name = config.Name,
            ResourceKind = config.Kind
        }));
        return true;
    }

    public void Open(NavigationLink navigation, bool forceNewTab = false)
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

        var document = Create(navigation.Cluster, config);
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

    private IResourceConfig? ResolveConfig(NavigationLink navigation)
    {
        return navigation.ResourceKind is { } kind
            ? navigation.Cluster.GetResourceConfig(kind) as IResourceConfig
            : null;
    }

    private IDockable? Create(ClusterWorkspace cluster, IResourceConfig config)
    {
        if (_resourceListDocumentFactory.Create(cluster, config) is not IDockable document)
        {
            return null;
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
