using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using NavMenu = Ursa.Controls.NavMenu;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class NavMenuEventsExtensions
{
    public static T OnSelectionChanged<T>(this T control, Action<System.Object, Avalonia.Controls.SelectionChangedEventArgs> action) where T : Ursa.Controls.NavMenu => 
        control._setEvent((System.EventHandler<Avalonia.Controls.SelectionChangedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.SelectionChanged += h);
}

