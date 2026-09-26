namespace KubeUI.DynamicTableView;

/// <summary>In-memory display state keyed to stable dynamic column keys.</summary>
public sealed record DynamicTableViewState(
    IReadOnlyList<DynamicTableViewColumnState> Columns,
    IReadOnlyList<DynamicTableViewSortDescriptor> Sorts,
    IReadOnlyList<DynamicTableViewFilterDescriptor> Filters,
    Vector ScrollOffset = default);
