using System.Collections.Specialized;
using KubeUI.Kubernetes;
using Swordfish.NET.Collections;

namespace KubeUI.Avalonia.Features.Clusters.Workspace;

public sealed class ClusterWorkspaceComparer : IComparer<ClusterWorkspace>
{
    public int Compare(ClusterWorkspace? x, ClusterWorkspace? y)
    {
        return string.Compare(x?.Runtime.Name, y?.Runtime.Name, StringComparison.Ordinal);
    }
}

public sealed class ClusterWorkspaceCatalog : IDisposable
{
    private readonly IClusterRuntimeCatalog _runtimeCatalog;
    private readonly IServiceProvider _serviceProvider;

    public ObservableCollection<ClusterWorkspace> Clusters { get; } = new ObservableSortedCollection<ClusterWorkspace>(new ClusterWorkspaceComparer());

    public ClusterWorkspaceCatalog(IClusterRuntimeCatalog runtimeCatalog, IServiceProvider serviceProvider)
    {
        _runtimeCatalog = runtimeCatalog;
        _serviceProvider = serviceProvider;

        if (_runtimeCatalog.Clusters is INotifyCollectionChanged changed)
        {
            changed.CollectionChanged += RuntimeClustersChanged;
        }

        ReconcileWorkspaces();
    }

    public ClusterWorkspace? GetCluster(string name)
    {
        var runtime = _runtimeCatalog.GetCluster(name);
        return runtime == null ? null : GetOrCreate(runtime);
    }

    public ClusterWorkspace? GetDefault()
    {
        var runtime = _runtimeCatalog.GetDefault();
        return runtime == null ? null : GetOrCreate(runtime);
    }

    public void Dispose()
    {
        if (_runtimeCatalog.Clusters is INotifyCollectionChanged changed)
        {
            changed.CollectionChanged -= RuntimeClustersChanged;
        }

        foreach (var workspace in Clusters.ToList())
        {
            workspace.Dispose();
        }

        Clusters.Clear();
    }

    private void RuntimeClustersChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var action = e.Action;
        var newRuntimes = e.NewItems?.OfType<IClusterRuntime>().ToArray() ?? [];
        var oldRuntimes = e.OldItems?.OfType<IClusterRuntime>().ToArray() ?? [];

        Dispatcher.UIThread.Post(() => ApplyRuntimeClustersChanged(action, newRuntimes, oldRuntimes));
    }

    private void ApplyRuntimeClustersChanged(
        NotifyCollectionChangedAction action,
        IReadOnlyList<IClusterRuntime> newRuntimes,
        IReadOnlyList<IClusterRuntime> oldRuntimes)
    {
        switch (action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (var runtime in newRuntimes)
                {
                    GetOrCreate(runtime);
                }

                break;

            case NotifyCollectionChangedAction.Remove:
                foreach (var runtime in oldRuntimes)
                {
                    RemoveWorkspace(runtime);
                }

                break;

            case NotifyCollectionChangedAction.Replace:
                foreach (var runtime in oldRuntimes)
                {
                    RemoveWorkspace(runtime);
                }

                foreach (var runtime in newRuntimes)
                {
                    GetOrCreate(runtime);
                }

                break;

            case NotifyCollectionChangedAction.Reset:
                ReconcileWorkspaces();
                break;

            case NotifyCollectionChangedAction.Move:
                break;
        }
    }

    private void ReconcileWorkspaces()
    {
        var runtimes = _runtimeCatalog.Clusters.ToList();

        foreach (var workspace in Clusters
                     .Where(workspace => !runtimes.Any(runtime => ReferenceEquals(runtime, workspace.Runtime)))
                     .ToList())
        {
            RemoveWorkspace(workspace.Runtime);
        }

        foreach (var runtime in runtimes)
        {
            GetOrCreate(runtime);
        }
    }

    private ClusterWorkspace GetOrCreate(IClusterRuntime runtime)
    {
        var workspace = Clusters.FirstOrDefault(workspace => ReferenceEquals(workspace.Runtime, runtime));
        if (workspace != null)
        {
            return workspace;
        }

        workspace = ActivatorUtilities.CreateInstance<ClusterWorkspace>(_serviceProvider, runtime);
        Clusters.Add(workspace);
        return workspace;
    }

    private void RemoveWorkspace(IClusterRuntime runtime)
    {
        var workspace = Clusters.FirstOrDefault(workspace => ReferenceEquals(workspace.Runtime, runtime));
        if (workspace == null)
        {
            return;
        }

        Clusters.Remove(workspace);
        workspace.Dispose();
    }
}
