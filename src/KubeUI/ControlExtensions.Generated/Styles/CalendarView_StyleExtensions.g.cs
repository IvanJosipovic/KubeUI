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
public static Style<T> IsTodayHighlighted<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.CalendarView
=> style._addSetter(Ursa.Controls.CalendarView.IsTodayHighlightedProperty, value);
public static Style<T> IsTodayHighlighted<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.CalendarView
=> style._addSetter(Ursa.Controls.CalendarView.IsTodayHighlightedProperty, binding);
public static Style<T> FirstDayOfWeek<T>(this Style<T> style, System.DayOfWeek value) where T : Ursa.Controls.CalendarView
=> style._addSetter(Ursa.Controls.CalendarView.FirstDayOfWeekProperty, value);
public static Style<T> FirstDayOfWeek<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.CalendarView
=> style._addSetter(Ursa.Controls.CalendarView.FirstDayOfWeekProperty, binding);
}

