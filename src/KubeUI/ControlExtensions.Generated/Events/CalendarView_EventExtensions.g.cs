using Avalonia.Data;
using Avalonia.Data.Converters;
using CalendarView = Ursa.Controls.CalendarView;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class CalendarViewEventsExtensions
{
    public static T OnDateSelected<T>(this T control, Action<System.Object, Ursa.Controls.CalendarDayButtonEventArgs> action) where T : Ursa.Controls.CalendarView => 
        control._setEvent((System.EventHandler<Ursa.Controls.CalendarDayButtonEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.DateSelected += h);
    public static T OnDatePreviewed<T>(this T control, Action<System.Object, Ursa.Controls.CalendarDayButtonEventArgs> action) where T : Ursa.Controls.CalendarView => 
        control._setEvent((System.EventHandler<Ursa.Controls.CalendarDayButtonEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.DatePreviewed += h);
}

