#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
[global::System.CodeDom.Compiler.GeneratedCode("AvaloniaExtensionGenerator", "1.0.0.0")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static partial class TreeDataGrid_MarkupExtensions
{
//================= Properties ======================//
 // AutoDragDropRows

/*BindFromExpressionSetterGenerator*/
public static T AutoDragDropRows<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.TreeDataGrid 
   => control._set(Avalonia.Controls.TreeDataGrid.AutoDragDropRowsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T AutoDragDropRows<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.TreeDataGrid 
=> control._setEx(Avalonia.Controls.TreeDataGrid.AutoDragDropRowsProperty, ps, () => control.AutoDragDropRows = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T AutoDragDropRows<T>(this T control, IBinding binding) where T : Avalonia.Controls.TreeDataGrid 
   => control._set(Avalonia.Controls.TreeDataGrid.AutoDragDropRowsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T AutoDragDropRows<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.TreeDataGrid 
   => control._set(Avalonia.Controls.TreeDataGrid.AutoDragDropRowsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T AutoDragDropRows<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.TreeDataGrid 
=> control._setEx(Avalonia.Controls.TreeDataGrid.AutoDragDropRowsProperty, ps, () => control.AutoDragDropRows = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CanUserResizeColumns

/*BindFromExpressionSetterGenerator*/
public static T CanUserResizeColumns<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.TreeDataGrid 
   => control._set(Avalonia.Controls.TreeDataGrid.CanUserResizeColumnsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CanUserResizeColumns<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.TreeDataGrid 
=> control._setEx(Avalonia.Controls.TreeDataGrid.CanUserResizeColumnsProperty, ps, () => control.CanUserResizeColumns = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CanUserResizeColumns<T>(this T control, IBinding binding) where T : Avalonia.Controls.TreeDataGrid 
   => control._set(Avalonia.Controls.TreeDataGrid.CanUserResizeColumnsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CanUserResizeColumns<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.TreeDataGrid 
   => control._set(Avalonia.Controls.TreeDataGrid.CanUserResizeColumnsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CanUserResizeColumns<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.TreeDataGrid 
=> control._setEx(Avalonia.Controls.TreeDataGrid.CanUserResizeColumnsProperty, ps, () => control.CanUserResizeColumns = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CanUserSortColumns

/*BindFromExpressionSetterGenerator*/
public static T CanUserSortColumns<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.TreeDataGrid 
   => control._set(Avalonia.Controls.TreeDataGrid.CanUserSortColumnsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CanUserSortColumns<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.TreeDataGrid 
=> control._setEx(Avalonia.Controls.TreeDataGrid.CanUserSortColumnsProperty, ps, () => control.CanUserSortColumns = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CanUserSortColumns<T>(this T control, IBinding binding) where T : Avalonia.Controls.TreeDataGrid 
   => control._set(Avalonia.Controls.TreeDataGrid.CanUserSortColumnsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CanUserSortColumns<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.TreeDataGrid 
   => control._set(Avalonia.Controls.TreeDataGrid.CanUserSortColumnsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CanUserSortColumns<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.TreeDataGrid 
=> control._setEx(Avalonia.Controls.TreeDataGrid.CanUserSortColumnsProperty, ps, () => control.CanUserSortColumns = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ElementFactory

/*BindFromExpressionSetterGenerator*/
public static T ElementFactory<T>(this T control, Func<Avalonia.Controls.Primitives.TreeDataGridElementFactory> func, Action<Avalonia.Controls.Primitives.TreeDataGridElementFactory>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.TreeDataGrid 
   => control._set(Avalonia.Controls.TreeDataGrid.ElementFactoryProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ElementFactory<T>(this T control,Avalonia.Controls.Primitives.TreeDataGridElementFactory value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.TreeDataGrid 
=> control._setEx(Avalonia.Controls.TreeDataGrid.ElementFactoryProperty, ps, () => control.ElementFactory = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ElementFactory<T>(this T control, IBinding binding) where T : Avalonia.Controls.TreeDataGrid 
   => control._set(Avalonia.Controls.TreeDataGrid.ElementFactoryProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ElementFactory<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.TreeDataGrid 
   => control._set(Avalonia.Controls.TreeDataGrid.ElementFactoryProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ElementFactory<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Primitives.TreeDataGridElementFactory> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.TreeDataGrid 
=> control._setEx(Avalonia.Controls.TreeDataGrid.ElementFactoryProperty, ps, () => control.ElementFactory = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ShowColumnHeaders

/*BindFromExpressionSetterGenerator*/
public static T ShowColumnHeaders<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.TreeDataGrid 
   => control._set(Avalonia.Controls.TreeDataGrid.ShowColumnHeadersProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ShowColumnHeaders<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.TreeDataGrid 
=> control._setEx(Avalonia.Controls.TreeDataGrid.ShowColumnHeadersProperty, ps, () => control.ShowColumnHeaders = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ShowColumnHeaders<T>(this T control, IBinding binding) where T : Avalonia.Controls.TreeDataGrid 
   => control._set(Avalonia.Controls.TreeDataGrid.ShowColumnHeadersProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ShowColumnHeaders<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.TreeDataGrid 
   => control._set(Avalonia.Controls.TreeDataGrid.ShowColumnHeadersProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ShowColumnHeaders<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.TreeDataGrid 
=> control._setEx(Avalonia.Controls.TreeDataGrid.ShowColumnHeadersProperty, ps, () => control.ShowColumnHeaders = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // Source

/*BindFromExpressionSetterGenerator*/
public static T Source<T>(this T control, Func<Avalonia.Controls.ITreeDataGridSource> func, Action<Avalonia.Controls.ITreeDataGridSource>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.TreeDataGrid 
   => control._set(Avalonia.Controls.TreeDataGrid.SourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Source<T>(this T control,Avalonia.Controls.ITreeDataGridSource value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.TreeDataGrid 
=> control._setEx(Avalonia.Controls.TreeDataGrid.SourceProperty, ps, () => control.Source = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Source<T>(this T control, IBinding binding) where T : Avalonia.Controls.TreeDataGrid 
   => control._set(Avalonia.Controls.TreeDataGrid.SourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Source<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.TreeDataGrid 
   => control._set(Avalonia.Controls.TreeDataGrid.SourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Source<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.ITreeDataGridSource> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.TreeDataGrid 
=> control._setEx(Avalonia.Controls.TreeDataGrid.SourceProperty, ps, () => control.Source = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // CellClearing

/*ActionToEventGenerator*/
public static T OnCellClearing<T>(this T control, Action<Avalonia.Controls.TreeDataGridCellEventArgs> action) where T : Avalonia.Controls.TreeDataGrid  => 
 control._setEvent((System.EventHandler<Avalonia.Controls.TreeDataGridCellEventArgs>) ((arg0, arg1) => action(arg1)), h => control.CellClearing += h);


 // CellPrepared

/*ActionToEventGenerator*/
public static T OnCellPrepared<T>(this T control, Action<Avalonia.Controls.TreeDataGridCellEventArgs> action) where T : Avalonia.Controls.TreeDataGrid  => 
 control._setEvent((System.EventHandler<Avalonia.Controls.TreeDataGridCellEventArgs>) ((arg0, arg1) => action(arg1)), h => control.CellPrepared += h);


 // CellValueChanged

/*ActionToEventGenerator*/
public static T OnCellValueChanged<T>(this T control, Action<Avalonia.Controls.TreeDataGridCellEventArgs> action) where T : Avalonia.Controls.TreeDataGrid  => 
 control._setEvent((System.EventHandler<Avalonia.Controls.TreeDataGridCellEventArgs>) ((arg0, arg1) => action(arg1)), h => control.CellValueChanged += h);


 // RowClearing

/*ActionToEventGenerator*/
public static T OnRowClearing<T>(this T control, Action<Avalonia.Controls.TreeDataGridRowEventArgs> action) where T : Avalonia.Controls.TreeDataGrid  => 
 control._setEvent((System.EventHandler<Avalonia.Controls.TreeDataGridRowEventArgs>) ((arg0, arg1) => action(arg1)), h => control.RowClearing += h);


 // RowPrepared

/*ActionToEventGenerator*/
public static T OnRowPrepared<T>(this T control, Action<Avalonia.Controls.TreeDataGridRowEventArgs> action) where T : Avalonia.Controls.TreeDataGrid  => 
 control._setEvent((System.EventHandler<Avalonia.Controls.TreeDataGridRowEventArgs>) ((arg0, arg1) => action(arg1)), h => control.RowPrepared += h);


 // RowDragStarted

/*ActionToEventGenerator*/
public static T OnRowDragStarted<T>(this T control, Action<Avalonia.Controls.TreeDataGridRowDragStartedEventArgs> action, Avalonia.Interactivity.RoutingStrategies routes = Avalonia.Interactivity.RoutingStrategies.Tunnel | Avalonia.Interactivity.RoutingStrategies.Bubble) where T : Avalonia.Controls.TreeDataGrid 
{
  control.AddHandler(Avalonia.Controls.TreeDataGrid.RowDragStartedEvent, (_, args) => action(args), routes);
  return control; 
}



 // RowDragOver

/*ActionToEventGenerator*/
public static T OnRowDragOver<T>(this T control, Action<Avalonia.Controls.TreeDataGridRowDragEventArgs> action, Avalonia.Interactivity.RoutingStrategies routes = Avalonia.Interactivity.RoutingStrategies.Tunnel | Avalonia.Interactivity.RoutingStrategies.Bubble) where T : Avalonia.Controls.TreeDataGrid 
{
  control.AddHandler(Avalonia.Controls.TreeDataGrid.RowDragOverEvent, (_, args) => action(args), routes);
  return control; 
}



 // RowDrop

/*ActionToEventGenerator*/
public static T OnRowDrop<T>(this T control, Action<Avalonia.Controls.TreeDataGridRowDragEventArgs> action, Avalonia.Interactivity.RoutingStrategies routes = Avalonia.Interactivity.RoutingStrategies.Tunnel | Avalonia.Interactivity.RoutingStrategies.Bubble) where T : Avalonia.Controls.TreeDataGrid 
{
  control.AddHandler(Avalonia.Controls.TreeDataGrid.RowDropEvent, (_, args) => action(args), routes);
  return control; 
}



 // SelectionChanging

/*ActionToEventGenerator*/
public static T OnSelectionChanging<T>(this T control, Action<System.ComponentModel.CancelEventArgs> action) where T : Avalonia.Controls.TreeDataGrid  => 
 control._setEvent((System.ComponentModel.CancelEventHandler) ((arg0, arg1) => action(arg1)), h => control.SelectionChanging += h);



//================= Styles ======================//
 // AutoDragDropRows

/*ValueStyleSetterGenerator*/
public static Style<T> AutoDragDropRows<T>(this Style<T> style, System.Boolean value) where T : Avalonia.Controls.TreeDataGrid 
=> style._addSetter(Avalonia.Controls.TreeDataGrid.AutoDragDropRowsProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> AutoDragDropRows<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.TreeDataGrid 
=> style._addSetter(Avalonia.Controls.TreeDataGrid.AutoDragDropRowsProperty, binding);


 // CanUserResizeColumns

/*ValueStyleSetterGenerator*/
public static Style<T> CanUserResizeColumns<T>(this Style<T> style, System.Boolean value) where T : Avalonia.Controls.TreeDataGrid 
=> style._addSetter(Avalonia.Controls.TreeDataGrid.CanUserResizeColumnsProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CanUserResizeColumns<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.TreeDataGrid 
=> style._addSetter(Avalonia.Controls.TreeDataGrid.CanUserResizeColumnsProperty, binding);


 // CanUserSortColumns

/*ValueStyleSetterGenerator*/
public static Style<T> CanUserSortColumns<T>(this Style<T> style, System.Boolean value) where T : Avalonia.Controls.TreeDataGrid 
=> style._addSetter(Avalonia.Controls.TreeDataGrid.CanUserSortColumnsProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CanUserSortColumns<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.TreeDataGrid 
=> style._addSetter(Avalonia.Controls.TreeDataGrid.CanUserSortColumnsProperty, binding);


 // ShowColumnHeaders

/*ValueStyleSetterGenerator*/
public static Style<T> ShowColumnHeaders<T>(this Style<T> style, System.Boolean value) where T : Avalonia.Controls.TreeDataGrid 
=> style._addSetter(Avalonia.Controls.TreeDataGrid.ShowColumnHeadersProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ShowColumnHeaders<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.TreeDataGrid 
=> style._addSetter(Avalonia.Controls.TreeDataGrid.ShowColumnHeadersProperty, binding);



}
