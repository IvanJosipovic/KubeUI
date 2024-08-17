using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using NumericUpDown = Ursa.Controls.NumericUpDown;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class NumericUpDownEventsExtensions
{
    public static T OnSpinned<T>(this T control, Action<System.Object, Avalonia.Controls.SpinEventArgs> action) where T : Ursa.Controls.NumericUpDown => 
        control._setEvent((System.EventHandler<Avalonia.Controls.SpinEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Spinned += h);
}

