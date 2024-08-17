#nullable enable
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using ControlClassesInput = Ursa.Controls.ControlClassesInput;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class ControlClassesInputExtensions
{
public static T Target<T>(this T control, IBinding binding) where T : Ursa.Controls.ControlClassesInput
   => control._set(Ursa.Controls.ControlClassesInput.TargetProperty, binding);
public static T Target<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ControlClassesInput
   => control._set(Ursa.Controls.ControlClassesInput.TargetProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Target<T>(this T control, Func<Avalonia.Controls.Control> func, Action<Avalonia.Controls.Control>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ControlClassesInput
   => control._set(Ursa.Controls.ControlClassesInput.TargetProperty, func, onChanged, expression);
public static T Target<T>(this T control, Avalonia.Controls.Control value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ControlClassesInput
=> control._setEx(Ursa.Controls.ControlClassesInput.TargetProperty, ps, () => control.Target = value, bindingMode, converter, bindingSource);
public static T Target<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Control> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ControlClassesInput
=> control._setEx(Ursa.Controls.ControlClassesInput.TargetProperty, ps, () => control.Target = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Separator<T>(this T control, IBinding binding) where T : Ursa.Controls.ControlClassesInput
   => control._set(Ursa.Controls.ControlClassesInput.SeparatorProperty, binding);
public static T Separator<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ControlClassesInput
   => control._set(Ursa.Controls.ControlClassesInput.SeparatorProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Separator<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ControlClassesInput
   => control._set(Ursa.Controls.ControlClassesInput.SeparatorProperty, func, onChanged, expression);
public static T Separator<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ControlClassesInput
=> control._setEx(Ursa.Controls.ControlClassesInput.SeparatorProperty, ps, () => control.Separator = value, bindingMode, converter, bindingSource);
public static T Separator<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ControlClassesInput
=> control._setEx(Ursa.Controls.ControlClassesInput.SeparatorProperty, ps, () => control.Separator = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

