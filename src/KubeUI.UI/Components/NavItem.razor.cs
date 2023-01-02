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

    private System.Timers.Timer Timer { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        ClusterManager.OnChange += ClusterManager_OnChange;

        Timer = new System.Timers.Timer(TimeSpan.FromSeconds(1));
        Timer.Elapsed += Timer_Elapsed;
        Timer.Enabled = true;
        Timer.AutoReset = true;
    }

    private async void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        await InvokeAsync(StateHasChanged);
    }

    private void ClusterManager_OnChange(ClusterManagerEvents obj)
    {
        if (obj == ClusterManagerEvents.ActiveClusterChanged)
        {
            InvokeAsync(StateHasChanged);
        }
    }

    public void Dispose()
    {
        ClusterManager.OnChange -= ClusterManager_OnChange;
        Timer.Dispose();
    }
}