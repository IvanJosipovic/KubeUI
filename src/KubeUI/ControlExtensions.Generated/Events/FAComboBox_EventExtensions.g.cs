using Avalonia.Data;
using Avalonia.Data.Converters;
using FAComboBox = FluentAvalonia.UI.Controls.FAComboBox;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class FAComboBoxEventsExtensions
{
    public static T OnDropDownOpened<T>(this T control, Action<System.Object, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.FAComboBox => 
        control._setEvent((System.EventHandler<System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.DropDownOpened += h);
    public static T OnDropDownClosed<T>(this T control, Action<System.Object, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.FAComboBox => 
        control._setEvent((System.EventHandler<System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.DropDownClosed += h);
    public static T OnTextSubmitted<T>(this T control, Action<FluentAvalonia.UI.Controls.FAComboBox, FluentAvalonia.UI.Controls.FAComboBoxTextSubmittedEventArgs> action) where T : FluentAvalonia.UI.Controls.FAComboBox => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.FAComboBox,FluentAvalonia.UI.Controls.FAComboBoxTextSubmittedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.TextSubmitted += h);
}

