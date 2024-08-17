using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Input;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using XamlUICommand = FluentAvalonia.UI.Input.XamlUICommand;

namespace Avalonia.Markup.Declarative;
public static partial class XamlUICommandEventsExtensions
{
    public static T OnCanExecuteChanged<T>(this T control, Action<System.Object, System.EventArgs> action) where T : FluentAvalonia.UI.Input.XamlUICommand => 
        control._setEvent((System.EventHandler) ((arg0, arg1) => action(arg0, arg1)), h => control.CanExecuteChanged += h);
    public static T OnCanExecuteRequested<T>(this T control, Action<FluentAvalonia.UI.Input.XamlUICommand, FluentAvalonia.UI.Input.CanExecuteRequestedEventArgs> action) where T : FluentAvalonia.UI.Input.XamlUICommand => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Input.XamlUICommand,FluentAvalonia.UI.Input.CanExecuteRequestedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.CanExecuteRequested += h);
    public static T OnExecuteRequested<T>(this T control, Action<FluentAvalonia.UI.Input.XamlUICommand, FluentAvalonia.UI.Input.ExecuteRequestedEventArgs> action) where T : FluentAvalonia.UI.Input.XamlUICommand => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Input.XamlUICommand,FluentAvalonia.UI.Input.ExecuteRequestedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.ExecuteRequested += h);
}

