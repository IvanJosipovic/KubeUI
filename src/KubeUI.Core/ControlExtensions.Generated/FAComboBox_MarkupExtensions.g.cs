#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
[global::System.CodeDom.Compiler.GeneratedCode("AvaloniaExtensionGenerator", "11.1.3.0")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static partial class FAComboBox_MarkupExtensions
{
//================= Properties ======================//
 // MaxDropDownHeightProperty

/*BindFromExpressionSetterGenerator*/
public static T MaxDropDownHeight<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.MaxDropDownHeightProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T MaxDropDownHeight<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.MaxDropDownHeightProperty, ps, () => control.MaxDropDownHeight = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T MaxDropDownHeight<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.MaxDropDownHeightProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T MaxDropDownHeight<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.MaxDropDownHeightProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T MaxDropDownHeight<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.MaxDropDownHeightProperty, ps, () => control.MaxDropDownHeight = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsEditableProperty

/*BindFromExpressionSetterGenerator*/
public static T IsEditable<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.IsEditableProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsEditable<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.IsEditableProperty, ps, () => control.IsEditable = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsEditable<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.IsEditableProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsEditable<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.IsEditableProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsEditable<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.IsEditableProperty, ps, () => control.IsEditable = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsDropDownOpenProperty

/*BindFromExpressionSetterGenerator*/
public static T IsDropDownOpen<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.IsDropDownOpenProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsDropDownOpen<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.IsDropDownOpenProperty, ps, () => control.IsDropDownOpen = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsDropDownOpen<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.IsDropDownOpenProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsDropDownOpen<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.IsDropDownOpenProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsDropDownOpen<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.IsDropDownOpenProperty, ps, () => control.IsDropDownOpen = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // PlaceholderTextProperty

/*BindFromExpressionSetterGenerator*/
public static T PlaceholderText<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.PlaceholderTextProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T PlaceholderText<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.PlaceholderTextProperty, ps, () => control.PlaceholderText = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T PlaceholderText<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.PlaceholderTextProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T PlaceholderText<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.PlaceholderTextProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T PlaceholderText<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.PlaceholderTextProperty, ps, () => control.PlaceholderText = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SelectionChangedTriggerProperty

/*BindFromExpressionSetterGenerator*/
public static T SelectionChangedTrigger<T>(this T control, Func<FluentAvalonia.UI.Controls.FAComboBoxSelectionChangedTrigger> func, Action<FluentAvalonia.UI.Controls.FAComboBoxSelectionChangedTrigger>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.SelectionChangedTriggerProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SelectionChangedTrigger<T>(this T control, FluentAvalonia.UI.Controls.FAComboBoxSelectionChangedTrigger value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.SelectionChangedTriggerProperty, ps, () => control.SelectionChangedTrigger = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SelectionChangedTrigger<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.SelectionChangedTriggerProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SelectionChangedTrigger<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.SelectionChangedTriggerProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SelectionChangedTrigger<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.FAComboBoxSelectionChangedTrigger> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.SelectionChangedTriggerProperty, ps, () => control.SelectionChangedTrigger = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // PlaceholderForegroundProperty

/*BindFromExpressionSetterGenerator*/
public static T PlaceholderForeground<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.PlaceholderForegroundProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T PlaceholderForeground<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.PlaceholderForegroundProperty, ps, () => control.PlaceholderForeground = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T PlaceholderForeground<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.PlaceholderForegroundProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T PlaceholderForeground<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.PlaceholderForegroundProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T PlaceholderForeground<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.PlaceholderForegroundProperty, ps, () => control.PlaceholderForeground = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TextBoxThemeProperty

/*BindFromExpressionSetterGenerator*/
public static T TextBoxTheme<T>(this T control, Func<Avalonia.Styling.ControlTheme> func, Action<Avalonia.Styling.ControlTheme>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.TextBoxThemeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T TextBoxTheme<T>(this T control, Avalonia.Styling.ControlTheme value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.TextBoxThemeProperty, ps, () => control.TextBoxTheme = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TextBoxTheme<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.TextBoxThemeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TextBoxTheme<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.TextBoxThemeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T TextBoxTheme<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ControlTheme> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.TextBoxThemeProperty, ps, () => control.TextBoxTheme = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TextProperty

/*BindFromExpressionSetterGenerator*/
public static T Text<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.TextProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Text<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.TextProperty, ps, () => control.Text = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Text<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.TextProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Text<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.TextProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Text<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.TextProperty, ps, () => control.Text = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // HorizontalContentAlignmentProperty

/*BindFromExpressionSetterGenerator*/
public static T HorizontalContentAlignment<T>(this T control, Func<Avalonia.Layout.HorizontalAlignment> func, Action<Avalonia.Layout.HorizontalAlignment>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.HorizontalContentAlignmentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T HorizontalContentAlignment<T>(this T control, Avalonia.Layout.HorizontalAlignment value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.HorizontalContentAlignmentProperty, ps, () => control.HorizontalContentAlignment = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T HorizontalContentAlignment<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.HorizontalContentAlignmentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T HorizontalContentAlignment<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.HorizontalContentAlignmentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T HorizontalContentAlignment<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Layout.HorizontalAlignment> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.HorizontalContentAlignmentProperty, ps, () => control.HorizontalContentAlignment = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // VerticalContentAlignmentProperty

/*BindFromExpressionSetterGenerator*/
public static T VerticalContentAlignment<T>(this T control, Func<Avalonia.Layout.VerticalAlignment> func, Action<Avalonia.Layout.VerticalAlignment>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.VerticalContentAlignmentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T VerticalContentAlignment<T>(this T control, Avalonia.Layout.VerticalAlignment value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.VerticalContentAlignmentProperty, ps, () => control.VerticalContentAlignment = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T VerticalContentAlignment<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.VerticalContentAlignmentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T VerticalContentAlignment<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAComboBox
   => control._set(FluentAvalonia.UI.Controls.FAComboBox.VerticalContentAlignmentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T VerticalContentAlignment<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Layout.VerticalAlignment> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAComboBox
=> control._setEx(FluentAvalonia.UI.Controls.FAComboBox.VerticalContentAlignmentProperty, ps, () => control.VerticalContentAlignment = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // DropDownOpened

/*ActionToEventGenerator*/
    public static T OnDropDownOpened<T>(this T control, Action<System.EventArgs> action) where T : FluentAvalonia.UI.Controls.FAComboBox => 
        control._setEvent((System.EventHandler<System.EventArgs>) ((arg0, arg1) => action(arg1)), h => control.DropDownOpened += h);


 // DropDownClosed

/*ActionToEventGenerator*/
    public static T OnDropDownClosed<T>(this T control, Action<System.EventArgs> action) where T : FluentAvalonia.UI.Controls.FAComboBox => 
        control._setEvent((System.EventHandler<System.EventArgs>) ((arg0, arg1) => action(arg1)), h => control.DropDownClosed += h);


 // TextSubmitted

/*ActionToEventGenerator*/
    public static T OnTextSubmitted<T>(this T control, Action<FluentAvalonia.UI.Controls.FAComboBox, FluentAvalonia.UI.Controls.FAComboBoxTextSubmittedEventArgs> action) where T : FluentAvalonia.UI.Controls.FAComboBox => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.FAComboBox,FluentAvalonia.UI.Controls.FAComboBoxTextSubmittedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.TextSubmitted += h);



//================= Styles ======================//
 // MaxDropDownHeightProperty

/*ValueStyleSetterGenerator*/
public static Style<T> MaxDropDownHeight<T>(this Style<T> style, System.Double value) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.MaxDropDownHeightProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> MaxDropDownHeight<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.MaxDropDownHeightProperty, binding);


 // IsEditableProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsEditable<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.IsEditableProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsEditable<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.IsEditableProperty, binding);


 // IsDropDownOpenProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsDropDownOpen<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.IsDropDownOpenProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsDropDownOpen<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.IsDropDownOpenProperty, binding);


 // PlaceholderTextProperty

/*ValueStyleSetterGenerator*/
public static Style<T> PlaceholderText<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.PlaceholderTextProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> PlaceholderText<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.PlaceholderTextProperty, binding);


 // SelectionChangedTriggerProperty

/*ValueStyleSetterGenerator*/
public static Style<T> SelectionChangedTrigger<T>(this Style<T> style, FluentAvalonia.UI.Controls.FAComboBoxSelectionChangedTrigger value) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.SelectionChangedTriggerProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SelectionChangedTrigger<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.SelectionChangedTriggerProperty, binding);


 // PlaceholderForegroundProperty

/*ValueStyleSetterGenerator*/
public static Style<T> PlaceholderForeground<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.PlaceholderForegroundProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> PlaceholderForeground<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.PlaceholderForegroundProperty, binding);


 // TextBoxThemeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> TextBoxTheme<T>(this Style<T> style, Avalonia.Styling.ControlTheme value) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.TextBoxThemeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> TextBoxTheme<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.TextBoxThemeProperty, binding);


 // TextProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Text<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.TextProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Text<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.TextProperty, binding);


 // HorizontalContentAlignmentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> HorizontalContentAlignment<T>(this Style<T> style, Avalonia.Layout.HorizontalAlignment value) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.HorizontalContentAlignmentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> HorizontalContentAlignment<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.HorizontalContentAlignmentProperty, binding);


 // VerticalContentAlignmentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> VerticalContentAlignment<T>(this Style<T> style, Avalonia.Layout.VerticalAlignment value) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.VerticalContentAlignmentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> VerticalContentAlignment<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.VerticalContentAlignmentProperty, binding);



}
