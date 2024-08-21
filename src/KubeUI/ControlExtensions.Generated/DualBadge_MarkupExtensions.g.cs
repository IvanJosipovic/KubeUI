#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class DualBadge_MarkupExtensions
{
//================= Properties ======================//
 // IconProperty

/*BindFromExpressionSetterGenerator*/
public static T Icon<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DualBadge
   => control._set(Ursa.Controls.DualBadge.IconProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Icon<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DualBadge
=> control._setEx(Ursa.Controls.DualBadge.IconProperty, ps, () => control.Icon = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Icon<T>(this T control, IBinding binding) where T : Ursa.Controls.DualBadge
   => control._set(Ursa.Controls.DualBadge.IconProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Icon<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DualBadge
   => control._set(Ursa.Controls.DualBadge.IconProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Icon<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DualBadge
=> control._setEx(Ursa.Controls.DualBadge.IconProperty, ps, () => control.Icon = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IconTemplateProperty

/*BindFromExpressionSetterGenerator*/
public static T IconTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DualBadge
   => control._set(Ursa.Controls.DualBadge.IconTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IconTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DualBadge
=> control._setEx(Ursa.Controls.DualBadge.IconTemplateProperty, ps, () => control.IconTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IconTemplate<T>(this T control, IBinding binding) where T : Ursa.Controls.DualBadge
   => control._set(Ursa.Controls.DualBadge.IconTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IconTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DualBadge
   => control._set(Ursa.Controls.DualBadge.IconTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IconTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DualBadge
=> control._setEx(Ursa.Controls.DualBadge.IconTemplateProperty, ps, () => control.IconTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IconForegroundProperty

/*BindFromExpressionSetterGenerator*/
public static T IconForeground<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DualBadge
   => control._set(Ursa.Controls.DualBadge.IconForegroundProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IconForeground<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DualBadge
=> control._setEx(Ursa.Controls.DualBadge.IconForegroundProperty, ps, () => control.IconForeground = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IconForeground<T>(this T control, IBinding binding) where T : Ursa.Controls.DualBadge
   => control._set(Ursa.Controls.DualBadge.IconForegroundProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IconForeground<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DualBadge
   => control._set(Ursa.Controls.DualBadge.IconForegroundProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IconForeground<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DualBadge
=> control._setEx(Ursa.Controls.DualBadge.IconForegroundProperty, ps, () => control.IconForeground = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // HeaderForegroundProperty

/*BindFromExpressionSetterGenerator*/
public static T HeaderForeground<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DualBadge
   => control._set(Ursa.Controls.DualBadge.HeaderForegroundProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T HeaderForeground<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DualBadge
=> control._setEx(Ursa.Controls.DualBadge.HeaderForegroundProperty, ps, () => control.HeaderForeground = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T HeaderForeground<T>(this T control, IBinding binding) where T : Ursa.Controls.DualBadge
   => control._set(Ursa.Controls.DualBadge.HeaderForegroundProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T HeaderForeground<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DualBadge
   => control._set(Ursa.Controls.DualBadge.HeaderForegroundProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T HeaderForeground<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DualBadge
=> control._setEx(Ursa.Controls.DualBadge.HeaderForegroundProperty, ps, () => control.HeaderForeground = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // HeaderBackgroundProperty

/*BindFromExpressionSetterGenerator*/
public static T HeaderBackground<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DualBadge
   => control._set(Ursa.Controls.DualBadge.HeaderBackgroundProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T HeaderBackground<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DualBadge
=> control._setEx(Ursa.Controls.DualBadge.HeaderBackgroundProperty, ps, () => control.HeaderBackground = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T HeaderBackground<T>(this T control, IBinding binding) where T : Ursa.Controls.DualBadge
   => control._set(Ursa.Controls.DualBadge.HeaderBackgroundProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T HeaderBackground<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DualBadge
   => control._set(Ursa.Controls.DualBadge.HeaderBackgroundProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T HeaderBackground<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DualBadge
=> control._setEx(Ursa.Controls.DualBadge.HeaderBackgroundProperty, ps, () => control.HeaderBackground = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // IconProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Icon<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.DualBadge
=> style._addSetter(Ursa.Controls.DualBadge.IconProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Icon<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DualBadge
=> style._addSetter(Ursa.Controls.DualBadge.IconProperty, binding);


 // IconTemplateProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IconTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : Ursa.Controls.DualBadge
=> style._addSetter(Ursa.Controls.DualBadge.IconTemplateProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IconTemplate<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DualBadge
=> style._addSetter(Ursa.Controls.DualBadge.IconTemplateProperty, binding);


 // IconForegroundProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IconForeground<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Ursa.Controls.DualBadge
=> style._addSetter(Ursa.Controls.DualBadge.IconForegroundProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IconForeground<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DualBadge
=> style._addSetter(Ursa.Controls.DualBadge.IconForegroundProperty, binding);


 // HeaderForegroundProperty

/*ValueStyleSetterGenerator*/
public static Style<T> HeaderForeground<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Ursa.Controls.DualBadge
=> style._addSetter(Ursa.Controls.DualBadge.HeaderForegroundProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> HeaderForeground<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DualBadge
=> style._addSetter(Ursa.Controls.DualBadge.HeaderForegroundProperty, binding);


 // HeaderBackgroundProperty

/*ValueStyleSetterGenerator*/
public static Style<T> HeaderBackground<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Ursa.Controls.DualBadge
=> style._addSetter(Ursa.Controls.DualBadge.HeaderBackgroundProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> HeaderBackground<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DualBadge
=> style._addSetter(Ursa.Controls.DualBadge.HeaderBackgroundProperty, binding);



}
