#nullable enable
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using ToolBar = Ursa.Controls.ToolBar;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class ToolBarExtensions
{
public static T Orientation<T>(this T control, IBinding binding) where T : Ursa.Controls.ToolBar
   => control._set(Ursa.Controls.ToolBar.OrientationProperty, binding);
public static T Orientation<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ToolBar
   => control._set(Ursa.Controls.ToolBar.OrientationProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Orientation<T>(this T control, Func<Avalonia.Layout.Orientation> func, Action<Avalonia.Layout.Orientation>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ToolBar
   => control._set(Ursa.Controls.ToolBar.OrientationProperty, func, onChanged, expression);
public static T Orientation<T>(this T control, Avalonia.Layout.Orientation value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ToolBar
=> control._setEx(Ursa.Controls.ToolBar.OrientationProperty, ps, () => control.Orientation = value, bindingMode, converter, bindingSource);
public static T Orientation<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Layout.Orientation> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ToolBar
=> control._setEx(Ursa.Controls.ToolBar.OrientationProperty, ps, () => control.Orientation = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T PopupPlacement<T>(this T control, IBinding binding) where T : Ursa.Controls.ToolBar
   => control._set(Ursa.Controls.ToolBar.PopupPlacementProperty, binding);
public static T PopupPlacement<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ToolBar
   => control._set(Ursa.Controls.ToolBar.PopupPlacementProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T PopupPlacement<T>(this T control, Func<Avalonia.Controls.PlacementMode> func, Action<Avalonia.Controls.PlacementMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ToolBar
   => control._set(Ursa.Controls.ToolBar.PopupPlacementProperty, func, onChanged, expression);
public static T PopupPlacement<T>(this T control, Avalonia.Controls.PlacementMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ToolBar
=> control._setEx(Ursa.Controls.ToolBar.PopupPlacementProperty, ps, () => control.PopupPlacement = value, bindingMode, converter, bindingSource);
public static T PopupPlacement<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.PlacementMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ToolBar
=> control._setEx(Ursa.Controls.ToolBar.PopupPlacementProperty, ps, () => control.PopupPlacement = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

