#nullable enable
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls.Primitives;
using InfoBarPanel = FluentAvalonia.UI.Controls.Primitives.InfoBarPanel;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class InfoBarPanelExtensions
{
public static T HorizontalOrientationPadding<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
   => control._set(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.HorizontalOrientationPaddingProperty, binding);
public static T HorizontalOrientationPadding<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
   => control._set(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.HorizontalOrientationPaddingProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T HorizontalOrientationPadding<T>(this T control, Func<Avalonia.Thickness> func, Action<Avalonia.Thickness>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
   => control._set(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.HorizontalOrientationPaddingProperty, func, onChanged, expression);
public static T HorizontalOrientationPadding<T>(this T control, Avalonia.Thickness value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
=> control._setEx(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.HorizontalOrientationPaddingProperty, ps, () => control.HorizontalOrientationPadding = value, bindingMode, converter, bindingSource);
public static T HorizontalOrientationPadding<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Thickness> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
=> control._setEx(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.HorizontalOrientationPaddingProperty, ps, () => control.HorizontalOrientationPadding = converter.TryConvert(value), bindingMode, converter, bindingSource);

public static T HorizontalOrientationPadding<T>(this T control, Double uniformLength = default) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
   => control._set(() => control.HorizontalOrientationPadding = new Avalonia.Thickness(uniformLength));
public static T HorizontalOrientationPadding<T>(this T control, Double horizontal = default, Double vertical = default) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
   => control._set(() => control.HorizontalOrientationPadding = new Avalonia.Thickness(horizontal, vertical));
public static T HorizontalOrientationPadding<T>(this T control, Double left = default, Double top = default, Double right = default, Double bottom = default) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
   => control._set(() => control.HorizontalOrientationPadding = new Avalonia.Thickness(left, top, right, bottom));
public static T VerticalOrientationPadding<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
   => control._set(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.VerticalOrientationPaddingProperty, binding);
public static T VerticalOrientationPadding<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
   => control._set(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.VerticalOrientationPaddingProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T VerticalOrientationPadding<T>(this T control, Func<Avalonia.Thickness> func, Action<Avalonia.Thickness>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
   => control._set(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.VerticalOrientationPaddingProperty, func, onChanged, expression);
public static T VerticalOrientationPadding<T>(this T control, Avalonia.Thickness value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
=> control._setEx(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.VerticalOrientationPaddingProperty, ps, () => control.VerticalOrientationPadding = value, bindingMode, converter, bindingSource);
public static T VerticalOrientationPadding<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Thickness> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
=> control._setEx(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.VerticalOrientationPaddingProperty, ps, () => control.VerticalOrientationPadding = converter.TryConvert(value), bindingMode, converter, bindingSource);

public static T VerticalOrientationPadding<T>(this T control, Double uniformLength = default) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
   => control._set(() => control.VerticalOrientationPadding = new Avalonia.Thickness(uniformLength));
public static T VerticalOrientationPadding<T>(this T control, Double horizontal = default, Double vertical = default) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
   => control._set(() => control.VerticalOrientationPadding = new Avalonia.Thickness(horizontal, vertical));
public static T VerticalOrientationPadding<T>(this T control, Double left = default, Double top = default, Double right = default, Double bottom = default) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
   => control._set(() => control.VerticalOrientationPadding = new Avalonia.Thickness(left, top, right, bottom));
}

