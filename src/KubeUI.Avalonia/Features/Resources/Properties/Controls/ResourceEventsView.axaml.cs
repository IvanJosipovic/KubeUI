using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Infrastructure.Threading;
using k8s;
using k8s.Models;
using DynamicData;
using KubernetesClient.Informer.Client;
using KubeUI.Kubernetes;
using KubeUI.Avalonia.Features.Resources.Properties.ViewModels;
using KubeUI.Avalonia;
using System.Reactive.Linq;

namespace KubeUI.Avalonia.Features.Resources.Properties.Controls;

public sealed partial class ResourceEventsView : UserControl, IInitializeCluster
{
    private readonly DispatcherTimer _timer = new(DispatcherPriority.Background);

    private ClusterWorkspaceViewModel? _cluster;
    private ISourceCache<Corev1Event, string>? _eventCache;
    private IDisposable? _eventCacheSubscription;

    private IKubernetesObject<V1ObjectMeta>? _resource;

    [GeneratedDirectProperty]
    public partial IReadOnlyList<ResourceEventItemViewModel> Items { get; set; } = [];

    [GeneratedDirectProperty]
    public partial bool HasItems { get; set; }

    public ResourceEventsView()
    {
        InitializeComponent();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += Timer_Tick;
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        _resource = DataContext as IKubernetesObject<V1ObjectMeta>;
        Refresh();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (!_timer.IsEnabled)
        {
            _timer.Start();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (_timer.IsEnabled)
        {
            _timer.Stop();
        }
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        DisposeEventSubscription();
    }

    public void Initialize(ClusterWorkspaceViewModel cluster)
    {
        DisposeEventSubscription();

        _cluster = cluster;
        _eventCache = _cluster.GetResourceSourceCache<Corev1Event>();
        _eventCacheSubscription = _eventCache.Connect()
            .ObserveOn(AvaloniaScheduler.Instance)
            .Subscribe(_ => Refresh());
        Refresh();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        Refresh();
    }

    private void Refresh()
    {
        if (_cluster == null || _resource == null || _eventCache == null)
        {
            Items = [];
            HasItems = false;
            return;
        }

        var items = ResourceEventsSelector.SelectRecentEvents(_eventCache.Items, _resource, DateTime.UtcNow);
        Items = items;
        HasItems = items.Length > 0;
    }

    private void DisposeEventSubscription()
    {
        _eventCacheSubscription?.Dispose();
        _eventCacheSubscription = null;
        _eventCache = null;
    }
}
