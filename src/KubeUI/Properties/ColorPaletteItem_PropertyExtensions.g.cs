#nullable enable
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using ColorPaletteItem = FluentAvalonia.UI.Controls.ColorPaletteItem;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class ColorPaletteItemExtensions
{
public static T Color<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.ColorProperty, binding);
public static T Color<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.ColorProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Color<T>(this T control, Func<Avalonia.Media.Color> func, Action<Avalonia.Media.Color>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.ColorProperty, func, onChanged, expression);
public static T Color<T>(this T control, Avalonia.Media.Color value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
=> control._setEx(FluentAvalonia.UI.Controls.ColorPaletteItem.ColorProperty, ps, () => control.Color = value, bindingMode, converter, bindingSource);
public static T Color<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.Color> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
=> control._setEx(FluentAvalonia.UI.Controls.ColorPaletteItem.ColorProperty, ps, () => control.Color = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T BorderBrush<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushProperty, binding);
public static T BorderBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T BorderBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushProperty, func, onChanged, expression);
public static T BorderBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
=> control._setEx(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushProperty, ps, () => control.BorderBrush = value, bindingMode, converter, bindingSource);
public static T BorderBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
=> control._setEx(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushProperty, ps, () => control.BorderBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T BorderBrushPointerOver<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushPointerOverProperty, binding);
public static T BorderBrushPointerOver<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushPointerOverProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T BorderBrushPointerOver<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushPointerOverProperty, func, onChanged, expression);
public static T BorderBrushPointerOver<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
=> control._setEx(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushPointerOverProperty, ps, () => control.BorderBrushPointerOver = value, bindingMode, converter, bindingSource);
public static T BorderBrushPointerOver<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
=> control._setEx(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushPointerOverProperty, ps, () => control.BorderBrushPointerOver = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T BorderBrushPressed<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushPressedProperty, binding);
public static T BorderBrushPressed<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushPressedProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T BorderBrushPressed<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushPressedProperty, func, onChanged, expression);
public static T BorderBrushPressed<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
=> control._setEx(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushPressedProperty, ps, () => control.BorderBrushPressed = value, bindingMode, converter, bindingSource);
public static T BorderBrushPressed<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
=> control._setEx(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushPressedProperty, ps, () => control.BorderBrushPressed = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T BorderThickness<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessProperty, binding);
public static T BorderThickness<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T BorderThickness<T>(this T control, Func<Avalonia.Thickness> func, Action<Avalonia.Thickness>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessProperty, func, onChanged, expression);
public static T BorderThickness<T>(this T control, Avalonia.Thickness value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
=> control._setEx(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessProperty, ps, () => control.BorderThickness = value, bindingMode, converter, bindingSource);
public static T BorderThickness<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Thickness> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
=> control._setEx(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessProperty, ps, () => control.BorderThickness = converter.TryConvert(value), bindingMode, converter, bindingSource);

public static T BorderThickness<T>(this T control, Double uniformLength = default) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => control._set(() => control.BorderThickness = new Avalonia.Thickness(uniformLength));
public static T BorderThickness<T>(this T control, Double horizontal = default, Double vertical = default) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => control._set(() => control.BorderThickness = new Avalonia.Thickness(horizontal, vertical));
public static T BorderThickness<T>(this T control, Double left = default, Double top = default, Double right = default, Double bottom = default) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => control._set(() => control.BorderThickness = new Avalonia.Thickness(left, top, right, bottom));
public static T BorderThicknessPointerOver<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPointerOverProperty, binding);
public static T BorderThicknessPointerOver<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPointerOverProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T BorderThicknessPointerOver<T>(this T control, Func<Avalonia.Thickness> func, Action<Avalonia.Thickness>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPointerOverProperty, func, onChanged, expression);
public static T BorderThicknessPointerOver<T>(this T control, Avalonia.Thickness value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
=> control._setEx(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPointerOverProperty, ps, () => control.BorderThicknessPointerOver = value, bindingMode, converter, bindingSource);
public static T BorderThicknessPointerOver<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Thickness> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
=> control._setEx(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPointerOverProperty, ps, () => control.BorderThicknessPointerOver = converter.TryConvert(value), bindingMode, converter, bindingSource);

public static T BorderThicknessPointerOver<T>(this T control, Double uniformLength = default) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => control._set(() => control.BorderThicknessPointerOver = new Avalonia.Thickness(uniformLength));
public static T BorderThicknessPointerOver<T>(this T control, Double horizontal = default, Double vertical = default) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => control._set(() => control.BorderThicknessPointerOver = new Avalonia.Thickness(horizontal, vertical));
public static T BorderThicknessPointerOver<T>(this T control, Double left = default, Double top = default, Double right = default, Double bottom = default) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => control._set(() => control.BorderThicknessPointerOver = new Avalonia.Thickness(left, top, right, bottom));
public static T BorderThicknessPressed<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPressedProperty, binding);
public static T BorderThicknessPressed<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPressedProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T BorderThicknessPressed<T>(this T control, Func<Avalonia.Thickness> func, Action<Avalonia.Thickness>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPressedProperty, func, onChanged, expression);
public static T BorderThicknessPressed<T>(this T control, Avalonia.Thickness value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
=> control._setEx(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPressedProperty, ps, () => control.BorderThicknessPressed = value, bindingMode, converter, bindingSource);
public static T BorderThicknessPressed<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Thickness> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
=> control._setEx(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPressedProperty, ps, () => control.BorderThicknessPressed = converter.TryConvert(value), bindingMode, converter, bindingSource);

public static T BorderThicknessPressed<T>(this T control, Double uniformLength = default) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => control._set(() => control.BorderThicknessPressed = new Avalonia.Thickness(uniformLength));
public static T BorderThicknessPressed<T>(this T control, Double horizontal = default, Double vertical = default) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => control._set(() => control.BorderThicknessPressed = new Avalonia.Thickness(horizontal, vertical));
public static T BorderThicknessPressed<T>(this T control, Double left = default, Double top = default, Double right = default, Double bottom = default) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => control._set(() => control.BorderThicknessPressed = new Avalonia.Thickness(left, top, right, bottom));
public static T CornerRadius<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.CornerRadiusProperty, binding);
public static T CornerRadius<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.CornerRadiusProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CornerRadius<T>(this T control, Func<Avalonia.CornerRadius> func, Action<Avalonia.CornerRadius>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.CornerRadiusProperty, func, onChanged, expression);
public static T CornerRadius<T>(this T control, Avalonia.CornerRadius value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
=> control._setEx(FluentAvalonia.UI.Controls.ColorPaletteItem.CornerRadiusProperty, ps, () => control.CornerRadius = value, bindingMode, converter, bindingSource);
public static T CornerRadius<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.CornerRadius> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
=> control._setEx(FluentAvalonia.UI.Controls.ColorPaletteItem.CornerRadiusProperty, ps, () => control.CornerRadius = converter.TryConvert(value), bindingMode, converter, bindingSource);

public static T CornerRadius<T>(this T control, Double uniformRadius = default) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => control._set(() => control.CornerRadius = new Avalonia.CornerRadius(uniformRadius));
public static T CornerRadius<T>(this T control, Double top = default, Double bottom = default) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => control._set(() => control.CornerRadius = new Avalonia.CornerRadius(top, bottom));
public static T CornerRadius<T>(this T control, Double topLeft = default, Double topRight = default, Double bottomRight = default, Double bottomLeft = default) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => control._set(() => control.CornerRadius = new Avalonia.CornerRadius(topLeft, topRight, bottomRight, bottomLeft));
}

