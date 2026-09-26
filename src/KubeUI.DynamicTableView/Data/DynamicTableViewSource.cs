using System.Collections.Specialized;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using DynamicData;

namespace KubeUI.DynamicTableView;

/// <summary>DynamicData-backed source that owns filtering, search, sorting, and selection.</summary>
public sealed class DynamicTableViewSource<T, TKey> : IDynamicTableViewSource
    where T : notnull
    where TKey : notnull
{
    private readonly Func<T, TKey> _keySelector;
    private readonly Func<T, object> _selectionIdentitySelector;
    private readonly IEqualityComparer<object> _selectionIdentityComparer;
    private readonly bool _useReplaceForUpdates;
    private readonly IScheduler _workerScheduler;
    private readonly IScheduler _uiScheduler;
    private readonly IScheduler _searchScheduler;
    private readonly TimeSpan _searchDebounce;
    private readonly Subject<string> _searchChanges = new();
    private readonly BehaviorSubject<Func<T, bool>> _filterSubject = new(static _ => true);
    private readonly BehaviorSubject<IComparer<T>> _sortSubject = new(Comparer<T>.Create(static (_, _) => 0));
    private readonly Dictionary<string, DynamicTableViewFilterDescriptor> _filters = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Func<T, bool>> _customFilters = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Func<T, bool>> _scopeFilters = new(StringComparer.Ordinal);
    private readonly IdentityPreservingSelectionModel<T, object> _selectionModel;
    private readonly IDisposable _searchSubscription;
    private readonly IDisposable _pipelineSubscription;
    private IReadOnlyList<DynamicTableViewSortDescriptor> _sortDescriptors = [];
    private ReadOnlyObservableCollection<T> _items;
    private string _searchText = string.Empty;
    private bool _disposed;

    /// <summary>Creates a source from DynamicData changesets and stable row keys.</summary>
    public DynamicTableViewSource(
        IObservable<IChangeSet<T, TKey>> changes,
        Func<T, TKey> keySelector,
        IEnumerable<DynamicTableViewColumn<T>>? columns = null,
        IScheduler? workerScheduler = null,
        IScheduler? uiScheduler = null,
        TimeSpan? searchDebounce = null,
        IScheduler? searchScheduler = null,
        DynamicTableViewSourceOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(changes);
        _keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
        options ??= new DynamicTableViewSourceOptions();
        _useReplaceForUpdates = options.UseReplaceForUpdates;
        _selectionIdentitySelector = options.SelectionIdentityMode switch
        {
            DynamicTableViewSelectionIdentityMode.Key => item => _keySelector(item)!,
            DynamicTableViewSelectionIdentityMode.Reference => static item => item,
            _ => throw new ArgumentOutOfRangeException(nameof(options), options.SelectionIdentityMode, "Unsupported selection identity mode.")
        };
        _sortSubject.OnNext(BuildComparer([]));
        _workerScheduler = workerScheduler ?? TaskPoolScheduler.Default;
        _uiScheduler = uiScheduler ?? new AvaloniaDispatcherScheduler();
        _searchScheduler = searchScheduler ?? _workerScheduler;
        _searchDebounce = searchDebounce ?? TimeSpan.FromMilliseconds(250);
        if (_searchDebounce < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(searchDebounce));
        _selectionIdentityComparer = options.SelectionIdentityMode == DynamicTableViewSelectionIdentityMode.Reference
            ? ReferenceEqualityComparer.Instance
            : EqualityComparer<object>.Default;
        _selectionModel = new IdentityPreservingSelectionModel<T, object>(
            _selectionIdentitySelector,
            _uiScheduler,
            _selectionIdentityComparer)
        {
            SingleSelect = false
        };

        Columns = [];
        if (columns is not null)
        {
            foreach (var column in columns)
            {
                Columns.Add(column);
            }
        }

        Columns.CollectionChanged += ColumnsOnCollectionChanged;
        _searchSubscription = _searchChanges
            .Select(query => _searchDebounce == TimeSpan.Zero || string.IsNullOrWhiteSpace(query)
                ? Observable.Return(query)
                : Observable.Timer(_searchDebounce, _searchScheduler).Select(_ => query))
            .Switch()
            .ObserveOn(_workerScheduler)
            .Subscribe(_ => UpdateFilterPredicate());

#pragma warning disable IL2091 // DynamicData 9.4.33 annotates SortAndBind<T> with All, but its implementation uses the supplied comparer and does not reflect over row members.
        _pipelineSubscription = changes
            .ObserveOn(_workerScheduler)
            .Filter(_filterSubject, _filterSubject.Select(static _ => Unit.Default))
            .SortAndBind(out _items, _sortSubject, new()
            {
                ResetOnFirstTimeLoad = true,
                UseReplaceForUpdates = _useReplaceForUpdates,
                Scheduler = _uiScheduler
            })
            .Subscribe(
                static _ => { },
                ex => SourceError?.Invoke(this, ex));
#pragma warning restore IL2091

        _selectionModel.SetIdentitySource(_items);
    }

    /// <inheritdoc />
    public IEnumerable Items => _items;

    /// <inheritdoc />
    public ObservableCollection<DynamicTableViewColumn> Columns { get; }

    /// <inheritdoc />
    public ISelectionModel SelectionModel => _selectionModel;

    /// <inheritdoc />
    public string SearchText
    {
        get => _searchText;
        set
        {
            ThrowIfDisposed();
            value ??= string.Empty;
            if (string.Equals(_searchText, value, StringComparison.Ordinal))
                return;

            _searchText = value;
            _searchChanges.OnNext(value);
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<DynamicTableViewSortDescriptor> SortDescriptors => _sortDescriptors;

    /// <inheritdoc />
    public IReadOnlyList<DynamicTableViewFilterDescriptor> FilterDescriptors
        => _filters.Values.ToArray();

    /// <summary>Raised when DynamicData source pipeline fails.</summary>
    public event EventHandler<Exception>? SourceError;

    /// <inheritdoc />
    public event EventHandler? Changed;

    /// <inheritdoc />
    public void SetFilter(DynamicTableViewFilterDescriptor? descriptor, string? columnKey = null)
    {
        ThrowIfDisposed();
        var key = descriptor?.ColumnKey ?? columnKey ?? throw new ArgumentException("Column key required when clearing a filter.", nameof(columnKey));
        if (descriptor is null)
            _filters.Remove(key);
        else
            _filters[key] = descriptor;
        UpdateFilterPredicate();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Sets or clears a custom predicate for one column.</summary>
    public void SetCustomFilter(string key, Func<object, bool>? predicate)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        if (predicate is null)
            _customFilters.Remove(key);
        else
            _customFilters[key] = item => predicate(item);
        UpdateFilterPredicate();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public void ClearFilters()
    {
        ThrowIfDisposed();
        if (_filters.Count == 0 && _customFilters.Count == 0)
            return;
        _filters.Clear();
        _customFilters.Clear();
        UpdateFilterPredicate();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public void SetScopeFilter(string key, Func<object, bool>? predicate)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        if (predicate is null)
            _scopeFilters.Remove(key);
        else
            _scopeFilters[key] = item => predicate(item);
        UpdateFilterPredicate();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public void SetSort(IReadOnlyList<DynamicTableViewSortDescriptor> descriptors)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(descriptors);
        _sortDescriptors = descriptors.ToArray();
        _sortSubject.OnNext(BuildComparer(_sortDescriptors));
        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public bool AreSameRows(object? first, object? second)
        => first is T typedFirst && second is T typedSecond &&
           _selectionIdentityComparer.Equals(_selectionIdentitySelector(typedFirst), _selectionIdentitySelector(typedSecond));

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        Columns.CollectionChanged -= ColumnsOnCollectionChanged;
        _searchSubscription.Dispose();
        _pipelineSubscription.Dispose();
        _searchChanges.Dispose();
        _filterSubject.Dispose();
        _sortSubject.Dispose();
        _selectionModel.Dispose();
        GC.SuppressFinalize(this);
    }

    private void ColumnsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateFilterPredicate();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateFilterPredicate()
    {
        var predicates = new List<Func<T, bool>>(_filters.Count + _customFilters.Count + _scopeFilters.Count + 1);
        foreach (var descriptor in _filters.Values)
        {
            var column = FindColumn(descriptor.ColumnKey);
            if (column is not null)
                predicates.Add(item => Matches(column.GetValue(item), descriptor));
        }
        foreach ((var key, var predicate) in _customFilters)
        {
            if (FindColumn(key) is not null)
                predicates.Add(predicate);
        }
        foreach (var predicate in _scopeFilters.Values)
            predicates.Add(predicate);

        var query = _searchText.Trim();
        if (query.Length > 0)
        {
            var searchableColumns = Columns.Where(static column => column.IsSearchable).ToArray();
            predicates.Add(item =>
            {
                for (var i = 0; i < searchableColumns.Length; i++)
                {
                    var text = searchableColumns[i].GetDisplayValue(item);
                    if (text.Contains(query, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                return false;
            });
        }

        var filter = predicates.Count switch
        {
            0 => static _ => true,
            1 => predicates[0],
            _ => item =>
            {
                for (var i = 0; i < predicates.Count; i++)
                {
                    if (!predicates[i](item))
                        return false;
                }
                return true;
            }
        };
        _filterSubject.OnNext(filter);
    }

    private IComparer<T> BuildComparer(IReadOnlyList<DynamicTableViewSortDescriptor> descriptors)
    {
        if (descriptors.Count == 0)
            return Comparer<T>.Create((left, right) => CompareValues(_keySelector(left), _keySelector(right)));
        var entries = new List<(DynamicTableViewColumn Column, ListSortDirection Direction)>(descriptors.Count);
        foreach (var descriptor in descriptors)
        {
            var column = FindColumn(descriptor.ColumnKey);
            if (column is not null)
                entries.Add((column, descriptor.Direction));
        }
        if (entries.Count == 0)
            return Comparer<T>.Create((left, right) => CompareValues(_keySelector(left), _keySelector(right)));

        return Comparer<T>.Create((left, right) =>
        {
            for (var i = 0; i < entries.Count; i++)
            {
                (var column, var direction) = entries[i];
                var result = CompareValues(column.GetValue(left), column.GetValue(right));
                if (result != 0)
                    return direction == ListSortDirection.Ascending ? result : -result;
            }
            return CompareValues(_keySelector(left), _keySelector(right));
        });
    }

    private DynamicTableViewColumn? FindColumn(string key)
    {
        foreach (var column in Columns)
        {
            if (string.Equals(column.Key, key, StringComparison.Ordinal))
                return column;
        }
        return null;
    }

    private static bool Matches(object? candidate, DynamicTableViewFilterDescriptor descriptor)
    {
        var value = descriptor.Value;
        switch (descriptor.Operator)
        {
            case DynamicTableViewFilterOperator.IsNull: return candidate is null;
            case DynamicTableViewFilterOperator.IsNotNull: return candidate is not null;
            case DynamicTableViewFilterOperator.IsTrue: return candidate is true;
            case DynamicTableViewFilterOperator.IsFalse: return candidate is false;
            case DynamicTableViewFilterOperator.Contains: return candidate is string contains && value is string query && contains.Contains(query, descriptor.StringComparison);
            case DynamicTableViewFilterOperator.DoesNotContain: return candidate is not string notContains || value is not string notQuery || !notContains.Contains(notQuery, descriptor.StringComparison);
            case DynamicTableViewFilterOperator.StartsWith: return candidate is string starts && value is string start && starts.StartsWith(start, descriptor.StringComparison);
            case DynamicTableViewFilterOperator.DoesNotStartWith: return candidate is not string notStarts || value is not string notStart || !notStarts.StartsWith(notStart, descriptor.StringComparison);
            case DynamicTableViewFilterOperator.EndsWith: return candidate is string ends && value is string end && ends.EndsWith(end, descriptor.StringComparison);
            case DynamicTableViewFilterOperator.DoesNotEndWith: return candidate is not string notEnds || value is not string notEnd || !notEnds.EndsWith(notEnd, descriptor.StringComparison);
            case DynamicTableViewFilterOperator.Equals: return CompareValues(candidate, value) == 0;
            case DynamicTableViewFilterOperator.NotEquals: return CompareValues(candidate, value) != 0;
            case DynamicTableViewFilterOperator.GreaterThan: return CompareValues(candidate, value) > 0;
            case DynamicTableViewFilterOperator.GreaterThanOrEqual: return CompareValues(candidate, value) >= 0;
            case DynamicTableViewFilterOperator.LessThan: return CompareValues(candidate, value) < 0;
            case DynamicTableViewFilterOperator.LessThanOrEqual: return CompareValues(candidate, value) <= 0;
            case DynamicTableViewFilterOperator.Between: return CompareValues(candidate, value) >= 0 && CompareValues(candidate, descriptor.SecondValue) <= 0;
            case DynamicTableViewFilterOperator.NotBetween: return CompareValues(candidate, value) < 0 || CompareValues(candidate, descriptor.SecondValue) > 0;
            case DynamicTableViewFilterOperator.In:
                var values = descriptor.Values ?? [];
                for (var i = 0; i < values.Count; i++)
                    if (CompareValues(candidate, values[i]) == 0) return true;
                return false;
            default: return true;
        }
    }

    private static int CompareValues(object? left, object? right)
    {
        if (ReferenceEquals(left, right)) return 0;
        if (left is null) return -1;
        if (right is null) return 1;
        if (left is DateTime leftDate && right is DateTimeOffset rightOffset)
            return new DateTimeOffset(leftDate.ToUniversalTime()).CompareTo(rightOffset);
        if (left is DateTimeOffset leftOffset && right is DateTime rightDate)
            return leftOffset.CompareTo(new DateTimeOffset(rightDate.ToUniversalTime()));
        if (IsNumber(left) && IsNumber(right))
            return Convert.ToDecimal(left, CultureInfo.InvariantCulture).CompareTo(Convert.ToDecimal(right, CultureInfo.InvariantCulture));
        if (left is IComparable comparable)
        {
            try { return comparable.CompareTo(right); }
            catch (ArgumentException) { }
        }
        return string.Compare(left.ToString(), right.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNumber(object value)
        => value is byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal;

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
