using Dock.Model.Controls;
using Dock.Model.Core;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Features.Resources.List;
using KubeUI.Avalonia.Infrastructure.Docking;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Resources;

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

        await global::Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => Open(new ResourceNavigationLink
        {
            Cluster = workspace,
            Name = config.Name,
            ControlType = config.Type
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
            if (existing is IResourceListViewModel list
                && list.ResourceConfig.Type != config.Type)
            {
                existing = Replace(list, config.Type) ?? existing;
            }

            Activate(existing);
            return;
        }

        var document = Create(navigation.Cluster, config.Type);
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
        return navigation.ControlType == null
            ? null
            : navigation.Cluster.GetResourceConfigs()
                .FirstOrDefault(config => config.Kind == GroupApiVersionKind.From(navigation.ControlType));
    }

    private IDockable? Create(ClusterWorkspace cluster, Type resourceType)
    {
        var type = typeof(ResourceListViewModel<>).MakeGenericType(resourceType);
        if (_serviceProvider.GetRequiredService(type) is not IDockable document)
        {
            return null;
        }

        if (document is IInitializeCluster initialize)
        {
            initialize.Initialize(cluster);
        }

        return document;
    }

    private IDockable? Replace(IResourceListViewModel existing, Type resourceType)
    {
        if (existing is not IDockable oldDocument)
        {
            return null;
        }

        var documents = _factory().GetDockable<IDocumentDock>("Documents");
        if (documents?.VisibleDockables == null)
        {
            return null;
        }

        if (Create(existing.Cluster, resourceType) is not IResourceListViewModel replacement)
        {
            return null;
        }

        var replacementDockable = (IDockable)replacement;
        replacementDockable.Id = oldDocument.Id;
        replacement.IsNamespaceSelectionLinked = existing.IsNamespaceSelectionLinked;
        replacement.SearchQuery = existing.SearchQuery;

        if (!replacement.IsNamespaceSelectionLinked)
        {
            replacement.SelectedNamespaces.Clear();
            foreach (var selectedNamespace in existing.SelectedNamespaces)
            {
                replacement.SelectedNamespaces.Add(selectedNamespace);
            }
        }

        var index = documents.VisibleDockables.IndexOf(oldDocument);
        var wasActive = ReferenceEquals(documents.ActiveDockable, oldDocument);
        _factory().CloseDockable(oldDocument);
        _factory().InsertDockable(documents, replacementDockable, Math.Max(0, Math.Min(index, documents.VisibleDockables.Count)));

        if (wasActive)
        {
            Activate(replacementDockable);
        }

        return replacementDockable;
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
