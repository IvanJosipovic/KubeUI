using Avalonia.Data;
using Avalonia.Data.Converters;
using OverlayFeedbackElement = Ursa.Controls.OverlayShared.OverlayFeedbackElement;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls.OverlayShared;
using Ursa.EventArgs;

namespace Avalonia.Markup.Declarative;
public static partial class OverlayFeedbackElementEventsExtensions
{
    public static T OnClosed<T>(this T control, Action<System.Object, Ursa.EventArgs.ResultEventArgs> action) where T : Ursa.Controls.OverlayShared.OverlayFeedbackElement => 
        control._setEvent((System.EventHandler<Ursa.EventArgs.ResultEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Closed += h);
}

