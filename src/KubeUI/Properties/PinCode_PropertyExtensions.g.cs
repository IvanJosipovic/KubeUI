#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using PinCode = Ursa.Controls.PinCode;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class PinCodeExtensions
{
public static T CompleteCommand<T>(this T control, IBinding binding) where T : Ursa.Controls.PinCode
   => control._set(Ursa.Controls.PinCode.CompleteCommandProperty, binding);
public static T CompleteCommand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.PinCode
   => control._set(Ursa.Controls.PinCode.CompleteCommandProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CompleteCommand<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.PinCode
   => control._set(Ursa.Controls.PinCode.CompleteCommandProperty, func, onChanged, expression);
public static T CompleteCommand<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.PinCode
=> control._setEx(Ursa.Controls.PinCode.CompleteCommandProperty, ps, () => control.CompleteCommand = value, bindingMode, converter, bindingSource);
public static T CompleteCommand<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.PinCode
=> control._setEx(Ursa.Controls.PinCode.CompleteCommandProperty, ps, () => control.CompleteCommand = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Count<T>(this T control, IBinding binding) where T : Ursa.Controls.PinCode
   => control._set(Ursa.Controls.PinCode.CountProperty, binding);
public static T Count<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.PinCode
   => control._set(Ursa.Controls.PinCode.CountProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Count<T>(this T control, Func<System.Int32> func, Action<System.Int32>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.PinCode
   => control._set(Ursa.Controls.PinCode.CountProperty, func, onChanged, expression);
public static T Count<T>(this T control, System.Int32 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.PinCode
=> control._setEx(Ursa.Controls.PinCode.CountProperty, ps, () => control.Count = value, bindingMode, converter, bindingSource);
public static T Count<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Int32> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.PinCode
=> control._setEx(Ursa.Controls.PinCode.CountProperty, ps, () => control.Count = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T PasswordChar<T>(this T control, IBinding binding) where T : Ursa.Controls.PinCode
   => control._set(Ursa.Controls.PinCode.PasswordCharProperty, binding);
public static T PasswordChar<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.PinCode
   => control._set(Ursa.Controls.PinCode.PasswordCharProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T PasswordChar<T>(this T control, Func<System.Char> func, Action<System.Char>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.PinCode
   => control._set(Ursa.Controls.PinCode.PasswordCharProperty, func, onChanged, expression);
public static T PasswordChar<T>(this T control, System.Char value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.PinCode
=> control._setEx(Ursa.Controls.PinCode.PasswordCharProperty, ps, () => control.PasswordChar = value, bindingMode, converter, bindingSource);
public static T PasswordChar<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Char> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.PinCode
=> control._setEx(Ursa.Controls.PinCode.PasswordCharProperty, ps, () => control.PasswordChar = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Mode<T>(this T control, IBinding binding) where T : Ursa.Controls.PinCode
   => control._set(Ursa.Controls.PinCode.ModeProperty, binding);
public static T Mode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.PinCode
   => control._set(Ursa.Controls.PinCode.ModeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Mode<T>(this T control, Func<Ursa.Controls.PinCodeMode> func, Action<Ursa.Controls.PinCodeMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.PinCode
   => control._set(Ursa.Controls.PinCode.ModeProperty, func, onChanged, expression);
public static T Mode<T>(this T control, Ursa.Controls.PinCodeMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.PinCode
=> control._setEx(Ursa.Controls.PinCode.ModeProperty, ps, () => control.Mode = value, bindingMode, converter, bindingSource);
public static T Mode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Ursa.Controls.PinCodeMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.PinCode
=> control._setEx(Ursa.Controls.PinCode.ModeProperty, ps, () => control.Mode = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

