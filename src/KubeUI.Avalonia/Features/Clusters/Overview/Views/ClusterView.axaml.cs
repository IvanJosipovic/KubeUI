using KubeUI.Avalonia.Features.Clusters.Overview.ViewModels;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.Presentation;
using System.ComponentModel;

namespace KubeUI.Avalonia.Features.Clusters.Overview.Views;

public sealed partial class ClusterView : UserControl
{
    private readonly DispatcherTimer _timer = new(DispatcherPriority.Background);
    private MetricsControl? _metricsControl;
    private INotifyPropertyChanged? _viewModelNotifications;

    public ClusterViewModel? ViewModel => DataContext as ClusterViewModel;

    public ClusterView()
    {
        InitializeComponent();

#if DEBUG
        if (Design.IsDesignMode)
        {
            Dispatcher.UIThread.Post(async () =>
            {
                if (Application.Current == null)
                {
                    return;
                }

                var cluster = Application.Current.GetRequiredService<ClusterWorkspaceCatalog>().GetDefault();
                await cluster.Connect().ConfigureAwait(false);

                var vm = Application.Current.GetRequiredService<ClusterViewModel>();
                if (vm is IInitializeCluster init)
                {
                    init.Initialize(cluster);
                }

                DataContext = vm;
            });
        }
#endif

        _timer.Interval = TimeSpan.FromSeconds(30);
        _timer.Tick += TimerOnTick;
    }

    protected override async void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (!_timer.IsEnabled)
        {
            _timer.Start();
        }

        AttachViewModelNotifications();
        if (ViewModel != null)
        {
            await ViewModel.RefreshData();
        }

        _metricsControl ??= this.FindControl<MetricsControl>("ClusterMetricsControl");
        if (_metricsControl != null)
        {
            _metricsControl.ClusterOverviewMode = true;
        }

        TryInitializeMetricsControl();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _timer.Stop();
        DetachViewModelNotifications();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        AttachViewModelNotifications();
        TryInitializeMetricsControl();
    }

    private async void TimerOnTick(object? sender, EventArgs e)
    {
        if (ViewModel != null)
        {
            await ViewModel.RefreshData();
        }
    }

    private void AttachViewModelNotifications()
    {
        if (_viewModelNotifications == ViewModel)
        {
            return;
        }

        DetachViewModelNotifications();
        if (ViewModel is INotifyPropertyChanged propertyChanged)
        {
            _viewModelNotifications = propertyChanged;
            _viewModelNotifications.PropertyChanged += ViewModelOnPropertyChanged;
        }
    }

    private void DetachViewModelNotifications()
    {
        if (_viewModelNotifications != null)
        {
            _viewModelNotifications.PropertyChanged -= ViewModelOnPropertyChanged;
            _viewModelNotifications = null;
        }
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ClusterViewModel.Cluster))
        {
            TryInitializeMetricsControl();
        }
    }

    private void TryInitializeMetricsControl()
    {
        _metricsControl ??= this.FindControl<MetricsControl>("ClusterMetricsControl");
        if (_metricsControl != null && ViewModel?.Cluster != null)
        {
            _metricsControl.ClusterOverviewMode = true;
            _metricsControl.Initialize(ViewModel.Cluster);
        }
    }
}
