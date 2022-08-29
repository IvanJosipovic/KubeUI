namespace KubeUI.Core.Components;

public partial class NavItem<TItem> : IDisposable where TItem : class, IKubernetesObject<V1ObjectMeta>, new()
{
    [Parameter]
    public string Icon { get; set; }

    [Parameter]
    public string? Name { get; set; }

    [Inject]
    private ILogger<NavItem<TItem>> Logger { get; set; }

    [Inject]
    private ClusterManager ClusterManager { get; set; }

    private bool disposedValue;

    private GroupApiVersionKind GroupApiVersionKind => GroupApiVersionKind.From<TItem>();

    protected override void OnInitialized()
    {
        base.OnInitialized();

        ClusterManager.OnChange += ClusterManager_OnChange;
        ClusterManager.GetActiveCluster().OnChange += NavMenu_OnChange;
    }

    private void ClusterManager_OnChange(ClusterManagerEvents obj)
    {
        if (obj == ClusterManagerEvents.ActiveClusterChanged)
        {
            foreach (var cluster in ClusterManager.GetClusters())
            {
                cluster.OnChange -= NavMenu_OnChange;
            }

            ClusterManager.GetActiveCluster().OnChange += NavMenu_OnChange;
            InvokeAsync(StateHasChanged);
        }
    }

    private void NavMenu_OnChange(WatchEventType arg1, GroupApiVersionKind arg2, IKubernetesObject<V1ObjectMeta> arg3)
    {
        if (arg2.Equals(GroupApiVersionKind))
        {
            InvokeAsync(StateHasChanged);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                ClusterManager.OnChange -= ClusterManager_OnChange;
                ClusterManager.GetActiveCluster().OnChange -= NavMenu_OnChange;
            }

            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}