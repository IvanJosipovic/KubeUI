#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using NumPad = Ursa.Controls.NumPad;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class NumPadExtensions
{
public static T Target<T>(this T control, IBinding binding) where T : Ursa.Controls.NumPad
   => control._set(Ursa.Controls.NumPad.TargetProperty, binding);
public static T Target<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumPad
   => control._set(Ursa.Controls.NumPad.TargetProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Target<T>(this T control, Func<Avalonia.Input.InputElement> func, Action<Avalonia.Input.InputElement>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumPad
   => control._set(Ursa.Controls.NumPad.TargetProperty, func, onChanged, expression);
public static T Target<T>(this T control, Avalonia.Input.InputElement value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumPad
=> control._setEx(Ursa.Controls.NumPad.TargetProperty, ps, () => control.Target = value, bindingMode, converter, bindingSource);
public static T Target<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Input.InputElement> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumPad
=> control._setEx(Ursa.Controls.NumPad.TargetProperty, ps, () => control.Target = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T NumMode<T>(this T control, IBinding binding) where T : Ursa.Controls.NumPad
   => control._set(Ursa.Controls.NumPad.NumModeProperty, binding);
public static T NumMode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumPad
   => control._set(Ursa.Controls.NumPad.NumModeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T NumMode<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumPad
   => control._set(Ursa.Controls.NumPad.NumModeProperty, func, onChanged, expression);
public static T NumMode<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumPad
=> control._setEx(Ursa.Controls.NumPad.NumModeProperty, ps, () => control.NumMode = value, bindingMode, converter, bindingSource);
public static T NumMode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumPad
=> control._setEx(Ursa.Controls.NumPad.NumModeProperty, ps, () => control.NumMode = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

