namespace KubeUI.UI.Components;

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

    private GroupApiVersionKind GroupApiVersionKind => GroupApiVersionKind.From<TItem>();

    ICluster CurrentCluster;

    protected override void OnInitialized()
    {
        ClusterManager.OnChange += ClusterManager_OnChange;
        CurrentCluster = ClusterManager.GetActiveCluster()!;
        CurrentCluster.OnChange += CurrentCluster_OnChange;
        CurrentCluster.PropertyChanged += CurrentCluster_PropertyChanged;
    }

    private async void CurrentCluster_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "SelectedNamespaces")
        {
            await InvokeAsync(StateHasChanged);
        }
    }

    private async void CurrentCluster_OnChange(WatchEventType arg1, GroupApiVersionKind arg2, IKubernetesObject<V1ObjectMeta> arg3)
    {
        if (arg2 == GroupApiVersionKind && (arg1 == WatchEventType.Added || arg1 == WatchEventType.Deleted))
        {
            await InvokeAsync(StateHasChanged);
        }
    }

    private async void ClusterManager_OnChange(ClusterManagerEvents obj)
    {
        if (obj == ClusterManagerEvents.ActiveClusterChanged)
        {
            CurrentCluster.OnChange -= CurrentCluster_OnChange;
            CurrentCluster = ClusterManager.GetActiveCluster()!;
            CurrentCluster.OnChange += CurrentCluster_OnChange;

            await InvokeAsync(StateHasChanged);
        }
    }

    public void Dispose()
    {
        CurrentCluster.PropertyChanged -= CurrentCluster_PropertyChanged;
        CurrentCluster.OnChange -= CurrentCluster_OnChange;
        ClusterManager.OnChange -= ClusterManager_OnChange;
    }
}