using System.Collections.Specialized;
using Avalonia.Controls.Selection;

namespace KubeUI.Avalonia.Infrastructure.DataGrid;

internal sealed class IdentityPreservingSelectionModel<T> : ISelectionModel, INotifyPropertyChanged, IDisposable where T : notnull
{
    private readonly SelectionModel<object?> _inner = new();
    private readonly Func<T, object?> _identitySelector;
    private readonly List<object> _selectionSnapshot = [];
    private readonly HashSet<object> _selectionIdentities = [];
    private readonly List<int> _restoredIndexes = [];
    private readonly Dictionary<T, object?> _identityCache = new();
    private INotifyCollectionChanged? _sourceNotifications;
    private IEnumerable? _identitySource;
    private INotifyCollectionChanged? _identitySourceNotifications;
    private bool _sourceMutationInProgress;
    private bool _suppressSnapshotUpdates;
    private int _sourceChangeVersion;

    public IdentityPreservingSelectionModel(Func<T, object?> identitySelector)
    {
        _identitySelector = identitySelector ?? throw new ArgumentNullException(nameof(identitySelector));

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

        if (_identitySourceNotifications is not null)
        {
            _identitySourceNotifications.CollectionChanged -= SourceOnCollectionChanged;
        }

        _sourceChangeVersion++;
        _sourceMutationInProgress = false;
        _identityCache.Clear();
        _identitySource = source;
        _identitySourceNotifications = source as INotifyCollectionChanged;
        if (_identitySourceNotifications is not null)
        {
            _identitySourceNotifications.CollectionChanged += SourceOnCollectionChanged;
        }

        ReconcileSelection();
    }

    public void Dispose()
    {
        DetachSourceNotifications();

        if (_identitySourceNotifications is not null)
        {
            _identitySourceNotifications.CollectionChanged -= SourceOnCollectionChanged;
            _identitySourceNotifications = null;
        }

        _identitySource = null;
        _identityCache.Clear();
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
        set => _inner.SelectedIndex = value;
    }

    public IReadOnlyList<int> SelectedIndexes => _inner.SelectedIndexes;

    public object? SelectedItem
    {
        get => _inner.SelectedItem;
        set => _inner.SelectedItem = value;
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
    }

    public void Deselect(int index)
    {
        _inner.Deselect(index);
    }

    public void SelectRange(int start, int end)
    {
        _inner.SelectRange(start, end);
    }

    public void DeselectRange(int start, int end)
    {
        _inner.DeselectRange(start, end);
    }

    public void SelectAll()
    {
        _inner.SelectAll();
    }

    public void Clear()
    {
        _inner.Clear();
    }

    private void InnerSelectionChanged(object? sender, SelectionModelSelectionChangedEventArgs e)
    {
        if (!_sourceMutationInProgress && !_suppressSnapshotUpdates)
        {
            UpdateSelectionSnapshot();
        }

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
        _identityCache.Clear();

        if (_selectionSnapshot.Count == 0)
        {
            return;
        }

        var snapshot = _selectionSnapshot.ToArray();
        var version = ++_sourceChangeVersion;
        _sourceMutationInProgress = true;

        Dispatcher.UIThread.Post(() =>
        {
            if (version != _sourceChangeVersion)
            {
                return;
            }

            _sourceMutationInProgress = false;
            RestoreSelectionSnapshot(snapshot);
        }, DispatcherPriority.Normal);
    }

    private void RestoreSelectionSnapshot(IReadOnlyList<object> snapshot)
    {
        if (snapshot.Count == 0 || Source is null)
        {
            return;
        }

        var indexes = FindIndexes(snapshot);
        if (SelectionMatchesIndexes(indexes))
        {
            UpdateSelectionSnapshot();
            return;
        }

        _suppressSnapshotUpdates = true;
        try
        {
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
        }
        finally
        {
            _suppressSnapshotUpdates = false;
        }

        UpdateSelectionSnapshot();
    }

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

        return true;
    }

    private List<int> FindIndexes(IReadOnlyList<object> snapshot)
    {
        if (snapshot.Count <= 4)
        {
            var indexes = new List<int>(snapshot.Count);
            var fastSource = _identitySource ?? Source;
            var fastSourceIndex = 0;
            foreach (var identity in snapshot)
            {
                foreach (var item in fastSource)
                {
                    if (item is not null && Equals(identity, GetIdentity(item)))
                    {
                        indexes.Add(fastSourceIndex);
                        break;
                    }

                    fastSourceIndex++;
                }

                fastSourceIndex = 0;
            }

            return indexes;
        }

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

                if (_selectionIdentities.Contains(GetIdentity(item)!))
                {
                    _restoredIndexes.Add(index);
                }
            }

            return _restoredIndexes;
        }

        var sourceIndex = 0;
        foreach (var item in source)
        {
            if (item is not null && _selectionIdentities.Contains(GetIdentity(item)!))
            {
                _restoredIndexes.Add(sourceIndex);
            }

            sourceIndex++;
        }

        return _restoredIndexes;
    }

    private object? GetIdentity(object? item)
    {
        if (item is not T typedItem)
        {
            return item;
        }

        if (_identityCache.TryGetValue(typedItem, out var identity))
        {
            return identity;
        }

        identity = _identitySelector(typedItem);
        _identityCache.Add(typedItem, identity);
        return identity;
    }

    private void UpdateSelectionSnapshot()
    {
        _selectionSnapshot.Clear();
        foreach (var selectedItem in _inner.SelectedItems)
        {
            var identity = GetIdentity(selectedItem);
            if (identity is not null)
            {
                _selectionSnapshot.Add(identity);
            }
        }
    }
}
