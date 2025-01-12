using AvaloniaEdit.Document;
using k8s.Models;
using k8s;
using KubeUI.Client;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using XtermSharp;

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
    });

    [ObservableProperty]
    public partial double Width { get; set; }

    [ObservableProperty]
    public partial double Height { get; set; }

    private WebSocket? _webSocket;
    private StreamDemuxer? _streamDemuxer;
    private Stream? _stream;

    public override void OnVisibleBoundsChanged(double x, double y, double width, double height)
    {
        base.OnVisibleBoundsChanged(x, y, width, height);

        Width = width;
        Height = height;

        const double toolHeaderHeight = 23;
        const double rowHeight = 19;
        const double colWidth = 7;

        if (Width > 0 && Height > 0)
        {
            Terminal.Resize((int)(width / colWidth), (int)((height - toolHeaderHeight)  / rowHeight));
            Terminal.Delegate.SizeChanged(Terminal);
            ReDraw();
        }
    }

    public async Task Connect()
    {
        var command = new string[]
        {
            "sh",
            "-c",
            "clear; (bash || ash || sh || echo 'No Shell Found!')",
            "env",
            "COLUMNS=120",
            "LINES=1000",
            "TERM=xterm"
        };

        _webSocket = await Cluster.Client.WebSocketNamespacedPodExecAsync(Object.Name(), Object.Namespace(), command, ContainerName);

        _streamDemuxer = new StreamDemuxer(_webSocket);
        _streamDemuxer.Start();

        _stream = _streamDemuxer.GetStream(ChannelIndex.StdOut, ChannelIndex.StdIn);

        _ = Task.Run(async () =>
        {
            while (_stream.CanRead)
            {
                try
                {
                    const int bufferSize = 4096; // 4KB buffer size
                    byte[] buffer = new byte[bufferSize];
                    if (await _stream.ReadAsync(buffer, 0, bufferSize) > 0)
                    {
                        Terminal.Feed(buffer, bufferSize);
                        ReDraw();
                    }
                }
                catch (IOException ex) when (ex.Message.Equals("The request was aborted.")) { break; }
                catch (ObjectDisposedException) { break; }
            }
        });
    }

    private void ReDraw()
    {
        Dispatcher.UIThread.Post(() => Console.Text = TerminalToString(Terminal), DispatcherPriority.Background);
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

    public void Send(byte bytes)
    {
        if (_stream?.CanWrite == true)
        {
            try
            {
                _stream.Write([bytes]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending text to console: ");
            }
        }
    }

    public void Send(byte[] bytes)
    {
        if (_stream?.CanWrite == true)
        {
            try
            {
                _stream.Write(bytes);
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
                    lineText += "";
                else
                    lineText += (char)cd.Rune;
            }
            // rtrim empty cells as xterm does
            //lineText = lineText.TrimEnd();
            result += lineText;
            if (line < term.Buffer.YBase + term.Rows - 1)
            {
                result += '\n';
            }
        }
        return result;
    }
}
