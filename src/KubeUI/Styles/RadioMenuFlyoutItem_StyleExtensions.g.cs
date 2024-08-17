using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using RadioMenuFlyoutItem = FluentAvalonia.UI.Controls.RadioMenuFlyoutItem;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class RadioMenuFlyoutItemExtensions
{
public static Style<T> GroupName<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.RadioMenuFlyoutItem
=> style._addSetter(FluentAvalonia.UI.Controls.RadioMenuFlyoutItem.GroupNameProperty, value);
public static Style<T> GroupName<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.RadioMenuFlyoutItem
=> style._addSetter(FluentAvalonia.UI.Controls.RadioMenuFlyoutItem.GroupNameProperty, binding);
public static Style<T> IsChecked<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.RadioMenuFlyoutItem
=> style._addSetter(FluentAvalonia.UI.Controls.RadioMenuFlyoutItem.IsCheckedProperty, value);
public static Style<T> IsChecked<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.RadioMenuFlyoutItem
=> style._addSetter(FluentAvalonia.UI.Controls.RadioMenuFlyoutItem.IsCheckedProperty, binding);
}

