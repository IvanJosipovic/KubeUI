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
public static partial class PinPressedBehavior_MarkupExtensions
{
//================= Properties ======================//
 // PinSource

/*BindFromExpressionSetterGenerator*/
public static T PinSource<T>(this T control, Func<NodeEditor.Model.IPin> func, Action<NodeEditor.Model.IPin>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Behaviors.PinPressedBehavior 
   => control._set(NodeEditor.Behaviors.PinPressedBehavior.PinSourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T PinSource<T>(this T control,NodeEditor.Model.IPin value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Behaviors.PinPressedBehavior 
=> control._setEx(NodeEditor.Behaviors.PinPressedBehavior.PinSourceProperty, ps, () => control.PinSource = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T PinSource<T>(this T control, IBinding binding) where T : NodeEditor.Behaviors.PinPressedBehavior 
   => control._set(NodeEditor.Behaviors.PinPressedBehavior.PinSourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T PinSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Behaviors.PinPressedBehavior 
   => control._set(NodeEditor.Behaviors.PinPressedBehavior.PinSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T PinSource<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, NodeEditor.Model.IPin> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Behaviors.PinPressedBehavior 
=> control._setEx(NodeEditor.Behaviors.PinPressedBehavior.PinSourceProperty, ps, () => control.PinSource = converter.TryConvert(value), bindingMode, converter, bindingSource);



}
