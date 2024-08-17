#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using CommandBarButton = FluentAvalonia.UI.Controls.CommandBarButton;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class CommandBarButtonExtensions
{
public static T IconSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBarButton
   => control._set(FluentAvalonia.UI.Controls.CommandBarButton.IconSourceProperty, binding);
public static T IconSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.CommandBarButton
   => control._set(FluentAvalonia.UI.Controls.CommandBarButton.IconSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IconSource<T>(this T control, Func<FluentAvalonia.UI.Controls.IconSource> func, Action<FluentAvalonia.UI.Controls.IconSource>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.CommandBarButton
   => control._set(FluentAvalonia.UI.Controls.CommandBarButton.IconSourceProperty, func, onChanged, expression);
public static T IconSource<T>(this T control, FluentAvalonia.UI.Controls.IconSource value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBarButton
=> control._setEx(FluentAvalonia.UI.Controls.CommandBarButton.IconSourceProperty, ps, () => control.IconSource = value, bindingMode, converter, bindingSource);
public static T IconSource<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.IconSource> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBarButton
=> control._setEx(FluentAvalonia.UI.Controls.CommandBarButton.IconSourceProperty, ps, () => control.IconSource = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Label<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBarButton
   => control._set(FluentAvalonia.UI.Controls.CommandBarButton.LabelProperty, binding);
public static T Label<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.CommandBarButton
   => control._set(FluentAvalonia.UI.Controls.CommandBarButton.LabelProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Label<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.CommandBarButton
   => control._set(FluentAvalonia.UI.Controls.CommandBarButton.LabelProperty, func, onChanged, expression);
public static T Label<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBarButton
=> control._setEx(FluentAvalonia.UI.Controls.CommandBarButton.LabelProperty, ps, () => control.Label = value, bindingMode, converter, bindingSource);
public static T Label<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBarButton
=> control._setEx(FluentAvalonia.UI.Controls.CommandBarButton.LabelProperty, ps, () => control.Label = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T DynamicOverflowOrder<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBarButton
   => control._set(FluentAvalonia.UI.Controls.CommandBarButton.DynamicOverflowOrderProperty, binding);
public static T DynamicOverflowOrder<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.CommandBarButton
   => control._set(FluentAvalonia.UI.Controls.CommandBarButton.DynamicOverflowOrderProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T DynamicOverflowOrder<T>(this T control, Func<System.Int32> func, Action<System.Int32>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.CommandBarButton
   => control._set(FluentAvalonia.UI.Controls.CommandBarButton.DynamicOverflowOrderProperty, func, onChanged, expression);
public static T DynamicOverflowOrder<T>(this T control, System.Int32 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBarButton
=> control._setEx(FluentAvalonia.UI.Controls.CommandBarButton.DynamicOverflowOrderProperty, ps, () => control.DynamicOverflowOrder = value, bindingMode, converter, bindingSource);
public static T DynamicOverflowOrder<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Int32> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBarButton
=> control._setEx(FluentAvalonia.UI.Controls.CommandBarButton.DynamicOverflowOrderProperty, ps, () => control.DynamicOverflowOrder = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsCompact<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBarButton
   => control._set(FluentAvalonia.UI.Controls.CommandBarButton.IsCompactProperty, binding);
public static T IsCompact<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.CommandBarButton
   => control._set(FluentAvalonia.UI.Controls.CommandBarButton.IsCompactProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsCompact<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.CommandBarButton
   => control._set(FluentAvalonia.UI.Controls.CommandBarButton.IsCompactProperty, func, onChanged, expression);
public static T IsCompact<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBarButton
=> control._setEx(FluentAvalonia.UI.Controls.CommandBarButton.IsCompactProperty, ps, () => control.IsCompact = value, bindingMode, converter, bindingSource);
public static T IsCompact<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBarButton
=> control._setEx(FluentAvalonia.UI.Controls.CommandBarButton.IsCompactProperty, ps, () => control.IsCompact = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

