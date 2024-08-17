#nullable enable
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using DualBadge = Ursa.Controls.DualBadge;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class DualBadgeExtensions
{
public static T Icon<T>(this T control, IBinding binding) where T : Ursa.Controls.DualBadge
   => control._set(Ursa.Controls.DualBadge.IconProperty, binding);
public static T Icon<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DualBadge
   => control._set(Ursa.Controls.DualBadge.IconProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Icon<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DualBadge
   => control._set(Ursa.Controls.DualBadge.IconProperty, func, onChanged, expression);
public static T Icon<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DualBadge
=> control._setEx(Ursa.Controls.DualBadge.IconProperty, ps, () => control.Icon = value, bindingMode, converter, bindingSource);
public static T Icon<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DualBadge
=> control._setEx(Ursa.Controls.DualBadge.IconProperty, ps, () => control.Icon = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IconTemplate<T>(this T control, IBinding binding) where T : Ursa.Controls.DualBadge
   => control._set(Ursa.Controls.DualBadge.IconTemplateProperty, binding);
public static T IconTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DualBadge
   => control._set(Ursa.Controls.DualBadge.IconTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IconTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DualBadge
   => control._set(Ursa.Controls.DualBadge.IconTemplateProperty, func, onChanged, expression);
public static T IconTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DualBadge
=> control._setEx(Ursa.Controls.DualBadge.IconTemplateProperty, ps, () => control.IconTemplate = value, bindingMode, converter, bindingSource);
public static T IconTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DualBadge
=> control._setEx(Ursa.Controls.DualBadge.IconTemplateProperty, ps, () => control.IconTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IconForeground<T>(this T control, IBinding binding) where T : Ursa.Controls.DualBadge
   => control._set(Ursa.Controls.DualBadge.IconForegroundProperty, binding);
public static T IconForeground<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DualBadge
   => control._set(Ursa.Controls.DualBadge.IconForegroundProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IconForeground<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DualBadge
   => control._set(Ursa.Controls.DualBadge.IconForegroundProperty, func, onChanged, expression);
public static T IconForeground<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DualBadge
=> control._setEx(Ursa.Controls.DualBadge.IconForegroundProperty, ps, () => control.IconForeground = value, bindingMode, converter, bindingSource);
public static T IconForeground<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DualBadge
=> control._setEx(Ursa.Controls.DualBadge.IconForegroundProperty, ps, () => control.IconForeground = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T HeaderForeground<T>(this T control, IBinding binding) where T : Ursa.Controls.DualBadge
   => control._set(Ursa.Controls.DualBadge.HeaderForegroundProperty, binding);
public static T HeaderForeground<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DualBadge
   => control._set(Ursa.Controls.DualBadge.HeaderForegroundProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T HeaderForeground<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DualBadge
   => control._set(Ursa.Controls.DualBadge.HeaderForegroundProperty, func, onChanged, expression);
public static T HeaderForeground<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DualBadge
=> control._setEx(Ursa.Controls.DualBadge.HeaderForegroundProperty, ps, () => control.HeaderForeground = value, bindingMode, converter, bindingSource);
public static T HeaderForeground<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DualBadge
=> control._setEx(Ursa.Controls.DualBadge.HeaderForegroundProperty, ps, () => control.HeaderForeground = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T HeaderBackground<T>(this T control, IBinding binding) where T : Ursa.Controls.DualBadge
   => control._set(Ursa.Controls.DualBadge.HeaderBackgroundProperty, binding);
public static T HeaderBackground<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DualBadge
   => control._set(Ursa.Controls.DualBadge.HeaderBackgroundProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T HeaderBackground<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DualBadge
   => control._set(Ursa.Controls.DualBadge.HeaderBackgroundProperty, func, onChanged, expression);
public static T HeaderBackground<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DualBadge
=> control._setEx(Ursa.Controls.DualBadge.HeaderBackgroundProperty, ps, () => control.HeaderBackground = value, bindingMode, converter, bindingSource);
public static T HeaderBackground<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DualBadge
=> control._setEx(Ursa.Controls.DualBadge.HeaderBackgroundProperty, ps, () => control.HeaderBackground = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

