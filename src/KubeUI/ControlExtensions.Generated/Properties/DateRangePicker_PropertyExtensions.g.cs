#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using DateRangePicker = Ursa.Controls.DateRangePicker;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class DateRangePickerExtensions
{
public static T SelectedStartDate<T>(this T control, IBinding binding) where T : Ursa.Controls.DateRangePicker
   => control._set(Ursa.Controls.DateRangePicker.SelectedStartDateProperty, binding);
public static T SelectedStartDate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DateRangePicker
   => control._set(Ursa.Controls.DateRangePicker.SelectedStartDateProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SelectedStartDate<T>(this T control, Func<System.Nullable<System.DateTime>> func, Action<System.Nullable<System.DateTime>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DateRangePicker
   => control._set(Ursa.Controls.DateRangePicker.SelectedStartDateProperty, func, onChanged, expression);
public static T SelectedStartDate<T>(this T control, System.Nullable<System.DateTime> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DateRangePicker
=> control._setEx(Ursa.Controls.DateRangePicker.SelectedStartDateProperty, ps, () => control.SelectedStartDate = value, bindingMode, converter, bindingSource);
public static T SelectedStartDate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Nullable<System.DateTime>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DateRangePicker
=> control._setEx(Ursa.Controls.DateRangePicker.SelectedStartDateProperty, ps, () => control.SelectedStartDate = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SelectedEndDate<T>(this T control, IBinding binding) where T : Ursa.Controls.DateRangePicker
   => control._set(Ursa.Controls.DateRangePicker.SelectedEndDateProperty, binding);
public static T SelectedEndDate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DateRangePicker
   => control._set(Ursa.Controls.DateRangePicker.SelectedEndDateProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SelectedEndDate<T>(this T control, Func<System.Nullable<System.DateTime>> func, Action<System.Nullable<System.DateTime>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DateRangePicker
   => control._set(Ursa.Controls.DateRangePicker.SelectedEndDateProperty, func, onChanged, expression);
public static T SelectedEndDate<T>(this T control, System.Nullable<System.DateTime> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DateRangePicker
=> control._setEx(Ursa.Controls.DateRangePicker.SelectedEndDateProperty, ps, () => control.SelectedEndDate = value, bindingMode, converter, bindingSource);
public static T SelectedEndDate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Nullable<System.DateTime>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DateRangePicker
=> control._setEx(Ursa.Controls.DateRangePicker.SelectedEndDateProperty, ps, () => control.SelectedEndDate = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T EnableMonthSync<T>(this T control, IBinding binding) where T : Ursa.Controls.DateRangePicker
   => control._set(Ursa.Controls.DateRangePicker.EnableMonthSyncProperty, binding);
public static T EnableMonthSync<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DateRangePicker
   => control._set(Ursa.Controls.DateRangePicker.EnableMonthSyncProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T EnableMonthSync<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DateRangePicker
   => control._set(Ursa.Controls.DateRangePicker.EnableMonthSyncProperty, func, onChanged, expression);
public static T EnableMonthSync<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DateRangePicker
=> control._setEx(Ursa.Controls.DateRangePicker.EnableMonthSyncProperty, ps, () => control.EnableMonthSync = value, bindingMode, converter, bindingSource);
public static T EnableMonthSync<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DateRangePicker
=> control._setEx(Ursa.Controls.DateRangePicker.EnableMonthSyncProperty, ps, () => control.EnableMonthSync = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

