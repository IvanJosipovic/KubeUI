using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TextArea = AvaloniaEdit.Editing.TextArea;

namespace Avalonia.Markup.Declarative;
public static partial class TextAreaEventsExtensions
{
    public static T OnActiveInputHandlerChanged<T>(this T control, Action<System.Object, System.EventArgs> action) where T : AvaloniaEdit.Editing.TextArea => 
        control._setEvent((System.EventHandler) ((arg0, arg1) => action(arg0, arg1)), h => control.ActiveInputHandlerChanged += h);
    public static T OnDocumentChanged<T>(this T control, Action<System.Object, AvaloniaEdit.Document.DocumentChangedEventArgs> action) where T : AvaloniaEdit.Editing.TextArea => 
        control._setEvent((System.EventHandler<AvaloniaEdit.Document.DocumentChangedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.DocumentChanged += h);
    public static T OnOptionChanged<T>(this T control, Action<System.Object, System.ComponentModel.PropertyChangedEventArgs> action) where T : AvaloniaEdit.Editing.TextArea => 
        control._setEvent((System.ComponentModel.PropertyChangedEventHandler) ((arg0, arg1) => action(arg0, arg1)), h => control.OptionChanged += h);
    public static T OnSelectionChanged<T>(this T control, Action<System.Object, System.EventArgs> action) where T : AvaloniaEdit.Editing.TextArea => 
        control._setEvent((System.EventHandler) ((arg0, arg1) => action(arg0, arg1)), h => control.SelectionChanged += h);
    public static T OnTextEntering<T>(this T control, Action<System.Object, Avalonia.Input.TextInputEventArgs> action) where T : AvaloniaEdit.Editing.TextArea => 
        control._setEvent((System.EventHandler<Avalonia.Input.TextInputEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.TextEntering += h);
    public static T OnTextEntered<T>(this T control, Action<System.Object, Avalonia.Input.TextInputEventArgs> action) where T : AvaloniaEdit.Editing.TextArea => 
        control._setEvent((System.EventHandler<Avalonia.Input.TextInputEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.TextEntered += h);
    public static T OnTextCopied<T>(this T control, Action<System.Object, AvaloniaEdit.Editing.TextEventArgs> action) where T : AvaloniaEdit.Editing.TextArea => 
        control._setEvent((System.EventHandler<AvaloniaEdit.Editing.TextEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.TextCopied += h);
}

