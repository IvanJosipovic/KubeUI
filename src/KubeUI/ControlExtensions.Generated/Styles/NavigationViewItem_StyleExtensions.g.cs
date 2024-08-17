using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using NavigationViewItem = FluentAvalonia.UI.Controls.NavigationViewItem;
using System;
using System.Collections;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class NavigationViewItemExtensions
{
public static Style<T> IconSource<T>(this Style<T> style, FluentAvalonia.UI.Controls.IconSource value) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationViewItem.IconSourceProperty, value);
public static Style<T> IconSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationViewItem.IconSourceProperty, binding);
public static Style<T> MenuItemsSource<T>(this Style<T> style, System.Collections.IEnumerable value) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationViewItem.MenuItemsSourceProperty, value);
public static Style<T> MenuItemsSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationViewItem.MenuItemsSourceProperty, binding);
public static Style<T> InfoBadge<T>(this Style<T> style, FluentAvalonia.UI.Controls.InfoBadge value) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationViewItem.InfoBadgeProperty, value);
public static Style<T> InfoBadge<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationViewItem.InfoBadgeProperty, binding);
}

