#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using FlowLayout = FluentAvalonia.UI.Controls.FlowLayout;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class FlowLayoutExtensions
{
public static T LineAlignment<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FlowLayout
   => control._set(FluentAvalonia.UI.Controls.FlowLayout.LineAlignmentProperty, binding);
public static T LineAlignment<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FlowLayout
   => control._set(FluentAvalonia.UI.Controls.FlowLayout.LineAlignmentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T LineAlignment<T>(this T control, Func<FluentAvalonia.UI.Controls.FlowLayoutLineAlignment> func, Action<FluentAvalonia.UI.Controls.FlowLayoutLineAlignment>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FlowLayout
   => control._set(FluentAvalonia.UI.Controls.FlowLayout.LineAlignmentProperty, func, onChanged, expression);
public static T LineAlignment<T>(this T control, FluentAvalonia.UI.Controls.FlowLayoutLineAlignment value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FlowLayout
=> control._setEx(FluentAvalonia.UI.Controls.FlowLayout.LineAlignmentProperty, ps, () => control.LineAlignment = value, bindingMode, converter, bindingSource);
public static T LineAlignment<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.FlowLayoutLineAlignment> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FlowLayout
=> control._setEx(FluentAvalonia.UI.Controls.FlowLayout.LineAlignmentProperty, ps, () => control.LineAlignment = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T MinColumnSpacing<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FlowLayout
   => control._set(FluentAvalonia.UI.Controls.FlowLayout.MinColumnSpacingProperty, binding);
public static T MinColumnSpacing<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FlowLayout
   => control._set(FluentAvalonia.UI.Controls.FlowLayout.MinColumnSpacingProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T MinColumnSpacing<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FlowLayout
   => control._set(FluentAvalonia.UI.Controls.FlowLayout.MinColumnSpacingProperty, func, onChanged, expression);
public static T MinColumnSpacing<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FlowLayout
=> control._setEx(FluentAvalonia.UI.Controls.FlowLayout.MinColumnSpacingProperty, ps, () => control.MinColumnSpacing = value, bindingMode, converter, bindingSource);
public static T MinColumnSpacing<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FlowLayout
=> control._setEx(FluentAvalonia.UI.Controls.FlowLayout.MinColumnSpacingProperty, ps, () => control.MinColumnSpacing = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T MinRowSpacing<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FlowLayout
   => control._set(FluentAvalonia.UI.Controls.FlowLayout.MinRowSpacingProperty, binding);
public static T MinRowSpacing<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FlowLayout
   => control._set(FluentAvalonia.UI.Controls.FlowLayout.MinRowSpacingProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T MinRowSpacing<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FlowLayout
   => control._set(FluentAvalonia.UI.Controls.FlowLayout.MinRowSpacingProperty, func, onChanged, expression);
public static T MinRowSpacing<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FlowLayout
=> control._setEx(FluentAvalonia.UI.Controls.FlowLayout.MinRowSpacingProperty, ps, () => control.MinRowSpacing = value, bindingMode, converter, bindingSource);
public static T MinRowSpacing<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FlowLayout
=> control._setEx(FluentAvalonia.UI.Controls.FlowLayout.MinRowSpacingProperty, ps, () => control.MinRowSpacing = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Orientation<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FlowLayout
   => control._set(FluentAvalonia.UI.Controls.FlowLayout.OrientationProperty, binding);
public static T Orientation<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FlowLayout
   => control._set(FluentAvalonia.UI.Controls.FlowLayout.OrientationProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Orientation<T>(this T control, Func<Avalonia.Layout.Orientation> func, Action<Avalonia.Layout.Orientation>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FlowLayout
   => control._set(FluentAvalonia.UI.Controls.FlowLayout.OrientationProperty, func, onChanged, expression);
public static T Orientation<T>(this T control, Avalonia.Layout.Orientation value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FlowLayout
=> control._setEx(FluentAvalonia.UI.Controls.FlowLayout.OrientationProperty, ps, () => control.Orientation = value, bindingMode, converter, bindingSource);
public static T Orientation<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Layout.Orientation> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FlowLayout
=> control._setEx(FluentAvalonia.UI.Controls.FlowLayout.OrientationProperty, ps, () => control.Orientation = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

