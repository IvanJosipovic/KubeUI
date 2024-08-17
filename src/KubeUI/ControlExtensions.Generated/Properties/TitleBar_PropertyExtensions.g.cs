#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TitleBar = Ursa.Controls.TitleBar;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class TitleBarExtensions
{
public static T LeftContent<T>(this T control, IBinding binding) where T : Ursa.Controls.TitleBar
   => control._set(Ursa.Controls.TitleBar.LeftContentProperty, binding);
public static T LeftContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TitleBar
   => control._set(Ursa.Controls.TitleBar.LeftContentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T LeftContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TitleBar
   => control._set(Ursa.Controls.TitleBar.LeftContentProperty, func, onChanged, expression);
public static T LeftContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TitleBar
=> control._setEx(Ursa.Controls.TitleBar.LeftContentProperty, ps, () => control.LeftContent = value, bindingMode, converter, bindingSource);
public static T LeftContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TitleBar
=> control._setEx(Ursa.Controls.TitleBar.LeftContentProperty, ps, () => control.LeftContent = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T RightContent<T>(this T control, IBinding binding) where T : Ursa.Controls.TitleBar
   => control._set(Ursa.Controls.TitleBar.RightContentProperty, binding);
public static T RightContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TitleBar
   => control._set(Ursa.Controls.TitleBar.RightContentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T RightContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TitleBar
   => control._set(Ursa.Controls.TitleBar.RightContentProperty, func, onChanged, expression);
public static T RightContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TitleBar
=> control._setEx(Ursa.Controls.TitleBar.RightContentProperty, ps, () => control.RightContent = value, bindingMode, converter, bindingSource);
public static T RightContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TitleBar
=> control._setEx(Ursa.Controls.TitleBar.RightContentProperty, ps, () => control.RightContent = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

