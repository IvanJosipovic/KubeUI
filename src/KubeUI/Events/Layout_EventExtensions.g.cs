using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using Layout = FluentAvalonia.UI.Controls.Layout;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class LayoutEventsExtensions
{
    public static T OnMeasureInvalidated<T>(this T control, Action<FluentAvalonia.UI.Controls.Layout, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.Layout => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.Layout,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.MeasureInvalidated += h);
    public static T OnArrangeInvalidated<T>(this T control, Action<FluentAvalonia.UI.Controls.Layout, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.Layout => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.Layout,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.ArrangeInvalidated += h);
}

