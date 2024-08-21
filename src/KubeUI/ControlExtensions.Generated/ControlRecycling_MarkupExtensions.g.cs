#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class ControlRecycling_MarkupExtensions
{
//================= Properties ======================//
 // TryToUseIdAsKeyProperty

/*BindFromExpressionSetterGenerator*/
public static T TryToUseIdAsKey<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.Recycling.ControlRecycling
   => control._set(Dock.Avalonia.Controls.Recycling.ControlRecycling.TryToUseIdAsKeyProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T TryToUseIdAsKey<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.Recycling.ControlRecycling
=> control._setEx(Dock.Avalonia.Controls.Recycling.ControlRecycling.TryToUseIdAsKeyProperty, ps, () => control.TryToUseIdAsKey = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TryToUseIdAsKey<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.Recycling.ControlRecycling
   => control._set(Dock.Avalonia.Controls.Recycling.ControlRecycling.TryToUseIdAsKeyProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TryToUseIdAsKey<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.Recycling.ControlRecycling
   => control._set(Dock.Avalonia.Controls.Recycling.ControlRecycling.TryToUseIdAsKeyProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T TryToUseIdAsKey<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.Recycling.ControlRecycling
=> control._setEx(Dock.Avalonia.Controls.Recycling.ControlRecycling.TryToUseIdAsKeyProperty, ps, () => control.TryToUseIdAsKey = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//

}
