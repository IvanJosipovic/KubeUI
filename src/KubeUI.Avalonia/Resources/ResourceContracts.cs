namespace KubeUI.Avalonia.Resources;

public interface IResourceListColumn
{
    string Key { get; }
    string Name { get; }
    string? Width { get; }
    double MinWidth { get; }
    SortDirection Sort { get; set; }
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
