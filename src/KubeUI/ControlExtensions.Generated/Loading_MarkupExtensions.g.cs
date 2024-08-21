#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class Loading_MarkupExtensions
{
//================= Properties ======================//
 // IndicatorProperty

/*BindFromExpressionSetterGenerator*/
public static T Indicator<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Loading
   => control._set(Ursa.Controls.Loading.IndicatorProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Indicator<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Loading
=> control._setEx(Ursa.Controls.Loading.IndicatorProperty, ps, () => control.Indicator = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Indicator<T>(this T control, IBinding binding) where T : Ursa.Controls.Loading
   => control._set(Ursa.Controls.Loading.IndicatorProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Indicator<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Loading
   => control._set(Ursa.Controls.Loading.IndicatorProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Indicator<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Loading
=> control._setEx(Ursa.Controls.Loading.IndicatorProperty, ps, () => control.Indicator = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsLoadingProperty

/*BindFromExpressionSetterGenerator*/
public static T IsLoading<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Loading
   => control._set(Ursa.Controls.Loading.IsLoadingProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsLoading<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Loading
=> control._setEx(Ursa.Controls.Loading.IsLoadingProperty, ps, () => control.IsLoading = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsLoading<T>(this T control, IBinding binding) where T : Ursa.Controls.Loading
   => control._set(Ursa.Controls.Loading.IsLoadingProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsLoading<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Loading
   => control._set(Ursa.Controls.Loading.IsLoadingProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsLoading<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Loading
=> control._setEx(Ursa.Controls.Loading.IsLoadingProperty, ps, () => control.IsLoading = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // IndicatorProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Indicator<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.Loading
=> style._addSetter(Ursa.Controls.Loading.IndicatorProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Indicator<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Loading
=> style._addSetter(Ursa.Controls.Loading.IndicatorProperty, binding);


 // IsLoadingProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsLoading<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.Loading
=> style._addSetter(Ursa.Controls.Loading.IsLoadingProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsLoading<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Loading
=> style._addSetter(Ursa.Controls.Loading.IsLoadingProperty, binding);



}
