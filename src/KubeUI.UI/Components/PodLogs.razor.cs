using XtermBlazor;

namespace KubeUI.UI.Components;

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

    private bool IsDisposed;

    private Xterm _terminal;

    private TerminalOptions _options = new TerminalOptions
    {
        CursorBlink = true,
        CursorStyle = CursorStyle.Bar
    };

    private CancellationTokenSource token = new CancellationTokenSource();

    bool shouldClear;

    string FilterString;

    List<string> Logs = new();

    Stream Stream;

    StreamReader StreamReader;

    Task Task;

    protected override void OnInitialized()
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
        await Task.Delay(50);
        await _terminal.InvokeAddonFunctionVoidAsync("xterm-addon-fit", "fit");
    }

    protected override void OnParametersSet()
    {
        shouldClear = true;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender && shouldClear)
        {
            shouldClear = false;
            await Update();
        }
    }

    public void Dispose()
    {
        IsDisposed = true;
        token?.Cancel();
        Stream?.Dispose();
        StreamReader?.Dispose();
    }

    private async Task Update()
    {
        try
        {
            Logs.Clear();
            await _terminal.Clear();

            Stream?.Dispose();
            StreamReader?.Dispose();

            var cluster = (Cluster)ClusterManager.GetActiveCluster();
            Stream = await cluster.Client.CoreV1.ReadNamespacedPodLogAsync(Name, Namespace, container: Container, tailLines: Lines, previous: Previous, follow: true, pretty: true, cancellationToken: token.Token);

            StreamReader = new StreamReader(Stream);

            Task = Task.Run(async () =>
            {
                while (!IsDisposed)
                {
                    try
                    {
                        var log = await StreamReader.ReadLineAsync();

                        if (!string.IsNullOrEmpty(log))
                        {
                            await AddEntry(log);
                        }
                    }
                    catch (IOException ex) when (ex.Message.Equals("The request was aborted."))
                    {
                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        break;
                    }
                }
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting logs");
        }
    }

    static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

    private async Task AddEntry(string logEntry)
    {
        await semaphoreSlim.WaitAsync();

        if (string.IsNullOrEmpty(FilterString) || logEntry.Contains(FilterString))
        {
            await _terminal.WriteLine(logEntry);
            await InvokeAsync(StateHasChanged);
        }

        Logs.Add(logEntry);

        semaphoreSlim.Release();
    }

    private async Task UpdateFilter()
    {
        await semaphoreSlim.WaitAsync();

        await _terminal.Clear();

        foreach (var log in Logs)
        {
            if (string.IsNullOrEmpty(FilterString) || log.Contains(FilterString))
            {
                await _terminal.WriteLine(log);
            }
        }

        semaphoreSlim.Release();
    }

    private string[] _addonIds = new string[]
    {
        "xterm-addon-fit",
    };
}