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
public static partial class DrawingNodeProperties_MarkupExtensions
{
//================= Properties ======================//
 // EnableSnap

/*BindFromExpressionSetterGenerator*/
public static T EnableSnap<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.DrawingNodeProperties 
   => control._set(NodeEditor.Controls.DrawingNodeProperties.EnableSnapProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T EnableSnap<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.DrawingNodeProperties 
=> control._setEx(NodeEditor.Controls.DrawingNodeProperties.EnableSnapProperty, ps, () => control.EnableSnap = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T EnableSnap<T>(this T control, IBinding binding) where T : NodeEditor.Controls.DrawingNodeProperties 
   => control._set(NodeEditor.Controls.DrawingNodeProperties.EnableSnapProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T EnableSnap<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.DrawingNodeProperties 
   => control._set(NodeEditor.Controls.DrawingNodeProperties.EnableSnapProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T EnableSnap<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.DrawingNodeProperties 
=> control._setEx(NodeEditor.Controls.DrawingNodeProperties.EnableSnapProperty, ps, () => control.EnableSnap = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SnapX

/*BindFromExpressionSetterGenerator*/
public static T SnapX<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.DrawingNodeProperties 
   => control._set(NodeEditor.Controls.DrawingNodeProperties.SnapXProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SnapX<T>(this T control,System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.DrawingNodeProperties 
=> control._setEx(NodeEditor.Controls.DrawingNodeProperties.SnapXProperty, ps, () => control.SnapX = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SnapX<T>(this T control, IBinding binding) where T : NodeEditor.Controls.DrawingNodeProperties 
   => control._set(NodeEditor.Controls.DrawingNodeProperties.SnapXProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SnapX<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.DrawingNodeProperties 
   => control._set(NodeEditor.Controls.DrawingNodeProperties.SnapXProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SnapX<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.DrawingNodeProperties 
=> control._setEx(NodeEditor.Controls.DrawingNodeProperties.SnapXProperty, ps, () => control.SnapX = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SnapY

/*BindFromExpressionSetterGenerator*/
public static T SnapY<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.DrawingNodeProperties 
   => control._set(NodeEditor.Controls.DrawingNodeProperties.SnapYProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SnapY<T>(this T control,System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.DrawingNodeProperties 
=> control._setEx(NodeEditor.Controls.DrawingNodeProperties.SnapYProperty, ps, () => control.SnapY = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SnapY<T>(this T control, IBinding binding) where T : NodeEditor.Controls.DrawingNodeProperties 
   => control._set(NodeEditor.Controls.DrawingNodeProperties.SnapYProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SnapY<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.DrawingNodeProperties 
   => control._set(NodeEditor.Controls.DrawingNodeProperties.SnapYProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SnapY<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.DrawingNodeProperties 
=> control._setEx(NodeEditor.Controls.DrawingNodeProperties.SnapYProperty, ps, () => control.SnapY = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // EnableGrid

/*BindFromExpressionSetterGenerator*/
public static T EnableGrid<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.DrawingNodeProperties 
   => control._set(NodeEditor.Controls.DrawingNodeProperties.EnableGridProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T EnableGrid<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.DrawingNodeProperties 
=> control._setEx(NodeEditor.Controls.DrawingNodeProperties.EnableGridProperty, ps, () => control.EnableGrid = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T EnableGrid<T>(this T control, IBinding binding) where T : NodeEditor.Controls.DrawingNodeProperties 
   => control._set(NodeEditor.Controls.DrawingNodeProperties.EnableGridProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T EnableGrid<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.DrawingNodeProperties 
   => control._set(NodeEditor.Controls.DrawingNodeProperties.EnableGridProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T EnableGrid<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.DrawingNodeProperties 
=> control._setEx(NodeEditor.Controls.DrawingNodeProperties.EnableGridProperty, ps, () => control.EnableGrid = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // GridCellWidth

/*BindFromExpressionSetterGenerator*/
public static T GridCellWidth<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.DrawingNodeProperties 
   => control._set(NodeEditor.Controls.DrawingNodeProperties.GridCellWidthProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T GridCellWidth<T>(this T control,System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.DrawingNodeProperties 
=> control._setEx(NodeEditor.Controls.DrawingNodeProperties.GridCellWidthProperty, ps, () => control.GridCellWidth = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T GridCellWidth<T>(this T control, IBinding binding) where T : NodeEditor.Controls.DrawingNodeProperties 
   => control._set(NodeEditor.Controls.DrawingNodeProperties.GridCellWidthProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T GridCellWidth<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.DrawingNodeProperties 
   => control._set(NodeEditor.Controls.DrawingNodeProperties.GridCellWidthProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T GridCellWidth<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.DrawingNodeProperties 
=> control._setEx(NodeEditor.Controls.DrawingNodeProperties.GridCellWidthProperty, ps, () => control.GridCellWidth = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // GridCellHeight

/*BindFromExpressionSetterGenerator*/
public static T GridCellHeight<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.DrawingNodeProperties 
   => control._set(NodeEditor.Controls.DrawingNodeProperties.GridCellHeightProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T GridCellHeight<T>(this T control,System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.DrawingNodeProperties 
=> control._setEx(NodeEditor.Controls.DrawingNodeProperties.GridCellHeightProperty, ps, () => control.GridCellHeight = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T GridCellHeight<T>(this T control, IBinding binding) where T : NodeEditor.Controls.DrawingNodeProperties 
   => control._set(NodeEditor.Controls.DrawingNodeProperties.GridCellHeightProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T GridCellHeight<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.DrawingNodeProperties 
   => control._set(NodeEditor.Controls.DrawingNodeProperties.GridCellHeightProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T GridCellHeight<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.DrawingNodeProperties 
=> control._setEx(NodeEditor.Controls.DrawingNodeProperties.GridCellHeightProperty, ps, () => control.GridCellHeight = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // DrawingWidth

/*BindFromExpressionSetterGenerator*/
public static T DrawingWidth<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.DrawingNodeProperties 
   => control._set(NodeEditor.Controls.DrawingNodeProperties.DrawingWidthProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T DrawingWidth<T>(this T control,System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.DrawingNodeProperties 
=> control._setEx(NodeEditor.Controls.DrawingNodeProperties.DrawingWidthProperty, ps, () => control.DrawingWidth = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T DrawingWidth<T>(this T control, IBinding binding) where T : NodeEditor.Controls.DrawingNodeProperties 
   => control._set(NodeEditor.Controls.DrawingNodeProperties.DrawingWidthProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T DrawingWidth<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.DrawingNodeProperties 
   => control._set(NodeEditor.Controls.DrawingNodeProperties.DrawingWidthProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T DrawingWidth<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.DrawingNodeProperties 
=> control._setEx(NodeEditor.Controls.DrawingNodeProperties.DrawingWidthProperty, ps, () => control.DrawingWidth = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // DrawingHeight

/*BindFromExpressionSetterGenerator*/
public static T DrawingHeight<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.DrawingNodeProperties 
   => control._set(NodeEditor.Controls.DrawingNodeProperties.DrawingHeightProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T DrawingHeight<T>(this T control,System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.DrawingNodeProperties 
=> control._setEx(NodeEditor.Controls.DrawingNodeProperties.DrawingHeightProperty, ps, () => control.DrawingHeight = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T DrawingHeight<T>(this T control, IBinding binding) where T : NodeEditor.Controls.DrawingNodeProperties 
   => control._set(NodeEditor.Controls.DrawingNodeProperties.DrawingHeightProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T DrawingHeight<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.DrawingNodeProperties 
   => control._set(NodeEditor.Controls.DrawingNodeProperties.DrawingHeightProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T DrawingHeight<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.DrawingNodeProperties 
=> control._setEx(NodeEditor.Controls.DrawingNodeProperties.DrawingHeightProperty, ps, () => control.DrawingHeight = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Styles ======================//
 // EnableSnap

/*ValueStyleSetterGenerator*/
public static Style<T> EnableSnap<T>(this Style<T> style, System.Boolean value) where T : NodeEditor.Controls.DrawingNodeProperties 
=> style._addSetter(NodeEditor.Controls.DrawingNodeProperties.EnableSnapProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> EnableSnap<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.DrawingNodeProperties 
=> style._addSetter(NodeEditor.Controls.DrawingNodeProperties.EnableSnapProperty, binding);


 // SnapX

/*ValueStyleSetterGenerator*/
public static Style<T> SnapX<T>(this Style<T> style, System.Double value) where T : NodeEditor.Controls.DrawingNodeProperties 
=> style._addSetter(NodeEditor.Controls.DrawingNodeProperties.SnapXProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SnapX<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.DrawingNodeProperties 
=> style._addSetter(NodeEditor.Controls.DrawingNodeProperties.SnapXProperty, binding);


 // SnapY

/*ValueStyleSetterGenerator*/
public static Style<T> SnapY<T>(this Style<T> style, System.Double value) where T : NodeEditor.Controls.DrawingNodeProperties 
=> style._addSetter(NodeEditor.Controls.DrawingNodeProperties.SnapYProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SnapY<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.DrawingNodeProperties 
=> style._addSetter(NodeEditor.Controls.DrawingNodeProperties.SnapYProperty, binding);


 // EnableGrid

/*ValueStyleSetterGenerator*/
public static Style<T> EnableGrid<T>(this Style<T> style, System.Boolean value) where T : NodeEditor.Controls.DrawingNodeProperties 
=> style._addSetter(NodeEditor.Controls.DrawingNodeProperties.EnableGridProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> EnableGrid<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.DrawingNodeProperties 
=> style._addSetter(NodeEditor.Controls.DrawingNodeProperties.EnableGridProperty, binding);


 // GridCellWidth

/*ValueStyleSetterGenerator*/
public static Style<T> GridCellWidth<T>(this Style<T> style, System.Double value) where T : NodeEditor.Controls.DrawingNodeProperties 
=> style._addSetter(NodeEditor.Controls.DrawingNodeProperties.GridCellWidthProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> GridCellWidth<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.DrawingNodeProperties 
=> style._addSetter(NodeEditor.Controls.DrawingNodeProperties.GridCellWidthProperty, binding);


 // GridCellHeight

/*ValueStyleSetterGenerator*/
public static Style<T> GridCellHeight<T>(this Style<T> style, System.Double value) where T : NodeEditor.Controls.DrawingNodeProperties 
=> style._addSetter(NodeEditor.Controls.DrawingNodeProperties.GridCellHeightProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> GridCellHeight<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.DrawingNodeProperties 
=> style._addSetter(NodeEditor.Controls.DrawingNodeProperties.GridCellHeightProperty, binding);


 // DrawingWidth

/*ValueStyleSetterGenerator*/
public static Style<T> DrawingWidth<T>(this Style<T> style, System.Double value) where T : NodeEditor.Controls.DrawingNodeProperties 
=> style._addSetter(NodeEditor.Controls.DrawingNodeProperties.DrawingWidthProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> DrawingWidth<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.DrawingNodeProperties 
=> style._addSetter(NodeEditor.Controls.DrawingNodeProperties.DrawingWidthProperty, binding);


 // DrawingHeight

/*ValueStyleSetterGenerator*/
public static Style<T> DrawingHeight<T>(this Style<T> style, System.Double value) where T : NodeEditor.Controls.DrawingNodeProperties 
=> style._addSetter(NodeEditor.Controls.DrawingNodeProperties.DrawingHeightProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> DrawingHeight<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.DrawingNodeProperties 
=> style._addSetter(NodeEditor.Controls.DrawingNodeProperties.DrawingHeightProperty, binding);



}
