using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using InfoBar = FluentAvalonia.UI.Controls.InfoBar;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class InfoBarEventsExtensions
{
    public static T OnCloseButtonClick<T>(this T control, Action<FluentAvalonia.UI.Controls.InfoBar, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.InfoBar => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.InfoBar,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.CloseButtonClick += h);
    public static T OnClosing<T>(this T control, Action<FluentAvalonia.UI.Controls.InfoBar, FluentAvalonia.UI.Controls.InfoBarClosingEventArgs> action) where T : FluentAvalonia.UI.Controls.InfoBar => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.InfoBar,FluentAvalonia.UI.Controls.InfoBarClosingEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Closing += h);
    public static T OnClosed<T>(this T control, Action<FluentAvalonia.UI.Controls.InfoBar, FluentAvalonia.UI.Controls.InfoBarClosedEventArgs> action) where T : FluentAvalonia.UI.Controls.InfoBar => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.InfoBar,FluentAvalonia.UI.Controls.InfoBarClosedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Closed += h);
}

