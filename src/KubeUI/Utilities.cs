using Avalonia.Data.Converters;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace KubeUI;

public static class Utilities
{
    public static FuncValueConverter<bool, bool> InverseBooleanConverter { get; } = new FuncValueConverter<bool, bool>(b => !b);

    public static FuncValueConverter<object, bool> NotNullConverter { get; } = new FuncValueConverter<object, bool>((x) => x != null);

    public static T GetRequiredService<T>(this Application? app)
    {
        if (app.TryFindResource(typeof(IServiceProvider), out var service))
        {
            return ((IServiceProvider)service).GetRequiredService<T>();
        }
        else
        {
            throw new Exception($"Cant find {typeof(IServiceProvider).Name}");
        }
    }

    public static object GetRequiredService(this Application? app, Type type)
    {
        if (app.TryFindResource(typeof(IServiceProvider), out var service))
        {
            return ((IServiceProvider)service!).GetRequiredService(type);
        }
        else
        {
            throw new Exception($"Cant find {typeof(IServiceProvider).Name}");
        }
    }

    public static T Set<T>(this T control, Func<T,T> func) where T : Control
    {
        return func.Invoke(control);
    }

    public static TDataGrid Columns<TDataGrid>(this TDataGrid container, params DataGridColumn[] items) where TDataGrid : DataGrid
    {
        IList items2 = container.Columns;
        if (items2 != null)
        {
            foreach (DataGridColumn value in items)
            {
                items2.Add(value);
            }
        }

        return container;
    }

    //public static TControl ContextMenu<TControl>(this TControl container, params Control[] items) where TControl : Control
    //{
    //    container.ContextMenu ??= new ContextMenu();

    //    foreach (var item in items)
    //    {
    //        container.ContextMenu.Items.Add(item);
    //    }

    //    return container;
    //}

    public static TControl Set<TControl,TValue>(this TControl control, AvaloniaProperty property, TValue value, FuncValueConverter<TValue, object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where TControl : Control
    {
        return control._setEx(property, ps, () => control[property] = converter.TryConvert(value), bindingMode, converter, bindingSource);
    }
}
