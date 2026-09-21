using KubeUI.Avalonia.Infrastructure.DataGrid;
using KubeUI.Avalonia.Resources;

namespace KubeUI.Avalonia.Infrastructure.DataGrid;

/// <summary>Describes a typed, read-only DataGrid column backed by a value accessor.</summary>
public class DataGridValueColumn<T, TValue> : IResourceListColumn where T : class
{
    private const string NullableValueMissingMessage = "Nullable object must have a value.";
    private Func<T, TValue>? _fieldAccessor;
    private IDataGridColumnValueAccessor? _valueAccessor;

    public required string Key { get; set; }
    public required string Name { get; set; }
    public required Func<T, TValue> Field { get; set; }
    public Func<T, string>? Display { get; set; }
    public SortDirection Sort { get; set; } = SortDirection.None;
    public Type? CustomControl { get; set; }
    public string? Width { get; set; }
    public double MinWidth { get; set; } = 90;
    public Type ItemType => typeof(T);
    public Type ValueType => typeof(TValue);
    public IDataGridColumnValueAccessor ValueAccessor => _valueAccessor ??= new LambdaColumnValueAccessor(GetFieldAccessor());
    public Func<object, IComparable?> SortKey => o => GetFieldValue((T)o) switch
    {
        IComparable comparable => comparable,
        _ => null
    };
    public Func<object, string> DisplayValue => o =>
    {
        var item = (T)o;
        try
        {
            if (Display != null)
            {
                return Display(item);
            }

            var value = GetFieldValue(item);
            return value?.ToString() ?? string.Empty;
        }
        catch (Exception ex) when (IsMissingOptionalValue(ex))
        {
            return string.Empty;
        }
    };

    private Func<T, TValue> GetFieldAccessor() => _fieldAccessor ??= Field;

    private object? GetFieldValue(T item)
    {
        try
        {
            return GetFieldAccessor()(item);
        }
        catch (Exception ex) when (IsMissingOptionalValue(ex))
        {
            return null;
        }
    }

    private static bool IsMissingOptionalValue(Exception ex) =>
        ex is KeyNotFoundException
        || (ex is InvalidOperationException invalidOperationException
            && invalidOperationException.Message == NullableValueMissingMessage);

    private sealed class LambdaColumnValueAccessor(Func<T, TValue> getter) : IDataGridColumnValueAccessor
    {
        public Type ItemType => typeof(T);
        public Type ValueType => typeof(TValue);
        public bool CanWrite => false;
        public object GetValue(object item) => getter((T)item)!;
        public void SetValue(object item, object value) => throw new NotSupportedException();
    }
}


