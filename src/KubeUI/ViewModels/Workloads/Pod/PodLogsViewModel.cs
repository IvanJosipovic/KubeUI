using AvaloniaEdit.Document;
using k8s.Models;
using k8s;
using KubeUI.Client;

namespace KubeUI.ViewModels;

public sealed partial class PodLogsViewModel : ViewModelBase, IDisposable
{
    private readonly ILogger<PodLogsViewModel> _logger;

    [ObservableProperty]
    private ICluster _cluster;

    [ObservableProperty]
    private V1Pod _object;

    [ObservableProperty]
    private string _containerName;

    [ObservableProperty]
    private TextDocument _logs = new();

    [ObservableProperty]
    private bool _previous;

    [ObservableProperty]
    private bool _timestamps;

    [ObservableProperty]
    private bool _autoScrollToBottom = true;

    [ObservableProperty]
    private bool _wordWrap;

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
