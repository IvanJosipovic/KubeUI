using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using DataGridColumnHeader = Avalonia.Controls.DataGridColumnHeader;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class DataGridColumnHeaderEventsExtensions
{
    public static T OnLeftClick<T>(this T control, Action<System.Object, Avalonia.Input.KeyModifiers> action) where T : Avalonia.Controls.DataGridColumnHeader => 
        control._setEvent((System.EventHandler<Avalonia.Input.KeyModifiers>) ((arg0, arg1) => action(arg0, arg1)), h => control.LeftClick += h);
}

