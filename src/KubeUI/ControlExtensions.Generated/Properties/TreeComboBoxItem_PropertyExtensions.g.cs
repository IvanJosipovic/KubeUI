#nullable enable
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
public static T IsSelected<T>(this T control, IBinding binding) where T : Ursa.Controls.TreeComboBoxItem
   => control._set(Ursa.Controls.TreeComboBoxItem.IsSelectedProperty, binding);
public static T IsSelected<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TreeComboBoxItem
   => control._set(Ursa.Controls.TreeComboBoxItem.IsSelectedProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsSelected<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TreeComboBoxItem
   => control._set(Ursa.Controls.TreeComboBoxItem.IsSelectedProperty, func, onChanged, expression);
public static T IsSelected<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TreeComboBoxItem
=> control._setEx(Ursa.Controls.TreeComboBoxItem.IsSelectedProperty, ps, () => control.IsSelected = value, bindingMode, converter, bindingSource);
public static T IsSelected<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TreeComboBoxItem
=> control._setEx(Ursa.Controls.TreeComboBoxItem.IsSelectedProperty, ps, () => control.IsSelected = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsExpanded<T>(this T control, IBinding binding) where T : Ursa.Controls.TreeComboBoxItem
   => control._set(Ursa.Controls.TreeComboBoxItem.IsExpandedProperty, binding);
public static T IsExpanded<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TreeComboBoxItem
   => control._set(Ursa.Controls.TreeComboBoxItem.IsExpandedProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsExpanded<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TreeComboBoxItem
   => control._set(Ursa.Controls.TreeComboBoxItem.IsExpandedProperty, func, onChanged, expression);
public static T IsExpanded<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TreeComboBoxItem
=> control._setEx(Ursa.Controls.TreeComboBoxItem.IsExpandedProperty, ps, () => control.IsExpanded = value, bindingMode, converter, bindingSource);
public static T IsExpanded<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TreeComboBoxItem
=> control._setEx(Ursa.Controls.TreeComboBoxItem.IsExpandedProperty, ps, () => control.IsExpanded = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

