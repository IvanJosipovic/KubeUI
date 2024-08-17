#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using NumPadButton = Ursa.Controls.NumPadButton;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class NumPadButtonExtensions
{
public static T NumKey<T>(this T control, IBinding binding) where T : Ursa.Controls.NumPadButton
   => control._set(Ursa.Controls.NumPadButton.NumKeyProperty, binding);
public static T NumKey<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumPadButton
   => control._set(Ursa.Controls.NumPadButton.NumKeyProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T NumKey<T>(this T control, Func<System.Nullable<Avalonia.Input.Key>> func, Action<System.Nullable<Avalonia.Input.Key>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumPadButton
   => control._set(Ursa.Controls.NumPadButton.NumKeyProperty, func, onChanged, expression);
public static T NumKey<T>(this T control, System.Nullable<Avalonia.Input.Key> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumPadButton
=> control._setEx(Ursa.Controls.NumPadButton.NumKeyProperty, ps, () => control.NumKey = value, bindingMode, converter, bindingSource);
public static T NumKey<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Nullable<Avalonia.Input.Key>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumPadButton
=> control._setEx(Ursa.Controls.NumPadButton.NumKeyProperty, ps, () => control.NumKey = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T FunctionKey<T>(this T control, IBinding binding) where T : Ursa.Controls.NumPadButton
   => control._set(Ursa.Controls.NumPadButton.FunctionKeyProperty, binding);
public static T FunctionKey<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumPadButton
   => control._set(Ursa.Controls.NumPadButton.FunctionKeyProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T FunctionKey<T>(this T control, Func<System.Nullable<Avalonia.Input.Key>> func, Action<System.Nullable<Avalonia.Input.Key>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumPadButton
   => control._set(Ursa.Controls.NumPadButton.FunctionKeyProperty, func, onChanged, expression);
public static T FunctionKey<T>(this T control, System.Nullable<Avalonia.Input.Key> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumPadButton
=> control._setEx(Ursa.Controls.NumPadButton.FunctionKeyProperty, ps, () => control.FunctionKey = value, bindingMode, converter, bindingSource);
public static T FunctionKey<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Nullable<Avalonia.Input.Key>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumPadButton
=> control._setEx(Ursa.Controls.NumPadButton.FunctionKeyProperty, ps, () => control.FunctionKey = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T NumMode<T>(this T control, IBinding binding) where T : Ursa.Controls.NumPadButton
   => control._set(Ursa.Controls.NumPadButton.NumModeProperty, binding);
public static T NumMode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumPadButton
   => control._set(Ursa.Controls.NumPadButton.NumModeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T NumMode<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumPadButton
   => control._set(Ursa.Controls.NumPadButton.NumModeProperty, func, onChanged, expression);
public static T NumMode<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumPadButton
=> control._setEx(Ursa.Controls.NumPadButton.NumModeProperty, ps, () => control.NumMode = value, bindingMode, converter, bindingSource);
public static T NumMode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumPadButton
=> control._setEx(Ursa.Controls.NumPadButton.NumModeProperty, ps, () => control.NumMode = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T NumContent<T>(this T control, IBinding binding) where T : Ursa.Controls.NumPadButton
   => control._set(Ursa.Controls.NumPadButton.NumContentProperty, binding);
public static T NumContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumPadButton
   => control._set(Ursa.Controls.NumPadButton.NumContentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T NumContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumPadButton
   => control._set(Ursa.Controls.NumPadButton.NumContentProperty, func, onChanged, expression);
public static T NumContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumPadButton
=> control._setEx(Ursa.Controls.NumPadButton.NumContentProperty, ps, () => control.NumContent = value, bindingMode, converter, bindingSource);
public static T NumContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumPadButton
=> control._setEx(Ursa.Controls.NumPadButton.NumContentProperty, ps, () => control.NumContent = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T FunctionContent<T>(this T control, IBinding binding) where T : Ursa.Controls.NumPadButton
   => control._set(Ursa.Controls.NumPadButton.FunctionContentProperty, binding);
public static T FunctionContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumPadButton
   => control._set(Ursa.Controls.NumPadButton.FunctionContentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T FunctionContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumPadButton
   => control._set(Ursa.Controls.NumPadButton.FunctionContentProperty, func, onChanged, expression);
public static T FunctionContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumPadButton
=> control._setEx(Ursa.Controls.NumPadButton.FunctionContentProperty, ps, () => control.FunctionContent = value, bindingMode, converter, bindingSource);
public static T FunctionContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumPadButton
=> control._setEx(Ursa.Controls.NumPadButton.FunctionContentProperty, ps, () => control.FunctionContent = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

