#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using DatePicker = Ursa.Controls.DatePicker;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class DatePickerExtensions
{
public static T SelectedDate<T>(this T control, IBinding binding) where T : Ursa.Controls.DatePicker
   => control._set(Ursa.Controls.DatePicker.SelectedDateProperty, binding);
public static T SelectedDate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DatePicker
   => control._set(Ursa.Controls.DatePicker.SelectedDateProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SelectedDate<T>(this T control, Func<System.Nullable<System.DateTime>> func, Action<System.Nullable<System.DateTime>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DatePicker
   => control._set(Ursa.Controls.DatePicker.SelectedDateProperty, func, onChanged, expression);
public static T SelectedDate<T>(this T control, System.Nullable<System.DateTime> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePicker
=> control._setEx(Ursa.Controls.DatePicker.SelectedDateProperty, ps, () => control.SelectedDate = value, bindingMode, converter, bindingSource);
public static T SelectedDate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Nullable<System.DateTime>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePicker
=> control._setEx(Ursa.Controls.DatePicker.SelectedDateProperty, ps, () => control.SelectedDate = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Watermark<T>(this T control, IBinding binding) where T : Ursa.Controls.DatePicker
   => control._set(Ursa.Controls.DatePicker.WatermarkProperty, binding);
public static T Watermark<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DatePicker
   => control._set(Ursa.Controls.DatePicker.WatermarkProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Watermark<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DatePicker
   => control._set(Ursa.Controls.DatePicker.WatermarkProperty, func, onChanged, expression);
public static T Watermark<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePicker
=> control._setEx(Ursa.Controls.DatePicker.WatermarkProperty, ps, () => control.Watermark = value, bindingMode, converter, bindingSource);
public static T Watermark<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePicker
=> control._setEx(Ursa.Controls.DatePicker.WatermarkProperty, ps, () => control.Watermark = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

