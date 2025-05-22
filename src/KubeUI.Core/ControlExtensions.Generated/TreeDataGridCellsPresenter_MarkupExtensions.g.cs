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
public static partial class TreeDataGridCellsPresenter_MarkupExtensions
{
//================= Properties ======================//
 // Rows

/*BindFromExpressionSetterGenerator*/
public static T Rows<T>(this T control, Func<Avalonia.Controls.Models.TreeDataGrid.IRows> func, Action<Avalonia.Controls.Models.TreeDataGrid.IRows>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.Primitives.TreeDataGridCellsPresenter 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridCellsPresenter.RowsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Rows<T>(this T control,Avalonia.Controls.Models.TreeDataGrid.IRows value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.Primitives.TreeDataGridCellsPresenter 
=> control._setEx(Avalonia.Controls.Primitives.TreeDataGridCellsPresenter.RowsProperty, ps, () => control.Rows = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Rows<T>(this T control, IBinding binding) where T : Avalonia.Controls.Primitives.TreeDataGridCellsPresenter 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridCellsPresenter.RowsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Rows<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.Primitives.TreeDataGridCellsPresenter 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridCellsPresenter.RowsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Rows<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Models.TreeDataGrid.IRows> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.Primitives.TreeDataGridCellsPresenter 
=> control._setEx(Avalonia.Controls.Primitives.TreeDataGridCellsPresenter.RowsProperty, ps, () => control.Rows = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // ChildIndexChanged

/*ActionToEventGenerator*/
public static T OnChildIndexChanged<T>(this T control, Action<Avalonia.LogicalTree.ChildIndexChangedEventArgs> action) where T : Avalonia.Controls.Primitives.TreeDataGridCellsPresenter  => 
 control._setEvent((System.EventHandler<Avalonia.LogicalTree.ChildIndexChangedEventArgs>) ((arg0, arg1) => action(arg1)), h => control.ChildIndexChanged += h);



}
