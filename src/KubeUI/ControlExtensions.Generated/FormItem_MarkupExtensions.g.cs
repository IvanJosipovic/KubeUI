#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class FormItem_MarkupExtensions
{
//================= Properties ======================//
 // LabelWidthProperty

/*BindFromExpressionSetterGenerator*/
public static T LabelWidth<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.FormItem
   => control._set(Ursa.Controls.FormItem.LabelWidthProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T LabelWidth<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.FormItem
=> control._setEx(Ursa.Controls.FormItem.LabelWidthProperty, ps, () => control.LabelWidth = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T LabelWidth<T>(this T control, IBinding binding) where T : Ursa.Controls.FormItem
   => control._set(Ursa.Controls.FormItem.LabelWidthProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T LabelWidth<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.FormItem
   => control._set(Ursa.Controls.FormItem.LabelWidthProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T LabelWidth<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.FormItem
=> control._setEx(Ursa.Controls.FormItem.LabelWidthProperty, ps, () => control.LabelWidth = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // LabelAlignmentProperty

/*BindFromExpressionSetterGenerator*/
public static T LabelAlignment<T>(this T control, Func<Avalonia.Layout.HorizontalAlignment> func, Action<Avalonia.Layout.HorizontalAlignment>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.FormItem
   => control._set(Ursa.Controls.FormItem.LabelAlignmentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T LabelAlignment<T>(this T control, Avalonia.Layout.HorizontalAlignment value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.FormItem
=> control._setEx(Ursa.Controls.FormItem.LabelAlignmentProperty, ps, () => control.LabelAlignment = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T LabelAlignment<T>(this T control, IBinding binding) where T : Ursa.Controls.FormItem
   => control._set(Ursa.Controls.FormItem.LabelAlignmentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T LabelAlignment<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.FormItem
   => control._set(Ursa.Controls.FormItem.LabelAlignmentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T LabelAlignment<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Layout.HorizontalAlignment> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.FormItem
=> control._setEx(Ursa.Controls.FormItem.LabelAlignmentProperty, ps, () => control.LabelAlignment = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // LabelWidthProperty

/*ValueStyleSetterGenerator*/
public static Style<T> LabelWidth<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.FormItem
=> style._addSetter(Ursa.Controls.FormItem.LabelWidthProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> LabelWidth<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.FormItem
=> style._addSetter(Ursa.Controls.FormItem.LabelWidthProperty, binding);


 // LabelAlignmentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> LabelAlignment<T>(this Style<T> style, Avalonia.Layout.HorizontalAlignment value) where T : Ursa.Controls.FormItem
=> style._addSetter(Ursa.Controls.FormItem.LabelAlignmentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> LabelAlignment<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.FormItem
=> style._addSetter(Ursa.Controls.FormItem.LabelAlignmentProperty, binding);



}
