using Avalonia.Data;
using Avalonia.Data.Converters;
using MultiComboBoxSelectedItemList = Ursa.Controls.MultiComboBoxSelectedItemList;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class MultiComboBoxSelectedItemListExtensions
{
public static Style<T> RemoveCommand<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : Ursa.Controls.MultiComboBoxSelectedItemList
=> style._addSetter(Ursa.Controls.MultiComboBoxSelectedItemList.RemoveCommandProperty, value);
public static Style<T> RemoveCommand<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.MultiComboBoxSelectedItemList
=> style._addSetter(Ursa.Controls.MultiComboBoxSelectedItemList.RemoveCommandProperty, binding);
}

