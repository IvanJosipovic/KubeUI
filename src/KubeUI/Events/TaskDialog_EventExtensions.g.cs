using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TaskDialog = FluentAvalonia.UI.Controls.TaskDialog;

namespace Avalonia.Markup.Declarative;
public static partial class TaskDialogEventsExtensions
{
    public static T OnOpening<T>(this T control, Action<FluentAvalonia.UI.Controls.TaskDialog, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.TaskDialog => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.TaskDialog,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Opening += h);
    public static T OnOpened<T>(this T control, Action<FluentAvalonia.UI.Controls.TaskDialog, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.TaskDialog => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.TaskDialog,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Opened += h);
    public static T OnClosing<T>(this T control, Action<FluentAvalonia.UI.Controls.TaskDialog, FluentAvalonia.UI.Controls.TaskDialogClosingEventArgs> action) where T : FluentAvalonia.UI.Controls.TaskDialog => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.TaskDialog,FluentAvalonia.UI.Controls.TaskDialogClosingEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Closing += h);
    public static T OnClosed<T>(this T control, Action<FluentAvalonia.UI.Controls.TaskDialog, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.TaskDialog => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.TaskDialog,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Closed += h);
}

