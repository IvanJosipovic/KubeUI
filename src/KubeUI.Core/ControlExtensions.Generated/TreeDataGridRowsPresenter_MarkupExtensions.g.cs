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
public static partial class TreeDataGridRowsPresenter_MarkupExtensions
{
//================= Properties ======================//
 // Columns

/*BindFromExpressionSetterGenerator*/
public static T Columns<T>(this T control, Func<Avalonia.Controls.Models.TreeDataGrid.IColumns> func, Action<Avalonia.Controls.Models.TreeDataGrid.IColumns>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.Primitives.TreeDataGridRowsPresenter 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridRowsPresenter.ColumnsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Columns<T>(this T control,Avalonia.Controls.Models.TreeDataGrid.IColumns value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.Primitives.TreeDataGridRowsPresenter 
=> control._setEx(Avalonia.Controls.Primitives.TreeDataGridRowsPresenter.ColumnsProperty, ps, () => control.Columns = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Columns<T>(this T control, IBinding binding) where T : Avalonia.Controls.Primitives.TreeDataGridRowsPresenter 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridRowsPresenter.ColumnsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Columns<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.Primitives.TreeDataGridRowsPresenter 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridRowsPresenter.ColumnsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Columns<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Models.TreeDataGrid.IColumns> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.Primitives.TreeDataGridRowsPresenter 
=> control._setEx(Avalonia.Controls.Primitives.TreeDataGridRowsPresenter.ColumnsProperty, ps, () => control.Columns = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // ChildIndexChanged

/*ActionToEventGenerator*/
public static T OnChildIndexChanged<T>(this T control, Action<Avalonia.LogicalTree.ChildIndexChangedEventArgs> action) where T : Avalonia.Controls.Primitives.TreeDataGridRowsPresenter  => 
 control._setEvent((System.EventHandler<Avalonia.LogicalTree.ChildIndexChangedEventArgs>) ((arg0, arg1) => action(arg1)), h => control.ChildIndexChanged += h);



}
