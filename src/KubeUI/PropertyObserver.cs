using System.Linq.Expressions;

namespace KubeUI;

public class PropertyObserver<TSource, TResult> : IDisposable where TSource : INotifyPropertyChanged
{
    private readonly TSource _source;
    private readonly Func<TSource, TResult> _getter;
    private readonly string _propertyName;
    private readonly Action<TResult> _onChange;

    public PropertyObserver(TSource source, Expression<Func<TSource, TResult>> expression, Action<TResult> onChange)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _getter = expression.Compile();
        _onChange = onChange ?? throw new ArgumentNullException(nameof(onChange));

        if (expression.Body is MemberExpression member)
        {
            _propertyName = member.Member.Name;
        }
        else
        {
            throw new ArgumentException("Invalid property expression");
        }

        // Subscribe to property changes
        _source.PropertyChanged += OnSourcePropertyChanged;

        // Initialize with the current value
        _onChange(_getter(_source));
    }

    private void OnSourcePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == _propertyName)
        {
            _onChange(_getter(_source));
        }
    }

    public void Dispose()
    {
        _source.PropertyChanged -= OnSourcePropertyChanged;
    }
}
