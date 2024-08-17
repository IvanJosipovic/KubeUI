#nullable enable
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
public static T RemoveCommand<T>(this T control, IBinding binding) where T : Ursa.Controls.MultiComboBoxSelectedItemList
   => control._set(Ursa.Controls.MultiComboBoxSelectedItemList.RemoveCommandProperty, binding);
public static T RemoveCommand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.MultiComboBoxSelectedItemList
   => control._set(Ursa.Controls.MultiComboBoxSelectedItemList.RemoveCommandProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T RemoveCommand<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.MultiComboBoxSelectedItemList
   => control._set(Ursa.Controls.MultiComboBoxSelectedItemList.RemoveCommandProperty, func, onChanged, expression);
public static T RemoveCommand<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MultiComboBoxSelectedItemList
=> control._setEx(Ursa.Controls.MultiComboBoxSelectedItemList.RemoveCommandProperty, ps, () => control.RemoveCommand = value, bindingMode, converter, bindingSource);
public static T RemoveCommand<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MultiComboBoxSelectedItemList
=> control._setEx(Ursa.Controls.MultiComboBoxSelectedItemList.RemoveCommandProperty, ps, () => control.RemoveCommand = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

