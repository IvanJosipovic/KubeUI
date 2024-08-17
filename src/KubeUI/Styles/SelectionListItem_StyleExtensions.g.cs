using Avalonia.Data;
using Avalonia.Data.Converters;
using SelectionListItem = Ursa.Controls.SelectionListItem;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class SelectionListItemExtensions
{
public static Style<T> IsSelected<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.SelectionListItem
=> style._addSetter(Ursa.Controls.SelectionListItem.IsSelectedProperty, value);
public static Style<T> IsSelected<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.SelectionListItem
=> style._addSetter(Ursa.Controls.SelectionListItem.IsSelectedProperty, binding);
}

