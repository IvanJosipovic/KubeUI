using Avalonia.Threading;
using DynamicData;
using DynamicData.Binding;
using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;
using KubeUI.Avalonia.Infrastructure.Threading;
using k8s.Models;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace KubeUI.Avalonia.Features.Clusters.Overview.ViewModels;

public sealed partial class ClusterOverviewEventGridViewModel : ObservableObject, IDisposable
{
    private readonly Func<Corev1Event, bool> _filter;
    private readonly int _limit;
    private readonly ReadOnlyObservableCollection<Corev1Event> _emptyEvents = new([]);
    private ReadOnlyObservableCollection<Corev1Event> _matchedEvents;
    private ISourceCache<Corev1Event, string>? _eventCache;
    private IDisposable? _eventCacheSubscription;

    public ClusterOverviewEventGridViewModel(string title, string emptyText, Func<Corev1Event, bool> filter, int limit = 25)
    {
        Title = title;
        EmptyText = emptyText;
        _filter = filter;
        _limit = limit;
        _matchedEvents = _emptyEvents;
    }

    [ObservableProperty]
    public partial string Title { get; set; }

    [ObservableProperty]
    public partial string EmptyText { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<ClusterOverviewEventRow> Rows { get; set; } = [];

    [ObservableProperty]
    public partial bool HasRows { get; set; }

    public bool HasNoRows => !HasRows;

public void Initialize(ClusterWorkspaceViewModel cluster)
{
try
{
_eventCache = cluster.GetResourceSourceCache<Corev1Event>();
}
catch (Exception)
{
_eventCache = null;
}

RebuildSubscription();
RefreshRows();
}

    public void Dispose()
    {
        _eventCacheSubscription?.Dispose();
        _eventCacheSubscription = null;
        _matchedEvents = _emptyEvents;
        Rows = [];
        HasRows = false;
    }

    private void RebuildSubscription()
    {
        _eventCacheSubscription?.Dispose();
        _eventCacheSubscription = null;

        if (_eventCache == null)
        {
            _matchedEvents = _emptyEvents;
            return;
        }

        _eventCacheSubscription = _eventCache.Connect()
            .Filter(_filter)
            .ObserveOn(AvaloniaScheduler.Instance)
            .SortAndBind(
                out _matchedEvents,
                SortExpressionComparer<Corev1Event>.Descending(static @event => ResourceEventsSelector.GetSortTimestamp(@event)))
            .Subscribe(_ => RefreshRows());
    }

    private void RefreshRows()
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(RefreshRows, DispatcherPriority.Background);
            return;
        }

        var utcNow = DateTime.UtcNow;
        var rows = _matchedEvents
            .Take(_limit)
            .Select(@event => new ClusterOverviewEventRow(
                @event.Type ?? string.Empty,
                @event.Namespace() ?? string.Empty,
                @event.InvolvedObject?.Name ?? string.Empty,
                @event.Reason ?? string.Empty,
                string.IsNullOrWhiteSpace(@event.Message) ? (@event.Reason ?? string.Empty) : @event.Message.Trim(),
                FormatSource(@event),
                @event.Count ?? 0,
                EventTimeFormatter.FormatPrettyLastSeen(EventTimeFormatter.ResolveTimestamp(@event), utcNow),
                string.Equals(@event.Type, "Warning", StringComparison.Ordinal)))
            .ToArray();

        Rows = rows;
        HasRows = rows.Length > 0;
        OnPropertyChanged(nameof(HasNoRows));
    }

    private static string FormatSource(Corev1Event @event)
    {
        string? component = @event.Source?.Component ?? @event.ReportingComponent;
        string? host = @event.Source?.Host ?? @event.ReportingInstance;

        if (string.IsNullOrWhiteSpace(component))
        {
            return host ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(host))
        {
            return component;
        }

        return $"{component} {host}";
    }
}
