#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Svg.Skia;
using Svg = Avalonia.Svg.Skia.Svg;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class SvgExtensions
{
public static T Path<T>(this T control, IBinding binding) where T : Avalonia.Svg.Skia.Svg
   => control._set(Avalonia.Svg.Skia.Svg.PathProperty, binding);
public static T Path<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Svg.Skia.Svg
   => control._set(Avalonia.Svg.Skia.Svg.PathProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Path<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Svg.Skia.Svg
   => control._set(Avalonia.Svg.Skia.Svg.PathProperty, func, onChanged, expression);
public static T Path<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Svg.Skia.Svg
=> control._setEx(Avalonia.Svg.Skia.Svg.PathProperty, ps, () => control.Path = value, bindingMode, converter, bindingSource);
public static T Path<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Svg.Skia.Svg
=> control._setEx(Avalonia.Svg.Skia.Svg.PathProperty, ps, () => control.Path = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Source<T>(this T control, IBinding binding) where T : Avalonia.Svg.Skia.Svg
   => control._set(Avalonia.Svg.Skia.Svg.SourceProperty, binding);
public static T Source<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Svg.Skia.Svg
   => control._set(Avalonia.Svg.Skia.Svg.SourceProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Source<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Svg.Skia.Svg
   => control._set(Avalonia.Svg.Skia.Svg.SourceProperty, func, onChanged, expression);
public static T Source<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Svg.Skia.Svg
=> control._setEx(Avalonia.Svg.Skia.Svg.SourceProperty, ps, () => control.Source = value, bindingMode, converter, bindingSource);
public static T Source<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Svg.Skia.Svg
=> control._setEx(Avalonia.Svg.Skia.Svg.SourceProperty, ps, () => control.Source = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Stretch<T>(this T control, IBinding binding) where T : Avalonia.Svg.Skia.Svg
   => control._set(Avalonia.Svg.Skia.Svg.StretchProperty, binding);
public static T Stretch<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Svg.Skia.Svg
   => control._set(Avalonia.Svg.Skia.Svg.StretchProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Stretch<T>(this T control, Func<Avalonia.Media.Stretch> func, Action<Avalonia.Media.Stretch>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Svg.Skia.Svg
   => control._set(Avalonia.Svg.Skia.Svg.StretchProperty, func, onChanged, expression);
public static T Stretch<T>(this T control, Avalonia.Media.Stretch value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Svg.Skia.Svg
=> control._setEx(Avalonia.Svg.Skia.Svg.StretchProperty, ps, () => control.Stretch = value, bindingMode, converter, bindingSource);
public static T Stretch<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.Stretch> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Svg.Skia.Svg
=> control._setEx(Avalonia.Svg.Skia.Svg.StretchProperty, ps, () => control.Stretch = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T StretchDirection<T>(this T control, IBinding binding) where T : Avalonia.Svg.Skia.Svg
   => control._set(Avalonia.Svg.Skia.Svg.StretchDirectionProperty, binding);
public static T StretchDirection<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Svg.Skia.Svg
   => control._set(Avalonia.Svg.Skia.Svg.StretchDirectionProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T StretchDirection<T>(this T control, Func<Avalonia.Media.StretchDirection> func, Action<Avalonia.Media.StretchDirection>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Svg.Skia.Svg
   => control._set(Avalonia.Svg.Skia.Svg.StretchDirectionProperty, func, onChanged, expression);
public static T StretchDirection<T>(this T control, Avalonia.Media.StretchDirection value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Svg.Skia.Svg
=> control._setEx(Avalonia.Svg.Skia.Svg.StretchDirectionProperty, ps, () => control.StretchDirection = value, bindingMode, converter, bindingSource);
public static T StretchDirection<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.StretchDirection> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Svg.Skia.Svg
=> control._setEx(Avalonia.Svg.Skia.Svg.StretchDirectionProperty, ps, () => control.StretchDirection = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T EnableCache<T>(this T control, IBinding binding) where T : Avalonia.Svg.Skia.Svg
   => control._set(Avalonia.Svg.Skia.Svg.EnableCacheProperty, binding);
public static T EnableCache<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Svg.Skia.Svg
   => control._set(Avalonia.Svg.Skia.Svg.EnableCacheProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T EnableCache<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Svg.Skia.Svg
   => control._set(Avalonia.Svg.Skia.Svg.EnableCacheProperty, func, onChanged, expression);
public static T EnableCache<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Svg.Skia.Svg
=> control._setEx(Avalonia.Svg.Skia.Svg.EnableCacheProperty, ps, () => control.EnableCache = value, bindingMode, converter, bindingSource);
public static T EnableCache<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Svg.Skia.Svg
=> control._setEx(Avalonia.Svg.Skia.Svg.EnableCacheProperty, ps, () => control.EnableCache = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

