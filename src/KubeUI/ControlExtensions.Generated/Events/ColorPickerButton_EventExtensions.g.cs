using Avalonia.Data;
using Avalonia.Data.Converters;
using ColorPickerButton = FluentAvalonia.UI.Controls.ColorPickerButton;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class ColorPickerButtonEventsExtensions
{
    public static T OnFlyoutConfirmed<T>(this T control, Action<FluentAvalonia.UI.Controls.ColorPickerButton, FluentAvalonia.UI.Controls.ColorButtonColorChangedEventArgs> action) where T : FluentAvalonia.UI.Controls.ColorPickerButton => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.ColorPickerButton,FluentAvalonia.UI.Controls.ColorButtonColorChangedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.FlyoutConfirmed += h);
    public static T OnFlyoutDismissed<T>(this T control, Action<FluentAvalonia.UI.Controls.ColorPickerButton, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.ColorPickerButton => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.ColorPickerButton,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.FlyoutDismissed += h);
    public static T OnFlyoutOpened<T>(this T control, Action<FluentAvalonia.UI.Controls.ColorPickerButton, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.ColorPickerButton => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.ColorPickerButton,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.FlyoutOpened += h);
    public static T OnFlyoutClosed<T>(this T control, Action<FluentAvalonia.UI.Controls.ColorPickerButton, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.ColorPickerButton => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.ColorPickerButton,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.FlyoutClosed += h);
    public static T OnColorChanged<T>(this T control, Action<FluentAvalonia.UI.Controls.ColorPickerButton, FluentAvalonia.UI.Controls.ColorButtonColorChangedEventArgs> action) where T : FluentAvalonia.UI.Controls.ColorPickerButton => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.ColorPickerButton,FluentAvalonia.UI.Controls.ColorButtonColorChangedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.ColorChanged += h);
}

