using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using DataGridColumn = Avalonia.Controls.DataGridColumn;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class DataGridColumnEventsExtensions
{
    public static T OnHeaderPointerPressed<T>(this T control, Action<System.Object, Avalonia.Input.PointerPressedEventArgs> action) where T : Avalonia.Controls.DataGridColumn => 
        control._setEvent((System.EventHandler<Avalonia.Input.PointerPressedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.HeaderPointerPressed += h);
    public static T OnHeaderPointerReleased<T>(this T control, Action<System.Object, Avalonia.Input.PointerReleasedEventArgs> action) where T : Avalonia.Controls.DataGridColumn => 
        control._setEvent((System.EventHandler<Avalonia.Input.PointerReleasedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.HeaderPointerReleased += h);
}

