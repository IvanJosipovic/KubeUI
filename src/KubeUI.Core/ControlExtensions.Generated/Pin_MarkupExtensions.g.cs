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
public static partial class Pin_MarkupExtensions
{
//================= Properties ======================//
 // PinSource

/*BindFromExpressionSetterGenerator*/
public static T PinSource<T>(this T control, Func<NodeEditor.Model.IPin> func, Action<NodeEditor.Model.IPin>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.Pin 
   => control._set(NodeEditor.Controls.Pin.PinSourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T PinSource<T>(this T control,NodeEditor.Model.IPin value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.Pin 
=> control._setEx(NodeEditor.Controls.Pin.PinSourceProperty, ps, () => control.PinSource = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T PinSource<T>(this T control, IBinding binding) where T : NodeEditor.Controls.Pin 
   => control._set(NodeEditor.Controls.Pin.PinSourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T PinSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.Pin 
   => control._set(NodeEditor.Controls.Pin.PinSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T PinSource<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, NodeEditor.Model.IPin> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.Pin 
=> control._setEx(NodeEditor.Controls.Pin.PinSourceProperty, ps, () => control.PinSource = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // Alignment

/*BindFromExpressionSetterGenerator*/
public static T Alignment<T>(this T control, Func<NodeEditor.Model.PinAlignment> func, Action<NodeEditor.Model.PinAlignment>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.Pin 
   => control._set(NodeEditor.Controls.Pin.AlignmentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Alignment<T>(this T control,NodeEditor.Model.PinAlignment value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.Pin 
=> control._setEx(NodeEditor.Controls.Pin.AlignmentProperty, ps, () => control.Alignment = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Alignment<T>(this T control, IBinding binding) where T : NodeEditor.Controls.Pin 
   => control._set(NodeEditor.Controls.Pin.AlignmentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Alignment<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.Pin 
   => control._set(NodeEditor.Controls.Pin.AlignmentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Alignment<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, NodeEditor.Model.PinAlignment> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.Pin 
=> control._setEx(NodeEditor.Controls.Pin.AlignmentProperty, ps, () => control.Alignment = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // Id

/*BindFromExpressionSetterGenerator*/
public static T Id<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.Pin 
   => control._set(NodeEditor.Controls.Pin.IdProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Id<T>(this T control,System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.Pin 
=> control._setEx(NodeEditor.Controls.Pin.IdProperty, ps, () => control.Id = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Id<T>(this T control, IBinding binding) where T : NodeEditor.Controls.Pin 
   => control._set(NodeEditor.Controls.Pin.IdProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Id<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.Pin 
   => control._set(NodeEditor.Controls.Pin.IdProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Id<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.Pin 
=> control._setEx(NodeEditor.Controls.Pin.IdProperty, ps, () => control.Id = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Styles ======================//
 // PinSource

/*ValueStyleSetterGenerator*/
public static Style<T> PinSource<T>(this Style<T> style, NodeEditor.Model.IPin value) where T : NodeEditor.Controls.Pin 
=> style._addSetter(NodeEditor.Controls.Pin.PinSourceProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> PinSource<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.Pin 
=> style._addSetter(NodeEditor.Controls.Pin.PinSourceProperty, binding);


 // Alignment

/*ValueStyleSetterGenerator*/
public static Style<T> Alignment<T>(this Style<T> style, NodeEditor.Model.PinAlignment value) where T : NodeEditor.Controls.Pin 
=> style._addSetter(NodeEditor.Controls.Pin.AlignmentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Alignment<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.Pin 
=> style._addSetter(NodeEditor.Controls.Pin.AlignmentProperty, binding);


 // Id

/*ValueStyleSetterGenerator*/
public static Style<T> Id<T>(this Style<T> style, System.String value) where T : NodeEditor.Controls.Pin 
=> style._addSetter(NodeEditor.Controls.Pin.IdProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Id<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.Pin 
=> style._addSetter(NodeEditor.Controls.Pin.IdProperty, binding);



}
