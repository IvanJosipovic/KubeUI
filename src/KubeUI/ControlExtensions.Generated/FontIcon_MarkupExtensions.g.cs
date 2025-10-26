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
public static partial class FontIcon_MarkupExtensions
{
//================= Properties ======================//
 // FontFamily

/*BindFromExpressionSetterGenerator*/
public static T FontFamily<T>(this T control, Func<Avalonia.Media.FontFamily> func, Action<Avalonia.Media.FontFamily>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FontIcon 
   => control._set(FluentAvalonia.UI.Controls.FontIcon.FontFamilyProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T FontFamily<T>(this T control,Avalonia.Media.FontFamily value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FontIcon 
=> control._setEx(FluentAvalonia.UI.Controls.FontIcon.FontFamilyProperty, ps, () => control.FontFamily = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T FontFamily<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FontIcon 
   => control._set(FluentAvalonia.UI.Controls.FontIcon.FontFamilyProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T FontFamily<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FontIcon 
   => control._set(FluentAvalonia.UI.Controls.FontIcon.FontFamilyProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T FontFamily<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.FontFamily> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FontIcon 
=> control._setEx(FluentAvalonia.UI.Controls.FontIcon.FontFamilyProperty, ps, () => control.FontFamily = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // FontSize

/*BindFromExpressionSetterGenerator*/
public static T FontSize<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FontIcon 
   => control._set(FluentAvalonia.UI.Controls.FontIcon.FontSizeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T FontSize<T>(this T control,System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FontIcon 
=> control._setEx(FluentAvalonia.UI.Controls.FontIcon.FontSizeProperty, ps, () => control.FontSize = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T FontSize<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FontIcon 
   => control._set(FluentAvalonia.UI.Controls.FontIcon.FontSizeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T FontSize<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FontIcon 
   => control._set(FluentAvalonia.UI.Controls.FontIcon.FontSizeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T FontSize<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FontIcon 
=> control._setEx(FluentAvalonia.UI.Controls.FontIcon.FontSizeProperty, ps, () => control.FontSize = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // FontWeight

/*BindFromExpressionSetterGenerator*/
public static T FontWeight<T>(this T control, Func<Avalonia.Media.FontWeight> func, Action<Avalonia.Media.FontWeight>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FontIcon 
   => control._set(FluentAvalonia.UI.Controls.FontIcon.FontWeightProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T FontWeight<T>(this T control,Avalonia.Media.FontWeight value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FontIcon 
=> control._setEx(FluentAvalonia.UI.Controls.FontIcon.FontWeightProperty, ps, () => control.FontWeight = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T FontWeight<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FontIcon 
   => control._set(FluentAvalonia.UI.Controls.FontIcon.FontWeightProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T FontWeight<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FontIcon 
   => control._set(FluentAvalonia.UI.Controls.FontIcon.FontWeightProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T FontWeight<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.FontWeight> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FontIcon 
=> control._setEx(FluentAvalonia.UI.Controls.FontIcon.FontWeightProperty, ps, () => control.FontWeight = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // FontStyle

/*BindFromExpressionSetterGenerator*/
public static T FontStyle<T>(this T control, Func<Avalonia.Media.FontStyle> func, Action<Avalonia.Media.FontStyle>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FontIcon 
   => control._set(FluentAvalonia.UI.Controls.FontIcon.FontStyleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T FontStyle<T>(this T control,Avalonia.Media.FontStyle value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FontIcon 
=> control._setEx(FluentAvalonia.UI.Controls.FontIcon.FontStyleProperty, ps, () => control.FontStyle = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T FontStyle<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FontIcon 
   => control._set(FluentAvalonia.UI.Controls.FontIcon.FontStyleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T FontStyle<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FontIcon 
   => control._set(FluentAvalonia.UI.Controls.FontIcon.FontStyleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T FontStyle<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.FontStyle> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FontIcon 
=> control._setEx(FluentAvalonia.UI.Controls.FontIcon.FontStyleProperty, ps, () => control.FontStyle = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // Glyph

/*BindFromExpressionSetterGenerator*/
public static T Glyph<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FontIcon 
   => control._set(FluentAvalonia.UI.Controls.FontIcon.GlyphProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Glyph<T>(this T control,System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FontIcon 
=> control._setEx(FluentAvalonia.UI.Controls.FontIcon.GlyphProperty, ps, () => control.Glyph = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Glyph<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FontIcon 
   => control._set(FluentAvalonia.UI.Controls.FontIcon.GlyphProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Glyph<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FontIcon 
   => control._set(FluentAvalonia.UI.Controls.FontIcon.GlyphProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Glyph<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FontIcon 
=> control._setEx(FluentAvalonia.UI.Controls.FontIcon.GlyphProperty, ps, () => control.Glyph = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Styles ======================//
 // FontFamily

/*ValueStyleSetterGenerator*/
public static Style<T> FontFamily<T>(this Style<T> style, Avalonia.Media.FontFamily value) where T : FluentAvalonia.UI.Controls.FontIcon 
=> style._addSetter(FluentAvalonia.UI.Controls.FontIcon.FontFamilyProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> FontFamily<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FontIcon 
=> style._addSetter(FluentAvalonia.UI.Controls.FontIcon.FontFamilyProperty, binding);


 // FontSize

/*ValueStyleSetterGenerator*/
public static Style<T> FontSize<T>(this Style<T> style, System.Double value) where T : FluentAvalonia.UI.Controls.FontIcon 
=> style._addSetter(FluentAvalonia.UI.Controls.FontIcon.FontSizeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> FontSize<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FontIcon 
=> style._addSetter(FluentAvalonia.UI.Controls.FontIcon.FontSizeProperty, binding);


 // FontWeight

/*ValueStyleSetterGenerator*/
public static Style<T> FontWeight<T>(this Style<T> style, Avalonia.Media.FontWeight value) where T : FluentAvalonia.UI.Controls.FontIcon 
=> style._addSetter(FluentAvalonia.UI.Controls.FontIcon.FontWeightProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> FontWeight<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FontIcon 
=> style._addSetter(FluentAvalonia.UI.Controls.FontIcon.FontWeightProperty, binding);


 // FontStyle

/*ValueStyleSetterGenerator*/
public static Style<T> FontStyle<T>(this Style<T> style, Avalonia.Media.FontStyle value) where T : FluentAvalonia.UI.Controls.FontIcon 
=> style._addSetter(FluentAvalonia.UI.Controls.FontIcon.FontStyleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> FontStyle<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FontIcon 
=> style._addSetter(FluentAvalonia.UI.Controls.FontIcon.FontStyleProperty, binding);


 // Glyph

/*ValueStyleSetterGenerator*/
public static Style<T> Glyph<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.FontIcon 
=> style._addSetter(FluentAvalonia.UI.Controls.FontIcon.GlyphProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Glyph<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FontIcon 
=> style._addSetter(FluentAvalonia.UI.Controls.FontIcon.GlyphProperty, binding);



}
