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
public static partial class DrawingSelectionBehavior_MarkupExtensions
{
//================= Properties ======================//
 // DrawingSource

/*BindFromExpressionSetterGenerator*/
public static T DrawingSource<T>(this T control, Func<NodeEditor.Model.IDrawingNode> func, Action<NodeEditor.Model.IDrawingNode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Behaviors.DrawingSelectionBehavior 
   => control._set(NodeEditor.Behaviors.DrawingSelectionBehavior.DrawingSourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T DrawingSource<T>(this T control,NodeEditor.Model.IDrawingNode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Behaviors.DrawingSelectionBehavior 
=> control._setEx(NodeEditor.Behaviors.DrawingSelectionBehavior.DrawingSourceProperty, ps, () => control.DrawingSource = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T DrawingSource<T>(this T control, IBinding binding) where T : NodeEditor.Behaviors.DrawingSelectionBehavior 
   => control._set(NodeEditor.Behaviors.DrawingSelectionBehavior.DrawingSourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T DrawingSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Behaviors.DrawingSelectionBehavior 
   => control._set(NodeEditor.Behaviors.DrawingSelectionBehavior.DrawingSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T DrawingSource<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, NodeEditor.Model.IDrawingNode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Behaviors.DrawingSelectionBehavior 
=> control._setEx(NodeEditor.Behaviors.DrawingSelectionBehavior.DrawingSourceProperty, ps, () => control.DrawingSource = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // InputSource

/*BindFromExpressionSetterGenerator*/
public static T InputSource<T>(this T control, Func<Avalonia.Controls.Control> func, Action<Avalonia.Controls.Control>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Behaviors.DrawingSelectionBehavior 
   => control._set(NodeEditor.Behaviors.DrawingSelectionBehavior.InputSourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T InputSource<T>(this T control,Avalonia.Controls.Control value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Behaviors.DrawingSelectionBehavior 
=> control._setEx(NodeEditor.Behaviors.DrawingSelectionBehavior.InputSourceProperty, ps, () => control.InputSource = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T InputSource<T>(this T control, IBinding binding) where T : NodeEditor.Behaviors.DrawingSelectionBehavior 
   => control._set(NodeEditor.Behaviors.DrawingSelectionBehavior.InputSourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T InputSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Behaviors.DrawingSelectionBehavior 
   => control._set(NodeEditor.Behaviors.DrawingSelectionBehavior.InputSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T InputSource<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Control> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Behaviors.DrawingSelectionBehavior 
=> control._setEx(NodeEditor.Behaviors.DrawingSelectionBehavior.InputSourceProperty, ps, () => control.InputSource = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // AdornerCanvas

/*BindFromExpressionSetterGenerator*/
public static T AdornerCanvas<T>(this T control, Func<Avalonia.Controls.Canvas> func, Action<Avalonia.Controls.Canvas>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Behaviors.DrawingSelectionBehavior 
   => control._set(NodeEditor.Behaviors.DrawingSelectionBehavior.AdornerCanvasProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T AdornerCanvas<T>(this T control,Avalonia.Controls.Canvas value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Behaviors.DrawingSelectionBehavior 
=> control._setEx(NodeEditor.Behaviors.DrawingSelectionBehavior.AdornerCanvasProperty, ps, () => control.AdornerCanvas = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T AdornerCanvas<T>(this T control, IBinding binding) where T : NodeEditor.Behaviors.DrawingSelectionBehavior 
   => control._set(NodeEditor.Behaviors.DrawingSelectionBehavior.AdornerCanvasProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T AdornerCanvas<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Behaviors.DrawingSelectionBehavior 
   => control._set(NodeEditor.Behaviors.DrawingSelectionBehavior.AdornerCanvasProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T AdornerCanvas<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Canvas> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Behaviors.DrawingSelectionBehavior 
=> control._setEx(NodeEditor.Behaviors.DrawingSelectionBehavior.AdornerCanvasProperty, ps, () => control.AdornerCanvas = converter.TryConvert(value), bindingMode, converter, bindingSource);



}
