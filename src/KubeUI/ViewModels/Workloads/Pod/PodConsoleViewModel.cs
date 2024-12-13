using AvaloniaEdit.Document;
using k8s.Models;
using k8s;
using KubeUI.Client;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;

namespace KubeUI.ViewModels;

public sealed partial class PodConsoleViewModel : ViewModelBase, IDisposable
{
    private readonly ILogger<PodConsoleViewModel> _logger;

    public PodConsoleViewModel(ILogger<PodConsoleViewModel> logger)
    {
        _logger = logger;
        Title = Resources.PodConsoleViewModel_Title;
    }

    [ObservableProperty]
    public partial ICluster Cluster { get; set; }

    [ObservableProperty]
    public partial V1Pod Object { get; set; }

    [ObservableProperty]
    public partial string ContainerName { get; set; }

    [ObservableProperty]
    public partial TextDocument Console { get; set; } = new();

    private WebSocket? _webSocket;
    private StreamDemuxer? _streamDemuxer;
    private Stream? _stream;
    private StreamReader? _streamReader;

    public async Task Connect()
    {
        var command = new string[]
        {
            "sh",
            "-c",
            "clear; (bash || ash || sh || echo 'No Shell Found!')"
        };

        _webSocket = await Cluster.Client.WebSocketNamespacedPodExecAsync(Object.Name(), Object.Namespace(), command, ContainerName);

        _streamDemuxer = new StreamDemuxer(_webSocket);
        _streamDemuxer.Start();

        _stream = _streamDemuxer.GetStream(ChannelIndex.StdOut, ChannelIndex.StdIn);

        _streamReader = new StreamReader(_stream);

        _ = Task.Run(async () =>
        {
            while (_stream.CanRead)
            {
                try
                {
                    var memory = new Memory<char>(new char[1024]);
                    await _streamReader.ReadAsync(memory).ConfigureAwait(false);
                    var str = memory.ToString()
                        .Replace("\0", "", StringComparison.Ordinal) // null character
                        .Replace("\a", "", StringComparison.Ordinal) // bell or alert
                        ;
                    str = RemoveAnsiEscapeSequences(str);
                    if (!string.IsNullOrEmpty(str))
                    {
                       await Dispatcher.UIThread.InvokeAsync(() =>
                       {
                           // backspace
                           if (str.Equals("\b", StringComparison.Ordinal) || str.Equals("\b \b", StringComparison.Ordinal))
                           {
                               Console.Remove(Console.TextLength - 1, 1);
                           }
                           else
                           {
                               Console.Insert(Console.TextLength, str);
                           }
                       }, DispatcherPriority.Background);
                    }
                }
                catch (IOException ex) when (ex.Message.Equals("The request was aborted.")) { break; }
                catch (ObjectDisposedException) { break; }
            }
        });
    }

    // ANSI escape sequences pattern
    [GeneratedRegex(@"\x1B\[[0-?]*[ -/]*[@-~]", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex AnsiEscape();

    public static string RemoveAnsiEscapeSequences(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return AnsiEscape().Replace(text, string.Empty);
    }

    public void Send(string text)
    {
        if (_stream?.CanWrite == true)
        {
            try
            {
                if (!string.IsNullOrEmpty(text))
                {
                    _stream.Write(Encoding.UTF8.GetBytes(text));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending text to console: ");
            }
        }
    }

    [RelayCommand]
    public async Task Paste()
    {
        if (_stream?.CanWrite == true)
        {
            try
            {
                var clipboard = App.TopLevel.Clipboard;

                var text = await clipboard.GetTextAsync();

                if (!string.IsNullOrEmpty(text))
                {
                    _stream.Write(Encoding.UTF8.GetBytes(text));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pasting text to console: ");
            }
        }
    }

    public void Dispose()
    {
        _webSocket?.Dispose();
        _streamDemuxer?.Dispose();
        _stream?.Dispose();
        _streamReader?.Dispose();
    }
}
