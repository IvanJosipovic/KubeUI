namespace KubeUI.UI.Components;

public partial class CRDNavList : IDisposable
{
    [Inject]
    private ClusterManager ClusterManager { get; set; }

    [Parameter]
    public bool Expanded { get; set; }

    [Parameter]
    public EventCallback<bool> ExpandedChanged { get; set; }

    public Dictionary<string, bool> ExpandedGroups { get; set; } = new();

    public Dictionary<string, bool> ExpandedGroups2 { get; set; } = new();

    ICluster CurrentCluster;

    GroupApiVersionKind crd = GroupApiVersionKind.From<V1CustomResourceDefinition>();

    private SemaphoreSlim throttleSemaphore = new(1);

    protected override void OnInitialized()
    {
        ClusterManager.OnChange += ClusterManager_OnChange;

        CurrentCluster = ClusterManager.GetActiveCluster()!;
        CurrentCluster.OnChange += ClusterObject_OnChange;
    }

    private async void ClusterObject_OnChange(WatchEventType arg1, GroupApiVersionKind arg2, IKubernetesObject<V1ObjectMeta> arg3)
    {
        if (arg2 == crd && (arg1 == WatchEventType.Added || arg1 == WatchEventType.Deleted) && await throttleSemaphore.WaitAsync(100))
        {
            await InvokeAsync(StateHasChanged);

            await Task.Delay(1000);

            throttleSemaphore.Release();
        }
    }

    private async void ClusterManager_OnChange(ClusterManagerEvents obj)
    {
        if (obj == ClusterManagerEvents.ActiveClusterChanged)
        {
            CurrentCluster.OnChange -= ClusterObject_OnChange;
            CurrentCluster = ClusterManager.GetActiveCluster()!;
            CurrentCluster.OnChange += ClusterObject_OnChange;
            await InvokeAsync(StateHasChanged);
        }
    }

    public void Dispose()
    {
        CurrentCluster.OnChange -= ClusterObject_OnChange;
        ClusterManager.OnChange -= ClusterManager_OnChange;
    }

    public static string GetSLD(string domain)
    {
        var parts = domain.Split('.');

        if (parts.Length < 2)
        {
            return domain;
        }

        return parts[^2] + "." + parts[^1];
    }
}