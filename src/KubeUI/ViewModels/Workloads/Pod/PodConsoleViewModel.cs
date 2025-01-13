using AvaloniaEdit.Document;
using k8s.Models;
using k8s;
using KubeUI.Client;
using System.Net.WebSockets;
using System.Text;
using XtermSharp;
using AvaloniaEdit.Highlighting;
using Color = Avalonia.Media.Color;
using System.Text.Json;
using Avalonia.Media.TextFormatting;

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
    public partial Terminal Terminal { get; set; } = new();

    [ObservableProperty]
    public partial double Width { get; set; }

    [ObservableProperty]
    public partial double Height { get; set; }

    [ObservableProperty]
    public partial int FontSize { get; set; } = 14;

    [ObservableProperty]
    public partial int BufferLength { get; set; }

    [ObservableProperty]
    public partial string FontFamily { get; set; } = "Cascadia Mono";

    private WebSocket? _webSocket;
    private StreamDemuxer? _streamDemuxer;
    private Stream? _stream;
    private Stream? _refreshStream;

    /// <summary>
    /// Gets a value indicating whether or not the user can scroll the terminal contents
    /// </summary>
    public bool CanScroll
    {
        get
        {
            var shouldBeEnabled = !Terminal.Buffers.IsAlternateBuffer;
            shouldBeEnabled = shouldBeEnabled && Terminal.Buffer.HasScrollback;
            shouldBeEnabled = shouldBeEnabled && Terminal.Buffer.Lines.Length > Terminal.Rows;
            return shouldBeEnabled;
        }
    }

    /// <summary>
    /// Gets a value indicating the scroll thumbsize
    /// </summary>
    public float ScrollThumbsize
    {
        get
        {
            if (Terminal.Buffers.IsAlternateBuffer)
                return 0;

            // the thumb size is the proportion of the visible content of the
            // entire content but don't make it too small
            return Math.Max((float)Terminal.Rows / (float)Terminal.Buffer.Lines.Length, 0.01f);
        }
    }

    /// <summary>
    /// Gets a value indicating the relative position of the terminal scroller
    /// </summary>
    public double ScrollPosition
    {
        get
        {
            if (Terminal.Buffers.IsAlternateBuffer)
                return 0;

            // strictly speaking these ought not to be outside these bounds
            if (Terminal.Buffer.YDisp <= 0)
                return 0;

            var maxScrollback = Terminal.Buffer.Lines.Length - Terminal.Rows;
            if (Terminal.Buffer.YDisp >= maxScrollback)
                return 1;

            return (double)Terminal.Buffer.YDisp / (double)maxScrollback;
        }
    }

    public override void OnVisibleBoundsChanged(double x, double y, double width, double height)
    {
        base.OnVisibleBoundsChanged(x, y, width, height);

        Width = width;
        Height = height;

        const double toolHeaderHeight = 23;

        if (Width > 0 && Height > 0)
        {
            var size = CalculateTextSize("a", FontFamily, FontSize);

            var cols = (int)((width - 16) / size.Width);
            var rows = (int)((height - toolHeaderHeight) / (size.Height * 1.17));

            Terminal.Resize(cols, rows);
            Terminal.Delegate.SizeChanged(Terminal);
            SendResize();
            BufferLength = rows + Terminal.Options.Scrollback ?? 0;
        }
    }

    public static Size CalculateTextSize(string text, string fontName, int myFontSize)
    {
        var myFont = Avalonia.Media.FontFamily.Parse(fontName) ?? throw new ArgumentException($"The resource {fontName} is not a FontFamily.");

        var typeface = new Typeface(myFont);
        var shaped = TextShaper.Current.ShapeText(text, new TextShaperOptions(typeface.GlyphTypeface, myFontSize));
        var run = new ShapedTextRun(shaped, new GenericTextRunProperties(typeface, myFontSize));
        return run.Size;
    }

    public void SendResize()
    {
        var size = new TerminalSize
        {
            Width = (ushort)Terminal.Cols,
            Height = (ushort)Terminal.Rows,
        };

        if (_refreshStream?.CanWrite == true)
        {
            try
            {
                _refreshStream?.Write(JsonSerializer.SerializeToUtf8Bytes(size));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Sending Resize to console");
            }
        }
    }

    public async Task Connect()
    {
        var command = new string[]
        {
            "sh",
            "-c",
            "clear; (bash || ash || sh || echo 'No Shell Found!')",
        };

        _webSocket = await Cluster.Client.WebSocketNamespacedPodExecAsync(Object.Name(), Object.Namespace(), command, ContainerName);

        _streamDemuxer = new StreamDemuxer(_webSocket);
        _streamDemuxer.Start();

        _stream = _streamDemuxer.GetStream(ChannelIndex.StdOut, ChannelIndex.StdIn);
        _refreshStream = _streamDemuxer.GetStream(null, ChannelIndex.Resize);

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
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            UpdateTerminalText();
                            UpdateTerminalColors();
                        }, DispatcherPriority.Background);
                    }
                }
                catch (IOException ex) when (ex.Message.Equals("The request was aborted.")) { break; }
                catch (ObjectDisposedException) { break; }
            }
        });
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
        _refreshStream?.Dispose();
    }

    private void UpdateTerminalText()
    {
        var result = "";
        var lineText = "";
        for (var line = Terminal.Buffer.YBase; line < Terminal.Buffer.YBase + Terminal.Rows; line++)
        {
            lineText = "";
            for (var cell = 0; cell < Terminal.Cols; cell++)
            {
                var cd = Terminal.Buffer.Lines[line][cell];

                if (cd.Code == 0)
                    lineText += " ";
                else
                    lineText += (char)cd.Rune;
            }

            result += lineText;

            // All except last line
            if (line < Terminal.Buffer.YBase + Terminal.Rows - 1)
            {
                result += '\n';
            }
        }

        Console.Text = result;
    }

    private void UpdateTerminalColors()
    {
        for (var line = Terminal.Buffer.YBase; line < Terminal.Buffer.YBase + Terminal.Rows; line++)
        {
            for (var cell = 0; cell < Terminal.Cols; cell++)
            {
                var cd = Terminal.Buffer.Lines[line][cell];
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
                    hc.Foreground = new SimpleHighlightingBrush(ConvertXtermColor(fg));
                }
                else if (fg == 256) // DefaultColor
                {
                    hc.Foreground = new SimpleHighlightingBrush(ConvertXtermColor(15));
                }
                else if (fg == 257) // InvertedDefaultColor
                {
                    hc.Foreground = new SimpleHighlightingBrush(ConvertXtermColor(0));
                }

                if (bg <= 255)
                {
                    hc.Background = new SimpleHighlightingBrush(ConvertXtermColor(bg));
                }
                else if (bg == 256) // DefaultColor
                {
                    hc.Background = new SimpleHighlightingBrush(ConvertXtermColor(0));
                }
                else if (bg == 257) // InvertedDefaultColor
                {
                    hc.Background = new SimpleHighlightingBrush(ConvertXtermColor(15));
                }

                var append = line > 0 && line < Terminal.Buffer.YBase + Terminal.Rows ? 1 : 0;

                var offset = (line * (Terminal.Cols + append)) + cell;

                ConsoleColor.SetHighlighting(offset, 1, hc);
            }
        }
    }

    private static Color ConvertXtermColor(int xtermColor)
    {
        return xtermColor switch
        {
            0 => Color.FromRgb(0, 0, 0),
            1 => Color.FromRgb(128, 0, 0),
            2 => Color.FromRgb(0, 128, 0),
            3 => Color.FromRgb(128, 128, 0),
            4 => Color.FromRgb(0, 0, 128),
            5 => Color.FromRgb(128, 0, 128),
            6 => Color.FromRgb(0, 128, 128),
            7 => Color.FromRgb(192, 192, 192),
            8 => Color.FromRgb(128, 128, 128),
            9 => Color.FromRgb(255, 0, 0),
            10 => Color.FromRgb(0, 255, 0),
            11 => Color.FromRgb(255, 255, 0),
            12 => Color.FromRgb(0, 0, 255),
            13 => Color.FromRgb(255, 0, 255),
            14 => Color.FromRgb(0, 255, 255),
            15 => Color.FromRgb(255, 255, 255),
            16 => Color.FromRgb(0, 0, 0),
            17 => Color.FromRgb(0, 0, 95),
            18 => Color.FromRgb(0, 0, 135),
            19 => Color.FromRgb(0, 0, 175),
            20 => Color.FromRgb(0, 0, 215),
            21 => Color.FromRgb(0, 0, 255),
            22 => Color.FromRgb(0, 95, 0),
            23 => Color.FromRgb(0, 95, 95),
            24 => Color.FromRgb(0, 95, 135),
            25 => Color.FromRgb(0, 95, 175),
            26 => Color.FromRgb(0, 95, 215),
            27 => Color.FromRgb(0, 95, 255),
            28 => Color.FromRgb(0, 135, 0),
            29 => Color.FromRgb(0, 135, 95),
            30 => Color.FromRgb(0, 135, 135),
            31 => Color.FromRgb(0, 135, 175),
            32 => Color.FromRgb(0, 135, 215),
            33 => Color.FromRgb(0, 135, 255),
            34 => Color.FromRgb(0, 175, 0),
            35 => Color.FromRgb(0, 175, 95),
            36 => Color.FromRgb(0, 175, 135),
            37 => Color.FromRgb(0, 175, 175),
            38 => Color.FromRgb(0, 175, 215),
            39 => Color.FromRgb(0, 175, 255),
            40 => Color.FromRgb(0, 215, 0),
            41 => Color.FromRgb(0, 215, 95),
            42 => Color.FromRgb(0, 215, 135),
            43 => Color.FromRgb(0, 215, 175),
            44 => Color.FromRgb(0, 215, 215),
            45 => Color.FromRgb(0, 215, 255),
            46 => Color.FromRgb(0, 255, 0),
            47 => Color.FromRgb(0, 255, 95),
            48 => Color.FromRgb(0, 255, 135),
            49 => Color.FromRgb(0, 255, 175),
            50 => Color.FromRgb(0, 255, 215),
            51 => Color.FromRgb(0, 255, 255),
            52 => Color.FromRgb(95, 0, 0),
            53 => Color.FromRgb(95, 0, 95),
            54 => Color.FromRgb(95, 0, 135),
            55 => Color.FromRgb(95, 0, 175),
            56 => Color.FromRgb(95, 0, 215),
            57 => Color.FromRgb(95, 0, 255),
            58 => Color.FromRgb(95, 95, 0),
            59 => Color.FromRgb(95, 95, 95),
            60 => Color.FromRgb(95, 95, 135),
            61 => Color.FromRgb(95, 95, 175),
            62 => Color.FromRgb(95, 95, 215),
            63 => Color.FromRgb(95, 95, 255),
            64 => Color.FromRgb(95, 135, 0),
            65 => Color.FromRgb(95, 135, 95),
            66 => Color.FromRgb(95, 135, 135),
            67 => Color.FromRgb(95, 135, 175),
            68 => Color.FromRgb(95, 135, 215),
            69 => Color.FromRgb(95, 135, 255),
            70 => Color.FromRgb(95, 175, 0),
            71 => Color.FromRgb(95, 175, 95),
            72 => Color.FromRgb(95, 175, 135),
            73 => Color.FromRgb(95, 175, 175),
            74 => Color.FromRgb(95, 175, 215),
            75 => Color.FromRgb(95, 175, 255),
            76 => Color.FromRgb(95, 215, 0),
            77 => Color.FromRgb(95, 215, 95),
            78 => Color.FromRgb(95, 215, 135),
            79 => Color.FromRgb(95, 215, 175),
            80 => Color.FromRgb(95, 215, 215),
            81 => Color.FromRgb(95, 215, 255),
            82 => Color.FromRgb(95, 255, 0),
            83 => Color.FromRgb(95, 255, 95),
            84 => Color.FromRgb(95, 255, 135),
            85 => Color.FromRgb(95, 255, 175),
            86 => Color.FromRgb(95, 255, 215),
            87 => Color.FromRgb(95, 255, 255),
            88 => Color.FromRgb(135, 0, 0),
            89 => Color.FromRgb(135, 0, 95),
            90 => Color.FromRgb(135, 0, 135),
            91 => Color.FromRgb(135, 0, 175),
            92 => Color.FromRgb(135, 0, 215),
            93 => Color.FromRgb(135, 0, 255),
            94 => Color.FromRgb(135, 95, 0),
            95 => Color.FromRgb(135, 95, 95),
            96 => Color.FromRgb(135, 95, 135),
            97 => Color.FromRgb(135, 95, 175),
            98 => Color.FromRgb(135, 95, 215),
            99 => Color.FromRgb(135, 95, 255),
            100 => Color.FromRgb(135, 135, 0),
            101 => Color.FromRgb(135, 135, 95),
            102 => Color.FromRgb(135, 135, 135),
            103 => Color.FromRgb(135, 135, 175),
            104 => Color.FromRgb(135, 135, 215),
            105 => Color.FromRgb(135, 135, 255),
            106 => Color.FromRgb(135, 175, 0),
            107 => Color.FromRgb(135, 175, 95),
            108 => Color.FromRgb(135, 175, 135),
            109 => Color.FromRgb(135, 175, 175),
            110 => Color.FromRgb(135, 175, 215),
            111 => Color.FromRgb(135, 175, 255),
            112 => Color.FromRgb(135, 215, 0),
            113 => Color.FromRgb(135, 215, 95),
            114 => Color.FromRgb(135, 215, 135),
            115 => Color.FromRgb(135, 215, 175),
            116 => Color.FromRgb(135, 215, 215),
            117 => Color.FromRgb(135, 215, 255),
            118 => Color.FromRgb(135, 255, 0),
            119 => Color.FromRgb(135, 255, 95),
            120 => Color.FromRgb(135, 255, 135),
            121 => Color.FromRgb(135, 255, 175),
            122 => Color.FromRgb(135, 255, 215),
            123 => Color.FromRgb(135, 255, 255),
            124 => Color.FromRgb(175, 0, 0),
            125 => Color.FromRgb(175, 0, 95),
            126 => Color.FromRgb(175, 0, 135),
            127 => Color.FromRgb(175, 0, 175),
            128 => Color.FromRgb(175, 0, 215),
            129 => Color.FromRgb(175, 0, 255),
            130 => Color.FromRgb(175, 95, 0),
            131 => Color.FromRgb(175, 95, 95),
            132 => Color.FromRgb(175, 95, 135),
            133 => Color.FromRgb(175, 95, 175),
            134 => Color.FromRgb(175, 95, 215),
            135 => Color.FromRgb(175, 95, 255),
            136 => Color.FromRgb(175, 135, 0),
            137 => Color.FromRgb(175, 135, 95),
            138 => Color.FromRgb(175, 135, 135),
            139 => Color.FromRgb(175, 135, 175),
            140 => Color.FromRgb(175, 135, 215),
            141 => Color.FromRgb(175, 135, 255),
            142 => Color.FromRgb(175, 175, 0),
            143 => Color.FromRgb(175, 175, 95),
            144 => Color.FromRgb(175, 175, 135),
            145 => Color.FromRgb(175, 175, 175),
            146 => Color.FromRgb(175, 175, 215),
            147 => Color.FromRgb(175, 175, 255),
            148 => Color.FromRgb(175, 215, 0),
            149 => Color.FromRgb(175, 215, 95),
            150 => Color.FromRgb(175, 215, 135),
            151 => Color.FromRgb(175, 215, 175),
            152 => Color.FromRgb(175, 215, 215),
            153 => Color.FromRgb(175, 215, 255),
            154 => Color.FromRgb(175, 255, 0),
            155 => Color.FromRgb(175, 255, 95),
            156 => Color.FromRgb(175, 255, 135),
            157 => Color.FromRgb(175, 255, 175),
            158 => Color.FromRgb(175, 255, 215),
            159 => Color.FromRgb(175, 255, 255),
            160 => Color.FromRgb(215, 0, 0),
            161 => Color.FromRgb(215, 0, 95),
            162 => Color.FromRgb(215, 0, 135),
            163 => Color.FromRgb(215, 0, 175),
            164 => Color.FromRgb(215, 0, 215),
            165 => Color.FromRgb(215, 0, 255),
            166 => Color.FromRgb(215, 95, 0),
            167 => Color.FromRgb(215, 95, 95),
            168 => Color.FromRgb(215, 95, 135),
            169 => Color.FromRgb(215, 95, 175),
            170 => Color.FromRgb(215, 95, 215),
            171 => Color.FromRgb(215, 95, 255),
            172 => Color.FromRgb(215, 135, 0),
            173 => Color.FromRgb(215, 135, 95),
            174 => Color.FromRgb(215, 135, 135),
            175 => Color.FromRgb(215, 135, 175),
            176 => Color.FromRgb(215, 135, 215),
            177 => Color.FromRgb(215, 135, 255),
            178 => Color.FromRgb(215, 175, 0),
            179 => Color.FromRgb(215, 175, 95),
            180 => Color.FromRgb(215, 175, 135),
            181 => Color.FromRgb(215, 175, 175),
            182 => Color.FromRgb(215, 175, 215),
            183 => Color.FromRgb(215, 175, 255),
            184 => Color.FromRgb(215, 215, 0),
            185 => Color.FromRgb(215, 215, 95),
            186 => Color.FromRgb(215, 215, 135),
            187 => Color.FromRgb(215, 215, 175),
            188 => Color.FromRgb(215, 215, 215),
            189 => Color.FromRgb(215, 215, 255),
            190 => Color.FromRgb(215, 255, 0),
            191 => Color.FromRgb(215, 255, 95),
            192 => Color.FromRgb(215, 255, 135),
            193 => Color.FromRgb(215, 255, 175),
            194 => Color.FromRgb(215, 255, 215),
            195 => Color.FromRgb(215, 255, 255),
            196 => Color.FromRgb(255, 0, 0),
            197 => Color.FromRgb(255, 0, 95),
            198 => Color.FromRgb(255, 0, 135),
            199 => Color.FromRgb(255, 0, 175),
            200 => Color.FromRgb(255, 0, 215),
            201 => Color.FromRgb(255, 0, 255),
            202 => Color.FromRgb(255, 95, 0),
            203 => Color.FromRgb(255, 95, 95),
            204 => Color.FromRgb(255, 95, 135),
            205 => Color.FromRgb(255, 95, 175),
            206 => Color.FromRgb(255, 95, 215),
            207 => Color.FromRgb(255, 95, 255),
            208 => Color.FromRgb(255, 135, 0),
            209 => Color.FromRgb(255, 135, 95),
            210 => Color.FromRgb(255, 135, 135),
            211 => Color.FromRgb(255, 135, 175),
            212 => Color.FromRgb(255, 135, 215),
            213 => Color.FromRgb(255, 135, 255),
            214 => Color.FromRgb(255, 175, 0),
            215 => Color.FromRgb(255, 175, 95),
            216 => Color.FromRgb(255, 175, 135),
            217 => Color.FromRgb(255, 175, 175),
            218 => Color.FromRgb(255, 175, 215),
            219 => Color.FromRgb(255, 175, 255),
            220 => Color.FromRgb(255, 215, 0),
            221 => Color.FromRgb(255, 215, 95),
            222 => Color.FromRgb(255, 215, 135),
            223 => Color.FromRgb(255, 215, 175),
            224 => Color.FromRgb(255, 215, 215),
            225 => Color.FromRgb(255, 215, 255),
            226 => Color.FromRgb(255, 255, 0),
            227 => Color.FromRgb(255, 255, 95),
            228 => Color.FromRgb(255, 255, 135),
            229 => Color.FromRgb(255, 255, 175),
            230 => Color.FromRgb(255, 255, 215),
            231 => Color.FromRgb(255, 255, 255),
            232 => Color.FromRgb(8, 8, 8),
            233 => Color.FromRgb(18, 18, 18),
            234 => Color.FromRgb(28, 28, 28),
            235 => Color.FromRgb(38, 38, 38),
            236 => Color.FromRgb(48, 48, 48),
            237 => Color.FromRgb(58, 58, 58),
            238 => Color.FromRgb(68, 68, 68),
            239 => Color.FromRgb(78, 78, 78),
            240 => Color.FromRgb(88, 88, 88),
            241 => Color.FromRgb(98, 98, 98),
            242 => Color.FromRgb(108, 108, 108),
            243 => Color.FromRgb(118, 118, 118),
            244 => Color.FromRgb(128, 128, 128),
            245 => Color.FromRgb(138, 138, 138),
            246 => Color.FromRgb(148, 148, 148),
            247 => Color.FromRgb(158, 158, 158),
            248 => Color.FromRgb(168, 168, 168),
            249 => Color.FromRgb(178, 178, 178),
            250 => Color.FromRgb(188, 188, 188),
            251 => Color.FromRgb(198, 198, 198),
            252 => Color.FromRgb(208, 208, 208),
            253 => Color.FromRgb(218, 218, 218),
            254 => Color.FromRgb(228, 228, 228),
            255 => Color.FromRgb(238, 238, 238),
            _ => throw new ArgumentOutOfRangeException(nameof(xtermColor), "Color code must be between 0 and 255."),
        };
    }
}

public struct TerminalSize
{
    public ushort Width { get; set; }
    public ushort Height { get; set; }
}
