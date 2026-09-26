namespace KubeUI.DynamicTableView;

/// <summary>Non-generic source contract consumed by <see cref="DynamicTableView"/>.</summary>
public interface IDynamicTableViewSource : IDisposable
{
    /// <summary>Gets the current, sorted and filtered rows.</summary>
    IEnumerable Items { get; }

    /// <summary>Gets runtime columns. Collection changes update the view.</summary>
    ObservableCollection<DynamicTableViewColumn> Columns { get; }

    /// <summary>Gets the row selection model.</summary>
    ISelectionModel SelectionModel { get; }

    /// <summary>Gets or sets all-column search text.</summary>
    string SearchText { get; set; }

    /// <summary>Gets the active sort descriptors in priority order.</summary>
    IReadOnlyList<DynamicTableViewSortDescriptor> SortDescriptors { get; }

    /// <summary>Gets active column filters keyed by column key.</summary>
    IReadOnlyList<DynamicTableViewFilterDescriptor> FilterDescriptors { get; }

    /// <summary>Raised when the source's columns, query, or descriptors change.</summary>
    event EventHandler? Changed;

    /// <summary>Sets or clears a keyed column filter.</summary>
    void SetFilter(DynamicTableViewFilterDescriptor? descriptor, string? columnKey = null);

    /// <summary>Clears all column filters while retaining external scope filters.</summary>
    void ClearFilters();

    /// <summary>Sets or replaces an external filter, such as a namespace scope.</summary>
    void SetScopeFilter(string key, Func<object, bool>? predicate);

    /// <summary>Sets the sort descriptors in priority order.</summary>
    void SetSort(IReadOnlyList<DynamicTableViewSortDescriptor> descriptors);

    /// <summary>Gets the stable identity key for a row.</summary>
    /// <summary>Returns whether two row objects represent the same selected row under the configured identity mode.</summary>
    bool AreSameRows(object? first, object? second);
}
