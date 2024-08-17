#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using UniformGridLayout = FluentAvalonia.UI.Controls.UniformGridLayout;

namespace Avalonia.Markup.Declarative;
public static partial class UniformGridLayoutExtensions
{
public static T Orientation<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.UniformGridLayout
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.OrientationProperty, binding);
public static T Orientation<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.OrientationProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Orientation<T>(this T control, Func<Avalonia.Layout.Orientation> func, Action<Avalonia.Layout.Orientation>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.OrientationProperty, func, onChanged, expression);
public static T Orientation<T>(this T control, Avalonia.Layout.Orientation value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout
=> control._setEx(FluentAvalonia.UI.Controls.UniformGridLayout.OrientationProperty, ps, () => control.Orientation = value, bindingMode, converter, bindingSource);
public static T Orientation<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Layout.Orientation> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout
=> control._setEx(FluentAvalonia.UI.Controls.UniformGridLayout.OrientationProperty, ps, () => control.Orientation = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T MinItemWidth<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.UniformGridLayout
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.MinItemWidthProperty, binding);
public static T MinItemWidth<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.MinItemWidthProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T MinItemWidth<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.MinItemWidthProperty, func, onChanged, expression);
public static T MinItemWidth<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout
=> control._setEx(FluentAvalonia.UI.Controls.UniformGridLayout.MinItemWidthProperty, ps, () => control.MinItemWidth = value, bindingMode, converter, bindingSource);
public static T MinItemWidth<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout
=> control._setEx(FluentAvalonia.UI.Controls.UniformGridLayout.MinItemWidthProperty, ps, () => control.MinItemWidth = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T MinItemHeight<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.UniformGridLayout
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.MinItemHeightProperty, binding);
public static T MinItemHeight<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.MinItemHeightProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T MinItemHeight<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.MinItemHeightProperty, func, onChanged, expression);
public static T MinItemHeight<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout
=> control._setEx(FluentAvalonia.UI.Controls.UniformGridLayout.MinItemHeightProperty, ps, () => control.MinItemHeight = value, bindingMode, converter, bindingSource);
public static T MinItemHeight<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout
=> control._setEx(FluentAvalonia.UI.Controls.UniformGridLayout.MinItemHeightProperty, ps, () => control.MinItemHeight = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T MinRowSpacing<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.UniformGridLayout
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.MinRowSpacingProperty, binding);
public static T MinRowSpacing<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.MinRowSpacingProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T MinRowSpacing<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.MinRowSpacingProperty, func, onChanged, expression);
public static T MinRowSpacing<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout
=> control._setEx(FluentAvalonia.UI.Controls.UniformGridLayout.MinRowSpacingProperty, ps, () => control.MinRowSpacing = value, bindingMode, converter, bindingSource);
public static T MinRowSpacing<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout
=> control._setEx(FluentAvalonia.UI.Controls.UniformGridLayout.MinRowSpacingProperty, ps, () => control.MinRowSpacing = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T MinColumnSpacing<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.UniformGridLayout
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.MinColumnSpacingProperty, binding);
public static T MinColumnSpacing<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.MinColumnSpacingProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T MinColumnSpacing<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.MinColumnSpacingProperty, func, onChanged, expression);
public static T MinColumnSpacing<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout
=> control._setEx(FluentAvalonia.UI.Controls.UniformGridLayout.MinColumnSpacingProperty, ps, () => control.MinColumnSpacing = value, bindingMode, converter, bindingSource);
public static T MinColumnSpacing<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout
=> control._setEx(FluentAvalonia.UI.Controls.UniformGridLayout.MinColumnSpacingProperty, ps, () => control.MinColumnSpacing = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ItemsJustification<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.UniformGridLayout
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.ItemsJustificationProperty, binding);
public static T ItemsJustification<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.ItemsJustificationProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ItemsJustification<T>(this T control, Func<FluentAvalonia.UI.Controls.UniformGridLayoutItemsJustification> func, Action<FluentAvalonia.UI.Controls.UniformGridLayoutItemsJustification>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.ItemsJustificationProperty, func, onChanged, expression);
public static T ItemsJustification<T>(this T control, FluentAvalonia.UI.Controls.UniformGridLayoutItemsJustification value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout
=> control._setEx(FluentAvalonia.UI.Controls.UniformGridLayout.ItemsJustificationProperty, ps, () => control.ItemsJustification = value, bindingMode, converter, bindingSource);
public static T ItemsJustification<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.UniformGridLayoutItemsJustification> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout
=> control._setEx(FluentAvalonia.UI.Controls.UniformGridLayout.ItemsJustificationProperty, ps, () => control.ItemsJustification = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ItemsStretch<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.UniformGridLayout
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.ItemsStretchProperty, binding);
public static T ItemsStretch<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.ItemsStretchProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ItemsStretch<T>(this T control, Func<FluentAvalonia.UI.Controls.UniformGridLayoutItemsStretch> func, Action<FluentAvalonia.UI.Controls.UniformGridLayoutItemsStretch>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.ItemsStretchProperty, func, onChanged, expression);
public static T ItemsStretch<T>(this T control, FluentAvalonia.UI.Controls.UniformGridLayoutItemsStretch value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout
=> control._setEx(FluentAvalonia.UI.Controls.UniformGridLayout.ItemsStretchProperty, ps, () => control.ItemsStretch = value, bindingMode, converter, bindingSource);
public static T ItemsStretch<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.UniformGridLayoutItemsStretch> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout
=> control._setEx(FluentAvalonia.UI.Controls.UniformGridLayout.ItemsStretchProperty, ps, () => control.ItemsStretch = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T MaximumRowsOrColumns<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.UniformGridLayout
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.MaximumRowsOrColumnsProperty, binding);
public static T MaximumRowsOrColumns<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.MaximumRowsOrColumnsProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T MaximumRowsOrColumns<T>(this T control, Func<System.Int32> func, Action<System.Int32>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.MaximumRowsOrColumnsProperty, func, onChanged, expression);
public static T MaximumRowsOrColumns<T>(this T control, System.Int32 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout
=> control._setEx(FluentAvalonia.UI.Controls.UniformGridLayout.MaximumRowsOrColumnsProperty, ps, () => control.MaximumRowsOrColumns = value, bindingMode, converter, bindingSource);
public static T MaximumRowsOrColumns<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Int32> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout
=> control._setEx(FluentAvalonia.UI.Controls.UniformGridLayout.MaximumRowsOrColumnsProperty, ps, () => control.MaximumRowsOrColumns = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

