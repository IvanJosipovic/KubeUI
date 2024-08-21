#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class DefaultDialogWindow_MarkupExtensions
{
//================= Properties ======================//
 // ButtonsProperty

/*BindFromExpressionSetterGenerator*/
public static T Buttons<T>(this T control, Func<Ursa.Controls.DialogButton> func, Action<Ursa.Controls.DialogButton>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DefaultDialogWindow
   => control._set(Ursa.Controls.DefaultDialogWindow.ButtonsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Buttons<T>(this T control, Ursa.Controls.DialogButton value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DefaultDialogWindow
=> control._setEx(Ursa.Controls.DefaultDialogWindow.ButtonsProperty, ps, () => control.Buttons = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Buttons<T>(this T control, IBinding binding) where T : Ursa.Controls.DefaultDialogWindow
   => control._set(Ursa.Controls.DefaultDialogWindow.ButtonsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Buttons<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DefaultDialogWindow
   => control._set(Ursa.Controls.DefaultDialogWindow.ButtonsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Buttons<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Ursa.Controls.DialogButton> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DefaultDialogWindow
=> control._setEx(Ursa.Controls.DefaultDialogWindow.ButtonsProperty, ps, () => control.Buttons = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ModeProperty

/*BindFromExpressionSetterGenerator*/
public static T Mode<T>(this T control, Func<Ursa.Controls.DialogMode> func, Action<Ursa.Controls.DialogMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DefaultDialogWindow
   => control._set(Ursa.Controls.DefaultDialogWindow.ModeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Mode<T>(this T control, Ursa.Controls.DialogMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DefaultDialogWindow
=> control._setEx(Ursa.Controls.DefaultDialogWindow.ModeProperty, ps, () => control.Mode = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Mode<T>(this T control, IBinding binding) where T : Ursa.Controls.DefaultDialogWindow
   => control._set(Ursa.Controls.DefaultDialogWindow.ModeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Mode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DefaultDialogWindow
   => control._set(Ursa.Controls.DefaultDialogWindow.ModeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Mode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Ursa.Controls.DialogMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DefaultDialogWindow
=> control._setEx(Ursa.Controls.DefaultDialogWindow.ModeProperty, ps, () => control.Mode = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // ButtonsProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Buttons<T>(this Style<T> style, Ursa.Controls.DialogButton value) where T : Ursa.Controls.DefaultDialogWindow
=> style._addSetter(Ursa.Controls.DefaultDialogWindow.ButtonsProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Buttons<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DefaultDialogWindow
=> style._addSetter(Ursa.Controls.DefaultDialogWindow.ButtonsProperty, binding);


 // ModeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Mode<T>(this Style<T> style, Ursa.Controls.DialogMode value) where T : Ursa.Controls.DefaultDialogWindow
=> style._addSetter(Ursa.Controls.DefaultDialogWindow.ModeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Mode<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DefaultDialogWindow
=> style._addSetter(Ursa.Controls.DefaultDialogWindow.ModeProperty, binding);



}
