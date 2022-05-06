using Microsoft.AspNetCore.Components;
using k8s;
using KubeUI.Core.Client;
using Microsoft.Extensions.Logging;

namespace KubeUI.Core.Components;

public partial class PodLogs : IDisposable
{
    [Parameter]
    public string Namespace { get; set; }

    [Parameter]
    public string Name { get; set; }

    [Parameter]
    public string Container { get; set; }

    [Parameter]
    public int Lines { get; set; }

    [Parameter]
    public bool Previous { get; set; }

    [Inject]
    protected ILogger<PodLogs> Logger { get; set; }

    [Inject]
    private ClusterManager ClusterManager { get; set; }

    private Stream stream;

    private string Logs;

    private Timer timer;

    protected override void OnParametersSet()
    {
        SetTimer();
    }

    public void Dispose()
    {
        timer?.Dispose();
        stream?.Dispose();
    }

    public void SetTimer()
    {
        timer?.Dispose();
        stream?.Dispose();
        stream = null;

        timer = new Timer(async (_) => await Update(), null, 0, 1000);
    }

    private async Task Update()
    {
        try
        {
            if (stream == null)
            {
                var cluster = (Cluster)ClusterManager.GetActiveCluster();
                stream = await cluster.Client.ReadNamespacedPodLogAsync(Name, Namespace, container: Container, tailLines: Lines, previous: Previous, follow: false, pretty: true);
            }
            using (var reader = new StreamReader(stream))
            {
                Logs = await reader.ReadToEndAsync();
            }

            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            stream = null;
            Logger.LogError(ex, "Error getting logs");
        }
    }
}