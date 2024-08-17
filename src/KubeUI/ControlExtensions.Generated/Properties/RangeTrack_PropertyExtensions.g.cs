#nullable enable
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using RangeTrack = Ursa.Controls.RangeTrack;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class RangeTrackExtensions
{
public static T Minimum<T>(this T control, IBinding binding) where T : Ursa.Controls.RangeTrack
   => control._set(Ursa.Controls.RangeTrack.MinimumProperty, binding);
public static T Minimum<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.RangeTrack
   => control._set(Ursa.Controls.RangeTrack.MinimumProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Minimum<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.RangeTrack
   => control._set(Ursa.Controls.RangeTrack.MinimumProperty, func, onChanged, expression);
public static T Minimum<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeTrack
=> control._setEx(Ursa.Controls.RangeTrack.MinimumProperty, ps, () => control.Minimum = value, bindingMode, converter, bindingSource);
public static T Minimum<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeTrack
=> control._setEx(Ursa.Controls.RangeTrack.MinimumProperty, ps, () => control.Minimum = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Maximum<T>(this T control, IBinding binding) where T : Ursa.Controls.RangeTrack
   => control._set(Ursa.Controls.RangeTrack.MaximumProperty, binding);
public static T Maximum<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.RangeTrack
   => control._set(Ursa.Controls.RangeTrack.MaximumProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Maximum<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.RangeTrack
   => control._set(Ursa.Controls.RangeTrack.MaximumProperty, func, onChanged, expression);
public static T Maximum<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeTrack
=> control._setEx(Ursa.Controls.RangeTrack.MaximumProperty, ps, () => control.Maximum = value, bindingMode, converter, bindingSource);
public static T Maximum<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeTrack
=> control._setEx(Ursa.Controls.RangeTrack.MaximumProperty, ps, () => control.Maximum = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T LowerValue<T>(this T control, IBinding binding) where T : Ursa.Controls.RangeTrack
   => control._set(Ursa.Controls.RangeTrack.LowerValueProperty, binding);
public static T LowerValue<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.RangeTrack
   => control._set(Ursa.Controls.RangeTrack.LowerValueProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T LowerValue<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.RangeTrack
   => control._set(Ursa.Controls.RangeTrack.LowerValueProperty, func, onChanged, expression);
public static T LowerValue<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeTrack
=> control._setEx(Ursa.Controls.RangeTrack.LowerValueProperty, ps, () => control.LowerValue = value, bindingMode, converter, bindingSource);
public static T LowerValue<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeTrack
=> control._setEx(Ursa.Controls.RangeTrack.LowerValueProperty, ps, () => control.LowerValue = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T UpperValue<T>(this T control, IBinding binding) where T : Ursa.Controls.RangeTrack
   => control._set(Ursa.Controls.RangeTrack.UpperValueProperty, binding);
public static T UpperValue<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.RangeTrack
   => control._set(Ursa.Controls.RangeTrack.UpperValueProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T UpperValue<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.RangeTrack
   => control._set(Ursa.Controls.RangeTrack.UpperValueProperty, func, onChanged, expression);
public static T UpperValue<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeTrack
=> control._setEx(Ursa.Controls.RangeTrack.UpperValueProperty, ps, () => control.UpperValue = value, bindingMode, converter, bindingSource);
public static T UpperValue<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeTrack
=> control._setEx(Ursa.Controls.RangeTrack.UpperValueProperty, ps, () => control.UpperValue = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Orientation<T>(this T control, IBinding binding) where T : Ursa.Controls.RangeTrack
   => control._set(Ursa.Controls.RangeTrack.OrientationProperty, binding);
public static T Orientation<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.RangeTrack
   => control._set(Ursa.Controls.RangeTrack.OrientationProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Orientation<T>(this T control, Func<Avalonia.Layout.Orientation> func, Action<Avalonia.Layout.Orientation>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.RangeTrack
   => control._set(Ursa.Controls.RangeTrack.OrientationProperty, func, onChanged, expression);
public static T Orientation<T>(this T control, Avalonia.Layout.Orientation value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeTrack
=> control._setEx(Ursa.Controls.RangeTrack.OrientationProperty, ps, () => control.Orientation = value, bindingMode, converter, bindingSource);
public static T Orientation<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Layout.Orientation> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeTrack
=> control._setEx(Ursa.Controls.RangeTrack.OrientationProperty, ps, () => control.Orientation = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T UpperSection<T>(this T control, IBinding binding) where T : Ursa.Controls.RangeTrack
   => control._set(Ursa.Controls.RangeTrack.UpperSectionProperty, binding);
public static T UpperSection<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.RangeTrack
   => control._set(Ursa.Controls.RangeTrack.UpperSectionProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T UpperSection<T>(this T control, Func<Avalonia.Controls.Control> func, Action<Avalonia.Controls.Control>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.RangeTrack
   => control._set(Ursa.Controls.RangeTrack.UpperSectionProperty, func, onChanged, expression);
public static T UpperSection<T>(this T control, Avalonia.Controls.Control value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeTrack
=> control._setEx(Ursa.Controls.RangeTrack.UpperSectionProperty, ps, () => control.UpperSection = value, bindingMode, converter, bindingSource);
public static T UpperSection<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Control> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeTrack
=> control._setEx(Ursa.Controls.RangeTrack.UpperSectionProperty, ps, () => control.UpperSection = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T LowerSection<T>(this T control, IBinding binding) where T : Ursa.Controls.RangeTrack
   => control._set(Ursa.Controls.RangeTrack.LowerSectionProperty, binding);
public static T LowerSection<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.RangeTrack
   => control._set(Ursa.Controls.RangeTrack.LowerSectionProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T LowerSection<T>(this T control, Func<Avalonia.Controls.Control> func, Action<Avalonia.Controls.Control>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.RangeTrack
   => control._set(Ursa.Controls.RangeTrack.LowerSectionProperty, func, onChanged, expression);
public static T LowerSection<T>(this T control, Avalonia.Controls.Control value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeTrack
=> control._setEx(Ursa.Controls.RangeTrack.LowerSectionProperty, ps, () => control.LowerSection = value, bindingMode, converter, bindingSource);
public static T LowerSection<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Control> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeTrack
=> control._setEx(Ursa.Controls.RangeTrack.LowerSectionProperty, ps, () => control.LowerSection = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T InnerSection<T>(this T control, IBinding binding) where T : Ursa.Controls.RangeTrack
   => control._set(Ursa.Controls.RangeTrack.InnerSectionProperty, binding);
public static T InnerSection<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.RangeTrack
   => control._set(Ursa.Controls.RangeTrack.InnerSectionProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T InnerSection<T>(this T control, Func<Avalonia.Controls.Control> func, Action<Avalonia.Controls.Control>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.RangeTrack
   => control._set(Ursa.Controls.RangeTrack.InnerSectionProperty, func, onChanged, expression);
public static T InnerSection<T>(this T control, Avalonia.Controls.Control value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeTrack
=> control._setEx(Ursa.Controls.RangeTrack.InnerSectionProperty, ps, () => control.InnerSection = value, bindingMode, converter, bindingSource);
public static T InnerSection<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Control> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeTrack
=> control._setEx(Ursa.Controls.RangeTrack.InnerSectionProperty, ps, () => control.InnerSection = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T TrackBackground<T>(this T control, IBinding binding) where T : Ursa.Controls.RangeTrack
   => control._set(Ursa.Controls.RangeTrack.TrackBackgroundProperty, binding);
public static T TrackBackground<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.RangeTrack
   => control._set(Ursa.Controls.RangeTrack.TrackBackgroundProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T TrackBackground<T>(this T control, Func<Avalonia.Controls.Control> func, Action<Avalonia.Controls.Control>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.RangeTrack
   => control._set(Ursa.Controls.RangeTrack.TrackBackgroundProperty, func, onChanged, expression);
public static T TrackBackground<T>(this T control, Avalonia.Controls.Control value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeTrack
=> control._setEx(Ursa.Controls.RangeTrack.TrackBackgroundProperty, ps, () => control.TrackBackground = value, bindingMode, converter, bindingSource);
public static T TrackBackground<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Control> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeTrack
=> control._setEx(Ursa.Controls.RangeTrack.TrackBackgroundProperty, ps, () => control.TrackBackground = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T UpperThumb<T>(this T control, IBinding binding) where T : Ursa.Controls.RangeTrack
   => control._set(Ursa.Controls.RangeTrack.UpperThumbProperty, binding);
public static T UpperThumb<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.RangeTrack
   => control._set(Ursa.Controls.RangeTrack.UpperThumbProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T UpperThumb<T>(this T control, Func<Avalonia.Controls.Primitives.Thumb> func, Action<Avalonia.Controls.Primitives.Thumb>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.RangeTrack
   => control._set(Ursa.Controls.RangeTrack.UpperThumbProperty, func, onChanged, expression);
public static T UpperThumb<T>(this T control, Avalonia.Controls.Primitives.Thumb value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeTrack
=> control._setEx(Ursa.Controls.RangeTrack.UpperThumbProperty, ps, () => control.UpperThumb = value, bindingMode, converter, bindingSource);
public static T UpperThumb<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Primitives.Thumb> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeTrack
=> control._setEx(Ursa.Controls.RangeTrack.UpperThumbProperty, ps, () => control.UpperThumb = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T LowerThumb<T>(this T control, IBinding binding) where T : Ursa.Controls.RangeTrack
   => control._set(Ursa.Controls.RangeTrack.LowerThumbProperty, binding);
public static T LowerThumb<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.RangeTrack
   => control._set(Ursa.Controls.RangeTrack.LowerThumbProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T LowerThumb<T>(this T control, Func<Avalonia.Controls.Primitives.Thumb> func, Action<Avalonia.Controls.Primitives.Thumb>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.RangeTrack
   => control._set(Ursa.Controls.RangeTrack.LowerThumbProperty, func, onChanged, expression);
public static T LowerThumb<T>(this T control, Avalonia.Controls.Primitives.Thumb value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeTrack
=> control._setEx(Ursa.Controls.RangeTrack.LowerThumbProperty, ps, () => control.LowerThumb = value, bindingMode, converter, bindingSource);
public static T LowerThumb<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Primitives.Thumb> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeTrack
=> control._setEx(Ursa.Controls.RangeTrack.LowerThumbProperty, ps, () => control.LowerThumb = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsDirectionReversed<T>(this T control, IBinding binding) where T : Ursa.Controls.RangeTrack
   => control._set(Ursa.Controls.RangeTrack.IsDirectionReversedProperty, binding);
public static T IsDirectionReversed<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.RangeTrack
   => control._set(Ursa.Controls.RangeTrack.IsDirectionReversedProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsDirectionReversed<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.RangeTrack
   => control._set(Ursa.Controls.RangeTrack.IsDirectionReversedProperty, func, onChanged, expression);
public static T IsDirectionReversed<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeTrack
=> control._setEx(Ursa.Controls.RangeTrack.IsDirectionReversedProperty, ps, () => control.IsDirectionReversed = value, bindingMode, converter, bindingSource);
public static T IsDirectionReversed<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeTrack
=> control._setEx(Ursa.Controls.RangeTrack.IsDirectionReversedProperty, ps, () => control.IsDirectionReversed = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

