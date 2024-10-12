#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
[global::System.CodeDom.Compiler.GeneratedCode("AvaloniaExtensionGenerator", "11.1.3.0")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static partial class DrawingSelectionBehavior_MarkupExtensions
{
//================= Properties ======================//
 // InputSourceProperty

/*BindFromExpressionSetterGenerator*/
public static T InputSource<T>(this T control, Func<Avalonia.Controls.Control> func, Action<Avalonia.Controls.Control>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Behaviors.DrawingSelectionBehavior
   => control._set(NodeEditor.Behaviors.DrawingSelectionBehavior.InputSourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T InputSource<T>(this T control, Avalonia.Controls.Control value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Behaviors.DrawingSelectionBehavior
=> control._setEx(NodeEditor.Behaviors.DrawingSelectionBehavior.InputSourceProperty, ps, () => control.InputSource = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T InputSource<T>(this T control, IBinding binding) where T : NodeEditor.Behaviors.DrawingSelectionBehavior
   => control._set(NodeEditor.Behaviors.DrawingSelectionBehavior.InputSourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T InputSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Behaviors.DrawingSelectionBehavior
   => control._set(NodeEditor.Behaviors.DrawingSelectionBehavior.InputSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T InputSource<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Control> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Behaviors.DrawingSelectionBehavior
=> control._setEx(NodeEditor.Behaviors.DrawingSelectionBehavior.InputSourceProperty, ps, () => control.InputSource = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // AdornerCanvasProperty

/*BindFromExpressionSetterGenerator*/
public static T AdornerCanvas<T>(this T control, Func<Avalonia.Controls.Canvas> func, Action<Avalonia.Controls.Canvas>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Behaviors.DrawingSelectionBehavior
   => control._set(NodeEditor.Behaviors.DrawingSelectionBehavior.AdornerCanvasProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T AdornerCanvas<T>(this T control, Avalonia.Controls.Canvas value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Behaviors.DrawingSelectionBehavior
=> control._setEx(NodeEditor.Behaviors.DrawingSelectionBehavior.AdornerCanvasProperty, ps, () => control.AdornerCanvas = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T AdornerCanvas<T>(this T control, IBinding binding) where T : NodeEditor.Behaviors.DrawingSelectionBehavior
   => control._set(NodeEditor.Behaviors.DrawingSelectionBehavior.AdornerCanvasProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T AdornerCanvas<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Behaviors.DrawingSelectionBehavior
   => control._set(NodeEditor.Behaviors.DrawingSelectionBehavior.AdornerCanvasProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T AdornerCanvas<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Canvas> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Behaviors.DrawingSelectionBehavior
=> control._setEx(NodeEditor.Behaviors.DrawingSelectionBehavior.AdornerCanvasProperty, ps, () => control.AdornerCanvas = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // EnableSnapProperty

/*BindFromExpressionSetterGenerator*/
public static T EnableSnap<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Behaviors.DrawingSelectionBehavior
   => control._set(NodeEditor.Behaviors.DrawingSelectionBehavior.EnableSnapProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T EnableSnap<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Behaviors.DrawingSelectionBehavior
=> control._setEx(NodeEditor.Behaviors.DrawingSelectionBehavior.EnableSnapProperty, ps, () => control.EnableSnap = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T EnableSnap<T>(this T control, IBinding binding) where T : NodeEditor.Behaviors.DrawingSelectionBehavior
   => control._set(NodeEditor.Behaviors.DrawingSelectionBehavior.EnableSnapProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T EnableSnap<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Behaviors.DrawingSelectionBehavior
   => control._set(NodeEditor.Behaviors.DrawingSelectionBehavior.EnableSnapProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T EnableSnap<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Behaviors.DrawingSelectionBehavior
=> control._setEx(NodeEditor.Behaviors.DrawingSelectionBehavior.EnableSnapProperty, ps, () => control.EnableSnap = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SnapXProperty

/*BindFromExpressionSetterGenerator*/
public static T SnapX<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Behaviors.DrawingSelectionBehavior
   => control._set(NodeEditor.Behaviors.DrawingSelectionBehavior.SnapXProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SnapX<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Behaviors.DrawingSelectionBehavior
=> control._setEx(NodeEditor.Behaviors.DrawingSelectionBehavior.SnapXProperty, ps, () => control.SnapX = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SnapX<T>(this T control, IBinding binding) where T : NodeEditor.Behaviors.DrawingSelectionBehavior
   => control._set(NodeEditor.Behaviors.DrawingSelectionBehavior.SnapXProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SnapX<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Behaviors.DrawingSelectionBehavior
   => control._set(NodeEditor.Behaviors.DrawingSelectionBehavior.SnapXProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SnapX<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Behaviors.DrawingSelectionBehavior
=> control._setEx(NodeEditor.Behaviors.DrawingSelectionBehavior.SnapXProperty, ps, () => control.SnapX = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SnapYProperty

/*BindFromExpressionSetterGenerator*/
public static T SnapY<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Behaviors.DrawingSelectionBehavior
   => control._set(NodeEditor.Behaviors.DrawingSelectionBehavior.SnapYProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SnapY<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Behaviors.DrawingSelectionBehavior
=> control._setEx(NodeEditor.Behaviors.DrawingSelectionBehavior.SnapYProperty, ps, () => control.SnapY = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SnapY<T>(this T control, IBinding binding) where T : NodeEditor.Behaviors.DrawingSelectionBehavior
   => control._set(NodeEditor.Behaviors.DrawingSelectionBehavior.SnapYProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SnapY<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Behaviors.DrawingSelectionBehavior
   => control._set(NodeEditor.Behaviors.DrawingSelectionBehavior.SnapYProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SnapY<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Behaviors.DrawingSelectionBehavior
=> control._setEx(NodeEditor.Behaviors.DrawingSelectionBehavior.SnapYProperty, ps, () => control.SnapY = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//

}
