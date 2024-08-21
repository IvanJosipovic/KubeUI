#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class Clock_MarkupExtensions
{
//================= Properties ======================//
 // TimeProperty

/*BindFromExpressionSetterGenerator*/
public static T Time<T>(this T control, Func<System.DateTime> func, Action<System.DateTime>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.TimeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Time<T>(this T control, System.DateTime value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Clock
=> control._setEx(Ursa.Controls.Clock.TimeProperty, ps, () => control.Time = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Time<T>(this T control, IBinding binding) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.TimeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Time<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.TimeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Time<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.DateTime> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Clock
=> control._setEx(Ursa.Controls.Clock.TimeProperty, ps, () => control.Time = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ShowHourTicksProperty

/*BindFromExpressionSetterGenerator*/
public static T ShowHourTicks<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.ShowHourTicksProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ShowHourTicks<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Clock
=> control._setEx(Ursa.Controls.Clock.ShowHourTicksProperty, ps, () => control.ShowHourTicks = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ShowHourTicks<T>(this T control, IBinding binding) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.ShowHourTicksProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ShowHourTicks<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.ShowHourTicksProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ShowHourTicks<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Clock
=> control._setEx(Ursa.Controls.Clock.ShowHourTicksProperty, ps, () => control.ShowHourTicks = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ShowMinuteTicksProperty

/*BindFromExpressionSetterGenerator*/
public static T ShowMinuteTicks<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.ShowMinuteTicksProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ShowMinuteTicks<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Clock
=> control._setEx(Ursa.Controls.Clock.ShowMinuteTicksProperty, ps, () => control.ShowMinuteTicks = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ShowMinuteTicks<T>(this T control, IBinding binding) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.ShowMinuteTicksProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ShowMinuteTicks<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.ShowMinuteTicksProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ShowMinuteTicks<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Clock
=> control._setEx(Ursa.Controls.Clock.ShowMinuteTicksProperty, ps, () => control.ShowMinuteTicks = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // HandBrushProperty

/*BindFromExpressionSetterGenerator*/
public static T HandBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.HandBrushProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T HandBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Clock
=> control._setEx(Ursa.Controls.Clock.HandBrushProperty, ps, () => control.HandBrush = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T HandBrush<T>(this T control, IBinding binding) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.HandBrushProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T HandBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.HandBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T HandBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Clock
=> control._setEx(Ursa.Controls.Clock.HandBrushProperty, ps, () => control.HandBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ShowHourHandProperty

/*BindFromExpressionSetterGenerator*/
public static T ShowHourHand<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.ShowHourHandProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ShowHourHand<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Clock
=> control._setEx(Ursa.Controls.Clock.ShowHourHandProperty, ps, () => control.ShowHourHand = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ShowHourHand<T>(this T control, IBinding binding) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.ShowHourHandProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ShowHourHand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.ShowHourHandProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ShowHourHand<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Clock
=> control._setEx(Ursa.Controls.Clock.ShowHourHandProperty, ps, () => control.ShowHourHand = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ShowMinuteHandProperty

/*BindFromExpressionSetterGenerator*/
public static T ShowMinuteHand<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.ShowMinuteHandProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ShowMinuteHand<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Clock
=> control._setEx(Ursa.Controls.Clock.ShowMinuteHandProperty, ps, () => control.ShowMinuteHand = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ShowMinuteHand<T>(this T control, IBinding binding) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.ShowMinuteHandProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ShowMinuteHand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.ShowMinuteHandProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ShowMinuteHand<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Clock
=> control._setEx(Ursa.Controls.Clock.ShowMinuteHandProperty, ps, () => control.ShowMinuteHand = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ShowSecondHandProperty

/*BindFromExpressionSetterGenerator*/
public static T ShowSecondHand<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.ShowSecondHandProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ShowSecondHand<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Clock
=> control._setEx(Ursa.Controls.Clock.ShowSecondHandProperty, ps, () => control.ShowSecondHand = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ShowSecondHand<T>(this T control, IBinding binding) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.ShowSecondHandProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ShowSecondHand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.ShowSecondHandProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ShowSecondHand<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Clock
=> control._setEx(Ursa.Controls.Clock.ShowSecondHandProperty, ps, () => control.ShowSecondHand = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsSmoothProperty

/*BindFromExpressionSetterGenerator*/
public static T IsSmooth<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.IsSmoothProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsSmooth<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Clock
=> control._setEx(Ursa.Controls.Clock.IsSmoothProperty, ps, () => control.IsSmooth = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsSmooth<T>(this T control, IBinding binding) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.IsSmoothProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsSmooth<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Clock
   => control._set(Ursa.Controls.Clock.IsSmoothProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsSmooth<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Clock
=> control._setEx(Ursa.Controls.Clock.IsSmoothProperty, ps, () => control.IsSmooth = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // TimeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Time<T>(this Style<T> style, System.DateTime value) where T : Ursa.Controls.Clock
=> style._addSetter(Ursa.Controls.Clock.TimeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Time<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Clock
=> style._addSetter(Ursa.Controls.Clock.TimeProperty, binding);


 // ShowHourTicksProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ShowHourTicks<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.Clock
=> style._addSetter(Ursa.Controls.Clock.ShowHourTicksProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ShowHourTicks<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Clock
=> style._addSetter(Ursa.Controls.Clock.ShowHourTicksProperty, binding);


 // ShowMinuteTicksProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ShowMinuteTicks<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.Clock
=> style._addSetter(Ursa.Controls.Clock.ShowMinuteTicksProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ShowMinuteTicks<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Clock
=> style._addSetter(Ursa.Controls.Clock.ShowMinuteTicksProperty, binding);


 // HandBrushProperty

/*ValueStyleSetterGenerator*/
public static Style<T> HandBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Ursa.Controls.Clock
=> style._addSetter(Ursa.Controls.Clock.HandBrushProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> HandBrush<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Clock
=> style._addSetter(Ursa.Controls.Clock.HandBrushProperty, binding);


 // ShowHourHandProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ShowHourHand<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.Clock
=> style._addSetter(Ursa.Controls.Clock.ShowHourHandProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ShowHourHand<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Clock
=> style._addSetter(Ursa.Controls.Clock.ShowHourHandProperty, binding);


 // ShowMinuteHandProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ShowMinuteHand<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.Clock
=> style._addSetter(Ursa.Controls.Clock.ShowMinuteHandProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ShowMinuteHand<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Clock
=> style._addSetter(Ursa.Controls.Clock.ShowMinuteHandProperty, binding);


 // ShowSecondHandProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ShowSecondHand<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.Clock
=> style._addSetter(Ursa.Controls.Clock.ShowSecondHandProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ShowSecondHand<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Clock
=> style._addSetter(Ursa.Controls.Clock.ShowSecondHandProperty, binding);


 // IsSmoothProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsSmooth<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.Clock
=> style._addSetter(Ursa.Controls.Clock.IsSmoothProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsSmooth<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Clock
=> style._addSetter(Ursa.Controls.Clock.IsSmoothProperty, binding);



}
