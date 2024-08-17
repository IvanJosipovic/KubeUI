#nullable enable
using Avalonia.Controls.Notifications;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Banner = Ursa.Controls.Banner;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class BannerExtensions
{
public static T CanClose<T>(this T control, IBinding binding) where T : Ursa.Controls.Banner
   => control._set(Ursa.Controls.Banner.CanCloseProperty, binding);
public static T CanClose<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Banner
   => control._set(Ursa.Controls.Banner.CanCloseProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CanClose<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Banner
   => control._set(Ursa.Controls.Banner.CanCloseProperty, func, onChanged, expression);
public static T CanClose<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Banner
=> control._setEx(Ursa.Controls.Banner.CanCloseProperty, ps, () => control.CanClose = value, bindingMode, converter, bindingSource);
public static T CanClose<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Banner
=> control._setEx(Ursa.Controls.Banner.CanCloseProperty, ps, () => control.CanClose = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ShowIcon<T>(this T control, IBinding binding) where T : Ursa.Controls.Banner
   => control._set(Ursa.Controls.Banner.ShowIconProperty, binding);
public static T ShowIcon<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Banner
   => control._set(Ursa.Controls.Banner.ShowIconProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ShowIcon<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Banner
   => control._set(Ursa.Controls.Banner.ShowIconProperty, func, onChanged, expression);
public static T ShowIcon<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Banner
=> control._setEx(Ursa.Controls.Banner.ShowIconProperty, ps, () => control.ShowIcon = value, bindingMode, converter, bindingSource);
public static T ShowIcon<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Banner
=> control._setEx(Ursa.Controls.Banner.ShowIconProperty, ps, () => control.ShowIcon = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Icon<T>(this T control, IBinding binding) where T : Ursa.Controls.Banner
   => control._set(Ursa.Controls.Banner.IconProperty, binding);
public static T Icon<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Banner
   => control._set(Ursa.Controls.Banner.IconProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Icon<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Banner
   => control._set(Ursa.Controls.Banner.IconProperty, func, onChanged, expression);
public static T Icon<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Banner
=> control._setEx(Ursa.Controls.Banner.IconProperty, ps, () => control.Icon = value, bindingMode, converter, bindingSource);
public static T Icon<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Banner
=> control._setEx(Ursa.Controls.Banner.IconProperty, ps, () => control.Icon = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Type<T>(this T control, IBinding binding) where T : Ursa.Controls.Banner
   => control._set(Ursa.Controls.Banner.TypeProperty, binding);
public static T Type<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Banner
   => control._set(Ursa.Controls.Banner.TypeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Type<T>(this T control, Func<Avalonia.Controls.Notifications.NotificationType> func, Action<Avalonia.Controls.Notifications.NotificationType>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Banner
   => control._set(Ursa.Controls.Banner.TypeProperty, func, onChanged, expression);
public static T Type<T>(this T control, Avalonia.Controls.Notifications.NotificationType value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Banner
=> control._setEx(Ursa.Controls.Banner.TypeProperty, ps, () => control.Type = value, bindingMode, converter, bindingSource);
public static T Type<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Notifications.NotificationType> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Banner
=> control._setEx(Ursa.Controls.Banner.TypeProperty, ps, () => control.Type = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

