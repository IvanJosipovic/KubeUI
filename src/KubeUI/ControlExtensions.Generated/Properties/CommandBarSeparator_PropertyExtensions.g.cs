#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using CommandBarSeparator = FluentAvalonia.UI.Controls.CommandBarSeparator;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class CommandBarSeparatorExtensions
{
public static T DynamicOverflowOrder<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBarSeparator
   => control._set(FluentAvalonia.UI.Controls.CommandBarSeparator.DynamicOverflowOrderProperty, binding);
public static T DynamicOverflowOrder<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.CommandBarSeparator
   => control._set(FluentAvalonia.UI.Controls.CommandBarSeparator.DynamicOverflowOrderProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T DynamicOverflowOrder<T>(this T control, Func<System.Int32> func, Action<System.Int32>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.CommandBarSeparator
   => control._set(FluentAvalonia.UI.Controls.CommandBarSeparator.DynamicOverflowOrderProperty, func, onChanged, expression);
public static T DynamicOverflowOrder<T>(this T control, System.Int32 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBarSeparator
=> control._setEx(FluentAvalonia.UI.Controls.CommandBarSeparator.DynamicOverflowOrderProperty, ps, () => control.DynamicOverflowOrder = value, bindingMode, converter, bindingSource);
public static T DynamicOverflowOrder<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Int32> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBarSeparator
=> control._setEx(FluentAvalonia.UI.Controls.CommandBarSeparator.DynamicOverflowOrderProperty, ps, () => control.DynamicOverflowOrder = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsCompact<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBarSeparator
   => control._set(FluentAvalonia.UI.Controls.CommandBarSeparator.IsCompactProperty, binding);
public static T IsCompact<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.CommandBarSeparator
   => control._set(FluentAvalonia.UI.Controls.CommandBarSeparator.IsCompactProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsCompact<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.CommandBarSeparator
   => control._set(FluentAvalonia.UI.Controls.CommandBarSeparator.IsCompactProperty, func, onChanged, expression);
public static T IsCompact<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBarSeparator
=> control._setEx(FluentAvalonia.UI.Controls.CommandBarSeparator.IsCompactProperty, ps, () => control.IsCompact = value, bindingMode, converter, bindingSource);
public static T IsCompact<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBarSeparator
=> control._setEx(FluentAvalonia.UI.Controls.CommandBarSeparator.IsCompactProperty, ps, () => control.IsCompact = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

