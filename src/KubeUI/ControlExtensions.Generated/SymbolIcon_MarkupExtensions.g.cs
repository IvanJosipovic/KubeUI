#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class SymbolIcon_MarkupExtensions
{
//================= Properties ======================//
 // SymbolProperty

/*BindFromExpressionSetterGenerator*/
public static T Symbol<T>(this T control, Func<FluentAvalonia.UI.Controls.Symbol> func, Action<FluentAvalonia.UI.Controls.Symbol>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.SymbolIcon
   => control._set(FluentAvalonia.UI.Controls.SymbolIcon.SymbolProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Symbol<T>(this T control, FluentAvalonia.UI.Controls.Symbol value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SymbolIcon
=> control._setEx(FluentAvalonia.UI.Controls.SymbolIcon.SymbolProperty, ps, () => control.Symbol = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Symbol<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.SymbolIcon
   => control._set(FluentAvalonia.UI.Controls.SymbolIcon.SymbolProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Symbol<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.SymbolIcon
   => control._set(FluentAvalonia.UI.Controls.SymbolIcon.SymbolProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Symbol<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.Symbol> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SymbolIcon
=> control._setEx(FluentAvalonia.UI.Controls.SymbolIcon.SymbolProperty, ps, () => control.Symbol = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // FontSizeProperty

/*BindFromExpressionSetterGenerator*/
public static T FontSize<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.SymbolIcon
   => control._set(FluentAvalonia.UI.Controls.SymbolIcon.FontSizeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T FontSize<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SymbolIcon
=> control._setEx(FluentAvalonia.UI.Controls.SymbolIcon.FontSizeProperty, ps, () => control.FontSize = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T FontSize<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.SymbolIcon
   => control._set(FluentAvalonia.UI.Controls.SymbolIcon.FontSizeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T FontSize<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.SymbolIcon
   => control._set(FluentAvalonia.UI.Controls.SymbolIcon.FontSizeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T FontSize<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SymbolIcon
=> control._setEx(FluentAvalonia.UI.Controls.SymbolIcon.FontSizeProperty, ps, () => control.FontSize = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // SymbolProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Symbol<T>(this Style<T> style, FluentAvalonia.UI.Controls.Symbol value) where T : FluentAvalonia.UI.Controls.SymbolIcon
=> style._addSetter(FluentAvalonia.UI.Controls.SymbolIcon.SymbolProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Symbol<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.SymbolIcon
=> style._addSetter(FluentAvalonia.UI.Controls.SymbolIcon.SymbolProperty, binding);


 // FontSizeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> FontSize<T>(this Style<T> style, System.Double value) where T : FluentAvalonia.UI.Controls.SymbolIcon
=> style._addSetter(FluentAvalonia.UI.Controls.SymbolIcon.FontSizeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> FontSize<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.SymbolIcon
=> style._addSetter(FluentAvalonia.UI.Controls.SymbolIcon.FontSizeProperty, binding);



}
