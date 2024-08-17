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
public static Style<T> DisplayFormat<T>(this Style<T> style, System.String value) where T : Ursa.Controls.DatePickerBase
=> style._addSetter(Ursa.Controls.DatePickerBase.DisplayFormatProperty, value);
public static Style<T> DisplayFormat<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DatePickerBase
=> style._addSetter(Ursa.Controls.DatePickerBase.DisplayFormatProperty, binding);
public static Style<T> BlackoutDates<T>(this Style<T> style, Avalonia.Collections.AvaloniaList<Ursa.Controls.DateRange> value) where T : Ursa.Controls.DatePickerBase
=> style._addSetter(Ursa.Controls.DatePickerBase.BlackoutDatesProperty, value);
public static Style<T> BlackoutDates<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DatePickerBase
=> style._addSetter(Ursa.Controls.DatePickerBase.BlackoutDatesProperty, binding);
public static Style<T> BlackoutDateRule<T>(this Style<T> style, Ursa.Controls.IDateSelector value) where T : Ursa.Controls.DatePickerBase
=> style._addSetter(Ursa.Controls.DatePickerBase.BlackoutDateRuleProperty, value);
public static Style<T> BlackoutDateRule<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DatePickerBase
=> style._addSetter(Ursa.Controls.DatePickerBase.BlackoutDateRuleProperty, binding);
public static Style<T> FirstDayOfWeek<T>(this Style<T> style, System.DayOfWeek value) where T : Ursa.Controls.DatePickerBase
=> style._addSetter(Ursa.Controls.DatePickerBase.FirstDayOfWeekProperty, value);
public static Style<T> FirstDayOfWeek<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DatePickerBase
=> style._addSetter(Ursa.Controls.DatePickerBase.FirstDayOfWeekProperty, binding);
public static Style<T> IsTodayHighlighted<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.DatePickerBase
=> style._addSetter(Ursa.Controls.DatePickerBase.IsTodayHighlightedProperty, value);
public static Style<T> IsTodayHighlighted<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DatePickerBase
=> style._addSetter(Ursa.Controls.DatePickerBase.IsTodayHighlightedProperty, binding);
public static Style<T> InnerLeftContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.DatePickerBase
=> style._addSetter(Ursa.Controls.DatePickerBase.InnerLeftContentProperty, value);
public static Style<T> InnerLeftContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DatePickerBase
=> style._addSetter(Ursa.Controls.DatePickerBase.InnerLeftContentProperty, binding);
public static Style<T> InnerRightContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.DatePickerBase
=> style._addSetter(Ursa.Controls.DatePickerBase.InnerRightContentProperty, value);
public static Style<T> InnerRightContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DatePickerBase
=> style._addSetter(Ursa.Controls.DatePickerBase.InnerRightContentProperty, binding);
public static Style<T> PopupInnerTopContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.DatePickerBase
=> style._addSetter(Ursa.Controls.DatePickerBase.PopupInnerTopContentProperty, value);
public static Style<T> PopupInnerTopContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DatePickerBase
=> style._addSetter(Ursa.Controls.DatePickerBase.PopupInnerTopContentProperty, binding);
public static Style<T> PopupInnerBottomContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.DatePickerBase
=> style._addSetter(Ursa.Controls.DatePickerBase.PopupInnerBottomContentProperty, value);
public static Style<T> PopupInnerBottomContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DatePickerBase
=> style._addSetter(Ursa.Controls.DatePickerBase.PopupInnerBottomContentProperty, binding);
public static Style<T> IsDropdownOpen<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.DatePickerBase
=> style._addSetter(Ursa.Controls.DatePickerBase.IsDropdownOpenProperty, value);
public static Style<T> IsDropdownOpen<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DatePickerBase
=> style._addSetter(Ursa.Controls.DatePickerBase.IsDropdownOpenProperty, binding);
public static Style<T> IsReadonly<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.DatePickerBase
=> style._addSetter(Ursa.Controls.DatePickerBase.IsReadonlyProperty, value);
public static Style<T> IsReadonly<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DatePickerBase
=> style._addSetter(Ursa.Controls.DatePickerBase.IsReadonlyProperty, binding);
}

