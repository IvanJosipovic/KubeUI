#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class MessageBoxControl_MarkupExtensions
{
//================= Properties ======================//
 // MessageIconProperty

/*BindFromExpressionSetterGenerator*/
public static T MessageIcon<T>(this T control, Func<Ursa.Controls.MessageBoxIcon> func, Action<Ursa.Controls.MessageBoxIcon>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.MessageBoxControl
   => control._set(Ursa.Controls.MessageBoxControl.MessageIconProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T MessageIcon<T>(this T control, Ursa.Controls.MessageBoxIcon value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MessageBoxControl
=> control._setEx(Ursa.Controls.MessageBoxControl.MessageIconProperty, ps, () => control.MessageIcon = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T MessageIcon<T>(this T control, IBinding binding) where T : Ursa.Controls.MessageBoxControl
   => control._set(Ursa.Controls.MessageBoxControl.MessageIconProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T MessageIcon<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.MessageBoxControl
   => control._set(Ursa.Controls.MessageBoxControl.MessageIconProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T MessageIcon<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Ursa.Controls.MessageBoxIcon> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MessageBoxControl
=> control._setEx(Ursa.Controls.MessageBoxControl.MessageIconProperty, ps, () => control.MessageIcon = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ButtonsProperty

/*BindFromExpressionSetterGenerator*/
public static T Buttons<T>(this T control, Func<Ursa.Controls.MessageBoxButton> func, Action<Ursa.Controls.MessageBoxButton>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.MessageBoxControl
   => control._set(Ursa.Controls.MessageBoxControl.ButtonsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Buttons<T>(this T control, Ursa.Controls.MessageBoxButton value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MessageBoxControl
=> control._setEx(Ursa.Controls.MessageBoxControl.ButtonsProperty, ps, () => control.Buttons = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Buttons<T>(this T control, IBinding binding) where T : Ursa.Controls.MessageBoxControl
   => control._set(Ursa.Controls.MessageBoxControl.ButtonsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Buttons<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.MessageBoxControl
   => control._set(Ursa.Controls.MessageBoxControl.ButtonsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Buttons<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Ursa.Controls.MessageBoxButton> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MessageBoxControl
=> control._setEx(Ursa.Controls.MessageBoxControl.ButtonsProperty, ps, () => control.Buttons = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TitleProperty

/*BindFromExpressionSetterGenerator*/
public static T Title<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.MessageBoxControl
   => control._set(Ursa.Controls.MessageBoxControl.TitleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Title<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MessageBoxControl
=> control._setEx(Ursa.Controls.MessageBoxControl.TitleProperty, ps, () => control.Title = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Title<T>(this T control, IBinding binding) where T : Ursa.Controls.MessageBoxControl
   => control._set(Ursa.Controls.MessageBoxControl.TitleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Title<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.MessageBoxControl
   => control._set(Ursa.Controls.MessageBoxControl.TitleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Title<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MessageBoxControl
=> control._setEx(Ursa.Controls.MessageBoxControl.TitleProperty, ps, () => control.Title = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // MessageIconProperty

/*ValueStyleSetterGenerator*/
public static Style<T> MessageIcon<T>(this Style<T> style, Ursa.Controls.MessageBoxIcon value) where T : Ursa.Controls.MessageBoxControl
=> style._addSetter(Ursa.Controls.MessageBoxControl.MessageIconProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> MessageIcon<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.MessageBoxControl
=> style._addSetter(Ursa.Controls.MessageBoxControl.MessageIconProperty, binding);


 // ButtonsProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Buttons<T>(this Style<T> style, Ursa.Controls.MessageBoxButton value) where T : Ursa.Controls.MessageBoxControl
=> style._addSetter(Ursa.Controls.MessageBoxControl.ButtonsProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Buttons<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.MessageBoxControl
=> style._addSetter(Ursa.Controls.MessageBoxControl.ButtonsProperty, binding);


 // TitleProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Title<T>(this Style<T> style, System.String value) where T : Ursa.Controls.MessageBoxControl
=> style._addSetter(Ursa.Controls.MessageBoxControl.TitleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Title<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.MessageBoxControl
=> style._addSetter(Ursa.Controls.MessageBoxControl.TitleProperty, binding);



}
