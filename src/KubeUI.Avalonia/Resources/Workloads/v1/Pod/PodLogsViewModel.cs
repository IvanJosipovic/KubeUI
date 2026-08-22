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

    internal const int MaxLogEntries = 10_000;

    private Stream? _stream;

    private StreamReader? _streamReader;

    private CancellationTokenSource? _connectionCancellation;

    public PodLogsViewModel(ILogger<PodLogsViewModel> logger)
    {
        _logger = logger;
        Title = Assets.Resources.PodLogsView_Title;
    }

    public async Task Connect()
    {
        Disconnect();

        var connectionCancellation = new CancellationTokenSource();
        _connectionCancellation = connectionCancellation;

        try
        {
            Logs.Text = string.Empty;

            Stream stream = await Cluster!.Runtime.Client!.CoreV1.ReadNamespacedPodLogAsync(Object.Name(), Object.Namespace(), container: ContainerName, tailLines: 100, previous: Previous, follow: true, pretty: true, timestamps: Timestamps, cancellationToken: connectionCancellation.Token);

            if (connectionCancellation.IsCancellationRequested)
            {
                stream.Dispose();
                return;
            }

            StreamReader streamReader = new(stream);
            _stream = stream;
            _streamReader = streamReader;

            _ = ReadLogsAsync(streamReader, connectionCancellation);
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

    private async Task ReadLogsAsync(StreamReader streamReader, CancellationTokenSource connectionCancellation)
    {
        try
        {
            while (!connectionCancellation.IsCancellationRequested)
            {
                var log = await streamReader.ReadLineAsync(connectionCancellation.Token);

                if (log == null)
                {
                    break;
                }

                if (!string.IsNullOrEmpty(log))
                {
                    await Dispatcher.UIThread.InvokeAsync(() => AppendLog(log), DispatcherPriority.Background);
                }
            }
        }
        catch (OperationCanceledException) when (connectionCancellation.IsCancellationRequested)
        {
        }
        catch (IOException ex) when (ex.Message.Equals("The request was aborted.", StringComparison.Ordinal))
        {
        }
    }

    internal void AppendLog(string log)
    {
        Logs.Insert(Logs.TextLength, log + Environment.NewLine);

        var excessLines = Logs.LineCount - (MaxLogEntries + 1);
        if (excessLines > 0)
        {
            var lastLineToRemove = Logs.GetLineByNumber(excessLines);
            Logs.Remove(0, lastLineToRemove.TotalLength);
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
        Disconnect();
    }

    private void Disconnect()
    {
        _connectionCancellation?.Cancel();
        _stream?.Dispose();
        _streamReader?.Dispose();
        _connectionCancellation?.Dispose();
        _connectionCancellation = null;
        _stream = null;
        _streamReader = null;
    }
}
