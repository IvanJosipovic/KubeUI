using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using SettingsExpander = FluentAvalonia.UI.Controls.SettingsExpander;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class SettingsExpanderEventsExtensions
{
    public static T OnClick<T>(this T control, Action<System.Object, Avalonia.Interactivity.RoutedEventArgs> action) where T : FluentAvalonia.UI.Controls.SettingsExpander => 
        control._setEvent((System.EventHandler<Avalonia.Interactivity.RoutedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Click += h);
}

