using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using NumberBox = FluentAvalonia.UI.Controls.NumberBox;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class NumberBoxEventsExtensions
{
    public static T OnValueChanged<T>(this T control, Action<FluentAvalonia.UI.Controls.NumberBox, FluentAvalonia.UI.Controls.NumberBoxValueChangedEventArgs> action) where T : FluentAvalonia.UI.Controls.NumberBox => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.NumberBox,FluentAvalonia.UI.Controls.NumberBoxValueChangedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.ValueChanged += h);
}

