#nullable enable
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
public static T CanUserReorderColumns<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.CanUserReorderColumnsProperty, binding);
public static T CanUserReorderColumns<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.CanUserReorderColumnsProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CanUserReorderColumns<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.CanUserReorderColumnsProperty, func, onChanged, expression);
public static T CanUserReorderColumns<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.CanUserReorderColumnsProperty, ps, () => control.CanUserReorderColumns = value, bindingMode, converter, bindingSource);
public static T CanUserReorderColumns<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.CanUserReorderColumnsProperty, ps, () => control.CanUserReorderColumns = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T CanUserResizeColumns<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.CanUserResizeColumnsProperty, binding);
public static T CanUserResizeColumns<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.CanUserResizeColumnsProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CanUserResizeColumns<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.CanUserResizeColumnsProperty, func, onChanged, expression);
public static T CanUserResizeColumns<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.CanUserResizeColumnsProperty, ps, () => control.CanUserResizeColumns = value, bindingMode, converter, bindingSource);
public static T CanUserResizeColumns<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.CanUserResizeColumnsProperty, ps, () => control.CanUserResizeColumns = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T CanUserSortColumns<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.CanUserSortColumnsProperty, binding);
public static T CanUserSortColumns<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.CanUserSortColumnsProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CanUserSortColumns<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.CanUserSortColumnsProperty, func, onChanged, expression);
public static T CanUserSortColumns<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.CanUserSortColumnsProperty, ps, () => control.CanUserSortColumns = value, bindingMode, converter, bindingSource);
public static T CanUserSortColumns<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.CanUserSortColumnsProperty, ps, () => control.CanUserSortColumns = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ColumnHeaderHeight<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.ColumnHeaderHeightProperty, binding);
public static T ColumnHeaderHeight<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.ColumnHeaderHeightProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ColumnHeaderHeight<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.ColumnHeaderHeightProperty, func, onChanged, expression);
public static T ColumnHeaderHeight<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.ColumnHeaderHeightProperty, ps, () => control.ColumnHeaderHeight = value, bindingMode, converter, bindingSource);
public static T ColumnHeaderHeight<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.ColumnHeaderHeightProperty, ps, () => control.ColumnHeaderHeight = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ColumnWidth<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.ColumnWidthProperty, binding);
public static T ColumnWidth<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.ColumnWidthProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ColumnWidth<T>(this T control, Func<Avalonia.Controls.DataGridLength> func, Action<Avalonia.Controls.DataGridLength>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.ColumnWidthProperty, func, onChanged, expression);
public static T ColumnWidth<T>(this T control, Avalonia.Controls.DataGridLength value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.ColumnWidthProperty, ps, () => control.ColumnWidth = value, bindingMode, converter, bindingSource);
public static T ColumnWidth<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.DataGridLength> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.ColumnWidthProperty, ps, () => control.ColumnWidth = converter.TryConvert(value), bindingMode, converter, bindingSource);

public static T ColumnWidth<T>(this T control, Double value = default) where T : Avalonia.Controls.DataGrid
   => control._set(() => control.ColumnWidth = new Avalonia.Controls.DataGridLength(value));
public static T ColumnWidth<T>(this T control, Double value = default, DataGridLengthUnitType type = default) where T : Avalonia.Controls.DataGrid
   => control._set(() => control.ColumnWidth = new Avalonia.Controls.DataGridLength(value, type));
public static T ColumnWidth<T>(this T control, Double value = default, DataGridLengthUnitType type = default, Double desiredValue = default, Double displayValue = default) where T : Avalonia.Controls.DataGrid
   => control._set(() => control.ColumnWidth = new Avalonia.Controls.DataGridLength(value, type, desiredValue, displayValue));
public static T RowTheme<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowThemeProperty, binding);
public static T RowTheme<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowThemeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T RowTheme<T>(this T control, Func<Avalonia.Styling.ControlTheme> func, Action<Avalonia.Styling.ControlTheme>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowThemeProperty, func, onChanged, expression);
public static T RowTheme<T>(this T control, Avalonia.Styling.ControlTheme value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.RowThemeProperty, ps, () => control.RowTheme = value, bindingMode, converter, bindingSource);
public static T RowTheme<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ControlTheme> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.RowThemeProperty, ps, () => control.RowTheme = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T CellTheme<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.CellThemeProperty, binding);
public static T CellTheme<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.CellThemeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CellTheme<T>(this T control, Func<Avalonia.Styling.ControlTheme> func, Action<Avalonia.Styling.ControlTheme>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.CellThemeProperty, func, onChanged, expression);
public static T CellTheme<T>(this T control, Avalonia.Styling.ControlTheme value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.CellThemeProperty, ps, () => control.CellTheme = value, bindingMode, converter, bindingSource);
public static T CellTheme<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ControlTheme> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.CellThemeProperty, ps, () => control.CellTheme = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ColumnHeaderTheme<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.ColumnHeaderThemeProperty, binding);
public static T ColumnHeaderTheme<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.ColumnHeaderThemeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ColumnHeaderTheme<T>(this T control, Func<Avalonia.Styling.ControlTheme> func, Action<Avalonia.Styling.ControlTheme>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.ColumnHeaderThemeProperty, func, onChanged, expression);
public static T ColumnHeaderTheme<T>(this T control, Avalonia.Styling.ControlTheme value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.ColumnHeaderThemeProperty, ps, () => control.ColumnHeaderTheme = value, bindingMode, converter, bindingSource);
public static T ColumnHeaderTheme<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ControlTheme> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.ColumnHeaderThemeProperty, ps, () => control.ColumnHeaderTheme = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T RowGroupTheme<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowGroupThemeProperty, binding);
public static T RowGroupTheme<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowGroupThemeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T RowGroupTheme<T>(this T control, Func<Avalonia.Styling.ControlTheme> func, Action<Avalonia.Styling.ControlTheme>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowGroupThemeProperty, func, onChanged, expression);
public static T RowGroupTheme<T>(this T control, Avalonia.Styling.ControlTheme value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.RowGroupThemeProperty, ps, () => control.RowGroupTheme = value, bindingMode, converter, bindingSource);
public static T RowGroupTheme<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ControlTheme> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.RowGroupThemeProperty, ps, () => control.RowGroupTheme = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T FrozenColumnCount<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.FrozenColumnCountProperty, binding);
public static T FrozenColumnCount<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.FrozenColumnCountProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T FrozenColumnCount<T>(this T control, Func<System.Int32> func, Action<System.Int32>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.FrozenColumnCountProperty, func, onChanged, expression);
public static T FrozenColumnCount<T>(this T control, System.Int32 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.FrozenColumnCountProperty, ps, () => control.FrozenColumnCount = value, bindingMode, converter, bindingSource);
public static T FrozenColumnCount<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Int32> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.FrozenColumnCountProperty, ps, () => control.FrozenColumnCount = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T GridLinesVisibility<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.GridLinesVisibilityProperty, binding);
public static T GridLinesVisibility<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.GridLinesVisibilityProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T GridLinesVisibility<T>(this T control, Func<Avalonia.Controls.DataGridGridLinesVisibility> func, Action<Avalonia.Controls.DataGridGridLinesVisibility>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.GridLinesVisibilityProperty, func, onChanged, expression);
public static T GridLinesVisibility<T>(this T control, Avalonia.Controls.DataGridGridLinesVisibility value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.GridLinesVisibilityProperty, ps, () => control.GridLinesVisibility = value, bindingMode, converter, bindingSource);
public static T GridLinesVisibility<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.DataGridGridLinesVisibility> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.GridLinesVisibilityProperty, ps, () => control.GridLinesVisibility = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T HeadersVisibility<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.HeadersVisibilityProperty, binding);
public static T HeadersVisibility<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.HeadersVisibilityProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T HeadersVisibility<T>(this T control, Func<Avalonia.Controls.DataGridHeadersVisibility> func, Action<Avalonia.Controls.DataGridHeadersVisibility>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.HeadersVisibilityProperty, func, onChanged, expression);
public static T HeadersVisibility<T>(this T control, Avalonia.Controls.DataGridHeadersVisibility value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.HeadersVisibilityProperty, ps, () => control.HeadersVisibility = value, bindingMode, converter, bindingSource);
public static T HeadersVisibility<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.DataGridHeadersVisibility> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.HeadersVisibilityProperty, ps, () => control.HeadersVisibility = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T HorizontalGridLinesBrush<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.HorizontalGridLinesBrushProperty, binding);
public static T HorizontalGridLinesBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.HorizontalGridLinesBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T HorizontalGridLinesBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.HorizontalGridLinesBrushProperty, func, onChanged, expression);
public static T HorizontalGridLinesBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.HorizontalGridLinesBrushProperty, ps, () => control.HorizontalGridLinesBrush = value, bindingMode, converter, bindingSource);
public static T HorizontalGridLinesBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.HorizontalGridLinesBrushProperty, ps, () => control.HorizontalGridLinesBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T HorizontalScrollBarVisibility<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.HorizontalScrollBarVisibilityProperty, binding);
public static T HorizontalScrollBarVisibility<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.HorizontalScrollBarVisibilityProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T HorizontalScrollBarVisibility<T>(this T control, Func<Avalonia.Controls.Primitives.ScrollBarVisibility> func, Action<Avalonia.Controls.Primitives.ScrollBarVisibility>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.HorizontalScrollBarVisibilityProperty, func, onChanged, expression);
public static T HorizontalScrollBarVisibility<T>(this T control, Avalonia.Controls.Primitives.ScrollBarVisibility value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.HorizontalScrollBarVisibilityProperty, ps, () => control.HorizontalScrollBarVisibility = value, bindingMode, converter, bindingSource);
public static T HorizontalScrollBarVisibility<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Primitives.ScrollBarVisibility> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.HorizontalScrollBarVisibilityProperty, ps, () => control.HorizontalScrollBarVisibility = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsReadOnly<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.IsReadOnlyProperty, binding);
public static T IsReadOnly<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.IsReadOnlyProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsReadOnly<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.IsReadOnlyProperty, func, onChanged, expression);
public static T IsReadOnly<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.IsReadOnlyProperty, ps, () => control.IsReadOnly = value, bindingMode, converter, bindingSource);
public static T IsReadOnly<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.IsReadOnlyProperty, ps, () => control.IsReadOnly = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T AreRowGroupHeadersFrozen<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.AreRowGroupHeadersFrozenProperty, binding);
public static T AreRowGroupHeadersFrozen<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.AreRowGroupHeadersFrozenProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T AreRowGroupHeadersFrozen<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.AreRowGroupHeadersFrozenProperty, func, onChanged, expression);
public static T AreRowGroupHeadersFrozen<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.AreRowGroupHeadersFrozenProperty, ps, () => control.AreRowGroupHeadersFrozen = value, bindingMode, converter, bindingSource);
public static T AreRowGroupHeadersFrozen<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.AreRowGroupHeadersFrozenProperty, ps, () => control.AreRowGroupHeadersFrozen = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsScrollInertiaEnabled<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.IsScrollInertiaEnabledProperty, binding);
public static T IsScrollInertiaEnabled<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.IsScrollInertiaEnabledProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsScrollInertiaEnabled<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.IsScrollInertiaEnabledProperty, func, onChanged, expression);
public static T IsScrollInertiaEnabled<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.IsScrollInertiaEnabledProperty, ps, () => control.IsScrollInertiaEnabled = value, bindingMode, converter, bindingSource);
public static T IsScrollInertiaEnabled<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.IsScrollInertiaEnabledProperty, ps, () => control.IsScrollInertiaEnabled = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T MaxColumnWidth<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.MaxColumnWidthProperty, binding);
public static T MaxColumnWidth<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.MaxColumnWidthProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T MaxColumnWidth<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.MaxColumnWidthProperty, func, onChanged, expression);
public static T MaxColumnWidth<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.MaxColumnWidthProperty, ps, () => control.MaxColumnWidth = value, bindingMode, converter, bindingSource);
public static T MaxColumnWidth<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.MaxColumnWidthProperty, ps, () => control.MaxColumnWidth = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T MinColumnWidth<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.MinColumnWidthProperty, binding);
public static T MinColumnWidth<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.MinColumnWidthProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T MinColumnWidth<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.MinColumnWidthProperty, func, onChanged, expression);
public static T MinColumnWidth<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.MinColumnWidthProperty, ps, () => control.MinColumnWidth = value, bindingMode, converter, bindingSource);
public static T MinColumnWidth<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.MinColumnWidthProperty, ps, () => control.MinColumnWidth = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T RowBackground<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowBackgroundProperty, binding);
public static T RowBackground<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowBackgroundProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T RowBackground<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowBackgroundProperty, func, onChanged, expression);
public static T RowBackground<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.RowBackgroundProperty, ps, () => control.RowBackground = value, bindingMode, converter, bindingSource);
public static T RowBackground<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.RowBackgroundProperty, ps, () => control.RowBackground = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T RowHeight<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowHeightProperty, binding);
public static T RowHeight<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowHeightProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T RowHeight<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowHeightProperty, func, onChanged, expression);
public static T RowHeight<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.RowHeightProperty, ps, () => control.RowHeight = value, bindingMode, converter, bindingSource);
public static T RowHeight<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.RowHeightProperty, ps, () => control.RowHeight = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T RowHeaderWidth<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowHeaderWidthProperty, binding);
public static T RowHeaderWidth<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowHeaderWidthProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T RowHeaderWidth<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowHeaderWidthProperty, func, onChanged, expression);
public static T RowHeaderWidth<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.RowHeaderWidthProperty, ps, () => control.RowHeaderWidth = value, bindingMode, converter, bindingSource);
public static T RowHeaderWidth<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.RowHeaderWidthProperty, ps, () => control.RowHeaderWidth = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SelectionMode<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.SelectionModeProperty, binding);
public static T SelectionMode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.SelectionModeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SelectionMode<T>(this T control, Func<Avalonia.Controls.DataGridSelectionMode> func, Action<Avalonia.Controls.DataGridSelectionMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.SelectionModeProperty, func, onChanged, expression);
public static T SelectionMode<T>(this T control, Avalonia.Controls.DataGridSelectionMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.SelectionModeProperty, ps, () => control.SelectionMode = value, bindingMode, converter, bindingSource);
public static T SelectionMode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.DataGridSelectionMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.SelectionModeProperty, ps, () => control.SelectionMode = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T VerticalGridLinesBrush<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.VerticalGridLinesBrushProperty, binding);
public static T VerticalGridLinesBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.VerticalGridLinesBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T VerticalGridLinesBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.VerticalGridLinesBrushProperty, func, onChanged, expression);
public static T VerticalGridLinesBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.VerticalGridLinesBrushProperty, ps, () => control.VerticalGridLinesBrush = value, bindingMode, converter, bindingSource);
public static T VerticalGridLinesBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.VerticalGridLinesBrushProperty, ps, () => control.VerticalGridLinesBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T VerticalScrollBarVisibility<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.VerticalScrollBarVisibilityProperty, binding);
public static T VerticalScrollBarVisibility<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.VerticalScrollBarVisibilityProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T VerticalScrollBarVisibility<T>(this T control, Func<Avalonia.Controls.Primitives.ScrollBarVisibility> func, Action<Avalonia.Controls.Primitives.ScrollBarVisibility>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.VerticalScrollBarVisibilityProperty, func, onChanged, expression);
public static T VerticalScrollBarVisibility<T>(this T control, Avalonia.Controls.Primitives.ScrollBarVisibility value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.VerticalScrollBarVisibilityProperty, ps, () => control.VerticalScrollBarVisibility = value, bindingMode, converter, bindingSource);
public static T VerticalScrollBarVisibility<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Primitives.ScrollBarVisibility> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.VerticalScrollBarVisibilityProperty, ps, () => control.VerticalScrollBarVisibility = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T DropLocationIndicatorTemplate<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.DropLocationIndicatorTemplateProperty, binding);
public static T DropLocationIndicatorTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.DropLocationIndicatorTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T DropLocationIndicatorTemplate<T>(this T control, Func<Avalonia.Controls.ITemplate<Avalonia.Controls.Control>> func, Action<Avalonia.Controls.ITemplate<Avalonia.Controls.Control>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.DropLocationIndicatorTemplateProperty, func, onChanged, expression);
public static T DropLocationIndicatorTemplate<T>(this T control, Avalonia.Controls.ITemplate<Avalonia.Controls.Control> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.DropLocationIndicatorTemplateProperty, ps, () => control.DropLocationIndicatorTemplate = value, bindingMode, converter, bindingSource);
public static T DropLocationIndicatorTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.ITemplate<Avalonia.Controls.Control>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.DropLocationIndicatorTemplateProperty, ps, () => control.DropLocationIndicatorTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SelectedIndex<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.SelectedIndexProperty, binding);
public static T SelectedIndex<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.SelectedIndexProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SelectedIndex<T>(this T control, Func<System.Int32> func, Action<System.Int32>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.SelectedIndexProperty, func, onChanged, expression);
public static T SelectedIndex<T>(this T control, System.Int32 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.SelectedIndexProperty, ps, () => control.SelectedIndex = value, bindingMode, converter, bindingSource);
public static T SelectedIndex<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Int32> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.SelectedIndexProperty, ps, () => control.SelectedIndex = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SelectedItem<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.SelectedItemProperty, binding);
public static T SelectedItem<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.SelectedItemProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SelectedItem<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.SelectedItemProperty, func, onChanged, expression);
public static T SelectedItem<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.SelectedItemProperty, ps, () => control.SelectedItem = value, bindingMode, converter, bindingSource);
public static T SelectedItem<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.SelectedItemProperty, ps, () => control.SelectedItem = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ClipboardCopyMode<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.ClipboardCopyModeProperty, binding);
public static T ClipboardCopyMode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.ClipboardCopyModeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ClipboardCopyMode<T>(this T control, Func<Avalonia.Controls.DataGridClipboardCopyMode> func, Action<Avalonia.Controls.DataGridClipboardCopyMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.ClipboardCopyModeProperty, func, onChanged, expression);
public static T ClipboardCopyMode<T>(this T control, Avalonia.Controls.DataGridClipboardCopyMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.ClipboardCopyModeProperty, ps, () => control.ClipboardCopyMode = value, bindingMode, converter, bindingSource);
public static T ClipboardCopyMode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.DataGridClipboardCopyMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.ClipboardCopyModeProperty, ps, () => control.ClipboardCopyMode = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T AutoGenerateColumns<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.AutoGenerateColumnsProperty, binding);
public static T AutoGenerateColumns<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.AutoGenerateColumnsProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T AutoGenerateColumns<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.AutoGenerateColumnsProperty, func, onChanged, expression);
public static T AutoGenerateColumns<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.AutoGenerateColumnsProperty, ps, () => control.AutoGenerateColumns = value, bindingMode, converter, bindingSource);
public static T AutoGenerateColumns<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.AutoGenerateColumnsProperty, ps, () => control.AutoGenerateColumns = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ItemsSource<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.ItemsSourceProperty, binding);
public static T ItemsSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.ItemsSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ItemsSource<T>(this T control, Func<System.Collections.IEnumerable> func, Action<System.Collections.IEnumerable>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.ItemsSourceProperty, func, onChanged, expression);
public static T ItemsSource<T>(this T control, System.Collections.IEnumerable value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.ItemsSourceProperty, ps, () => control.ItemsSource = value, bindingMode, converter, bindingSource);
public static T ItemsSource<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.IEnumerable> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.ItemsSourceProperty, ps, () => control.ItemsSource = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T AreRowDetailsFrozen<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.AreRowDetailsFrozenProperty, binding);
public static T AreRowDetailsFrozen<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.AreRowDetailsFrozenProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T AreRowDetailsFrozen<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.AreRowDetailsFrozenProperty, func, onChanged, expression);
public static T AreRowDetailsFrozen<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.AreRowDetailsFrozenProperty, ps, () => control.AreRowDetailsFrozen = value, bindingMode, converter, bindingSource);
public static T AreRowDetailsFrozen<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.AreRowDetailsFrozenProperty, ps, () => control.AreRowDetailsFrozen = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T RowDetailsTemplate<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowDetailsTemplateProperty, binding);
public static T RowDetailsTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowDetailsTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T RowDetailsTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowDetailsTemplateProperty, func, onChanged, expression);
public static T RowDetailsTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.RowDetailsTemplateProperty, ps, () => control.RowDetailsTemplate = value, bindingMode, converter, bindingSource);
public static T RowDetailsTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.RowDetailsTemplateProperty, ps, () => control.RowDetailsTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T RowDetailsVisibilityMode<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowDetailsVisibilityModeProperty, binding);
public static T RowDetailsVisibilityMode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowDetailsVisibilityModeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T RowDetailsVisibilityMode<T>(this T control, Func<Avalonia.Controls.DataGridRowDetailsVisibilityMode> func, Action<Avalonia.Controls.DataGridRowDetailsVisibilityMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowDetailsVisibilityModeProperty, func, onChanged, expression);
public static T RowDetailsVisibilityMode<T>(this T control, Avalonia.Controls.DataGridRowDetailsVisibilityMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.RowDetailsVisibilityModeProperty, ps, () => control.RowDetailsVisibilityMode = value, bindingMode, converter, bindingSource);
public static T RowDetailsVisibilityMode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.DataGridRowDetailsVisibilityMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.RowDetailsVisibilityModeProperty, ps, () => control.RowDetailsVisibilityMode = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

