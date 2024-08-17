#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using InfoBadge = FluentAvalonia.UI.Controls.InfoBadge;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class InfoBadgeExtensions
{
public static T Value<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBadge
   => control._set(FluentAvalonia.UI.Controls.InfoBadge.ValueProperty, binding);
public static T Value<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.InfoBadge
   => control._set(FluentAvalonia.UI.Controls.InfoBadge.ValueProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Value<T>(this T control, Func<System.Int32> func, Action<System.Int32>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.InfoBadge
   => control._set(FluentAvalonia.UI.Controls.InfoBadge.ValueProperty, func, onChanged, expression);
public static T Value<T>(this T control, System.Int32 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBadge
=> control._setEx(FluentAvalonia.UI.Controls.InfoBadge.ValueProperty, ps, () => control.Value = value, bindingMode, converter, bindingSource);
public static T Value<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Int32> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBadge
=> control._setEx(FluentAvalonia.UI.Controls.InfoBadge.ValueProperty, ps, () => control.Value = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IconSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBadge
   => control._set(FluentAvalonia.UI.Controls.InfoBadge.IconSourceProperty, binding);
public static T IconSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.InfoBadge
   => control._set(FluentAvalonia.UI.Controls.InfoBadge.IconSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IconSource<T>(this T control, Func<FluentAvalonia.UI.Controls.IconSource> func, Action<FluentAvalonia.UI.Controls.IconSource>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.InfoBadge
   => control._set(FluentAvalonia.UI.Controls.InfoBadge.IconSourceProperty, func, onChanged, expression);
public static T IconSource<T>(this T control, FluentAvalonia.UI.Controls.IconSource value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBadge
=> control._setEx(FluentAvalonia.UI.Controls.InfoBadge.IconSourceProperty, ps, () => control.IconSource = value, bindingMode, converter, bindingSource);
public static T IconSource<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.IconSource> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBadge
=> control._setEx(FluentAvalonia.UI.Controls.InfoBadge.IconSourceProperty, ps, () => control.IconSource = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

