#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Dock.Avalonia.Controls;
using HostWindow = Dock.Avalonia.Controls.HostWindow;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class HostWindowExtensions
{
public static T IsToolWindow<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.HostWindow
   => control._set(Dock.Avalonia.Controls.HostWindow.IsToolWindowProperty, binding);
public static T IsToolWindow<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.HostWindow
   => control._set(Dock.Avalonia.Controls.HostWindow.IsToolWindowProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsToolWindow<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.HostWindow
   => control._set(Dock.Avalonia.Controls.HostWindow.IsToolWindowProperty, func, onChanged, expression);
public static T IsToolWindow<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.HostWindow
=> control._setEx(Dock.Avalonia.Controls.HostWindow.IsToolWindowProperty, ps, () => control.IsToolWindow = value, bindingMode, converter, bindingSource);
public static T IsToolWindow<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.HostWindow
=> control._setEx(Dock.Avalonia.Controls.HostWindow.IsToolWindowProperty, ps, () => control.IsToolWindow = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ToolChromeControlsWholeWindow<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.HostWindow
   => control._set(Dock.Avalonia.Controls.HostWindow.ToolChromeControlsWholeWindowProperty, binding);
public static T ToolChromeControlsWholeWindow<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.HostWindow
   => control._set(Dock.Avalonia.Controls.HostWindow.ToolChromeControlsWholeWindowProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ToolChromeControlsWholeWindow<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.HostWindow
   => control._set(Dock.Avalonia.Controls.HostWindow.ToolChromeControlsWholeWindowProperty, func, onChanged, expression);
public static T ToolChromeControlsWholeWindow<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.HostWindow
=> control._setEx(Dock.Avalonia.Controls.HostWindow.ToolChromeControlsWholeWindowProperty, ps, () => control.ToolChromeControlsWholeWindow = value, bindingMode, converter, bindingSource);
public static T ToolChromeControlsWholeWindow<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.HostWindow
=> control._setEx(Dock.Avalonia.Controls.HostWindow.ToolChromeControlsWholeWindowProperty, ps, () => control.ToolChromeControlsWholeWindow = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

