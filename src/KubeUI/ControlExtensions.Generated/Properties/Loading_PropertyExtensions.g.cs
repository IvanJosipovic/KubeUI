#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Loading = Ursa.Controls.Loading;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class LoadingExtensions
{
public static T Indicator<T>(this T control, IBinding binding) where T : Ursa.Controls.Loading
   => control._set(Ursa.Controls.Loading.IndicatorProperty, binding);
public static T Indicator<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Loading
   => control._set(Ursa.Controls.Loading.IndicatorProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Indicator<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Loading
   => control._set(Ursa.Controls.Loading.IndicatorProperty, func, onChanged, expression);
public static T Indicator<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Loading
=> control._setEx(Ursa.Controls.Loading.IndicatorProperty, ps, () => control.Indicator = value, bindingMode, converter, bindingSource);
public static T Indicator<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Loading
=> control._setEx(Ursa.Controls.Loading.IndicatorProperty, ps, () => control.Indicator = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsLoading<T>(this T control, IBinding binding) where T : Ursa.Controls.Loading
   => control._set(Ursa.Controls.Loading.IsLoadingProperty, binding);
public static T IsLoading<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Loading
   => control._set(Ursa.Controls.Loading.IsLoadingProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsLoading<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Loading
   => control._set(Ursa.Controls.Loading.IsLoadingProperty, func, onChanged, expression);
public static T IsLoading<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Loading
=> control._setEx(Ursa.Controls.Loading.IsLoadingProperty, ps, () => control.IsLoading = value, bindingMode, converter, bindingSource);
public static T IsLoading<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Loading
=> control._setEx(Ursa.Controls.Loading.IsLoadingProperty, ps, () => control.IsLoading = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

