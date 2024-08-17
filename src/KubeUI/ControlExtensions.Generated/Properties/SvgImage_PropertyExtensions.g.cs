#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Svg.Skia;
using SvgImage = Avalonia.Svg.Skia.SvgImage;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class SvgImageExtensions
{
public static T Source<T>(this T control, IBinding binding) where T : Avalonia.Svg.Skia.SvgImage
   => control._set(Avalonia.Svg.Skia.SvgImage.SourceProperty, binding);
public static T Source<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Svg.Skia.SvgImage
   => control._set(Avalonia.Svg.Skia.SvgImage.SourceProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Source<T>(this T control, Func<Avalonia.Svg.Skia.SvgSource> func, Action<Avalonia.Svg.Skia.SvgSource>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Svg.Skia.SvgImage
   => control._set(Avalonia.Svg.Skia.SvgImage.SourceProperty, func, onChanged, expression);
public static T Source<T>(this T control, Avalonia.Svg.Skia.SvgSource value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Svg.Skia.SvgImage
=> control._setEx(Avalonia.Svg.Skia.SvgImage.SourceProperty, ps, () => control.Source = value, bindingMode, converter, bindingSource);
public static T Source<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Svg.Skia.SvgSource> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Svg.Skia.SvgImage
=> control._setEx(Avalonia.Svg.Skia.SvgImage.SourceProperty, ps, () => control.Source = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Css<T>(this T control, IBinding binding) where T : Avalonia.Svg.Skia.SvgImage
   => control._set(Avalonia.Svg.Skia.SvgImage.CssProperty, binding);
public static T Css<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Svg.Skia.SvgImage
   => control._set(Avalonia.Svg.Skia.SvgImage.CssProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Css<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Svg.Skia.SvgImage
   => control._set(Avalonia.Svg.Skia.SvgImage.CssProperty, func, onChanged, expression);
public static T Css<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Svg.Skia.SvgImage
=> control._setEx(Avalonia.Svg.Skia.SvgImage.CssProperty, ps, () => control.Css = value, bindingMode, converter, bindingSource);
public static T Css<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Svg.Skia.SvgImage
=> control._setEx(Avalonia.Svg.Skia.SvgImage.CssProperty, ps, () => control.Css = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T CurrentCss<T>(this T control, IBinding binding) where T : Avalonia.Svg.Skia.SvgImage
   => control._set(Avalonia.Svg.Skia.SvgImage.CurrentCssProperty, binding);
public static T CurrentCss<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Svg.Skia.SvgImage
   => control._set(Avalonia.Svg.Skia.SvgImage.CurrentCssProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CurrentCss<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Svg.Skia.SvgImage
   => control._set(Avalonia.Svg.Skia.SvgImage.CurrentCssProperty, func, onChanged, expression);
public static T CurrentCss<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Svg.Skia.SvgImage
=> control._setEx(Avalonia.Svg.Skia.SvgImage.CurrentCssProperty, ps, () => control.CurrentCss = value, bindingMode, converter, bindingSource);
public static T CurrentCss<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Svg.Skia.SvgImage
=> control._setEx(Avalonia.Svg.Skia.SvgImage.CurrentCssProperty, ps, () => control.CurrentCss = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

