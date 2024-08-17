#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Skeleton = Ursa.Controls.Skeleton;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class SkeletonExtensions
{
public static T IsActive<T>(this T control, IBinding binding) where T : Ursa.Controls.Skeleton
   => control._set(Ursa.Controls.Skeleton.IsActiveProperty, binding);
public static T IsActive<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Skeleton
   => control._set(Ursa.Controls.Skeleton.IsActiveProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsActive<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Skeleton
   => control._set(Ursa.Controls.Skeleton.IsActiveProperty, func, onChanged, expression);
public static T IsActive<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Skeleton
=> control._setEx(Ursa.Controls.Skeleton.IsActiveProperty, ps, () => control.IsActive = value, bindingMode, converter, bindingSource);
public static T IsActive<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Skeleton
=> control._setEx(Ursa.Controls.Skeleton.IsActiveProperty, ps, () => control.IsActive = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsLoading<T>(this T control, IBinding binding) where T : Ursa.Controls.Skeleton
   => control._set(Ursa.Controls.Skeleton.IsLoadingProperty, binding);
public static T IsLoading<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Skeleton
   => control._set(Ursa.Controls.Skeleton.IsLoadingProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsLoading<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Skeleton
   => control._set(Ursa.Controls.Skeleton.IsLoadingProperty, func, onChanged, expression);
public static T IsLoading<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Skeleton
=> control._setEx(Ursa.Controls.Skeleton.IsLoadingProperty, ps, () => control.IsLoading = value, bindingMode, converter, bindingSource);
public static T IsLoading<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Skeleton
=> control._setEx(Ursa.Controls.Skeleton.IsLoadingProperty, ps, () => control.IsLoading = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

