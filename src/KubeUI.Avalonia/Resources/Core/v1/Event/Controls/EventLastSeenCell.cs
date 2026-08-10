using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Infrastructure.Threading;

namespace KubeUI.Avalonia.Resources.Core.v1.Event.Controls;

public sealed partial class EventLastSeenCell : ViewBase<Corev1Event>, IInitializeCluster
{
    private readonly IUiRefreshClock _refreshClock;
    private IDisposable? _refreshSubscription;

    public ClusterWorkspace? Cluster { get; private set; }

    private Corev1Event? _viewModel => DataContext as Corev1Event;

    [GeneratedDirectProperty]
    public partial string PrettyString { get; set; } = string.Empty;

    public EventLastSeenCell(IUiRefreshClock refreshClock)
    {
        _refreshClock = refreshClock;
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

    protected override object Build(Corev1Event vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new TextBlock()
            .Margin(12, 0, 12, 0)
            .HorizontalAlignment(HorizontalAlignment.Left)
            .VerticalAlignment(VerticalAlignment.Center)
            .Text(this, x => x.PrettyString)
            .ToolTip_Tip(this, x => x.PrettyString);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        SetPrettyString();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _refreshSubscription = _refreshClock.Subscribe(SetPrettyString);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _refreshSubscription?.Dispose();
        _refreshSubscription = null;
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        Cluster?.Runtime.OnChange -= _cluster_OnChange;
    }

    private void SetPrettyString()
    {
        if (DataContext is Corev1Event ev)
        {
            var date = EventTimeFormatter.ResolveTimestamp(ev);
            if (!date.HasValue)
            {
                PrettyString = string.Empty;
                return;
            }

            PrettyString = EventTimeFormatter.FormatPrettyAge(date.Value, DateTime.UtcNow);
        }
        else
        {
            PrettyString = string.Empty;
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

    public void Initialize(ClusterWorkspace cluster)
    {
        Cluster = cluster;
        Cluster.Runtime.OnChange += _cluster_OnChange;
    }
}
