#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using PathIconSource = FluentAvalonia.UI.Controls.PathIconSource;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class PathIconSourceExtensions
{
public static T Data<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.PathIconSource
   => control._set(FluentAvalonia.UI.Controls.PathIconSource.DataProperty, binding);
public static T Data<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.PathIconSource
   => control._set(FluentAvalonia.UI.Controls.PathIconSource.DataProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Data<T>(this T control, Func<Avalonia.Media.Geometry> func, Action<Avalonia.Media.Geometry>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.PathIconSource
   => control._set(FluentAvalonia.UI.Controls.PathIconSource.DataProperty, func, onChanged, expression);
public static T Data<T>(this T control, Avalonia.Media.Geometry value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.PathIconSource
=> control._setEx(FluentAvalonia.UI.Controls.PathIconSource.DataProperty, ps, () => control.Data = value, bindingMode, converter, bindingSource);
public static T Data<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.Geometry> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.PathIconSource
=> control._setEx(FluentAvalonia.UI.Controls.PathIconSource.DataProperty, ps, () => control.Data = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Stretch<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.PathIconSource
   => control._set(FluentAvalonia.UI.Controls.PathIconSource.StretchProperty, binding);
public static T Stretch<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.PathIconSource
   => control._set(FluentAvalonia.UI.Controls.PathIconSource.StretchProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Stretch<T>(this T control, Func<Avalonia.Media.Stretch> func, Action<Avalonia.Media.Stretch>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.PathIconSource
   => control._set(FluentAvalonia.UI.Controls.PathIconSource.StretchProperty, func, onChanged, expression);
public static T Stretch<T>(this T control, Avalonia.Media.Stretch value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.PathIconSource
=> control._setEx(FluentAvalonia.UI.Controls.PathIconSource.StretchProperty, ps, () => control.Stretch = value, bindingMode, converter, bindingSource);
public static T Stretch<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.Stretch> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.PathIconSource
=> control._setEx(FluentAvalonia.UI.Controls.PathIconSource.StretchProperty, ps, () => control.Stretch = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T StretchDirection<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.PathIconSource
   => control._set(FluentAvalonia.UI.Controls.PathIconSource.StretchDirectionProperty, binding);
public static T StretchDirection<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.PathIconSource
   => control._set(FluentAvalonia.UI.Controls.PathIconSource.StretchDirectionProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T StretchDirection<T>(this T control, Func<Avalonia.Media.StretchDirection> func, Action<Avalonia.Media.StretchDirection>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.PathIconSource
   => control._set(FluentAvalonia.UI.Controls.PathIconSource.StretchDirectionProperty, func, onChanged, expression);
public static T StretchDirection<T>(this T control, Avalonia.Media.StretchDirection value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.PathIconSource
=> control._setEx(FluentAvalonia.UI.Controls.PathIconSource.StretchDirectionProperty, ps, () => control.StretchDirection = value, bindingMode, converter, bindingSource);
public static T StretchDirection<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.StretchDirection> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.PathIconSource
=> control._setEx(FluentAvalonia.UI.Controls.PathIconSource.StretchDirectionProperty, ps, () => control.StretchDirection = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

