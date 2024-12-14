using AvaloniaEdit.Document;
using k8s.Models;
using k8s;
using KubeUI.Client;
using AvaloniaEdit;
using System.Reflection;

namespace KubeUI.ViewModels;

public sealed partial class PodLogsViewModel : ViewModelBase, IDisposable
{
    private readonly ILogger<PodLogsViewModel> _logger;

    [ObservableProperty]
    public partial ICluster Cluster { get; set; }

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

    private readonly int _lines = 100;

    private bool _isConnected;

    public PodLogsViewModel(ILogger<PodLogsViewModel> logger)
    {
        _logger = logger;
        Title = Resources.PodLogsViewModel_Title;
    }

    public async Task Connect()
    {
        try
        {
            Logs.Text = string.Empty;

            _stream = await Cluster!.Client!.CoreV1.ReadNamespacedPodLogAsync(Object.Name(), Object.Namespace(), container: ContainerName, tailLines: _lines, previous: Previous, follow: true, pretty: true, timestamps: Timestamps);

            _streamReader = new StreamReader(_stream);

            _isConnected = true;

            _ = Task.Run(async () =>
            {
                while (_isConnected)
                {
                    try
                    {
                        var log = await _streamReader.ReadLineAsync();

                        if (!string.IsNullOrEmpty(log))
                        {
                            await Dispatcher.UIThread.InvokeAsync(() => Logs.Insert(Logs.TextLength, log + Environment.NewLine), DispatcherPriority.Background);
                        }
                    }
                    catch (IOException ex) when (ex.Message.Equals("The request was aborted."))
                    {
                        _isConnected = false;

                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        _isConnected = false;

                        break;
                    }
                }
            });
        }
        catch (Exception ex)
        {
            //to display notification
            _logger.LogError(ex, "Unable to View Logs");
        }
    }

    protected override async void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if(e.PropertyName?.Equals(nameof(Previous), StringComparison.Ordinal) == true || e.PropertyName?.Equals(nameof(Timestamps), StringComparison.Ordinal) == true)
        {
            await Connect();
        }
    }

    public void Dispose()
    {
        _isConnected = false;

        _stream?.Dispose();

        _streamReader?.Dispose();
    }
}
