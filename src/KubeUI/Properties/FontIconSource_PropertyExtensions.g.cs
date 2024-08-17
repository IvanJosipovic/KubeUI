#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using FontIconSource = FluentAvalonia.UI.Controls.FontIconSource;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class FontIconSourceExtensions
{
public static T FontFamily<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FontIconSource
   => control._set(FluentAvalonia.UI.Controls.FontIconSource.FontFamilyProperty, binding);
public static T FontFamily<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FontIconSource
   => control._set(FluentAvalonia.UI.Controls.FontIconSource.FontFamilyProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T FontFamily<T>(this T control, Func<Avalonia.Media.FontFamily> func, Action<Avalonia.Media.FontFamily>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FontIconSource
   => control._set(FluentAvalonia.UI.Controls.FontIconSource.FontFamilyProperty, func, onChanged, expression);
public static T FontFamily<T>(this T control, Avalonia.Media.FontFamily value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FontIconSource
=> control._setEx(FluentAvalonia.UI.Controls.FontIconSource.FontFamilyProperty, ps, () => control.FontFamily = value, bindingMode, converter, bindingSource);
public static T FontFamily<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.FontFamily> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FontIconSource
=> control._setEx(FluentAvalonia.UI.Controls.FontIconSource.FontFamilyProperty, ps, () => control.FontFamily = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T FontSize<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FontIconSource
   => control._set(FluentAvalonia.UI.Controls.FontIconSource.FontSizeProperty, binding);
public static T FontSize<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FontIconSource
   => control._set(FluentAvalonia.UI.Controls.FontIconSource.FontSizeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T FontSize<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FontIconSource
   => control._set(FluentAvalonia.UI.Controls.FontIconSource.FontSizeProperty, func, onChanged, expression);
public static T FontSize<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FontIconSource
=> control._setEx(FluentAvalonia.UI.Controls.FontIconSource.FontSizeProperty, ps, () => control.FontSize = value, bindingMode, converter, bindingSource);
public static T FontSize<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FontIconSource
=> control._setEx(FluentAvalonia.UI.Controls.FontIconSource.FontSizeProperty, ps, () => control.FontSize = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T FontWeight<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FontIconSource
   => control._set(FluentAvalonia.UI.Controls.FontIconSource.FontWeightProperty, binding);
public static T FontWeight<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FontIconSource
   => control._set(FluentAvalonia.UI.Controls.FontIconSource.FontWeightProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T FontWeight<T>(this T control, Func<Avalonia.Media.FontWeight> func, Action<Avalonia.Media.FontWeight>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FontIconSource
   => control._set(FluentAvalonia.UI.Controls.FontIconSource.FontWeightProperty, func, onChanged, expression);
public static T FontWeight<T>(this T control, Avalonia.Media.FontWeight value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FontIconSource
=> control._setEx(FluentAvalonia.UI.Controls.FontIconSource.FontWeightProperty, ps, () => control.FontWeight = value, bindingMode, converter, bindingSource);
public static T FontWeight<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.FontWeight> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FontIconSource
=> control._setEx(FluentAvalonia.UI.Controls.FontIconSource.FontWeightProperty, ps, () => control.FontWeight = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T FontStyle<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FontIconSource
   => control._set(FluentAvalonia.UI.Controls.FontIconSource.FontStyleProperty, binding);
public static T FontStyle<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FontIconSource
   => control._set(FluentAvalonia.UI.Controls.FontIconSource.FontStyleProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T FontStyle<T>(this T control, Func<Avalonia.Media.FontStyle> func, Action<Avalonia.Media.FontStyle>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FontIconSource
   => control._set(FluentAvalonia.UI.Controls.FontIconSource.FontStyleProperty, func, onChanged, expression);
public static T FontStyle<T>(this T control, Avalonia.Media.FontStyle value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FontIconSource
=> control._setEx(FluentAvalonia.UI.Controls.FontIconSource.FontStyleProperty, ps, () => control.FontStyle = value, bindingMode, converter, bindingSource);
public static T FontStyle<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.FontStyle> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FontIconSource
=> control._setEx(FluentAvalonia.UI.Controls.FontIconSource.FontStyleProperty, ps, () => control.FontStyle = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Glyph<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FontIconSource
   => control._set(FluentAvalonia.UI.Controls.FontIconSource.GlyphProperty, binding);
public static T Glyph<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FontIconSource
   => control._set(FluentAvalonia.UI.Controls.FontIconSource.GlyphProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Glyph<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FontIconSource
   => control._set(FluentAvalonia.UI.Controls.FontIconSource.GlyphProperty, func, onChanged, expression);
public static T Glyph<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FontIconSource
=> control._setEx(FluentAvalonia.UI.Controls.FontIconSource.GlyphProperty, ps, () => control.Glyph = value, bindingMode, converter, bindingSource);
public static T Glyph<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FontIconSource
=> control._setEx(FluentAvalonia.UI.Controls.FontIconSource.GlyphProperty, ps, () => control.Glyph = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

