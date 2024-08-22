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
public static partial class RangeSlider_MarkupExtensions
{
//================= Properties ======================//
 // MinimumProperty

/*BindFromExpressionSetterGenerator*/
public static T Minimum<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.MinimumProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Minimum<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.MinimumProperty, ps, () => control.Minimum = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Minimum<T>(this T control, IBinding binding) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.MinimumProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Minimum<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.MinimumProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Minimum<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.MinimumProperty, ps, () => control.Minimum = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // MaximumProperty

/*BindFromExpressionSetterGenerator*/
public static T Maximum<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.MaximumProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Maximum<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.MaximumProperty, ps, () => control.Maximum = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Maximum<T>(this T control, IBinding binding) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.MaximumProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Maximum<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.MaximumProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Maximum<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.MaximumProperty, ps, () => control.Maximum = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // LowerValueProperty

/*BindFromExpressionSetterGenerator*/
public static T LowerValue<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.LowerValueProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T LowerValue<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.LowerValueProperty, ps, () => control.LowerValue = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T LowerValue<T>(this T control, IBinding binding) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.LowerValueProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T LowerValue<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.LowerValueProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T LowerValue<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.LowerValueProperty, ps, () => control.LowerValue = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // UpperValueProperty

/*BindFromExpressionSetterGenerator*/
public static T UpperValue<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.UpperValueProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T UpperValue<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.UpperValueProperty, ps, () => control.UpperValue = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T UpperValue<T>(this T control, IBinding binding) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.UpperValueProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T UpperValue<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.UpperValueProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T UpperValue<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.UpperValueProperty, ps, () => control.UpperValue = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TrackWidthProperty

/*BindFromExpressionSetterGenerator*/
public static T TrackWidth<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.TrackWidthProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T TrackWidth<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.TrackWidthProperty, ps, () => control.TrackWidth = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TrackWidth<T>(this T control, IBinding binding) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.TrackWidthProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TrackWidth<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.TrackWidthProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T TrackWidth<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.TrackWidthProperty, ps, () => control.TrackWidth = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // OrientationProperty

/*BindFromExpressionSetterGenerator*/
public static T Orientation<T>(this T control, Func<Avalonia.Layout.Orientation> func, Action<Avalonia.Layout.Orientation>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.OrientationProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Orientation<T>(this T control, Avalonia.Layout.Orientation value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.OrientationProperty, ps, () => control.Orientation = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Orientation<T>(this T control, IBinding binding) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.OrientationProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Orientation<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.OrientationProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Orientation<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Layout.Orientation> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.OrientationProperty, ps, () => control.Orientation = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsDirectionReversedProperty

/*BindFromExpressionSetterGenerator*/
public static T IsDirectionReversed<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.IsDirectionReversedProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsDirectionReversed<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.IsDirectionReversedProperty, ps, () => control.IsDirectionReversed = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsDirectionReversed<T>(this T control, IBinding binding) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.IsDirectionReversedProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsDirectionReversed<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.IsDirectionReversedProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsDirectionReversed<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.IsDirectionReversedProperty, ps, () => control.IsDirectionReversed = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TickFrequencyProperty

/*BindFromExpressionSetterGenerator*/
public static T TickFrequency<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.TickFrequencyProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T TickFrequency<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.TickFrequencyProperty, ps, () => control.TickFrequency = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TickFrequency<T>(this T control, IBinding binding) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.TickFrequencyProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TickFrequency<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.TickFrequencyProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T TickFrequency<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.TickFrequencyProperty, ps, () => control.TickFrequency = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TicksProperty

/*BindFromExpressionSetterGenerator*/
public static T Ticks<T>(this T control, Func<Avalonia.Collections.AvaloniaList<System.Double>> func, Action<Avalonia.Collections.AvaloniaList<System.Double>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.TicksProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Ticks<T>(this T control, Avalonia.Collections.AvaloniaList<System.Double> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.TicksProperty, ps, () => control.Ticks = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Ticks<T>(this T control, IBinding binding) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.TicksProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Ticks<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.TicksProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Ticks<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Collections.AvaloniaList<System.Double>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.TicksProperty, ps, () => control.Ticks = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TickPlacementProperty

/*BindFromExpressionSetterGenerator*/
public static T TickPlacement<T>(this T control, Func<Avalonia.Controls.TickPlacement> func, Action<Avalonia.Controls.TickPlacement>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.TickPlacementProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T TickPlacement<T>(this T control, Avalonia.Controls.TickPlacement value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.TickPlacementProperty, ps, () => control.TickPlacement = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TickPlacement<T>(this T control, IBinding binding) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.TickPlacementProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TickPlacement<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.TickPlacementProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T TickPlacement<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.TickPlacement> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.TickPlacementProperty, ps, () => control.TickPlacement = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsSnapToTickProperty

/*BindFromExpressionSetterGenerator*/
public static T IsSnapToTick<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.IsSnapToTickProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsSnapToTick<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.IsSnapToTickProperty, ps, () => control.IsSnapToTick = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsSnapToTick<T>(this T control, IBinding binding) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.IsSnapToTickProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsSnapToTick<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.RangeSlider
   => control._set(Ursa.Controls.RangeSlider.IsSnapToTickProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsSnapToTick<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RangeSlider
=> control._setEx(Ursa.Controls.RangeSlider.IsSnapToTickProperty, ps, () => control.IsSnapToTick = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // ValueChanged

/*ActionToEventGenerator*/
    public static T OnValueChanged<T>(this T control, Action<Ursa.Controls.RangeValueChangedEventArgs> action) where T : Ursa.Controls.RangeSlider => 
        control._setEvent((System.EventHandler<Ursa.Controls.RangeValueChangedEventArgs>) ((arg0, arg1) => action(arg1)), h => control.ValueChanged += h);



//================= Styles ======================//
 // MinimumProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Minimum<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.MinimumProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Minimum<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.MinimumProperty, binding);


 // MaximumProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Maximum<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.MaximumProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Maximum<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.MaximumProperty, binding);


 // LowerValueProperty

/*ValueStyleSetterGenerator*/
public static Style<T> LowerValue<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.LowerValueProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> LowerValue<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.LowerValueProperty, binding);


 // UpperValueProperty

/*ValueStyleSetterGenerator*/
public static Style<T> UpperValue<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.UpperValueProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> UpperValue<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.UpperValueProperty, binding);


 // TrackWidthProperty

/*ValueStyleSetterGenerator*/
public static Style<T> TrackWidth<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.TrackWidthProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> TrackWidth<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.TrackWidthProperty, binding);


 // OrientationProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Orientation<T>(this Style<T> style, Avalonia.Layout.Orientation value) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.OrientationProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Orientation<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.OrientationProperty, binding);


 // IsDirectionReversedProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsDirectionReversed<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.IsDirectionReversedProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsDirectionReversed<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.IsDirectionReversedProperty, binding);


 // TickFrequencyProperty

/*ValueStyleSetterGenerator*/
public static Style<T> TickFrequency<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.TickFrequencyProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> TickFrequency<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.TickFrequencyProperty, binding);


 // TicksProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Ticks<T>(this Style<T> style, Avalonia.Collections.AvaloniaList<System.Double> value) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.TicksProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Ticks<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.TicksProperty, binding);


 // TickPlacementProperty

/*ValueStyleSetterGenerator*/
public static Style<T> TickPlacement<T>(this Style<T> style, Avalonia.Controls.TickPlacement value) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.TickPlacementProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> TickPlacement<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.TickPlacementProperty, binding);


 // IsSnapToTickProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsSnapToTick<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.IsSnapToTickProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsSnapToTick<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.IsSnapToTickProperty, binding);



}
