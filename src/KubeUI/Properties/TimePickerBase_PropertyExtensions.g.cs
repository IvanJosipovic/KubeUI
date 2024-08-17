#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TimePickerBase = Ursa.Controls.TimePickerBase;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class TimePickerBaseExtensions
{
public static T DisplayFormat<T>(this T control, IBinding binding) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.DisplayFormatProperty, binding);
public static T DisplayFormat<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.DisplayFormatProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T DisplayFormat<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.DisplayFormatProperty, func, onChanged, expression);
public static T DisplayFormat<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerBase
=> control._setEx(Ursa.Controls.TimePickerBase.DisplayFormatProperty, ps, () => control.DisplayFormat = value, bindingMode, converter, bindingSource);
public static T DisplayFormat<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerBase
=> control._setEx(Ursa.Controls.TimePickerBase.DisplayFormatProperty, ps, () => control.DisplayFormat = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T PanelFormat<T>(this T control, IBinding binding) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.PanelFormatProperty, binding);
public static T PanelFormat<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.PanelFormatProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T PanelFormat<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.PanelFormatProperty, func, onChanged, expression);
public static T PanelFormat<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerBase
=> control._setEx(Ursa.Controls.TimePickerBase.PanelFormatProperty, ps, () => control.PanelFormat = value, bindingMode, converter, bindingSource);
public static T PanelFormat<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerBase
=> control._setEx(Ursa.Controls.TimePickerBase.PanelFormatProperty, ps, () => control.PanelFormat = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T NeedConfirmation<T>(this T control, IBinding binding) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.NeedConfirmationProperty, binding);
public static T NeedConfirmation<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.NeedConfirmationProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T NeedConfirmation<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.NeedConfirmationProperty, func, onChanged, expression);
public static T NeedConfirmation<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerBase
=> control._setEx(Ursa.Controls.TimePickerBase.NeedConfirmationProperty, ps, () => control.NeedConfirmation = value, bindingMode, converter, bindingSource);
public static T NeedConfirmation<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerBase
=> control._setEx(Ursa.Controls.TimePickerBase.NeedConfirmationProperty, ps, () => control.NeedConfirmation = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T InnerLeftContent<T>(this T control, IBinding binding) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.InnerLeftContentProperty, binding);
public static T InnerLeftContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.InnerLeftContentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T InnerLeftContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.InnerLeftContentProperty, func, onChanged, expression);
public static T InnerLeftContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerBase
=> control._setEx(Ursa.Controls.TimePickerBase.InnerLeftContentProperty, ps, () => control.InnerLeftContent = value, bindingMode, converter, bindingSource);
public static T InnerLeftContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerBase
=> control._setEx(Ursa.Controls.TimePickerBase.InnerLeftContentProperty, ps, () => control.InnerLeftContent = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T InnerRightContent<T>(this T control, IBinding binding) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.InnerRightContentProperty, binding);
public static T InnerRightContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.InnerRightContentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T InnerRightContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.InnerRightContentProperty, func, onChanged, expression);
public static T InnerRightContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerBase
=> control._setEx(Ursa.Controls.TimePickerBase.InnerRightContentProperty, ps, () => control.InnerRightContent = value, bindingMode, converter, bindingSource);
public static T InnerRightContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerBase
=> control._setEx(Ursa.Controls.TimePickerBase.InnerRightContentProperty, ps, () => control.InnerRightContent = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T PopupInnerTopContent<T>(this T control, IBinding binding) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.PopupInnerTopContentProperty, binding);
public static T PopupInnerTopContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.PopupInnerTopContentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T PopupInnerTopContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.PopupInnerTopContentProperty, func, onChanged, expression);
public static T PopupInnerTopContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerBase
=> control._setEx(Ursa.Controls.TimePickerBase.PopupInnerTopContentProperty, ps, () => control.PopupInnerTopContent = value, bindingMode, converter, bindingSource);
public static T PopupInnerTopContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerBase
=> control._setEx(Ursa.Controls.TimePickerBase.PopupInnerTopContentProperty, ps, () => control.PopupInnerTopContent = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T PopupInnerBottomContent<T>(this T control, IBinding binding) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.PopupInnerBottomContentProperty, binding);
public static T PopupInnerBottomContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.PopupInnerBottomContentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T PopupInnerBottomContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.PopupInnerBottomContentProperty, func, onChanged, expression);
public static T PopupInnerBottomContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerBase
=> control._setEx(Ursa.Controls.TimePickerBase.PopupInnerBottomContentProperty, ps, () => control.PopupInnerBottomContent = value, bindingMode, converter, bindingSource);
public static T PopupInnerBottomContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerBase
=> control._setEx(Ursa.Controls.TimePickerBase.PopupInnerBottomContentProperty, ps, () => control.PopupInnerBottomContent = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsDropdownOpen<T>(this T control, IBinding binding) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.IsDropdownOpenProperty, binding);
public static T IsDropdownOpen<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.IsDropdownOpenProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsDropdownOpen<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.IsDropdownOpenProperty, func, onChanged, expression);
public static T IsDropdownOpen<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerBase
=> control._setEx(Ursa.Controls.TimePickerBase.IsDropdownOpenProperty, ps, () => control.IsDropdownOpen = value, bindingMode, converter, bindingSource);
public static T IsDropdownOpen<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerBase
=> control._setEx(Ursa.Controls.TimePickerBase.IsDropdownOpenProperty, ps, () => control.IsDropdownOpen = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsReadonly<T>(this T control, IBinding binding) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.IsReadonlyProperty, binding);
public static T IsReadonly<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.IsReadonlyProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsReadonly<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.IsReadonlyProperty, func, onChanged, expression);
public static T IsReadonly<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerBase
=> control._setEx(Ursa.Controls.TimePickerBase.IsReadonlyProperty, ps, () => control.IsReadonly = value, bindingMode, converter, bindingSource);
public static T IsReadonly<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerBase
=> control._setEx(Ursa.Controls.TimePickerBase.IsReadonlyProperty, ps, () => control.IsReadonly = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

