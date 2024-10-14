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
public static partial class Editor_MarkupExtensions
{
//================= Properties ======================//
 // DrawingSource

/*BindFromExpressionSetterGenerator*/
public static T DrawingSource<T>(this T control, Func<NodeEditor.Model.IDrawingNode> func, Action<NodeEditor.Model.IDrawingNode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.Editor 
   => control._set(NodeEditor.Controls.Editor.DrawingSourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T DrawingSource<T>(this T control,NodeEditor.Model.IDrawingNode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.Editor 
=> control._setEx(NodeEditor.Controls.Editor.DrawingSourceProperty, ps, () => control.DrawingSource = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T DrawingSource<T>(this T control, IBinding binding) where T : NodeEditor.Controls.Editor 
   => control._set(NodeEditor.Controls.Editor.DrawingSourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T DrawingSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.Editor 
   => control._set(NodeEditor.Controls.Editor.DrawingSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T DrawingSource<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, NodeEditor.Model.IDrawingNode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.Editor 
=> control._setEx(NodeEditor.Controls.Editor.DrawingSourceProperty, ps, () => control.DrawingSource = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // InputSource

/*BindFromExpressionSetterGenerator*/
public static T InputSource<T>(this T control, Func<Avalonia.Controls.Control> func, Action<Avalonia.Controls.Control>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.Editor 
   => control._set(NodeEditor.Controls.Editor.InputSourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T InputSource<T>(this T control,Avalonia.Controls.Control value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.Editor 
=> control._setEx(NodeEditor.Controls.Editor.InputSourceProperty, ps, () => control.InputSource = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T InputSource<T>(this T control, IBinding binding) where T : NodeEditor.Controls.Editor 
   => control._set(NodeEditor.Controls.Editor.InputSourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T InputSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.Editor 
   => control._set(NodeEditor.Controls.Editor.InputSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T InputSource<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Control> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.Editor 
=> control._setEx(NodeEditor.Controls.Editor.InputSourceProperty, ps, () => control.InputSource = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // AdornerCanvas

/*BindFromExpressionSetterGenerator*/
public static T AdornerCanvas<T>(this T control, Func<Avalonia.Controls.Canvas> func, Action<Avalonia.Controls.Canvas>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.Editor 
   => control._set(NodeEditor.Controls.Editor.AdornerCanvasProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T AdornerCanvas<T>(this T control,Avalonia.Controls.Canvas value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.Editor 
=> control._setEx(NodeEditor.Controls.Editor.AdornerCanvasProperty, ps, () => control.AdornerCanvas = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T AdornerCanvas<T>(this T control, IBinding binding) where T : NodeEditor.Controls.Editor 
   => control._set(NodeEditor.Controls.Editor.AdornerCanvasProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T AdornerCanvas<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.Editor 
   => control._set(NodeEditor.Controls.Editor.AdornerCanvasProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T AdornerCanvas<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Canvas> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.Editor 
=> control._setEx(NodeEditor.Controls.Editor.AdornerCanvasProperty, ps, () => control.AdornerCanvas = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ZoomControl

/*BindFromExpressionSetterGenerator*/
public static T ZoomControl<T>(this T control, Func<NodeEditor.Controls.NodeZoomBorder> func, Action<NodeEditor.Controls.NodeZoomBorder>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.Editor 
   => control._set(NodeEditor.Controls.Editor.ZoomControlProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ZoomControl<T>(this T control,NodeEditor.Controls.NodeZoomBorder value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.Editor 
=> control._setEx(NodeEditor.Controls.Editor.ZoomControlProperty, ps, () => control.ZoomControl = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ZoomControl<T>(this T control, IBinding binding) where T : NodeEditor.Controls.Editor 
   => control._set(NodeEditor.Controls.Editor.ZoomControlProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ZoomControl<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.Editor 
   => control._set(NodeEditor.Controls.Editor.ZoomControlProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ZoomControl<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, NodeEditor.Controls.NodeZoomBorder> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.Editor 
=> control._setEx(NodeEditor.Controls.Editor.ZoomControlProperty, ps, () => control.ZoomControl = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Styles ======================//
 // DrawingSource

/*ValueStyleSetterGenerator*/
public static Style<T> DrawingSource<T>(this Style<T> style, NodeEditor.Model.IDrawingNode value) where T : NodeEditor.Controls.Editor 
=> style._addSetter(NodeEditor.Controls.Editor.DrawingSourceProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> DrawingSource<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.Editor 
=> style._addSetter(NodeEditor.Controls.Editor.DrawingSourceProperty, binding);


 // InputSource

/*ValueStyleSetterGenerator*/
public static Style<T> InputSource<T>(this Style<T> style, Avalonia.Controls.Control value) where T : NodeEditor.Controls.Editor 
=> style._addSetter(NodeEditor.Controls.Editor.InputSourceProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> InputSource<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.Editor 
=> style._addSetter(NodeEditor.Controls.Editor.InputSourceProperty, binding);


 // AdornerCanvas

/*ValueStyleSetterGenerator*/
public static Style<T> AdornerCanvas<T>(this Style<T> style, Avalonia.Controls.Canvas value) where T : NodeEditor.Controls.Editor 
=> style._addSetter(NodeEditor.Controls.Editor.AdornerCanvasProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> AdornerCanvas<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.Editor 
=> style._addSetter(NodeEditor.Controls.Editor.AdornerCanvasProperty, binding);


 // ZoomControl

/*ValueStyleSetterGenerator*/
public static Style<T> ZoomControl<T>(this Style<T> style, NodeEditor.Controls.NodeZoomBorder value) where T : NodeEditor.Controls.Editor 
=> style._addSetter(NodeEditor.Controls.Editor.ZoomControlProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ZoomControl<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.Editor 
=> style._addSetter(NodeEditor.Controls.Editor.ZoomControlProperty, binding);



}
