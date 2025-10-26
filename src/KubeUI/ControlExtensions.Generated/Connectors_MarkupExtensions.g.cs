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
public static partial class Connectors_MarkupExtensions
{
//================= Properties ======================//
 // DrawingSource

/*BindFromExpressionSetterGenerator*/
public static T DrawingSource<T>(this T control, Func<NodeEditor.Model.IDrawingNode> func, Action<NodeEditor.Model.IDrawingNode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.Connectors 
   => control._set(NodeEditor.Controls.Connectors.DrawingSourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T DrawingSource<T>(this T control,NodeEditor.Model.IDrawingNode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.Connectors 
=> control._setEx(NodeEditor.Controls.Connectors.DrawingSourceProperty, ps, () => control.DrawingSource = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T DrawingSource<T>(this T control, IBinding binding) where T : NodeEditor.Controls.Connectors 
   => control._set(NodeEditor.Controls.Connectors.DrawingSourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T DrawingSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.Connectors 
   => control._set(NodeEditor.Controls.Connectors.DrawingSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T DrawingSource<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, NodeEditor.Model.IDrawingNode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.Connectors 
=> control._setEx(NodeEditor.Controls.Connectors.DrawingSourceProperty, ps, () => control.DrawingSource = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Styles ======================//
 // DrawingSource

/*ValueStyleSetterGenerator*/
public static Style<T> DrawingSource<T>(this Style<T> style, NodeEditor.Model.IDrawingNode value) where T : NodeEditor.Controls.Connectors 
=> style._addSetter(NodeEditor.Controls.Connectors.DrawingSourceProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> DrawingSource<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.Connectors 
=> style._addSetter(NodeEditor.Controls.Connectors.DrawingSourceProperty, binding);



}
