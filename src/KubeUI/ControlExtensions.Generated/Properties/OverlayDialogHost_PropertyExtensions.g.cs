#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using OverlayDialogHost = Ursa.Controls.OverlayDialogHost;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class OverlayDialogHostExtensions
{
public static T IsInModalStatus<T>(this T control, IBinding binding) where T : Ursa.Controls.OverlayDialogHost
   => control._set(Ursa.Controls.OverlayDialogHost.IsInModalStatusProperty, binding);
public static T IsInModalStatus<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.OverlayDialogHost
   => control._set(Ursa.Controls.OverlayDialogHost.IsInModalStatusProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsInModalStatus<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.OverlayDialogHost
   => control._set(Ursa.Controls.OverlayDialogHost.IsInModalStatusProperty, func, onChanged, expression);
public static T IsInModalStatus<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.OverlayDialogHost
=> control._setEx(Ursa.Controls.OverlayDialogHost.IsInModalStatusProperty, ps, () => control.IsInModalStatus = value, bindingMode, converter, bindingSource);
public static T IsInModalStatus<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.OverlayDialogHost
=> control._setEx(Ursa.Controls.OverlayDialogHost.IsInModalStatusProperty, ps, () => control.IsInModalStatus = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsModalStatusReporter<T>(this T control, IBinding binding) where T : Ursa.Controls.OverlayDialogHost
   => control._set(Ursa.Controls.OverlayDialogHost.IsModalStatusReporterProperty, binding);
public static T IsModalStatusReporter<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.OverlayDialogHost
   => control._set(Ursa.Controls.OverlayDialogHost.IsModalStatusReporterProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsModalStatusReporter<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.OverlayDialogHost
   => control._set(Ursa.Controls.OverlayDialogHost.IsModalStatusReporterProperty, func, onChanged, expression);
public static T IsModalStatusReporter<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.OverlayDialogHost
=> control._setEx(Ursa.Controls.OverlayDialogHost.IsModalStatusReporterProperty, ps, () => control.IsModalStatusReporter = value, bindingMode, converter, bindingSource);
public static T IsModalStatusReporter<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.OverlayDialogHost
=> control._setEx(Ursa.Controls.OverlayDialogHost.IsModalStatusReporterProperty, ps, () => control.IsModalStatusReporter = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T OverlayMaskBrush<T>(this T control, IBinding binding) where T : Ursa.Controls.OverlayDialogHost
   => control._set(Ursa.Controls.OverlayDialogHost.OverlayMaskBrushProperty, binding);
public static T OverlayMaskBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.OverlayDialogHost
   => control._set(Ursa.Controls.OverlayDialogHost.OverlayMaskBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T OverlayMaskBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.OverlayDialogHost
   => control._set(Ursa.Controls.OverlayDialogHost.OverlayMaskBrushProperty, func, onChanged, expression);
public static T OverlayMaskBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.OverlayDialogHost
=> control._setEx(Ursa.Controls.OverlayDialogHost.OverlayMaskBrushProperty, ps, () => control.OverlayMaskBrush = value, bindingMode, converter, bindingSource);
public static T OverlayMaskBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.OverlayDialogHost
=> control._setEx(Ursa.Controls.OverlayDialogHost.OverlayMaskBrushProperty, ps, () => control.OverlayMaskBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

