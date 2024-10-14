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
public static partial class TitleBar_MarkupExtensions
{
//================= Properties ======================//
 // LeftContentProperty

/*BindFromExpressionSetterGenerator*/
public static T LeftContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TitleBar
   => control._set(Ursa.Controls.TitleBar.LeftContentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T LeftContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TitleBar
=> control._setEx(Ursa.Controls.TitleBar.LeftContentProperty, ps, () => control.LeftContent = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T LeftContent<T>(this T control, IBinding binding) where T : Ursa.Controls.TitleBar
   => control._set(Ursa.Controls.TitleBar.LeftContentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T LeftContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TitleBar
   => control._set(Ursa.Controls.TitleBar.LeftContentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T LeftContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TitleBar
=> control._setEx(Ursa.Controls.TitleBar.LeftContentProperty, ps, () => control.LeftContent = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // RightContentProperty

/*BindFromExpressionSetterGenerator*/
public static T RightContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TitleBar
   => control._set(Ursa.Controls.TitleBar.RightContentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T RightContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TitleBar
=> control._setEx(Ursa.Controls.TitleBar.RightContentProperty, ps, () => control.RightContent = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T RightContent<T>(this T control, IBinding binding) where T : Ursa.Controls.TitleBar
   => control._set(Ursa.Controls.TitleBar.RightContentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T RightContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TitleBar
   => control._set(Ursa.Controls.TitleBar.RightContentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T RightContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TitleBar
=> control._setEx(Ursa.Controls.TitleBar.RightContentProperty, ps, () => control.RightContent = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsTitleVisibleProperty

/*BindFromExpressionSetterGenerator*/
public static T IsTitleVisible<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TitleBar
   => control._set(Ursa.Controls.TitleBar.IsTitleVisibleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsTitleVisible<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TitleBar
=> control._setEx(Ursa.Controls.TitleBar.IsTitleVisibleProperty, ps, () => control.IsTitleVisible = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsTitleVisible<T>(this T control, IBinding binding) where T : Ursa.Controls.TitleBar
   => control._set(Ursa.Controls.TitleBar.IsTitleVisibleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsTitleVisible<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TitleBar
   => control._set(Ursa.Controls.TitleBar.IsTitleVisibleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsTitleVisible<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TitleBar
=> control._setEx(Ursa.Controls.TitleBar.IsTitleVisibleProperty, ps, () => control.IsTitleVisible = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // LeftContentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> LeftContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.TitleBar
=> style._addSetter(Ursa.Controls.TitleBar.LeftContentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> LeftContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TitleBar
=> style._addSetter(Ursa.Controls.TitleBar.LeftContentProperty, binding);


 // RightContentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> RightContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.TitleBar
=> style._addSetter(Ursa.Controls.TitleBar.RightContentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> RightContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TitleBar
=> style._addSetter(Ursa.Controls.TitleBar.RightContentProperty, binding);


 // IsTitleVisibleProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsTitleVisible<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.TitleBar
=> style._addSetter(Ursa.Controls.TitleBar.IsTitleVisibleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsTitleVisible<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TitleBar
=> style._addSetter(Ursa.Controls.TitleBar.IsTitleVisibleProperty, binding);



}
