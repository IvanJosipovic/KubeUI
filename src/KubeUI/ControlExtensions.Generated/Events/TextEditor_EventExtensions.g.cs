using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TextEditor = AvaloniaEdit.TextEditor;

namespace Avalonia.Markup.Declarative;
public static partial class TextEditorEventsExtensions
{
    public static T OnDocumentChanged<T>(this T control, Action<System.Object, AvaloniaEdit.Document.DocumentChangedEventArgs> action) where T : AvaloniaEdit.TextEditor => 
        control._setEvent((System.EventHandler<AvaloniaEdit.Document.DocumentChangedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.DocumentChanged += h);
    public static T OnOptionChanged<T>(this T control, Action<System.Object, System.ComponentModel.PropertyChangedEventArgs> action) where T : AvaloniaEdit.TextEditor => 
        control._setEvent((System.ComponentModel.PropertyChangedEventHandler) ((arg0, arg1) => action(arg0, arg1)), h => control.OptionChanged += h);
    public static T OnTextChanged<T>(this T control, Action<System.Object, System.EventArgs> action) where T : AvaloniaEdit.TextEditor => 
        control._setEvent((System.EventHandler) ((arg0, arg1) => action(arg0, arg1)), h => control.TextChanged += h);
    public static T OnPreviewPointerHover<T>(this T control, Action<System.Object, Avalonia.Input.PointerEventArgs> action) where T : AvaloniaEdit.TextEditor => 
        control._setEvent((System.EventHandler<Avalonia.Input.PointerEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.PreviewPointerHover += h);
    public static T OnPointerHover<T>(this T control, Action<System.Object, Avalonia.Input.PointerEventArgs> action) where T : AvaloniaEdit.TextEditor => 
        control._setEvent((System.EventHandler<Avalonia.Input.PointerEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.PointerHover += h);
    public static T OnPreviewPointerHoverStopped<T>(this T control, Action<System.Object, Avalonia.Input.PointerEventArgs> action) where T : AvaloniaEdit.TextEditor => 
        control._setEvent((System.EventHandler<Avalonia.Input.PointerEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.PreviewPointerHoverStopped += h);
    public static T OnPointerHoverStopped<T>(this T control, Action<System.Object, Avalonia.Input.PointerEventArgs> action) where T : AvaloniaEdit.TextEditor => 
        control._setEvent((System.EventHandler<Avalonia.Input.PointerEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.PointerHoverStopped += h);
}

