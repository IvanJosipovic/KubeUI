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
public static Style<T> Document<T>(this Style<T> style, AvaloniaEdit.Document.TextDocument value) where T : AvaloniaEdit.TextEditor
=> style._addSetter(AvaloniaEdit.TextEditor.DocumentProperty, value);
public static Style<T> Document<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.TextEditor
=> style._addSetter(AvaloniaEdit.TextEditor.DocumentProperty, binding);
public static Style<T> Options<T>(this Style<T> style, AvaloniaEdit.TextEditorOptions value) where T : AvaloniaEdit.TextEditor
=> style._addSetter(AvaloniaEdit.TextEditor.OptionsProperty, value);
public static Style<T> Options<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.TextEditor
=> style._addSetter(AvaloniaEdit.TextEditor.OptionsProperty, binding);
public static Style<T> SyntaxHighlighting<T>(this Style<T> style, AvaloniaEdit.Highlighting.IHighlightingDefinition value) where T : AvaloniaEdit.TextEditor
=> style._addSetter(AvaloniaEdit.TextEditor.SyntaxHighlightingProperty, value);
public static Style<T> SyntaxHighlighting<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.TextEditor
=> style._addSetter(AvaloniaEdit.TextEditor.SyntaxHighlightingProperty, binding);
public static Style<T> WordWrap<T>(this Style<T> style, System.Boolean value) where T : AvaloniaEdit.TextEditor
=> style._addSetter(AvaloniaEdit.TextEditor.WordWrapProperty, value);
public static Style<T> WordWrap<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.TextEditor
=> style._addSetter(AvaloniaEdit.TextEditor.WordWrapProperty, binding);
public static Style<T> IsReadOnly<T>(this Style<T> style, System.Boolean value) where T : AvaloniaEdit.TextEditor
=> style._addSetter(AvaloniaEdit.TextEditor.IsReadOnlyProperty, value);
public static Style<T> IsReadOnly<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.TextEditor
=> style._addSetter(AvaloniaEdit.TextEditor.IsReadOnlyProperty, binding);
public static Style<T> IsModified<T>(this Style<T> style, System.Boolean value) where T : AvaloniaEdit.TextEditor
=> style._addSetter(AvaloniaEdit.TextEditor.IsModifiedProperty, value);
public static Style<T> IsModified<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.TextEditor
=> style._addSetter(AvaloniaEdit.TextEditor.IsModifiedProperty, binding);
public static Style<T> ShowLineNumbers<T>(this Style<T> style, System.Boolean value) where T : AvaloniaEdit.TextEditor
=> style._addSetter(AvaloniaEdit.TextEditor.ShowLineNumbersProperty, value);
public static Style<T> ShowLineNumbers<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.TextEditor
=> style._addSetter(AvaloniaEdit.TextEditor.ShowLineNumbersProperty, binding);
public static Style<T> SearchResultsBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : AvaloniaEdit.TextEditor
=> style._addSetter(AvaloniaEdit.TextEditor.SearchResultsBrushProperty, value);
public static Style<T> SearchResultsBrush<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.TextEditor
=> style._addSetter(AvaloniaEdit.TextEditor.SearchResultsBrushProperty, binding);
public static Style<T> LineNumbersForeground<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : AvaloniaEdit.TextEditor
=> style._addSetter(AvaloniaEdit.TextEditor.LineNumbersForegroundProperty, value);
public static Style<T> LineNumbersForeground<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.TextEditor
=> style._addSetter(AvaloniaEdit.TextEditor.LineNumbersForegroundProperty, binding);
public static Style<T> Encoding<T>(this Style<T> style, System.Text.Encoding value) where T : AvaloniaEdit.TextEditor
=> style._addSetter(AvaloniaEdit.TextEditor.EncodingProperty, value);
public static Style<T> Encoding<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.TextEditor
=> style._addSetter(AvaloniaEdit.TextEditor.EncodingProperty, binding);
}

