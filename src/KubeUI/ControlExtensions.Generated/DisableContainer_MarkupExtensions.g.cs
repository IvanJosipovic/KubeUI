#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class DisableContainer_MarkupExtensions
{
//================= Properties ======================//
 // ContentProperty

/*BindFromExpressionSetterGenerator*/
public static T Content<T>(this T control, Func<Avalonia.Input.InputElement> func, Action<Avalonia.Input.InputElement>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DisableContainer
   => control._set(Ursa.Controls.DisableContainer.ContentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Content<T>(this T control, Avalonia.Input.InputElement value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DisableContainer
=> control._setEx(Ursa.Controls.DisableContainer.ContentProperty, ps, () => control.Content = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Content<T>(this T control, IBinding binding) where T : Ursa.Controls.DisableContainer
   => control._set(Ursa.Controls.DisableContainer.ContentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Content<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DisableContainer
   => control._set(Ursa.Controls.DisableContainer.ContentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Content<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Input.InputElement> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DisableContainer
=> control._setEx(Ursa.Controls.DisableContainer.ContentProperty, ps, () => control.Content = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // DisabledTipProperty

/*BindFromExpressionSetterGenerator*/
public static T DisabledTip<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DisableContainer
   => control._set(Ursa.Controls.DisableContainer.DisabledTipProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T DisabledTip<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DisableContainer
=> control._setEx(Ursa.Controls.DisableContainer.DisabledTipProperty, ps, () => control.DisabledTip = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T DisabledTip<T>(this T control, IBinding binding) where T : Ursa.Controls.DisableContainer
   => control._set(Ursa.Controls.DisableContainer.DisabledTipProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T DisabledTip<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DisableContainer
   => control._set(Ursa.Controls.DisableContainer.DisabledTipProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T DisabledTip<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DisableContainer
=> control._setEx(Ursa.Controls.DisableContainer.DisabledTipProperty, ps, () => control.DisabledTip = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // ContentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Content<T>(this Style<T> style, Avalonia.Input.InputElement value) where T : Ursa.Controls.DisableContainer
=> style._addSetter(Ursa.Controls.DisableContainer.ContentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Content<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DisableContainer
=> style._addSetter(Ursa.Controls.DisableContainer.ContentProperty, binding);


 // DisabledTipProperty

/*ValueStyleSetterGenerator*/
public static Style<T> DisabledTip<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.DisableContainer
=> style._addSetter(Ursa.Controls.DisableContainer.DisabledTipProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> DisabledTip<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DisableContainer
=> style._addSetter(Ursa.Controls.DisableContainer.DisabledTipProperty, binding);



}