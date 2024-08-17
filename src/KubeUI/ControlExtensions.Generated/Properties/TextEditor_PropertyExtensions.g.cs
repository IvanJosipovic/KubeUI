#nullable enable
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Highlighting;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using TextEditor = AvaloniaEdit.TextEditor;

namespace Avalonia.Markup.Declarative;
public static partial class TextEditorExtensions
{
public static T Document<T>(this T control, IBinding binding) where T : AvaloniaEdit.TextEditor
   => control._set(AvaloniaEdit.TextEditor.DocumentProperty, binding);
public static T Document<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.TextEditor
   => control._set(AvaloniaEdit.TextEditor.DocumentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Document<T>(this T control, Func<AvaloniaEdit.Document.TextDocument> func, Action<AvaloniaEdit.Document.TextDocument>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.TextEditor
   => control._set(AvaloniaEdit.TextEditor.DocumentProperty, func, onChanged, expression);
public static T Document<T>(this T control, AvaloniaEdit.Document.TextDocument value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor
=> control._setEx(AvaloniaEdit.TextEditor.DocumentProperty, ps, () => control.Document = value, bindingMode, converter, bindingSource);
public static T Document<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, AvaloniaEdit.Document.TextDocument> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor
=> control._setEx(AvaloniaEdit.TextEditor.DocumentProperty, ps, () => control.Document = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Options<T>(this T control, IBinding binding) where T : AvaloniaEdit.TextEditor
   => control._set(AvaloniaEdit.TextEditor.OptionsProperty, binding);
public static T Options<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.TextEditor
   => control._set(AvaloniaEdit.TextEditor.OptionsProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Options<T>(this T control, Func<AvaloniaEdit.TextEditorOptions> func, Action<AvaloniaEdit.TextEditorOptions>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.TextEditor
   => control._set(AvaloniaEdit.TextEditor.OptionsProperty, func, onChanged, expression);
public static T Options<T>(this T control, AvaloniaEdit.TextEditorOptions value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor
=> control._setEx(AvaloniaEdit.TextEditor.OptionsProperty, ps, () => control.Options = value, bindingMode, converter, bindingSource);
public static T Options<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, AvaloniaEdit.TextEditorOptions> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor
=> control._setEx(AvaloniaEdit.TextEditor.OptionsProperty, ps, () => control.Options = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SyntaxHighlighting<T>(this T control, IBinding binding) where T : AvaloniaEdit.TextEditor
   => control._set(AvaloniaEdit.TextEditor.SyntaxHighlightingProperty, binding);
public static T SyntaxHighlighting<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.TextEditor
   => control._set(AvaloniaEdit.TextEditor.SyntaxHighlightingProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SyntaxHighlighting<T>(this T control, Func<AvaloniaEdit.Highlighting.IHighlightingDefinition> func, Action<AvaloniaEdit.Highlighting.IHighlightingDefinition>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.TextEditor
   => control._set(AvaloniaEdit.TextEditor.SyntaxHighlightingProperty, func, onChanged, expression);
public static T SyntaxHighlighting<T>(this T control, AvaloniaEdit.Highlighting.IHighlightingDefinition value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor
=> control._setEx(AvaloniaEdit.TextEditor.SyntaxHighlightingProperty, ps, () => control.SyntaxHighlighting = value, bindingMode, converter, bindingSource);
public static T SyntaxHighlighting<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, AvaloniaEdit.Highlighting.IHighlightingDefinition> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor
=> control._setEx(AvaloniaEdit.TextEditor.SyntaxHighlightingProperty, ps, () => control.SyntaxHighlighting = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T WordWrap<T>(this T control, IBinding binding) where T : AvaloniaEdit.TextEditor
   => control._set(AvaloniaEdit.TextEditor.WordWrapProperty, binding);
public static T WordWrap<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.TextEditor
   => control._set(AvaloniaEdit.TextEditor.WordWrapProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T WordWrap<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.TextEditor
   => control._set(AvaloniaEdit.TextEditor.WordWrapProperty, func, onChanged, expression);
public static T WordWrap<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor
=> control._setEx(AvaloniaEdit.TextEditor.WordWrapProperty, ps, () => control.WordWrap = value, bindingMode, converter, bindingSource);
public static T WordWrap<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor
=> control._setEx(AvaloniaEdit.TextEditor.WordWrapProperty, ps, () => control.WordWrap = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsReadOnly<T>(this T control, IBinding binding) where T : AvaloniaEdit.TextEditor
   => control._set(AvaloniaEdit.TextEditor.IsReadOnlyProperty, binding);
public static T IsReadOnly<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.TextEditor
   => control._set(AvaloniaEdit.TextEditor.IsReadOnlyProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsReadOnly<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.TextEditor
   => control._set(AvaloniaEdit.TextEditor.IsReadOnlyProperty, func, onChanged, expression);
public static T IsReadOnly<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor
=> control._setEx(AvaloniaEdit.TextEditor.IsReadOnlyProperty, ps, () => control.IsReadOnly = value, bindingMode, converter, bindingSource);
public static T IsReadOnly<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor
=> control._setEx(AvaloniaEdit.TextEditor.IsReadOnlyProperty, ps, () => control.IsReadOnly = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsModified<T>(this T control, IBinding binding) where T : AvaloniaEdit.TextEditor
   => control._set(AvaloniaEdit.TextEditor.IsModifiedProperty, binding);
public static T IsModified<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.TextEditor
   => control._set(AvaloniaEdit.TextEditor.IsModifiedProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsModified<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.TextEditor
   => control._set(AvaloniaEdit.TextEditor.IsModifiedProperty, func, onChanged, expression);
public static T IsModified<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor
=> control._setEx(AvaloniaEdit.TextEditor.IsModifiedProperty, ps, () => control.IsModified = value, bindingMode, converter, bindingSource);
public static T IsModified<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor
=> control._setEx(AvaloniaEdit.TextEditor.IsModifiedProperty, ps, () => control.IsModified = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ShowLineNumbers<T>(this T control, IBinding binding) where T : AvaloniaEdit.TextEditor
   => control._set(AvaloniaEdit.TextEditor.ShowLineNumbersProperty, binding);
public static T ShowLineNumbers<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.TextEditor
   => control._set(AvaloniaEdit.TextEditor.ShowLineNumbersProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ShowLineNumbers<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.TextEditor
   => control._set(AvaloniaEdit.TextEditor.ShowLineNumbersProperty, func, onChanged, expression);
public static T ShowLineNumbers<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor
=> control._setEx(AvaloniaEdit.TextEditor.ShowLineNumbersProperty, ps, () => control.ShowLineNumbers = value, bindingMode, converter, bindingSource);
public static T ShowLineNumbers<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor
=> control._setEx(AvaloniaEdit.TextEditor.ShowLineNumbersProperty, ps, () => control.ShowLineNumbers = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SearchResultsBrush<T>(this T control, IBinding binding) where T : AvaloniaEdit.TextEditor
   => control._set(AvaloniaEdit.TextEditor.SearchResultsBrushProperty, binding);
public static T SearchResultsBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.TextEditor
   => control._set(AvaloniaEdit.TextEditor.SearchResultsBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SearchResultsBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.TextEditor
   => control._set(AvaloniaEdit.TextEditor.SearchResultsBrushProperty, func, onChanged, expression);
public static T SearchResultsBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor
=> control._setEx(AvaloniaEdit.TextEditor.SearchResultsBrushProperty, ps, () => control.SearchResultsBrush = value, bindingMode, converter, bindingSource);
public static T SearchResultsBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor
=> control._setEx(AvaloniaEdit.TextEditor.SearchResultsBrushProperty, ps, () => control.SearchResultsBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T LineNumbersForeground<T>(this T control, IBinding binding) where T : AvaloniaEdit.TextEditor
   => control._set(AvaloniaEdit.TextEditor.LineNumbersForegroundProperty, binding);
public static T LineNumbersForeground<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.TextEditor
   => control._set(AvaloniaEdit.TextEditor.LineNumbersForegroundProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T LineNumbersForeground<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.TextEditor
   => control._set(AvaloniaEdit.TextEditor.LineNumbersForegroundProperty, func, onChanged, expression);
public static T LineNumbersForeground<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor
=> control._setEx(AvaloniaEdit.TextEditor.LineNumbersForegroundProperty, ps, () => control.LineNumbersForeground = value, bindingMode, converter, bindingSource);
public static T LineNumbersForeground<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor
=> control._setEx(AvaloniaEdit.TextEditor.LineNumbersForegroundProperty, ps, () => control.LineNumbersForeground = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Encoding<T>(this T control, IBinding binding) where T : AvaloniaEdit.TextEditor
   => control._set(AvaloniaEdit.TextEditor.EncodingProperty, binding);
public static T Encoding<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.TextEditor
   => control._set(AvaloniaEdit.TextEditor.EncodingProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Encoding<T>(this T control, Func<System.Text.Encoding> func, Action<System.Text.Encoding>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.TextEditor
   => control._set(AvaloniaEdit.TextEditor.EncodingProperty, func, onChanged, expression);
public static T Encoding<T>(this T control, System.Text.Encoding value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor
=> control._setEx(AvaloniaEdit.TextEditor.EncodingProperty, ps, () => control.Encoding = value, bindingMode, converter, bindingSource);
public static T Encoding<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Text.Encoding> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor
=> control._setEx(AvaloniaEdit.TextEditor.EncodingProperty, ps, () => control.Encoding = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T HorizontalScrollBarVisibility<T>(this T control, IBinding binding) where T : AvaloniaEdit.TextEditor
   => control._set(AvaloniaEdit.TextEditor.HorizontalScrollBarVisibilityProperty, binding);
public static T HorizontalScrollBarVisibility<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.TextEditor
   => control._set(AvaloniaEdit.TextEditor.HorizontalScrollBarVisibilityProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T HorizontalScrollBarVisibility<T>(this T control, Func<Avalonia.Controls.Primitives.ScrollBarVisibility> func, Action<Avalonia.Controls.Primitives.ScrollBarVisibility>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.TextEditor
   => control._set(AvaloniaEdit.TextEditor.HorizontalScrollBarVisibilityProperty, func, onChanged, expression);
public static T HorizontalScrollBarVisibility<T>(this T control, Avalonia.Controls.Primitives.ScrollBarVisibility value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor
=> control._setEx(AvaloniaEdit.TextEditor.HorizontalScrollBarVisibilityProperty, ps, () => control.HorizontalScrollBarVisibility = value, bindingMode, converter, bindingSource);
public static T HorizontalScrollBarVisibility<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Primitives.ScrollBarVisibility> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor
=> control._setEx(AvaloniaEdit.TextEditor.HorizontalScrollBarVisibilityProperty, ps, () => control.HorizontalScrollBarVisibility = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T VerticalScrollBarVisibility<T>(this T control, IBinding binding) where T : AvaloniaEdit.TextEditor
   => control._set(AvaloniaEdit.TextEditor.VerticalScrollBarVisibilityProperty, binding);
public static T VerticalScrollBarVisibility<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.TextEditor
   => control._set(AvaloniaEdit.TextEditor.VerticalScrollBarVisibilityProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T VerticalScrollBarVisibility<T>(this T control, Func<Avalonia.Controls.Primitives.ScrollBarVisibility> func, Action<Avalonia.Controls.Primitives.ScrollBarVisibility>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.TextEditor
   => control._set(AvaloniaEdit.TextEditor.VerticalScrollBarVisibilityProperty, func, onChanged, expression);
public static T VerticalScrollBarVisibility<T>(this T control, Avalonia.Controls.Primitives.ScrollBarVisibility value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor
=> control._setEx(AvaloniaEdit.TextEditor.VerticalScrollBarVisibilityProperty, ps, () => control.VerticalScrollBarVisibility = value, bindingMode, converter, bindingSource);
public static T VerticalScrollBarVisibility<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Primitives.ScrollBarVisibility> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.TextEditor
=> control._setEx(AvaloniaEdit.TextEditor.VerticalScrollBarVisibilityProperty, ps, () => control.VerticalScrollBarVisibility = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

