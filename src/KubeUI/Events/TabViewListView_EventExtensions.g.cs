using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls.Primitives;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TabViewListView = FluentAvalonia.UI.Controls.Primitives.TabViewListView;

namespace Avalonia.Markup.Declarative;
public static partial class TabViewListViewEventsExtensions
{
    public static T OnDragOver<T>(this T control, Action<System.Object, Avalonia.Input.DragEventArgs> action) where T : FluentAvalonia.UI.Controls.Primitives.TabViewListView => 
        control._setEvent((System.EventHandler<Avalonia.Input.DragEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.DragOver += h);
    public static T OnDrop<T>(this T control, Action<System.Object, Avalonia.Input.DragEventArgs> action) where T : FluentAvalonia.UI.Controls.Primitives.TabViewListView => 
        control._setEvent((System.EventHandler<Avalonia.Input.DragEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Drop += h);
    public static T OnDragItemsStarting<T>(this T control, Action<System.Object, DragItemsStartingEventArgs> action) where T : FluentAvalonia.UI.Controls.Primitives.TabViewListView => 
        control._setEvent((DragItemsStartingEventHandler) ((arg0, arg1) => action(arg0, arg1)), h => control.DragItemsStarting += h);
    public static T OnDragItemsCompleted<T>(this T control, Action<FluentAvalonia.UI.Controls.Primitives.TabViewListView, DragItemsCompletedEventArgs> action) where T : FluentAvalonia.UI.Controls.Primitives.TabViewListView => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.Primitives.TabViewListView,DragItemsCompletedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.DragItemsCompleted += h);
}

