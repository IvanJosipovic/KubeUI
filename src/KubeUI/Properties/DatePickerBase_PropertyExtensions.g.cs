#nullable enable
using Avalonia.Collections;
using Avalonia.Data;
using Avalonia.Data.Converters;
using DatePickerBase = Ursa.Controls.DatePickerBase;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class DatePickerBaseExtensions
{
public static T DisplayFormat<T>(this T control, IBinding binding) where T : Ursa.Controls.DatePickerBase
   => control._set(Ursa.Controls.DatePickerBase.DisplayFormatProperty, binding);
public static T DisplayFormat<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DatePickerBase
   => control._set(Ursa.Controls.DatePickerBase.DisplayFormatProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T DisplayFormat<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DatePickerBase
   => control._set(Ursa.Controls.DatePickerBase.DisplayFormatProperty, func, onChanged, expression);
public static T DisplayFormat<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase
=> control._setEx(Ursa.Controls.DatePickerBase.DisplayFormatProperty, ps, () => control.DisplayFormat = value, bindingMode, converter, bindingSource);
public static T DisplayFormat<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase
=> control._setEx(Ursa.Controls.DatePickerBase.DisplayFormatProperty, ps, () => control.DisplayFormat = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T BlackoutDates<T>(this T control, IBinding binding) where T : Ursa.Controls.DatePickerBase
   => control._set(Ursa.Controls.DatePickerBase.BlackoutDatesProperty, binding);
public static T BlackoutDates<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DatePickerBase
   => control._set(Ursa.Controls.DatePickerBase.BlackoutDatesProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T BlackoutDates<T>(this T control, Func<Avalonia.Collections.AvaloniaList<Ursa.Controls.DateRange>> func, Action<Avalonia.Collections.AvaloniaList<Ursa.Controls.DateRange>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DatePickerBase
   => control._set(Ursa.Controls.DatePickerBase.BlackoutDatesProperty, func, onChanged, expression);
public static T BlackoutDates<T>(this T control, Avalonia.Collections.AvaloniaList<Ursa.Controls.DateRange> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase
=> control._setEx(Ursa.Controls.DatePickerBase.BlackoutDatesProperty, ps, () => control.BlackoutDates = value, bindingMode, converter, bindingSource);
public static T BlackoutDates<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Collections.AvaloniaList<Ursa.Controls.DateRange>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase
=> control._setEx(Ursa.Controls.DatePickerBase.BlackoutDatesProperty, ps, () => control.BlackoutDates = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T BlackoutDateRule<T>(this T control, IBinding binding) where T : Ursa.Controls.DatePickerBase
   => control._set(Ursa.Controls.DatePickerBase.BlackoutDateRuleProperty, binding);
public static T BlackoutDateRule<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DatePickerBase
   => control._set(Ursa.Controls.DatePickerBase.BlackoutDateRuleProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T BlackoutDateRule<T>(this T control, Func<Ursa.Controls.IDateSelector> func, Action<Ursa.Controls.IDateSelector>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DatePickerBase
   => control._set(Ursa.Controls.DatePickerBase.BlackoutDateRuleProperty, func, onChanged, expression);
public static T BlackoutDateRule<T>(this T control, Ursa.Controls.IDateSelector value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase
=> control._setEx(Ursa.Controls.DatePickerBase.BlackoutDateRuleProperty, ps, () => control.BlackoutDateRule = value, bindingMode, converter, bindingSource);
public static T BlackoutDateRule<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Ursa.Controls.IDateSelector> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase
=> control._setEx(Ursa.Controls.DatePickerBase.BlackoutDateRuleProperty, ps, () => control.BlackoutDateRule = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T FirstDayOfWeek<T>(this T control, IBinding binding) where T : Ursa.Controls.DatePickerBase
   => control._set(Ursa.Controls.DatePickerBase.FirstDayOfWeekProperty, binding);
public static T FirstDayOfWeek<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DatePickerBase
   => control._set(Ursa.Controls.DatePickerBase.FirstDayOfWeekProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T FirstDayOfWeek<T>(this T control, Func<System.DayOfWeek> func, Action<System.DayOfWeek>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DatePickerBase
   => control._set(Ursa.Controls.DatePickerBase.FirstDayOfWeekProperty, func, onChanged, expression);
public static T FirstDayOfWeek<T>(this T control, System.DayOfWeek value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase
=> control._setEx(Ursa.Controls.DatePickerBase.FirstDayOfWeekProperty, ps, () => control.FirstDayOfWeek = value, bindingMode, converter, bindingSource);
public static T FirstDayOfWeek<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.DayOfWeek> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase
=> control._setEx(Ursa.Controls.DatePickerBase.FirstDayOfWeekProperty, ps, () => control.FirstDayOfWeek = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsTodayHighlighted<T>(this T control, IBinding binding) where T : Ursa.Controls.DatePickerBase
   => control._set(Ursa.Controls.DatePickerBase.IsTodayHighlightedProperty, binding);
public static T IsTodayHighlighted<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DatePickerBase
   => control._set(Ursa.Controls.DatePickerBase.IsTodayHighlightedProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsTodayHighlighted<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DatePickerBase
   => control._set(Ursa.Controls.DatePickerBase.IsTodayHighlightedProperty, func, onChanged, expression);
public static T IsTodayHighlighted<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase
=> control._setEx(Ursa.Controls.DatePickerBase.IsTodayHighlightedProperty, ps, () => control.IsTodayHighlighted = value, bindingMode, converter, bindingSource);
public static T IsTodayHighlighted<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase
=> control._setEx(Ursa.Controls.DatePickerBase.IsTodayHighlightedProperty, ps, () => control.IsTodayHighlighted = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T InnerLeftContent<T>(this T control, IBinding binding) where T : Ursa.Controls.DatePickerBase
   => control._set(Ursa.Controls.DatePickerBase.InnerLeftContentProperty, binding);
public static T InnerLeftContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DatePickerBase
   => control._set(Ursa.Controls.DatePickerBase.InnerLeftContentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T InnerLeftContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DatePickerBase
   => control._set(Ursa.Controls.DatePickerBase.InnerLeftContentProperty, func, onChanged, expression);
public static T InnerLeftContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase
=> control._setEx(Ursa.Controls.DatePickerBase.InnerLeftContentProperty, ps, () => control.InnerLeftContent = value, bindingMode, converter, bindingSource);
public static T InnerLeftContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase
=> control._setEx(Ursa.Controls.DatePickerBase.InnerLeftContentProperty, ps, () => control.InnerLeftContent = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T InnerRightContent<T>(this T control, IBinding binding) where T : Ursa.Controls.DatePickerBase
   => control._set(Ursa.Controls.DatePickerBase.InnerRightContentProperty, binding);
public static T InnerRightContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DatePickerBase
   => control._set(Ursa.Controls.DatePickerBase.InnerRightContentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T InnerRightContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DatePickerBase
   => control._set(Ursa.Controls.DatePickerBase.InnerRightContentProperty, func, onChanged, expression);
public static T InnerRightContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase
=> control._setEx(Ursa.Controls.DatePickerBase.InnerRightContentProperty, ps, () => control.InnerRightContent = value, bindingMode, converter, bindingSource);
public static T InnerRightContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase
=> control._setEx(Ursa.Controls.DatePickerBase.InnerRightContentProperty, ps, () => control.InnerRightContent = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T PopupInnerTopContent<T>(this T control, IBinding binding) where T : Ursa.Controls.DatePickerBase
   => control._set(Ursa.Controls.DatePickerBase.PopupInnerTopContentProperty, binding);
public static T PopupInnerTopContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DatePickerBase
   => control._set(Ursa.Controls.DatePickerBase.PopupInnerTopContentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T PopupInnerTopContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DatePickerBase
   => control._set(Ursa.Controls.DatePickerBase.PopupInnerTopContentProperty, func, onChanged, expression);
public static T PopupInnerTopContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase
=> control._setEx(Ursa.Controls.DatePickerBase.PopupInnerTopContentProperty, ps, () => control.PopupInnerTopContent = value, bindingMode, converter, bindingSource);
public static T PopupInnerTopContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase
=> control._setEx(Ursa.Controls.DatePickerBase.PopupInnerTopContentProperty, ps, () => control.PopupInnerTopContent = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T PopupInnerBottomContent<T>(this T control, IBinding binding) where T : Ursa.Controls.DatePickerBase
   => control._set(Ursa.Controls.DatePickerBase.PopupInnerBottomContentProperty, binding);
public static T PopupInnerBottomContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DatePickerBase
   => control._set(Ursa.Controls.DatePickerBase.PopupInnerBottomContentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T PopupInnerBottomContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DatePickerBase
   => control._set(Ursa.Controls.DatePickerBase.PopupInnerBottomContentProperty, func, onChanged, expression);
public static T PopupInnerBottomContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase
=> control._setEx(Ursa.Controls.DatePickerBase.PopupInnerBottomContentProperty, ps, () => control.PopupInnerBottomContent = value, bindingMode, converter, bindingSource);
public static T PopupInnerBottomContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase
=> control._setEx(Ursa.Controls.DatePickerBase.PopupInnerBottomContentProperty, ps, () => control.PopupInnerBottomContent = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsDropdownOpen<T>(this T control, IBinding binding) where T : Ursa.Controls.DatePickerBase
   => control._set(Ursa.Controls.DatePickerBase.IsDropdownOpenProperty, binding);
public static T IsDropdownOpen<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DatePickerBase
   => control._set(Ursa.Controls.DatePickerBase.IsDropdownOpenProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsDropdownOpen<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DatePickerBase
   => control._set(Ursa.Controls.DatePickerBase.IsDropdownOpenProperty, func, onChanged, expression);
public static T IsDropdownOpen<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase
=> control._setEx(Ursa.Controls.DatePickerBase.IsDropdownOpenProperty, ps, () => control.IsDropdownOpen = value, bindingMode, converter, bindingSource);
public static T IsDropdownOpen<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase
=> control._setEx(Ursa.Controls.DatePickerBase.IsDropdownOpenProperty, ps, () => control.IsDropdownOpen = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsReadonly<T>(this T control, IBinding binding) where T : Ursa.Controls.DatePickerBase
   => control._set(Ursa.Controls.DatePickerBase.IsReadonlyProperty, binding);
public static T IsReadonly<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DatePickerBase
   => control._set(Ursa.Controls.DatePickerBase.IsReadonlyProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsReadonly<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DatePickerBase
   => control._set(Ursa.Controls.DatePickerBase.IsReadonlyProperty, func, onChanged, expression);
public static T IsReadonly<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase
=> control._setEx(Ursa.Controls.DatePickerBase.IsReadonlyProperty, ps, () => control.IsReadonly = value, bindingMode, converter, bindingSource);
public static T IsReadonly<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase
=> control._setEx(Ursa.Controls.DatePickerBase.IsReadonlyProperty, ps, () => control.IsReadonly = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

