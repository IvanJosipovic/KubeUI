using KubeUI.Avalonia.Features.Clusters.Overview.ViewModels;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.Presentation;
using System.ComponentModel;
using System.Linq;

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

#if DEBUG
        if (Design.IsDesignMode)
        {
            _ = InitializeDesignTimeDataContextAsync();
        }
#endif

        _timer.Interval = TimeSpan.FromSeconds(30);
        _timer.Tick += TimerOnTick;
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

    private async Task InitializeDesignTimeDataContextAsync()
    {
        if (Application.Current == null)
        {
            return;
        }

        var catalog = Application.Current.GetRequiredService<ClusterWorkspaceCatalog>();
        var cluster = catalog.GetDefault() ?? catalog.Clusters.FirstOrDefault();
        if (cluster == null)
        {
            return;
        }

        try
        {
            var vm = Application.Current.GetRequiredService<ClusterViewModel>();
            await Dispatcher.UIThread.InvokeAsync(() => DataContext = vm);

            _ = Task.Run(async () =>
            {
                try
                {
                    await cluster.Connect().ConfigureAwait(false);

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        if (vm is IInitializeCluster init)
                        {
                            init.Initialize(cluster);
                        }
                    });
                }
                catch
                {
                    // Design-time preview should fail closed; runtime still uses the normal app path.
                }
            });
        }
        catch
        {
            // Design-time preview should fail closed; runtime still uses the normal app path.
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
