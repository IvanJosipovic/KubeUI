using Avalonia.Data;
using Avalonia.Data.Converters;
using AvaloniaEdit.Search;
using SearchPanel = AvaloniaEdit.Search.SearchPanel;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class SearchPanelEventsExtensions
{
    public static T OnSearchOptionsChanged<T>(this T control, Action<System.Object, AvaloniaEdit.Search.SearchOptionsChangedEventArgs> action) where T : AvaloniaEdit.Search.SearchPanel => 
        control._setEvent((System.EventHandler<AvaloniaEdit.Search.SearchOptionsChangedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.SearchOptionsChanged += h);
}

