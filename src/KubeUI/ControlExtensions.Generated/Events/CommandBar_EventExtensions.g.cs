using Avalonia.Data;
using Avalonia.Data.Converters;
using CommandBar = FluentAvalonia.UI.Controls.CommandBar;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class CommandBarEventsExtensions
{
    public static T OnOpened<T>(this T control, Action<FluentAvalonia.UI.Controls.CommandBar, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.CommandBar => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.CommandBar,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Opened += h);
    public static T OnOpening<T>(this T control, Action<FluentAvalonia.UI.Controls.CommandBar, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.CommandBar => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.CommandBar,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Opening += h);
    public static T OnClosed<T>(this T control, Action<FluentAvalonia.UI.Controls.CommandBar, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.CommandBar => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.CommandBar,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Closed += h);
    public static T OnClosing<T>(this T control, Action<FluentAvalonia.UI.Controls.CommandBar, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.CommandBar => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.CommandBar,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Closing += h);
}

