using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using Avalonia.Input.Platform;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Infrastructure.Platform;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Kubernetes;
using SvcSystems.UI.Terminal;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod.ViewModels;

public sealed partial class PodConsoleViewModel : ViewModelBase, IDisposable
{
    private readonly ILogger<PodConsoleViewModel> _logger;

    private readonly ISettingsService _settingsService;

    public PodConsoleViewModel(ILogger<PodConsoleViewModel> logger, ISettingsService settings)
    {
        _logger = logger;
        _settingsService = settings;
        Title = Assets.Resources.PodConsoleViewModel_Title;

        Model.UserInput += Input;
        Model.SizeChanged += Terminal_SizeChanged;
    }

    [ObservableProperty]
    public partial ClusterWorkspaceViewModel Cluster { get; set; }

    [ObservableProperty]
    public partial V1Pod Object { get; set; }

    [ObservableProperty]
    public partial string ContainerName { get; set; }

    [ObservableProperty]
    internal partial bool UseAttach { get; set; }

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
    private CancellationTokenSource? _readerCancellation;
    private bool _disconnected;

    internal bool IsDisconnected => _disconnected;

    public async Task ConnectAsync()
    {
        if (_webSocket != null || _disconnected)
        {
            return;
        }

        _webSocket = await OpenConnectionAsync().ConfigureAwait(false);

        _streamDemuxer = new StreamDemuxer(_webSocket);
        _streamDemuxer.Start();

        _stream = _streamDemuxer.GetStream(ChannelIndex.StdOut, ChannelIndex.StdIn);
        _refreshStream = _streamDemuxer.GetStream(null, ChannelIndex.Resize);

        SendResize(Model.Terminal.Cols, Model.Terminal.Rows);

        _readerCancellation = new CancellationTokenSource();
        _runner = Task.Run(() => ReadConsoleOutputAsync(_stream, _readerCancellation.Token));
    }

    internal void SetStreamsForTesting(Stream? stream = null, Stream? refreshStream = null)
    {
        _stream = stream;
        _refreshStream = refreshStream;
    }

    private async Task ReadConsoleOutputAsync(Stream consoleOutput, CancellationToken cancellationToken)
    {
        const int bufferSize = 4096;
        byte[] buffer = new byte[bufferSize];

        while (!cancellationToken.IsCancellationRequested && consoleOutput.CanRead)
        {
            try
            {
                int bytesRead = await consoleOutput.ReadAsync(buffer.AsMemory(0, bufferSize), cancellationToken).ConfigureAwait(false);

                if (bytesRead <= 0)
                {
                    Disconnect();
                    return;
                }

                cancellationToken.ThrowIfCancellationRequested();
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Model.Feed(buffer, bytesRead);
                });
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                Disconnect(ex, "reading console output");
                return;
            }
        }
    }

    internal Task<WebSocket> OpenConnectionAsync()
    {
        if (UseAttach)
        {
            return Cluster.Client!.WebSocketNamespacedPodAttachAsync(
                Object.Name(),
                Object.Namespace(),
                ContainerName,
                stderr: true,
                stdin: true,
                stdout: true,
                tty: true);
        }

        string[] command =
        [
            "sh",
            "-c",
            "clear; (bash || ash || sh || echo 'No Shell Found!')",
        ];

        return Cluster.Client!.WebSocketNamespacedPodExecAsync(Object.Name(), Object.Namespace(), command, ContainerName);
    }

    private void Input(object? sender, TerminalUserInputEventArgs args)
    {
        WriteInput(args.Data);
    }

    internal void WriteInput(ReadOnlyMemory<byte> data)
    {
        TryWrite(_stream, data.Span, "sending terminal input");
    }

    private void Terminal_SizeChanged(object? sender, TerminalSizeChangedEventArgs args)
    {
        SendResize(args.Cols, args.Rows);
    }

    public void SendResize(int cols, int rows)
    {
        var size = new TerminalSize((ushort)cols, (ushort)rows);

        TryWrite(_refreshStream, JsonSerializer.SerializeToUtf8Bytes(size), "sending terminal resize");
    }

    [RelayCommand]
    public async Task Paste()
    {
        var clipboard = TopLevelAccessor.GetRequired().Clipboard;

        var text = await clipboard.TryGetTextAsync();

        if (!string.IsNullOrEmpty(text))
        {
            TryWrite(_stream, Encoding.UTF8.GetBytes(text), "pasting text to console");
        }
    }

    private void TryWrite(Stream? stream, ReadOnlySpan<byte> data, string operation)
    {
        if (_disconnected || stream?.CanWrite != true)
        {
            return;
        }

        try
        {
            stream.Write(data);
        }
        catch (Exception ex) when (IsTerminalStreamClosed(ex))
        {
            Disconnect(ex, operation);
        }
    }

    private void Disconnect(Exception? ex = null, string? operation = null)
    {
        if (_disconnected)
        {
            return;
        }

        _disconnected = true;
        _readerCancellation?.Cancel();
        Model.UserInput -= Input;
        Model.SizeChanged -= Terminal_SizeChanged;

        var stream = _stream;
        var refreshStream = _refreshStream;
        var streamDemuxer = _streamDemuxer;
        var webSocket = _webSocket;
        var readerCancellation = _readerCancellation;
        var runner = _runner;

        _stream = null;
        _refreshStream = null;
        _streamDemuxer = null;
        _webSocket = null;
        _readerCancellation = null;
        _runner = null;

        stream?.Dispose();
        refreshStream?.Dispose();
        streamDemuxer?.Dispose();
        webSocket?.Dispose();
        DisposeWhenReaderCompletes(readerCancellation, runner);

        if (ex == null)
        {
            _logger.LogInformation("Pod console disconnected.");
            return;
        }

        _logger.LogWarning(ex, "Pod console disconnected while {Operation}.", operation ?? "processing console IO");
    }

    private static bool IsTerminalStreamClosed(Exception ex)
    {
        return ex is WebSocketException or IOException or ObjectDisposedException;
    }

    private static void DisposeWhenReaderCompletes(CancellationTokenSource? readerCancellation, Task? runner)
    {
        if (readerCancellation == null)
        {
            return;
        }

        if (runner?.IsCompleted != false)
        {
            readerCancellation.Dispose();
            return;
        }

        _ = runner.ContinueWith(
            static (_, state) => ((CancellationTokenSource)state!).Dispose(),
            readerCancellation,
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }

    public void Dispose()
    {
        Disconnect();
    }
}

public readonly record struct TerminalSize(ushort Width, ushort Height);
