#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using ButtonGroup = Ursa.Controls.ButtonGroup;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class ButtonGroupExtensions
{
public static T CommandBinding<T>(this T control, IBinding binding) where T : Ursa.Controls.ButtonGroup
   => control._set(Ursa.Controls.ButtonGroup.CommandBindingProperty, binding);
public static T CommandBinding<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ButtonGroup
   => control._set(Ursa.Controls.ButtonGroup.CommandBindingProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CommandBinding<T>(this T control, Func<Avalonia.Data.IBinding> func, Action<Avalonia.Data.IBinding>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ButtonGroup
   => control._set(Ursa.Controls.ButtonGroup.CommandBindingProperty, func, onChanged, expression);
public static T CommandBinding<T>(this T control, Avalonia.Data.IBinding value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ButtonGroup
=> control._setEx(Ursa.Controls.ButtonGroup.CommandBindingProperty, ps, () => control.CommandBinding = value, bindingMode, converter, bindingSource);
public static T CommandBinding<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Data.IBinding> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ButtonGroup
=> control._setEx(Ursa.Controls.ButtonGroup.CommandBindingProperty, ps, () => control.CommandBinding = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T CommandParameterBinding<T>(this T control, IBinding binding) where T : Ursa.Controls.ButtonGroup
   => control._set(Ursa.Controls.ButtonGroup.CommandParameterBindingProperty, binding);
public static T CommandParameterBinding<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ButtonGroup
   => control._set(Ursa.Controls.ButtonGroup.CommandParameterBindingProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CommandParameterBinding<T>(this T control, Func<Avalonia.Data.IBinding> func, Action<Avalonia.Data.IBinding>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ButtonGroup
   => control._set(Ursa.Controls.ButtonGroup.CommandParameterBindingProperty, func, onChanged, expression);
public static T CommandParameterBinding<T>(this T control, Avalonia.Data.IBinding value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ButtonGroup
=> control._setEx(Ursa.Controls.ButtonGroup.CommandParameterBindingProperty, ps, () => control.CommandParameterBinding = value, bindingMode, converter, bindingSource);
public static T CommandParameterBinding<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Data.IBinding> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ButtonGroup
=> control._setEx(Ursa.Controls.ButtonGroup.CommandParameterBindingProperty, ps, () => control.CommandParameterBinding = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ContentBinding<T>(this T control, IBinding binding) where T : Ursa.Controls.ButtonGroup
   => control._set(Ursa.Controls.ButtonGroup.ContentBindingProperty, binding);
public static T ContentBinding<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ButtonGroup
   => control._set(Ursa.Controls.ButtonGroup.ContentBindingProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ContentBinding<T>(this T control, Func<Avalonia.Data.IBinding> func, Action<Avalonia.Data.IBinding>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ButtonGroup
   => control._set(Ursa.Controls.ButtonGroup.ContentBindingProperty, func, onChanged, expression);
public static T ContentBinding<T>(this T control, Avalonia.Data.IBinding value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ButtonGroup
=> control._setEx(Ursa.Controls.ButtonGroup.ContentBindingProperty, ps, () => control.ContentBinding = value, bindingMode, converter, bindingSource);
public static T ContentBinding<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Data.IBinding> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ButtonGroup
=> control._setEx(Ursa.Controls.ButtonGroup.ContentBindingProperty, ps, () => control.ContentBinding = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

