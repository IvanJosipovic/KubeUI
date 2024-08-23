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
public static partial class Badge_MarkupExtensions
{
//================= Properties ======================//
 // BadgeThemeProperty

/*BindFromExpressionSetterGenerator*/
public static T BadgeTheme<T>(this T control, Func<Avalonia.Styling.ControlTheme> func, Action<Avalonia.Styling.ControlTheme>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Badge
   => control._set(Ursa.Controls.Badge.BadgeThemeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T BadgeTheme<T>(this T control, Avalonia.Styling.ControlTheme value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Badge
=> control._setEx(Ursa.Controls.Badge.BadgeThemeProperty, ps, () => control.BadgeTheme = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T BadgeTheme<T>(this T control, IBinding binding) where T : Ursa.Controls.Badge
   => control._set(Ursa.Controls.Badge.BadgeThemeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T BadgeTheme<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Badge
   => control._set(Ursa.Controls.Badge.BadgeThemeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T BadgeTheme<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ControlTheme> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Badge
=> control._setEx(Ursa.Controls.Badge.BadgeThemeProperty, ps, () => control.BadgeTheme = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // DotProperty

/*BindFromExpressionSetterGenerator*/
public static T Dot<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Badge
   => control._set(Ursa.Controls.Badge.DotProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Dot<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Badge
=> control._setEx(Ursa.Controls.Badge.DotProperty, ps, () => control.Dot = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Dot<T>(this T control, IBinding binding) where T : Ursa.Controls.Badge
   => control._set(Ursa.Controls.Badge.DotProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Dot<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Badge
   => control._set(Ursa.Controls.Badge.DotProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Dot<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Badge
=> control._setEx(Ursa.Controls.Badge.DotProperty, ps, () => control.Dot = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CornerPositionProperty

/*BindFromExpressionSetterGenerator*/
public static T CornerPosition<T>(this T control, Func<Ursa.Common.CornerPosition> func, Action<Ursa.Common.CornerPosition>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Badge
   => control._set(Ursa.Controls.Badge.CornerPositionProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CornerPosition<T>(this T control, Ursa.Common.CornerPosition value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Badge
=> control._setEx(Ursa.Controls.Badge.CornerPositionProperty, ps, () => control.CornerPosition = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CornerPosition<T>(this T control, IBinding binding) where T : Ursa.Controls.Badge
   => control._set(Ursa.Controls.Badge.CornerPositionProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CornerPosition<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Badge
   => control._set(Ursa.Controls.Badge.CornerPositionProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CornerPosition<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Ursa.Common.CornerPosition> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Badge
=> control._setEx(Ursa.Controls.Badge.CornerPositionProperty, ps, () => control.CornerPosition = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // OverflowCountProperty

/*BindFromExpressionSetterGenerator*/
public static T OverflowCount<T>(this T control, Func<System.Int32> func, Action<System.Int32>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Badge
   => control._set(Ursa.Controls.Badge.OverflowCountProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T OverflowCount<T>(this T control, System.Int32 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Badge
=> control._setEx(Ursa.Controls.Badge.OverflowCountProperty, ps, () => control.OverflowCount = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T OverflowCount<T>(this T control, IBinding binding) where T : Ursa.Controls.Badge
   => control._set(Ursa.Controls.Badge.OverflowCountProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T OverflowCount<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Badge
   => control._set(Ursa.Controls.Badge.OverflowCountProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T OverflowCount<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Int32> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Badge
=> control._setEx(Ursa.Controls.Badge.OverflowCountProperty, ps, () => control.OverflowCount = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // BadgeFontSizeProperty

/*BindFromExpressionSetterGenerator*/
public static T BadgeFontSize<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Badge
   => control._set(Ursa.Controls.Badge.BadgeFontSizeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T BadgeFontSize<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Badge
=> control._setEx(Ursa.Controls.Badge.BadgeFontSizeProperty, ps, () => control.BadgeFontSize = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T BadgeFontSize<T>(this T control, IBinding binding) where T : Ursa.Controls.Badge
   => control._set(Ursa.Controls.Badge.BadgeFontSizeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T BadgeFontSize<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Badge
   => control._set(Ursa.Controls.Badge.BadgeFontSizeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T BadgeFontSize<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Badge
=> control._setEx(Ursa.Controls.Badge.BadgeFontSizeProperty, ps, () => control.BadgeFontSize = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // BadgeThemeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> BadgeTheme<T>(this Style<T> style, Avalonia.Styling.ControlTheme value) where T : Ursa.Controls.Badge
=> style._addSetter(Ursa.Controls.Badge.BadgeThemeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> BadgeTheme<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Badge
=> style._addSetter(Ursa.Controls.Badge.BadgeThemeProperty, binding);


 // DotProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Dot<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.Badge
=> style._addSetter(Ursa.Controls.Badge.DotProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Dot<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Badge
=> style._addSetter(Ursa.Controls.Badge.DotProperty, binding);


 // CornerPositionProperty

/*ValueStyleSetterGenerator*/
public static Style<T> CornerPosition<T>(this Style<T> style, Ursa.Common.CornerPosition value) where T : Ursa.Controls.Badge
=> style._addSetter(Ursa.Controls.Badge.CornerPositionProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CornerPosition<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Badge
=> style._addSetter(Ursa.Controls.Badge.CornerPositionProperty, binding);


 // OverflowCountProperty

/*ValueStyleSetterGenerator*/
public static Style<T> OverflowCount<T>(this Style<T> style, System.Int32 value) where T : Ursa.Controls.Badge
=> style._addSetter(Ursa.Controls.Badge.OverflowCountProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> OverflowCount<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Badge
=> style._addSetter(Ursa.Controls.Badge.OverflowCountProperty, binding);


 // BadgeFontSizeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> BadgeFontSize<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.Badge
=> style._addSetter(Ursa.Controls.Badge.BadgeFontSizeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> BadgeFontSize<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Badge
=> style._addSetter(Ursa.Controls.Badge.BadgeFontSizeProperty, binding);



}
