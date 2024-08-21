#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class DataGridTextColumn_MarkupExtensions
{
//================= Properties ======================//
 // FontFamilyProperty

/*BindFromExpressionSetterGenerator*/
public static T FontFamily<T>(this T control, Func<Avalonia.Media.FontFamily> func, Action<Avalonia.Media.FontFamily>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGridTextColumn
   => control._set(Avalonia.Controls.DataGridTextColumn.FontFamilyProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T FontFamily<T>(this T control, Avalonia.Media.FontFamily value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridTextColumn
=> control._setEx(Avalonia.Controls.DataGridTextColumn.FontFamilyProperty, ps, () => control.FontFamily = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T FontFamily<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGridTextColumn
   => control._set(Avalonia.Controls.DataGridTextColumn.FontFamilyProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T FontFamily<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGridTextColumn
   => control._set(Avalonia.Controls.DataGridTextColumn.FontFamilyProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T FontFamily<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.FontFamily> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridTextColumn
=> control._setEx(Avalonia.Controls.DataGridTextColumn.FontFamilyProperty, ps, () => control.FontFamily = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // FontSizeProperty

/*BindFromExpressionSetterGenerator*/
public static T FontSize<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGridTextColumn
   => control._set(Avalonia.Controls.DataGridTextColumn.FontSizeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T FontSize<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridTextColumn
=> control._setEx(Avalonia.Controls.DataGridTextColumn.FontSizeProperty, ps, () => control.FontSize = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T FontSize<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGridTextColumn
   => control._set(Avalonia.Controls.DataGridTextColumn.FontSizeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T FontSize<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGridTextColumn
   => control._set(Avalonia.Controls.DataGridTextColumn.FontSizeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T FontSize<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridTextColumn
=> control._setEx(Avalonia.Controls.DataGridTextColumn.FontSizeProperty, ps, () => control.FontSize = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // FontStyleProperty

/*BindFromExpressionSetterGenerator*/
public static T FontStyle<T>(this T control, Func<Avalonia.Media.FontStyle> func, Action<Avalonia.Media.FontStyle>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGridTextColumn
   => control._set(Avalonia.Controls.DataGridTextColumn.FontStyleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T FontStyle<T>(this T control, Avalonia.Media.FontStyle value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridTextColumn
=> control._setEx(Avalonia.Controls.DataGridTextColumn.FontStyleProperty, ps, () => control.FontStyle = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T FontStyle<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGridTextColumn
   => control._set(Avalonia.Controls.DataGridTextColumn.FontStyleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T FontStyle<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGridTextColumn
   => control._set(Avalonia.Controls.DataGridTextColumn.FontStyleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T FontStyle<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.FontStyle> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridTextColumn
=> control._setEx(Avalonia.Controls.DataGridTextColumn.FontStyleProperty, ps, () => control.FontStyle = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // FontWeightProperty

/*BindFromExpressionSetterGenerator*/
public static T FontWeight<T>(this T control, Func<Avalonia.Media.FontWeight> func, Action<Avalonia.Media.FontWeight>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGridTextColumn
   => control._set(Avalonia.Controls.DataGridTextColumn.FontWeightProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T FontWeight<T>(this T control, Avalonia.Media.FontWeight value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridTextColumn
=> control._setEx(Avalonia.Controls.DataGridTextColumn.FontWeightProperty, ps, () => control.FontWeight = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T FontWeight<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGridTextColumn
   => control._set(Avalonia.Controls.DataGridTextColumn.FontWeightProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T FontWeight<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGridTextColumn
   => control._set(Avalonia.Controls.DataGridTextColumn.FontWeightProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T FontWeight<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.FontWeight> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridTextColumn
=> control._setEx(Avalonia.Controls.DataGridTextColumn.FontWeightProperty, ps, () => control.FontWeight = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // FontStretchProperty

/*BindFromExpressionSetterGenerator*/
public static T FontStretch<T>(this T control, Func<Avalonia.Media.FontStretch> func, Action<Avalonia.Media.FontStretch>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGridTextColumn
   => control._set(Avalonia.Controls.DataGridTextColumn.FontStretchProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T FontStretch<T>(this T control, Avalonia.Media.FontStretch value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridTextColumn
=> control._setEx(Avalonia.Controls.DataGridTextColumn.FontStretchProperty, ps, () => control.FontStretch = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T FontStretch<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGridTextColumn
   => control._set(Avalonia.Controls.DataGridTextColumn.FontStretchProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T FontStretch<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGridTextColumn
   => control._set(Avalonia.Controls.DataGridTextColumn.FontStretchProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T FontStretch<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.FontStretch> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridTextColumn
=> control._setEx(Avalonia.Controls.DataGridTextColumn.FontStretchProperty, ps, () => control.FontStretch = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ForegroundProperty

/*BindFromExpressionSetterGenerator*/
public static T Foreground<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGridTextColumn
   => control._set(Avalonia.Controls.DataGridTextColumn.ForegroundProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Foreground<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridTextColumn
=> control._setEx(Avalonia.Controls.DataGridTextColumn.ForegroundProperty, ps, () => control.Foreground = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Foreground<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGridTextColumn
   => control._set(Avalonia.Controls.DataGridTextColumn.ForegroundProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Foreground<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGridTextColumn
   => control._set(Avalonia.Controls.DataGridTextColumn.ForegroundProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Foreground<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridTextColumn
=> control._setEx(Avalonia.Controls.DataGridTextColumn.ForegroundProperty, ps, () => control.Foreground = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//

}
