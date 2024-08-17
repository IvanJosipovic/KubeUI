using Avalonia.Data;
using Avalonia.Data.Converters;
using RangeSlider = Ursa.Controls.RangeSlider;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class RangeSliderEventsExtensions
{
    public static T OnValueChanged<T>(this T control, Action<System.Object, Ursa.Controls.RangeValueChangedEventArgs> action) where T : Ursa.Controls.RangeSlider => 
        control._setEvent((System.EventHandler<Ursa.Controls.RangeValueChangedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.ValueChanged += h);
}

