#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using FluentAvalonia.UI.Controls;
using StackLayout = FluentAvalonia.UI.Controls.StackLayout;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class StackLayoutExtensions
{
public static T Spacing<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.StackLayout
   => control._set(FluentAvalonia.UI.Controls.StackLayout.SpacingProperty, binding);
public static T Spacing<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.StackLayout
   => control._set(FluentAvalonia.UI.Controls.StackLayout.SpacingProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Spacing<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.StackLayout
   => control._set(FluentAvalonia.UI.Controls.StackLayout.SpacingProperty, func, onChanged, expression);
public static T Spacing<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.StackLayout
=> control._setEx(FluentAvalonia.UI.Controls.StackLayout.SpacingProperty, ps, () => control.Spacing = value, bindingMode, converter, bindingSource);
public static T Spacing<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.StackLayout
=> control._setEx(FluentAvalonia.UI.Controls.StackLayout.SpacingProperty, ps, () => control.Spacing = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Orientation<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.StackLayout
   => control._set(FluentAvalonia.UI.Controls.StackLayout.OrientationProperty, binding);
public static T Orientation<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.StackLayout
   => control._set(FluentAvalonia.UI.Controls.StackLayout.OrientationProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Orientation<T>(this T control, Func<Avalonia.Layout.Orientation> func, Action<Avalonia.Layout.Orientation>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.StackLayout
   => control._set(FluentAvalonia.UI.Controls.StackLayout.OrientationProperty, func, onChanged, expression);
public static T Orientation<T>(this T control, Avalonia.Layout.Orientation value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.StackLayout
=> control._setEx(FluentAvalonia.UI.Controls.StackLayout.OrientationProperty, ps, () => control.Orientation = value, bindingMode, converter, bindingSource);
public static T Orientation<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Layout.Orientation> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.StackLayout
=> control._setEx(FluentAvalonia.UI.Controls.StackLayout.OrientationProperty, ps, () => control.Orientation = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

