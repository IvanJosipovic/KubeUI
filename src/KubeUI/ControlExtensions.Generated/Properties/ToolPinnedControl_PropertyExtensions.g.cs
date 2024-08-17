#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Dock.Avalonia.Controls;
using System;
using System.Collections;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using ToolPinnedControl = Dock.Avalonia.Controls.ToolPinnedControl;

namespace Avalonia.Markup.Declarative;
public static partial class ToolPinnedControlExtensions
{
public static T Items<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.ToolPinnedControl
   => control._set(Dock.Avalonia.Controls.ToolPinnedControl.ItemsProperty, binding);
public static T Items<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.ToolPinnedControl
   => control._set(Dock.Avalonia.Controls.ToolPinnedControl.ItemsProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Items<T>(this T control, Func<System.Collections.IEnumerable> func, Action<System.Collections.IEnumerable>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.ToolPinnedControl
   => control._set(Dock.Avalonia.Controls.ToolPinnedControl.ItemsProperty, func, onChanged, expression);
public static T Items<T>(this T control, System.Collections.IEnumerable value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.ToolPinnedControl
=> control._setEx(Dock.Avalonia.Controls.ToolPinnedControl.ItemsProperty, ps, () => control.Items = value, bindingMode, converter, bindingSource);
public static T Items<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.IEnumerable> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.ToolPinnedControl
=> control._setEx(Dock.Avalonia.Controls.ToolPinnedControl.ItemsProperty, ps, () => control.Items = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Orientation<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.ToolPinnedControl
   => control._set(Dock.Avalonia.Controls.ToolPinnedControl.OrientationProperty, binding);
public static T Orientation<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.ToolPinnedControl
   => control._set(Dock.Avalonia.Controls.ToolPinnedControl.OrientationProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Orientation<T>(this T control, Func<Avalonia.Layout.Orientation> func, Action<Avalonia.Layout.Orientation>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.ToolPinnedControl
   => control._set(Dock.Avalonia.Controls.ToolPinnedControl.OrientationProperty, func, onChanged, expression);
public static T Orientation<T>(this T control, Avalonia.Layout.Orientation value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.ToolPinnedControl
=> control._setEx(Dock.Avalonia.Controls.ToolPinnedControl.OrientationProperty, ps, () => control.Orientation = value, bindingMode, converter, bindingSource);
public static T Orientation<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Layout.Orientation> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.ToolPinnedControl
=> control._setEx(Dock.Avalonia.Controls.ToolPinnedControl.OrientationProperty, ps, () => control.Orientation = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

