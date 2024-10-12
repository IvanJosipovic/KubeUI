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
public static partial class Connector_MarkupExtensions
{
//================= Properties ======================//
 // StartPointProperty

/*BindFromExpressionSetterGenerator*/
public static T StartPoint<T>(this T control, Func<Avalonia.Point> func, Action<Avalonia.Point>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.Connector
   => control._set(NodeEditor.Controls.Connector.StartPointProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T StartPoint<T>(this T control, Avalonia.Point value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.Connector
=> control._setEx(NodeEditor.Controls.Connector.StartPointProperty, ps, () => control.StartPoint = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T StartPoint<T>(this T control, IBinding binding) where T : NodeEditor.Controls.Connector
   => control._set(NodeEditor.Controls.Connector.StartPointProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T StartPoint<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.Connector
   => control._set(NodeEditor.Controls.Connector.StartPointProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T StartPoint<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Point> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.Connector
=> control._setEx(NodeEditor.Controls.Connector.StartPointProperty, ps, () => control.StartPoint = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // EndPointProperty

/*BindFromExpressionSetterGenerator*/
public static T EndPoint<T>(this T control, Func<Avalonia.Point> func, Action<Avalonia.Point>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.Connector
   => control._set(NodeEditor.Controls.Connector.EndPointProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T EndPoint<T>(this T control, Avalonia.Point value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.Connector
=> control._setEx(NodeEditor.Controls.Connector.EndPointProperty, ps, () => control.EndPoint = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T EndPoint<T>(this T control, IBinding binding) where T : NodeEditor.Controls.Connector
   => control._set(NodeEditor.Controls.Connector.EndPointProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T EndPoint<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.Connector
   => control._set(NodeEditor.Controls.Connector.EndPointProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T EndPoint<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Point> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.Connector
=> control._setEx(NodeEditor.Controls.Connector.EndPointProperty, ps, () => control.EndPoint = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // OffsetProperty

/*BindFromExpressionSetterGenerator*/
public static T Offset<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.Connector
   => control._set(NodeEditor.Controls.Connector.OffsetProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Offset<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.Connector
=> control._setEx(NodeEditor.Controls.Connector.OffsetProperty, ps, () => control.Offset = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Offset<T>(this T control, IBinding binding) where T : NodeEditor.Controls.Connector
   => control._set(NodeEditor.Controls.Connector.OffsetProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Offset<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.Connector
   => control._set(NodeEditor.Controls.Connector.OffsetProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Offset<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.Connector
=> control._setEx(NodeEditor.Controls.Connector.OffsetProperty, ps, () => control.Offset = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // StartPointProperty

/*ValueStyleSetterGenerator*/
public static Style<T> StartPoint<T>(this Style<T> style, Avalonia.Point value) where T : NodeEditor.Controls.Connector
=> style._addSetter(NodeEditor.Controls.Connector.StartPointProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> StartPoint<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.Connector
=> style._addSetter(NodeEditor.Controls.Connector.StartPointProperty, binding);


 // EndPointProperty

/*ValueStyleSetterGenerator*/
public static Style<T> EndPoint<T>(this Style<T> style, Avalonia.Point value) where T : NodeEditor.Controls.Connector
=> style._addSetter(NodeEditor.Controls.Connector.EndPointProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> EndPoint<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.Connector
=> style._addSetter(NodeEditor.Controls.Connector.EndPointProperty, binding);


 // OffsetProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Offset<T>(this Style<T> style, System.Double value) where T : NodeEditor.Controls.Connector
=> style._addSetter(NodeEditor.Controls.Connector.OffsetProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Offset<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.Connector
=> style._addSetter(NodeEditor.Controls.Connector.OffsetProperty, binding);



}
