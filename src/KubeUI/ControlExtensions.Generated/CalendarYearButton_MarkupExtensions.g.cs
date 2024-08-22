#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class CalendarYearButton_MarkupExtensions
{
//================= Properties ======================//

//================= Events ======================//
 // ItemSelected

/*ActionToEventGenerator*/
    public static T OnItemSelected<T>(this T control, Action<Ursa.Controls.CalendarDayButtonEventArgs> action) where T : Ursa.Controls.CalendarYearButton => 
        control._setEvent((System.EventHandler<Ursa.Controls.CalendarDayButtonEventArgs>) ((arg0, arg1) => action(arg1)), h => control.ItemSelected += h);



//================= Styles ======================//

}
