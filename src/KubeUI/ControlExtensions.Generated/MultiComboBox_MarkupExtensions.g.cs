#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class MultiComboBox_MarkupExtensions
{
//================= Properties ======================//
 // IsDropDownOpenProperty

/*BindFromExpressionSetterGenerator*/
public static T IsDropDownOpen<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.IsDropDownOpenProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsDropDownOpen<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MultiComboBox
=> control._setEx(Ursa.Controls.MultiComboBox.IsDropDownOpenProperty, ps, () => control.IsDropDownOpen = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsDropDownOpen<T>(this T control, IBinding binding) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.IsDropDownOpenProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsDropDownOpen<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.IsDropDownOpenProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsDropDownOpen<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MultiComboBox
=> control._setEx(Ursa.Controls.MultiComboBox.IsDropDownOpenProperty, ps, () => control.IsDropDownOpen = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // MaxDropdownHeightProperty

/*BindFromExpressionSetterGenerator*/
public static T MaxDropdownHeight<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.MaxDropdownHeightProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T MaxDropdownHeight<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MultiComboBox
=> control._setEx(Ursa.Controls.MultiComboBox.MaxDropdownHeightProperty, ps, () => control.MaxDropdownHeight = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T MaxDropdownHeight<T>(this T control, IBinding binding) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.MaxDropdownHeightProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T MaxDropdownHeight<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.MaxDropdownHeightProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T MaxDropdownHeight<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MultiComboBox
=> control._setEx(Ursa.Controls.MultiComboBox.MaxDropdownHeightProperty, ps, () => control.MaxDropdownHeight = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // MaxSelectionBoxHeightProperty

/*BindFromExpressionSetterGenerator*/
public static T MaxSelectionBoxHeight<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.MaxSelectionBoxHeightProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T MaxSelectionBoxHeight<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MultiComboBox
=> control._setEx(Ursa.Controls.MultiComboBox.MaxSelectionBoxHeightProperty, ps, () => control.MaxSelectionBoxHeight = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T MaxSelectionBoxHeight<T>(this T control, IBinding binding) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.MaxSelectionBoxHeightProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T MaxSelectionBoxHeight<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.MaxSelectionBoxHeightProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T MaxSelectionBoxHeight<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MultiComboBox
=> control._setEx(Ursa.Controls.MultiComboBox.MaxSelectionBoxHeightProperty, ps, () => control.MaxSelectionBoxHeight = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SelectedItemsProperty

/*BindFromExpressionSetterGenerator*/
public static T SelectedItems<T>(this T control, Func<System.Collections.IList> func, Action<System.Collections.IList>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.SelectedItemsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SelectedItems<T>(this T control, System.Collections.IList value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MultiComboBox
=> control._setEx(Ursa.Controls.MultiComboBox.SelectedItemsProperty, ps, () => control.SelectedItems = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SelectedItems<T>(this T control, IBinding binding) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.SelectedItemsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SelectedItems<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.SelectedItemsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SelectedItems<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.IList> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MultiComboBox
=> control._setEx(Ursa.Controls.MultiComboBox.SelectedItemsProperty, ps, () => control.SelectedItems = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // InnerLeftContentProperty

/*BindFromExpressionSetterGenerator*/
public static T InnerLeftContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.InnerLeftContentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T InnerLeftContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MultiComboBox
=> control._setEx(Ursa.Controls.MultiComboBox.InnerLeftContentProperty, ps, () => control.InnerLeftContent = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T InnerLeftContent<T>(this T control, IBinding binding) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.InnerLeftContentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T InnerLeftContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.InnerLeftContentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T InnerLeftContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MultiComboBox
=> control._setEx(Ursa.Controls.MultiComboBox.InnerLeftContentProperty, ps, () => control.InnerLeftContent = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // InnerRightContentProperty

/*BindFromExpressionSetterGenerator*/
public static T InnerRightContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.InnerRightContentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T InnerRightContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MultiComboBox
=> control._setEx(Ursa.Controls.MultiComboBox.InnerRightContentProperty, ps, () => control.InnerRightContent = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T InnerRightContent<T>(this T control, IBinding binding) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.InnerRightContentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T InnerRightContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.InnerRightContentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T InnerRightContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MultiComboBox
=> control._setEx(Ursa.Controls.MultiComboBox.InnerRightContentProperty, ps, () => control.InnerRightContent = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SelectedItemTemplateProperty

/*BindFromExpressionSetterGenerator*/
public static T SelectedItemTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.SelectedItemTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SelectedItemTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MultiComboBox
=> control._setEx(Ursa.Controls.MultiComboBox.SelectedItemTemplateProperty, ps, () => control.SelectedItemTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SelectedItemTemplate<T>(this T control, IBinding binding) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.SelectedItemTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SelectedItemTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.MultiComboBox
   => control._set(Ursa.Controls.MultiComboBox.SelectedItemTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SelectedItemTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MultiComboBox
=> control._setEx(Ursa.Controls.MultiComboBox.SelectedItemTemplateProperty, ps, () => control.SelectedItemTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // IsDropDownOpenProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsDropDownOpen<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.MultiComboBox
=> style._addSetter(Ursa.Controls.MultiComboBox.IsDropDownOpenProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsDropDownOpen<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.MultiComboBox
=> style._addSetter(Ursa.Controls.MultiComboBox.IsDropDownOpenProperty, binding);


 // MaxDropdownHeightProperty

/*ValueStyleSetterGenerator*/
public static Style<T> MaxDropdownHeight<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.MultiComboBox
=> style._addSetter(Ursa.Controls.MultiComboBox.MaxDropdownHeightProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> MaxDropdownHeight<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.MultiComboBox
=> style._addSetter(Ursa.Controls.MultiComboBox.MaxDropdownHeightProperty, binding);


 // MaxSelectionBoxHeightProperty

/*ValueStyleSetterGenerator*/
public static Style<T> MaxSelectionBoxHeight<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.MultiComboBox
=> style._addSetter(Ursa.Controls.MultiComboBox.MaxSelectionBoxHeightProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> MaxSelectionBoxHeight<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.MultiComboBox
=> style._addSetter(Ursa.Controls.MultiComboBox.MaxSelectionBoxHeightProperty, binding);


 // SelectedItemsProperty

/*ValueStyleSetterGenerator*/
public static Style<T> SelectedItems<T>(this Style<T> style, System.Collections.IList value) where T : Ursa.Controls.MultiComboBox
=> style._addSetter(Ursa.Controls.MultiComboBox.SelectedItemsProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SelectedItems<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.MultiComboBox
=> style._addSetter(Ursa.Controls.MultiComboBox.SelectedItemsProperty, binding);


 // InnerLeftContentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> InnerLeftContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.MultiComboBox
=> style._addSetter(Ursa.Controls.MultiComboBox.InnerLeftContentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> InnerLeftContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.MultiComboBox
=> style._addSetter(Ursa.Controls.MultiComboBox.InnerLeftContentProperty, binding);


 // InnerRightContentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> InnerRightContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.MultiComboBox
=> style._addSetter(Ursa.Controls.MultiComboBox.InnerRightContentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> InnerRightContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.MultiComboBox
=> style._addSetter(Ursa.Controls.MultiComboBox.InnerRightContentProperty, binding);


 // SelectedItemTemplateProperty

/*ValueStyleSetterGenerator*/
public static Style<T> SelectedItemTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : Ursa.Controls.MultiComboBox
=> style._addSetter(Ursa.Controls.MultiComboBox.SelectedItemTemplateProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SelectedItemTemplate<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.MultiComboBox
=> style._addSetter(Ursa.Controls.MultiComboBox.SelectedItemTemplateProperty, binding);



}
