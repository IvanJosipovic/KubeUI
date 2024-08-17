#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using MessageBoxControl = Ursa.Controls.MessageBoxControl;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class MessageBoxControlExtensions
{
public static T MessageIcon<T>(this T control, IBinding binding) where T : Ursa.Controls.MessageBoxControl
   => control._set(Ursa.Controls.MessageBoxControl.MessageIconProperty, binding);
public static T MessageIcon<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.MessageBoxControl
   => control._set(Ursa.Controls.MessageBoxControl.MessageIconProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T MessageIcon<T>(this T control, Func<Ursa.Controls.MessageBoxIcon> func, Action<Ursa.Controls.MessageBoxIcon>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.MessageBoxControl
   => control._set(Ursa.Controls.MessageBoxControl.MessageIconProperty, func, onChanged, expression);
public static T MessageIcon<T>(this T control, Ursa.Controls.MessageBoxIcon value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MessageBoxControl
=> control._setEx(Ursa.Controls.MessageBoxControl.MessageIconProperty, ps, () => control.MessageIcon = value, bindingMode, converter, bindingSource);
public static T MessageIcon<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Ursa.Controls.MessageBoxIcon> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MessageBoxControl
=> control._setEx(Ursa.Controls.MessageBoxControl.MessageIconProperty, ps, () => control.MessageIcon = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Buttons<T>(this T control, IBinding binding) where T : Ursa.Controls.MessageBoxControl
   => control._set(Ursa.Controls.MessageBoxControl.ButtonsProperty, binding);
public static T Buttons<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.MessageBoxControl
   => control._set(Ursa.Controls.MessageBoxControl.ButtonsProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Buttons<T>(this T control, Func<Ursa.Controls.MessageBoxButton> func, Action<Ursa.Controls.MessageBoxButton>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.MessageBoxControl
   => control._set(Ursa.Controls.MessageBoxControl.ButtonsProperty, func, onChanged, expression);
public static T Buttons<T>(this T control, Ursa.Controls.MessageBoxButton value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MessageBoxControl
=> control._setEx(Ursa.Controls.MessageBoxControl.ButtonsProperty, ps, () => control.Buttons = value, bindingMode, converter, bindingSource);
public static T Buttons<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Ursa.Controls.MessageBoxButton> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MessageBoxControl
=> control._setEx(Ursa.Controls.MessageBoxControl.ButtonsProperty, ps, () => control.Buttons = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Title<T>(this T control, IBinding binding) where T : Ursa.Controls.MessageBoxControl
   => control._set(Ursa.Controls.MessageBoxControl.TitleProperty, binding);
public static T Title<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.MessageBoxControl
   => control._set(Ursa.Controls.MessageBoxControl.TitleProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Title<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.MessageBoxControl
   => control._set(Ursa.Controls.MessageBoxControl.TitleProperty, func, onChanged, expression);
public static T Title<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MessageBoxControl
=> control._setEx(Ursa.Controls.MessageBoxControl.TitleProperty, ps, () => control.Title = value, bindingMode, converter, bindingSource);
public static T Title<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MessageBoxControl
=> control._setEx(Ursa.Controls.MessageBoxControl.TitleProperty, ps, () => control.Title = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

