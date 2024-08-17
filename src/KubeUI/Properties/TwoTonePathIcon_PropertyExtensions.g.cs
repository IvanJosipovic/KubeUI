#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TwoTonePathIcon = Ursa.Controls.TwoTonePathIcon;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class TwoTonePathIconExtensions
{
public static T StrokeBrush<T>(this T control, IBinding binding) where T : Ursa.Controls.TwoTonePathIcon
   => control._set(Ursa.Controls.TwoTonePathIcon.StrokeBrushProperty, binding);
public static T StrokeBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TwoTonePathIcon
   => control._set(Ursa.Controls.TwoTonePathIcon.StrokeBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T StrokeBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TwoTonePathIcon
   => control._set(Ursa.Controls.TwoTonePathIcon.StrokeBrushProperty, func, onChanged, expression);
public static T StrokeBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TwoTonePathIcon
=> control._setEx(Ursa.Controls.TwoTonePathIcon.StrokeBrushProperty, ps, () => control.StrokeBrush = value, bindingMode, converter, bindingSource);
public static T StrokeBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TwoTonePathIcon
=> control._setEx(Ursa.Controls.TwoTonePathIcon.StrokeBrushProperty, ps, () => control.StrokeBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Data<T>(this T control, IBinding binding) where T : Ursa.Controls.TwoTonePathIcon
   => control._set(Ursa.Controls.TwoTonePathIcon.DataProperty, binding);
public static T Data<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TwoTonePathIcon
   => control._set(Ursa.Controls.TwoTonePathIcon.DataProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Data<T>(this T control, Func<Avalonia.Media.Geometry> func, Action<Avalonia.Media.Geometry>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TwoTonePathIcon
   => control._set(Ursa.Controls.TwoTonePathIcon.DataProperty, func, onChanged, expression);
public static T Data<T>(this T control, Avalonia.Media.Geometry value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TwoTonePathIcon
=> control._setEx(Ursa.Controls.TwoTonePathIcon.DataProperty, ps, () => control.Data = value, bindingMode, converter, bindingSource);
public static T Data<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.Geometry> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TwoTonePathIcon
=> control._setEx(Ursa.Controls.TwoTonePathIcon.DataProperty, ps, () => control.Data = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsActive<T>(this T control, IBinding binding) where T : Ursa.Controls.TwoTonePathIcon
   => control._set(Ursa.Controls.TwoTonePathIcon.IsActiveProperty, binding);
public static T IsActive<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TwoTonePathIcon
   => control._set(Ursa.Controls.TwoTonePathIcon.IsActiveProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsActive<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TwoTonePathIcon
   => control._set(Ursa.Controls.TwoTonePathIcon.IsActiveProperty, func, onChanged, expression);
public static T IsActive<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TwoTonePathIcon
=> control._setEx(Ursa.Controls.TwoTonePathIcon.IsActiveProperty, ps, () => control.IsActive = value, bindingMode, converter, bindingSource);
public static T IsActive<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TwoTonePathIcon
=> control._setEx(Ursa.Controls.TwoTonePathIcon.IsActiveProperty, ps, () => control.IsActive = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ActiveForeground<T>(this T control, IBinding binding) where T : Ursa.Controls.TwoTonePathIcon
   => control._set(Ursa.Controls.TwoTonePathIcon.ActiveForegroundProperty, binding);
public static T ActiveForeground<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TwoTonePathIcon
   => control._set(Ursa.Controls.TwoTonePathIcon.ActiveForegroundProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ActiveForeground<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TwoTonePathIcon
   => control._set(Ursa.Controls.TwoTonePathIcon.ActiveForegroundProperty, func, onChanged, expression);
public static T ActiveForeground<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TwoTonePathIcon
=> control._setEx(Ursa.Controls.TwoTonePathIcon.ActiveForegroundProperty, ps, () => control.ActiveForeground = value, bindingMode, converter, bindingSource);
public static T ActiveForeground<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TwoTonePathIcon
=> control._setEx(Ursa.Controls.TwoTonePathIcon.ActiveForegroundProperty, ps, () => control.ActiveForeground = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ActiveStrokeBrush<T>(this T control, IBinding binding) where T : Ursa.Controls.TwoTonePathIcon
   => control._set(Ursa.Controls.TwoTonePathIcon.ActiveStrokeBrushProperty, binding);
public static T ActiveStrokeBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TwoTonePathIcon
   => control._set(Ursa.Controls.TwoTonePathIcon.ActiveStrokeBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ActiveStrokeBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TwoTonePathIcon
   => control._set(Ursa.Controls.TwoTonePathIcon.ActiveStrokeBrushProperty, func, onChanged, expression);
public static T ActiveStrokeBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TwoTonePathIcon
=> control._setEx(Ursa.Controls.TwoTonePathIcon.ActiveStrokeBrushProperty, ps, () => control.ActiveStrokeBrush = value, bindingMode, converter, bindingSource);
public static T ActiveStrokeBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TwoTonePathIcon
=> control._setEx(Ursa.Controls.TwoTonePathIcon.ActiveStrokeBrushProperty, ps, () => control.ActiveStrokeBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T StrokeThickness<T>(this T control, IBinding binding) where T : Ursa.Controls.TwoTonePathIcon
   => control._set(Ursa.Controls.TwoTonePathIcon.StrokeThicknessProperty, binding);
public static T StrokeThickness<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TwoTonePathIcon
   => control._set(Ursa.Controls.TwoTonePathIcon.StrokeThicknessProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T StrokeThickness<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TwoTonePathIcon
   => control._set(Ursa.Controls.TwoTonePathIcon.StrokeThicknessProperty, func, onChanged, expression);
public static T StrokeThickness<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TwoTonePathIcon
=> control._setEx(Ursa.Controls.TwoTonePathIcon.StrokeThicknessProperty, ps, () => control.StrokeThickness = value, bindingMode, converter, bindingSource);
public static T StrokeThickness<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TwoTonePathIcon
=> control._setEx(Ursa.Controls.TwoTonePathIcon.StrokeThicknessProperty, ps, () => control.StrokeThickness = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

