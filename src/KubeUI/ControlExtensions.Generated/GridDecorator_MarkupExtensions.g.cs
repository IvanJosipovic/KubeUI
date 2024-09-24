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
public static partial class GridDecorator_MarkupExtensions
{
//================= Properties ======================//
 // EnableGridProperty

/*BindFromExpressionSetterGenerator*/
public static T EnableGrid<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.GridDecorator
   => control._set(NodeEditor.Controls.GridDecorator.EnableGridProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T EnableGrid<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.GridDecorator
=> control._setEx(NodeEditor.Controls.GridDecorator.EnableGridProperty, ps, () => control.EnableGrid = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T EnableGrid<T>(this T control, IBinding binding) where T : NodeEditor.Controls.GridDecorator
   => control._set(NodeEditor.Controls.GridDecorator.EnableGridProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T EnableGrid<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.GridDecorator
   => control._set(NodeEditor.Controls.GridDecorator.EnableGridProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T EnableGrid<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.GridDecorator
=> control._setEx(NodeEditor.Controls.GridDecorator.EnableGridProperty, ps, () => control.EnableGrid = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // GridCellWidthProperty

/*BindFromExpressionSetterGenerator*/
public static T GridCellWidth<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.GridDecorator
   => control._set(NodeEditor.Controls.GridDecorator.GridCellWidthProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T GridCellWidth<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.GridDecorator
=> control._setEx(NodeEditor.Controls.GridDecorator.GridCellWidthProperty, ps, () => control.GridCellWidth = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T GridCellWidth<T>(this T control, IBinding binding) where T : NodeEditor.Controls.GridDecorator
   => control._set(NodeEditor.Controls.GridDecorator.GridCellWidthProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T GridCellWidth<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.GridDecorator
   => control._set(NodeEditor.Controls.GridDecorator.GridCellWidthProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T GridCellWidth<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.GridDecorator
=> control._setEx(NodeEditor.Controls.GridDecorator.GridCellWidthProperty, ps, () => control.GridCellWidth = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // GridCellHeightProperty

/*BindFromExpressionSetterGenerator*/
public static T GridCellHeight<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.GridDecorator
   => control._set(NodeEditor.Controls.GridDecorator.GridCellHeightProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T GridCellHeight<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.GridDecorator
=> control._setEx(NodeEditor.Controls.GridDecorator.GridCellHeightProperty, ps, () => control.GridCellHeight = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T GridCellHeight<T>(this T control, IBinding binding) where T : NodeEditor.Controls.GridDecorator
   => control._set(NodeEditor.Controls.GridDecorator.GridCellHeightProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T GridCellHeight<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.GridDecorator
   => control._set(NodeEditor.Controls.GridDecorator.GridCellHeightProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T GridCellHeight<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.GridDecorator
=> control._setEx(NodeEditor.Controls.GridDecorator.GridCellHeightProperty, ps, () => control.GridCellHeight = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // EnableGridProperty

/*ValueStyleSetterGenerator*/
public static Style<T> EnableGrid<T>(this Style<T> style, System.Boolean value) where T : NodeEditor.Controls.GridDecorator
=> style._addSetter(NodeEditor.Controls.GridDecorator.EnableGridProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> EnableGrid<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.GridDecorator
=> style._addSetter(NodeEditor.Controls.GridDecorator.EnableGridProperty, binding);


 // GridCellWidthProperty

/*ValueStyleSetterGenerator*/
public static Style<T> GridCellWidth<T>(this Style<T> style, System.Double value) where T : NodeEditor.Controls.GridDecorator
=> style._addSetter(NodeEditor.Controls.GridDecorator.GridCellWidthProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> GridCellWidth<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.GridDecorator
=> style._addSetter(NodeEditor.Controls.GridDecorator.GridCellWidthProperty, binding);


 // GridCellHeightProperty

/*ValueStyleSetterGenerator*/
public static Style<T> GridCellHeight<T>(this Style<T> style, System.Double value) where T : NodeEditor.Controls.GridDecorator
=> style._addSetter(NodeEditor.Controls.GridDecorator.GridCellHeightProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> GridCellHeight<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.GridDecorator
=> style._addSetter(NodeEditor.Controls.GridDecorator.GridCellHeightProperty, binding);



}
