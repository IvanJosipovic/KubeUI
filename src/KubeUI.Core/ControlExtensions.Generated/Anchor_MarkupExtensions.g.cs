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
public static partial class Anchor_MarkupExtensions
{
//================= Properties ======================//
 // TargetContainer

/*ValueSetterGenerator*/
public static T TargetContainer<T>(this T control, Avalonia.Controls.ScrollViewer value) where T : Ursa.Controls.Anchor 
=> control._set(() => control.TargetContainer = value!);

/*BindFromExpressionSetterGenerator*/
public static T TargetContainer<T>(this T control, Func<Avalonia.Controls.ScrollViewer> func, Action<Avalonia.Controls.ScrollViewer>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : Ursa.Controls.Anchor 
   => control._set(Ursa.Controls.Anchor.TargetContainerProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T TargetContainer<T>(this T control,Avalonia.Controls.ScrollViewer value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Ursa.Controls.Anchor 
=> control._setEx(Ursa.Controls.Anchor.TargetContainerProperty, ps, () => control.TargetContainer = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TargetContainer<T>(this T control, IBinding binding) where T : Ursa.Controls.Anchor 
   => control._set(Ursa.Controls.Anchor.TargetContainerProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TargetContainer<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Anchor 
   => control._set(Ursa.Controls.Anchor.TargetContainerProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T TargetContainer<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.ScrollViewer> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Ursa.Controls.Anchor 
=> control._setEx(Ursa.Controls.Anchor.TargetContainerProperty, ps, () => control.TargetContainer = converter.TryConvert(value)!, bindingMode, converter, bindingSource);


 // TopOffset

/*ValueSetterGenerator*/
public static T TopOffset<T>(this T control, System.Double value) where T : Ursa.Controls.Anchor 
=> control._set(() => control.TopOffset = value!);

/*BindFromExpressionSetterGenerator*/
public static T TopOffset<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : Ursa.Controls.Anchor 
   => control._set(Ursa.Controls.Anchor.TopOffsetProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T TopOffset<T>(this T control,System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Ursa.Controls.Anchor 
=> control._setEx(Ursa.Controls.Anchor.TopOffsetProperty, ps, () => control.TopOffset = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TopOffset<T>(this T control, IBinding binding) where T : Ursa.Controls.Anchor 
   => control._set(Ursa.Controls.Anchor.TopOffsetProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TopOffset<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Anchor 
   => control._set(Ursa.Controls.Anchor.TopOffsetProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T TopOffset<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Ursa.Controls.Anchor 
=> control._setEx(Ursa.Controls.Anchor.TopOffsetProperty, ps, () => control.TopOffset = converter.TryConvert(value)!, bindingMode, converter, bindingSource);



//================= Attached Properties ======================//
 // Id

/*AttachedPropertyMagicalSetterGenerator*/
public static T Anchor_Id<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Avalonia.Visual
 => control._setEx(Ursa.Controls.Anchor.IdProperty, ps, () => Ursa.Controls.Anchor.SetId(control, value), bindingMode, converter, bindingSource);

/*AttachedPropertyBindFromExpressionSetterGenerator*/
public static T Anchor_Id<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : Avalonia.Visual 
   => control._set(Ursa.Controls.Anchor.IdProperty!, func, onChanged, expression);



//================= Styles ======================//
 // TargetContainer

/*ValueStyleSetterGenerator*/
public static Style<T> TargetContainer<T>(this Style<T> style, Avalonia.Controls.ScrollViewer value) where T : Ursa.Controls.Anchor 
=> style._addSetter(Ursa.Controls.Anchor.TargetContainerProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> TargetContainer<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Anchor 
=> style._addSetter(Ursa.Controls.Anchor.TargetContainerProperty, binding);


 // TopOffset

/*ValueStyleSetterGenerator*/
public static Style<T> TopOffset<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.Anchor 
=> style._addSetter(Ursa.Controls.Anchor.TopOffsetProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> TopOffset<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Anchor 
=> style._addSetter(Ursa.Controls.Anchor.TopOffsetProperty, binding);



}
