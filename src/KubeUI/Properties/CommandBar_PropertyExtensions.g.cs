#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using CommandBar = FluentAvalonia.UI.Controls.CommandBar;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class CommandBarExtensions
{
public static T IsSticky<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.IsStickyProperty, binding);
public static T IsSticky<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.IsStickyProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsSticky<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.IsStickyProperty, func, onChanged, expression);
public static T IsSticky<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBar
=> control._setEx(FluentAvalonia.UI.Controls.CommandBar.IsStickyProperty, ps, () => control.IsSticky = value, bindingMode, converter, bindingSource);
public static T IsSticky<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBar
=> control._setEx(FluentAvalonia.UI.Controls.CommandBar.IsStickyProperty, ps, () => control.IsSticky = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsOpen<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.IsOpenProperty, binding);
public static T IsOpen<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.IsOpenProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsOpen<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.IsOpenProperty, func, onChanged, expression);
public static T IsOpen<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBar
=> control._setEx(FluentAvalonia.UI.Controls.CommandBar.IsOpenProperty, ps, () => control.IsOpen = value, bindingMode, converter, bindingSource);
public static T IsOpen<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBar
=> control._setEx(FluentAvalonia.UI.Controls.CommandBar.IsOpenProperty, ps, () => control.IsOpen = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ClosedDisplayMode<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.ClosedDisplayModeProperty, binding);
public static T ClosedDisplayMode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.ClosedDisplayModeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ClosedDisplayMode<T>(this T control, Func<FluentAvalonia.UI.Controls.CommandBarClosedDisplayMode> func, Action<FluentAvalonia.UI.Controls.CommandBarClosedDisplayMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.ClosedDisplayModeProperty, func, onChanged, expression);
public static T ClosedDisplayMode<T>(this T control, FluentAvalonia.UI.Controls.CommandBarClosedDisplayMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBar
=> control._setEx(FluentAvalonia.UI.Controls.CommandBar.ClosedDisplayModeProperty, ps, () => control.ClosedDisplayMode = value, bindingMode, converter, bindingSource);
public static T ClosedDisplayMode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.CommandBarClosedDisplayMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBar
=> control._setEx(FluentAvalonia.UI.Controls.CommandBar.ClosedDisplayModeProperty, ps, () => control.ClosedDisplayMode = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T OverflowButtonVisibility<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.OverflowButtonVisibilityProperty, binding);
public static T OverflowButtonVisibility<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.OverflowButtonVisibilityProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T OverflowButtonVisibility<T>(this T control, Func<FluentAvalonia.UI.Controls.CommandBarOverflowButtonVisibility> func, Action<FluentAvalonia.UI.Controls.CommandBarOverflowButtonVisibility>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.OverflowButtonVisibilityProperty, func, onChanged, expression);
public static T OverflowButtonVisibility<T>(this T control, FluentAvalonia.UI.Controls.CommandBarOverflowButtonVisibility value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBar
=> control._setEx(FluentAvalonia.UI.Controls.CommandBar.OverflowButtonVisibilityProperty, ps, () => control.OverflowButtonVisibility = value, bindingMode, converter, bindingSource);
public static T OverflowButtonVisibility<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.CommandBarOverflowButtonVisibility> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBar
=> control._setEx(FluentAvalonia.UI.Controls.CommandBar.OverflowButtonVisibilityProperty, ps, () => control.OverflowButtonVisibility = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsDynamicOverflowEnabled<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.IsDynamicOverflowEnabledProperty, binding);
public static T IsDynamicOverflowEnabled<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.IsDynamicOverflowEnabledProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsDynamicOverflowEnabled<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.IsDynamicOverflowEnabledProperty, func, onChanged, expression);
public static T IsDynamicOverflowEnabled<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBar
=> control._setEx(FluentAvalonia.UI.Controls.CommandBar.IsDynamicOverflowEnabledProperty, ps, () => control.IsDynamicOverflowEnabled = value, bindingMode, converter, bindingSource);
public static T IsDynamicOverflowEnabled<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBar
=> control._setEx(FluentAvalonia.UI.Controls.CommandBar.IsDynamicOverflowEnabledProperty, ps, () => control.IsDynamicOverflowEnabled = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ItemsAlignment<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.ItemsAlignmentProperty, binding);
public static T ItemsAlignment<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.ItemsAlignmentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ItemsAlignment<T>(this T control, Func<FluentAvalonia.UI.Controls.CommandBarItemsAlignment> func, Action<FluentAvalonia.UI.Controls.CommandBarItemsAlignment>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.ItemsAlignmentProperty, func, onChanged, expression);
public static T ItemsAlignment<T>(this T control, FluentAvalonia.UI.Controls.CommandBarItemsAlignment value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBar
=> control._setEx(FluentAvalonia.UI.Controls.CommandBar.ItemsAlignmentProperty, ps, () => control.ItemsAlignment = value, bindingMode, converter, bindingSource);
public static T ItemsAlignment<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.CommandBarItemsAlignment> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBar
=> control._setEx(FluentAvalonia.UI.Controls.CommandBar.ItemsAlignmentProperty, ps, () => control.ItemsAlignment = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T DefaultLabelPosition<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.DefaultLabelPositionProperty, binding);
public static T DefaultLabelPosition<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.DefaultLabelPositionProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T DefaultLabelPosition<T>(this T control, Func<FluentAvalonia.UI.Controls.CommandBarDefaultLabelPosition> func, Action<FluentAvalonia.UI.Controls.CommandBarDefaultLabelPosition>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.DefaultLabelPositionProperty, func, onChanged, expression);
public static T DefaultLabelPosition<T>(this T control, FluentAvalonia.UI.Controls.CommandBarDefaultLabelPosition value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBar
=> control._setEx(FluentAvalonia.UI.Controls.CommandBar.DefaultLabelPositionProperty, ps, () => control.DefaultLabelPosition = value, bindingMode, converter, bindingSource);
public static T DefaultLabelPosition<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.CommandBarDefaultLabelPosition> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBar
=> control._setEx(FluentAvalonia.UI.Controls.CommandBar.DefaultLabelPositionProperty, ps, () => control.DefaultLabelPosition = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

