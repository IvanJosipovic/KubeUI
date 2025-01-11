using AvaloniaEdit.Document;
using k8s.Models;
using k8s;
using KubeUI.Client;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using XtermSharp;
using System.IO;

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

    [ObservableProperty]
    public partial Terminal Terminal { get; set; } = new(null, new()
    {
        CursorBlink = true,
        CursorStyle = CursorStyle.SteadyBar,
    });

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
            const int bufferSize = 4096; // 4KB buffer size
            char[] buffer = new char[bufferSize];

            while (_stream.CanRead)
            {
                try
                {
                    await _streamReader.ReadAsync(buffer, 0, bufferSize);

                    //await _stream.ReadExactlyAsync(buffer, 0, bufferSize);

                    Terminal.Feed(new string(buffer));

                    var str = TerminalToString(Terminal);
                    await Dispatcher.UIThread.InvokeAsync(() => Console.Text = str, DispatcherPriority.Background);
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
        //_streamReader?.Dispose();
    }

    private static string TerminalToString(Terminal term)
    {
        var result = "";
        var lineText = "";
        for (var line = term.Buffer.YBase; line < term.Buffer.YBase + term.Rows; line++)
        {
            lineText = "";
            for (var cell = 0; cell < term.Cols; ++cell)
            {
                var cd = term.Buffer.Lines[line][cell];
                // (line).get (cell) [CHAR_DATA_CHAR_INDEX] || WHITESPACE_CELL_CHAR;
                if (cd.Code == 0)
                    lineText += " ";
                else
                    lineText += (char)cd.Rune;
            }
            // rtrim empty cells as xterm does
            lineText = lineText.TrimEnd();
            result += lineText;
            result += '\n';
        }
        return result;
    }
}
