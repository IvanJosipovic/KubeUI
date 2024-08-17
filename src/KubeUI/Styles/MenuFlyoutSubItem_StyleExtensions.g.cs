using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Styling;
using FluentAvalonia.UI.Controls;
using MenuFlyoutSubItem = FluentAvalonia.UI.Controls.MenuFlyoutSubItem;
using System;
using System.Collections;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class MenuFlyoutSubItemExtensions
{
public static Style<T> Text<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.TextProperty, value);
public static Style<T> Text<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.TextProperty, binding);
public static Style<T> IconSource<T>(this Style<T> style, FluentAvalonia.UI.Controls.IconSource value) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.IconSourceProperty, value);
public static Style<T> IconSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.IconSourceProperty, binding);
public static Style<T> ItemsSource<T>(this Style<T> style, System.Collections.IEnumerable value) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemsSourceProperty, value);
public static Style<T> ItemsSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemsSourceProperty, binding);
public static Style<T> ItemTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemTemplateProperty, value);
public static Style<T> ItemTemplate<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemTemplateProperty, binding);
public static Style<T> ItemContainerTheme<T>(this Style<T> style, Avalonia.Styling.ControlTheme value) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemContainerThemeProperty, value);
public static Style<T> ItemContainerTheme<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemContainerThemeProperty, binding);
}

