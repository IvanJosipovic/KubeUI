#nullable enable
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;
using UrsaView = Ursa.Controls.UrsaView;

namespace Avalonia.Markup.Declarative;
public static partial class UrsaViewExtensions
{
public static T IsTitleBarVisible<T>(this T control, IBinding binding) where T : Ursa.Controls.UrsaView
   => control._set(Ursa.Controls.UrsaView.IsTitleBarVisibleProperty, binding);
public static T IsTitleBarVisible<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.UrsaView
   => control._set(Ursa.Controls.UrsaView.IsTitleBarVisibleProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsTitleBarVisible<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.UrsaView
   => control._set(Ursa.Controls.UrsaView.IsTitleBarVisibleProperty, func, onChanged, expression);
public static T IsTitleBarVisible<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaView
=> control._setEx(Ursa.Controls.UrsaView.IsTitleBarVisibleProperty, ps, () => control.IsTitleBarVisible = value, bindingMode, converter, bindingSource);
public static T IsTitleBarVisible<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaView
=> control._setEx(Ursa.Controls.UrsaView.IsTitleBarVisibleProperty, ps, () => control.IsTitleBarVisible = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T LeftContent<T>(this T control, IBinding binding) where T : Ursa.Controls.UrsaView
   => control._set(Ursa.Controls.UrsaView.LeftContentProperty, binding);
public static T LeftContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.UrsaView
   => control._set(Ursa.Controls.UrsaView.LeftContentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T LeftContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.UrsaView
   => control._set(Ursa.Controls.UrsaView.LeftContentProperty, func, onChanged, expression);
public static T LeftContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaView
=> control._setEx(Ursa.Controls.UrsaView.LeftContentProperty, ps, () => control.LeftContent = value, bindingMode, converter, bindingSource);
public static T LeftContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaView
=> control._setEx(Ursa.Controls.UrsaView.LeftContentProperty, ps, () => control.LeftContent = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T RightContent<T>(this T control, IBinding binding) where T : Ursa.Controls.UrsaView
   => control._set(Ursa.Controls.UrsaView.RightContentProperty, binding);
public static T RightContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.UrsaView
   => control._set(Ursa.Controls.UrsaView.RightContentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T RightContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.UrsaView
   => control._set(Ursa.Controls.UrsaView.RightContentProperty, func, onChanged, expression);
public static T RightContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaView
=> control._setEx(Ursa.Controls.UrsaView.RightContentProperty, ps, () => control.RightContent = value, bindingMode, converter, bindingSource);
public static T RightContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaView
=> control._setEx(Ursa.Controls.UrsaView.RightContentProperty, ps, () => control.RightContent = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T TitleBarContent<T>(this T control, IBinding binding) where T : Ursa.Controls.UrsaView
   => control._set(Ursa.Controls.UrsaView.TitleBarContentProperty, binding);
public static T TitleBarContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.UrsaView
   => control._set(Ursa.Controls.UrsaView.TitleBarContentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T TitleBarContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.UrsaView
   => control._set(Ursa.Controls.UrsaView.TitleBarContentProperty, func, onChanged, expression);
public static T TitleBarContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaView
=> control._setEx(Ursa.Controls.UrsaView.TitleBarContentProperty, ps, () => control.TitleBarContent = value, bindingMode, converter, bindingSource);
public static T TitleBarContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaView
=> control._setEx(Ursa.Controls.UrsaView.TitleBarContentProperty, ps, () => control.TitleBarContent = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T TitleBarMargin<T>(this T control, IBinding binding) where T : Ursa.Controls.UrsaView
   => control._set(Ursa.Controls.UrsaView.TitleBarMarginProperty, binding);
public static T TitleBarMargin<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.UrsaView
   => control._set(Ursa.Controls.UrsaView.TitleBarMarginProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T TitleBarMargin<T>(this T control, Func<Avalonia.Thickness> func, Action<Avalonia.Thickness>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.UrsaView
   => control._set(Ursa.Controls.UrsaView.TitleBarMarginProperty, func, onChanged, expression);
public static T TitleBarMargin<T>(this T control, Avalonia.Thickness value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaView
=> control._setEx(Ursa.Controls.UrsaView.TitleBarMarginProperty, ps, () => control.TitleBarMargin = value, bindingMode, converter, bindingSource);
public static T TitleBarMargin<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Thickness> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaView
=> control._setEx(Ursa.Controls.UrsaView.TitleBarMarginProperty, ps, () => control.TitleBarMargin = converter.TryConvert(value), bindingMode, converter, bindingSource);

public static T TitleBarMargin<T>(this T control, Double uniformLength = default) where T : Ursa.Controls.UrsaView
   => control._set(() => control.TitleBarMargin = new Avalonia.Thickness(uniformLength));
public static T TitleBarMargin<T>(this T control, Double horizontal = default, Double vertical = default) where T : Ursa.Controls.UrsaView
   => control._set(() => control.TitleBarMargin = new Avalonia.Thickness(horizontal, vertical));
public static T TitleBarMargin<T>(this T control, Double left = default, Double top = default, Double right = default, Double bottom = default) where T : Ursa.Controls.UrsaView
   => control._set(() => control.TitleBarMargin = new Avalonia.Thickness(left, top, right, bottom));
}

