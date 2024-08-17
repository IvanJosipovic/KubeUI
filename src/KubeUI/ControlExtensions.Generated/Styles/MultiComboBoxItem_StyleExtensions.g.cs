using Avalonia.Data;
using Avalonia.Data.Converters;
using MultiComboBoxItem = Ursa.Controls.MultiComboBoxItem;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class MultiComboBoxItemExtensions
{
public static Style<T> IsSelected<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.MultiComboBoxItem
=> style._addSetter(Ursa.Controls.MultiComboBoxItem.IsSelectedProperty, value);
public static Style<T> IsSelected<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.MultiComboBoxItem
=> style._addSetter(Ursa.Controls.MultiComboBoxItem.IsSelectedProperty, binding);
}

