using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using MenuFlyoutItem = FluentAvalonia.UI.Controls.MenuFlyoutItem;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class MenuFlyoutItemEventsExtensions
{
    public static T OnClick<T>(this T control, Action<System.Object, Avalonia.Interactivity.RoutedEventArgs> action) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem => 
        control._setEvent((System.EventHandler<Avalonia.Interactivity.RoutedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Click += h);
}

