using System.Collections.Specialized;
using System.Windows.Input;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Resources;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Shell.Navigation;

internal sealed class NavigationClusterCatalogSynchronizer : IDisposable
{
    private readonly ClusterWorkspaceCatalog _catalog;
    private readonly ObservableCollection<ClusterNavigationNode> _nodes;
    private readonly Action<ClusterWorkspace> _subscribe;
    private readonly Action<ClusterWorkspace> _unsubscribe;
    private readonly Action<ClusterWorkspace, IResourceConfig> _applyResourceConfig;
    private readonly ICommand _toggleConnectionCommand;
    private readonly ICommand _openSettingsCommand;
    private readonly Dictionary<ClusterWorkspace, ClusterNavigationNode> _nodesByWorkspace = [];
    private readonly Dictionary<IClusterRuntime, ClusterWorkspace> _workspacesByRuntime = [];

    public NavigationClusterCatalogSynchronizer(
        ClusterWorkspaceCatalog catalog,
        ObservableCollection<ClusterNavigationNode> nodes,
        Action<ClusterWorkspace> subscribe,
        Action<ClusterWorkspace> unsubscribe,
        Action<ClusterWorkspace, IResourceConfig> applyResourceConfig,
        ICommand toggleConnectionCommand,
        ICommand openSettingsCommand)
    {
        _catalog = catalog;
        _nodes = nodes;
        _subscribe = subscribe;
        _unsubscribe = unsubscribe;
        _applyResourceConfig = applyResourceConfig;
        _toggleConnectionCommand = toggleConnectionCommand;
        _openSettingsCommand = openSettingsCommand;

        if (_catalog.Clusters is INotifyCollectionChanged collection)
        {
            collection.CollectionChanged += OnCollectionChanged;
        }

    }

    public IReadOnlyDictionary<ClusterWorkspace, ClusterNavigationNode> Nodes => _nodesByWorkspace;

    public bool TryGetNode(ClusterWorkspace workspace, out ClusterNavigationNode node) =>
        _nodesByWorkspace.TryGetValue(workspace, out node!);

    public bool TryGetWorkspace(IClusterRuntime runtime, out ClusterWorkspace workspace) =>
        _workspacesByRuntime.TryGetValue(runtime, out workspace!);

    public void Dispose()
    {
        if (_catalog.Clusters is INotifyCollectionChanged collection)
        {
            collection.CollectionChanged -= OnCollectionChanged;
        }

        foreach (var workspace in _nodesByWorkspace.Keys.ToArray())
        {
            Remove(workspace);
        }

        _nodes.Clear();
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (ClusterWorkspace workspace in e.NewItems!)
                {
                    Add(workspace);
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (ClusterWorkspace workspace in e.OldItems!)
                {
                    Remove(workspace);
                }
                break;
            case NotifyCollectionChangedAction.Replace:
                foreach (ClusterWorkspace workspace in e.OldItems!)
                {
                    Remove(workspace);
                }

                foreach (ClusterWorkspace workspace in e.NewItems!)
                {
                    Add(workspace);
                }
                break;
            case NotifyCollectionChangedAction.Reset:
                Reload();
                break;
        }
    }

    public void Reload()
    {
        foreach (var workspace in _nodesByWorkspace.Keys.ToArray())
        {
            Remove(workspace);
        }

        _nodes.Clear();

        foreach (var workspace in _catalog.Clusters)
        {
            Add(workspace);
        }
    }

    private void Add(ClusterWorkspace workspace)
    {
        if (_nodesByWorkspace.ContainsKey(workspace))
        {
            return;
        }

        _subscribe(workspace);
        _workspacesByRuntime[workspace.Runtime] = workspace;

        var node = new ClusterNavigationNode(workspace)
        {
            ToggleConnectionCommand = _toggleConnectionCommand,
            OpenSettingsCommand = _openSettingsCommand,
        };

        _nodesByWorkspace.Add(workspace, node);
        _nodes.Add(node);

        foreach (var resourceConfig in workspace.GetResourceConfigs())
        {
            _applyResourceConfig(workspace, resourceConfig);
        }
    }

    private void Remove(ClusterWorkspace workspace)
    {
        if (!_nodesByWorkspace.Remove(workspace, out var node))
        {
            return;
        }

        _unsubscribe(workspace);
        _workspacesByRuntime.Remove(workspace.Runtime);
        node.Dispose();
        _nodes.Remove(node);
    }
}
