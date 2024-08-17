#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using ClockTicks = Ursa.Controls.ClockTicks;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class ClockTicksExtensions
{
public static T ShowHourTicks<T>(this T control, IBinding binding) where T : Ursa.Controls.ClockTicks
   => control._set(Ursa.Controls.ClockTicks.ShowHourTicksProperty, binding);
public static T ShowHourTicks<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ClockTicks
   => control._set(Ursa.Controls.ClockTicks.ShowHourTicksProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ShowHourTicks<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ClockTicks
   => control._set(Ursa.Controls.ClockTicks.ShowHourTicksProperty, func, onChanged, expression);
public static T ShowHourTicks<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ClockTicks
=> control._setEx(Ursa.Controls.ClockTicks.ShowHourTicksProperty, ps, () => control.ShowHourTicks = value, bindingMode, converter, bindingSource);
public static T ShowHourTicks<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ClockTicks
=> control._setEx(Ursa.Controls.ClockTicks.ShowHourTicksProperty, ps, () => control.ShowHourTicks = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ShowMinuteTicks<T>(this T control, IBinding binding) where T : Ursa.Controls.ClockTicks
   => control._set(Ursa.Controls.ClockTicks.ShowMinuteTicksProperty, binding);
public static T ShowMinuteTicks<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ClockTicks
   => control._set(Ursa.Controls.ClockTicks.ShowMinuteTicksProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ShowMinuteTicks<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ClockTicks
   => control._set(Ursa.Controls.ClockTicks.ShowMinuteTicksProperty, func, onChanged, expression);
public static T ShowMinuteTicks<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ClockTicks
=> control._setEx(Ursa.Controls.ClockTicks.ShowMinuteTicksProperty, ps, () => control.ShowMinuteTicks = value, bindingMode, converter, bindingSource);
public static T ShowMinuteTicks<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ClockTicks
=> control._setEx(Ursa.Controls.ClockTicks.ShowMinuteTicksProperty, ps, () => control.ShowMinuteTicks = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T HourTickForeground<T>(this T control, IBinding binding) where T : Ursa.Controls.ClockTicks
   => control._set(Ursa.Controls.ClockTicks.HourTickForegroundProperty, binding);
public static T HourTickForeground<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ClockTicks
   => control._set(Ursa.Controls.ClockTicks.HourTickForegroundProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T HourTickForeground<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ClockTicks
   => control._set(Ursa.Controls.ClockTicks.HourTickForegroundProperty, func, onChanged, expression);
public static T HourTickForeground<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ClockTicks
=> control._setEx(Ursa.Controls.ClockTicks.HourTickForegroundProperty, ps, () => control.HourTickForeground = value, bindingMode, converter, bindingSource);
public static T HourTickForeground<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ClockTicks
=> control._setEx(Ursa.Controls.ClockTicks.HourTickForegroundProperty, ps, () => control.HourTickForeground = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T MinuteTickForeground<T>(this T control, IBinding binding) where T : Ursa.Controls.ClockTicks
   => control._set(Ursa.Controls.ClockTicks.MinuteTickForegroundProperty, binding);
public static T MinuteTickForeground<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ClockTicks
   => control._set(Ursa.Controls.ClockTicks.MinuteTickForegroundProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T MinuteTickForeground<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ClockTicks
   => control._set(Ursa.Controls.ClockTicks.MinuteTickForegroundProperty, func, onChanged, expression);
public static T MinuteTickForeground<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ClockTicks
=> control._setEx(Ursa.Controls.ClockTicks.MinuteTickForegroundProperty, ps, () => control.MinuteTickForeground = value, bindingMode, converter, bindingSource);
public static T MinuteTickForeground<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ClockTicks
=> control._setEx(Ursa.Controls.ClockTicks.MinuteTickForegroundProperty, ps, () => control.MinuteTickForeground = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T HourTickLength<T>(this T control, IBinding binding) where T : Ursa.Controls.ClockTicks
   => control._set(Ursa.Controls.ClockTicks.HourTickLengthProperty, binding);
public static T HourTickLength<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ClockTicks
   => control._set(Ursa.Controls.ClockTicks.HourTickLengthProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T HourTickLength<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ClockTicks
   => control._set(Ursa.Controls.ClockTicks.HourTickLengthProperty, func, onChanged, expression);
public static T HourTickLength<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ClockTicks
=> control._setEx(Ursa.Controls.ClockTicks.HourTickLengthProperty, ps, () => control.HourTickLength = value, bindingMode, converter, bindingSource);
public static T HourTickLength<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ClockTicks
=> control._setEx(Ursa.Controls.ClockTicks.HourTickLengthProperty, ps, () => control.HourTickLength = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T MinuteTickLength<T>(this T control, IBinding binding) where T : Ursa.Controls.ClockTicks
   => control._set(Ursa.Controls.ClockTicks.MinuteTickLengthProperty, binding);
public static T MinuteTickLength<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ClockTicks
   => control._set(Ursa.Controls.ClockTicks.MinuteTickLengthProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T MinuteTickLength<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ClockTicks
   => control._set(Ursa.Controls.ClockTicks.MinuteTickLengthProperty, func, onChanged, expression);
public static T MinuteTickLength<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ClockTicks
=> control._setEx(Ursa.Controls.ClockTicks.MinuteTickLengthProperty, ps, () => control.MinuteTickLength = value, bindingMode, converter, bindingSource);
public static T MinuteTickLength<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ClockTicks
=> control._setEx(Ursa.Controls.ClockTicks.MinuteTickLengthProperty, ps, () => control.MinuteTickLength = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T HourTickWidth<T>(this T control, IBinding binding) where T : Ursa.Controls.ClockTicks
   => control._set(Ursa.Controls.ClockTicks.HourTickWidthProperty, binding);
public static T HourTickWidth<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ClockTicks
   => control._set(Ursa.Controls.ClockTicks.HourTickWidthProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T HourTickWidth<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ClockTicks
   => control._set(Ursa.Controls.ClockTicks.HourTickWidthProperty, func, onChanged, expression);
public static T HourTickWidth<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ClockTicks
=> control._setEx(Ursa.Controls.ClockTicks.HourTickWidthProperty, ps, () => control.HourTickWidth = value, bindingMode, converter, bindingSource);
public static T HourTickWidth<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ClockTicks
=> control._setEx(Ursa.Controls.ClockTicks.HourTickWidthProperty, ps, () => control.HourTickWidth = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T MinuteTickWidth<T>(this T control, IBinding binding) where T : Ursa.Controls.ClockTicks
   => control._set(Ursa.Controls.ClockTicks.MinuteTickWidthProperty, binding);
public static T MinuteTickWidth<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ClockTicks
   => control._set(Ursa.Controls.ClockTicks.MinuteTickWidthProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T MinuteTickWidth<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ClockTicks
   => control._set(Ursa.Controls.ClockTicks.MinuteTickWidthProperty, func, onChanged, expression);
public static T MinuteTickWidth<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ClockTicks
=> control._setEx(Ursa.Controls.ClockTicks.MinuteTickWidthProperty, ps, () => control.MinuteTickWidth = value, bindingMode, converter, bindingSource);
public static T MinuteTickWidth<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ClockTicks
=> control._setEx(Ursa.Controls.ClockTicks.MinuteTickWidthProperty, ps, () => control.MinuteTickWidth = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

