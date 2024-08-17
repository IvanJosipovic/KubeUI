using Avalonia.Data;
using Avalonia.Data.Converters;
using CalendarYearButton = Ursa.Controls.CalendarYearButton;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class CalendarYearButtonEventsExtensions
{
    public static T OnItemSelected<T>(this T control, Action<System.Object, Ursa.Controls.CalendarDayButtonEventArgs> action) where T : Ursa.Controls.CalendarYearButton => 
        control._setEvent((System.EventHandler<Ursa.Controls.CalendarDayButtonEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.ItemSelected += h);
}

