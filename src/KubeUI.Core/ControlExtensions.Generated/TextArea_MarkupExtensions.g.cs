#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
[global::System.CodeDom.Compiler.GeneratedCode("AvaloniaExtensionGenerator", "1.0.0.0")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static partial class TextArea_MarkupExtensions
{
//================= Properties ======================//
 // Watermark

/*ValueSetterGenerator*/
public static T Watermark<T>(this T control, System.String value) where T : AvaloniaEdit.Editing.TextArea 
=> control._set(() => control.Watermark = value!);

/*BindFromExpressionSetterGenerator*/
public static T Watermark<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : AvaloniaEdit.Editing.TextArea 
   => control._set(AvaloniaEdit.Editing.TextArea.WatermarkProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T Watermark<T>(this T control,System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : AvaloniaEdit.Editing.TextArea 
=> control._setEx(AvaloniaEdit.Editing.TextArea.WatermarkProperty, ps, () => control.Watermark = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Watermark<T>(this T control, IBinding binding) where T : AvaloniaEdit.Editing.TextArea 
   => control._set(AvaloniaEdit.Editing.TextArea.WatermarkProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Watermark<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Editing.TextArea 
   => control._set(AvaloniaEdit.Editing.TextArea.WatermarkProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T Watermark<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : AvaloniaEdit.Editing.TextArea 
=> control._setEx(AvaloniaEdit.Editing.TextArea.WatermarkProperty, ps, () => control.Watermark = converter.TryConvert(value)!, bindingMode, converter, bindingSource);


 // Document

/*ValueSetterGenerator*/
public static T Document<T>(this T control, AvaloniaEdit.Document.TextDocument value) where T : AvaloniaEdit.Editing.TextArea 
=> control._set(() => control.Document = value!);

/*BindFromExpressionSetterGenerator*/
public static T Document<T>(this T control, Func<AvaloniaEdit.Document.TextDocument> func, Action<AvaloniaEdit.Document.TextDocument>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : AvaloniaEdit.Editing.TextArea 
   => control._set(AvaloniaEdit.Editing.TextArea.DocumentProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T Document<T>(this T control,AvaloniaEdit.Document.TextDocument value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : AvaloniaEdit.Editing.TextArea 
=> control._setEx(AvaloniaEdit.Editing.TextArea.DocumentProperty, ps, () => control.Document = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Document<T>(this T control, IBinding binding) where T : AvaloniaEdit.Editing.TextArea 
   => control._set(AvaloniaEdit.Editing.TextArea.DocumentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Document<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Editing.TextArea 
   => control._set(AvaloniaEdit.Editing.TextArea.DocumentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T Document<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, AvaloniaEdit.Document.TextDocument> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : AvaloniaEdit.Editing.TextArea 
=> control._setEx(AvaloniaEdit.Editing.TextArea.DocumentProperty, ps, () => control.Document = converter.TryConvert(value)!, bindingMode, converter, bindingSource);


 // Options

/*ValueSetterGenerator*/
public static T Options<T>(this T control, AvaloniaEdit.TextEditorOptions value) where T : AvaloniaEdit.Editing.TextArea 
=> control._set(() => control.Options = value!);

/*BindFromExpressionSetterGenerator*/
public static T Options<T>(this T control, Func<AvaloniaEdit.TextEditorOptions> func, Action<AvaloniaEdit.TextEditorOptions>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : AvaloniaEdit.Editing.TextArea 
   => control._set(AvaloniaEdit.Editing.TextArea.OptionsProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T Options<T>(this T control,AvaloniaEdit.TextEditorOptions value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : AvaloniaEdit.Editing.TextArea 
=> control._setEx(AvaloniaEdit.Editing.TextArea.OptionsProperty, ps, () => control.Options = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Options<T>(this T control, IBinding binding) where T : AvaloniaEdit.Editing.TextArea 
   => control._set(AvaloniaEdit.Editing.TextArea.OptionsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Options<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Editing.TextArea 
   => control._set(AvaloniaEdit.Editing.TextArea.OptionsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T Options<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, AvaloniaEdit.TextEditorOptions> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : AvaloniaEdit.Editing.TextArea 
=> control._setEx(AvaloniaEdit.Editing.TextArea.OptionsProperty, ps, () => control.Options = converter.TryConvert(value)!, bindingMode, converter, bindingSource);


 // SelectionBrush

/*ValueSetterGenerator*/
public static T SelectionBrush<T>(this T control, Avalonia.Media.IBrush value) where T : AvaloniaEdit.Editing.TextArea 
=> control._set(() => control.SelectionBrush = value!);

/*BindFromExpressionSetterGenerator*/
public static T SelectionBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : AvaloniaEdit.Editing.TextArea 
   => control._set(AvaloniaEdit.Editing.TextArea.SelectionBrushProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T SelectionBrush<T>(this T control,Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : AvaloniaEdit.Editing.TextArea 
=> control._setEx(AvaloniaEdit.Editing.TextArea.SelectionBrushProperty, ps, () => control.SelectionBrush = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SelectionBrush<T>(this T control, IBinding binding) where T : AvaloniaEdit.Editing.TextArea 
   => control._set(AvaloniaEdit.Editing.TextArea.SelectionBrushProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SelectionBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Editing.TextArea 
   => control._set(AvaloniaEdit.Editing.TextArea.SelectionBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T SelectionBrush<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : AvaloniaEdit.Editing.TextArea 
=> control._setEx(AvaloniaEdit.Editing.TextArea.SelectionBrushProperty, ps, () => control.SelectionBrush = converter.TryConvert(value)!, bindingMode, converter, bindingSource);


 // SelectionForeground

/*ValueSetterGenerator*/
public static T SelectionForeground<T>(this T control, Avalonia.Media.IBrush value) where T : AvaloniaEdit.Editing.TextArea 
=> control._set(() => control.SelectionForeground = value!);

/*BindFromExpressionSetterGenerator*/
public static T SelectionForeground<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : AvaloniaEdit.Editing.TextArea 
   => control._set(AvaloniaEdit.Editing.TextArea.SelectionForegroundProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T SelectionForeground<T>(this T control,Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : AvaloniaEdit.Editing.TextArea 
=> control._setEx(AvaloniaEdit.Editing.TextArea.SelectionForegroundProperty, ps, () => control.SelectionForeground = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SelectionForeground<T>(this T control, IBinding binding) where T : AvaloniaEdit.Editing.TextArea 
   => control._set(AvaloniaEdit.Editing.TextArea.SelectionForegroundProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SelectionForeground<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Editing.TextArea 
   => control._set(AvaloniaEdit.Editing.TextArea.SelectionForegroundProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T SelectionForeground<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : AvaloniaEdit.Editing.TextArea 
=> control._setEx(AvaloniaEdit.Editing.TextArea.SelectionForegroundProperty, ps, () => control.SelectionForeground = converter.TryConvert(value)!, bindingMode, converter, bindingSource);


 // SelectionBorder

/*ValueSetterGenerator*/
public static T SelectionBorder<T>(this T control, Avalonia.Media.Pen value) where T : AvaloniaEdit.Editing.TextArea 
=> control._set(() => control.SelectionBorder = value!);

/*BindFromExpressionSetterGenerator*/
public static T SelectionBorder<T>(this T control, Func<Avalonia.Media.Pen> func, Action<Avalonia.Media.Pen>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : AvaloniaEdit.Editing.TextArea 
   => control._set(AvaloniaEdit.Editing.TextArea.SelectionBorderProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T SelectionBorder<T>(this T control,Avalonia.Media.Pen value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : AvaloniaEdit.Editing.TextArea 
=> control._setEx(AvaloniaEdit.Editing.TextArea.SelectionBorderProperty, ps, () => control.SelectionBorder = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SelectionBorder<T>(this T control, IBinding binding) where T : AvaloniaEdit.Editing.TextArea 
   => control._set(AvaloniaEdit.Editing.TextArea.SelectionBorderProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SelectionBorder<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Editing.TextArea 
   => control._set(AvaloniaEdit.Editing.TextArea.SelectionBorderProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T SelectionBorder<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.Pen> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : AvaloniaEdit.Editing.TextArea 
=> control._setEx(AvaloniaEdit.Editing.TextArea.SelectionBorderProperty, ps, () => control.SelectionBorder = converter.TryConvert(value)!, bindingMode, converter, bindingSource);


 // SelectionCornerRadius

/*ValueSetterGenerator*/
public static T SelectionCornerRadius<T>(this T control, System.Double value) where T : AvaloniaEdit.Editing.TextArea 
=> control._set(() => control.SelectionCornerRadius = value!);

/*BindFromExpressionSetterGenerator*/
public static T SelectionCornerRadius<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : AvaloniaEdit.Editing.TextArea 
   => control._set(AvaloniaEdit.Editing.TextArea.SelectionCornerRadiusProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T SelectionCornerRadius<T>(this T control,System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : AvaloniaEdit.Editing.TextArea 
=> control._setEx(AvaloniaEdit.Editing.TextArea.SelectionCornerRadiusProperty, ps, () => control.SelectionCornerRadius = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SelectionCornerRadius<T>(this T control, IBinding binding) where T : AvaloniaEdit.Editing.TextArea 
   => control._set(AvaloniaEdit.Editing.TextArea.SelectionCornerRadiusProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SelectionCornerRadius<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Editing.TextArea 
   => control._set(AvaloniaEdit.Editing.TextArea.SelectionCornerRadiusProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T SelectionCornerRadius<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : AvaloniaEdit.Editing.TextArea 
=> control._setEx(AvaloniaEdit.Editing.TextArea.SelectionCornerRadiusProperty, ps, () => control.SelectionCornerRadius = converter.TryConvert(value)!, bindingMode, converter, bindingSource);


 // RightClickMovesCaret

/*ValueSetterGenerator*/
public static T RightClickMovesCaret<T>(this T control, System.Boolean value) where T : AvaloniaEdit.Editing.TextArea 
=> control._set(() => control.RightClickMovesCaret = value!);

/*BindFromExpressionSetterGenerator*/
public static T RightClickMovesCaret<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : AvaloniaEdit.Editing.TextArea 
   => control._set(AvaloniaEdit.Editing.TextArea.RightClickMovesCaretProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T RightClickMovesCaret<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : AvaloniaEdit.Editing.TextArea 
=> control._setEx(AvaloniaEdit.Editing.TextArea.RightClickMovesCaretProperty, ps, () => control.RightClickMovesCaret = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T RightClickMovesCaret<T>(this T control, IBinding binding) where T : AvaloniaEdit.Editing.TextArea 
   => control._set(AvaloniaEdit.Editing.TextArea.RightClickMovesCaretProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T RightClickMovesCaret<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Editing.TextArea 
   => control._set(AvaloniaEdit.Editing.TextArea.RightClickMovesCaretProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T RightClickMovesCaret<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : AvaloniaEdit.Editing.TextArea 
=> control._setEx(AvaloniaEdit.Editing.TextArea.RightClickMovesCaretProperty, ps, () => control.RightClickMovesCaret = converter.TryConvert(value)!, bindingMode, converter, bindingSource);


 // IndentationStrategy

/*ValueSetterGenerator*/
public static T IndentationStrategy<T>(this T control, AvaloniaEdit.Indentation.IIndentationStrategy value) where T : AvaloniaEdit.Editing.TextArea 
=> control._set(() => control.IndentationStrategy = value!);

/*BindFromExpressionSetterGenerator*/
public static T IndentationStrategy<T>(this T control, Func<AvaloniaEdit.Indentation.IIndentationStrategy> func, Action<AvaloniaEdit.Indentation.IIndentationStrategy>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : AvaloniaEdit.Editing.TextArea 
   => control._set(AvaloniaEdit.Editing.TextArea.IndentationStrategyProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T IndentationStrategy<T>(this T control,AvaloniaEdit.Indentation.IIndentationStrategy value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : AvaloniaEdit.Editing.TextArea 
=> control._setEx(AvaloniaEdit.Editing.TextArea.IndentationStrategyProperty, ps, () => control.IndentationStrategy = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IndentationStrategy<T>(this T control, IBinding binding) where T : AvaloniaEdit.Editing.TextArea 
   => control._set(AvaloniaEdit.Editing.TextArea.IndentationStrategyProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IndentationStrategy<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Editing.TextArea 
   => control._set(AvaloniaEdit.Editing.TextArea.IndentationStrategyProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T IndentationStrategy<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, AvaloniaEdit.Indentation.IIndentationStrategy> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : AvaloniaEdit.Editing.TextArea 
=> control._setEx(AvaloniaEdit.Editing.TextArea.IndentationStrategyProperty, ps, () => control.IndentationStrategy = converter.TryConvert(value)!, bindingMode, converter, bindingSource);


 // OverstrikeMode

/*ValueSetterGenerator*/
public static T OverstrikeMode<T>(this T control, System.Boolean value) where T : AvaloniaEdit.Editing.TextArea 
=> control._set(() => control.OverstrikeMode = value!);

/*BindFromExpressionSetterGenerator*/
public static T OverstrikeMode<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : AvaloniaEdit.Editing.TextArea 
   => control._set(AvaloniaEdit.Editing.TextArea.OverstrikeModeProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T OverstrikeMode<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : AvaloniaEdit.Editing.TextArea 
=> control._setEx(AvaloniaEdit.Editing.TextArea.OverstrikeModeProperty, ps, () => control.OverstrikeMode = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T OverstrikeMode<T>(this T control, IBinding binding) where T : AvaloniaEdit.Editing.TextArea 
   => control._set(AvaloniaEdit.Editing.TextArea.OverstrikeModeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T OverstrikeMode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Editing.TextArea 
   => control._set(AvaloniaEdit.Editing.TextArea.OverstrikeModeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T OverstrikeMode<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : AvaloniaEdit.Editing.TextArea 
=> control._setEx(AvaloniaEdit.Editing.TextArea.OverstrikeModeProperty, ps, () => control.OverstrikeMode = converter.TryConvert(value)!, bindingMode, converter, bindingSource);



//================= Events ======================//
 // ActiveInputHandlerChanged

/*ActionToEventGenerator*/
public static T OnActiveInputHandlerChanged<T>(this T control, Action<System.EventArgs> action) where T : AvaloniaEdit.Editing.TextArea  => 
 control._setEvent((System.EventHandler) ((arg0, arg1) => action(arg1)), h => control.ActiveInputHandlerChanged += h);


 // DocumentChanged

/*ActionToEventGenerator*/
public static T OnDocumentChanged<T>(this T control, Action<AvaloniaEdit.Document.DocumentChangedEventArgs> action) where T : AvaloniaEdit.Editing.TextArea  => 
 control._setEvent((System.EventHandler<AvaloniaEdit.Document.DocumentChangedEventArgs>) ((arg0, arg1) => action(arg1)), h => control.DocumentChanged += h);


 // OptionChanged

/*ActionToEventGenerator*/
public static T OnOptionChanged<T>(this T control, Action<System.ComponentModel.PropertyChangedEventArgs> action) where T : AvaloniaEdit.Editing.TextArea  => 
 control._setEvent((System.ComponentModel.PropertyChangedEventHandler) ((arg0, arg1) => action(arg1)), h => control.OptionChanged += h);


 // SelectionChanged

/*ActionToEventGenerator*/
public static T OnSelectionChanged<T>(this T control, Action<System.EventArgs> action) where T : AvaloniaEdit.Editing.TextArea  => 
 control._setEvent((System.EventHandler) ((arg0, arg1) => action(arg1)), h => control.SelectionChanged += h);


 // TextEntering

/*ActionToEventGenerator*/
public static T OnTextEntering<T>(this T control, Action<Avalonia.Input.TextInputEventArgs> action) where T : AvaloniaEdit.Editing.TextArea  => 
 control._setEvent((System.EventHandler<Avalonia.Input.TextInputEventArgs>) ((arg0, arg1) => action(arg1)), h => control.TextEntering += h);


 // TextEntered

/*ActionToEventGenerator*/
public static T OnTextEntered<T>(this T control, Action<Avalonia.Input.TextInputEventArgs> action) where T : AvaloniaEdit.Editing.TextArea  => 
 control._setEvent((System.EventHandler<Avalonia.Input.TextInputEventArgs>) ((arg0, arg1) => action(arg1)), h => control.TextEntered += h);


 // TextCopied

/*ActionToEventGenerator*/
public static T OnTextCopied<T>(this T control, Action<AvaloniaEdit.Editing.TextEventArgs> action) where T : AvaloniaEdit.Editing.TextArea  => 
 control._setEvent((System.EventHandler<AvaloniaEdit.Editing.TextEventArgs>) ((arg0, arg1) => action(arg1)), h => control.TextCopied += h);



//================= Styles ======================//
 // Watermark

/*ValueStyleSetterGenerator*/
public static Style<T> Watermark<T>(this Style<T> style, System.String value) where T : AvaloniaEdit.Editing.TextArea 
=> style._addSetter(AvaloniaEdit.Editing.TextArea.WatermarkProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> Watermark<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Editing.TextArea 
=> style._addSetter(AvaloniaEdit.Editing.TextArea.WatermarkProperty, binding);


 // Document

/*ValueStyleSetterGenerator*/
public static Style<T> Document<T>(this Style<T> style, AvaloniaEdit.Document.TextDocument value) where T : AvaloniaEdit.Editing.TextArea 
=> style._addSetter(AvaloniaEdit.Editing.TextArea.DocumentProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> Document<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Editing.TextArea 
=> style._addSetter(AvaloniaEdit.Editing.TextArea.DocumentProperty, binding);


 // Options

/*ValueStyleSetterGenerator*/
public static Style<T> Options<T>(this Style<T> style, AvaloniaEdit.TextEditorOptions value) where T : AvaloniaEdit.Editing.TextArea 
=> style._addSetter(AvaloniaEdit.Editing.TextArea.OptionsProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> Options<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Editing.TextArea 
=> style._addSetter(AvaloniaEdit.Editing.TextArea.OptionsProperty, binding);


 // SelectionBrush

/*ValueStyleSetterGenerator*/
public static Style<T> SelectionBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : AvaloniaEdit.Editing.TextArea 
=> style._addSetter(AvaloniaEdit.Editing.TextArea.SelectionBrushProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> SelectionBrush<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Editing.TextArea 
=> style._addSetter(AvaloniaEdit.Editing.TextArea.SelectionBrushProperty, binding);


 // SelectionForeground

/*ValueStyleSetterGenerator*/
public static Style<T> SelectionForeground<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : AvaloniaEdit.Editing.TextArea 
=> style._addSetter(AvaloniaEdit.Editing.TextArea.SelectionForegroundProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> SelectionForeground<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Editing.TextArea 
=> style._addSetter(AvaloniaEdit.Editing.TextArea.SelectionForegroundProperty, binding);


 // SelectionBorder

/*ValueStyleSetterGenerator*/
public static Style<T> SelectionBorder<T>(this Style<T> style, Avalonia.Media.Pen value) where T : AvaloniaEdit.Editing.TextArea 
=> style._addSetter(AvaloniaEdit.Editing.TextArea.SelectionBorderProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> SelectionBorder<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Editing.TextArea 
=> style._addSetter(AvaloniaEdit.Editing.TextArea.SelectionBorderProperty, binding);


 // SelectionCornerRadius

/*ValueStyleSetterGenerator*/
public static Style<T> SelectionCornerRadius<T>(this Style<T> style, System.Double value) where T : AvaloniaEdit.Editing.TextArea 
=> style._addSetter(AvaloniaEdit.Editing.TextArea.SelectionCornerRadiusProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> SelectionCornerRadius<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Editing.TextArea 
=> style._addSetter(AvaloniaEdit.Editing.TextArea.SelectionCornerRadiusProperty, binding);


 // RightClickMovesCaret

/*ValueStyleSetterGenerator*/
public static Style<T> RightClickMovesCaret<T>(this Style<T> style, System.Boolean value) where T : AvaloniaEdit.Editing.TextArea 
=> style._addSetter(AvaloniaEdit.Editing.TextArea.RightClickMovesCaretProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> RightClickMovesCaret<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Editing.TextArea 
=> style._addSetter(AvaloniaEdit.Editing.TextArea.RightClickMovesCaretProperty, binding);


 // IndentationStrategy

/*ValueStyleSetterGenerator*/
public static Style<T> IndentationStrategy<T>(this Style<T> style, AvaloniaEdit.Indentation.IIndentationStrategy value) where T : AvaloniaEdit.Editing.TextArea 
=> style._addSetter(AvaloniaEdit.Editing.TextArea.IndentationStrategyProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> IndentationStrategy<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Editing.TextArea 
=> style._addSetter(AvaloniaEdit.Editing.TextArea.IndentationStrategyProperty, binding);


 // OverstrikeMode

/*ValueStyleSetterGenerator*/
public static Style<T> OverstrikeMode<T>(this Style<T> style, System.Boolean value) where T : AvaloniaEdit.Editing.TextArea 
=> style._addSetter(AvaloniaEdit.Editing.TextArea.OverstrikeModeProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> OverstrikeMode<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Editing.TextArea 
=> style._addSetter(AvaloniaEdit.Editing.TextArea.OverstrikeModeProperty, binding);



}
