#nullable enable
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;
using UrsaWindow = Ursa.Controls.UrsaWindow;

namespace Avalonia.Markup.Declarative;
public static partial class UrsaWindowExtensions
{
public static T IsFullScreenButtonVisible<T>(this T control, IBinding binding) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.IsFullScreenButtonVisibleProperty, binding);
public static T IsFullScreenButtonVisible<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.IsFullScreenButtonVisibleProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsFullScreenButtonVisible<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.IsFullScreenButtonVisibleProperty, func, onChanged, expression);
public static T IsFullScreenButtonVisible<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaWindow
=> control._setEx(Ursa.Controls.UrsaWindow.IsFullScreenButtonVisibleProperty, ps, () => control.IsFullScreenButtonVisible = value, bindingMode, converter, bindingSource);
public static T IsFullScreenButtonVisible<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaWindow
=> control._setEx(Ursa.Controls.UrsaWindow.IsFullScreenButtonVisibleProperty, ps, () => control.IsFullScreenButtonVisible = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsMinimizeButtonVisible<T>(this T control, IBinding binding) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.IsMinimizeButtonVisibleProperty, binding);
public static T IsMinimizeButtonVisible<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.IsMinimizeButtonVisibleProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsMinimizeButtonVisible<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.IsMinimizeButtonVisibleProperty, func, onChanged, expression);
public static T IsMinimizeButtonVisible<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaWindow
=> control._setEx(Ursa.Controls.UrsaWindow.IsMinimizeButtonVisibleProperty, ps, () => control.IsMinimizeButtonVisible = value, bindingMode, converter, bindingSource);
public static T IsMinimizeButtonVisible<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaWindow
=> control._setEx(Ursa.Controls.UrsaWindow.IsMinimizeButtonVisibleProperty, ps, () => control.IsMinimizeButtonVisible = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsRestoreButtonVisible<T>(this T control, IBinding binding) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.IsRestoreButtonVisibleProperty, binding);
public static T IsRestoreButtonVisible<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.IsRestoreButtonVisibleProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsRestoreButtonVisible<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.IsRestoreButtonVisibleProperty, func, onChanged, expression);
public static T IsRestoreButtonVisible<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaWindow
=> control._setEx(Ursa.Controls.UrsaWindow.IsRestoreButtonVisibleProperty, ps, () => control.IsRestoreButtonVisible = value, bindingMode, converter, bindingSource);
public static T IsRestoreButtonVisible<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaWindow
=> control._setEx(Ursa.Controls.UrsaWindow.IsRestoreButtonVisibleProperty, ps, () => control.IsRestoreButtonVisible = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsCloseButtonVisible<T>(this T control, IBinding binding) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.IsCloseButtonVisibleProperty, binding);
public static T IsCloseButtonVisible<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.IsCloseButtonVisibleProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsCloseButtonVisible<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.IsCloseButtonVisibleProperty, func, onChanged, expression);
public static T IsCloseButtonVisible<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaWindow
=> control._setEx(Ursa.Controls.UrsaWindow.IsCloseButtonVisibleProperty, ps, () => control.IsCloseButtonVisible = value, bindingMode, converter, bindingSource);
public static T IsCloseButtonVisible<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaWindow
=> control._setEx(Ursa.Controls.UrsaWindow.IsCloseButtonVisibleProperty, ps, () => control.IsCloseButtonVisible = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsTitleBarVisible<T>(this T control, IBinding binding) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.IsTitleBarVisibleProperty, binding);
public static T IsTitleBarVisible<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.IsTitleBarVisibleProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsTitleBarVisible<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.IsTitleBarVisibleProperty, func, onChanged, expression);
public static T IsTitleBarVisible<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaWindow
=> control._setEx(Ursa.Controls.UrsaWindow.IsTitleBarVisibleProperty, ps, () => control.IsTitleBarVisible = value, bindingMode, converter, bindingSource);
public static T IsTitleBarVisible<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaWindow
=> control._setEx(Ursa.Controls.UrsaWindow.IsTitleBarVisibleProperty, ps, () => control.IsTitleBarVisible = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T TitleBarContent<T>(this T control, IBinding binding) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.TitleBarContentProperty, binding);
public static T TitleBarContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.TitleBarContentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T TitleBarContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.TitleBarContentProperty, func, onChanged, expression);
public static T TitleBarContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaWindow
=> control._setEx(Ursa.Controls.UrsaWindow.TitleBarContentProperty, ps, () => control.TitleBarContent = value, bindingMode, converter, bindingSource);
public static T TitleBarContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaWindow
=> control._setEx(Ursa.Controls.UrsaWindow.TitleBarContentProperty, ps, () => control.TitleBarContent = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T LeftContent<T>(this T control, IBinding binding) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.LeftContentProperty, binding);
public static T LeftContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.LeftContentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T LeftContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.LeftContentProperty, func, onChanged, expression);
public static T LeftContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaWindow
=> control._setEx(Ursa.Controls.UrsaWindow.LeftContentProperty, ps, () => control.LeftContent = value, bindingMode, converter, bindingSource);
public static T LeftContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaWindow
=> control._setEx(Ursa.Controls.UrsaWindow.LeftContentProperty, ps, () => control.LeftContent = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T RightContent<T>(this T control, IBinding binding) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.RightContentProperty, binding);
public static T RightContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.RightContentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T RightContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.RightContentProperty, func, onChanged, expression);
public static T RightContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaWindow
=> control._setEx(Ursa.Controls.UrsaWindow.RightContentProperty, ps, () => control.RightContent = value, bindingMode, converter, bindingSource);
public static T RightContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaWindow
=> control._setEx(Ursa.Controls.UrsaWindow.RightContentProperty, ps, () => control.RightContent = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T TitleBarMargin<T>(this T control, IBinding binding) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.TitleBarMarginProperty, binding);
public static T TitleBarMargin<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.TitleBarMarginProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T TitleBarMargin<T>(this T control, Func<Avalonia.Thickness> func, Action<Avalonia.Thickness>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.TitleBarMarginProperty, func, onChanged, expression);
public static T TitleBarMargin<T>(this T control, Avalonia.Thickness value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaWindow
=> control._setEx(Ursa.Controls.UrsaWindow.TitleBarMarginProperty, ps, () => control.TitleBarMargin = value, bindingMode, converter, bindingSource);
public static T TitleBarMargin<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Thickness> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaWindow
=> control._setEx(Ursa.Controls.UrsaWindow.TitleBarMarginProperty, ps, () => control.TitleBarMargin = converter.TryConvert(value), bindingMode, converter, bindingSource);

public static T TitleBarMargin<T>(this T control, Double uniformLength = default) where T : Ursa.Controls.UrsaWindow
   => control._set(() => control.TitleBarMargin = new Avalonia.Thickness(uniformLength));
public static T TitleBarMargin<T>(this T control, Double horizontal = default, Double vertical = default) where T : Ursa.Controls.UrsaWindow
   => control._set(() => control.TitleBarMargin = new Avalonia.Thickness(horizontal, vertical));
public static T TitleBarMargin<T>(this T control, Double left = default, Double top = default, Double right = default, Double bottom = default) where T : Ursa.Controls.UrsaWindow
   => control._set(() => control.TitleBarMargin = new Avalonia.Thickness(left, top, right, bottom));
}

