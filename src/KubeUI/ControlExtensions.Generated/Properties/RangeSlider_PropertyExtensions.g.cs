#nullable enable
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using RangeSlider = Ursa.Controls.RangeSlider;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class RangeSliderExtensions
{
public static T Minimum<T>(this T control, IBinding binding) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.MinimumProperty, binding);
public static T Minimum<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.MinimumProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Minimum<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.MinimumProperty, func, onChanged, expression);
public static T Minimum<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.MinimumProperty, ps, () => control.Minimum = value, bindingMode, converter, bindingSource);
public static T Minimum<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.MinimumProperty, ps, () => control.Minimum = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Maximum<T>(this T control, IBinding binding) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.MaximumProperty, binding);
public static T Maximum<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.MaximumProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Maximum<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.MaximumProperty, func, onChanged, expression);
public static T Maximum<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.MaximumProperty, ps, () => control.Maximum = value, bindingMode, converter, bindingSource);
public static T Maximum<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.MaximumProperty, ps, () => control.Maximum = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T LowerValue<T>(this T control, IBinding binding) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.LowerValueProperty, binding);
public static T LowerValue<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.LowerValueProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T LowerValue<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.LowerValueProperty, func, onChanged, expression);
public static T LowerValue<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.LowerValueProperty, ps, () => control.LowerValue = value, bindingMode, converter, bindingSource);
public static T LowerValue<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.LowerValueProperty, ps, () => control.LowerValue = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T UpperValue<T>(this T control, IBinding binding) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.UpperValueProperty, binding);
public static T UpperValue<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.UpperValueProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T UpperValue<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.UpperValueProperty, func, onChanged, expression);
public static T UpperValue<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.UpperValueProperty, ps, () => control.UpperValue = value, bindingMode, converter, bindingSource);
public static T UpperValue<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.UpperValueProperty, ps, () => control.UpperValue = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T TrackWidth<T>(this T control, IBinding binding) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.TrackWidthProperty, binding);
public static T TrackWidth<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.TrackWidthProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T TrackWidth<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.TrackWidthProperty, func, onChanged, expression);
public static T TrackWidth<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.TrackWidthProperty, ps, () => control.TrackWidth = value, bindingMode, converter, bindingSource);
public static T TrackWidth<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.TrackWidthProperty, ps, () => control.TrackWidth = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Orientation<T>(this T control, IBinding binding) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.OrientationProperty, binding);
public static T Orientation<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.OrientationProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Orientation<T>(this T control, Func<Avalonia.Layout.Orientation> func, Action<Avalonia.Layout.Orientation>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.OrientationProperty, func, onChanged, expression);
public static T Orientation<T>(this T control, Avalonia.Layout.Orientation value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.OrientationProperty, ps, () => control.Orientation = value, bindingMode, converter, bindingSource);
public static T Orientation<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Layout.Orientation> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.OrientationProperty, ps, () => control.Orientation = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsDirectionReversed<T>(this T control, IBinding binding) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.IsDirectionReversedProperty, binding);
public static T IsDirectionReversed<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.IsDirectionReversedProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsDirectionReversed<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.IsDirectionReversedProperty, func, onChanged, expression);
public static T IsDirectionReversed<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.IsDirectionReversedProperty, ps, () => control.IsDirectionReversed = value, bindingMode, converter, bindingSource);
public static T IsDirectionReversed<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.IsDirectionReversedProperty, ps, () => control.IsDirectionReversed = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T TickFrequency<T>(this T control, IBinding binding) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.TickFrequencyProperty, binding);
public static T TickFrequency<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.TickFrequencyProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T TickFrequency<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.TickFrequencyProperty, func, onChanged, expression);
public static T TickFrequency<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.TickFrequencyProperty, ps, () => control.TickFrequency = value, bindingMode, converter, bindingSource);
public static T TickFrequency<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.TickFrequencyProperty, ps, () => control.TickFrequency = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Ticks<T>(this T control, IBinding binding) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.TicksProperty, binding);
public static T Ticks<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.TicksProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Ticks<T>(this T control, Func<Avalonia.Collections.AvaloniaList<System.Double>> func, Action<Avalonia.Collections.AvaloniaList<System.Double>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.TicksProperty, func, onChanged, expression);
public static T Ticks<T>(this T control, Avalonia.Collections.AvaloniaList<System.Double> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.TicksProperty, ps, () => control.Ticks = value, bindingMode, converter, bindingSource);
public static T Ticks<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Collections.AvaloniaList<System.Double>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.TicksProperty, ps, () => control.Ticks = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T TickPlacement<T>(this T control, IBinding binding) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.TickPlacementProperty, binding);
public static T TickPlacement<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.TickPlacementProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T TickPlacement<T>(this T control, Func<Avalonia.Controls.TickPlacement> func, Action<Avalonia.Controls.TickPlacement>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.TickPlacementProperty, func, onChanged, expression);
public static T TickPlacement<T>(this T control, Avalonia.Controls.TickPlacement value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.TickPlacementProperty, ps, () => control.TickPlacement = value, bindingMode, converter, bindingSource);
public static T TickPlacement<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.TickPlacement> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.TickPlacementProperty, ps, () => control.TickPlacement = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsSnapToTick<T>(this T control, IBinding binding) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.IsSnapToTickProperty, binding);
public static T IsSnapToTick<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.IsSnapToTickProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsSnapToTick<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.IsSnapToTickProperty, func, onChanged, expression);
public static T IsSnapToTick<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.IsSnapToTickProperty, ps, () => control.IsSnapToTick = value, bindingMode, converter, bindingSource);
public static T IsSnapToTick<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.IsSnapToTickProperty, ps, () => control.IsSnapToTick = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

