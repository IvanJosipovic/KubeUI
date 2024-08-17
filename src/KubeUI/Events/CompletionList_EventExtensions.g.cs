using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using AvaloniaEdit.CodeCompletion;
using CompletionList = AvaloniaEdit.CodeCompletion.CompletionList;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class CompletionListEventsExtensions
{
    public static T OnInsertionRequested<T>(this T control, Action<System.Object, System.EventArgs> action) where T : AvaloniaEdit.CodeCompletion.CompletionList => 
        control._setEvent((System.EventHandler) ((arg0, arg1) => action(arg0, arg1)), h => control.InsertionRequested += h);
    public static T OnSelectionChanged<T>(this T control, Action<System.Object, Avalonia.Controls.SelectionChangedEventArgs> action) where T : AvaloniaEdit.CodeCompletion.CompletionList => 
        control._setEvent((System.EventHandler<Avalonia.Controls.SelectionChangedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.SelectionChanged += h);
}

