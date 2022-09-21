using Microsoft.AspNetCore.Components.Web;
using MudBlazor.Services;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using XtermBlazor;

namespace KubeUI.Core.Components;

public partial class PodConsole : IDisposable
{
    [Parameter]
    public string Namespace { get; set; }

    [Parameter]
    public string Name { get; set; }

    [Parameter]
    public string Container { get; set; }

    [Inject]
    protected ILogger<PodLogs> Logger { get; set; }

    [Inject]
    private ClusterManager ClusterManager { get; set; }

    [Inject]
    private IResizeListenerService ResizeListenerService { get; set; }

    private WebSocket WebSocket;

    private Stream Stream;

    private StreamReader streamReader;

    private bool IsDisposed;

    private Xterm _terminal;

    private TerminalOptions _options = new TerminalOptions
    {
        CursorBlink = true,
        CursorStyle = CursorStyle.Bar,
        Columns = 150,
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
        await PodExec();
    }

    public void Dispose()
    {
        IsDisposed = true;
        WebSocket?.Dispose();
        Stream?.Dispose();
        streamReader?.Dispose();
    }

    private async Task PodExec()
    {
        var cluster = (Cluster)ClusterManager.GetActiveCluster();

        string[] command = new string[]
        {
            "sh",
            "-c",
            "clear; (bash || ash || sh)"
        };

        WebSocket = await cluster.Client.WebSocketNamespacedPodExecAsync(Name, Namespace, command, Container).ConfigureAwait(false);

        var demux = new StreamDemuxer(WebSocket);
        demux.Start();

        Stream = demux.GetStream(ChannelIndex.StdOut, ChannelIndex.StdIn);

        using (streamReader = new StreamReader(Stream))
        {
            while (!IsDisposed)
            {
                try
                {
                    var memory = new Memory<char>(new char[1024]);
                    await streamReader.ReadAsync(memory);
                    var str = memory.ToString().Replace("\0", "");
                    await _terminal.Write(str);
                }
                catch (IOException ex) when (ex.Message.Equals("The request was aborted.")) { break; }
                catch (ObjectDisposedException) { break; }
            }
        }
    }

    private void KeyboardEventHandler(KeyboardEventArgs args)
    {
        if (Stream.CanWrite)
        {
            try
            {
                if (args.Key.Equals("Enter"))
                {
                    Stream.Write(Encoding.Default.GetBytes("\r"));
                }
                else if (args.Key.Equals("Backspace"))
                {
                    Stream.Write(Encoding.Default.GetBytes("\b"));
                }
                else if (args.Key.Equals("Tab"))
                {
                    Stream.Write(Encoding.Default.GetBytes("\t"));
                }
                else if (args.Key.Length == 1)
                {
                    Stream.Write(Encoding.Default.GetBytes(args.Key));
                }
            }
            catch (Exception ex) when (ex is IOException && ex is SocketException)
            {
                Logger.LogError(ex, "Error sending key to console");
            }
        }
    }

    private string[] _addonIds = new string[]
    {
        "xterm-addon-fit",
    };
}