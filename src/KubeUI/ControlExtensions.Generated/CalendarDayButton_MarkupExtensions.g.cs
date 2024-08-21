#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class CalendarDayButton_MarkupExtensions
{
//================= Properties ======================//

//================= Events ======================//
 // DateSelected

/*ActionToEventGenerator*/
    public static T OnDateSelected<T>(this T control, Action<Ursa.Controls.CalendarDayButtonEventArgs> action) where T : Ursa.Controls.CalendarDayButton => 
        control._setEvent((System.EventHandler<Ursa.Controls.CalendarDayButtonEventArgs>) ((arg0, arg1) => action(arg1)), h => control.DateSelected += h);


 // DatePreviewed

/*ActionToEventGenerator*/
    public static T OnDatePreviewed<T>(this T control, Action<Ursa.Controls.CalendarDayButtonEventArgs> action) where T : Ursa.Controls.CalendarDayButton => 
        control._setEvent((System.EventHandler<Ursa.Controls.CalendarDayButtonEventArgs>) ((arg0, arg1) => action(arg1)), h => control.DatePreviewed += h);



//================= Styles ======================//

}
