using Microsoft.AspNetCore.Components.Web;
using System.IO;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;

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

    private WebSocket WebSocket;

    private Stream Stream;

    private StreamReader streamReader;

    private string Console;

    private bool IsDisposed;

    private string textbox;

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

        var command2 = "sh -c \"clear; (bash || ash || sh)\"";
        var command = "sh";

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
                    Console += str;
                }
                catch (IOException ex) when (ex.Message.Equals("The request was aborted.")) { break; }
                catch (ObjectDisposedException) { break; }

                await InvokeAsync(StateHasChanged);
            }
        }
    }

    private void KeyboardEventHandler(KeyboardEventArgs args)
    {
        if (Stream.CanWrite)
        {
            textbox = "";

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
}