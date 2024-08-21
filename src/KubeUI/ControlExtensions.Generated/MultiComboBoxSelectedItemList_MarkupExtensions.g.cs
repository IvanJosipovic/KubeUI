#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class MultiComboBoxSelectedItemList_MarkupExtensions
{
//================= Properties ======================//
 // RemoveCommandProperty

/*BindFromExpressionSetterGenerator*/
public static T RemoveCommand<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.MultiComboBoxSelectedItemList
   => control._set(Ursa.Controls.MultiComboBoxSelectedItemList.RemoveCommandProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T RemoveCommand<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MultiComboBoxSelectedItemList
=> control._setEx(Ursa.Controls.MultiComboBoxSelectedItemList.RemoveCommandProperty, ps, () => control.RemoveCommand = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T RemoveCommand<T>(this T control, IBinding binding) where T : Ursa.Controls.MultiComboBoxSelectedItemList
   => control._set(Ursa.Controls.MultiComboBoxSelectedItemList.RemoveCommandProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T RemoveCommand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.MultiComboBoxSelectedItemList
   => control._set(Ursa.Controls.MultiComboBoxSelectedItemList.RemoveCommandProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T RemoveCommand<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MultiComboBoxSelectedItemList
=> control._setEx(Ursa.Controls.MultiComboBoxSelectedItemList.RemoveCommandProperty, ps, () => control.RemoveCommand = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // RemoveCommandProperty

/*ValueStyleSetterGenerator*/
public static Style<T> RemoveCommand<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : Ursa.Controls.MultiComboBoxSelectedItemList
=> style._addSetter(Ursa.Controls.MultiComboBoxSelectedItemList.RemoveCommandProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> RemoveCommand<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.MultiComboBoxSelectedItemList
=> style._addSetter(Ursa.Controls.MultiComboBoxSelectedItemList.RemoveCommandProperty, binding);



}