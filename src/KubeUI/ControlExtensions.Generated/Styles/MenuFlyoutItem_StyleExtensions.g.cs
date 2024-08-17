using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using FluentAvalonia.UI.Controls;
using MenuFlyoutItem = FluentAvalonia.UI.Controls.MenuFlyoutItem;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Avalonia.Markup.Declarative;
public static partial class MenuFlyoutItemExtensions
{
public static Style<T> Text<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutItem.TextProperty, value);
public static Style<T> Text<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutItem.TextProperty, binding);
public static Style<T> IconSource<T>(this Style<T> style, FluentAvalonia.UI.Controls.IconSource value) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutItem.IconSourceProperty, value);
public static Style<T> IconSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutItem.IconSourceProperty, binding);
public static Style<T> Command<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutItem.CommandProperty, value);
public static Style<T> Command<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutItem.CommandProperty, binding);
public static Style<T> CommandParameter<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutItem.CommandParameterProperty, value);
public static Style<T> CommandParameter<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutItem.CommandParameterProperty, binding);
public static Style<T> HotKey<T>(this Style<T> style, Avalonia.Input.KeyGesture value) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutItem.HotKeyProperty, value);
public static Style<T> HotKey<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutItem.HotKeyProperty, binding);
public static Style<T> InputGesture<T>(this Style<T> style, Avalonia.Input.KeyGesture value) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutItem.InputGestureProperty, value);
public static Style<T> InputGesture<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutItem.InputGestureProperty, binding);
}

