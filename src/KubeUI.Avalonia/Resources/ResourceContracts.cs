namespace KubeUI.Avalonia.Resources;

public interface IResourceListColumn
{
    string Key { get; }
    string Name { get; }
    string? Width { get; }
    double MinWidth { get; }
    SortDirection Sort { get; set; }
    /// <summary>
    /// Gets custom control type, or <see langword="null"/> to render the column as text.
    /// </summary>
    Type? CustomControl { get; }
    Type ItemType { get; }
    Type ValueType { get; }
    IDataGridColumnValueAccessor ValueAccessor { get; }
    Func<object, IComparable?> SortKey { get; }
    Func<object, string> DisplayValue { get; }
}

public enum SortDirection
{
    None,
    Ascending,
    Descending
}
