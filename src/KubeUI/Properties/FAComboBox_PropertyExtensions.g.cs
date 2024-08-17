#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using FAComboBox = FluentAvalonia.UI.Controls.FAComboBox;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class FAComboBoxExtensions
{
public static T MaxDropDownHeight<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.MaxDropDownHeightProperty, binding);
public static T MaxDropDownHeight<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.MaxDropDownHeightProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T MaxDropDownHeight<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.MaxDropDownHeightProperty, func, onChanged, expression);
public static T MaxDropDownHeight<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.MaxDropDownHeightProperty, ps, () => control.MaxDropDownHeight = value, bindingMode, converter, bindingSource);
public static T MaxDropDownHeight<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.MaxDropDownHeightProperty, ps, () => control.MaxDropDownHeight = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsEditable<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.IsEditableProperty, binding);
public static T IsEditable<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.IsEditableProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsEditable<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.IsEditableProperty, func, onChanged, expression);
public static T IsEditable<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.IsEditableProperty, ps, () => control.IsEditable = value, bindingMode, converter, bindingSource);
public static T IsEditable<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.IsEditableProperty, ps, () => control.IsEditable = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsDropDownOpen<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.IsDropDownOpenProperty, binding);
public static T IsDropDownOpen<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.IsDropDownOpenProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsDropDownOpen<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.IsDropDownOpenProperty, func, onChanged, expression);
public static T IsDropDownOpen<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.IsDropDownOpenProperty, ps, () => control.IsDropDownOpen = value, bindingMode, converter, bindingSource);
public static T IsDropDownOpen<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.IsDropDownOpenProperty, ps, () => control.IsDropDownOpen = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T PlaceholderText<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.PlaceholderTextProperty, binding);
public static T PlaceholderText<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.PlaceholderTextProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T PlaceholderText<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.PlaceholderTextProperty, func, onChanged, expression);
public static T PlaceholderText<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.PlaceholderTextProperty, ps, () => control.PlaceholderText = value, bindingMode, converter, bindingSource);
public static T PlaceholderText<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.PlaceholderTextProperty, ps, () => control.PlaceholderText = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SelectionChangedTrigger<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.SelectionChangedTriggerProperty, binding);
public static T SelectionChangedTrigger<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.SelectionChangedTriggerProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SelectionChangedTrigger<T>(this T control, Func<FluentAvalonia.UI.Controls.FAComboBoxSelectionChangedTrigger> func, Action<FluentAvalonia.UI.Controls.FAComboBoxSelectionChangedTrigger>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.SelectionChangedTriggerProperty, func, onChanged, expression);
public static T SelectionChangedTrigger<T>(this T control, FluentAvalonia.UI.Controls.FAComboBoxSelectionChangedTrigger value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.SelectionChangedTriggerProperty, ps, () => control.SelectionChangedTrigger = value, bindingMode, converter, bindingSource);
public static T SelectionChangedTrigger<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.FAComboBoxSelectionChangedTrigger> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.SelectionChangedTriggerProperty, ps, () => control.SelectionChangedTrigger = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T PlaceholderForeground<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.PlaceholderForegroundProperty, binding);
public static T PlaceholderForeground<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.PlaceholderForegroundProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T PlaceholderForeground<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.PlaceholderForegroundProperty, func, onChanged, expression);
public static T PlaceholderForeground<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.PlaceholderForegroundProperty, ps, () => control.PlaceholderForeground = value, bindingMode, converter, bindingSource);
public static T PlaceholderForeground<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.PlaceholderForegroundProperty, ps, () => control.PlaceholderForeground = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T TextBoxTheme<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.TextBoxThemeProperty, binding);
public static T TextBoxTheme<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.TextBoxThemeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T TextBoxTheme<T>(this T control, Func<Avalonia.Styling.ControlTheme> func, Action<Avalonia.Styling.ControlTheme>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.TextBoxThemeProperty, func, onChanged, expression);
public static T TextBoxTheme<T>(this T control, Avalonia.Styling.ControlTheme value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.TextBoxThemeProperty, ps, () => control.TextBoxTheme = value, bindingMode, converter, bindingSource);
public static T TextBoxTheme<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ControlTheme> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.TextBoxThemeProperty, ps, () => control.TextBoxTheme = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Text<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.TextProperty, binding);
public static T Text<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.TextProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Text<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.TextProperty, func, onChanged, expression);
public static T Text<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.TextProperty, ps, () => control.Text = value, bindingMode, converter, bindingSource);
public static T Text<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.TextProperty, ps, () => control.Text = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T HorizontalContentAlignment<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.HorizontalContentAlignmentProperty, binding);
public static T HorizontalContentAlignment<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.HorizontalContentAlignmentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T HorizontalContentAlignment<T>(this T control, Func<Avalonia.Layout.HorizontalAlignment> func, Action<Avalonia.Layout.HorizontalAlignment>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.HorizontalContentAlignmentProperty, func, onChanged, expression);
public static T HorizontalContentAlignment<T>(this T control, Avalonia.Layout.HorizontalAlignment value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.HorizontalContentAlignmentProperty, ps, () => control.HorizontalContentAlignment = value, bindingMode, converter, bindingSource);
public static T HorizontalContentAlignment<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Layout.HorizontalAlignment> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.HorizontalContentAlignmentProperty, ps, () => control.HorizontalContentAlignment = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T VerticalContentAlignment<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.VerticalContentAlignmentProperty, binding);
public static T VerticalContentAlignment<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.VerticalContentAlignmentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T VerticalContentAlignment<T>(this T control, Func<Avalonia.Layout.VerticalAlignment> func, Action<Avalonia.Layout.VerticalAlignment>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.VerticalContentAlignmentProperty, func, onChanged, expression);
public static T VerticalContentAlignment<T>(this T control, Avalonia.Layout.VerticalAlignment value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.VerticalContentAlignmentProperty, ps, () => control.VerticalContentAlignment = value, bindingMode, converter, bindingSource);
public static T VerticalContentAlignment<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Layout.VerticalAlignment> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.VerticalContentAlignmentProperty, ps, () => control.VerticalContentAlignment = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

