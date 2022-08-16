using MudBlazor.Services;
using XtermBlazor;

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

    [Inject]
    private IResizeListenerService ResizeListenerService { get; set; }

    private Stream stream;

    private StreamReader streamReader;

    private bool IsDisposed;

    private Xterm _terminal;

    private TerminalOptions _options = new TerminalOptions
    {
        CursorBlink = true,
        CursorStyle = CursorStyle.Bar,
        //Columns = 150,
        Rows = 60,
        Theme =
        {
            //Background = "#17615e",
        },
    };

    protected override async Task OnInitializedAsync()
    {
        ResizeListenerService.OnResized += ResizeListenerService_OnResized;
    }

    private async void ResizeListenerService_OnResized(object? sender, BrowserWindowSize e)
    {
        try
        {
            await _terminal.InvokeAddonFunctionVoidAsync("xterm-addon-fit", "fit");
        }
        catch (Exception)
        {
        }
    }

    private async Task OnFirstRender()
    {
        await _terminal.InvokeAddonFunctionVoidAsync("xterm-addon-fit", "fit");
    }

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
                        await _terminal.WriteLine(await streamReader.ReadLineAsync());
                    }
                    catch (IOException ex) when (ex.Message.Equals("The request was aborted.")) { break; }
                    catch (ObjectDisposedException) { break; }
                }
            }
        }
        catch (Exception ex)
        {
            stream = null;
            Logger.LogError(ex, "Error getting logs");
        }
    }

    private string[] _addonIds = new string[]
    {
        "xterm-addon-fit",
    };
}