using System.Collections.Specialized;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace KubeUI.DynamicTableView;

/// <summary>
/// Delegates selection operations and state to Avalonia's SelectionModel. Adds only stable-key restoration
/// around DynamicData view updates, and drops keys that are no longer visible.
/// </summary>
internal sealed class IdentityPreservingSelectionModel<T, TIdentity> : ISelectionModel, INotifyPropertyChanged, IDisposable
    where T : notnull
    where TIdentity : notnull
{
    private readonly SelectionModel<object?> _inner = new();
    private readonly Func<T, TIdentity> _identitySelector;
    private readonly IScheduler _uiScheduler;
    private readonly IEqualityComparer<TIdentity> _identityComparer;
    private readonly List<TIdentity> _selectionSnapshot = [];
    private readonly HashSet<TIdentity> _selectionIdentities;
    private readonly List<int> _restoredIndexes = [];
    private INotifyCollectionChanged? _sourceNotifications;
    private IEnumerable? _identitySource;
    private int _sourceChangeVersion;

    public IdentityPreservingSelectionModel(
        Func<T, TIdentity> identitySelector,
        IScheduler uiScheduler,
        IEqualityComparer<TIdentity>? identityComparer = null)
    {
        _identitySelector = identitySelector ?? throw new ArgumentNullException(nameof(identitySelector));
        _uiScheduler = uiScheduler ?? throw new ArgumentNullException(nameof(uiScheduler));
        _identityComparer = identityComparer ?? EqualityComparer<TIdentity>.Default;
        _selectionIdentities = new(_identityComparer);

        _inner.SelectionChanged += InnerSelectionChanged;
        _inner.IndexesChanged += (_, args) => IndexesChanged?.Invoke(this, args);
        _inner.LostSelection += (_, args) => LostSelection?.Invoke(this, args);
        _inner.SourceReset += (_, args) => SourceReset?.Invoke(this, args);
        _inner.PropertyChanged += (_, args) => PropertyChanged?.Invoke(this, args);
    }

    public IEnumerable Source
    {
        get => _inner.Source;
        set
        {
            if (ReferenceEquals(_inner.Source, value))
            {
                return;
            }

            DetachSourceNotifications();
            AttachSourceNotifications(value as INotifyCollectionChanged);
            _inner.Source = value;
            ReconcileSelection();
        }
    }

    public void SetIdentitySource(IEnumerable source)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (ReferenceEquals(_identitySource, source))
        {
            ReconcileSelection();
            return;
        }

        _sourceChangeVersion++;
        _identitySource = source;
        if (!ReferenceEquals(Source, source))
            Source = source;

        ReconcileSelection();
    }

    public void Dispose()
    {
        DetachSourceNotifications();

        _identitySource = null;
        _inner.SelectionChanged -= InnerSelectionChanged;
    }

    public bool SingleSelect
    {
        get => _inner.SingleSelect;
        set => _inner.SingleSelect = value;
    }

    public int SelectedIndex
    {
        get => _inner.SelectedIndex;
        set
        {
            if (value < 0)
            {
                Clear();
                return;
            }

            _inner.SelectedIndex = value;
            CaptureVisibleSelection();
        }
    }

    public IReadOnlyList<int> SelectedIndexes => _inner.SelectedIndexes;

    public object? SelectedItem
    {
        get => _inner.SelectedItem;
        set
        {
            if (value is null)
            {
                Clear();
                return;
            }

            _inner.SelectedItem = value;
            CaptureVisibleSelection();
        }
    }

    public IReadOnlyList<object?> SelectedItems => _inner.SelectedItems;

    public int AnchorIndex
    {
        get => _inner.AnchorIndex;
        set => _inner.AnchorIndex = value;
    }

    public int Count => _inner.Count;

    public event EventHandler<SelectionModelIndexesChangedEventArgs>? IndexesChanged;
    public event EventHandler<SelectionModelSelectionChangedEventArgs>? SelectionChanged;
    public event EventHandler? LostSelection;
    public event EventHandler? SourceReset;
    public event PropertyChangedEventHandler? PropertyChanged;

    public void BeginBatchUpdate()
    {
        _inner.BeginBatchUpdate();
    }

    public void EndBatchUpdate()
    {
        _inner.EndBatchUpdate();
    }

    public bool IsSelected(int index)
    {
        return _inner.IsSelected(index);
    }

    public void Select(int index)
    {
        _inner.Select(index);
        CaptureVisibleSelection();
    }

    public void Deselect(int index)
    {
        _inner.Deselect(index);
        CaptureVisibleSelection();
    }

    public void SelectRange(int start, int end)
    {
        _inner.SelectRange(start, end);
        CaptureVisibleSelection();
    }

    public void DeselectRange(int start, int end)
    {
        _inner.DeselectRange(start, end);
        CaptureVisibleSelection();
    }

    public void SelectAll()
    {
        _inner.SelectAll();
        CaptureVisibleSelection();
    }

    public void Clear()
    {
        _inner.Clear();
        _selectionSnapshot.Clear();
        _selectionIdentities.Clear();
    }

    private void InnerSelectionChanged(object? sender, SelectionModelSelectionChangedEventArgs e)
    {
        SelectionChanged?.Invoke(this, e);
    }

    private void AttachSourceNotifications(INotifyCollectionChanged? source)
    {
        _sourceNotifications = source;
        if (_sourceNotifications is not null)
        {
            _sourceNotifications.CollectionChanged += SourceOnCollectionChanged;
        }
    }

    private void DetachSourceNotifications()
    {
        if (_sourceNotifications is not null)
        {
            _sourceNotifications.CollectionChanged -= SourceOnCollectionChanged;
            _sourceNotifications = null;
        }
    }

    private void SourceOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_selectionSnapshot.Count == 0)
        {
            return;
        }

        var snapshot = _selectionSnapshot.ToArray();
        var version = ++_sourceChangeVersion;

        _uiScheduler.Schedule(snapshot, TimeSpan.Zero, (_, selectedIdentities) =>
        {
            if (version != _sourceChangeVersion)
            {
                return Disposable.Empty;
            }

            RestoreSelectionSnapshot(selectedIdentities);
            return Disposable.Empty;
        });
    }

    private void RestoreSelectionSnapshot(IReadOnlyList<TIdentity> snapshot)
    {
        if (snapshot.Count == 0 || Source is null)
        {
            return;
        }

        var indexes = FindIndexes(snapshot);
        if (indexes.Count == 0)
        {
            _inner.Clear();
            UpdateSelectionSnapshot();
            return;
        }

        if (SelectionMatchesIndexes(indexes))
        {
            UpdateSelectionSnapshot();
            return;
        }

        using (_inner.BatchUpdate())
        {
            _inner.Clear();
            if (indexes.Count > 0)
            {
                var selectedIndex = indexes.Min();
                _inner.SelectedIndex = selectedIndex;

                foreach (var index in indexes)
                {
                    if (index != selectedIndex)
                    {
                        _inner.Select(index);
                    }
                }
            }
        }

        UpdateSelectionSnapshot();
    }

    private void CaptureVisibleSelection()
        => UpdateSelectionSnapshot(_selectionSnapshot.ToArray());

    private void ReconcileSelection()
    {
        if (_selectionSnapshot.Count == 0)
        {
            UpdateSelectionSnapshot();
            return;
        }

        RestoreSelectionSnapshot(_selectionSnapshot.ToArray());
    }

    private bool SelectionMatchesIndexes(IReadOnlyList<int> indexes)
    {
        if (_inner.SelectedIndexes.Count != indexes.Count)
        {
            return false;
        }

        for (var i = 0; i < indexes.Count; i++)
        {
            if (_inner.SelectedIndexes[i] != indexes[i])
            {
                return false;
            }
        }

        var source = _identitySource ?? Source;
        if (source is not IList sourceList)
        {
            return false;
        }

        _selectionIdentities.Clear();
        foreach (var index in indexes)
        {
            if (index < 0 || index >= sourceList.Count || sourceList[index] is not T item)
            {
                return false;
            }

            _selectionIdentities.Add(GetIdentity(item));
        }

        foreach (var selectedItem in _inner.SelectedItems)
        {
            if (!TryGetIdentity(selectedItem, out var identity) || !_selectionIdentities.Remove(identity))
            {
                return false;
            }
        }

        return _selectionIdentities.Count == 0;
    }

    private List<int> FindIndexes(IReadOnlyList<TIdentity> snapshot)
    {
        _selectionIdentities.Clear();
        foreach (var identity in snapshot)
        {
            _selectionIdentities.Add(identity);
        }

        _restoredIndexes.Clear();
        var source = _identitySource ?? Source;

        if (source is IList list)
        {
            for (var index = 0; index < list.Count; index++)
            {
                if (list[index] is not { } item)
                {
                    continue;
                }

                if (item is T typedItem && _selectionIdentities.Contains(GetIdentity(typedItem)))
                {
                    _restoredIndexes.Add(index);
                }
            }

            return _restoredIndexes;
        }

        var sourceIndex = 0;
        foreach (var item in source)
        {
            if (item is T typedItem && _selectionIdentities.Contains(GetIdentity(typedItem)))
            {
                _restoredIndexes.Add(sourceIndex);
            }

            sourceIndex++;
        }

        return _restoredIndexes;
    }

    private bool TryGetIdentity(object? item, out TIdentity identity)
    {
        if (item is not T typedItem)
        {
            identity = default!;
            return false;
        }

        identity = _identitySelector(typedItem);
        return true;
    }

    private TIdentity GetIdentity(T typedItem)
        => _identitySelector(typedItem);

    private void UpdateSelectionSnapshot(IReadOnlyList<TIdentity>? previousSnapshot = null)
    {
        _selectionSnapshot.Clear();
        _selectionIdentities.Clear();

        if (previousSnapshot is not null)
        {
            foreach (var identity in previousSnapshot)
                _selectionIdentities.Add(identity);

            foreach (var item in Source)
            {
                if (item is T typedItem)
                    _selectionIdentities.Remove(GetIdentity(typedItem));
            }

            foreach (var identity in previousSnapshot)
            {
                if (_selectionIdentities.Contains(identity))
                    _selectionSnapshot.Add(identity);
            }
        }

        foreach (var selectedItem in _inner.SelectedItems)
        {
            if (TryGetIdentity(selectedItem, out var identity))
            {
                if (_selectionIdentities.Add(identity))
                    _selectionSnapshot.Add(identity);
            }
        }
    }
}
