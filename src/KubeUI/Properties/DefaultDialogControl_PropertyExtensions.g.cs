#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using DefaultDialogControl = Ursa.Controls.DefaultDialogControl;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class DefaultDialogControlExtensions
{
public static T Title<T>(this T control, IBinding binding) where T : Ursa.Controls.DefaultDialogControl
   => control._set(Ursa.Controls.DefaultDialogControl.TitleProperty, binding);
public static T Title<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DefaultDialogControl
   => control._set(Ursa.Controls.DefaultDialogControl.TitleProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Title<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DefaultDialogControl
   => control._set(Ursa.Controls.DefaultDialogControl.TitleProperty, func, onChanged, expression);
public static T Title<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DefaultDialogControl
=> control._setEx(Ursa.Controls.DefaultDialogControl.TitleProperty, ps, () => control.Title = value, bindingMode, converter, bindingSource);
public static T Title<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DefaultDialogControl
=> control._setEx(Ursa.Controls.DefaultDialogControl.TitleProperty, ps, () => control.Title = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Buttons<T>(this T control, IBinding binding) where T : Ursa.Controls.DefaultDialogControl
   => control._set(Ursa.Controls.DefaultDialogControl.ButtonsProperty, binding);
public static T Buttons<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DefaultDialogControl
   => control._set(Ursa.Controls.DefaultDialogControl.ButtonsProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Buttons<T>(this T control, Func<Ursa.Controls.DialogButton> func, Action<Ursa.Controls.DialogButton>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DefaultDialogControl
   => control._set(Ursa.Controls.DefaultDialogControl.ButtonsProperty, func, onChanged, expression);
public static T Buttons<T>(this T control, Ursa.Controls.DialogButton value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DefaultDialogControl
=> control._setEx(Ursa.Controls.DefaultDialogControl.ButtonsProperty, ps, () => control.Buttons = value, bindingMode, converter, bindingSource);
public static T Buttons<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Ursa.Controls.DialogButton> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DefaultDialogControl
=> control._setEx(Ursa.Controls.DefaultDialogControl.ButtonsProperty, ps, () => control.Buttons = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Mode<T>(this T control, IBinding binding) where T : Ursa.Controls.DefaultDialogControl
   => control._set(Ursa.Controls.DefaultDialogControl.ModeProperty, binding);
public static T Mode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DefaultDialogControl
   => control._set(Ursa.Controls.DefaultDialogControl.ModeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Mode<T>(this T control, Func<Ursa.Controls.DialogMode> func, Action<Ursa.Controls.DialogMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DefaultDialogControl
   => control._set(Ursa.Controls.DefaultDialogControl.ModeProperty, func, onChanged, expression);
public static T Mode<T>(this T control, Ursa.Controls.DialogMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DefaultDialogControl
=> control._setEx(Ursa.Controls.DefaultDialogControl.ModeProperty, ps, () => control.Mode = value, bindingMode, converter, bindingSource);
public static T Mode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Ursa.Controls.DialogMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DefaultDialogControl
=> control._setEx(Ursa.Controls.DefaultDialogControl.ModeProperty, ps, () => control.Mode = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

