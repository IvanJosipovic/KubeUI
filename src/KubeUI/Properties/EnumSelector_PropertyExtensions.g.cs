#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using EnumSelector = Ursa.Controls.EnumSelector;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class EnumSelectorExtensions
{
public static T EnumType<T>(this T control, IBinding binding) where T : Ursa.Controls.EnumSelector
   => control._set(Ursa.Controls.EnumSelector.EnumTypeProperty, binding);
public static T EnumType<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.EnumSelector
   => control._set(Ursa.Controls.EnumSelector.EnumTypeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T EnumType<T>(this T control, Func<System.Type> func, Action<System.Type>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.EnumSelector
   => control._set(Ursa.Controls.EnumSelector.EnumTypeProperty, func, onChanged, expression);
public static T EnumType<T>(this T control, System.Type value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.EnumSelector
=> control._setEx(Ursa.Controls.EnumSelector.EnumTypeProperty, ps, () => control.EnumType = value, bindingMode, converter, bindingSource);
public static T EnumType<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Type> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.EnumSelector
=> control._setEx(Ursa.Controls.EnumSelector.EnumTypeProperty, ps, () => control.EnumType = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Value<T>(this T control, IBinding binding) where T : Ursa.Controls.EnumSelector
   => control._set(Ursa.Controls.EnumSelector.ValueProperty, binding);
public static T Value<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.EnumSelector
   => control._set(Ursa.Controls.EnumSelector.ValueProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Value<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.EnumSelector
   => control._set(Ursa.Controls.EnumSelector.ValueProperty, func, onChanged, expression);
public static T Value<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.EnumSelector
=> control._setEx(Ursa.Controls.EnumSelector.ValueProperty, ps, () => control.Value = value, bindingMode, converter, bindingSource);
public static T Value<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.EnumSelector
=> control._setEx(Ursa.Controls.EnumSelector.ValueProperty, ps, () => control.Value = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T DisplayDescription<T>(this T control, IBinding binding) where T : Ursa.Controls.EnumSelector
   => control._set(Ursa.Controls.EnumSelector.DisplayDescriptionProperty, binding);
public static T DisplayDescription<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.EnumSelector
   => control._set(Ursa.Controls.EnumSelector.DisplayDescriptionProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T DisplayDescription<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.EnumSelector
   => control._set(Ursa.Controls.EnumSelector.DisplayDescriptionProperty, func, onChanged, expression);
public static T DisplayDescription<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.EnumSelector
=> control._setEx(Ursa.Controls.EnumSelector.DisplayDescriptionProperty, ps, () => control.DisplayDescription = value, bindingMode, converter, bindingSource);
public static T DisplayDescription<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.EnumSelector
=> control._setEx(Ursa.Controls.EnumSelector.DisplayDescriptionProperty, ps, () => control.DisplayDescription = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

