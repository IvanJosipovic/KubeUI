using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TeachingTip = FluentAvalonia.UI.Controls.TeachingTip;

namespace Avalonia.Markup.Declarative;
public static partial class TeachingTipEventsExtensions
{
    public static T OnActionButtonClick<T>(this T control, Action<FluentAvalonia.UI.Controls.TeachingTip, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.TeachingTip => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.TeachingTip,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.ActionButtonClick += h);
    public static T OnCloseButtonClick<T>(this T control, Action<FluentAvalonia.UI.Controls.TeachingTip, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.TeachingTip => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.TeachingTip,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.CloseButtonClick += h);
    public static T OnClosing<T>(this T control, Action<FluentAvalonia.UI.Controls.TeachingTip, FluentAvalonia.UI.Controls.TeachingTipClosingEventArgs> action) where T : FluentAvalonia.UI.Controls.TeachingTip => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.TeachingTip,FluentAvalonia.UI.Controls.TeachingTipClosingEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Closing += h);
    public static T OnClosed<T>(this T control, Action<FluentAvalonia.UI.Controls.TeachingTip, FluentAvalonia.UI.Controls.TeachingTipClosedEventArgs> action) where T : FluentAvalonia.UI.Controls.TeachingTip => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.TeachingTip,FluentAvalonia.UI.Controls.TeachingTipClosedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Closed += h);
}

