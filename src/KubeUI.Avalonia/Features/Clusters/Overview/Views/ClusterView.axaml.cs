using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Overview.ViewModels;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;

namespace KubeUI.Avalonia.Features.Clusters.Overview.Views;

public sealed partial class ClusterView : UserControl
{
    private readonly DispatcherTimer _timer = new();
    public ClusterViewModel? ViewModel => DataContext as ClusterViewModel;

    public ClusterView()
    {
        InitializeComponent();

        DesignTimePreview.Run(InitializeDesignTimeDataAsync);
    }

    private async Task InitializeDesignTimeDataAsync()
    {
        DataContext = await DesignTimePreview.CreateClusterBoundViewModelAsync<ClusterViewModel, V1Pod>();
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



