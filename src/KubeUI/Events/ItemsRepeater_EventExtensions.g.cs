using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using ItemsRepeater = FluentAvalonia.UI.Controls.ItemsRepeater;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class ItemsRepeaterEventsExtensions
{
    public static T OnElementPrepared<T>(this T control, Action<FluentAvalonia.UI.Controls.ItemsRepeater, FluentAvalonia.UI.Controls.ItemsRepeaterElementPreparedEventArgs> action) where T : FluentAvalonia.UI.Controls.ItemsRepeater => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.ItemsRepeater,FluentAvalonia.UI.Controls.ItemsRepeaterElementPreparedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.ElementPrepared += h);
    public static T OnElementClearing<T>(this T control, Action<FluentAvalonia.UI.Controls.ItemsRepeater, FluentAvalonia.UI.Controls.ItemsRepeaterElementClearingEventArgs> action) where T : FluentAvalonia.UI.Controls.ItemsRepeater => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.ItemsRepeater,FluentAvalonia.UI.Controls.ItemsRepeaterElementClearingEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.ElementClearing += h);
    public static T OnElementIndexChanged<T>(this T control, Action<FluentAvalonia.UI.Controls.ItemsRepeater, FluentAvalonia.UI.Controls.ItemsRepeaterElementIndexChangedEventArgs> action) where T : FluentAvalonia.UI.Controls.ItemsRepeater => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.ItemsRepeater,FluentAvalonia.UI.Controls.ItemsRepeaterElementIndexChangedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.ElementIndexChanged += h);
    public static T OnContainerContentChanging<T>(this T control, Action<FluentAvalonia.UI.Controls.ItemsRepeater, FluentAvalonia.UI.Controls.ContainerContentChangingEventArgs> action) where T : FluentAvalonia.UI.Controls.ItemsRepeater => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.ItemsRepeater,FluentAvalonia.UI.Controls.ContainerContentChangingEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.ContainerContentChanging += h);
}

