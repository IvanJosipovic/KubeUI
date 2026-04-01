using KubeUI.Avalonia.Features.Clusters.Overview.ViewModels;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Clusters.Overview.Views;

public sealed partial class ClusterView : UserControl
{
    private readonly DispatcherTimer _timer = new();
    public ClusterViewModel? ViewModel => DataContext as ClusterViewModel;

    public ClusterView()
    {
        InitializeComponent();

#if DEBUG
        if (Design.IsDesignMode)
        {
            Dispatcher.UIThread.Post(async () =>
            {
                var cluster = Application.Current.GetRequiredService<ClusterWorkspaceCatalog>().GetDefault();
                await cluster.Connect();

                var vm = Application.Current.GetRequiredService<ClusterViewModel>();

                if (vm is IInitializeCluster init)
                {
                    init.Initialize(cluster);
                }

                DataContext = vm;
            });
        }
#endif
    }

    private async void TimerOnTick(object? sender, EventArgs e)
    {
        if (ViewModel != null)
        {
            try
            {
                await ViewModel.RefreshData();
            }
            catch
            {
                // Swallow refresh exceptions to keep timer going.
            }
        }
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (!_timer.IsEnabled)
        {
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += TimerOnTick;
            _timer.Start();
        }
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        _timer.Stop();
        _timer.Tick -= TimerOnTick;
    }
}



