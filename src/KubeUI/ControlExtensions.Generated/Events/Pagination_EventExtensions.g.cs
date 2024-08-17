using Avalonia.Data;
using Avalonia.Data.Converters;
using Pagination = Ursa.Controls.Pagination;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class PaginationEventsExtensions
{
    public static T OnCurrentPageChanged<T>(this T control, Action<System.Object, Ursa.Controls.ValueChangedEventArgs<System.Int32>> action) where T : Ursa.Controls.Pagination => 
        control._setEvent((System.EventHandler<Ursa.Controls.ValueChangedEventArgs<System.Int32>>) ((arg0, arg1) => action(arg0, arg1)), h => control.CurrentPageChanged += h);
}

