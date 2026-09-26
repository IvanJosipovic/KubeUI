namespace KubeUI.DynamicTableView;

/// <summary>A sort instruction keyed to a dynamic column.</summary>
public sealed record DynamicTableViewSortDescriptor(string ColumnKey, ListSortDirection Direction);
