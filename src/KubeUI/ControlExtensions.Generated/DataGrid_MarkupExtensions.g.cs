#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
[global::System.CodeDom.Compiler.GeneratedCode("AvaloniaExtensionGenerator", "11.1.3.0")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static partial class DataGrid_MarkupExtensions
{
//================= Properties ======================//
 // CanUserReorderColumnsProperty

/*BindFromExpressionSetterGenerator*/
public static T CanUserReorderColumns<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.CanUserReorderColumnsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CanUserReorderColumns<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.CanUserReorderColumnsProperty, ps, () => control.CanUserReorderColumns = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CanUserReorderColumns<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.CanUserReorderColumnsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CanUserReorderColumns<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.CanUserReorderColumnsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CanUserReorderColumns<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.CanUserReorderColumnsProperty, ps, () => control.CanUserReorderColumns = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CanUserResizeColumnsProperty

/*BindFromExpressionSetterGenerator*/
public static T CanUserResizeColumns<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.CanUserResizeColumnsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CanUserResizeColumns<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.CanUserResizeColumnsProperty, ps, () => control.CanUserResizeColumns = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CanUserResizeColumns<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.CanUserResizeColumnsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CanUserResizeColumns<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.CanUserResizeColumnsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CanUserResizeColumns<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.CanUserResizeColumnsProperty, ps, () => control.CanUserResizeColumns = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CanUserSortColumnsProperty

/*BindFromExpressionSetterGenerator*/
public static T CanUserSortColumns<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.CanUserSortColumnsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CanUserSortColumns<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.CanUserSortColumnsProperty, ps, () => control.CanUserSortColumns = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CanUserSortColumns<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.CanUserSortColumnsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CanUserSortColumns<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.CanUserSortColumnsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CanUserSortColumns<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.CanUserSortColumnsProperty, ps, () => control.CanUserSortColumns = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ColumnHeaderHeightProperty

/*BindFromExpressionSetterGenerator*/
public static T ColumnHeaderHeight<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.ColumnHeaderHeightProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ColumnHeaderHeight<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.ColumnHeaderHeightProperty, ps, () => control.ColumnHeaderHeight = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ColumnHeaderHeight<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.ColumnHeaderHeightProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ColumnHeaderHeight<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.ColumnHeaderHeightProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ColumnHeaderHeight<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.ColumnHeaderHeightProperty, ps, () => control.ColumnHeaderHeight = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ColumnWidthProperty

/*BindFromExpressionSetterGenerator*/
public static T ColumnWidth<T>(this T control, Func<Avalonia.Controls.DataGridLength> func, Action<Avalonia.Controls.DataGridLength>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.ColumnWidthProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ColumnWidth<T>(this T control, Avalonia.Controls.DataGridLength value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.ColumnWidthProperty, ps, () => control.ColumnWidth = value, bindingMode, converter, bindingSource);

/*ValueOverloadsSetterGenerator*/

public static T ColumnWidth<T>(this T control, System.Double value = default) where T : Avalonia.Controls.DataGrid
   => control._set(() => control.ColumnWidth = new Avalonia.Controls.DataGridLength(value));
public static T ColumnWidth<T>(this T control, System.Double value = default, Avalonia.Controls.DataGridLengthUnitType type = default) where T : Avalonia.Controls.DataGrid
   => control._set(() => control.ColumnWidth = new Avalonia.Controls.DataGridLength(value, type));
public static T ColumnWidth<T>(this T control, System.Double value = default, Avalonia.Controls.DataGridLengthUnitType type = default, System.Double desiredValue = default, System.Double displayValue = default) where T : Avalonia.Controls.DataGrid
   => control._set(() => control.ColumnWidth = new Avalonia.Controls.DataGridLength(value, type, desiredValue, displayValue));

/*BindSetterGenerator*/
public static T ColumnWidth<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.ColumnWidthProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ColumnWidth<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.ColumnWidthProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ColumnWidth<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.DataGridLength> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.ColumnWidthProperty, ps, () => control.ColumnWidth = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // RowThemeProperty

/*BindFromExpressionSetterGenerator*/
public static T RowTheme<T>(this T control, Func<Avalonia.Styling.ControlTheme> func, Action<Avalonia.Styling.ControlTheme>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowThemeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T RowTheme<T>(this T control, Avalonia.Styling.ControlTheme value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.RowThemeProperty, ps, () => control.RowTheme = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T RowTheme<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowThemeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T RowTheme<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowThemeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T RowTheme<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ControlTheme> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.RowThemeProperty, ps, () => control.RowTheme = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CellThemeProperty

/*BindFromExpressionSetterGenerator*/
public static T CellTheme<T>(this T control, Func<Avalonia.Styling.ControlTheme> func, Action<Avalonia.Styling.ControlTheme>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.CellThemeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CellTheme<T>(this T control, Avalonia.Styling.ControlTheme value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.CellThemeProperty, ps, () => control.CellTheme = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CellTheme<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.CellThemeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CellTheme<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.CellThemeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CellTheme<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ControlTheme> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.CellThemeProperty, ps, () => control.CellTheme = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ColumnHeaderThemeProperty

/*BindFromExpressionSetterGenerator*/
public static T ColumnHeaderTheme<T>(this T control, Func<Avalonia.Styling.ControlTheme> func, Action<Avalonia.Styling.ControlTheme>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.ColumnHeaderThemeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ColumnHeaderTheme<T>(this T control, Avalonia.Styling.ControlTheme value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.ColumnHeaderThemeProperty, ps, () => control.ColumnHeaderTheme = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ColumnHeaderTheme<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.ColumnHeaderThemeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ColumnHeaderTheme<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.ColumnHeaderThemeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ColumnHeaderTheme<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ControlTheme> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.ColumnHeaderThemeProperty, ps, () => control.ColumnHeaderTheme = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // RowGroupThemeProperty

/*BindFromExpressionSetterGenerator*/
public static T RowGroupTheme<T>(this T control, Func<Avalonia.Styling.ControlTheme> func, Action<Avalonia.Styling.ControlTheme>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowGroupThemeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T RowGroupTheme<T>(this T control, Avalonia.Styling.ControlTheme value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.RowGroupThemeProperty, ps, () => control.RowGroupTheme = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T RowGroupTheme<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowGroupThemeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T RowGroupTheme<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowGroupThemeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T RowGroupTheme<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ControlTheme> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.RowGroupThemeProperty, ps, () => control.RowGroupTheme = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // FrozenColumnCountProperty

/*BindFromExpressionSetterGenerator*/
public static T FrozenColumnCount<T>(this T control, Func<System.Int32> func, Action<System.Int32>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.FrozenColumnCountProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T FrozenColumnCount<T>(this T control, System.Int32 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.FrozenColumnCountProperty, ps, () => control.FrozenColumnCount = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T FrozenColumnCount<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.FrozenColumnCountProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T FrozenColumnCount<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.FrozenColumnCountProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T FrozenColumnCount<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Int32> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.FrozenColumnCountProperty, ps, () => control.FrozenColumnCount = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // GridLinesVisibilityProperty

/*BindFromExpressionSetterGenerator*/
public static T GridLinesVisibility<T>(this T control, Func<Avalonia.Controls.DataGridGridLinesVisibility> func, Action<Avalonia.Controls.DataGridGridLinesVisibility>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.GridLinesVisibilityProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T GridLinesVisibility<T>(this T control, Avalonia.Controls.DataGridGridLinesVisibility value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.GridLinesVisibilityProperty, ps, () => control.GridLinesVisibility = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T GridLinesVisibility<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.GridLinesVisibilityProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T GridLinesVisibility<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.GridLinesVisibilityProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T GridLinesVisibility<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.DataGridGridLinesVisibility> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.GridLinesVisibilityProperty, ps, () => control.GridLinesVisibility = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // HeadersVisibilityProperty

/*BindFromExpressionSetterGenerator*/
public static T HeadersVisibility<T>(this T control, Func<Avalonia.Controls.DataGridHeadersVisibility> func, Action<Avalonia.Controls.DataGridHeadersVisibility>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.HeadersVisibilityProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T HeadersVisibility<T>(this T control, Avalonia.Controls.DataGridHeadersVisibility value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.HeadersVisibilityProperty, ps, () => control.HeadersVisibility = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T HeadersVisibility<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.HeadersVisibilityProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T HeadersVisibility<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.HeadersVisibilityProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T HeadersVisibility<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.DataGridHeadersVisibility> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.HeadersVisibilityProperty, ps, () => control.HeadersVisibility = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // HorizontalGridLinesBrushProperty

/*BindFromExpressionSetterGenerator*/
public static T HorizontalGridLinesBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.HorizontalGridLinesBrushProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T HorizontalGridLinesBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.HorizontalGridLinesBrushProperty, ps, () => control.HorizontalGridLinesBrush = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T HorizontalGridLinesBrush<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.HorizontalGridLinesBrushProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T HorizontalGridLinesBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.HorizontalGridLinesBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T HorizontalGridLinesBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.HorizontalGridLinesBrushProperty, ps, () => control.HorizontalGridLinesBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // HorizontalScrollBarVisibilityProperty

/*BindFromExpressionSetterGenerator*/
public static T HorizontalScrollBarVisibility<T>(this T control, Func<Avalonia.Controls.Primitives.ScrollBarVisibility> func, Action<Avalonia.Controls.Primitives.ScrollBarVisibility>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.HorizontalScrollBarVisibilityProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T HorizontalScrollBarVisibility<T>(this T control, Avalonia.Controls.Primitives.ScrollBarVisibility value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.HorizontalScrollBarVisibilityProperty, ps, () => control.HorizontalScrollBarVisibility = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T HorizontalScrollBarVisibility<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.HorizontalScrollBarVisibilityProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T HorizontalScrollBarVisibility<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.HorizontalScrollBarVisibilityProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T HorizontalScrollBarVisibility<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Primitives.ScrollBarVisibility> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.HorizontalScrollBarVisibilityProperty, ps, () => control.HorizontalScrollBarVisibility = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsReadOnlyProperty

/*BindFromExpressionSetterGenerator*/
public static T IsReadOnly<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.IsReadOnlyProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsReadOnly<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.IsReadOnlyProperty, ps, () => control.IsReadOnly = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsReadOnly<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.IsReadOnlyProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsReadOnly<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.IsReadOnlyProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsReadOnly<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.IsReadOnlyProperty, ps, () => control.IsReadOnly = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // AreRowGroupHeadersFrozenProperty

/*BindFromExpressionSetterGenerator*/
public static T AreRowGroupHeadersFrozen<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.AreRowGroupHeadersFrozenProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T AreRowGroupHeadersFrozen<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.AreRowGroupHeadersFrozenProperty, ps, () => control.AreRowGroupHeadersFrozen = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T AreRowGroupHeadersFrozen<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.AreRowGroupHeadersFrozenProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T AreRowGroupHeadersFrozen<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.AreRowGroupHeadersFrozenProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T AreRowGroupHeadersFrozen<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.AreRowGroupHeadersFrozenProperty, ps, () => control.AreRowGroupHeadersFrozen = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsScrollInertiaEnabledProperty

/*BindFromExpressionSetterGenerator*/
public static T IsScrollInertiaEnabled<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.IsScrollInertiaEnabledProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsScrollInertiaEnabled<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.IsScrollInertiaEnabledProperty, ps, () => control.IsScrollInertiaEnabled = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsScrollInertiaEnabled<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.IsScrollInertiaEnabledProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsScrollInertiaEnabled<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.IsScrollInertiaEnabledProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsScrollInertiaEnabled<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.IsScrollInertiaEnabledProperty, ps, () => control.IsScrollInertiaEnabled = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // MaxColumnWidthProperty

/*BindFromExpressionSetterGenerator*/
public static T MaxColumnWidth<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.MaxColumnWidthProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T MaxColumnWidth<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.MaxColumnWidthProperty, ps, () => control.MaxColumnWidth = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T MaxColumnWidth<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.MaxColumnWidthProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T MaxColumnWidth<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.MaxColumnWidthProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T MaxColumnWidth<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.MaxColumnWidthProperty, ps, () => control.MaxColumnWidth = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // MinColumnWidthProperty

/*BindFromExpressionSetterGenerator*/
public static T MinColumnWidth<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.MinColumnWidthProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T MinColumnWidth<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.MinColumnWidthProperty, ps, () => control.MinColumnWidth = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T MinColumnWidth<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.MinColumnWidthProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T MinColumnWidth<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.MinColumnWidthProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T MinColumnWidth<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.MinColumnWidthProperty, ps, () => control.MinColumnWidth = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // RowBackgroundProperty

/*BindFromExpressionSetterGenerator*/
public static T RowBackground<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowBackgroundProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T RowBackground<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.RowBackgroundProperty, ps, () => control.RowBackground = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T RowBackground<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowBackgroundProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T RowBackground<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowBackgroundProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T RowBackground<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.RowBackgroundProperty, ps, () => control.RowBackground = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // RowHeightProperty

/*BindFromExpressionSetterGenerator*/
public static T RowHeight<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowHeightProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T RowHeight<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.RowHeightProperty, ps, () => control.RowHeight = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T RowHeight<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowHeightProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T RowHeight<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowHeightProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T RowHeight<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.RowHeightProperty, ps, () => control.RowHeight = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // RowHeaderWidthProperty

/*BindFromExpressionSetterGenerator*/
public static T RowHeaderWidth<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowHeaderWidthProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T RowHeaderWidth<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.RowHeaderWidthProperty, ps, () => control.RowHeaderWidth = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T RowHeaderWidth<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowHeaderWidthProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T RowHeaderWidth<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowHeaderWidthProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T RowHeaderWidth<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.RowHeaderWidthProperty, ps, () => control.RowHeaderWidth = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SelectionModeProperty

/*BindFromExpressionSetterGenerator*/
public static T SelectionMode<T>(this T control, Func<Avalonia.Controls.DataGridSelectionMode> func, Action<Avalonia.Controls.DataGridSelectionMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.SelectionModeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SelectionMode<T>(this T control, Avalonia.Controls.DataGridSelectionMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.SelectionModeProperty, ps, () => control.SelectionMode = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SelectionMode<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.SelectionModeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SelectionMode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.SelectionModeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SelectionMode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.DataGridSelectionMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.SelectionModeProperty, ps, () => control.SelectionMode = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // VerticalGridLinesBrushProperty

/*BindFromExpressionSetterGenerator*/
public static T VerticalGridLinesBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.VerticalGridLinesBrushProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T VerticalGridLinesBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.VerticalGridLinesBrushProperty, ps, () => control.VerticalGridLinesBrush = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T VerticalGridLinesBrush<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.VerticalGridLinesBrushProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T VerticalGridLinesBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.VerticalGridLinesBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T VerticalGridLinesBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.VerticalGridLinesBrushProperty, ps, () => control.VerticalGridLinesBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // VerticalScrollBarVisibilityProperty

/*BindFromExpressionSetterGenerator*/
public static T VerticalScrollBarVisibility<T>(this T control, Func<Avalonia.Controls.Primitives.ScrollBarVisibility> func, Action<Avalonia.Controls.Primitives.ScrollBarVisibility>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.VerticalScrollBarVisibilityProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T VerticalScrollBarVisibility<T>(this T control, Avalonia.Controls.Primitives.ScrollBarVisibility value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.VerticalScrollBarVisibilityProperty, ps, () => control.VerticalScrollBarVisibility = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T VerticalScrollBarVisibility<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.VerticalScrollBarVisibilityProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T VerticalScrollBarVisibility<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.VerticalScrollBarVisibilityProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T VerticalScrollBarVisibility<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Primitives.ScrollBarVisibility> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.VerticalScrollBarVisibilityProperty, ps, () => control.VerticalScrollBarVisibility = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // DropLocationIndicatorTemplateProperty

/*BindFromExpressionSetterGenerator*/
public static T DropLocationIndicatorTemplate<T>(this T control, Func<Avalonia.Controls.ITemplate<Avalonia.Controls.Control>> func, Action<Avalonia.Controls.ITemplate<Avalonia.Controls.Control>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.DropLocationIndicatorTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T DropLocationIndicatorTemplate<T>(this T control, Avalonia.Controls.ITemplate<Avalonia.Controls.Control> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.DropLocationIndicatorTemplateProperty, ps, () => control.DropLocationIndicatorTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T DropLocationIndicatorTemplate<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.DropLocationIndicatorTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T DropLocationIndicatorTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.DropLocationIndicatorTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T DropLocationIndicatorTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.ITemplate<Avalonia.Controls.Control>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.DropLocationIndicatorTemplateProperty, ps, () => control.DropLocationIndicatorTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SelectedIndexProperty

/*BindFromExpressionSetterGenerator*/
public static T SelectedIndex<T>(this T control, Func<System.Int32> func, Action<System.Int32>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.SelectedIndexProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SelectedIndex<T>(this T control, System.Int32 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.SelectedIndexProperty, ps, () => control.SelectedIndex = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SelectedIndex<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.SelectedIndexProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SelectedIndex<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.SelectedIndexProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SelectedIndex<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Int32> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.SelectedIndexProperty, ps, () => control.SelectedIndex = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SelectedItemProperty

/*BindFromExpressionSetterGenerator*/
public static T SelectedItem<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.SelectedItemProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SelectedItem<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.SelectedItemProperty, ps, () => control.SelectedItem = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SelectedItem<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.SelectedItemProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SelectedItem<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.SelectedItemProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SelectedItem<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.SelectedItemProperty, ps, () => control.SelectedItem = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ClipboardCopyModeProperty

/*BindFromExpressionSetterGenerator*/
public static T ClipboardCopyMode<T>(this T control, Func<Avalonia.Controls.DataGridClipboardCopyMode> func, Action<Avalonia.Controls.DataGridClipboardCopyMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.ClipboardCopyModeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ClipboardCopyMode<T>(this T control, Avalonia.Controls.DataGridClipboardCopyMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.ClipboardCopyModeProperty, ps, () => control.ClipboardCopyMode = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ClipboardCopyMode<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.ClipboardCopyModeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ClipboardCopyMode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.ClipboardCopyModeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ClipboardCopyMode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.DataGridClipboardCopyMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.ClipboardCopyModeProperty, ps, () => control.ClipboardCopyMode = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // AutoGenerateColumnsProperty

/*BindFromExpressionSetterGenerator*/
public static T AutoGenerateColumns<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.AutoGenerateColumnsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T AutoGenerateColumns<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.AutoGenerateColumnsProperty, ps, () => control.AutoGenerateColumns = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T AutoGenerateColumns<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.AutoGenerateColumnsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T AutoGenerateColumns<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.AutoGenerateColumnsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T AutoGenerateColumns<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.AutoGenerateColumnsProperty, ps, () => control.AutoGenerateColumns = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ItemsSourceProperty

/*BindFromExpressionSetterGenerator*/
public static T ItemsSource<T>(this T control, Func<System.Collections.IEnumerable> func, Action<System.Collections.IEnumerable>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.ItemsSourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ItemsSource<T>(this T control, System.Collections.IEnumerable value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.ItemsSourceProperty, ps, () => control.ItemsSource = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ItemsSource<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.ItemsSourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ItemsSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.ItemsSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ItemsSource<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.IEnumerable> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.ItemsSourceProperty, ps, () => control.ItemsSource = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // AreRowDetailsFrozenProperty

/*BindFromExpressionSetterGenerator*/
public static T AreRowDetailsFrozen<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.AreRowDetailsFrozenProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T AreRowDetailsFrozen<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.AreRowDetailsFrozenProperty, ps, () => control.AreRowDetailsFrozen = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T AreRowDetailsFrozen<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.AreRowDetailsFrozenProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T AreRowDetailsFrozen<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.AreRowDetailsFrozenProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T AreRowDetailsFrozen<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.AreRowDetailsFrozenProperty, ps, () => control.AreRowDetailsFrozen = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // RowDetailsTemplateProperty

/*BindFromExpressionSetterGenerator*/
public static T RowDetailsTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowDetailsTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T RowDetailsTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.RowDetailsTemplateProperty, ps, () => control.RowDetailsTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T RowDetailsTemplate<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowDetailsTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T RowDetailsTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowDetailsTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T RowDetailsTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.RowDetailsTemplateProperty, ps, () => control.RowDetailsTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // RowDetailsVisibilityModeProperty

/*BindFromExpressionSetterGenerator*/
public static T RowDetailsVisibilityMode<T>(this T control, Func<Avalonia.Controls.DataGridRowDetailsVisibilityMode> func, Action<Avalonia.Controls.DataGridRowDetailsVisibilityMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowDetailsVisibilityModeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T RowDetailsVisibilityMode<T>(this T control, Avalonia.Controls.DataGridRowDetailsVisibilityMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.RowDetailsVisibilityModeProperty, ps, () => control.RowDetailsVisibilityMode = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T RowDetailsVisibilityMode<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowDetailsVisibilityModeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T RowDetailsVisibilityMode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGrid
   => control._set(Avalonia.Controls.DataGrid.RowDetailsVisibilityModeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T RowDetailsVisibilityMode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.DataGridRowDetailsVisibilityMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGrid
=> control._setEx(Avalonia.Controls.DataGrid.RowDetailsVisibilityModeProperty, ps, () => control.RowDetailsVisibilityMode = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // HorizontalScroll

/*ActionToEventGenerator*/
    public static T OnHorizontalScroll<T>(this T control, Action<Avalonia.Controls.Primitives.ScrollEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.Primitives.ScrollEventArgs>) ((arg0, arg1) => action(arg1)), h => control.HorizontalScroll += h);


 // VerticalScroll

/*ActionToEventGenerator*/
    public static T OnVerticalScroll<T>(this T control, Action<Avalonia.Controls.Primitives.ScrollEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.Primitives.ScrollEventArgs>) ((arg0, arg1) => action(arg1)), h => control.VerticalScroll += h);


 // AutoGeneratingColumn

/*ActionToEventGenerator*/
    public static T OnAutoGeneratingColumn<T>(this T control, Action<Avalonia.Controls.DataGridAutoGeneratingColumnEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridAutoGeneratingColumnEventArgs>) ((arg0, arg1) => action(arg1)), h => control.AutoGeneratingColumn += h);


 // BeginningEdit

/*ActionToEventGenerator*/
    public static T OnBeginningEdit<T>(this T control, Action<Avalonia.Controls.DataGridBeginningEditEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridBeginningEditEventArgs>) ((arg0, arg1) => action(arg1)), h => control.BeginningEdit += h);


 // CellEditEnded

/*ActionToEventGenerator*/
    public static T OnCellEditEnded<T>(this T control, Action<Avalonia.Controls.DataGridCellEditEndedEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridCellEditEndedEventArgs>) ((arg0, arg1) => action(arg1)), h => control.CellEditEnded += h);


 // CellEditEnding

/*ActionToEventGenerator*/
    public static T OnCellEditEnding<T>(this T control, Action<Avalonia.Controls.DataGridCellEditEndingEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridCellEditEndingEventArgs>) ((arg0, arg1) => action(arg1)), h => control.CellEditEnding += h);


 // CellPointerPressed

/*ActionToEventGenerator*/
    public static T OnCellPointerPressed<T>(this T control, Action<Avalonia.Controls.DataGridCellPointerPressedEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridCellPointerPressedEventArgs>) ((arg0, arg1) => action(arg1)), h => control.CellPointerPressed += h);


 // ColumnDisplayIndexChanged

/*ActionToEventGenerator*/
    public static T OnColumnDisplayIndexChanged<T>(this T control, Action<Avalonia.Controls.DataGridColumnEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridColumnEventArgs>) ((arg0, arg1) => action(arg1)), h => control.ColumnDisplayIndexChanged += h);


 // ColumnReordered

/*ActionToEventGenerator*/
    public static T OnColumnReordered<T>(this T control, Action<Avalonia.Controls.DataGridColumnEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridColumnEventArgs>) ((arg0, arg1) => action(arg1)), h => control.ColumnReordered += h);


 // ColumnReordering

/*ActionToEventGenerator*/
    public static T OnColumnReordering<T>(this T control, Action<Avalonia.Controls.DataGridColumnReorderingEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridColumnReorderingEventArgs>) ((arg0, arg1) => action(arg1)), h => control.ColumnReordering += h);


 // CurrentCellChanged

/*ActionToEventGenerator*/
    public static T OnCurrentCellChanged<T>(this T control, Action<System.EventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<System.EventArgs>) ((arg0, arg1) => action(arg1)), h => control.CurrentCellChanged += h);


 // LoadingRow

/*ActionToEventGenerator*/
    public static T OnLoadingRow<T>(this T control, Action<Avalonia.Controls.DataGridRowEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridRowEventArgs>) ((arg0, arg1) => action(arg1)), h => control.LoadingRow += h);


 // PreparingCellForEdit

/*ActionToEventGenerator*/
    public static T OnPreparingCellForEdit<T>(this T control, Action<Avalonia.Controls.DataGridPreparingCellForEditEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridPreparingCellForEditEventArgs>) ((arg0, arg1) => action(arg1)), h => control.PreparingCellForEdit += h);


 // RowEditEnded

/*ActionToEventGenerator*/
    public static T OnRowEditEnded<T>(this T control, Action<Avalonia.Controls.DataGridRowEditEndedEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridRowEditEndedEventArgs>) ((arg0, arg1) => action(arg1)), h => control.RowEditEnded += h);


 // RowEditEnding

/*ActionToEventGenerator*/
    public static T OnRowEditEnding<T>(this T control, Action<Avalonia.Controls.DataGridRowEditEndingEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridRowEditEndingEventArgs>) ((arg0, arg1) => action(arg1)), h => control.RowEditEnding += h);


 // SelectionChanged

/*ActionToEventGenerator*/
    public static T OnSelectionChanged<T>(this T control, Action<Avalonia.Controls.SelectionChangedEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.SelectionChangedEventArgs>) ((arg0, arg1) => action(arg1)), h => control.SelectionChanged += h);


 // Sorting

/*ActionToEventGenerator*/
    public static T OnSorting<T>(this T control, Action<Avalonia.Controls.DataGridColumnEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridColumnEventArgs>) ((arg0, arg1) => action(arg1)), h => control.Sorting += h);


 // UnloadingRow

/*ActionToEventGenerator*/
    public static T OnUnloadingRow<T>(this T control, Action<Avalonia.Controls.DataGridRowEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridRowEventArgs>) ((arg0, arg1) => action(arg1)), h => control.UnloadingRow += h);


 // LoadingRowDetails

/*ActionToEventGenerator*/
    public static T OnLoadingRowDetails<T>(this T control, Action<Avalonia.Controls.DataGridRowDetailsEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridRowDetailsEventArgs>) ((arg0, arg1) => action(arg1)), h => control.LoadingRowDetails += h);


 // RowDetailsVisibilityChanged

/*ActionToEventGenerator*/
    public static T OnRowDetailsVisibilityChanged<T>(this T control, Action<Avalonia.Controls.DataGridRowDetailsEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridRowDetailsEventArgs>) ((arg0, arg1) => action(arg1)), h => control.RowDetailsVisibilityChanged += h);


 // UnloadingRowDetails

/*ActionToEventGenerator*/
    public static T OnUnloadingRowDetails<T>(this T control, Action<Avalonia.Controls.DataGridRowDetailsEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridRowDetailsEventArgs>) ((arg0, arg1) => action(arg1)), h => control.UnloadingRowDetails += h);


 // LoadingRowGroup

/*ActionToEventGenerator*/
    public static T OnLoadingRowGroup<T>(this T control, Action<Avalonia.Controls.DataGridRowGroupHeaderEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridRowGroupHeaderEventArgs>) ((arg0, arg1) => action(arg1)), h => control.LoadingRowGroup += h);


 // UnloadingRowGroup

/*ActionToEventGenerator*/
    public static T OnUnloadingRowGroup<T>(this T control, Action<Avalonia.Controls.DataGridRowGroupHeaderEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridRowGroupHeaderEventArgs>) ((arg0, arg1) => action(arg1)), h => control.UnloadingRowGroup += h);


 // CopyingRowClipboardContent

/*ActionToEventGenerator*/
    public static T OnCopyingRowClipboardContent<T>(this T control, Action<Avalonia.Controls.DataGridRowClipboardEventArgs> action) where T : Avalonia.Controls.DataGrid => 
        control._setEvent((System.EventHandler<Avalonia.Controls.DataGridRowClipboardEventArgs>) ((arg0, arg1) => action(arg1)), h => control.CopyingRowClipboardContent += h);



//================= Styles ======================//
 // CanUserReorderColumnsProperty

/*ValueStyleSetterGenerator*/
public static Style<T> CanUserReorderColumns<T>(this Style<T> style, System.Boolean value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.CanUserReorderColumnsProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CanUserReorderColumns<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.CanUserReorderColumnsProperty, binding);


 // CanUserResizeColumnsProperty

/*ValueStyleSetterGenerator*/
public static Style<T> CanUserResizeColumns<T>(this Style<T> style, System.Boolean value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.CanUserResizeColumnsProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CanUserResizeColumns<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.CanUserResizeColumnsProperty, binding);


 // CanUserSortColumnsProperty

/*ValueStyleSetterGenerator*/
public static Style<T> CanUserSortColumns<T>(this Style<T> style, System.Boolean value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.CanUserSortColumnsProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CanUserSortColumns<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.CanUserSortColumnsProperty, binding);


 // ColumnHeaderHeightProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ColumnHeaderHeight<T>(this Style<T> style, System.Double value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.ColumnHeaderHeightProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ColumnHeaderHeight<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.ColumnHeaderHeightProperty, binding);


 // ColumnWidthProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ColumnWidth<T>(this Style<T> style, Avalonia.Controls.DataGridLength value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.ColumnWidthProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ColumnWidth<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.ColumnWidthProperty, binding);

/*ValueOverloadsStyleSetterGenerator*/
public static Style<T> ColumnWidth<T>(this Style<T> style, System.Double value) where T : Avalonia.Controls.DataGrid
   => style._addSetter(Avalonia.Controls.DataGrid.ColumnWidthProperty, new Avalonia.Controls.DataGridLength(value));public static Style<T> ColumnWidth<T>(this Style<T> style, System.Double value, Avalonia.Controls.DataGridLengthUnitType type) where T : Avalonia.Controls.DataGrid
   => style._addSetter(Avalonia.Controls.DataGrid.ColumnWidthProperty, new Avalonia.Controls.DataGridLength(value, type));public static Style<T> ColumnWidth<T>(this Style<T> style, System.Double value, Avalonia.Controls.DataGridLengthUnitType type, System.Double desiredValue, System.Double displayValue) where T : Avalonia.Controls.DataGrid
   => style._addSetter(Avalonia.Controls.DataGrid.ColumnWidthProperty, new Avalonia.Controls.DataGridLength(value, type, desiredValue, displayValue));


 // RowThemeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> RowTheme<T>(this Style<T> style, Avalonia.Styling.ControlTheme value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.RowThemeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> RowTheme<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.RowThemeProperty, binding);


 // CellThemeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> CellTheme<T>(this Style<T> style, Avalonia.Styling.ControlTheme value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.CellThemeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CellTheme<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.CellThemeProperty, binding);


 // ColumnHeaderThemeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ColumnHeaderTheme<T>(this Style<T> style, Avalonia.Styling.ControlTheme value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.ColumnHeaderThemeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ColumnHeaderTheme<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.ColumnHeaderThemeProperty, binding);


 // RowGroupThemeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> RowGroupTheme<T>(this Style<T> style, Avalonia.Styling.ControlTheme value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.RowGroupThemeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> RowGroupTheme<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.RowGroupThemeProperty, binding);


 // FrozenColumnCountProperty

/*ValueStyleSetterGenerator*/
public static Style<T> FrozenColumnCount<T>(this Style<T> style, System.Int32 value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.FrozenColumnCountProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> FrozenColumnCount<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.FrozenColumnCountProperty, binding);


 // GridLinesVisibilityProperty

/*ValueStyleSetterGenerator*/
public static Style<T> GridLinesVisibility<T>(this Style<T> style, Avalonia.Controls.DataGridGridLinesVisibility value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.GridLinesVisibilityProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> GridLinesVisibility<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.GridLinesVisibilityProperty, binding);


 // HeadersVisibilityProperty

/*ValueStyleSetterGenerator*/
public static Style<T> HeadersVisibility<T>(this Style<T> style, Avalonia.Controls.DataGridHeadersVisibility value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.HeadersVisibilityProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> HeadersVisibility<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.HeadersVisibilityProperty, binding);


 // HorizontalGridLinesBrushProperty

/*ValueStyleSetterGenerator*/
public static Style<T> HorizontalGridLinesBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.HorizontalGridLinesBrushProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> HorizontalGridLinesBrush<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.HorizontalGridLinesBrushProperty, binding);


 // HorizontalScrollBarVisibilityProperty

/*ValueStyleSetterGenerator*/
public static Style<T> HorizontalScrollBarVisibility<T>(this Style<T> style, Avalonia.Controls.Primitives.ScrollBarVisibility value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.HorizontalScrollBarVisibilityProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> HorizontalScrollBarVisibility<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.HorizontalScrollBarVisibilityProperty, binding);


 // IsReadOnlyProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsReadOnly<T>(this Style<T> style, System.Boolean value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.IsReadOnlyProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsReadOnly<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.IsReadOnlyProperty, binding);


 // AreRowGroupHeadersFrozenProperty

/*ValueStyleSetterGenerator*/
public static Style<T> AreRowGroupHeadersFrozen<T>(this Style<T> style, System.Boolean value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.AreRowGroupHeadersFrozenProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> AreRowGroupHeadersFrozen<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.AreRowGroupHeadersFrozenProperty, binding);


 // IsScrollInertiaEnabledProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsScrollInertiaEnabled<T>(this Style<T> style, System.Boolean value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.IsScrollInertiaEnabledProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsScrollInertiaEnabled<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.IsScrollInertiaEnabledProperty, binding);


 // MaxColumnWidthProperty

/*ValueStyleSetterGenerator*/
public static Style<T> MaxColumnWidth<T>(this Style<T> style, System.Double value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.MaxColumnWidthProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> MaxColumnWidth<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.MaxColumnWidthProperty, binding);


 // MinColumnWidthProperty

/*ValueStyleSetterGenerator*/
public static Style<T> MinColumnWidth<T>(this Style<T> style, System.Double value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.MinColumnWidthProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> MinColumnWidth<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.MinColumnWidthProperty, binding);


 // RowBackgroundProperty

/*ValueStyleSetterGenerator*/
public static Style<T> RowBackground<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.RowBackgroundProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> RowBackground<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.RowBackgroundProperty, binding);


 // RowHeightProperty

/*ValueStyleSetterGenerator*/
public static Style<T> RowHeight<T>(this Style<T> style, System.Double value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.RowHeightProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> RowHeight<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.RowHeightProperty, binding);


 // RowHeaderWidthProperty

/*ValueStyleSetterGenerator*/
public static Style<T> RowHeaderWidth<T>(this Style<T> style, System.Double value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.RowHeaderWidthProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> RowHeaderWidth<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.RowHeaderWidthProperty, binding);


 // SelectionModeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> SelectionMode<T>(this Style<T> style, Avalonia.Controls.DataGridSelectionMode value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.SelectionModeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SelectionMode<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.SelectionModeProperty, binding);


 // VerticalGridLinesBrushProperty

/*ValueStyleSetterGenerator*/
public static Style<T> VerticalGridLinesBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.VerticalGridLinesBrushProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> VerticalGridLinesBrush<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.VerticalGridLinesBrushProperty, binding);


 // VerticalScrollBarVisibilityProperty

/*ValueStyleSetterGenerator*/
public static Style<T> VerticalScrollBarVisibility<T>(this Style<T> style, Avalonia.Controls.Primitives.ScrollBarVisibility value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.VerticalScrollBarVisibilityProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> VerticalScrollBarVisibility<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.VerticalScrollBarVisibilityProperty, binding);


 // DropLocationIndicatorTemplateProperty

/*ValueStyleSetterGenerator*/
public static Style<T> DropLocationIndicatorTemplate<T>(this Style<T> style, Avalonia.Controls.ITemplate<Avalonia.Controls.Control> value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.DropLocationIndicatorTemplateProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> DropLocationIndicatorTemplate<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.DropLocationIndicatorTemplateProperty, binding);


 // ClipboardCopyModeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ClipboardCopyMode<T>(this Style<T> style, Avalonia.Controls.DataGridClipboardCopyMode value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.ClipboardCopyModeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ClipboardCopyMode<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.ClipboardCopyModeProperty, binding);


 // AutoGenerateColumnsProperty

/*ValueStyleSetterGenerator*/
public static Style<T> AutoGenerateColumns<T>(this Style<T> style, System.Boolean value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.AutoGenerateColumnsProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> AutoGenerateColumns<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.AutoGenerateColumnsProperty, binding);


 // ItemsSourceProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ItemsSource<T>(this Style<T> style, System.Collections.IEnumerable value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.ItemsSourceProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ItemsSource<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.ItemsSourceProperty, binding);


 // AreRowDetailsFrozenProperty

/*ValueStyleSetterGenerator*/
public static Style<T> AreRowDetailsFrozen<T>(this Style<T> style, System.Boolean value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.AreRowDetailsFrozenProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> AreRowDetailsFrozen<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.AreRowDetailsFrozenProperty, binding);


 // RowDetailsTemplateProperty

/*ValueStyleSetterGenerator*/
public static Style<T> RowDetailsTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.RowDetailsTemplateProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> RowDetailsTemplate<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.RowDetailsTemplateProperty, binding);


 // RowDetailsVisibilityModeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> RowDetailsVisibilityMode<T>(this Style<T> style, Avalonia.Controls.DataGridRowDetailsVisibilityMode value) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.RowDetailsVisibilityModeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> RowDetailsVisibilityMode<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGrid
=> style._addSetter(Avalonia.Controls.DataGrid.RowDetailsVisibilityModeProperty, binding);



}
