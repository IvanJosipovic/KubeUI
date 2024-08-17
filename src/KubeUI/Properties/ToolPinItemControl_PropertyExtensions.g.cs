#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Dock.Avalonia.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using ToolPinItemControl = Dock.Avalonia.Controls.ToolPinItemControl;

namespace Avalonia.Markup.Declarative;
public static partial class ToolPinItemControlExtensions
{
public static T Orientation<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.ToolPinItemControl
   => control._set(Dock.Avalonia.Controls.ToolPinItemControl.OrientationProperty, binding);
public static T Orientation<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.ToolPinItemControl
   => control._set(Dock.Avalonia.Controls.ToolPinItemControl.OrientationProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Orientation<T>(this T control, Func<Avalonia.Layout.Orientation> func, Action<Avalonia.Layout.Orientation>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.ToolPinItemControl
   => control._set(Dock.Avalonia.Controls.ToolPinItemControl.OrientationProperty, func, onChanged, expression);
public static T Orientation<T>(this T control, Avalonia.Layout.Orientation value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.ToolPinItemControl
=> control._setEx(Dock.Avalonia.Controls.ToolPinItemControl.OrientationProperty, ps, () => control.Orientation = value, bindingMode, converter, bindingSource);
public static T Orientation<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Layout.Orientation> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.ToolPinItemControl
=> control._setEx(Dock.Avalonia.Controls.ToolPinItemControl.OrientationProperty, ps, () => control.Orientation = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

