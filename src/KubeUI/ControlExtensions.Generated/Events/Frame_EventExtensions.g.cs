using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using Frame = FluentAvalonia.UI.Controls.Frame;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class FrameEventsExtensions
{
    public static T OnNavigated<T>(this T control, Action<System.Object, FluentAvalonia.UI.Navigation.NavigationEventArgs> action) where T : FluentAvalonia.UI.Controls.Frame => 
        control._setEvent((FluentAvalonia.UI.Navigation.NavigatedEventHandler) ((arg0, arg1) => action(arg0, arg1)), h => control.Navigated += h);
    public static T OnNavigating<T>(this T control, Action<System.Object, FluentAvalonia.UI.Navigation.NavigatingCancelEventArgs> action) where T : FluentAvalonia.UI.Controls.Frame => 
        control._setEvent((FluentAvalonia.UI.Navigation.NavigatingCancelEventHandler) ((arg0, arg1) => action(arg0, arg1)), h => control.Navigating += h);
    public static T OnNavigationFailed<T>(this T control, Action<System.Object, FluentAvalonia.UI.Navigation.NavigationFailedEventArgs> action) where T : FluentAvalonia.UI.Controls.Frame => 
        control._setEvent((FluentAvalonia.UI.Navigation.NavigationFailedEventHandler) ((arg0, arg1) => action(arg0, arg1)), h => control.NavigationFailed += h);
    public static T OnNavigationStopped<T>(this T control, Action<System.Object, FluentAvalonia.UI.Navigation.NavigationEventArgs> action) where T : FluentAvalonia.UI.Controls.Frame => 
        control._setEvent((FluentAvalonia.UI.Navigation.NavigationStoppedEventHandler) ((arg0, arg1) => action(arg0, arg1)), h => control.NavigationStopped += h);
}

