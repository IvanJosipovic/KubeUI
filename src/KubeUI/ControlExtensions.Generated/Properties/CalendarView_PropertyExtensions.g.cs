#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using CalendarView = Ursa.Controls.CalendarView;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class CalendarViewExtensions
{
public static T IsTodayHighlighted<T>(this T control, IBinding binding) where T : Ursa.Controls.CalendarView
   => control._set(Ursa.Controls.CalendarView.IsTodayHighlightedProperty, binding);
public static T IsTodayHighlighted<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.CalendarView
   => control._set(Ursa.Controls.CalendarView.IsTodayHighlightedProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsTodayHighlighted<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.CalendarView
   => control._set(Ursa.Controls.CalendarView.IsTodayHighlightedProperty, func, onChanged, expression);
public static T IsTodayHighlighted<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.CalendarView
=> control._setEx(Ursa.Controls.CalendarView.IsTodayHighlightedProperty, ps, () => control.IsTodayHighlighted = value, bindingMode, converter, bindingSource);
public static T IsTodayHighlighted<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.CalendarView
=> control._setEx(Ursa.Controls.CalendarView.IsTodayHighlightedProperty, ps, () => control.IsTodayHighlighted = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T FirstDayOfWeek<T>(this T control, IBinding binding) where T : Ursa.Controls.CalendarView
   => control._set(Ursa.Controls.CalendarView.FirstDayOfWeekProperty, binding);
public static T FirstDayOfWeek<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.CalendarView
   => control._set(Ursa.Controls.CalendarView.FirstDayOfWeekProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T FirstDayOfWeek<T>(this T control, Func<System.DayOfWeek> func, Action<System.DayOfWeek>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.CalendarView
   => control._set(Ursa.Controls.CalendarView.FirstDayOfWeekProperty, func, onChanged, expression);
public static T FirstDayOfWeek<T>(this T control, System.DayOfWeek value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.CalendarView
=> control._setEx(Ursa.Controls.CalendarView.FirstDayOfWeekProperty, ps, () => control.FirstDayOfWeek = value, bindingMode, converter, bindingSource);
public static T FirstDayOfWeek<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.DayOfWeek> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.CalendarView
=> control._setEx(Ursa.Controls.CalendarView.FirstDayOfWeekProperty, ps, () => control.FirstDayOfWeek = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

