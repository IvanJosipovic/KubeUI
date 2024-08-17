using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Styling;
using DataGrid = Avalonia.Controls.DataGrid;
using System;
using System.Collections;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class DataGridExtensions
{
public static Style<T> CanUserReorderColumns<T>(this Style<T> style, System.Boolean value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.CanUserReorderColumnsProperty, value);
public static Style<T> CanUserReorderColumns<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.CanUserReorderColumnsProperty, binding);
public static Style<T> CanUserResizeColumns<T>(this Style<T> style, System.Boolean value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.CanUserResizeColumnsProperty, value);
public static Style<T> CanUserResizeColumns<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.CanUserResizeColumnsProperty, binding);
public static Style<T> CanUserSortColumns<T>(this Style<T> style, System.Boolean value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.CanUserSortColumnsProperty, value);
public static Style<T> CanUserSortColumns<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.CanUserSortColumnsProperty, binding);
public static Style<T> ColumnHeaderHeight<T>(this Style<T> style, System.Double value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.ColumnHeaderHeightProperty, value);
public static Style<T> ColumnHeaderHeight<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.ColumnHeaderHeightProperty, binding);
public static Style<T> ColumnWidth<T>(this Style<T> style, Avalonia.Controls.DataGridLength value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.ColumnWidthProperty, value);
public static Style<T> ColumnWidth<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.ColumnWidthProperty, binding);

public static Style<T> ColumnWidth<T>(this Style<T> style, Double value) where T : Avalonia.Controls.DataGrid
   => style._addSetter(Avalonia.Controls.DataGrid.ColumnWidthProperty, new Avalonia.Controls.DataGridLength(value));
public static Style<T> ColumnWidth<T>(this Style<T> style, Double value, DataGridLengthUnitType type) where T : Avalonia.Controls.DataGrid
   => style._addSetter(Avalonia.Controls.DataGrid.ColumnWidthProperty, new Avalonia.Controls.DataGridLength(value, type));
public static Style<T> ColumnWidth<T>(this Style<T> style, Double value, DataGridLengthUnitType type, Double desiredValue, Double displayValue) where T : Avalonia.Controls.DataGrid
   => style._addSetter(Avalonia.Controls.DataGrid.ColumnWidthProperty, new Avalonia.Controls.DataGridLength(value, type, desiredValue, displayValue));
public static Style<T> RowTheme<T>(this Style<T> style, Avalonia.Styling.ControlTheme value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.RowThemeProperty, value);
public static Style<T> RowTheme<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.RowThemeProperty, binding);
public static Style<T> CellTheme<T>(this Style<T> style, Avalonia.Styling.ControlTheme value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.CellThemeProperty, value);
public static Style<T> CellTheme<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.CellThemeProperty, binding);
public static Style<T> ColumnHeaderTheme<T>(this Style<T> style, Avalonia.Styling.ControlTheme value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.ColumnHeaderThemeProperty, value);
public static Style<T> ColumnHeaderTheme<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.ColumnHeaderThemeProperty, binding);
public static Style<T> RowGroupTheme<T>(this Style<T> style, Avalonia.Styling.ControlTheme value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.RowGroupThemeProperty, value);
public static Style<T> RowGroupTheme<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.RowGroupThemeProperty, binding);
public static Style<T> FrozenColumnCount<T>(this Style<T> style, System.Int32 value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.FrozenColumnCountProperty, value);
public static Style<T> FrozenColumnCount<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.FrozenColumnCountProperty, binding);
public static Style<T> GridLinesVisibility<T>(this Style<T> style, Avalonia.Controls.DataGridGridLinesVisibility value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.GridLinesVisibilityProperty, value);
public static Style<T> GridLinesVisibility<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.GridLinesVisibilityProperty, binding);
public static Style<T> HeadersVisibility<T>(this Style<T> style, Avalonia.Controls.DataGridHeadersVisibility value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.HeadersVisibilityProperty, value);
public static Style<T> HeadersVisibility<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.HeadersVisibilityProperty, binding);
public static Style<T> HorizontalGridLinesBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.HorizontalGridLinesBrushProperty, value);
public static Style<T> HorizontalGridLinesBrush<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.HorizontalGridLinesBrushProperty, binding);
public static Style<T> HorizontalScrollBarVisibility<T>(this Style<T> style, Avalonia.Controls.Primitives.ScrollBarVisibility value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.HorizontalScrollBarVisibilityProperty, value);
public static Style<T> HorizontalScrollBarVisibility<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.HorizontalScrollBarVisibilityProperty, binding);
public static Style<T> IsReadOnly<T>(this Style<T> style, System.Boolean value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.IsReadOnlyProperty, value);
public static Style<T> IsReadOnly<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.IsReadOnlyProperty, binding);
public static Style<T> AreRowGroupHeadersFrozen<T>(this Style<T> style, System.Boolean value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.AreRowGroupHeadersFrozenProperty, value);
public static Style<T> AreRowGroupHeadersFrozen<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.AreRowGroupHeadersFrozenProperty, binding);
public static Style<T> MaxColumnWidth<T>(this Style<T> style, System.Double value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.MaxColumnWidthProperty, value);
public static Style<T> MaxColumnWidth<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.MaxColumnWidthProperty, binding);
public static Style<T> MinColumnWidth<T>(this Style<T> style, System.Double value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.MinColumnWidthProperty, value);
public static Style<T> MinColumnWidth<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.MinColumnWidthProperty, binding);
public static Style<T> RowBackground<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.RowBackgroundProperty, value);
public static Style<T> RowBackground<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.RowBackgroundProperty, binding);
public static Style<T> RowHeight<T>(this Style<T> style, System.Double value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.RowHeightProperty, value);
public static Style<T> RowHeight<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.RowHeightProperty, binding);
public static Style<T> RowHeaderWidth<T>(this Style<T> style, System.Double value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.RowHeaderWidthProperty, value);
public static Style<T> RowHeaderWidth<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.RowHeaderWidthProperty, binding);
public static Style<T> SelectionMode<T>(this Style<T> style, Avalonia.Controls.DataGridSelectionMode value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.SelectionModeProperty, value);
public static Style<T> SelectionMode<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.SelectionModeProperty, binding);
public static Style<T> VerticalGridLinesBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.VerticalGridLinesBrushProperty, value);
public static Style<T> VerticalGridLinesBrush<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.VerticalGridLinesBrushProperty, binding);
public static Style<T> VerticalScrollBarVisibility<T>(this Style<T> style, Avalonia.Controls.Primitives.ScrollBarVisibility value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.VerticalScrollBarVisibilityProperty, value);
public static Style<T> VerticalScrollBarVisibility<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.VerticalScrollBarVisibilityProperty, binding);
public static Style<T> DropLocationIndicatorTemplate<T>(this Style<T> style, Avalonia.Controls.ITemplate<Avalonia.Controls.Control> value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.DropLocationIndicatorTemplateProperty, value);
public static Style<T> DropLocationIndicatorTemplate<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.DropLocationIndicatorTemplateProperty, binding);
public static Style<T> ClipboardCopyMode<T>(this Style<T> style, Avalonia.Controls.DataGridClipboardCopyMode value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.ClipboardCopyModeProperty, value);
public static Style<T> ClipboardCopyMode<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.ClipboardCopyModeProperty, binding);
public static Style<T> AutoGenerateColumns<T>(this Style<T> style, System.Boolean value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.AutoGenerateColumnsProperty, value);
public static Style<T> AutoGenerateColumns<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.AutoGenerateColumnsProperty, binding);
public static Style<T> ItemsSource<T>(this Style<T> style, System.Collections.IEnumerable value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.ItemsSourceProperty, value);
public static Style<T> ItemsSource<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.ItemsSourceProperty, binding);
public static Style<T> AreRowDetailsFrozen<T>(this Style<T> style, System.Boolean value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.AreRowDetailsFrozenProperty, value);
public static Style<T> AreRowDetailsFrozen<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.AreRowDetailsFrozenProperty, binding);
public static Style<T> RowDetailsTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.RowDetailsTemplateProperty, value);
public static Style<T> RowDetailsTemplate<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.RowDetailsTemplateProperty, binding);
public static Style<T> RowDetailsVisibilityMode<T>(this Style<T> style, Avalonia.Controls.DataGridRowDetailsVisibilityMode value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.RowDetailsVisibilityModeProperty, value);
public static Style<T> RowDetailsVisibilityMode<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.RowDetailsVisibilityModeProperty, binding);
}

