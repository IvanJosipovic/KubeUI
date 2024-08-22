#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class ElasticWrapPanel_MarkupExtensions
{
//================= Properties ======================//
 // IsFillHorizontalProperty

/*BindFromExpressionSetterGenerator*/
public static T IsFillHorizontal<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ElasticWrapPanel
   => control._set(Ursa.Controls.ElasticWrapPanel.IsFillHorizontalProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsFillHorizontal<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ElasticWrapPanel
=> control._setEx(Ursa.Controls.ElasticWrapPanel.IsFillHorizontalProperty, ps, () => control.IsFillHorizontal = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsFillHorizontal<T>(this T control, IBinding binding) where T : Ursa.Controls.ElasticWrapPanel
   => control._set(Ursa.Controls.ElasticWrapPanel.IsFillHorizontalProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsFillHorizontal<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ElasticWrapPanel
   => control._set(Ursa.Controls.ElasticWrapPanel.IsFillHorizontalProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsFillHorizontal<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ElasticWrapPanel
=> control._setEx(Ursa.Controls.ElasticWrapPanel.IsFillHorizontalProperty, ps, () => control.IsFillHorizontal = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsFillVerticalProperty

/*BindFromExpressionSetterGenerator*/
public static T IsFillVertical<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ElasticWrapPanel
   => control._set(Ursa.Controls.ElasticWrapPanel.IsFillVerticalProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsFillVertical<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ElasticWrapPanel
=> control._setEx(Ursa.Controls.ElasticWrapPanel.IsFillVerticalProperty, ps, () => control.IsFillVertical = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsFillVertical<T>(this T control, IBinding binding) where T : Ursa.Controls.ElasticWrapPanel
   => control._set(Ursa.Controls.ElasticWrapPanel.IsFillVerticalProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsFillVertical<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ElasticWrapPanel
   => control._set(Ursa.Controls.ElasticWrapPanel.IsFillVerticalProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsFillVertical<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ElasticWrapPanel
=> control._setEx(Ursa.Controls.ElasticWrapPanel.IsFillVerticalProperty, ps, () => control.IsFillVertical = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // IsFillHorizontalProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsFillHorizontal<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.ElasticWrapPanel
=> style._addSetter(Ursa.Controls.ElasticWrapPanel.IsFillHorizontalProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsFillHorizontal<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ElasticWrapPanel
=> style._addSetter(Ursa.Controls.ElasticWrapPanel.IsFillHorizontalProperty, binding);


 // IsFillVerticalProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsFillVertical<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.ElasticWrapPanel
=> style._addSetter(Ursa.Controls.ElasticWrapPanel.IsFillVerticalProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsFillVertical<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ElasticWrapPanel
=> style._addSetter(Ursa.Controls.ElasticWrapPanel.IsFillVerticalProperty, binding);



}
