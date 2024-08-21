#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class NumPad_MarkupExtensions
{
//================= Properties ======================//
 // TargetProperty

/*BindFromExpressionSetterGenerator*/
public static T Target<T>(this T control, Func<Avalonia.Input.InputElement> func, Action<Avalonia.Input.InputElement>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumPad
   => control._set(Ursa.Controls.NumPad.TargetProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Target<T>(this T control, Avalonia.Input.InputElement value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumPad
=> control._setEx(Ursa.Controls.NumPad.TargetProperty, ps, () => control.Target = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Target<T>(this T control, IBinding binding) where T : Ursa.Controls.NumPad
   => control._set(Ursa.Controls.NumPad.TargetProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Target<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumPad
   => control._set(Ursa.Controls.NumPad.TargetProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Target<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Input.InputElement> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumPad
=> control._setEx(Ursa.Controls.NumPad.TargetProperty, ps, () => control.Target = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // NumModeProperty

/*BindFromExpressionSetterGenerator*/
public static T NumMode<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumPad
   => control._set(Ursa.Controls.NumPad.NumModeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T NumMode<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumPad
=> control._setEx(Ursa.Controls.NumPad.NumModeProperty, ps, () => control.NumMode = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T NumMode<T>(this T control, IBinding binding) where T : Ursa.Controls.NumPad
   => control._set(Ursa.Controls.NumPad.NumModeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T NumMode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumPad
   => control._set(Ursa.Controls.NumPad.NumModeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T NumMode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumPad
=> control._setEx(Ursa.Controls.NumPad.NumModeProperty, ps, () => control.NumMode = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // TargetProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Target<T>(this Style<T> style, Avalonia.Input.InputElement value) where T : Ursa.Controls.NumPad
=> style._addSetter(Ursa.Controls.NumPad.TargetProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Target<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumPad
=> style._addSetter(Ursa.Controls.NumPad.TargetProperty, binding);


 // NumModeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> NumMode<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.NumPad
=> style._addSetter(Ursa.Controls.NumPad.NumModeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> NumMode<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumPad
=> style._addSetter(Ursa.Controls.NumPad.NumModeProperty, binding);



}
