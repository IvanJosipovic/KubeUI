using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using DataGrid = Avalonia.Controls.DataGrid;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class DataGridEventsExtensions
{
    public static T OnHorizontalScroll<T>(this T control, Action<System.Object, Avalonia.Controls.Primitives.ScrollEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.Primitives.ScrollEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.HorizontalScroll += h);
    public static T OnVerticalScroll<T>(this T control, Action<System.Object, Avalonia.Controls.Primitives.ScrollEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.Primitives.ScrollEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.VerticalScroll += h);
    public static T OnAutoGeneratingColumn<T>(this T control, Action<System.Object, Avalonia.Controls.DataGridAutoGeneratingColumnEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridAutoGeneratingColumnEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.AutoGeneratingColumn += h);
    public static T OnBeginningEdit<T>(this T control, Action<System.Object, Avalonia.Controls.DataGridBeginningEditEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridBeginningEditEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.BeginningEdit += h);
    public static T OnCellEditEnded<T>(this T control, Action<System.Object, Avalonia.Controls.DataGridCellEditEndedEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridCellEditEndedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.CellEditEnded += h);
    public static T OnCellEditEnding<T>(this T control, Action<System.Object, Avalonia.Controls.DataGridCellEditEndingEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridCellEditEndingEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.CellEditEnding += h);
    public static T OnCellPointerPressed<T>(this T control, Action<System.Object, Avalonia.Controls.DataGridCellPointerPressedEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridCellPointerPressedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.CellPointerPressed += h);
    public static T OnColumnDisplayIndexChanged<T>(this T control, Action<System.Object, Avalonia.Controls.DataGridColumnEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridColumnEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.ColumnDisplayIndexChanged += h);
    public static T OnColumnReordered<T>(this T control, Action<System.Object, Avalonia.Controls.DataGridColumnEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridColumnEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.ColumnReordered += h);
    public static T OnColumnReordering<T>(this T control, Action<System.Object, Avalonia.Controls.DataGridColumnReorderingEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridColumnReorderingEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.ColumnReordering += h);
    public static T OnCurrentCellChanged<T>(this T control, Action<System.Object, System.EventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.CurrentCellChanged += h);
    public static T OnLoadingRow<T>(this T control, Action<System.Object, Avalonia.Controls.DataGridRowEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridRowEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.LoadingRow += h);
    public static T OnPreparingCellForEdit<T>(this T control, Action<System.Object, Avalonia.Controls.DataGridPreparingCellForEditEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridPreparingCellForEditEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.PreparingCellForEdit += h);
    public static T OnRowEditEnded<T>(this T control, Action<System.Object, Avalonia.Controls.DataGridRowEditEndedEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridRowEditEndedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.RowEditEnded += h);
    public static T OnRowEditEnding<T>(this T control, Action<System.Object, Avalonia.Controls.DataGridRowEditEndingEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridRowEditEndingEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.RowEditEnding += h);
    public static T OnSelectionChanged<T>(this T control, Action<System.Object, Avalonia.Controls.SelectionChangedEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.SelectionChangedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.SelectionChanged += h);
    public static T OnSorting<T>(this T control, Action<System.Object, Avalonia.Controls.DataGridColumnEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridColumnEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Sorting += h);
    public static T OnUnloadingRow<T>(this T control, Action<System.Object, Avalonia.Controls.DataGridRowEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridRowEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.UnloadingRow += h);
    public static T OnLoadingRowDetails<T>(this T control, Action<System.Object, Avalonia.Controls.DataGridRowDetailsEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridRowDetailsEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.LoadingRowDetails += h);
    public static T OnRowDetailsVisibilityChanged<T>(this T control, Action<System.Object, Avalonia.Controls.DataGridRowDetailsEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridRowDetailsEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.RowDetailsVisibilityChanged += h);
    public static T OnUnloadingRowDetails<T>(this T control, Action<System.Object, Avalonia.Controls.DataGridRowDetailsEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridRowDetailsEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.UnloadingRowDetails += h);
    public static T OnLoadingRowGroup<T>(this T control, Action<System.Object, Avalonia.Controls.DataGridRowGroupHeaderEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridRowGroupHeaderEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.LoadingRowGroup += h);
    public static T OnUnloadingRowGroup<T>(this T control, Action<System.Object, Avalonia.Controls.DataGridRowGroupHeaderEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridRowGroupHeaderEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.UnloadingRowGroup += h);
    public static T OnCopyingRowClipboardContent<T>(this T control, Action<System.Object, Avalonia.Controls.DataGridRowClipboardEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridRowClipboardEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.CopyingRowClipboardContent += h);
}

