using AvaloniaEdit.Document;
using k8s.Models;
using k8s;
using KubeUI.Client;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using XtermSharp;
using AvaloniaEdit.Highlighting;
using Color = Avalonia.Media.Color;

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
    public partial RichTextModel ConsoleColor { get; set; } = new();

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
        const double colWidth = 7.7;

        if (Width > 0 && Height > 0)
        {
            Terminal.Resize((int)(width / colWidth), (int)((height - toolHeaderHeight)  / rowHeight));
            //Terminal.Delegate.SizeChanged(Terminal);
            ReDraw();
        }
    }

    public async Task Connect()
    {
        var command = new string[]
        {
            "env",
            "COLUMNS=" + Terminal.Cols,
            "TERM=xterm",
            "sh",
            "-c",
            "clear; (bash || ash || sh || echo 'No Shell Found!')",
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
        Dispatcher.UIThread.Post(() =>
        {
            Console.Text = TerminalToString(Terminal);
            //TerminalColors(Terminal);
        }, DispatcherPriority.Background);
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
            for (var cell = 0; cell < term.Cols; cell++)
            {
                var cd = term.Buffer.Lines[line][cell];

                if (cd.Code == 0)
                    lineText += " ";
                else
                    lineText += (char)cd.Rune;
            }

            result += lineText;

            // All except last line
            if (line < term.Buffer.YBase + term.Rows - 1)
            {
                result += '\n';
            }
        }
        return result;
    }

    private void TerminalColors(Terminal term)
    {
        for (var line = term.Buffer.YBase; line < term.Buffer.YBase + term.Rows; line++)
        {
            for (var cell = 0; cell < term.Cols; cell++)
            {
                var cd = term.Buffer.Lines[line][cell];
                var hc = new HighlightingColor();
                var attribute = cd.Attribute;

                // ((int)flags << 18) | (fg << 9) | bg;
                int bg = attribute & 0x1ff;
                int fg = (attribute >> 9) & 0x1ff;
                var flags = (FLAGS)(attribute >> 18);

                if (flags.HasFlag(FLAGS.INVERSE))
                {
                    var tmp = bg;
                    bg = fg;
                    fg = tmp;

                    if (fg == Renderer.DefaultColor)
                        fg = Renderer.InvertedDefaultColor;
                    if (bg == Renderer.DefaultColor)
                        bg = Renderer.InvertedDefaultColor;
                }
                if (flags.HasFlag(FLAGS.BOLD))
                {
                    hc.FontWeight = FontWeight.Bold;
                }
                if (flags.HasFlag(FLAGS.ITALIC))
                {
                    hc.FontStyle = FontStyle.Italic;
                }
                hc.Underline = flags.HasFlag(FLAGS.UNDERLINE);

                hc.Strikethrough = flags.HasFlag(FLAGS.CrossedOut);

                if (fg <= 255)
                {
                    hc.Foreground = new SimpleHighlightingBrush(ConvertAnsi256ToColor(fg));
                }

                if (bg <= 255)
                {
                    hc.Background = new SimpleHighlightingBrush(ConvertAnsi256ToColor(bg));
                }

                ConsoleColor.SetHighlighting((line * Terminal.Cols + 1) + cell, 1, hc);
            }
        }
    }

    public static Color ConvertAnsiToColor(int ansiCode)
    {
        // ANSI Color Table (8 colors + 8 bright variants)
        Color[] ansiColors =
        [
            Colors.Black,       // 0
            Colors.Red,         // 1
            Colors.Green,       // 2
            Colors.Yellow,      // 3
            Colors.Blue,        // 4
            Colors.Magenta,     // 5
            Colors.Cyan,        // 6
            Colors.White,       // 7
            Colors.DarkGray,    // 8 (Bright Black)
            Colors.LightCoral,  // 9 (Bright Red)
            Colors.LightGreen,  // 10 (Bright Green)
            Colors.LightYellow, // 11 (Bright Yellow)
            Colors.LightBlue,   // 12 (Bright Blue)
            Colors.Plum,        // 13 (Bright Magenta)
            Colors.LightCyan,   // 14 (Bright Cyan)
            Colors.WhiteSmoke   // 15 (Bright White)
        ];

        if (ansiCode >= 0 && ansiCode < ansiColors.Length)
        {
            return ansiColors[ansiCode];
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(ansiCode), "Invalid ANSI color code.");
        }
    }

    public static Color ConvertAnsi256ToColor(int ansiCode)
    {
        if (ansiCode < 0 || ansiCode > 255)
        {
            throw new ArgumentOutOfRangeException(nameof(ansiCode), "ANSI code must be between 0 and 255.");
        }

        if (ansiCode < 16)
        {
            // Standard ANSI colors (reuse the method above)
            return ConvertAnsiToColor(ansiCode);
        }
        else if (ansiCode >= 16 && ansiCode <= 231)
        {
            // 6x6x6 Color Cube
            int index = ansiCode - 16;
            int r = (index / 36) % 6 * 51; // Red component
            int g = (index / 6) % 6 * 51;  // Green component
            int b = index % 6 * 51;        // Blue component
            return Color.FromRgb(Convert.ToByte(r), Convert.ToByte(g), Convert.ToByte(b));
        }
        else
        {
            // Grayscale (232-255)
            int gray = (ansiCode - 232) * 10 + 8;
            var by = Convert.ToByte(gray);
            return Color.FromRgb(by, by, by);
        }
    }
}
