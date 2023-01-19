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

    private System.Timers.Timer Timer { get; set; }

    protected override void OnInitialized()
    {
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
        Timer?.Dispose();
    }
}