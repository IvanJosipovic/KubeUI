namespace KubeUI.DynamicTableView;

/// <summary>A value-based filter instruction keyed to a dynamic column.</summary>
public sealed record DynamicTableViewFilterDescriptor(
    string ColumnKey,
    DynamicTableViewFilterOperator Operator,
    object? Value = null,
    object? SecondValue = null,
    IReadOnlyList<object?>? Values = null,
    StringComparison StringComparison = StringComparison.OrdinalIgnoreCase);
