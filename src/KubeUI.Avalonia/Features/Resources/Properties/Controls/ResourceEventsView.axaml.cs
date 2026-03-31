using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Infrastructure.Threading;
using k8s;
using k8s.Models;
using DynamicData;
using DynamicData.Binding;
using KubeUI.Kubernetes;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace KubeUI.Avalonia.Features.Resources.Properties.Controls;

public sealed partial class ResourceEventsView : UserControl, IInitializeCluster
{
    private readonly DispatcherTimer _timer = new(DispatcherPriority.Background);
    private readonly ReadOnlyObservableCollection<Corev1Event> _emptyEvents = new([]);

    private ClusterWorkspaceViewModel? _cluster;
    private ISourceCache<Corev1Event, string>? _eventCache;
    private IDisposable? _eventCacheSubscription;
    private ReadOnlyObservableCollection<Corev1Event> _matchedEvents;

    private IKubernetesObject<V1ObjectMeta>? _resource;

    [GeneratedDirectProperty]
    public partial IReadOnlyList<ResourceEventItem> Items { get; set; } = [];

    [GeneratedDirectProperty]
    public partial bool HasItems { get; set; }

    public ResourceEventsView()
    {
        InitializeComponent();
        _matchedEvents = _emptyEvents;
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += Timer_Tick;
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        _resource = DataContext as IKubernetesObject<V1ObjectMeta>;
        RebuildEventSubscription();
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
_cluster = cluster;
try
{
_eventCache = cluster.GetResourceSourceCache<Corev1Event>();
}
catch (Exception)
{
_eventCache = null;
}

RebuildEventSubscription();
Refresh();
}

    private void Timer_Tick(object? sender, EventArgs e)
    {
        Refresh();
    }

    private void Refresh()
    {
        if (_resource == null)
        {
            Clear();
            return;
        }

        var items = _matchedEvents
            .Take(5)
            .Select(@event => ResourceEventsSelector.ToItem(@event, DateTime.UtcNow))
            .ToArray();

        UpdateItems(items);
    }

    private void RebuildEventSubscription()
    {
        DisposeEventSubscription();

        if (_resource == null || _eventCache == null)
        {
            _matchedEvents = _emptyEvents;
            return;
        }

        var resource = _resource;

        _eventCacheSubscription = _eventCache.Connect()
            .Filter(@event => ResourceEventsSelector.MatchesResource(@event, resource))
            .ObserveOn(AvaloniaScheduler.Instance)
            .SortAndBind(
                out _matchedEvents,
                SortExpressionComparer<Corev1Event>.Descending(@event => ResourceEventsSelector.GetSortTimestamp(@event)))
            .Subscribe(_ => Refresh());
    }

    private void DisposeEventSubscription()
    {
        _eventCacheSubscription?.Dispose();
        _eventCacheSubscription = null;
        _matchedEvents = _emptyEvents;
    }

    private void Clear()
    {
        Items = [];
        HasItems = false;
    }

    private void UpdateItems(ResourceEventItem[] items)
    {
        Items = items;
        HasItems = items.Length > 0;
    }
}
