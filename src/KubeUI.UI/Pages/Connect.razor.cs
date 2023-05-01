using k8s.Models;
using System.Diagnostics;

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
        KubernetesClientConfiguration.ExecStdError += KubernetesClientConfiguration_ExecStdError;
    }

    private async void KubernetesClientConfiguration_ExecStdError(object? sender, DataReceivedEventArgs e)
    {
        if (e?.Data != null)
        {
            Status += Environment.NewLine + e?.Data;
            await InvokeAsync(StateHasChanged);
        }
    }

    protected async override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Task.Run(ConnectToCluster);
        }
    }

    private void ClusterManager_OnChange(ClusterManagerEvents obj)
    {
        if (obj == ClusterManagerEvents.ActiveClusterChanged)
        {
            Status = null;
            ConnectToCluster();
        }
    }

    private async Task ConnectToCluster()
    {
        try
        {
            await ClusterManager.GetActiveCluster().Connect();

            await InvokeAsync(StateHasChanged);
            NavigationManager.NavigateTo("/");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to connect to cluster");
            Status = ex.Message;
            await InvokeAsync(StateHasChanged);
        }
    }

    public void Dispose()
    {
        ClusterManager.OnChange -= ClusterManager_OnChange;
        KubernetesClientConfiguration.ExecStdError -= KubernetesClientConfiguration_ExecStdError;
    }
}