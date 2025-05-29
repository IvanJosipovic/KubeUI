#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
[global::System.CodeDom.Compiler.GeneratedCode("AvaloniaExtensionGenerator", "1.0.0.0")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static partial class Svg_MarkupExtensions
{
//================= Properties ======================//
 // Path

/*ValueSetterGenerator*/
public static T Path<T>(this T control, System.String value) where T : Avalonia.Svg.Skia.Svg 
=> control._set(() => control.Path = value!);

/*BindFromExpressionSetterGenerator*/
public static T Path<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : Avalonia.Svg.Skia.Svg 
   => control._set(Avalonia.Svg.Skia.Svg.PathProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T Path<T>(this T control,System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Avalonia.Svg.Skia.Svg 
=> control._setEx(Avalonia.Svg.Skia.Svg.PathProperty, ps, () => control.Path = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Path<T>(this T control, IBinding binding) where T : Avalonia.Svg.Skia.Svg 
   => control._set(Avalonia.Svg.Skia.Svg.PathProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Path<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Svg.Skia.Svg 
   => control._set(Avalonia.Svg.Skia.Svg.PathProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T Path<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Avalonia.Svg.Skia.Svg 
=> control._setEx(Avalonia.Svg.Skia.Svg.PathProperty, ps, () => control.Path = converter.TryConvert(value)!, bindingMode, converter, bindingSource);


 // Source

/*ValueSetterGenerator*/
public static T Source<T>(this T control, System.String value) where T : Avalonia.Svg.Skia.Svg 
=> control._set(() => control.Source = value!);

/*BindFromExpressionSetterGenerator*/
public static T Source<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : Avalonia.Svg.Skia.Svg 
   => control._set(Avalonia.Svg.Skia.Svg.SourceProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T Source<T>(this T control,System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Avalonia.Svg.Skia.Svg 
=> control._setEx(Avalonia.Svg.Skia.Svg.SourceProperty, ps, () => control.Source = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Source<T>(this T control, IBinding binding) where T : Avalonia.Svg.Skia.Svg 
   => control._set(Avalonia.Svg.Skia.Svg.SourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Source<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Svg.Skia.Svg 
   => control._set(Avalonia.Svg.Skia.Svg.SourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T Source<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Avalonia.Svg.Skia.Svg 
=> control._setEx(Avalonia.Svg.Skia.Svg.SourceProperty, ps, () => control.Source = converter.TryConvert(value)!, bindingMode, converter, bindingSource);


 // Stretch

/*ValueSetterGenerator*/
public static T Stretch<T>(this T control, Avalonia.Media.Stretch value) where T : Avalonia.Svg.Skia.Svg 
=> control._set(() => control.Stretch = value!);

/*BindFromExpressionSetterGenerator*/
public static T Stretch<T>(this T control, Func<Avalonia.Media.Stretch> func, Action<Avalonia.Media.Stretch>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : Avalonia.Svg.Skia.Svg 
   => control._set(Avalonia.Svg.Skia.Svg.StretchProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T Stretch<T>(this T control,Avalonia.Media.Stretch value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Avalonia.Svg.Skia.Svg 
=> control._setEx(Avalonia.Svg.Skia.Svg.StretchProperty, ps, () => control.Stretch = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Stretch<T>(this T control, IBinding binding) where T : Avalonia.Svg.Skia.Svg 
   => control._set(Avalonia.Svg.Skia.Svg.StretchProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Stretch<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Svg.Skia.Svg 
   => control._set(Avalonia.Svg.Skia.Svg.StretchProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T Stretch<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.Stretch> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Avalonia.Svg.Skia.Svg 
=> control._setEx(Avalonia.Svg.Skia.Svg.StretchProperty, ps, () => control.Stretch = converter.TryConvert(value)!, bindingMode, converter, bindingSource);


 // StretchDirection

/*ValueSetterGenerator*/
public static T StretchDirection<T>(this T control, Avalonia.Media.StretchDirection value) where T : Avalonia.Svg.Skia.Svg 
=> control._set(() => control.StretchDirection = value!);

/*BindFromExpressionSetterGenerator*/
public static T StretchDirection<T>(this T control, Func<Avalonia.Media.StretchDirection> func, Action<Avalonia.Media.StretchDirection>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : Avalonia.Svg.Skia.Svg 
   => control._set(Avalonia.Svg.Skia.Svg.StretchDirectionProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T StretchDirection<T>(this T control,Avalonia.Media.StretchDirection value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Avalonia.Svg.Skia.Svg 
=> control._setEx(Avalonia.Svg.Skia.Svg.StretchDirectionProperty, ps, () => control.StretchDirection = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T StretchDirection<T>(this T control, IBinding binding) where T : Avalonia.Svg.Skia.Svg 
   => control._set(Avalonia.Svg.Skia.Svg.StretchDirectionProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T StretchDirection<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Svg.Skia.Svg 
   => control._set(Avalonia.Svg.Skia.Svg.StretchDirectionProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T StretchDirection<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.StretchDirection> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Avalonia.Svg.Skia.Svg 
=> control._setEx(Avalonia.Svg.Skia.Svg.StretchDirectionProperty, ps, () => control.StretchDirection = converter.TryConvert(value)!, bindingMode, converter, bindingSource);


 // EnableCache

/*ValueSetterGenerator*/
public static T EnableCache<T>(this T control, System.Boolean value) where T : Avalonia.Svg.Skia.Svg 
=> control._set(() => control.EnableCache = value!);

/*BindFromExpressionSetterGenerator*/
public static T EnableCache<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : Avalonia.Svg.Skia.Svg 
   => control._set(Avalonia.Svg.Skia.Svg.EnableCacheProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T EnableCache<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Avalonia.Svg.Skia.Svg 
=> control._setEx(Avalonia.Svg.Skia.Svg.EnableCacheProperty, ps, () => control.EnableCache = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T EnableCache<T>(this T control, IBinding binding) where T : Avalonia.Svg.Skia.Svg 
   => control._set(Avalonia.Svg.Skia.Svg.EnableCacheProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T EnableCache<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Svg.Skia.Svg 
   => control._set(Avalonia.Svg.Skia.Svg.EnableCacheProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T EnableCache<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Avalonia.Svg.Skia.Svg 
=> control._setEx(Avalonia.Svg.Skia.Svg.EnableCacheProperty, ps, () => control.EnableCache = converter.TryConvert(value)!, bindingMode, converter, bindingSource);



//================= Attached Properties ======================//
 // Css

/*AttachedPropertyMagicalSetterGenerator*/
public static T Svg_Css<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Avalonia.AvaloniaObject
 => control._setEx(Avalonia.Svg.Skia.Svg.CssProperty, ps, () => Avalonia.Svg.Skia.Svg.SetCss(control, value), bindingMode, converter, bindingSource);

/*AttachedPropertyBindFromExpressionSetterGenerator*/
public static T Svg_Css<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : Avalonia.AvaloniaObject 
   => control._set(Avalonia.Svg.Skia.Svg.CssProperty!, func, onChanged, expression);


 // CurrentCss

/*AttachedPropertyMagicalSetterGenerator*/
public static T Svg_CurrentCss<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Avalonia.AvaloniaObject
 => control._setEx(Avalonia.Svg.Skia.Svg.CurrentCssProperty, ps, () => Avalonia.Svg.Skia.Svg.SetCurrentCss(control, value), bindingMode, converter, bindingSource);

/*AttachedPropertyBindFromExpressionSetterGenerator*/
public static T Svg_CurrentCss<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : Avalonia.AvaloniaObject 
   => control._set(Avalonia.Svg.Skia.Svg.CurrentCssProperty!, func, onChanged, expression);



//================= Styles ======================//
 // Path

/*ValueStyleSetterGenerator*/
public static Style<T> Path<T>(this Style<T> style, System.String value) where T : Avalonia.Svg.Skia.Svg 
=> style._addSetter(Avalonia.Svg.Skia.Svg.PathProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> Path<T>(this Style<T> style, IBinding binding) where T : Avalonia.Svg.Skia.Svg 
=> style._addSetter(Avalonia.Svg.Skia.Svg.PathProperty, binding);


 // Source

/*ValueStyleSetterGenerator*/
public static Style<T> Source<T>(this Style<T> style, System.String value) where T : Avalonia.Svg.Skia.Svg 
=> style._addSetter(Avalonia.Svg.Skia.Svg.SourceProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> Source<T>(this Style<T> style, IBinding binding) where T : Avalonia.Svg.Skia.Svg 
=> style._addSetter(Avalonia.Svg.Skia.Svg.SourceProperty, binding);


 // Stretch

/*ValueStyleSetterGenerator*/
public static Style<T> Stretch<T>(this Style<T> style, Avalonia.Media.Stretch value) where T : Avalonia.Svg.Skia.Svg 
=> style._addSetter(Avalonia.Svg.Skia.Svg.StretchProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> Stretch<T>(this Style<T> style, IBinding binding) where T : Avalonia.Svg.Skia.Svg 
=> style._addSetter(Avalonia.Svg.Skia.Svg.StretchProperty, binding);


 // StretchDirection

/*ValueStyleSetterGenerator*/
public static Style<T> StretchDirection<T>(this Style<T> style, Avalonia.Media.StretchDirection value) where T : Avalonia.Svg.Skia.Svg 
=> style._addSetter(Avalonia.Svg.Skia.Svg.StretchDirectionProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> StretchDirection<T>(this Style<T> style, IBinding binding) where T : Avalonia.Svg.Skia.Svg 
=> style._addSetter(Avalonia.Svg.Skia.Svg.StretchDirectionProperty, binding);



}
