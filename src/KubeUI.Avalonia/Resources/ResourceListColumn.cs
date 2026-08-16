using k8s;
using k8s.Models;

namespace KubeUI.Avalonia.Resources;

public class ResourceListColumn<T, TValue> : IResourceListColumn where T : class, IKubernetesObject<V1ObjectMeta>, new()
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

    public Func<object, IComparable?> SortKey =>
        o => GetFieldValue((T)o) switch
        {
            IComparable comparable => comparable,
            _ => null
        };

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

    private sealed class LambdaColumnValueAccessor : IDataGridColumnValueAccessor
    {
        private readonly Func<T, TValue> _getter;

        public LambdaColumnValueAccessor(Func<T, TValue> getter)
        {
            _getter = getter;
        }

        public Type ItemType => typeof(T);

        public Type ValueType => typeof(TValue);

        public bool CanWrite => false;

        public object GetValue(object item)
        {
            try
            {
                return _getter((T)item)!;
            }
            catch (Exception ex) when (IsMissingOptionalValue(ex))
            {
                return null!;
            }
        }

        public void SetValue(object item, object value)
        {
            throw new NotSupportedException();
        }
    }
}
