#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Clock = Ursa.Controls.Clock;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class ClockExtensions
{
public static T Time<T>(this T control, IBinding binding) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.TimeProperty, binding);
public static T Time<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.TimeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Time<T>(this T control, Func<System.DateTime> func, Action<System.DateTime>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.TimeProperty, func, onChanged, expression);
public static T Time<T>(this T control, System.DateTime value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Clock
=> control._setEx(Ursa.Controls.Clock.TimeProperty, ps, () => control.Time = value, bindingMode, converter, bindingSource);
public static T Time<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.DateTime> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Clock
=> control._setEx(Ursa.Controls.Clock.TimeProperty, ps, () => control.Time = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ShowHourTicks<T>(this T control, IBinding binding) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.ShowHourTicksProperty, binding);
public static T ShowHourTicks<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.ShowHourTicksProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ShowHourTicks<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.ShowHourTicksProperty, func, onChanged, expression);
public static T ShowHourTicks<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Clock
=> control._setEx(Ursa.Controls.Clock.ShowHourTicksProperty, ps, () => control.ShowHourTicks = value, bindingMode, converter, bindingSource);
public static T ShowHourTicks<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Clock
=> control._setEx(Ursa.Controls.Clock.ShowHourTicksProperty, ps, () => control.ShowHourTicks = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ShowMinuteTicks<T>(this T control, IBinding binding) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.ShowMinuteTicksProperty, binding);
public static T ShowMinuteTicks<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.ShowMinuteTicksProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ShowMinuteTicks<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.ShowMinuteTicksProperty, func, onChanged, expression);
public static T ShowMinuteTicks<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Clock
=> control._setEx(Ursa.Controls.Clock.ShowMinuteTicksProperty, ps, () => control.ShowMinuteTicks = value, bindingMode, converter, bindingSource);
public static T ShowMinuteTicks<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Clock
=> control._setEx(Ursa.Controls.Clock.ShowMinuteTicksProperty, ps, () => control.ShowMinuteTicks = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T HandBrush<T>(this T control, IBinding binding) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.HandBrushProperty, binding);
public static T HandBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.HandBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T HandBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.HandBrushProperty, func, onChanged, expression);
public static T HandBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Clock
=> control._setEx(Ursa.Controls.Clock.HandBrushProperty, ps, () => control.HandBrush = value, bindingMode, converter, bindingSource);
public static T HandBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Clock
=> control._setEx(Ursa.Controls.Clock.HandBrushProperty, ps, () => control.HandBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ShowHourHand<T>(this T control, IBinding binding) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.ShowHourHandProperty, binding);
public static T ShowHourHand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.ShowHourHandProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ShowHourHand<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.ShowHourHandProperty, func, onChanged, expression);
public static T ShowHourHand<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Clock
=> control._setEx(Ursa.Controls.Clock.ShowHourHandProperty, ps, () => control.ShowHourHand = value, bindingMode, converter, bindingSource);
public static T ShowHourHand<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Clock
=> control._setEx(Ursa.Controls.Clock.ShowHourHandProperty, ps, () => control.ShowHourHand = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ShowMinuteHand<T>(this T control, IBinding binding) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.ShowMinuteHandProperty, binding);
public static T ShowMinuteHand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.ShowMinuteHandProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ShowMinuteHand<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.ShowMinuteHandProperty, func, onChanged, expression);
public static T ShowMinuteHand<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Clock
=> control._setEx(Ursa.Controls.Clock.ShowMinuteHandProperty, ps, () => control.ShowMinuteHand = value, bindingMode, converter, bindingSource);
public static T ShowMinuteHand<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Clock
=> control._setEx(Ursa.Controls.Clock.ShowMinuteHandProperty, ps, () => control.ShowMinuteHand = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ShowSecondHand<T>(this T control, IBinding binding) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.ShowSecondHandProperty, binding);
public static T ShowSecondHand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.ShowSecondHandProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ShowSecondHand<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.ShowSecondHandProperty, func, onChanged, expression);
public static T ShowSecondHand<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Clock
=> control._setEx(Ursa.Controls.Clock.ShowSecondHandProperty, ps, () => control.ShowSecondHand = value, bindingMode, converter, bindingSource);
public static T ShowSecondHand<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Clock
=> control._setEx(Ursa.Controls.Clock.ShowSecondHandProperty, ps, () => control.ShowSecondHand = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsSmooth<T>(this T control, IBinding binding) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.IsSmoothProperty, binding);
public static T IsSmooth<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.IsSmoothProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsSmooth<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.IsSmoothProperty, func, onChanged, expression);
public static T IsSmooth<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Clock
=> control._setEx(Ursa.Controls.Clock.IsSmoothProperty, ps, () => control.IsSmooth = value, bindingMode, converter, bindingSource);
public static T IsSmooth<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Clock
=> control._setEx(Ursa.Controls.Clock.IsSmoothProperty, ps, () => control.IsSmooth = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

