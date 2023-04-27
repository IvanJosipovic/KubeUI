namespace KubeUI.UI.Pages;

public partial class Connect : IDisposable
{
    [Inject]
    private ClusterManager ClusterManager { get; set; }

    [Inject]
    private ILogger<Connect> Logger { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    private string? Status;

    protected override void OnInitialized()
    {
        ClusterManager.OnChange += ClusterManager_OnChange;
    }

    protected async override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await ConnectToCluster();
        }
    }

    private void ClusterManager_OnChange(ClusterManagerEvents obj)
    {
        if (obj == ClusterManagerEvents.ActiveClusterChanged)
        {
            Status = null;
            InvokeAsync(ConnectToCluster);
        }
    }

    private async Task ConnectToCluster()
    {
        try
        {
            await ClusterManager.GetActiveCluster().Connect();

            StateHasChanged();
            NavigationManager.NavigateTo("/");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to connect to cluster");
            Status = ex.Message;
            StateHasChanged();
        }
    }

    public void Dispose()
    {
        ClusterManager.OnChange -= ClusterManager_OnChange;
    }
}