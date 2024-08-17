#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Dock.Avalonia.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using ToolChromeControl = Dock.Avalonia.Controls.ToolChromeControl;

namespace Avalonia.Markup.Declarative;
public static partial class ToolChromeControlExtensions
{
public static T IsActive<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.ToolChromeControl
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.IsActiveProperty, binding);
public static T IsActive<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.ToolChromeControl
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.IsActiveProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsActive<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.ToolChromeControl
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.IsActiveProperty, func, onChanged, expression);
public static T IsActive<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.ToolChromeControl
=> control._setEx(Dock.Avalonia.Controls.ToolChromeControl.IsActiveProperty, ps, () => control.IsActive = value, bindingMode, converter, bindingSource);
public static T IsActive<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.ToolChromeControl
=> control._setEx(Dock.Avalonia.Controls.ToolChromeControl.IsActiveProperty, ps, () => control.IsActive = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsPinned<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.ToolChromeControl
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.IsPinnedProperty, binding);
public static T IsPinned<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.ToolChromeControl
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.IsPinnedProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsPinned<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.ToolChromeControl
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.IsPinnedProperty, func, onChanged, expression);
public static T IsPinned<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.ToolChromeControl
=> control._setEx(Dock.Avalonia.Controls.ToolChromeControl.IsPinnedProperty, ps, () => control.IsPinned = value, bindingMode, converter, bindingSource);
public static T IsPinned<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.ToolChromeControl
=> control._setEx(Dock.Avalonia.Controls.ToolChromeControl.IsPinnedProperty, ps, () => control.IsPinned = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsFloating<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.ToolChromeControl
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.IsFloatingProperty, binding);
public static T IsFloating<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.ToolChromeControl
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.IsFloatingProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsFloating<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.ToolChromeControl
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.IsFloatingProperty, func, onChanged, expression);
public static T IsFloating<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.ToolChromeControl
=> control._setEx(Dock.Avalonia.Controls.ToolChromeControl.IsFloatingProperty, ps, () => control.IsFloating = value, bindingMode, converter, bindingSource);
public static T IsFloating<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.ToolChromeControl
=> control._setEx(Dock.Avalonia.Controls.ToolChromeControl.IsFloatingProperty, ps, () => control.IsFloating = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsMaximized<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.ToolChromeControl
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.IsMaximizedProperty, binding);
public static T IsMaximized<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.ToolChromeControl
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.IsMaximizedProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsMaximized<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.ToolChromeControl
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.IsMaximizedProperty, func, onChanged, expression);
public static T IsMaximized<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.ToolChromeControl
=> control._setEx(Dock.Avalonia.Controls.ToolChromeControl.IsMaximizedProperty, ps, () => control.IsMaximized = value, bindingMode, converter, bindingSource);
public static T IsMaximized<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.ToolChromeControl
=> control._setEx(Dock.Avalonia.Controls.ToolChromeControl.IsMaximizedProperty, ps, () => control.IsMaximized = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

