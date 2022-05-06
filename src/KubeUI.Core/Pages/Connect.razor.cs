using Microsoft.AspNetCore.Components;
using KubeUI.Core.Client;
using Microsoft.Extensions.Logging;

namespace KubeUI.Core.Pages;

public partial class Connect : IDisposable
{
    [Inject]
    private ClusterManager ClusterManager { get; set; }

    [Inject]
    private ILogger<Connect> Logger { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    private string? ErrorMessage;

    private Timer? Timer;

    protected override async Task OnInitializedAsync()
    {
        await ConnectToCluster();
    }

    private async Task ConnectToCluster()
    {
        ErrorMessage = null;
        await InvokeAsync(StateHasChanged);

        try
        {
            var ver = await ClusterManager.GetActiveCluster().GetVersion();
            ClusterManager.GetActiveCluster().IsConnected = true;
            await InvokeAsync(StateHasChanged);
            NavigationManager.NavigateTo("/");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to connect to cluster");
            ErrorMessage = ex.ToString();
            await InvokeAsync(StateHasChanged);

            if (Timer == null)
            {
                Timer = new Timer(async (_) => await ConnectToCluster(), null, 5000, 5000);
            }
        }
    }

    public void Dispose()
    {
        Timer?.Dispose();
    }
}