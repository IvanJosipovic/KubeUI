#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using FAPathIcon = FluentAvalonia.UI.Controls.FAPathIcon;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class FAPathIconExtensions
{
public static T Data<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAPathIcon
   => control._set(FluentAvalonia.UI.Controls.FAPathIcon.DataProperty, binding);
public static T Data<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAPathIcon
   => control._set(FluentAvalonia.UI.Controls.FAPathIcon.DataProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Data<T>(this T control, Func<Avalonia.Media.Geometry> func, Action<Avalonia.Media.Geometry>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAPathIcon
   => control._set(FluentAvalonia.UI.Controls.FAPathIcon.DataProperty, func, onChanged, expression);
public static T Data<T>(this T control, Avalonia.Media.Geometry value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAPathIcon
=> control._setEx(FluentAvalonia.UI.Controls.FAPathIcon.DataProperty, ps, () => control.Data = value, bindingMode, converter, bindingSource);
public static T Data<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.Geometry> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAPathIcon
=> control._setEx(FluentAvalonia.UI.Controls.FAPathIcon.DataProperty, ps, () => control.Data = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Stretch<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAPathIcon
   => control._set(FluentAvalonia.UI.Controls.FAPathIcon.StretchProperty, binding);
public static T Stretch<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAPathIcon
   => control._set(FluentAvalonia.UI.Controls.FAPathIcon.StretchProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Stretch<T>(this T control, Func<Avalonia.Media.Stretch> func, Action<Avalonia.Media.Stretch>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAPathIcon
   => control._set(FluentAvalonia.UI.Controls.FAPathIcon.StretchProperty, func, onChanged, expression);
public static T Stretch<T>(this T control, Avalonia.Media.Stretch value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAPathIcon
=> control._setEx(FluentAvalonia.UI.Controls.FAPathIcon.StretchProperty, ps, () => control.Stretch = value, bindingMode, converter, bindingSource);
public static T Stretch<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.Stretch> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAPathIcon
=> control._setEx(FluentAvalonia.UI.Controls.FAPathIcon.StretchProperty, ps, () => control.Stretch = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T StretchDirection<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAPathIcon
   => control._set(FluentAvalonia.UI.Controls.FAPathIcon.StretchDirectionProperty, binding);
public static T StretchDirection<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAPathIcon
   => control._set(FluentAvalonia.UI.Controls.FAPathIcon.StretchDirectionProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T StretchDirection<T>(this T control, Func<Avalonia.Media.StretchDirection> func, Action<Avalonia.Media.StretchDirection>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAPathIcon
   => control._set(FluentAvalonia.UI.Controls.FAPathIcon.StretchDirectionProperty, func, onChanged, expression);
public static T StretchDirection<T>(this T control, Avalonia.Media.StretchDirection value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAPathIcon
=> control._setEx(FluentAvalonia.UI.Controls.FAPathIcon.StretchDirectionProperty, ps, () => control.StretchDirection = value, bindingMode, converter, bindingSource);
public static T StretchDirection<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.StretchDirection> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAPathIcon
=> control._setEx(FluentAvalonia.UI.Controls.FAPathIcon.StretchDirectionProperty, ps, () => control.StretchDirection = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

