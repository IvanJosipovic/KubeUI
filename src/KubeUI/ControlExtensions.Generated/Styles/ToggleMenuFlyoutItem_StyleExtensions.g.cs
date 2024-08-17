using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using ToggleMenuFlyoutItem = FluentAvalonia.UI.Controls.ToggleMenuFlyoutItem;

namespace Avalonia.Markup.Declarative;
public static partial class ToggleMenuFlyoutItemExtensions
{
public static Style<T> IsChecked<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.ToggleMenuFlyoutItem
=> style._addSetter(FluentAvalonia.UI.Controls.ToggleMenuFlyoutItem.IsCheckedProperty, value);
public static Style<T> IsChecked<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ToggleMenuFlyoutItem
=> style._addSetter(FluentAvalonia.UI.Controls.ToggleMenuFlyoutItem.IsCheckedProperty, binding);
}

