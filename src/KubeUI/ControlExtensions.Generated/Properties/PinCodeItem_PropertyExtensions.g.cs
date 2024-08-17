#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using PinCodeItem = Ursa.Controls.PinCodeItem;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class PinCodeItemExtensions
{
public static T Text<T>(this T control, IBinding binding) where T : Ursa.Controls.PinCodeItem
   => control._set(Ursa.Controls.PinCodeItem.TextProperty, binding);
public static T Text<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.PinCodeItem
   => control._set(Ursa.Controls.PinCodeItem.TextProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Text<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.PinCodeItem
   => control._set(Ursa.Controls.PinCodeItem.TextProperty, func, onChanged, expression);
public static T Text<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.PinCodeItem
=> control._setEx(Ursa.Controls.PinCodeItem.TextProperty, ps, () => control.Text = value, bindingMode, converter, bindingSource);
public static T Text<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.PinCodeItem
=> control._setEx(Ursa.Controls.PinCodeItem.TextProperty, ps, () => control.Text = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T PasswordChar<T>(this T control, IBinding binding) where T : Ursa.Controls.PinCodeItem
   => control._set(Ursa.Controls.PinCodeItem.PasswordCharProperty, binding);
public static T PasswordChar<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.PinCodeItem
   => control._set(Ursa.Controls.PinCodeItem.PasswordCharProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T PasswordChar<T>(this T control, Func<System.Char> func, Action<System.Char>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.PinCodeItem
   => control._set(Ursa.Controls.PinCodeItem.PasswordCharProperty, func, onChanged, expression);
public static T PasswordChar<T>(this T control, System.Char value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.PinCodeItem
=> control._setEx(Ursa.Controls.PinCodeItem.PasswordCharProperty, ps, () => control.PasswordChar = value, bindingMode, converter, bindingSource);
public static T PasswordChar<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Char> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.PinCodeItem
=> control._setEx(Ursa.Controls.PinCodeItem.PasswordCharProperty, ps, () => control.PasswordChar = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

