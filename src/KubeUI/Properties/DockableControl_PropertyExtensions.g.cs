#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using DockableControl = Dock.Avalonia.Controls.DockableControl;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class DockableControlExtensions
{
public static T TrackingMode<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.DockableControl
   => control._set(Dock.Avalonia.Controls.DockableControl.TrackingModeProperty, binding);
public static T TrackingMode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.DockableControl
   => control._set(Dock.Avalonia.Controls.DockableControl.TrackingModeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T TrackingMode<T>(this T control, Func<Dock.Model.Core.TrackingMode> func, Action<Dock.Model.Core.TrackingMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.DockableControl
   => control._set(Dock.Avalonia.Controls.DockableControl.TrackingModeProperty, func, onChanged, expression);
public static T TrackingMode<T>(this T control, Dock.Model.Core.TrackingMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DockableControl
=> control._setEx(Dock.Avalonia.Controls.DockableControl.TrackingModeProperty, ps, () => control.TrackingMode = value, bindingMode, converter, bindingSource);
public static T TrackingMode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Dock.Model.Core.TrackingMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DockableControl
=> control._setEx(Dock.Avalonia.Controls.DockableControl.TrackingModeProperty, ps, () => control.TrackingMode = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

