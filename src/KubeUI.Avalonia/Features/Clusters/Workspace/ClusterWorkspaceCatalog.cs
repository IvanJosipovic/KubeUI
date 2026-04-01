using System.Collections.Specialized;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Kubernetes;
using Swordfish.NET.Collections;

namespace KubeUI.Avalonia.Features.Clusters.Workspace;

public sealed class ClusterWorkspaceComparer : IComparer<ClusterWorkspaceViewModel>
{
    public int Compare(ClusterWorkspaceViewModel? x, ClusterWorkspaceViewModel? y)
    {
        return string.Compare(x?.Name, y?.Name, StringComparison.Ordinal);
    }
}

public sealed class ClusterWorkspaceCatalog : IDisposable
{
    private readonly IClusterRuntimeCatalog _runtimeCatalog;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<IClusterRuntime, ClusterWorkspaceViewModel> _workspaces = [];

    public ObservableCollection<ClusterWorkspaceViewModel> Clusters { get; } = new ObservableSortedCollection<ClusterWorkspaceViewModel>(new ClusterWorkspaceComparer());

    public ClusterWorkspaceCatalog(IClusterRuntimeCatalog runtimeCatalog, IServiceProvider serviceProvider)
    {
        _runtimeCatalog = runtimeCatalog;
        _serviceProvider = serviceProvider;

        if (_runtimeCatalog.Clusters is INotifyCollectionChanged changed)
        {
            changed.CollectionChanged += RuntimeClustersChanged;
        }

        ReloadWorkspaces();
    }

    public ClusterWorkspaceViewModel? GetCluster(string name)
    {
        var runtime = _runtimeCatalog.GetCluster(name);
        return runtime == null ? null : GetOrCreate(runtime);
    }

    public ClusterWorkspaceViewModel? GetDefault()
    {
        var runtime = _runtimeCatalog.GetDefault();
        return runtime == null ? null : GetOrCreate(runtime);
    }

    public void LoadFromConfigFromPath(string path)
    {
        _runtimeCatalog.LoadFromConfigFromPath(path);
    }

    public void RemoveCluster(ClusterWorkspaceViewModel cluster)
    {
        _runtimeCatalog.RemoveCluster(cluster.Runtime);
        RemoveWorkspace(cluster.Runtime);
    }

    public void Dispose()
    {
        if (_runtimeCatalog.Clusters is INotifyCollectionChanged changed)
        {
            changed.CollectionChanged -= RuntimeClustersChanged;
        }

        foreach (var workspace in _workspaces.Values)
        {
            workspace.Dispose();
        }

        _workspaces.Clear();
        Clusters.Clear();
    }

    private void RuntimeClustersChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Dispatcher.UIThread.Post(ReloadWorkspaces);
    }

    private void ReloadWorkspaces()
    {
        var runtimes = _runtimeCatalog.Clusters.ToList();

        foreach (var runtime in _workspaces.Keys.Where(runtime => !runtimes.Contains(runtime)).ToList())
        {
            RemoveWorkspace(runtime);
        }

        foreach (var runtime in runtimes)
        {
            var workspace = GetOrCreate(runtime);
            if (!Clusters.Contains(workspace))
            {
                Clusters.Add(workspace);
            }
        }
    }

    private ClusterWorkspaceViewModel GetOrCreate(IClusterRuntime runtime)
    {
        if (_workspaces.TryGetValue(runtime, out var workspace))
        {
            return workspace;
        }

        workspace = ActivatorUtilities.CreateInstance<ClusterWorkspaceViewModel>(_serviceProvider, runtime);
        _workspaces[runtime] = workspace;
        return workspace;
    }

    private void RemoveWorkspace(IClusterRuntime runtime)
    {
        if (!_workspaces.Remove(runtime, out var workspace))
        {
            return;
        }

        Clusters.Remove(workspace);
        workspace.Dispose();
    }
}

