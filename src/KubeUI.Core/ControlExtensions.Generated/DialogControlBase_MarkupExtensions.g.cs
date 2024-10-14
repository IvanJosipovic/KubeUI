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
public static partial class DialogControlBase_MarkupExtensions
{
//================= Properties ======================//
 // IsFullScreen

/*BindFromExpressionSetterGenerator*/
public static T IsFullScreen<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DialogControlBase 
   => control._set(Ursa.Controls.DialogControlBase.IsFullScreenProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsFullScreen<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DialogControlBase 
=> control._setEx(Ursa.Controls.DialogControlBase.IsFullScreenProperty, ps, () => control.IsFullScreen = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsFullScreen<T>(this T control, IBinding binding) where T : Ursa.Controls.DialogControlBase 
   => control._set(Ursa.Controls.DialogControlBase.IsFullScreenProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsFullScreen<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DialogControlBase 
   => control._set(Ursa.Controls.DialogControlBase.IsFullScreenProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsFullScreen<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DialogControlBase 
=> control._setEx(Ursa.Controls.DialogControlBase.IsFullScreenProperty, ps, () => control.IsFullScreen = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Attached Properties ======================//
 // CanDragMove

/*AttachedPropertyMagicalSetterGenerator*/
public static T DialogControlBase_CanDragMove<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Input.InputElement
 => control._setEx(Ursa.Controls.DialogControlBase.CanDragMoveProperty, ps, () => Ursa.Controls.DialogControlBase.SetCanDragMove(control, value), bindingMode, converter, bindingSource);

/*AttachedPropertyBindFromExpressionSetterGenerator*/
public static T DialogControlBase_CanDragMove<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Input.InputElement 
   => control._set(Ursa.Controls.DialogControlBase.CanDragMoveProperty, func, onChanged, expression);


 // CanClose

/*AttachedPropertyMagicalSetterGenerator*/
public static T DialogControlBase_CanClose<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Input.InputElement
 => control._setEx(Ursa.Controls.DialogControlBase.CanCloseProperty, ps, () => Ursa.Controls.DialogControlBase.SetCanClose(control, value), bindingMode, converter, bindingSource);

/*AttachedPropertyBindFromExpressionSetterGenerator*/
public static T DialogControlBase_CanClose<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Input.InputElement 
   => control._set(Ursa.Controls.DialogControlBase.CanCloseProperty, func, onChanged, expression);



//================= Events ======================//
 // LayerChanged

/*ActionToEventGenerator*/
public static T OnLayerChanged<T>(this T control, Action<Ursa.Controls.DialogLayerChangeEventArgs> action, Avalonia.Interactivity.RoutingStrategies routes = Avalonia.Interactivity.RoutingStrategies.Tunnel | Avalonia.Interactivity.RoutingStrategies.Bubble) where T : Ursa.Controls.DialogControlBase 
{
  control.AddHandler(Ursa.Controls.DialogControlBase.LayerChangedEvent, (_, args) => action(args), routes);
  return control; 
}




}
