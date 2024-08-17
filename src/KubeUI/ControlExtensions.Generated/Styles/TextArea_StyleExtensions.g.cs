using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Indentation;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TextArea = AvaloniaEdit.Editing.TextArea;

namespace Avalonia.Markup.Declarative;
public static partial class TextAreaExtensions
{
public static Style<T> Document<T>(this Style<T> style, AvaloniaEdit.Document.TextDocument value) where T : AvaloniaEdit.Editing.TextArea
=> style._addSetter(AvaloniaEdit.Editing.TextArea.DocumentProperty, value);
public static Style<T> Document<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Editing.TextArea
=> style._addSetter(AvaloniaEdit.Editing.TextArea.DocumentProperty, binding);
public static Style<T> Options<T>(this Style<T> style, AvaloniaEdit.TextEditorOptions value) where T : AvaloniaEdit.Editing.TextArea
=> style._addSetter(AvaloniaEdit.Editing.TextArea.OptionsProperty, value);
public static Style<T> Options<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Editing.TextArea
=> style._addSetter(AvaloniaEdit.Editing.TextArea.OptionsProperty, binding);
public static Style<T> SelectionBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : AvaloniaEdit.Editing.TextArea
=> style._addSetter(AvaloniaEdit.Editing.TextArea.SelectionBrushProperty, value);
public static Style<T> SelectionBrush<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Editing.TextArea
=> style._addSetter(AvaloniaEdit.Editing.TextArea.SelectionBrushProperty, binding);
public static Style<T> SelectionForeground<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : AvaloniaEdit.Editing.TextArea
=> style._addSetter(AvaloniaEdit.Editing.TextArea.SelectionForegroundProperty, value);
public static Style<T> SelectionForeground<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Editing.TextArea
=> style._addSetter(AvaloniaEdit.Editing.TextArea.SelectionForegroundProperty, binding);
public static Style<T> SelectionBorder<T>(this Style<T> style, Avalonia.Media.Pen value) where T : AvaloniaEdit.Editing.TextArea
=> style._addSetter(AvaloniaEdit.Editing.TextArea.SelectionBorderProperty, value);
public static Style<T> SelectionBorder<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Editing.TextArea
=> style._addSetter(AvaloniaEdit.Editing.TextArea.SelectionBorderProperty, binding);
public static Style<T> SelectionCornerRadius<T>(this Style<T> style, System.Double value) where T : AvaloniaEdit.Editing.TextArea
=> style._addSetter(AvaloniaEdit.Editing.TextArea.SelectionCornerRadiusProperty, value);
public static Style<T> SelectionCornerRadius<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Editing.TextArea
=> style._addSetter(AvaloniaEdit.Editing.TextArea.SelectionCornerRadiusProperty, binding);
public static Style<T> RightClickMovesCaret<T>(this Style<T> style, System.Boolean value) where T : AvaloniaEdit.Editing.TextArea
=> style._addSetter(AvaloniaEdit.Editing.TextArea.RightClickMovesCaretProperty, value);
public static Style<T> RightClickMovesCaret<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Editing.TextArea
=> style._addSetter(AvaloniaEdit.Editing.TextArea.RightClickMovesCaretProperty, binding);
public static Style<T> IndentationStrategy<T>(this Style<T> style, AvaloniaEdit.Indentation.IIndentationStrategy value) where T : AvaloniaEdit.Editing.TextArea
=> style._addSetter(AvaloniaEdit.Editing.TextArea.IndentationStrategyProperty, value);
public static Style<T> IndentationStrategy<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Editing.TextArea
=> style._addSetter(AvaloniaEdit.Editing.TextArea.IndentationStrategyProperty, binding);
public static Style<T> OverstrikeMode<T>(this Style<T> style, System.Boolean value) where T : AvaloniaEdit.Editing.TextArea
=> style._addSetter(AvaloniaEdit.Editing.TextArea.OverstrikeModeProperty, value);
public static Style<T> OverstrikeMode<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Editing.TextArea
=> style._addSetter(AvaloniaEdit.Editing.TextArea.OverstrikeModeProperty, binding);
}

