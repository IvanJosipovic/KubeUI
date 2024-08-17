#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using IPv4Box = Ursa.Controls.IPv4Box;
using System;
using System.Linq.Expressions;
using System.Net;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class IPv4BoxExtensions
{
public static T IPAddress<T>(this T control, IBinding binding) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.IPAddressProperty, binding);
public static T IPAddress<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.IPAddressProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IPAddress<T>(this T control, Func<System.Net.IPAddress> func, Action<System.Net.IPAddress>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.IPAddressProperty, func, onChanged, expression);
public static T IPAddress<T>(this T control, System.Net.IPAddress value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.IPv4Box
=> control._setEx(Ursa.Controls.IPv4Box.IPAddressProperty, ps, () => control.IPAddress = value, bindingMode, converter, bindingSource);
public static T IPAddress<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Net.IPAddress> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.IPv4Box
=> control._setEx(Ursa.Controls.IPv4Box.IPAddressProperty, ps, () => control.IPAddress = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T TextAlignment<T>(this T control, IBinding binding) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.TextAlignmentProperty, binding);
public static T TextAlignment<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.TextAlignmentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T TextAlignment<T>(this T control, Func<Avalonia.Media.TextAlignment> func, Action<Avalonia.Media.TextAlignment>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.TextAlignmentProperty, func, onChanged, expression);
public static T TextAlignment<T>(this T control, Avalonia.Media.TextAlignment value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.IPv4Box
=> control._setEx(Ursa.Controls.IPv4Box.TextAlignmentProperty, ps, () => control.TextAlignment = value, bindingMode, converter, bindingSource);
public static T TextAlignment<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.TextAlignment> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.IPv4Box
=> control._setEx(Ursa.Controls.IPv4Box.TextAlignmentProperty, ps, () => control.TextAlignment = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SelectionBrush<T>(this T control, IBinding binding) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.SelectionBrushProperty, binding);
public static T SelectionBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.SelectionBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SelectionBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.SelectionBrushProperty, func, onChanged, expression);
public static T SelectionBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.IPv4Box
=> control._setEx(Ursa.Controls.IPv4Box.SelectionBrushProperty, ps, () => control.SelectionBrush = value, bindingMode, converter, bindingSource);
public static T SelectionBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.IPv4Box
=> control._setEx(Ursa.Controls.IPv4Box.SelectionBrushProperty, ps, () => control.SelectionBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SelectionForegroundBrush<T>(this T control, IBinding binding) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.SelectionForegroundBrushProperty, binding);
public static T SelectionForegroundBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.SelectionForegroundBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SelectionForegroundBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.SelectionForegroundBrushProperty, func, onChanged, expression);
public static T SelectionForegroundBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.IPv4Box
=> control._setEx(Ursa.Controls.IPv4Box.SelectionForegroundBrushProperty, ps, () => control.SelectionForegroundBrush = value, bindingMode, converter, bindingSource);
public static T SelectionForegroundBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.IPv4Box
=> control._setEx(Ursa.Controls.IPv4Box.SelectionForegroundBrushProperty, ps, () => control.SelectionForegroundBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T CaretBrush<T>(this T control, IBinding binding) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.CaretBrushProperty, binding);
public static T CaretBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.CaretBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CaretBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.CaretBrushProperty, func, onChanged, expression);
public static T CaretBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.IPv4Box
=> control._setEx(Ursa.Controls.IPv4Box.CaretBrushProperty, ps, () => control.CaretBrush = value, bindingMode, converter, bindingSource);
public static T CaretBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.IPv4Box
=> control._setEx(Ursa.Controls.IPv4Box.CaretBrushProperty, ps, () => control.CaretBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ShowLeadingZero<T>(this T control, IBinding binding) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.ShowLeadingZeroProperty, binding);
public static T ShowLeadingZero<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.ShowLeadingZeroProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ShowLeadingZero<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.ShowLeadingZeroProperty, func, onChanged, expression);
public static T ShowLeadingZero<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.IPv4Box
=> control._setEx(Ursa.Controls.IPv4Box.ShowLeadingZeroProperty, ps, () => control.ShowLeadingZero = value, bindingMode, converter, bindingSource);
public static T ShowLeadingZero<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.IPv4Box
=> control._setEx(Ursa.Controls.IPv4Box.ShowLeadingZeroProperty, ps, () => control.ShowLeadingZero = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T InputMode<T>(this T control, IBinding binding) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.InputModeProperty, binding);
public static T InputMode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.InputModeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T InputMode<T>(this T control, Func<Ursa.Controls.IPv4BoxInputMode> func, Action<Ursa.Controls.IPv4BoxInputMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.InputModeProperty, func, onChanged, expression);
public static T InputMode<T>(this T control, Ursa.Controls.IPv4BoxInputMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.IPv4Box
=> control._setEx(Ursa.Controls.IPv4Box.InputModeProperty, ps, () => control.InputMode = value, bindingMode, converter, bindingSource);
public static T InputMode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Ursa.Controls.IPv4BoxInputMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.IPv4Box
=> control._setEx(Ursa.Controls.IPv4Box.InputModeProperty, ps, () => control.InputMode = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

