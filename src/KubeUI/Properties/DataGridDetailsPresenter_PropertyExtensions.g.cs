#nullable enable
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using DataGridDetailsPresenter = Avalonia.Controls.Primitives.DataGridDetailsPresenter;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class DataGridDetailsPresenterExtensions
{
public static Avalonia.Controls.Primitives.DataGridDetailsPresenter ContentHeight(this Avalonia.Controls.Primitives.DataGridDetailsPresenter control, IBinding binding)
   => control._set(Avalonia.Controls.Primitives.DataGridDetailsPresenter.ContentHeightProperty, binding);
public static Avalonia.Controls.Primitives.DataGridDetailsPresenter ContentHeight(this Avalonia.Controls.Primitives.DataGridDetailsPresenter control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null)
   => control._set(Avalonia.Controls.Primitives.DataGridDetailsPresenter.ContentHeightProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static Avalonia.Controls.Primitives.DataGridDetailsPresenter ContentHeight(this Avalonia.Controls.Primitives.DataGridDetailsPresenter control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null)
   => control._set(Avalonia.Controls.Primitives.DataGridDetailsPresenter.ContentHeightProperty, func, onChanged, expression);
public static Avalonia.Controls.Primitives.DataGridDetailsPresenter ContentHeight(this Avalonia.Controls.Primitives.DataGridDetailsPresenter control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null)=> control._setEx(Avalonia.Controls.Primitives.DataGridDetailsPresenter.ContentHeightProperty, ps, () => control.ContentHeight = value, bindingMode, converter, bindingSource);
public static Avalonia.Controls.Primitives.DataGridDetailsPresenter ContentHeight<TValue>(this Avalonia.Controls.Primitives.DataGridDetailsPresenter control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null)=> control._setEx(Avalonia.Controls.Primitives.DataGridDetailsPresenter.ContentHeightProperty, ps, () => control.ContentHeight = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

