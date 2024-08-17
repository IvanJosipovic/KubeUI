using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TextView = AvaloniaEdit.Rendering.TextView;

namespace Avalonia.Markup.Declarative;
public static partial class TextViewExtensions
{
public static Style<T> Document<T>(this Style<T> style, AvaloniaEdit.Document.TextDocument value) where T : AvaloniaEdit.Rendering.TextView
=> style._addSetter(AvaloniaEdit.Rendering.TextView.DocumentProperty, value);
public static Style<T> Document<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Rendering.TextView
=> style._addSetter(AvaloniaEdit.Rendering.TextView.DocumentProperty, binding);
public static Style<T> Options<T>(this Style<T> style, AvaloniaEdit.TextEditorOptions value) where T : AvaloniaEdit.Rendering.TextView
=> style._addSetter(AvaloniaEdit.Rendering.TextView.OptionsProperty, value);
public static Style<T> Options<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Rendering.TextView
=> style._addSetter(AvaloniaEdit.Rendering.TextView.OptionsProperty, binding);
public static Style<T> NonPrintableCharacterBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : AvaloniaEdit.Rendering.TextView
=> style._addSetter(AvaloniaEdit.Rendering.TextView.NonPrintableCharacterBrushProperty, value);
public static Style<T> NonPrintableCharacterBrush<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Rendering.TextView
=> style._addSetter(AvaloniaEdit.Rendering.TextView.NonPrintableCharacterBrushProperty, binding);
public static Style<T> LinkTextForegroundBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : AvaloniaEdit.Rendering.TextView
=> style._addSetter(AvaloniaEdit.Rendering.TextView.LinkTextForegroundBrushProperty, value);
public static Style<T> LinkTextForegroundBrush<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Rendering.TextView
=> style._addSetter(AvaloniaEdit.Rendering.TextView.LinkTextForegroundBrushProperty, binding);
public static Style<T> LinkTextBackgroundBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : AvaloniaEdit.Rendering.TextView
=> style._addSetter(AvaloniaEdit.Rendering.TextView.LinkTextBackgroundBrushProperty, value);
public static Style<T> LinkTextBackgroundBrush<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Rendering.TextView
=> style._addSetter(AvaloniaEdit.Rendering.TextView.LinkTextBackgroundBrushProperty, binding);
public static Style<T> LinkTextUnderline<T>(this Style<T> style, System.Boolean value) where T : AvaloniaEdit.Rendering.TextView
=> style._addSetter(AvaloniaEdit.Rendering.TextView.LinkTextUnderlineProperty, value);
public static Style<T> LinkTextUnderline<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Rendering.TextView
=> style._addSetter(AvaloniaEdit.Rendering.TextView.LinkTextUnderlineProperty, binding);
public static Style<T> ColumnRulerPen<T>(this Style<T> style, Avalonia.Media.IPen value) where T : AvaloniaEdit.Rendering.TextView
=> style._addSetter(AvaloniaEdit.Rendering.TextView.ColumnRulerPenProperty, value);
public static Style<T> ColumnRulerPen<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Rendering.TextView
=> style._addSetter(AvaloniaEdit.Rendering.TextView.ColumnRulerPenProperty, binding);
public static Style<T> CurrentLineBackground<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : AvaloniaEdit.Rendering.TextView
=> style._addSetter(AvaloniaEdit.Rendering.TextView.CurrentLineBackgroundProperty, value);
public static Style<T> CurrentLineBackground<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Rendering.TextView
=> style._addSetter(AvaloniaEdit.Rendering.TextView.CurrentLineBackgroundProperty, binding);
public static Style<T> CurrentLineBorder<T>(this Style<T> style, Avalonia.Media.IPen value) where T : AvaloniaEdit.Rendering.TextView
=> style._addSetter(AvaloniaEdit.Rendering.TextView.CurrentLineBorderProperty, value);
public static Style<T> CurrentLineBorder<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Rendering.TextView
=> style._addSetter(AvaloniaEdit.Rendering.TextView.CurrentLineBorderProperty, binding);
}

