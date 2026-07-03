using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using Avalonia.Markup.Declarative;
using DynamicData;
using DynamicData.Binding;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Converters;
using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Infrastructure.Threading;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Resources.Properties.Controls;

public sealed partial class ResourceEventsView : UserControl, IInitializeCluster
{
    private static readonly EventWarningForegroundConverter EventWarningForegroundConverter = new();
    private static readonly FuncValueConverter<bool, bool> NotConverter = new(value => !value);
    private readonly DispatcherTimer _timer = new(DispatcherPriority.Background);
    private static readonly IReadOnlyList<ResourceEventItem> EmptyItems = Array.Empty<ResourceEventItem>();
    private ClusterWorkspaceViewModel? _cluster;
    private ISourceCache<Corev1Event, string>? _eventCache;
    private IDisposable? _eventCacheSubscription;
    private readonly ReadOnlyObservableCollection<Corev1Event> _emptyEvents = new([]);
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
        Content = CreateContent();
        Items = EmptyItems;
        _matchedEvents = _emptyEvents;
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += Timer_Tick;
    }

    private ExpandableSection CreateContent()
    {
        return new ExpandableSection()
            .Header(Assets.Resources.ResourceEventsView_Title)
            .IsExpanded(true)
            .Content(
                new StackPanel()
                    .Margin(0, -4, 0, 0)
                    .Spacing(2)
                    .Children(
                        new ItemsControl()
                            .IsVisible(this, x => x.HasItems)
                            .ItemsSource(this, x => x.Items)
                            .ItemTemplate(new FuncDataTemplate<ResourceEventItem>((item, _) => CreateEventCard(item!))),
                        new PropertyItem()
                            .IsVisible(this, x => x.HasItems, BindingMode.OneWay, NotConverter)
                            .Value(Assets.Resources.ResourceEventsView_NoEventsFound)));
    }

    private static Border CreateEventCard(ResourceEventItem item)
    {
        return new Border()
            .Margin(0)
            .Padding(item.CardPadding)
            .Classes("card")
            .Child(
                new StackPanel()
                    .Spacing(2)
                    .Children(
                        new TextBlock()
                            .FontWeight(FontWeight.SemiBold)
                            .Foreground(item, x => x.IsWarning, BindingMode.OneWay, EventWarningForegroundConverter)
                            .IsVisible(item.HasMessage)
                            .Text(item.Message)
                            .TextWrapping(TextWrapping.Wrap),
                        new StackPanel()
                            .Spacing(0)
                            .Children(
                                new PropertyItem()
                                    .Key(Assets.Resources.Shared_Source)
                                    .Value(item.Source),
                                new PropertyItem()
                                    .Key(Assets.Resources.Shared_Count)
                                    .Value(item.Count),
                                new PropertyItem()
                                    .Key(Assets.Resources.ResourceEventsView_SubObject)
                                    .Value(item.SubObject),
                                new PropertyItem()
                                    .Key(Assets.Resources.ResourceEventsView_LastSeen)
                                    .Value(item.LastSeen))));
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
        if (_isDetached || VisualRoot == null)
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
        if (Items.Count > 0)
        {
            Items = EmptyItems;
        }

        HasItems = false;
    }

    private void UpdateItems(ResourceEventItem[] items)
    {
        if (_isDetached || VisualRoot == null)
        {
            return;
        }

        if (items.Length == 0)
        {
            Clear();
            return;
        }

        if (!Items.SequenceEqual(items))
        {
            Items = items;
        }

        HasItems = true;
    }
}
