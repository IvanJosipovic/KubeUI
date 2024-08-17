using Avalonia.Data;
using Avalonia.Data.Converters;
using DialogControlBase = Ursa.Controls.DialogControlBase;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class DialogControlBaseEventsExtensions
{
    public static T OnLayerChanged<T>(this T control, Action<System.Object, Ursa.Controls.DialogLayerChangeEventArgs> action) where T : Ursa.Controls.DialogControlBase => 
        control._setEvent((System.EventHandler<Ursa.Controls.DialogLayerChangeEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.LayerChanged += h);
}

