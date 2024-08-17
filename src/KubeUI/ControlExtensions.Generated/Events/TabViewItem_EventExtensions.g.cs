using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TabViewItem = FluentAvalonia.UI.Controls.TabViewItem;

namespace Avalonia.Markup.Declarative;
public static partial class TabViewItemEventsExtensions
{
    public static T OnCloseRequested<T>(this T control, Action<FluentAvalonia.UI.Controls.TabViewItem, FluentAvalonia.UI.Controls.TabViewTabCloseRequestedEventArgs> action) where T : FluentAvalonia.UI.Controls.TabViewItem => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.TabViewItem,FluentAvalonia.UI.Controls.TabViewTabCloseRequestedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.CloseRequested += h);
}

