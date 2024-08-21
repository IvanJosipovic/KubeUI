#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class DialogControlBase_MarkupExtensions
{
//================= Properties ======================//
 // IsFullScreenProperty

/*BindFromExpressionSetterGenerator*/
public static T IsFullScreen<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DialogControlBase
   => control._set(Ursa.Controls.DialogControlBase.IsFullScreenProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsFullScreen<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DialogControlBase
=> control._setEx(Ursa.Controls.DialogControlBase.IsFullScreenProperty, ps, () => control.IsFullScreen = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsFullScreen<T>(this T control, IBinding binding) where T : Ursa.Controls.DialogControlBase
   => control._set(Ursa.Controls.DialogControlBase.IsFullScreenProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsFullScreen<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DialogControlBase
   => control._set(Ursa.Controls.DialogControlBase.IsFullScreenProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsFullScreen<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DialogControlBase
=> control._setEx(Ursa.Controls.DialogControlBase.IsFullScreenProperty, ps, () => control.IsFullScreen = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // LayerChanged

/*ActionToEventGenerator*/
    public static T OnLayerChanged<T>(this T control, Action<Ursa.Controls.DialogLayerChangeEventArgs> action) where T : Ursa.Controls.DialogControlBase => 
        control._setEvent((System.EventHandler<Ursa.Controls.DialogLayerChangeEventArgs>) ((arg0, arg1) => action(arg1)), h => control.LayerChanged += h);



//================= Styles ======================//

}
