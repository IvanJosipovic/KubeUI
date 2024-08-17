#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TimeBox = Ursa.Controls.TimeBox;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class TimeBoxExtensions
{
public static T Time<T>(this T control, IBinding binding) where T : Ursa.Controls.TimeBox
   => control._set(Ursa.Controls.TimeBox.TimeProperty, binding);
public static T Time<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimeBox
   => control._set(Ursa.Controls.TimeBox.TimeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Time<T>(this T control, Func<System.Nullable<System.TimeSpan>> func, Action<System.Nullable<System.TimeSpan>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimeBox
   => control._set(Ursa.Controls.TimeBox.TimeProperty, func, onChanged, expression);
public static T Time<T>(this T control, System.Nullable<System.TimeSpan> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimeBox
=> control._setEx(Ursa.Controls.TimeBox.TimeProperty, ps, () => control.Time = value, bindingMode, converter, bindingSource);
public static T Time<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Nullable<System.TimeSpan>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimeBox
=> control._setEx(Ursa.Controls.TimeBox.TimeProperty, ps, () => control.Time = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T TextAlignment<T>(this T control, IBinding binding) where T : Ursa.Controls.TimeBox
   => control._set(Ursa.Controls.TimeBox.TextAlignmentProperty, binding);
public static T TextAlignment<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimeBox
   => control._set(Ursa.Controls.TimeBox.TextAlignmentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T TextAlignment<T>(this T control, Func<Avalonia.Media.TextAlignment> func, Action<Avalonia.Media.TextAlignment>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimeBox
   => control._set(Ursa.Controls.TimeBox.TextAlignmentProperty, func, onChanged, expression);
public static T TextAlignment<T>(this T control, Avalonia.Media.TextAlignment value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimeBox
=> control._setEx(Ursa.Controls.TimeBox.TextAlignmentProperty, ps, () => control.TextAlignment = value, bindingMode, converter, bindingSource);
public static T TextAlignment<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.TextAlignment> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimeBox
=> control._setEx(Ursa.Controls.TimeBox.TextAlignmentProperty, ps, () => control.TextAlignment = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SelectionBrush<T>(this T control, IBinding binding) where T : Ursa.Controls.TimeBox
   => control._set(Ursa.Controls.TimeBox.SelectionBrushProperty, binding);
public static T SelectionBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimeBox
   => control._set(Ursa.Controls.TimeBox.SelectionBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SelectionBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimeBox
   => control._set(Ursa.Controls.TimeBox.SelectionBrushProperty, func, onChanged, expression);
public static T SelectionBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimeBox
=> control._setEx(Ursa.Controls.TimeBox.SelectionBrushProperty, ps, () => control.SelectionBrush = value, bindingMode, converter, bindingSource);
public static T SelectionBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimeBox
=> control._setEx(Ursa.Controls.TimeBox.SelectionBrushProperty, ps, () => control.SelectionBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SelectionForegroundBrush<T>(this T control, IBinding binding) where T : Ursa.Controls.TimeBox
   => control._set(Ursa.Controls.TimeBox.SelectionForegroundBrushProperty, binding);
public static T SelectionForegroundBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimeBox
   => control._set(Ursa.Controls.TimeBox.SelectionForegroundBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SelectionForegroundBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimeBox
   => control._set(Ursa.Controls.TimeBox.SelectionForegroundBrushProperty, func, onChanged, expression);
public static T SelectionForegroundBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimeBox
=> control._setEx(Ursa.Controls.TimeBox.SelectionForegroundBrushProperty, ps, () => control.SelectionForegroundBrush = value, bindingMode, converter, bindingSource);
public static T SelectionForegroundBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimeBox
=> control._setEx(Ursa.Controls.TimeBox.SelectionForegroundBrushProperty, ps, () => control.SelectionForegroundBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T CaretBrush<T>(this T control, IBinding binding) where T : Ursa.Controls.TimeBox
   => control._set(Ursa.Controls.TimeBox.CaretBrushProperty, binding);
public static T CaretBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimeBox
   => control._set(Ursa.Controls.TimeBox.CaretBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CaretBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimeBox
   => control._set(Ursa.Controls.TimeBox.CaretBrushProperty, func, onChanged, expression);
public static T CaretBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimeBox
=> control._setEx(Ursa.Controls.TimeBox.CaretBrushProperty, ps, () => control.CaretBrush = value, bindingMode, converter, bindingSource);
public static T CaretBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimeBox
=> control._setEx(Ursa.Controls.TimeBox.CaretBrushProperty, ps, () => control.CaretBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ShowLeadingZero<T>(this T control, IBinding binding) where T : Ursa.Controls.TimeBox
   => control._set(Ursa.Controls.TimeBox.ShowLeadingZeroProperty, binding);
public static T ShowLeadingZero<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimeBox
   => control._set(Ursa.Controls.TimeBox.ShowLeadingZeroProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ShowLeadingZero<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimeBox
   => control._set(Ursa.Controls.TimeBox.ShowLeadingZeroProperty, func, onChanged, expression);
public static T ShowLeadingZero<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimeBox
=> control._setEx(Ursa.Controls.TimeBox.ShowLeadingZeroProperty, ps, () => control.ShowLeadingZero = value, bindingMode, converter, bindingSource);
public static T ShowLeadingZero<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimeBox
=> control._setEx(Ursa.Controls.TimeBox.ShowLeadingZeroProperty, ps, () => control.ShowLeadingZero = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T InputMode<T>(this T control, IBinding binding) where T : Ursa.Controls.TimeBox
   => control._set(Ursa.Controls.TimeBox.InputModeProperty, binding);
public static T InputMode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimeBox
   => control._set(Ursa.Controls.TimeBox.InputModeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T InputMode<T>(this T control, Func<Ursa.Controls.TimeBoxInputMode> func, Action<Ursa.Controls.TimeBoxInputMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimeBox
   => control._set(Ursa.Controls.TimeBox.InputModeProperty, func, onChanged, expression);
public static T InputMode<T>(this T control, Ursa.Controls.TimeBoxInputMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimeBox
=> control._setEx(Ursa.Controls.TimeBox.InputModeProperty, ps, () => control.InputMode = value, bindingMode, converter, bindingSource);
public static T InputMode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Ursa.Controls.TimeBoxInputMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimeBox
=> control._setEx(Ursa.Controls.TimeBox.InputModeProperty, ps, () => control.InputMode = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T AllowDrag<T>(this T control, IBinding binding) where T : Ursa.Controls.TimeBox
   => control._set(Ursa.Controls.TimeBox.AllowDragProperty, binding);
public static T AllowDrag<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimeBox
   => control._set(Ursa.Controls.TimeBox.AllowDragProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T AllowDrag<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimeBox
   => control._set(Ursa.Controls.TimeBox.AllowDragProperty, func, onChanged, expression);
public static T AllowDrag<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimeBox
=> control._setEx(Ursa.Controls.TimeBox.AllowDragProperty, ps, () => control.AllowDrag = value, bindingMode, converter, bindingSource);
public static T AllowDrag<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimeBox
=> control._setEx(Ursa.Controls.TimeBox.AllowDragProperty, ps, () => control.AllowDrag = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T DragOrientation<T>(this T control, IBinding binding) where T : Ursa.Controls.TimeBox
   => control._set(Ursa.Controls.TimeBox.DragOrientationProperty, binding);
public static T DragOrientation<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimeBox
   => control._set(Ursa.Controls.TimeBox.DragOrientationProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T DragOrientation<T>(this T control, Func<Ursa.Controls.TimeBoxDragOrientation> func, Action<Ursa.Controls.TimeBoxDragOrientation>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimeBox
   => control._set(Ursa.Controls.TimeBox.DragOrientationProperty, func, onChanged, expression);
public static T DragOrientation<T>(this T control, Ursa.Controls.TimeBoxDragOrientation value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimeBox
=> control._setEx(Ursa.Controls.TimeBox.DragOrientationProperty, ps, () => control.DragOrientation = value, bindingMode, converter, bindingSource);
public static T DragOrientation<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Ursa.Controls.TimeBoxDragOrientation> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimeBox
=> control._setEx(Ursa.Controls.TimeBox.DragOrientationProperty, ps, () => control.DragOrientation = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsTimeLoop<T>(this T control, IBinding binding) where T : Ursa.Controls.TimeBox
   => control._set(Ursa.Controls.TimeBox.IsTimeLoopProperty, binding);
public static T IsTimeLoop<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimeBox
   => control._set(Ursa.Controls.TimeBox.IsTimeLoopProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsTimeLoop<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimeBox
   => control._set(Ursa.Controls.TimeBox.IsTimeLoopProperty, func, onChanged, expression);
public static T IsTimeLoop<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimeBox
=> control._setEx(Ursa.Controls.TimeBox.IsTimeLoopProperty, ps, () => control.IsTimeLoop = value, bindingMode, converter, bindingSource);
public static T IsTimeLoop<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimeBox
=> control._setEx(Ursa.Controls.TimeBox.IsTimeLoopProperty, ps, () => control.IsTimeLoop = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

