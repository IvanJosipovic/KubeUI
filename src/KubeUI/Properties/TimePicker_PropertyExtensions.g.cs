#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TimePicker = Ursa.Controls.TimePicker;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class TimePickerExtensions
{
public static T SelectedTime<T>(this T control, IBinding binding) where T : Ursa.Controls.TimePicker
   => control._set(Ursa.Controls.TimePicker.SelectedTimeProperty, binding);
public static T SelectedTime<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimePicker
   => control._set(Ursa.Controls.TimePicker.SelectedTimeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SelectedTime<T>(this T control, Func<System.Nullable<System.TimeSpan>> func, Action<System.Nullable<System.TimeSpan>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimePicker
   => control._set(Ursa.Controls.TimePicker.SelectedTimeProperty, func, onChanged, expression);
public static T SelectedTime<T>(this T control, System.Nullable<System.TimeSpan> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePicker
=> control._setEx(Ursa.Controls.TimePicker.SelectedTimeProperty, ps, () => control.SelectedTime = value, bindingMode, converter, bindingSource);
public static T SelectedTime<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Nullable<System.TimeSpan>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePicker
=> control._setEx(Ursa.Controls.TimePicker.SelectedTimeProperty, ps, () => control.SelectedTime = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Watermark<T>(this T control, IBinding binding) where T : Ursa.Controls.TimePicker
   => control._set(Ursa.Controls.TimePicker.WatermarkProperty, binding);
public static T Watermark<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimePicker
   => control._set(Ursa.Controls.TimePicker.WatermarkProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Watermark<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimePicker
   => control._set(Ursa.Controls.TimePicker.WatermarkProperty, func, onChanged, expression);
public static T Watermark<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePicker
=> control._setEx(Ursa.Controls.TimePicker.WatermarkProperty, ps, () => control.Watermark = value, bindingMode, converter, bindingSource);
public static T Watermark<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePicker
=> control._setEx(Ursa.Controls.TimePicker.WatermarkProperty, ps, () => control.Watermark = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

