#nullable enable
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using SelectionList = Ursa.Controls.SelectionList;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class SelectionListExtensions
{
public static T Indicator<T>(this T control, IBinding binding) where T : Ursa.Controls.SelectionList
   => control._set(Ursa.Controls.SelectionList.IndicatorProperty, binding);
public static T Indicator<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.SelectionList
   => control._set(Ursa.Controls.SelectionList.IndicatorProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Indicator<T>(this T control, Func<Avalonia.Controls.Control> func, Action<Avalonia.Controls.Control>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.SelectionList
   => control._set(Ursa.Controls.SelectionList.IndicatorProperty, func, onChanged, expression);
public static T Indicator<T>(this T control, Avalonia.Controls.Control value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.SelectionList
=> control._setEx(Ursa.Controls.SelectionList.IndicatorProperty, ps, () => control.Indicator = value, bindingMode, converter, bindingSource);
public static T Indicator<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Control> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.SelectionList
=> control._setEx(Ursa.Controls.SelectionList.IndicatorProperty, ps, () => control.Indicator = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

