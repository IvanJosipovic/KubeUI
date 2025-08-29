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
public static partial class AnchorItem_MarkupExtensions
{
//================= Properties ======================//
 // AnchorId

/*BindFromExpressionSetterGenerator*/
public static T AnchorId<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.AnchorItem 
   => control._set(Ursa.Controls.AnchorItem.AnchorIdProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T AnchorId<T>(this T control,System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.AnchorItem 
=> control._setEx(Ursa.Controls.AnchorItem.AnchorIdProperty, ps, () => control.AnchorId = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T AnchorId<T>(this T control, IBinding binding) where T : Ursa.Controls.AnchorItem 
   => control._set(Ursa.Controls.AnchorItem.AnchorIdProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T AnchorId<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.AnchorItem 
   => control._set(Ursa.Controls.AnchorItem.AnchorIdProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T AnchorId<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.AnchorItem 
=> control._setEx(Ursa.Controls.AnchorItem.AnchorIdProperty, ps, () => control.AnchorId = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsSelected

/*BindFromExpressionSetterGenerator*/
public static T IsSelected<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.AnchorItem 
   => control._set(Ursa.Controls.AnchorItem.IsSelectedProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsSelected<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.AnchorItem 
=> control._setEx(Ursa.Controls.AnchorItem.IsSelectedProperty, ps, () => control.IsSelected = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsSelected<T>(this T control, IBinding binding) where T : Ursa.Controls.AnchorItem 
   => control._set(Ursa.Controls.AnchorItem.IsSelectedProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsSelected<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.AnchorItem 
   => control._set(Ursa.Controls.AnchorItem.IsSelectedProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsSelected<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.AnchorItem 
=> control._setEx(Ursa.Controls.AnchorItem.IsSelectedProperty, ps, () => control.IsSelected = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Styles ======================//
 // AnchorId

/*ValueStyleSetterGenerator*/
public static Style<T> AnchorId<T>(this Style<T> style, System.String value) where T : Ursa.Controls.AnchorItem 
=> style._addSetter(Ursa.Controls.AnchorItem.AnchorIdProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> AnchorId<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.AnchorItem 
=> style._addSetter(Ursa.Controls.AnchorItem.AnchorIdProperty, binding);


 // IsSelected

/*ValueStyleSetterGenerator*/
public static Style<T> IsSelected<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.AnchorItem 
=> style._addSetter(Ursa.Controls.AnchorItem.IsSelectedProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsSelected<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.AnchorItem 
=> style._addSetter(Ursa.Controls.AnchorItem.IsSelectedProperty, binding);



}
