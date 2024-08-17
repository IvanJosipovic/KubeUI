using Avalonia.Data;
using Avalonia.Data.Converters;
using RangeTrack = Ursa.Controls.RangeTrack;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class RangeTrackEventsExtensions
{
    public static T OnValueChanged<T>(this T control, Action<System.Object, Ursa.Controls.RangeValueChangedEventArgs> action) where T : Ursa.Controls.RangeTrack => 
        control._setEvent((System.EventHandler<Ursa.Controls.RangeValueChangedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.ValueChanged += h);
}

