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


 // DrawingNode

/*BindFromExpressionSetterGenerator*/
public static T DrawingNode<T>(this T control, Func<NodeEditor.Controls.DrawingNode> func, Action<NodeEditor.Controls.DrawingNode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.Editor 
   => control._set(NodeEditor.Controls.Editor.DrawingNodeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T DrawingNode<T>(this T control,NodeEditor.Controls.DrawingNode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.Editor 
=> control._setEx(NodeEditor.Controls.Editor.DrawingNodeProperty, ps, () => control.DrawingNode = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T DrawingNode<T>(this T control, IBinding binding) where T : NodeEditor.Controls.Editor 
   => control._set(NodeEditor.Controls.Editor.DrawingNodeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T DrawingNode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.Editor 
   => control._set(NodeEditor.Controls.Editor.DrawingNodeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T DrawingNode<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, NodeEditor.Controls.DrawingNode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.Editor 
=> control._setEx(NodeEditor.Controls.Editor.DrawingNodeProperty, ps, () => control.DrawingNode = converter.TryConvert(value), bindingMode, converter, bindingSource);


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



//================= Styles ======================//
 // ZoomControl

/*ValueStyleSetterGenerator*/
public static Style<T> ZoomControl<T>(this Style<T> style, NodeEditor.Controls.NodeZoomBorder value) where T : NodeEditor.Controls.Editor 
=> style._addSetter(NodeEditor.Controls.Editor.ZoomControlProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ZoomControl<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.Editor 
=> style._addSetter(NodeEditor.Controls.Editor.ZoomControlProperty, binding);


 // DrawingNode

/*ValueStyleSetterGenerator*/
public static Style<T> DrawingNode<T>(this Style<T> style, NodeEditor.Controls.DrawingNode value) where T : NodeEditor.Controls.Editor 
=> style._addSetter(NodeEditor.Controls.Editor.DrawingNodeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> DrawingNode<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.Editor 
=> style._addSetter(NodeEditor.Controls.Editor.DrawingNodeProperty, binding);


 // AdornerCanvas

/*ValueStyleSetterGenerator*/
public static Style<T> AdornerCanvas<T>(this Style<T> style, Avalonia.Controls.Canvas value) where T : NodeEditor.Controls.Editor 
=> style._addSetter(NodeEditor.Controls.Editor.AdornerCanvasProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> AdornerCanvas<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.Editor 
=> style._addSetter(NodeEditor.Controls.Editor.AdornerCanvasProperty, binding);



}
