using System.Collections.ObjectModel;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Infrastructure.Threading;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Resources.Properties.Controls;

public sealed partial class ResourceEventsView : UserControl, IInitializeCluster
{
    private readonly DispatcherTimer _timer = new(DispatcherPriority.Background);
    private readonly ObservableCollection<ResourceEventItem> _items = [];
    private readonly ReadOnlyObservableCollection<ResourceEventItem> _readOnlyItems;
    private readonly ReadOnlyObservableCollection<Corev1Event> _emptyEvents = new([]);

    private ClusterWorkspaceViewModel? _cluster;
    private ISourceCache<Corev1Event, string>? _eventCache;
    private IDisposable? _eventCacheSubscription;
    private ReadOnlyObservableCollection<Corev1Event> _matchedEvents;
    private bool _isDetached;
    private bool _refreshPending;

    private IKubernetesObject<V1ObjectMeta>? _resource;

    [GeneratedDirectProperty]
    public partial IReadOnlyList<ResourceEventItem> Items { get; set; } = [];

    [GeneratedDirectProperty]
    public partial bool HasItems { get; set; }

    public ResourceEventsView()
    {
        InitializeComponent();
        _readOnlyItems = new ReadOnlyObservableCollection<ResourceEventItem>(_items);
        Items = _readOnlyItems;
        _matchedEvents = _emptyEvents;
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += Timer_Tick;
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        _resource = DataContext as IKubernetesObject<V1ObjectMeta>;
        RebuildEventSubscription();
        RequestRefresh();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isDetached = false;
        if (!_timer.IsEnabled)
        {
            _timer.Start();
        }

        RequestRefresh();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _isDetached = true;
        if (_timer.IsEnabled)
        {
            _timer.Stop();
        }

        DisposeEventSubscription();
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
        RequestRefresh();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        RequestRefresh();
    }

    private void RequestRefresh()
    {
        if (_isDetached || _refreshPending || VisualRoot == null)
        {
            return;
        }

        _refreshPending = true;
        Dispatcher.UIThread.Post(() =>
        {
            _refreshPending = false;
            Refresh();
        }, DispatcherPriority.Background);
    }

    private void Refresh()
    {
        if (_isDetached)
        {
            return;
        }

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
            .Subscribe(_ => RequestRefresh());
    }

    private void DisposeEventSubscription()
    {
        _eventCacheSubscription?.Dispose();
        _eventCacheSubscription = null;
        _matchedEvents = _emptyEvents;
        _refreshPending = false;
    }

    private void Clear()
    {
        if (_items.Count > 0)
        {
            _items.Clear();
        }

        HasItems = false;
    }

    private void UpdateItems(ResourceEventItem[] items)
    {
        _items.Clear();

        for (var index = 0; index < items.Length; index++)
        {
            _items.Add(items[index]);
        }

        HasItems = items.Length > 0;
    }
}
