#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
[global::System.CodeDom.Compiler.GeneratedCode("AvaloniaExtensionGenerator", "11.1.3.0")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static partial class DatePicker_MarkupExtensions
{
//================= Properties ======================//
 // SelectedDateProperty

/*BindFromExpressionSetterGenerator*/
public static T SelectedDate<T>(this T control, Func<System.Nullable<System.DateTime>> func, Action<System.Nullable<System.DateTime>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DatePicker
   => control._set(Ursa.Controls.DatePicker.SelectedDateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SelectedDate<T>(this T control, System.Nullable<System.DateTime> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePicker
=> control._setEx(Ursa.Controls.DatePicker.SelectedDateProperty, ps, () => control.SelectedDate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SelectedDate<T>(this T control, IBinding binding) where T : Ursa.Controls.DatePicker
   => control._set(Ursa.Controls.DatePicker.SelectedDateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SelectedDate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DatePicker
   => control._set(Ursa.Controls.DatePicker.SelectedDateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SelectedDate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Nullable<System.DateTime>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePicker
=> control._setEx(Ursa.Controls.DatePicker.SelectedDateProperty, ps, () => control.SelectedDate = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // WatermarkProperty

/*BindFromExpressionSetterGenerator*/
public static T Watermark<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DatePicker
   => control._set(Ursa.Controls.DatePicker.WatermarkProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Watermark<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePicker
=> control._setEx(Ursa.Controls.DatePicker.WatermarkProperty, ps, () => control.Watermark = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Watermark<T>(this T control, IBinding binding) where T : Ursa.Controls.DatePicker
   => control._set(Ursa.Controls.DatePicker.WatermarkProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Watermark<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DatePicker
   => control._set(Ursa.Controls.DatePicker.WatermarkProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Watermark<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePicker
=> control._setEx(Ursa.Controls.DatePicker.WatermarkProperty, ps, () => control.Watermark = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // SelectedDateProperty

/*ValueStyleSetterGenerator*/
public static Style<T> SelectedDate<T>(this Style<T> style, System.Nullable<System.DateTime> value) where T : Ursa.Controls.DatePicker
=> style._addSetter(Ursa.Controls.DatePicker.SelectedDateProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SelectedDate<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DatePicker
=> style._addSetter(Ursa.Controls.DatePicker.SelectedDateProperty, binding);


 // WatermarkProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Watermark<T>(this Style<T> style, System.String value) where T : Ursa.Controls.DatePicker
=> style._addSetter(Ursa.Controls.DatePicker.WatermarkProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Watermark<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DatePicker
=> style._addSetter(Ursa.Controls.DatePicker.WatermarkProperty, binding);



}
