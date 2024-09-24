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
public static partial class DrawingNode_MarkupExtensions
{
//================= Properties ======================//
 // InputSourceProperty

/*BindFromExpressionSetterGenerator*/
public static T InputSource<T>(this T control, Func<Avalonia.Controls.Control> func, Action<Avalonia.Controls.Control>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.DrawingNode
   => control._set(NodeEditor.Controls.DrawingNode.InputSourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T InputSource<T>(this T control, Avalonia.Controls.Control value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.DrawingNode
=> control._setEx(NodeEditor.Controls.DrawingNode.InputSourceProperty, ps, () => control.InputSource = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T InputSource<T>(this T control, IBinding binding) where T : NodeEditor.Controls.DrawingNode
   => control._set(NodeEditor.Controls.DrawingNode.InputSourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T InputSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.DrawingNode
   => control._set(NodeEditor.Controls.DrawingNode.InputSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T InputSource<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Control> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.DrawingNode
=> control._setEx(NodeEditor.Controls.DrawingNode.InputSourceProperty, ps, () => control.InputSource = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // AdornerCanvasProperty

/*BindFromExpressionSetterGenerator*/
public static T AdornerCanvas<T>(this T control, Func<Avalonia.Controls.Canvas> func, Action<Avalonia.Controls.Canvas>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.DrawingNode
   => control._set(NodeEditor.Controls.DrawingNode.AdornerCanvasProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T AdornerCanvas<T>(this T control, Avalonia.Controls.Canvas value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.DrawingNode
=> control._setEx(NodeEditor.Controls.DrawingNode.AdornerCanvasProperty, ps, () => control.AdornerCanvas = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T AdornerCanvas<T>(this T control, IBinding binding) where T : NodeEditor.Controls.DrawingNode
   => control._set(NodeEditor.Controls.DrawingNode.AdornerCanvasProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T AdornerCanvas<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.DrawingNode
   => control._set(NodeEditor.Controls.DrawingNode.AdornerCanvasProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T AdornerCanvas<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Canvas> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.DrawingNode
=> control._setEx(NodeEditor.Controls.DrawingNode.AdornerCanvasProperty, ps, () => control.AdornerCanvas = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // EnableSnapProperty

/*BindFromExpressionSetterGenerator*/
public static T EnableSnap<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.DrawingNode
   => control._set(NodeEditor.Controls.DrawingNode.EnableSnapProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T EnableSnap<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.DrawingNode
=> control._setEx(NodeEditor.Controls.DrawingNode.EnableSnapProperty, ps, () => control.EnableSnap = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T EnableSnap<T>(this T control, IBinding binding) where T : NodeEditor.Controls.DrawingNode
   => control._set(NodeEditor.Controls.DrawingNode.EnableSnapProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T EnableSnap<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.DrawingNode
   => control._set(NodeEditor.Controls.DrawingNode.EnableSnapProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T EnableSnap<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.DrawingNode
=> control._setEx(NodeEditor.Controls.DrawingNode.EnableSnapProperty, ps, () => control.EnableSnap = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SnapXProperty

/*BindFromExpressionSetterGenerator*/
public static T SnapX<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.DrawingNode
   => control._set(NodeEditor.Controls.DrawingNode.SnapXProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SnapX<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.DrawingNode
=> control._setEx(NodeEditor.Controls.DrawingNode.SnapXProperty, ps, () => control.SnapX = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SnapX<T>(this T control, IBinding binding) where T : NodeEditor.Controls.DrawingNode
   => control._set(NodeEditor.Controls.DrawingNode.SnapXProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SnapX<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.DrawingNode
   => control._set(NodeEditor.Controls.DrawingNode.SnapXProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SnapX<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.DrawingNode
=> control._setEx(NodeEditor.Controls.DrawingNode.SnapXProperty, ps, () => control.SnapX = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SnapYProperty

/*BindFromExpressionSetterGenerator*/
public static T SnapY<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.DrawingNode
   => control._set(NodeEditor.Controls.DrawingNode.SnapYProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SnapY<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.DrawingNode
=> control._setEx(NodeEditor.Controls.DrawingNode.SnapYProperty, ps, () => control.SnapY = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SnapY<T>(this T control, IBinding binding) where T : NodeEditor.Controls.DrawingNode
   => control._set(NodeEditor.Controls.DrawingNode.SnapYProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SnapY<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.DrawingNode
   => control._set(NodeEditor.Controls.DrawingNode.SnapYProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SnapY<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.DrawingNode
=> control._setEx(NodeEditor.Controls.DrawingNode.SnapYProperty, ps, () => control.SnapY = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // EnableGridProperty

/*BindFromExpressionSetterGenerator*/
public static T EnableGrid<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.DrawingNode
   => control._set(NodeEditor.Controls.DrawingNode.EnableGridProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T EnableGrid<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.DrawingNode
=> control._setEx(NodeEditor.Controls.DrawingNode.EnableGridProperty, ps, () => control.EnableGrid = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T EnableGrid<T>(this T control, IBinding binding) where T : NodeEditor.Controls.DrawingNode
   => control._set(NodeEditor.Controls.DrawingNode.EnableGridProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T EnableGrid<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.DrawingNode
   => control._set(NodeEditor.Controls.DrawingNode.EnableGridProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T EnableGrid<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.DrawingNode
=> control._setEx(NodeEditor.Controls.DrawingNode.EnableGridProperty, ps, () => control.EnableGrid = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // GridCellWidthProperty

/*BindFromExpressionSetterGenerator*/
public static T GridCellWidth<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.DrawingNode
   => control._set(NodeEditor.Controls.DrawingNode.GridCellWidthProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T GridCellWidth<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.DrawingNode
=> control._setEx(NodeEditor.Controls.DrawingNode.GridCellWidthProperty, ps, () => control.GridCellWidth = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T GridCellWidth<T>(this T control, IBinding binding) where T : NodeEditor.Controls.DrawingNode
   => control._set(NodeEditor.Controls.DrawingNode.GridCellWidthProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T GridCellWidth<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.DrawingNode
   => control._set(NodeEditor.Controls.DrawingNode.GridCellWidthProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T GridCellWidth<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.DrawingNode
=> control._setEx(NodeEditor.Controls.DrawingNode.GridCellWidthProperty, ps, () => control.GridCellWidth = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // GridCellHeightProperty

/*BindFromExpressionSetterGenerator*/
public static T GridCellHeight<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.DrawingNode
   => control._set(NodeEditor.Controls.DrawingNode.GridCellHeightProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T GridCellHeight<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.DrawingNode
=> control._setEx(NodeEditor.Controls.DrawingNode.GridCellHeightProperty, ps, () => control.GridCellHeight = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T GridCellHeight<T>(this T control, IBinding binding) where T : NodeEditor.Controls.DrawingNode
   => control._set(NodeEditor.Controls.DrawingNode.GridCellHeightProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T GridCellHeight<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.DrawingNode
   => control._set(NodeEditor.Controls.DrawingNode.GridCellHeightProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T GridCellHeight<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.DrawingNode
=> control._setEx(NodeEditor.Controls.DrawingNode.GridCellHeightProperty, ps, () => control.GridCellHeight = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // InputSourceProperty

/*ValueStyleSetterGenerator*/
public static Style<T> InputSource<T>(this Style<T> style, Avalonia.Controls.Control value) where T : NodeEditor.Controls.DrawingNode
=> style._addSetter(NodeEditor.Controls.DrawingNode.InputSourceProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> InputSource<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.DrawingNode
=> style._addSetter(NodeEditor.Controls.DrawingNode.InputSourceProperty, binding);


 // AdornerCanvasProperty

/*ValueStyleSetterGenerator*/
public static Style<T> AdornerCanvas<T>(this Style<T> style, Avalonia.Controls.Canvas value) where T : NodeEditor.Controls.DrawingNode
=> style._addSetter(NodeEditor.Controls.DrawingNode.AdornerCanvasProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> AdornerCanvas<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.DrawingNode
=> style._addSetter(NodeEditor.Controls.DrawingNode.AdornerCanvasProperty, binding);


 // EnableSnapProperty

/*ValueStyleSetterGenerator*/
public static Style<T> EnableSnap<T>(this Style<T> style, System.Boolean value) where T : NodeEditor.Controls.DrawingNode
=> style._addSetter(NodeEditor.Controls.DrawingNode.EnableSnapProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> EnableSnap<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.DrawingNode
=> style._addSetter(NodeEditor.Controls.DrawingNode.EnableSnapProperty, binding);


 // SnapXProperty

/*ValueStyleSetterGenerator*/
public static Style<T> SnapX<T>(this Style<T> style, System.Double value) where T : NodeEditor.Controls.DrawingNode
=> style._addSetter(NodeEditor.Controls.DrawingNode.SnapXProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SnapX<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.DrawingNode
=> style._addSetter(NodeEditor.Controls.DrawingNode.SnapXProperty, binding);


 // SnapYProperty

/*ValueStyleSetterGenerator*/
public static Style<T> SnapY<T>(this Style<T> style, System.Double value) where T : NodeEditor.Controls.DrawingNode
=> style._addSetter(NodeEditor.Controls.DrawingNode.SnapYProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SnapY<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.DrawingNode
=> style._addSetter(NodeEditor.Controls.DrawingNode.SnapYProperty, binding);


 // EnableGridProperty

/*ValueStyleSetterGenerator*/
public static Style<T> EnableGrid<T>(this Style<T> style, System.Boolean value) where T : NodeEditor.Controls.DrawingNode
=> style._addSetter(NodeEditor.Controls.DrawingNode.EnableGridProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> EnableGrid<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.DrawingNode
=> style._addSetter(NodeEditor.Controls.DrawingNode.EnableGridProperty, binding);


 // GridCellWidthProperty

/*ValueStyleSetterGenerator*/
public static Style<T> GridCellWidth<T>(this Style<T> style, System.Double value) where T : NodeEditor.Controls.DrawingNode
=> style._addSetter(NodeEditor.Controls.DrawingNode.GridCellWidthProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> GridCellWidth<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.DrawingNode
=> style._addSetter(NodeEditor.Controls.DrawingNode.GridCellWidthProperty, binding);


 // GridCellHeightProperty

/*ValueStyleSetterGenerator*/
public static Style<T> GridCellHeight<T>(this Style<T> style, System.Double value) where T : NodeEditor.Controls.DrawingNode
=> style._addSetter(NodeEditor.Controls.DrawingNode.GridCellHeightProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> GridCellHeight<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.DrawingNode
=> style._addSetter(NodeEditor.Controls.DrawingNode.GridCellHeightProperty, binding);



}
