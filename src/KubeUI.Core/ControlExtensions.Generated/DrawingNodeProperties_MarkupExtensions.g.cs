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
 // DrawingNode

/*BindFromExpressionSetterGenerator*/
public static T DrawingNode<T>(this T control, Func<NodeEditor.Controls.DrawingNode> func, Action<NodeEditor.Controls.DrawingNode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.DrawingNodeProperties 
   => control._set(NodeEditor.Controls.DrawingNodeProperties.DrawingNodeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T DrawingNode<T>(this T control,NodeEditor.Controls.DrawingNode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.DrawingNodeProperties 
=> control._setEx(NodeEditor.Controls.DrawingNodeProperties.DrawingNodeProperty, ps, () => control.DrawingNode = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T DrawingNode<T>(this T control, IBinding binding) where T : NodeEditor.Controls.DrawingNodeProperties 
   => control._set(NodeEditor.Controls.DrawingNodeProperties.DrawingNodeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T DrawingNode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.DrawingNodeProperties 
   => control._set(NodeEditor.Controls.DrawingNodeProperties.DrawingNodeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T DrawingNode<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, NodeEditor.Controls.DrawingNode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.DrawingNodeProperties 
=> control._setEx(NodeEditor.Controls.DrawingNodeProperties.DrawingNodeProperty, ps, () => control.DrawingNode = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Styles ======================//
 // DrawingNode

/*ValueStyleSetterGenerator*/
public static Style<T> DrawingNode<T>(this Style<T> style, NodeEditor.Controls.DrawingNode value) where T : NodeEditor.Controls.DrawingNodeProperties 
=> style._addSetter(NodeEditor.Controls.DrawingNodeProperties.DrawingNodeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> DrawingNode<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.DrawingNodeProperties 
=> style._addSetter(NodeEditor.Controls.DrawingNodeProperties.DrawingNodeProperty, binding);



}
