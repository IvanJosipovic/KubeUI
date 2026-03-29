using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Resources.Core.v1.Event.Controls;

public sealed partial class EventLastSeenCell : UserControl, IInitializeCluster
{
    private static readonly DispatcherTimer s_timer = new(DispatcherPriority.Default);

    private ClusterWorkspaceViewModel? _cluster;

    private Corev1Event? _viewModel;

    [GeneratedDirectProperty]
    public partial string PrettyString { get; set; } = string.Empty;

    public EventLastSeenCell()
    {
        InitializeComponent();

        if (!s_timer.IsEnabled)
        {
            s_timer.Interval = TimeSpan.FromSeconds(1);
            s_timer.Start();
        }

#if DEBUG
        if (Design.IsDesignMode)
        {
            DataContext = new Corev1Event()
            {
                LastTimestamp = DateTime.UtcNow,
            };
        }
#endif
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        SetPrettyString();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        s_timer.Tick += Timer_Tick;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        s_timer.Tick -= Timer_Tick;
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        _cluster?.OnChange -= _cluster_OnChange;
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        SetPrettyString();
    }

    private void SetPrettyString()
    {
        if (DataContext is Corev1Event ev)
        {
            _viewModel = ev;
            var _date = EventTimeFormatter.ResolveTimestamp(ev);
            if (!_date.HasValue)
            {
                PrettyString = string.Empty;
                return;
            }

            PrettyString = EventTimeFormatter.FormatPrettyAge(_date.Value, DateTime.UtcNow);
        }
    }

    private void _cluster_OnChange(WatchEventType arg1, GroupApiVersionKind arg2, IKubernetesObject<V1ObjectMeta> arg3)
    {
        if (_viewModel?.Name() == arg3.Name() && _viewModel?.Namespace() == arg3.Namespace())
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                DataContext = arg3;
                SetPrettyString();
            }, DispatcherPriority.Normal);
        }
    }

    public void Initialize(ClusterWorkspaceViewModel cluster)
    {
        _cluster = cluster;
        _cluster.OnChange += _cluster_OnChange;
    }
}


