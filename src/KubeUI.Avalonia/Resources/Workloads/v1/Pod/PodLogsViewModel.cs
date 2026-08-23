using AvaloniaEdit.Document;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.Presentation;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod;

public sealed partial class PodLogsViewModel : ViewModelBase, IDisposable
{
    private readonly ILogger<PodLogsViewModel> _logger;

    [ObservableProperty]
    public partial ClusterWorkspace Cluster { get; set; }

    [ObservableProperty]
    public partial V1Pod Object { get; set; }

    [ObservableProperty]
    public partial string ContainerName { get; set; }

    [ObservableProperty]
    public partial TextDocument Logs { get; set; } = new();

    [ObservableProperty]
    public partial bool Previous { get; set; }

    [ObservableProperty]
    public partial bool Timestamps { get; set; }

    [ObservableProperty]
    public partial bool AutoScrollToBottom { get; set; } = true;

    [ObservableProperty]
    public partial bool WordWrap { get; set; }

    [ObservableProperty]
    public partial Vector ScrollOffset { get; set; }

    private Stream? _stream;

    private StreamReader? _streamReader;

    private CancellationTokenSource? _connectionCancellation;

    private readonly int _lines = 100;

    public PodLogsViewModel(ILogger<PodLogsViewModel> logger)
    {
        _logger = logger;
        Title = Assets.Resources.PodLogsView_Title;
    }

    public async Task Connect()
    {
        CancellationTokenSource connectionCancellation = new();
        StopCurrentConnection();
        _connectionCancellation = connectionCancellation;

        try
        {
            Logs.Text = string.Empty;

            var stream = await Cluster!.Runtime.Client!.CoreV1.ReadNamespacedPodLogAsync(
                Object.Name(),
                Object.Namespace(),
                container: ContainerName,
                tailLines: _lines,
                previous: Previous,
                follow: true,
                pretty: true,
                timestamps: Timestamps,
                cancellationToken: connectionCancellation.Token);

            if (connectionCancellation.IsCancellationRequested)
            {
                stream.Dispose();
                return;
            }

            _stream = stream;
            var streamReader = new StreamReader(stream);
            _streamReader = streamReader;

            _ = ReadLogsAsync(streamReader, connectionCancellation.Token);
        }
        catch (OperationCanceledException) when (connectionCancellation.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            //todo display notification
            _logger.LogError(ex, "Unable to View Logs");
        }
    }

    protected override async void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName?.Equals(nameof(Previous), StringComparison.Ordinal) == true || e.PropertyName?.Equals(nameof(Timestamps), StringComparison.Ordinal) == true)
        {
            await Connect();
        }
    }

    [RelayCommand]
    public void Clear()
    {
        Logs.Text = string.Empty;
    }

    public void Dispose()
    {
        StopCurrentConnection();
    }

    internal Task ReadLogStreamForTesting(Stream stream)
    {
        return ReadLogsAsync(new StreamReader(stream), CancellationToken.None);
    }

    private async Task ReadLogsAsync(StreamReader streamReader, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var log = await streamReader.ReadLineAsync(cancellationToken);

                if (!string.IsNullOrEmpty(log))
                {
                    await Dispatcher.UIThread.InvokeAsync(
                        () => Logs.Insert(Logs.TextLength, log + Environment.NewLine),
                        DispatcherPriority.Background,
                        cancellationToken);
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex) when (cancellationToken.IsCancellationRequested && ex is IOException or ObjectDisposedException or InvalidOperationException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to read pod logs");
        }
        finally
        {
            streamReader.Dispose();
        }
    }

    private void StopCurrentConnection()
    {
        _connectionCancellation?.Cancel();
        _streamReader?.Dispose();
        _stream?.Dispose();
        _connectionCancellation?.Dispose();
        _connectionCancellation = null;
        _streamReader = null;
        _stream = null;
    }
}
