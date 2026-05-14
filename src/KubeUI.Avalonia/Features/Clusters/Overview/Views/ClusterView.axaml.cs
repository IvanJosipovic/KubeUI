using System.ComponentModel;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Overview.ViewModels;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;

namespace KubeUI.Avalonia.Features.Clusters.Overview.Views;

public sealed partial class ClusterView : UserControl
{
    private readonly DispatcherTimer _timer = new(DispatcherPriority.Background);
    private INotifyPropertyChanged? _viewModelNotifications;
    private bool _isAttachedToVisualTree;

    public ClusterViewModel? ViewModel => DataContext as ClusterViewModel;

    public ClusterView()
    {
        InitializeComponent();

        DesignTimePreview.Run(InitializeDesignTimeDataAsync);

        _timer.Interval = TimeSpan.FromSeconds(30);
        _timer.Tick += TimerOnTick;
    }

    private async Task InitializeDesignTimeDataAsync()
    {
        DataContext = await DesignTimePreview.CreateClusterBoundViewModelAsync<ClusterViewModel, V1Pod>();
    }

    protected override async void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttachedToVisualTree = true;

        if (!_timer.IsEnabled)
        {
            _timer.Start();
        }

        AttachViewModelNotifications();
        if (ViewModel != null)
        {
            await ViewModel.RefreshData();
        }
    }
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _isAttachedToVisualTree = false;
        _timer.Stop();
        DetachViewModelNotifications();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        AttachViewModelNotifications();

        if (_isAttachedToVisualTree && ViewModel != null)
        {
            _ = ViewModel.RefreshData();
        }
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
            if (_isAttachedToVisualTree && ViewModel != null)
            {
                _ = ViewModel.RefreshData();
            }
        }
    }
}
