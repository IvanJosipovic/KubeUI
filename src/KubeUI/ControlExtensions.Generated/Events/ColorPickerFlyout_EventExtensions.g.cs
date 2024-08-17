using Avalonia.Data;
using Avalonia.Data.Converters;
using ColorPickerFlyout = FluentAvalonia.UI.Controls.ColorPickerFlyout;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class ColorPickerFlyoutEventsExtensions
{
    public static FluentAvalonia.UI.Controls.ColorPickerFlyout OnConfirmed(this FluentAvalonia.UI.Controls.ColorPickerFlyout control, Action<FluentAvalonia.UI.Controls.ColorPickerFlyout, System.EventArgs> action) => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.ColorPickerFlyout,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Confirmed += h);
    public static FluentAvalonia.UI.Controls.ColorPickerFlyout OnDismissed(this FluentAvalonia.UI.Controls.ColorPickerFlyout control, Action<FluentAvalonia.UI.Controls.ColorPickerFlyout, System.EventArgs> action) => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.ColorPickerFlyout,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Dismissed += h);
}

