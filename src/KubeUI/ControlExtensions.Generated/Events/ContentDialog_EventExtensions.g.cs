using Avalonia.Data;
using Avalonia.Data.Converters;
using ContentDialog = FluentAvalonia.UI.Controls.ContentDialog;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class ContentDialogEventsExtensions
{
    public static T OnOpening<T>(this T control, Action<FluentAvalonia.UI.Controls.ContentDialog, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.ContentDialog => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.ContentDialog,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Opening += h);
    public static T OnOpened<T>(this T control, Action<FluentAvalonia.UI.Controls.ContentDialog, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.ContentDialog => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.ContentDialog,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Opened += h);
    public static T OnClosing<T>(this T control, Action<FluentAvalonia.UI.Controls.ContentDialog, FluentAvalonia.UI.Controls.ContentDialogClosingEventArgs> action) where T : FluentAvalonia.UI.Controls.ContentDialog => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.ContentDialog,FluentAvalonia.UI.Controls.ContentDialogClosingEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Closing += h);
    public static T OnClosed<T>(this T control, Action<FluentAvalonia.UI.Controls.ContentDialog, FluentAvalonia.UI.Controls.ContentDialogClosedEventArgs> action) where T : FluentAvalonia.UI.Controls.ContentDialog => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.ContentDialog,FluentAvalonia.UI.Controls.ContentDialogClosedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Closed += h);
    public static T OnPrimaryButtonClick<T>(this T control, Action<FluentAvalonia.UI.Controls.ContentDialog, FluentAvalonia.UI.Controls.ContentDialogButtonClickEventArgs> action) where T : FluentAvalonia.UI.Controls.ContentDialog => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.ContentDialog,FluentAvalonia.UI.Controls.ContentDialogButtonClickEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.PrimaryButtonClick += h);
    public static T OnSecondaryButtonClick<T>(this T control, Action<FluentAvalonia.UI.Controls.ContentDialog, FluentAvalonia.UI.Controls.ContentDialogButtonClickEventArgs> action) where T : FluentAvalonia.UI.Controls.ContentDialog => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.ContentDialog,FluentAvalonia.UI.Controls.ContentDialogButtonClickEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.SecondaryButtonClick += h);
    public static T OnCloseButtonClick<T>(this T control, Action<FluentAvalonia.UI.Controls.ContentDialog, FluentAvalonia.UI.Controls.ContentDialogButtonClickEventArgs> action) where T : FluentAvalonia.UI.Controls.ContentDialog => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.ContentDialog,FluentAvalonia.UI.Controls.ContentDialogButtonClickEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.CloseButtonClick += h);
}

