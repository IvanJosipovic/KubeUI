using k8s;
using k8s.Models;
using Kubernetes.Controller.Client;
using KubeUI.Client;

namespace KubeUI.Controls;

public sealed partial class EventLastSeenCell : UserControl, IInitializeCluster
{
    private static readonly DispatcherTimer s_timer = new(DispatcherPriority.Default);

    private ICluster? _cluster;

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
            var _date = ev.LastTimestamp ?? (ev.EventTime ?? ev!.Metadata!.CreationTimestamp!.Value);


            if (_date == default)
            {
                PrettyString = string.Empty;
                return;
            }

            var d = DateTime.UtcNow - _date;
            if (d.TotalMilliseconds <= 0)
                PrettyString = "0ms";
            else if (d.TotalDays >= 365)
                PrettyString = $"{(d.TotalDays / 365):N0}y";
            else if (d.TotalDays >= 1)
                PrettyString = $"{d.TotalDays:N0}d";
            else if (d.TotalHours >= 1)
                PrettyString = $"{d.TotalHours:N0}h";
            else if (d.TotalMinutes >= 1)
                PrettyString = $"{d.TotalMinutes:N0}m{d.Seconds:N0}s";
            else if (d.TotalSeconds >= 1)
                PrettyString = $"{d.TotalSeconds:N0}s";
            else
                PrettyString = $"{d.TotalMilliseconds:N0}ms";
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

    public void Initialize(ICluster cluster)
    {
        _cluster = cluster;
        _cluster.OnChange += _cluster_OnChange;
    }
}
