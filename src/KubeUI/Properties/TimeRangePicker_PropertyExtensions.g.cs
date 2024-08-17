#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TimeRangePicker = Ursa.Controls.TimeRangePicker;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class TimeRangePickerExtensions
{
public static T StartTime<T>(this T control, IBinding binding) where T : Ursa.Controls.TimeRangePicker
   => control._set(Ursa.Controls.TimeRangePicker.StartTimeProperty, binding);
public static T StartTime<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimeRangePicker
   => control._set(Ursa.Controls.TimeRangePicker.StartTimeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T StartTime<T>(this T control, Func<System.Nullable<System.TimeSpan>> func, Action<System.Nullable<System.TimeSpan>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimeRangePicker
   => control._set(Ursa.Controls.TimeRangePicker.StartTimeProperty, func, onChanged, expression);
public static T StartTime<T>(this T control, System.Nullable<System.TimeSpan> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimeRangePicker
=> control._setEx(Ursa.Controls.TimeRangePicker.StartTimeProperty, ps, () => control.StartTime = value, bindingMode, converter, bindingSource);
public static T StartTime<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Nullable<System.TimeSpan>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimeRangePicker
=> control._setEx(Ursa.Controls.TimeRangePicker.StartTimeProperty, ps, () => control.StartTime = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T EndTime<T>(this T control, IBinding binding) where T : Ursa.Controls.TimeRangePicker
   => control._set(Ursa.Controls.TimeRangePicker.EndTimeProperty, binding);
public static T EndTime<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimeRangePicker
   => control._set(Ursa.Controls.TimeRangePicker.EndTimeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T EndTime<T>(this T control, Func<System.Nullable<System.TimeSpan>> func, Action<System.Nullable<System.TimeSpan>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimeRangePicker
   => control._set(Ursa.Controls.TimeRangePicker.EndTimeProperty, func, onChanged, expression);
public static T EndTime<T>(this T control, System.Nullable<System.TimeSpan> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimeRangePicker
=> control._setEx(Ursa.Controls.TimeRangePicker.EndTimeProperty, ps, () => control.EndTime = value, bindingMode, converter, bindingSource);
public static T EndTime<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Nullable<System.TimeSpan>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimeRangePicker
=> control._setEx(Ursa.Controls.TimeRangePicker.EndTimeProperty, ps, () => control.EndTime = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T StartWatermark<T>(this T control, IBinding binding) where T : Ursa.Controls.TimeRangePicker
   => control._set(Ursa.Controls.TimeRangePicker.StartWatermarkProperty, binding);
public static T StartWatermark<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimeRangePicker
   => control._set(Ursa.Controls.TimeRangePicker.StartWatermarkProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T StartWatermark<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimeRangePicker
   => control._set(Ursa.Controls.TimeRangePicker.StartWatermarkProperty, func, onChanged, expression);
public static T StartWatermark<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimeRangePicker
=> control._setEx(Ursa.Controls.TimeRangePicker.StartWatermarkProperty, ps, () => control.StartWatermark = value, bindingMode, converter, bindingSource);
public static T StartWatermark<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimeRangePicker
=> control._setEx(Ursa.Controls.TimeRangePicker.StartWatermarkProperty, ps, () => control.StartWatermark = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T EndWatermark<T>(this T control, IBinding binding) where T : Ursa.Controls.TimeRangePicker
   => control._set(Ursa.Controls.TimeRangePicker.EndWatermarkProperty, binding);
public static T EndWatermark<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimeRangePicker
   => control._set(Ursa.Controls.TimeRangePicker.EndWatermarkProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T EndWatermark<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimeRangePicker
   => control._set(Ursa.Controls.TimeRangePicker.EndWatermarkProperty, func, onChanged, expression);
public static T EndWatermark<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimeRangePicker
=> control._setEx(Ursa.Controls.TimeRangePicker.EndWatermarkProperty, ps, () => control.EndWatermark = value, bindingMode, converter, bindingSource);
public static T EndWatermark<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimeRangePicker
=> control._setEx(Ursa.Controls.TimeRangePicker.EndWatermarkProperty, ps, () => control.EndWatermark = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

