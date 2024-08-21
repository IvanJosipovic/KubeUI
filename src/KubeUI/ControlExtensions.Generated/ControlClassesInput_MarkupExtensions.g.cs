#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class ControlClassesInput_MarkupExtensions
{
//================= Properties ======================//
 // TargetProperty

/*BindFromExpressionSetterGenerator*/
public static T Target<T>(this T control, Func<Avalonia.Controls.Control> func, Action<Avalonia.Controls.Control>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ControlClassesInput
   => control._set(Ursa.Controls.ControlClassesInput.TargetProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Target<T>(this T control, Avalonia.Controls.Control value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ControlClassesInput
=> control._setEx(Ursa.Controls.ControlClassesInput.TargetProperty, ps, () => control.Target = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Target<T>(this T control, IBinding binding) where T : Ursa.Controls.ControlClassesInput
   => control._set(Ursa.Controls.ControlClassesInput.TargetProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Target<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ControlClassesInput
   => control._set(Ursa.Controls.ControlClassesInput.TargetProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Target<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Control> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ControlClassesInput
=> control._setEx(Ursa.Controls.ControlClassesInput.TargetProperty, ps, () => control.Target = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SeparatorProperty

/*BindFromExpressionSetterGenerator*/
public static T Separator<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ControlClassesInput
   => control._set(Ursa.Controls.ControlClassesInput.SeparatorProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Separator<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ControlClassesInput
=> control._setEx(Ursa.Controls.ControlClassesInput.SeparatorProperty, ps, () => control.Separator = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Separator<T>(this T control, IBinding binding) where T : Ursa.Controls.ControlClassesInput
   => control._set(Ursa.Controls.ControlClassesInput.SeparatorProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Separator<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ControlClassesInput
   => control._set(Ursa.Controls.ControlClassesInput.SeparatorProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Separator<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ControlClassesInput
=> control._setEx(Ursa.Controls.ControlClassesInput.SeparatorProperty, ps, () => control.Separator = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // TargetProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Target<T>(this Style<T> style, Avalonia.Controls.Control value) where T : Ursa.Controls.ControlClassesInput
=> style._addSetter(Ursa.Controls.ControlClassesInput.TargetProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Target<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ControlClassesInput
=> style._addSetter(Ursa.Controls.ControlClassesInput.TargetProperty, binding);


 // SeparatorProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Separator<T>(this Style<T> style, System.String value) where T : Ursa.Controls.ControlClassesInput
=> style._addSetter(Ursa.Controls.ControlClassesInput.SeparatorProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Separator<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ControlClassesInput
=> style._addSetter(Ursa.Controls.ControlClassesInput.SeparatorProperty, binding);



}
