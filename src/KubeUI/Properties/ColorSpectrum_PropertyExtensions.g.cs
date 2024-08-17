#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using ColorSpectrum = FluentAvalonia.UI.Controls.ColorSpectrum;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class ColorSpectrumExtensions
{
public static T Shape<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorSpectrum
   => control._set(FluentAvalonia.UI.Controls.ColorSpectrum.ShapeProperty, binding);
public static T Shape<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorSpectrum
   => control._set(FluentAvalonia.UI.Controls.ColorSpectrum.ShapeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Shape<T>(this T control, Func<FluentAvalonia.UI.Controls.ColorSpectrumShape> func, Action<FluentAvalonia.UI.Controls.ColorSpectrumShape>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorSpectrum
   => control._set(FluentAvalonia.UI.Controls.ColorSpectrum.ShapeProperty, func, onChanged, expression);
public static T Shape<T>(this T control, FluentAvalonia.UI.Controls.ColorSpectrumShape value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorSpectrum
=> control._setEx(FluentAvalonia.UI.Controls.ColorSpectrum.ShapeProperty, ps, () => control.Shape = value, bindingMode, converter, bindingSource);
public static T Shape<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.ColorSpectrumShape> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorSpectrum
=> control._setEx(FluentAvalonia.UI.Controls.ColorSpectrum.ShapeProperty, ps, () => control.Shape = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T BorderBrush<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorSpectrum
   => control._set(FluentAvalonia.UI.Controls.ColorSpectrum.BorderBrushProperty, binding);
public static T BorderBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorSpectrum
   => control._set(FluentAvalonia.UI.Controls.ColorSpectrum.BorderBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T BorderBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorSpectrum
   => control._set(FluentAvalonia.UI.Controls.ColorSpectrum.BorderBrushProperty, func, onChanged, expression);
public static T BorderBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorSpectrum
=> control._setEx(FluentAvalonia.UI.Controls.ColorSpectrum.BorderBrushProperty, ps, () => control.BorderBrush = value, bindingMode, converter, bindingSource);
public static T BorderBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorSpectrum
=> control._setEx(FluentAvalonia.UI.Controls.ColorSpectrum.BorderBrushProperty, ps, () => control.BorderBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T BorderThickness<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorSpectrum
   => control._set(FluentAvalonia.UI.Controls.ColorSpectrum.BorderThicknessProperty, binding);
public static T BorderThickness<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorSpectrum
   => control._set(FluentAvalonia.UI.Controls.ColorSpectrum.BorderThicknessProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T BorderThickness<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorSpectrum
   => control._set(FluentAvalonia.UI.Controls.ColorSpectrum.BorderThicknessProperty, func, onChanged, expression);
public static T BorderThickness<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorSpectrum
=> control._setEx(FluentAvalonia.UI.Controls.ColorSpectrum.BorderThicknessProperty, ps, () => control.BorderThickness = value, bindingMode, converter, bindingSource);
public static T BorderThickness<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorSpectrum
=> control._setEx(FluentAvalonia.UI.Controls.ColorSpectrum.BorderThicknessProperty, ps, () => control.BorderThickness = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

