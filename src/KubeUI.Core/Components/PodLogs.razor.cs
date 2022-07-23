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

    private StreamReader streamReader;

    private string Logs;

    private bool IsDisposed;

    protected override async Task OnParametersSetAsync()
    {
        await Update();
    }

    public void Dispose()
    {
        IsDisposed = true;
        stream?.Dispose();
        streamReader?.Dispose();
    }

    private async Task Update()
    {
        try
        {
            var cluster = (Cluster)ClusterManager.GetActiveCluster();
            stream = await cluster.Client.CoreV1.ReadNamespacedPodLogAsync(Name, Namespace, container: Container, tailLines: Lines, previous: Previous, follow: true, pretty: true);

            using (streamReader = new StreamReader(stream))
            {
                while (!IsDisposed && streamReader.Peek() != -1)
                {
                    try
                    {
                        Logs += Environment.NewLine + await streamReader.ReadLineAsync();
                    }
                    catch (IOException ex) when (ex.Message.Equals("The request was aborted.")) { break; }
                    catch (ObjectDisposedException) { break; }

                    await InvokeAsync(StateHasChanged);
                }
            }
        }
        catch (Exception ex)
        {
            stream = null;
            Logger.LogError(ex, "Error getting logs");
        }
    }
}