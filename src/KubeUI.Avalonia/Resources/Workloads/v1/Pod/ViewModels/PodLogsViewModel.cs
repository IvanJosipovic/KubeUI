using AvaloniaEdit.Document;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod.ViewModels;

public sealed partial class PodLogsViewModel : ViewModelBase, IDisposable
{
    public ISettingsService SettingsService { get; }
    private readonly ILogger<PodLogsViewModel> _logger;

    [ObservableProperty]
    public partial ClusterWorkspaceViewModel Cluster { get; set; }

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

    public PodLogsViewModel(ILogger<PodLogsViewModel> logger, ISettingsService settingsService)
    {
        _logger = logger;
        SettingsService = settingsService;
        Title = Assets.Resources.PodLogsViewModel_Title;
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
        _isConnected = false;

        _stream?.Dispose();

        _streamReader?.Dispose();
    }
}



