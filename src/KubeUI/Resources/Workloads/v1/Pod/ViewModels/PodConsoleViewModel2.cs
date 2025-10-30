using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Avalonia.Input.Platform;
using AvaloniaTerminal;
using k8s;
using k8s.Models;
using KubeUI.Client;

namespace KubeUI.ViewModels;

public sealed partial class PodConsoleViewModel2 : ViewModelBase, IDisposable
{
    private readonly ILogger<PodConsoleViewModel> _logger;

    private readonly ISettingsService _settingsService;

    public PodConsoleViewModel2(ILogger<PodConsoleViewModel> logger, ISettingsService settings)
    {
        _logger = logger;
        _settingsService = settings;
        Title = Assets.Resources.PodConsoleViewModel_Title;

        Model.UserInput += Input;
        Model.SizeChanged += Terminal_SizeChanged;
    }

    [ObservableProperty]
    public partial ICluster Cluster { get; set; }

    [ObservableProperty]
    public partial V1Pod Object { get; set; }

    [ObservableProperty]
    public partial string ContainerName { get; set; }

    [ObservableProperty]
    public partial double Width { get; set; }

    [ObservableProperty]
    public partial double Height { get; set; }

    [ObservableProperty]
    public partial string FontFamily { get; set; } = "Cascadia Mono";

    [ObservableProperty]
    public partial TerminalControlModel Model { get; set; } = new();

    private WebSocket? _webSocket;
    private StreamDemuxer? _streamDemuxer;
    private Stream? _stream;
    private Stream? _refreshStream;
    private Task? _runner;

    public async Task Connect()
    {
        if (_webSocket != null)
        {
            return;
        }

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

        SendResize(Model.Terminal.Cols, Model.Terminal.Rows);

        _runner = Task.Run(async () =>
        {
            while (_stream.CanRead)
            {
                try
                {
                    const int bufferSize = 4096; // 4KB buffer size
                    byte[] buffer = new byte[bufferSize];

                    if (await _stream.ReadAsync(buffer.AsMemory(0, bufferSize)) > 0)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            Model.Feed(buffer, bufferSize);
                        });
                    }
                }
                catch (Exception) { break; }
            }
        });
    }

    public void Input(byte[] input)
    {
        if (_stream.CanWrite)
        {
            _stream.Write(input);
        }
    }

    private void Terminal_SizeChanged(int cols, int rows, double width, double height)
    {
        SendResize(cols, rows);
    }

    public void SendResize(int cols, int rows)
    {
        var size = new TerminalSize
        {
            Width = (ushort)cols,
            Height = (ushort)rows,
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

    [RelayCommand]
    public async Task Paste()
    {
        if (_stream?.CanWrite == true)
        {
            try
            {
                var clipboard = App.TopLevel.Clipboard;

                var text = await clipboard.TryGetTextAsync();

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
}
