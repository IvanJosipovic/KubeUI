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
public static partial class DrawingDropHandler_MarkupExtensions
{
//================= Properties ======================//
 // RelativeTo

/*BindFromExpressionSetterGenerator*/
public static T RelativeTo<T>(this T control, Func<Avalonia.Controls.Control> func, Action<Avalonia.Controls.Control>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Behaviors.DrawingDropHandler 
   => control._set(NodeEditor.Behaviors.DrawingDropHandler.RelativeToProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T RelativeTo<T>(this T control,Avalonia.Controls.Control value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Behaviors.DrawingDropHandler 
=> control._setEx(NodeEditor.Behaviors.DrawingDropHandler.RelativeToProperty, ps, () => control.RelativeTo = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T RelativeTo<T>(this T control, IBinding binding) where T : NodeEditor.Behaviors.DrawingDropHandler 
   => control._set(NodeEditor.Behaviors.DrawingDropHandler.RelativeToProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T RelativeTo<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Behaviors.DrawingDropHandler 
   => control._set(NodeEditor.Behaviors.DrawingDropHandler.RelativeToProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T RelativeTo<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Control> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Behaviors.DrawingDropHandler 
=> control._setEx(NodeEditor.Behaviors.DrawingDropHandler.RelativeToProperty, ps, () => control.RelativeTo = converter.TryConvert(value), bindingMode, converter, bindingSource);



}
