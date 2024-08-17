using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TreeComboBoxItem = Ursa.Controls.TreeComboBoxItem;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class TreeComboBoxItemExtensions
{
public static Style<T> IsSelected<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.TreeComboBoxItem
=> style._addSetter(Ursa.Controls.TreeComboBoxItem.IsSelectedProperty, value);
public static Style<T> IsSelected<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TreeComboBoxItem
=> style._addSetter(Ursa.Controls.TreeComboBoxItem.IsSelectedProperty, binding);
public static Style<T> IsExpanded<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.TreeComboBoxItem
=> style._addSetter(Ursa.Controls.TreeComboBoxItem.IsExpandedProperty, value);
public static Style<T> IsExpanded<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TreeComboBoxItem
=> style._addSetter(Ursa.Controls.TreeComboBoxItem.IsExpandedProperty, binding);
}

