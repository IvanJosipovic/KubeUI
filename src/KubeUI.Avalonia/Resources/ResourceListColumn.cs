using Avalonia.Controls.Templates;
using k8s;
using k8s.Models;
using KubeUI.DynamicTableView;

namespace KubeUI.Avalonia.Resources;

public class ResourceListColumn<T, TValue> : IResourceListColumn where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    private const string NullableValueMissingMessage = "Nullable object must have a value.";
    private Func<T, TValue>? _fieldAccessor;

    public required string Key { get; set; }

    public required string Name { get; set; }

    public required Func<T, TValue> Field { get; set; }

    public Func<T, string>? Display { get; set; }

    public SortDirection Sort { get; set; } = SortDirection.None;

    /// <summary>
    /// Gets or sets custom control type, or <see langword="null"/> to render the column as text.
    /// </summary>
    public Type? CustomControl { get; set; }

    public DynamicTableViewWidthMode WidthMode { get; set; } = DynamicTableViewWidthMode.Auto;

    public double Width { get; set; } = 1;

    public double MinWidth { get; set; } = 90;

    public Type ItemType => typeof(T);

    public Type ValueType => typeof(TValue);

    public IReadOnlyList<DynamicTableViewFilterChoice> FilterChoices { get; set; } = [];

    public DynamicTableViewColumn CreateDynamicTableViewColumn(IDataTemplate? cellTemplate = null)
    {
        DynamicTableViewColumn<T> column = new(
            Key,
            Name,
            ValueType,
            GetFieldValue,
            item => DisplayValue(item),
            cellTemplate)
        {
            WidthMode = WidthMode,
            Width = Width,
            MinWidth = MinWidth,
            FilterChoices = FilterChoices
        };
        return column;
    }

    public Func<object, string> DisplayValue =>
        o =>
        {
            var t = (T)o;
            try
            {
                if (Display != null)
                    return Display(t);
                var v = GetFieldValue(t);
                return v?.ToString() ?? "";
            }
            catch (Exception ex) when (IsMissingOptionalValue(ex))
            {
                return "";
            }
        };

    private Func<T, TValue> GetFieldAccessor()
    {
        _fieldAccessor ??= Field;
        return _fieldAccessor;
    }

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

    private static bool IsMissingOptionalValue(Exception ex)
    {
        return ex is KeyNotFoundException
            || (ex is InvalidOperationException invalidOperationException
                && invalidOperationException.Message == NullableValueMissingMessage);
    }

}
