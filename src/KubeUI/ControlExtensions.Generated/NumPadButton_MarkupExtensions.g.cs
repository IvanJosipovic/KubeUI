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
public static partial class NumPadButton_MarkupExtensions
{
//================= Properties ======================//
 // NumKeyProperty

/*BindFromExpressionSetterGenerator*/
public static T NumKey<T>(this T control, Func<System.Nullable<Avalonia.Input.Key>> func, Action<System.Nullable<Avalonia.Input.Key>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumPadButton
   => control._set(Ursa.Controls.NumPadButton.NumKeyProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T NumKey<T>(this T control, System.Nullable<Avalonia.Input.Key> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumPadButton
=> control._setEx(Ursa.Controls.NumPadButton.NumKeyProperty, ps, () => control.NumKey = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T NumKey<T>(this T control, IBinding binding) where T : Ursa.Controls.NumPadButton
   => control._set(Ursa.Controls.NumPadButton.NumKeyProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T NumKey<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumPadButton
   => control._set(Ursa.Controls.NumPadButton.NumKeyProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T NumKey<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Nullable<Avalonia.Input.Key>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumPadButton
=> control._setEx(Ursa.Controls.NumPadButton.NumKeyProperty, ps, () => control.NumKey = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // FunctionKeyProperty

/*BindFromExpressionSetterGenerator*/
public static T FunctionKey<T>(this T control, Func<System.Nullable<Avalonia.Input.Key>> func, Action<System.Nullable<Avalonia.Input.Key>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumPadButton
   => control._set(Ursa.Controls.NumPadButton.FunctionKeyProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T FunctionKey<T>(this T control, System.Nullable<Avalonia.Input.Key> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumPadButton
=> control._setEx(Ursa.Controls.NumPadButton.FunctionKeyProperty, ps, () => control.FunctionKey = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T FunctionKey<T>(this T control, IBinding binding) where T : Ursa.Controls.NumPadButton
   => control._set(Ursa.Controls.NumPadButton.FunctionKeyProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T FunctionKey<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumPadButton
   => control._set(Ursa.Controls.NumPadButton.FunctionKeyProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T FunctionKey<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Nullable<Avalonia.Input.Key>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumPadButton
=> control._setEx(Ursa.Controls.NumPadButton.FunctionKeyProperty, ps, () => control.FunctionKey = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // NumModeProperty

/*BindFromExpressionSetterGenerator*/
public static T NumMode<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumPadButton
   => control._set(Ursa.Controls.NumPadButton.NumModeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T NumMode<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumPadButton
=> control._setEx(Ursa.Controls.NumPadButton.NumModeProperty, ps, () => control.NumMode = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T NumMode<T>(this T control, IBinding binding) where T : Ursa.Controls.NumPadButton
   => control._set(Ursa.Controls.NumPadButton.NumModeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T NumMode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumPadButton
   => control._set(Ursa.Controls.NumPadButton.NumModeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T NumMode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumPadButton
=> control._setEx(Ursa.Controls.NumPadButton.NumModeProperty, ps, () => control.NumMode = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // NumContentProperty

/*BindFromExpressionSetterGenerator*/
public static T NumContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumPadButton
   => control._set(Ursa.Controls.NumPadButton.NumContentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T NumContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumPadButton
=> control._setEx(Ursa.Controls.NumPadButton.NumContentProperty, ps, () => control.NumContent = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T NumContent<T>(this T control, IBinding binding) where T : Ursa.Controls.NumPadButton
   => control._set(Ursa.Controls.NumPadButton.NumContentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T NumContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumPadButton
   => control._set(Ursa.Controls.NumPadButton.NumContentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T NumContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumPadButton
=> control._setEx(Ursa.Controls.NumPadButton.NumContentProperty, ps, () => control.NumContent = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // FunctionContentProperty

/*BindFromExpressionSetterGenerator*/
public static T FunctionContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumPadButton
   => control._set(Ursa.Controls.NumPadButton.FunctionContentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T FunctionContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumPadButton
=> control._setEx(Ursa.Controls.NumPadButton.FunctionContentProperty, ps, () => control.FunctionContent = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T FunctionContent<T>(this T control, IBinding binding) where T : Ursa.Controls.NumPadButton
   => control._set(Ursa.Controls.NumPadButton.FunctionContentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T FunctionContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumPadButton
   => control._set(Ursa.Controls.NumPadButton.FunctionContentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T FunctionContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumPadButton
=> control._setEx(Ursa.Controls.NumPadButton.FunctionContentProperty, ps, () => control.FunctionContent = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // NumKeyProperty

/*ValueStyleSetterGenerator*/
public static Style<T> NumKey<T>(this Style<T> style, System.Nullable<Avalonia.Input.Key> value) where T : Ursa.Controls.NumPadButton
=> style._addSetter(Ursa.Controls.NumPadButton.NumKeyProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> NumKey<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumPadButton
=> style._addSetter(Ursa.Controls.NumPadButton.NumKeyProperty, binding);


 // FunctionKeyProperty

/*ValueStyleSetterGenerator*/
public static Style<T> FunctionKey<T>(this Style<T> style, System.Nullable<Avalonia.Input.Key> value) where T : Ursa.Controls.NumPadButton
=> style._addSetter(Ursa.Controls.NumPadButton.FunctionKeyProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> FunctionKey<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumPadButton
=> style._addSetter(Ursa.Controls.NumPadButton.FunctionKeyProperty, binding);


 // NumModeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> NumMode<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.NumPadButton
=> style._addSetter(Ursa.Controls.NumPadButton.NumModeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> NumMode<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumPadButton
=> style._addSetter(Ursa.Controls.NumPadButton.NumModeProperty, binding);


 // NumContentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> NumContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.NumPadButton
=> style._addSetter(Ursa.Controls.NumPadButton.NumContentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> NumContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumPadButton
=> style._addSetter(Ursa.Controls.NumPadButton.NumContentProperty, binding);


 // FunctionContentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> FunctionContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.NumPadButton
=> style._addSetter(Ursa.Controls.NumPadButton.FunctionContentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> FunctionContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumPadButton
=> style._addSetter(Ursa.Controls.NumPadButton.FunctionContentProperty, binding);



}
