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
public static partial class Pins_MarkupExtensions
{
//================= Properties ======================//
 // NodeSource

/*BindFromExpressionSetterGenerator*/
public static T NodeSource<T>(this T control, Func<NodeEditor.Model.INode> func, Action<NodeEditor.Model.INode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.Pins 
   => control._set(NodeEditor.Controls.Pins.NodeSourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T NodeSource<T>(this T control,NodeEditor.Model.INode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.Pins 
=> control._setEx(NodeEditor.Controls.Pins.NodeSourceProperty, ps, () => control.NodeSource = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T NodeSource<T>(this T control, IBinding binding) where T : NodeEditor.Controls.Pins 
   => control._set(NodeEditor.Controls.Pins.NodeSourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T NodeSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.Pins 
   => control._set(NodeEditor.Controls.Pins.NodeSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T NodeSource<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, NodeEditor.Model.INode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.Pins 
=> control._setEx(NodeEditor.Controls.Pins.NodeSourceProperty, ps, () => control.NodeSource = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Styles ======================//
 // NodeSource

/*ValueStyleSetterGenerator*/
public static Style<T> NodeSource<T>(this Style<T> style, NodeEditor.Model.INode value) where T : NodeEditor.Controls.Pins 
=> style._addSetter(NodeEditor.Controls.Pins.NodeSourceProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> NodeSource<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.Pins 
=> style._addSetter(NodeEditor.Controls.Pins.NodeSourceProperty, binding);



}
