using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using NavigationView = FluentAvalonia.UI.Controls.NavigationView;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class NavigationViewEventsExtensions
{
    public static T OnPaneClosing<T>(this T control, Action<FluentAvalonia.UI.Controls.NavigationView, FluentAvalonia.UI.Controls.NavigationViewPaneClosingEventArgs> action) where T : FluentAvalonia.UI.Controls.NavigationView => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.NavigationView,FluentAvalonia.UI.Controls.NavigationViewPaneClosingEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.PaneClosing += h);
    public static T OnPaneClosed<T>(this T control, Action<FluentAvalonia.UI.Controls.NavigationView, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.NavigationView => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.NavigationView,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.PaneClosed += h);
    public static T OnPaneOpening<T>(this T control, Action<FluentAvalonia.UI.Controls.NavigationView, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.NavigationView => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.NavigationView,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.PaneOpening += h);
    public static T OnPaneOpened<T>(this T control, Action<FluentAvalonia.UI.Controls.NavigationView, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.NavigationView => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.NavigationView,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.PaneOpened += h);
    public static T OnBackRequested<T>(this T control, Action<System.Object, FluentAvalonia.UI.Controls.NavigationViewBackRequestedEventArgs> action) where T : FluentAvalonia.UI.Controls.NavigationView => 
        control._setEvent((System.EventHandler<FluentAvalonia.UI.Controls.NavigationViewBackRequestedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.BackRequested += h);
    public static T OnSelectionChanged<T>(this T control, Action<System.Object, FluentAvalonia.UI.Controls.NavigationViewSelectionChangedEventArgs> action) where T : FluentAvalonia.UI.Controls.NavigationView => 
        control._setEvent((System.EventHandler<FluentAvalonia.UI.Controls.NavigationViewSelectionChangedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.SelectionChanged += h);
    public static T OnItemInvoked<T>(this T control, Action<System.Object, FluentAvalonia.UI.Controls.NavigationViewItemInvokedEventArgs> action) where T : FluentAvalonia.UI.Controls.NavigationView => 
        control._setEvent((System.EventHandler<FluentAvalonia.UI.Controls.NavigationViewItemInvokedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.ItemInvoked += h);
    public static T OnDisplayModeChanged<T>(this T control, Action<System.Object, FluentAvalonia.UI.Controls.NavigationViewDisplayModeChangedEventArgs> action) where T : FluentAvalonia.UI.Controls.NavigationView => 
        control._setEvent((System.EventHandler<FluentAvalonia.UI.Controls.NavigationViewDisplayModeChangedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.DisplayModeChanged += h);
    public static T OnItemExpanding<T>(this T control, Action<System.Object, FluentAvalonia.UI.Controls.NavigationViewItemExpandingEventArgs> action) where T : FluentAvalonia.UI.Controls.NavigationView => 
        control._setEvent((System.EventHandler<FluentAvalonia.UI.Controls.NavigationViewItemExpandingEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.ItemExpanding += h);
    public static T OnItemCollapsed<T>(this T control, Action<System.Object, FluentAvalonia.UI.Controls.NavigationViewItemCollapsedEventArgs> action) where T : FluentAvalonia.UI.Controls.NavigationView => 
        control._setEvent((System.EventHandler<FluentAvalonia.UI.Controls.NavigationViewItemCollapsedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.ItemCollapsed += h);
}

