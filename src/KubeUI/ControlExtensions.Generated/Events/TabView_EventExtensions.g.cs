using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using System;
using System.Collections.Specialized;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TabView = FluentAvalonia.UI.Controls.TabView;

namespace Avalonia.Markup.Declarative;
public static partial class TabViewEventsExtensions
{
    public static T OnTabCloseRequested<T>(this T control, Action<FluentAvalonia.UI.Controls.TabView, FluentAvalonia.UI.Controls.TabViewTabCloseRequestedEventArgs> action) where T : FluentAvalonia.UI.Controls.TabView => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.TabView,FluentAvalonia.UI.Controls.TabViewTabCloseRequestedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.TabCloseRequested += h);
    public static T OnTabDroppedOutside<T>(this T control, Action<FluentAvalonia.UI.Controls.TabView, FluentAvalonia.UI.Controls.TabViewTabDroppedOutsideEventArgs> action) where T : FluentAvalonia.UI.Controls.TabView => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.TabView,FluentAvalonia.UI.Controls.TabViewTabDroppedOutsideEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.TabDroppedOutside += h);
    public static T OnAddTabButtonClick<T>(this T control, Action<FluentAvalonia.UI.Controls.TabView, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.TabView => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.TabView,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.AddTabButtonClick += h);
    public static T OnTabItemsChanged<T>(this T control, Action<FluentAvalonia.UI.Controls.TabView, System.Collections.Specialized.NotifyCollectionChangedEventArgs> action) where T : FluentAvalonia.UI.Controls.TabView => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.TabView,System.Collections.Specialized.NotifyCollectionChangedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.TabItemsChanged += h);
    public static T OnSelectionChanged<T>(this T control, Action<System.Object, Avalonia.Controls.SelectionChangedEventArgs> action) where T : FluentAvalonia.UI.Controls.TabView => 
        control._setEvent((FluentAvalonia.Core.SelectionChangedEventHandler) ((arg0, arg1) => action(arg0, arg1)), h => control.SelectionChanged += h);
    public static T OnTabDragStarting<T>(this T control, Action<FluentAvalonia.UI.Controls.TabView, FluentAvalonia.UI.Controls.TabViewTabDragStartingEventArgs> action) where T : FluentAvalonia.UI.Controls.TabView => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.TabView,FluentAvalonia.UI.Controls.TabViewTabDragStartingEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.TabDragStarting += h);
    public static T OnTabDragCompleted<T>(this T control, Action<FluentAvalonia.UI.Controls.TabView, FluentAvalonia.UI.Controls.TabViewTabDragCompletedEventArgs> action) where T : FluentAvalonia.UI.Controls.TabView => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.TabView,FluentAvalonia.UI.Controls.TabViewTabDragCompletedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.TabDragCompleted += h);
    public static T OnTabStripDragOver<T>(this T control, Action<System.Object, Avalonia.Input.DragEventArgs> action) where T : FluentAvalonia.UI.Controls.TabView => 
        control._setEvent((System.EventHandler<Avalonia.Input.DragEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.TabStripDragOver += h);
    public static T OnTabStripDrop<T>(this T control, Action<System.Object, Avalonia.Input.DragEventArgs> action) where T : FluentAvalonia.UI.Controls.TabView => 
        control._setEvent((System.EventHandler<Avalonia.Input.DragEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.TabStripDrop += h);
}

