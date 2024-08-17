#nullable enable
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using MultiComboBox = Ursa.Controls.MultiComboBox;
using System;
using System.Collections;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class MultiComboBoxExtensions
{
public static T IsDropDownOpen<T>(this T control, IBinding binding) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.IsDropDownOpenProperty, binding);
public static T IsDropDownOpen<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.IsDropDownOpenProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsDropDownOpen<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.IsDropDownOpenProperty, func, onChanged, expression);
public static T IsDropDownOpen<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MultiComboBox
=> control._setEx(Ursa.Controls.MultiComboBox.IsDropDownOpenProperty, ps, () => control.IsDropDownOpen = value, bindingMode, converter, bindingSource);
public static T IsDropDownOpen<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MultiComboBox
=> control._setEx(Ursa.Controls.MultiComboBox.IsDropDownOpenProperty, ps, () => control.IsDropDownOpen = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T MaxDropdownHeight<T>(this T control, IBinding binding) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.MaxDropdownHeightProperty, binding);
public static T MaxDropdownHeight<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.MaxDropdownHeightProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T MaxDropdownHeight<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.MaxDropdownHeightProperty, func, onChanged, expression);
public static T MaxDropdownHeight<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MultiComboBox
=> control._setEx(Ursa.Controls.MultiComboBox.MaxDropdownHeightProperty, ps, () => control.MaxDropdownHeight = value, bindingMode, converter, bindingSource);
public static T MaxDropdownHeight<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MultiComboBox
=> control._setEx(Ursa.Controls.MultiComboBox.MaxDropdownHeightProperty, ps, () => control.MaxDropdownHeight = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T MaxSelectionBoxHeight<T>(this T control, IBinding binding) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.MaxSelectionBoxHeightProperty, binding);
public static T MaxSelectionBoxHeight<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.MaxSelectionBoxHeightProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T MaxSelectionBoxHeight<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.MaxSelectionBoxHeightProperty, func, onChanged, expression);
public static T MaxSelectionBoxHeight<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MultiComboBox
=> control._setEx(Ursa.Controls.MultiComboBox.MaxSelectionBoxHeightProperty, ps, () => control.MaxSelectionBoxHeight = value, bindingMode, converter, bindingSource);
public static T MaxSelectionBoxHeight<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MultiComboBox
=> control._setEx(Ursa.Controls.MultiComboBox.MaxSelectionBoxHeightProperty, ps, () => control.MaxSelectionBoxHeight = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SelectedItems<T>(this T control, IBinding binding) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.SelectedItemsProperty, binding);
public static T SelectedItems<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.SelectedItemsProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SelectedItems<T>(this T control, Func<System.Collections.IList> func, Action<System.Collections.IList>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.SelectedItemsProperty, func, onChanged, expression);
public static T SelectedItems<T>(this T control, System.Collections.IList value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MultiComboBox
=> control._setEx(Ursa.Controls.MultiComboBox.SelectedItemsProperty, ps, () => control.SelectedItems = value, bindingMode, converter, bindingSource);
public static T SelectedItems<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.IList> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MultiComboBox
=> control._setEx(Ursa.Controls.MultiComboBox.SelectedItemsProperty, ps, () => control.SelectedItems = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T InnerLeftContent<T>(this T control, IBinding binding) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.InnerLeftContentProperty, binding);
public static T InnerLeftContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.InnerLeftContentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T InnerLeftContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.InnerLeftContentProperty, func, onChanged, expression);
public static T InnerLeftContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MultiComboBox
=> control._setEx(Ursa.Controls.MultiComboBox.InnerLeftContentProperty, ps, () => control.InnerLeftContent = value, bindingMode, converter, bindingSource);
public static T InnerLeftContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MultiComboBox
=> control._setEx(Ursa.Controls.MultiComboBox.InnerLeftContentProperty, ps, () => control.InnerLeftContent = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T InnerRightContent<T>(this T control, IBinding binding) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.InnerRightContentProperty, binding);
public static T InnerRightContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.InnerRightContentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T InnerRightContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.InnerRightContentProperty, func, onChanged, expression);
public static T InnerRightContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MultiComboBox
=> control._setEx(Ursa.Controls.MultiComboBox.InnerRightContentProperty, ps, () => control.InnerRightContent = value, bindingMode, converter, bindingSource);
public static T InnerRightContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MultiComboBox
=> control._setEx(Ursa.Controls.MultiComboBox.InnerRightContentProperty, ps, () => control.InnerRightContent = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SelectedItemTemplate<T>(this T control, IBinding binding) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.SelectedItemTemplateProperty, binding);
public static T SelectedItemTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.SelectedItemTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SelectedItemTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.SelectedItemTemplateProperty, func, onChanged, expression);
public static T SelectedItemTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MultiComboBox
=> control._setEx(Ursa.Controls.MultiComboBox.SelectedItemTemplateProperty, ps, () => control.SelectedItemTemplate = value, bindingMode, converter, bindingSource);
public static T SelectedItemTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MultiComboBox
=> control._setEx(Ursa.Controls.MultiComboBox.SelectedItemTemplateProperty, ps, () => control.SelectedItemTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

