using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TaskDialogButton = FluentAvalonia.UI.Controls.TaskDialogButton;

namespace Avalonia.Markup.Declarative;
public static partial class TaskDialogButtonEventsExtensions
{
    public static T OnClick<T>(this T control, Action<FluentAvalonia.UI.Controls.TaskDialogButton, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.TaskDialogButton => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.TaskDialogButton,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Click += h);
}

