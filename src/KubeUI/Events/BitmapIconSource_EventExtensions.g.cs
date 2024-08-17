using Avalonia.Data;
using Avalonia.Data.Converters;
using BitmapIconSource = FluentAvalonia.UI.Controls.BitmapIconSource;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class BitmapIconSourceEventsExtensions
{
    public static T OnOnBitmapChanged<T>(this T control, Action<System.Object, System.Object> action) where T : FluentAvalonia.UI.Controls.BitmapIconSource => 
        control._setEvent((System.EventHandler<System.Object>) ((arg0, arg1) => action(arg0, arg1)), h => control.OnBitmapChanged += h);
}

