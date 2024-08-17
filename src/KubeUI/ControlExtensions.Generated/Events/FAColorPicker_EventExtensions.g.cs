using Avalonia.Data;
using Avalonia.Data.Converters;
using FAColorPicker = FluentAvalonia.UI.Controls.FAColorPicker;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class FAColorPickerEventsExtensions
{
    public static T OnColorChanged<T>(this T control, Action<FluentAvalonia.UI.Controls.FAColorPicker, FluentAvalonia.UI.Controls.ColorChangedEventArgs> action) where T : FluentAvalonia.UI.Controls.FAColorPicker => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.FAColorPicker,FluentAvalonia.UI.Controls.ColorChangedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.ColorChanged += h);
}

