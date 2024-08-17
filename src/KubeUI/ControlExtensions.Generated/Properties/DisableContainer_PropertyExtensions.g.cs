#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using DisableContainer = Ursa.Controls.DisableContainer;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class DisableContainerExtensions
{
public static T Content<T>(this T control, IBinding binding) where T : Ursa.Controls.DisableContainer
   => control._set(Ursa.Controls.DisableContainer.ContentProperty, binding);
public static T Content<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DisableContainer
   => control._set(Ursa.Controls.DisableContainer.ContentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Content<T>(this T control, Func<Avalonia.Input.InputElement> func, Action<Avalonia.Input.InputElement>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DisableContainer
   => control._set(Ursa.Controls.DisableContainer.ContentProperty, func, onChanged, expression);
public static T Content<T>(this T control, Avalonia.Input.InputElement value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DisableContainer
=> control._setEx(Ursa.Controls.DisableContainer.ContentProperty, ps, () => control.Content = value, bindingMode, converter, bindingSource);
public static T Content<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Input.InputElement> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DisableContainer
=> control._setEx(Ursa.Controls.DisableContainer.ContentProperty, ps, () => control.Content = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T DisabledTip<T>(this T control, IBinding binding) where T : Ursa.Controls.DisableContainer
   => control._set(Ursa.Controls.DisableContainer.DisabledTipProperty, binding);
public static T DisabledTip<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DisableContainer
   => control._set(Ursa.Controls.DisableContainer.DisabledTipProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T DisabledTip<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DisableContainer
   => control._set(Ursa.Controls.DisableContainer.DisabledTipProperty, func, onChanged, expression);
public static T DisabledTip<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DisableContainer
=> control._setEx(Ursa.Controls.DisableContainer.DisabledTipProperty, ps, () => control.DisabledTip = value, bindingMode, converter, bindingSource);
public static T DisabledTip<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DisableContainer
=> control._setEx(Ursa.Controls.DisableContainer.DisabledTipProperty, ps, () => control.DisabledTip = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

