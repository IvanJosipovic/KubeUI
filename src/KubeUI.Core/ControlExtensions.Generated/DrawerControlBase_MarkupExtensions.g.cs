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
public static partial class DrawerControlBase_MarkupExtensions
{
//================= Properties ======================//
 // PositionProperty

/*BindFromExpressionSetterGenerator*/
public static T Position<T>(this T control, Func<Ursa.Common.Position> func, Action<Ursa.Common.Position>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DrawerControlBase
   => control._set(Ursa.Controls.DrawerControlBase.PositionProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Position<T>(this T control, Ursa.Common.Position value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DrawerControlBase
=> control._setEx(Ursa.Controls.DrawerControlBase.PositionProperty, ps, () => control.Position = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Position<T>(this T control, IBinding binding) where T : Ursa.Controls.DrawerControlBase
   => control._set(Ursa.Controls.DrawerControlBase.PositionProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Position<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DrawerControlBase
   => control._set(Ursa.Controls.DrawerControlBase.PositionProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Position<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Ursa.Common.Position> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DrawerControlBase
=> control._setEx(Ursa.Controls.DrawerControlBase.PositionProperty, ps, () => control.Position = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsOpenProperty

/*BindFromExpressionSetterGenerator*/
public static T IsOpen<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DrawerControlBase
   => control._set(Ursa.Controls.DrawerControlBase.IsOpenProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsOpen<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DrawerControlBase
=> control._setEx(Ursa.Controls.DrawerControlBase.IsOpenProperty, ps, () => control.IsOpen = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsOpen<T>(this T control, IBinding binding) where T : Ursa.Controls.DrawerControlBase
   => control._set(Ursa.Controls.DrawerControlBase.IsOpenProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsOpen<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DrawerControlBase
   => control._set(Ursa.Controls.DrawerControlBase.IsOpenProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsOpen<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DrawerControlBase
=> control._setEx(Ursa.Controls.DrawerControlBase.IsOpenProperty, ps, () => control.IsOpen = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // PositionProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Position<T>(this Style<T> style, Ursa.Common.Position value) where T : Ursa.Controls.DrawerControlBase
=> style._addSetter(Ursa.Controls.DrawerControlBase.PositionProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Position<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DrawerControlBase
=> style._addSetter(Ursa.Controls.DrawerControlBase.PositionProperty, binding);


 // IsOpenProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsOpen<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.DrawerControlBase
=> style._addSetter(Ursa.Controls.DrawerControlBase.IsOpenProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsOpen<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DrawerControlBase
=> style._addSetter(Ursa.Controls.DrawerControlBase.IsOpenProperty, binding);



}
