using Avalonia.Data;
using Avalonia.Data.Converters;
using PinCode = Ursa.Controls.PinCode;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class PinCodeEventsExtensions
{
    public static T OnComplete<T>(this T control, Action<System.Object, Ursa.Controls.PinCodeCompleteEventArgs> action) where T : Ursa.Controls.PinCode => 
        control._setEvent((System.EventHandler<Ursa.Controls.PinCodeCompleteEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Complete += h);
}

