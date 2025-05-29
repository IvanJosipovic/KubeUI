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
public static partial class TextEditor_MarkupExtensions
{
//================= Properties ======================//
 // Document

/*BindFromExpressionSetterGenerator*/
public static T Document<T>(this T control, Func<AvaloniaEdit.Document.TextDocument> func, Action<AvaloniaEdit.Document.TextDocument>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.TextEditor 
   => control._set(AvaloniaEdit.TextEditor.DocumentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Document<T>(this T control,AvaloniaEdit.Document.TextDocument value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor 
=> control._setEx(AvaloniaEdit.TextEditor.DocumentProperty, ps, () => control.Document = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Document<T>(this T control, IBinding binding) where T : AvaloniaEdit.TextEditor 
   => control._set(AvaloniaEdit.TextEditor.DocumentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Document<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.TextEditor 
   => control._set(AvaloniaEdit.TextEditor.DocumentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Document<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, AvaloniaEdit.Document.TextDocument> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor 
=> control._setEx(AvaloniaEdit.TextEditor.DocumentProperty, ps, () => control.Document = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // Options

/*BindFromExpressionSetterGenerator*/
public static T Options<T>(this T control, Func<AvaloniaEdit.TextEditorOptions> func, Action<AvaloniaEdit.TextEditorOptions>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.TextEditor 
   => control._set(AvaloniaEdit.TextEditor.OptionsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Options<T>(this T control,AvaloniaEdit.TextEditorOptions value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor 
=> control._setEx(AvaloniaEdit.TextEditor.OptionsProperty, ps, () => control.Options = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Options<T>(this T control, IBinding binding) where T : AvaloniaEdit.TextEditor 
   => control._set(AvaloniaEdit.TextEditor.OptionsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Options<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.TextEditor 
   => control._set(AvaloniaEdit.TextEditor.OptionsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Options<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, AvaloniaEdit.TextEditorOptions> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor 
=> control._setEx(AvaloniaEdit.TextEditor.OptionsProperty, ps, () => control.Options = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SyntaxHighlighting

/*BindFromExpressionSetterGenerator*/
public static T SyntaxHighlighting<T>(this T control, Func<AvaloniaEdit.Highlighting.IHighlightingDefinition> func, Action<AvaloniaEdit.Highlighting.IHighlightingDefinition>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.TextEditor 
   => control._set(AvaloniaEdit.TextEditor.SyntaxHighlightingProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SyntaxHighlighting<T>(this T control,AvaloniaEdit.Highlighting.IHighlightingDefinition value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor 
=> control._setEx(AvaloniaEdit.TextEditor.SyntaxHighlightingProperty, ps, () => control.SyntaxHighlighting = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SyntaxHighlighting<T>(this T control, IBinding binding) where T : AvaloniaEdit.TextEditor 
   => control._set(AvaloniaEdit.TextEditor.SyntaxHighlightingProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SyntaxHighlighting<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.TextEditor 
   => control._set(AvaloniaEdit.TextEditor.SyntaxHighlightingProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SyntaxHighlighting<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, AvaloniaEdit.Highlighting.IHighlightingDefinition> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor 
=> control._setEx(AvaloniaEdit.TextEditor.SyntaxHighlightingProperty, ps, () => control.SyntaxHighlighting = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // WordWrap

/*BindFromExpressionSetterGenerator*/
public static T WordWrap<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.TextEditor 
   => control._set(AvaloniaEdit.TextEditor.WordWrapProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T WordWrap<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor 
=> control._setEx(AvaloniaEdit.TextEditor.WordWrapProperty, ps, () => control.WordWrap = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T WordWrap<T>(this T control, IBinding binding) where T : AvaloniaEdit.TextEditor 
   => control._set(AvaloniaEdit.TextEditor.WordWrapProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T WordWrap<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.TextEditor 
   => control._set(AvaloniaEdit.TextEditor.WordWrapProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T WordWrap<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor 
=> control._setEx(AvaloniaEdit.TextEditor.WordWrapProperty, ps, () => control.WordWrap = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsReadOnly

/*BindFromExpressionSetterGenerator*/
public static T IsReadOnly<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.TextEditor 
   => control._set(AvaloniaEdit.TextEditor.IsReadOnlyProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsReadOnly<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor 
=> control._setEx(AvaloniaEdit.TextEditor.IsReadOnlyProperty, ps, () => control.IsReadOnly = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsReadOnly<T>(this T control, IBinding binding) where T : AvaloniaEdit.TextEditor 
   => control._set(AvaloniaEdit.TextEditor.IsReadOnlyProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsReadOnly<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.TextEditor 
   => control._set(AvaloniaEdit.TextEditor.IsReadOnlyProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsReadOnly<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor 
=> control._setEx(AvaloniaEdit.TextEditor.IsReadOnlyProperty, ps, () => control.IsReadOnly = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsModified

/*BindFromExpressionSetterGenerator*/
public static T IsModified<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.TextEditor 
   => control._set(AvaloniaEdit.TextEditor.IsModifiedProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsModified<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor 
=> control._setEx(AvaloniaEdit.TextEditor.IsModifiedProperty, ps, () => control.IsModified = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsModified<T>(this T control, IBinding binding) where T : AvaloniaEdit.TextEditor 
   => control._set(AvaloniaEdit.TextEditor.IsModifiedProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsModified<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.TextEditor 
   => control._set(AvaloniaEdit.TextEditor.IsModifiedProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsModified<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor 
=> control._setEx(AvaloniaEdit.TextEditor.IsModifiedProperty, ps, () => control.IsModified = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ShowLineNumbers

/*BindFromExpressionSetterGenerator*/
public static T ShowLineNumbers<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.TextEditor 
   => control._set(AvaloniaEdit.TextEditor.ShowLineNumbersProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ShowLineNumbers<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor 
=> control._setEx(AvaloniaEdit.TextEditor.ShowLineNumbersProperty, ps, () => control.ShowLineNumbers = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ShowLineNumbers<T>(this T control, IBinding binding) where T : AvaloniaEdit.TextEditor 
   => control._set(AvaloniaEdit.TextEditor.ShowLineNumbersProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ShowLineNumbers<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.TextEditor 
   => control._set(AvaloniaEdit.TextEditor.ShowLineNumbersProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ShowLineNumbers<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor 
=> control._setEx(AvaloniaEdit.TextEditor.ShowLineNumbersProperty, ps, () => control.ShowLineNumbers = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SearchResultsBrush

/*BindFromExpressionSetterGenerator*/
public static T SearchResultsBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.TextEditor 
   => control._set(AvaloniaEdit.TextEditor.SearchResultsBrushProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SearchResultsBrush<T>(this T control,Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor 
=> control._setEx(AvaloniaEdit.TextEditor.SearchResultsBrushProperty, ps, () => control.SearchResultsBrush = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SearchResultsBrush<T>(this T control, IBinding binding) where T : AvaloniaEdit.TextEditor 
   => control._set(AvaloniaEdit.TextEditor.SearchResultsBrushProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SearchResultsBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.TextEditor 
   => control._set(AvaloniaEdit.TextEditor.SearchResultsBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SearchResultsBrush<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor 
=> control._setEx(AvaloniaEdit.TextEditor.SearchResultsBrushProperty, ps, () => control.SearchResultsBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // LineNumbersForeground

/*BindFromExpressionSetterGenerator*/
public static T LineNumbersForeground<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.TextEditor 
   => control._set(AvaloniaEdit.TextEditor.LineNumbersForegroundProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T LineNumbersForeground<T>(this T control,Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor 
=> control._setEx(AvaloniaEdit.TextEditor.LineNumbersForegroundProperty, ps, () => control.LineNumbersForeground = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T LineNumbersForeground<T>(this T control, IBinding binding) where T : AvaloniaEdit.TextEditor 
   => control._set(AvaloniaEdit.TextEditor.LineNumbersForegroundProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T LineNumbersForeground<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.TextEditor 
   => control._set(AvaloniaEdit.TextEditor.LineNumbersForegroundProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T LineNumbersForeground<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor 
=> control._setEx(AvaloniaEdit.TextEditor.LineNumbersForegroundProperty, ps, () => control.LineNumbersForeground = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // Encoding

/*BindFromExpressionSetterGenerator*/
public static T Encoding<T>(this T control, Func<System.Text.Encoding> func, Action<System.Text.Encoding>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.TextEditor 
   => control._set(AvaloniaEdit.TextEditor.EncodingProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Encoding<T>(this T control,System.Text.Encoding value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor 
=> control._setEx(AvaloniaEdit.TextEditor.EncodingProperty, ps, () => control.Encoding = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Encoding<T>(this T control, IBinding binding) where T : AvaloniaEdit.TextEditor 
   => control._set(AvaloniaEdit.TextEditor.EncodingProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Encoding<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.TextEditor 
   => control._set(AvaloniaEdit.TextEditor.EncodingProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Encoding<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Text.Encoding> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor 
=> control._setEx(AvaloniaEdit.TextEditor.EncodingProperty, ps, () => control.Encoding = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // HorizontalScrollBarVisibility

/*BindFromExpressionSetterGenerator*/
public static T HorizontalScrollBarVisibility<T>(this T control, Func<Avalonia.Controls.Primitives.ScrollBarVisibility> func, Action<Avalonia.Controls.Primitives.ScrollBarVisibility>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.TextEditor 
   => control._set(AvaloniaEdit.TextEditor.HorizontalScrollBarVisibilityProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T HorizontalScrollBarVisibility<T>(this T control,Avalonia.Controls.Primitives.ScrollBarVisibility value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor 
=> control._setEx(AvaloniaEdit.TextEditor.HorizontalScrollBarVisibilityProperty, ps, () => control.HorizontalScrollBarVisibility = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T HorizontalScrollBarVisibility<T>(this T control, IBinding binding) where T : AvaloniaEdit.TextEditor 
   => control._set(AvaloniaEdit.TextEditor.HorizontalScrollBarVisibilityProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T HorizontalScrollBarVisibility<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.TextEditor 
   => control._set(AvaloniaEdit.TextEditor.HorizontalScrollBarVisibilityProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T HorizontalScrollBarVisibility<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Primitives.ScrollBarVisibility> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor 
=> control._setEx(AvaloniaEdit.TextEditor.HorizontalScrollBarVisibilityProperty, ps, () => control.HorizontalScrollBarVisibility = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // VerticalScrollBarVisibility

/*BindFromExpressionSetterGenerator*/
public static T VerticalScrollBarVisibility<T>(this T control, Func<Avalonia.Controls.Primitives.ScrollBarVisibility> func, Action<Avalonia.Controls.Primitives.ScrollBarVisibility>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.TextEditor 
   => control._set(AvaloniaEdit.TextEditor.VerticalScrollBarVisibilityProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T VerticalScrollBarVisibility<T>(this T control,Avalonia.Controls.Primitives.ScrollBarVisibility value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor 
=> control._setEx(AvaloniaEdit.TextEditor.VerticalScrollBarVisibilityProperty, ps, () => control.VerticalScrollBarVisibility = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T VerticalScrollBarVisibility<T>(this T control, IBinding binding) where T : AvaloniaEdit.TextEditor 
   => control._set(AvaloniaEdit.TextEditor.VerticalScrollBarVisibilityProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T VerticalScrollBarVisibility<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.TextEditor 
   => control._set(AvaloniaEdit.TextEditor.VerticalScrollBarVisibilityProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T VerticalScrollBarVisibility<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Primitives.ScrollBarVisibility> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor 
=> control._setEx(AvaloniaEdit.TextEditor.VerticalScrollBarVisibilityProperty, ps, () => control.VerticalScrollBarVisibility = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // DocumentChanged

/*ActionToEventGenerator*/
public static T OnDocumentChanged<T>(this T control, Action<AvaloniaEdit.Document.DocumentChangedEventArgs> action) where T : AvaloniaEdit.TextEditor  => 
 control._setEvent((System.EventHandler<AvaloniaEdit.Document.DocumentChangedEventArgs>) ((arg0, arg1) => action(arg1)), h => control.DocumentChanged += h);


 // OptionChanged

/*ActionToEventGenerator*/
public static T OnOptionChanged<T>(this T control, Action<System.ComponentModel.PropertyChangedEventArgs> action) where T : AvaloniaEdit.TextEditor  => 
 control._setEvent((System.ComponentModel.PropertyChangedEventHandler) ((arg0, arg1) => action(arg1)), h => control.OptionChanged += h);


 // TextChanged

/*ActionToEventGenerator*/
public static T OnTextChanged<T>(this T control, Action<System.EventArgs> action) where T : AvaloniaEdit.TextEditor  => 
 control._setEvent((System.EventHandler) ((arg0, arg1) => action(arg1)), h => control.TextChanged += h);


 // PreviewPointerHover

/*ActionToEventGenerator*/
public static T OnPreviewPointerHover<T>(this T control, Action<Avalonia.Input.PointerEventArgs> action, Avalonia.Interactivity.RoutingStrategies routes = Avalonia.Interactivity.RoutingStrategies.Tunnel | Avalonia.Interactivity.RoutingStrategies.Bubble) where T : AvaloniaEdit.TextEditor 
{
  control.AddHandler(AvaloniaEdit.TextEditor.PreviewPointerHoverEvent, (_, args) => action(args), routes);
  return control; 
}



 // PointerHover

/*ActionToEventGenerator*/
public static T OnPointerHover<T>(this T control, Action<Avalonia.Input.PointerEventArgs> action, Avalonia.Interactivity.RoutingStrategies routes = Avalonia.Interactivity.RoutingStrategies.Tunnel | Avalonia.Interactivity.RoutingStrategies.Bubble) where T : AvaloniaEdit.TextEditor 
{
  control.AddHandler(AvaloniaEdit.TextEditor.PointerHoverEvent, (_, args) => action(args), routes);
  return control; 
}



 // PreviewPointerHoverStopped

/*ActionToEventGenerator*/
public static T OnPreviewPointerHoverStopped<T>(this T control, Action<Avalonia.Input.PointerEventArgs> action, Avalonia.Interactivity.RoutingStrategies routes = Avalonia.Interactivity.RoutingStrategies.Tunnel | Avalonia.Interactivity.RoutingStrategies.Bubble) where T : AvaloniaEdit.TextEditor 
{
  control.AddHandler(AvaloniaEdit.TextEditor.PreviewPointerHoverStoppedEvent, (_, args) => action(args), routes);
  return control; 
}



 // PointerHoverStopped

/*ActionToEventGenerator*/
public static T OnPointerHoverStopped<T>(this T control, Action<Avalonia.Input.PointerEventArgs> action, Avalonia.Interactivity.RoutingStrategies routes = Avalonia.Interactivity.RoutingStrategies.Tunnel | Avalonia.Interactivity.RoutingStrategies.Bubble) where T : AvaloniaEdit.TextEditor 
{
  control.AddHandler(AvaloniaEdit.TextEditor.PointerHoverStoppedEvent, (_, args) => action(args), routes);
  return control; 
}




//================= Styles ======================//
 // Document

/*ValueStyleSetterGenerator*/
public static Style<T> Document<T>(this Style<T> style, AvaloniaEdit.Document.TextDocument value) where T : AvaloniaEdit.TextEditor 
=> style._addSetter(AvaloniaEdit.TextEditor.DocumentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Document<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.TextEditor 
=> style._addSetter(AvaloniaEdit.TextEditor.DocumentProperty, binding);


 // Options

/*ValueStyleSetterGenerator*/
public static Style<T> Options<T>(this Style<T> style, AvaloniaEdit.TextEditorOptions value) where T : AvaloniaEdit.TextEditor 
=> style._addSetter(AvaloniaEdit.TextEditor.OptionsProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Options<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.TextEditor 
=> style._addSetter(AvaloniaEdit.TextEditor.OptionsProperty, binding);


 // SyntaxHighlighting

/*ValueStyleSetterGenerator*/
public static Style<T> SyntaxHighlighting<T>(this Style<T> style, AvaloniaEdit.Highlighting.IHighlightingDefinition value) where T : AvaloniaEdit.TextEditor 
=> style._addSetter(AvaloniaEdit.TextEditor.SyntaxHighlightingProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SyntaxHighlighting<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.TextEditor 
=> style._addSetter(AvaloniaEdit.TextEditor.SyntaxHighlightingProperty, binding);


 // WordWrap

/*ValueStyleSetterGenerator*/
public static Style<T> WordWrap<T>(this Style<T> style, System.Boolean value) where T : AvaloniaEdit.TextEditor 
=> style._addSetter(AvaloniaEdit.TextEditor.WordWrapProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> WordWrap<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.TextEditor 
=> style._addSetter(AvaloniaEdit.TextEditor.WordWrapProperty, binding);


 // IsReadOnly

/*ValueStyleSetterGenerator*/
public static Style<T> IsReadOnly<T>(this Style<T> style, System.Boolean value) where T : AvaloniaEdit.TextEditor 
=> style._addSetter(AvaloniaEdit.TextEditor.IsReadOnlyProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsReadOnly<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.TextEditor 
=> style._addSetter(AvaloniaEdit.TextEditor.IsReadOnlyProperty, binding);


 // IsModified

/*ValueStyleSetterGenerator*/
public static Style<T> IsModified<T>(this Style<T> style, System.Boolean value) where T : AvaloniaEdit.TextEditor 
=> style._addSetter(AvaloniaEdit.TextEditor.IsModifiedProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsModified<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.TextEditor 
=> style._addSetter(AvaloniaEdit.TextEditor.IsModifiedProperty, binding);


 // ShowLineNumbers

/*ValueStyleSetterGenerator*/
public static Style<T> ShowLineNumbers<T>(this Style<T> style, System.Boolean value) where T : AvaloniaEdit.TextEditor 
=> style._addSetter(AvaloniaEdit.TextEditor.ShowLineNumbersProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ShowLineNumbers<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.TextEditor 
=> style._addSetter(AvaloniaEdit.TextEditor.ShowLineNumbersProperty, binding);


 // SearchResultsBrush

/*ValueStyleSetterGenerator*/
public static Style<T> SearchResultsBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : AvaloniaEdit.TextEditor 
=> style._addSetter(AvaloniaEdit.TextEditor.SearchResultsBrushProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SearchResultsBrush<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.TextEditor 
=> style._addSetter(AvaloniaEdit.TextEditor.SearchResultsBrushProperty, binding);


 // LineNumbersForeground

/*ValueStyleSetterGenerator*/
public static Style<T> LineNumbersForeground<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : AvaloniaEdit.TextEditor 
=> style._addSetter(AvaloniaEdit.TextEditor.LineNumbersForegroundProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> LineNumbersForeground<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.TextEditor 
=> style._addSetter(AvaloniaEdit.TextEditor.LineNumbersForegroundProperty, binding);


 // Encoding

/*ValueStyleSetterGenerator*/
public static Style<T> Encoding<T>(this Style<T> style, System.Text.Encoding value) where T : AvaloniaEdit.TextEditor 
=> style._addSetter(AvaloniaEdit.TextEditor.EncodingProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Encoding<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.TextEditor 
=> style._addSetter(AvaloniaEdit.TextEditor.EncodingProperty, binding);


 // HorizontalScrollBarVisibility

/*ValueStyleSetterGenerator*/
public static Style<T> HorizontalScrollBarVisibility<T>(this Style<T> style, Avalonia.Controls.Primitives.ScrollBarVisibility value) where T : AvaloniaEdit.TextEditor 
=> style._addSetter(AvaloniaEdit.TextEditor.HorizontalScrollBarVisibilityProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> HorizontalScrollBarVisibility<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.TextEditor 
=> style._addSetter(AvaloniaEdit.TextEditor.HorizontalScrollBarVisibilityProperty, binding);


 // VerticalScrollBarVisibility

/*ValueStyleSetterGenerator*/
public static Style<T> VerticalScrollBarVisibility<T>(this Style<T> style, Avalonia.Controls.Primitives.ScrollBarVisibility value) where T : AvaloniaEdit.TextEditor 
=> style._addSetter(AvaloniaEdit.TextEditor.VerticalScrollBarVisibilityProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> VerticalScrollBarVisibility<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.TextEditor 
=> style._addSetter(AvaloniaEdit.TextEditor.VerticalScrollBarVisibilityProperty, binding);



}
