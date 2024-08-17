#nullable enable
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TreeComboBox = Ursa.Controls.TreeComboBox;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class TreeComboBoxExtensions
{
public static T MaxDropDownHeight<T>(this T control, IBinding binding) where T : Ursa.Controls.TreeComboBox
   => control._set(Ursa.Controls.TreeComboBox.MaxDropDownHeightProperty, binding);
public static T MaxDropDownHeight<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TreeComboBox
   => control._set(Ursa.Controls.TreeComboBox.MaxDropDownHeightProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T MaxDropDownHeight<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TreeComboBox
   => control._set(Ursa.Controls.TreeComboBox.MaxDropDownHeightProperty, func, onChanged, expression);
public static T MaxDropDownHeight<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TreeComboBox
=> control._setEx(Ursa.Controls.TreeComboBox.MaxDropDownHeightProperty, ps, () => control.MaxDropDownHeight = value, bindingMode, converter, bindingSource);
public static T MaxDropDownHeight<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TreeComboBox
=> control._setEx(Ursa.Controls.TreeComboBox.MaxDropDownHeightProperty, ps, () => control.MaxDropDownHeight = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Watermark<T>(this T control, IBinding binding) where T : Ursa.Controls.TreeComboBox
   => control._set(Ursa.Controls.TreeComboBox.WatermarkProperty, binding);
public static T Watermark<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TreeComboBox
   => control._set(Ursa.Controls.TreeComboBox.WatermarkProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Watermark<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TreeComboBox
   => control._set(Ursa.Controls.TreeComboBox.WatermarkProperty, func, onChanged, expression);
public static T Watermark<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TreeComboBox
=> control._setEx(Ursa.Controls.TreeComboBox.WatermarkProperty, ps, () => control.Watermark = value, bindingMode, converter, bindingSource);
public static T Watermark<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TreeComboBox
=> control._setEx(Ursa.Controls.TreeComboBox.WatermarkProperty, ps, () => control.Watermark = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsDropDownOpen<T>(this T control, IBinding binding) where T : Ursa.Controls.TreeComboBox
   => control._set(Ursa.Controls.TreeComboBox.IsDropDownOpenProperty, binding);
public static T IsDropDownOpen<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TreeComboBox
   => control._set(Ursa.Controls.TreeComboBox.IsDropDownOpenProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsDropDownOpen<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TreeComboBox
   => control._set(Ursa.Controls.TreeComboBox.IsDropDownOpenProperty, func, onChanged, expression);
public static T IsDropDownOpen<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TreeComboBox
=> control._setEx(Ursa.Controls.TreeComboBox.IsDropDownOpenProperty, ps, () => control.IsDropDownOpen = value, bindingMode, converter, bindingSource);
public static T IsDropDownOpen<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TreeComboBox
=> control._setEx(Ursa.Controls.TreeComboBox.IsDropDownOpenProperty, ps, () => control.IsDropDownOpen = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T HorizontalContentAlignment<T>(this T control, IBinding binding) where T : Ursa.Controls.TreeComboBox
   => control._set(Ursa.Controls.TreeComboBox.HorizontalContentAlignmentProperty, binding);
public static T HorizontalContentAlignment<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TreeComboBox
   => control._set(Ursa.Controls.TreeComboBox.HorizontalContentAlignmentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T HorizontalContentAlignment<T>(this T control, Func<Avalonia.Layout.HorizontalAlignment> func, Action<Avalonia.Layout.HorizontalAlignment>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TreeComboBox
   => control._set(Ursa.Controls.TreeComboBox.HorizontalContentAlignmentProperty, func, onChanged, expression);
public static T HorizontalContentAlignment<T>(this T control, Avalonia.Layout.HorizontalAlignment value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TreeComboBox
=> control._setEx(Ursa.Controls.TreeComboBox.HorizontalContentAlignmentProperty, ps, () => control.HorizontalContentAlignment = value, bindingMode, converter, bindingSource);
public static T HorizontalContentAlignment<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Layout.HorizontalAlignment> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TreeComboBox
=> control._setEx(Ursa.Controls.TreeComboBox.HorizontalContentAlignmentProperty, ps, () => control.HorizontalContentAlignment = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T VerticalContentAlignment<T>(this T control, IBinding binding) where T : Ursa.Controls.TreeComboBox
   => control._set(Ursa.Controls.TreeComboBox.VerticalContentAlignmentProperty, binding);
public static T VerticalContentAlignment<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TreeComboBox
   => control._set(Ursa.Controls.TreeComboBox.VerticalContentAlignmentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T VerticalContentAlignment<T>(this T control, Func<Avalonia.Layout.VerticalAlignment> func, Action<Avalonia.Layout.VerticalAlignment>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TreeComboBox
   => control._set(Ursa.Controls.TreeComboBox.VerticalContentAlignmentProperty, func, onChanged, expression);
public static T VerticalContentAlignment<T>(this T control, Avalonia.Layout.VerticalAlignment value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TreeComboBox
=> control._setEx(Ursa.Controls.TreeComboBox.VerticalContentAlignmentProperty, ps, () => control.VerticalContentAlignment = value, bindingMode, converter, bindingSource);
public static T VerticalContentAlignment<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Layout.VerticalAlignment> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TreeComboBox
=> control._setEx(Ursa.Controls.TreeComboBox.VerticalContentAlignmentProperty, ps, () => control.VerticalContentAlignment = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SelectedItemTemplate<T>(this T control, IBinding binding) where T : Ursa.Controls.TreeComboBox
   => control._set(Ursa.Controls.TreeComboBox.SelectedItemTemplateProperty, binding);
public static T SelectedItemTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TreeComboBox
   => control._set(Ursa.Controls.TreeComboBox.SelectedItemTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SelectedItemTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TreeComboBox
   => control._set(Ursa.Controls.TreeComboBox.SelectedItemTemplateProperty, func, onChanged, expression);
public static T SelectedItemTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TreeComboBox
=> control._setEx(Ursa.Controls.TreeComboBox.SelectedItemTemplateProperty, ps, () => control.SelectedItemTemplate = value, bindingMode, converter, bindingSource);
public static T SelectedItemTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TreeComboBox
=> control._setEx(Ursa.Controls.TreeComboBox.SelectedItemTemplateProperty, ps, () => control.SelectedItemTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SelectedItem<T>(this T control, IBinding binding) where T : Ursa.Controls.TreeComboBox
   => control._set(Ursa.Controls.TreeComboBox.SelectedItemProperty, binding);
public static T SelectedItem<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TreeComboBox
   => control._set(Ursa.Controls.TreeComboBox.SelectedItemProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SelectedItem<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TreeComboBox
   => control._set(Ursa.Controls.TreeComboBox.SelectedItemProperty, func, onChanged, expression);
public static T SelectedItem<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TreeComboBox
=> control._setEx(Ursa.Controls.TreeComboBox.SelectedItemProperty, ps, () => control.SelectedItem = value, bindingMode, converter, bindingSource);
public static T SelectedItem<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TreeComboBox
=> control._setEx(Ursa.Controls.TreeComboBox.SelectedItemProperty, ps, () => control.SelectedItem = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T InnerLeftContent<T>(this T control, IBinding binding) where T : Ursa.Controls.TreeComboBox
   => control._set(Ursa.Controls.TreeComboBox.InnerLeftContentProperty, binding);
public static T InnerLeftContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TreeComboBox
   => control._set(Ursa.Controls.TreeComboBox.InnerLeftContentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T InnerLeftContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TreeComboBox
   => control._set(Ursa.Controls.TreeComboBox.InnerLeftContentProperty, func, onChanged, expression);
public static T InnerLeftContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TreeComboBox
=> control._setEx(Ursa.Controls.TreeComboBox.InnerLeftContentProperty, ps, () => control.InnerLeftContent = value, bindingMode, converter, bindingSource);
public static T InnerLeftContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TreeComboBox
=> control._setEx(Ursa.Controls.TreeComboBox.InnerLeftContentProperty, ps, () => control.InnerLeftContent = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T InnerRightContent<T>(this T control, IBinding binding) where T : Ursa.Controls.TreeComboBox
   => control._set(Ursa.Controls.TreeComboBox.InnerRightContentProperty, binding);
public static T InnerRightContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TreeComboBox
   => control._set(Ursa.Controls.TreeComboBox.InnerRightContentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T InnerRightContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TreeComboBox
   => control._set(Ursa.Controls.TreeComboBox.InnerRightContentProperty, func, onChanged, expression);
public static T InnerRightContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TreeComboBox
=> control._setEx(Ursa.Controls.TreeComboBox.InnerRightContentProperty, ps, () => control.InnerRightContent = value, bindingMode, converter, bindingSource);
public static T InnerRightContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TreeComboBox
=> control._setEx(Ursa.Controls.TreeComboBox.InnerRightContentProperty, ps, () => control.InnerRightContent = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T PopupInnerTopContent<T>(this T control, IBinding binding) where T : Ursa.Controls.TreeComboBox
   => control._set(Ursa.Controls.TreeComboBox.PopupInnerTopContentProperty, binding);
public static T PopupInnerTopContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TreeComboBox
   => control._set(Ursa.Controls.TreeComboBox.PopupInnerTopContentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T PopupInnerTopContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TreeComboBox
   => control._set(Ursa.Controls.TreeComboBox.PopupInnerTopContentProperty, func, onChanged, expression);
public static T PopupInnerTopContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TreeComboBox
=> control._setEx(Ursa.Controls.TreeComboBox.PopupInnerTopContentProperty, ps, () => control.PopupInnerTopContent = value, bindingMode, converter, bindingSource);
public static T PopupInnerTopContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TreeComboBox
=> control._setEx(Ursa.Controls.TreeComboBox.PopupInnerTopContentProperty, ps, () => control.PopupInnerTopContent = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T PopupInnerBottomContent<T>(this T control, IBinding binding) where T : Ursa.Controls.TreeComboBox
   => control._set(Ursa.Controls.TreeComboBox.PopupInnerBottomContentProperty, binding);
public static T PopupInnerBottomContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TreeComboBox
   => control._set(Ursa.Controls.TreeComboBox.PopupInnerBottomContentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T PopupInnerBottomContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TreeComboBox
   => control._set(Ursa.Controls.TreeComboBox.PopupInnerBottomContentProperty, func, onChanged, expression);
public static T PopupInnerBottomContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TreeComboBox
=> control._setEx(Ursa.Controls.TreeComboBox.PopupInnerBottomContentProperty, ps, () => control.PopupInnerBottomContent = value, bindingMode, converter, bindingSource);
public static T PopupInnerBottomContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TreeComboBox
=> control._setEx(Ursa.Controls.TreeComboBox.PopupInnerBottomContentProperty, ps, () => control.PopupInnerBottomContent = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

