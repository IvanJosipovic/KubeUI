using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TimePickerPresenter = Ursa.Controls.TimePickerPresenter;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class TimePickerPresenterEventsExtensions
{
    public static T OnSelectedTimeChanged<T>(this T control, Action<System.Object, Avalonia.Controls.TimePickerSelectedValueChangedEventArgs> action) where T : Ursa.Controls.TimePickerPresenter => 
        control._setEvent((System.EventHandler<Avalonia.Controls.TimePickerSelectedValueChangedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.SelectedTimeChanged += h);
}

