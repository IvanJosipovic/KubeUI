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
public static partial class Connector_MarkupExtensions
{
//================= Properties ======================//
 // ConnectorSource

/*BindFromExpressionSetterGenerator*/
public static T ConnectorSource<T>(this T control, Func<NodeEditor.Model.IConnector> func, Action<NodeEditor.Model.IConnector>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.Connector 
   => control._set(NodeEditor.Controls.Connector.ConnectorSourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ConnectorSource<T>(this T control,NodeEditor.Model.IConnector value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.Connector 
=> control._setEx(NodeEditor.Controls.Connector.ConnectorSourceProperty, ps, () => control.ConnectorSource = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ConnectorSource<T>(this T control, IBinding binding) where T : NodeEditor.Controls.Connector 
   => control._set(NodeEditor.Controls.Connector.ConnectorSourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ConnectorSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.Connector 
   => control._set(NodeEditor.Controls.Connector.ConnectorSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ConnectorSource<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, NodeEditor.Model.IConnector> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.Connector 
=> control._setEx(NodeEditor.Controls.Connector.ConnectorSourceProperty, ps, () => control.ConnectorSource = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // StartPoint

/*BindFromExpressionSetterGenerator*/
public static T StartPoint<T>(this T control, Func<Avalonia.Point> func, Action<Avalonia.Point>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.Connector 
   => control._set(NodeEditor.Controls.Connector.StartPointProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T StartPoint<T>(this T control,Avalonia.Point value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.Connector 
=> control._setEx(NodeEditor.Controls.Connector.StartPointProperty, ps, () => control.StartPoint = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T StartPoint<T>(this T control, IBinding binding) where T : NodeEditor.Controls.Connector 
   => control._set(NodeEditor.Controls.Connector.StartPointProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T StartPoint<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.Connector 
   => control._set(NodeEditor.Controls.Connector.StartPointProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T StartPoint<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Point> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.Connector 
=> control._setEx(NodeEditor.Controls.Connector.StartPointProperty, ps, () => control.StartPoint = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // EndPoint

/*BindFromExpressionSetterGenerator*/
public static T EndPoint<T>(this T control, Func<Avalonia.Point> func, Action<Avalonia.Point>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.Connector 
   => control._set(NodeEditor.Controls.Connector.EndPointProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T EndPoint<T>(this T control,Avalonia.Point value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.Connector 
=> control._setEx(NodeEditor.Controls.Connector.EndPointProperty, ps, () => control.EndPoint = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T EndPoint<T>(this T control, IBinding binding) where T : NodeEditor.Controls.Connector 
   => control._set(NodeEditor.Controls.Connector.EndPointProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T EndPoint<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.Connector 
   => control._set(NodeEditor.Controls.Connector.EndPointProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T EndPoint<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Point> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.Connector 
=> control._setEx(NodeEditor.Controls.Connector.EndPointProperty, ps, () => control.EndPoint = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // Offset

/*BindFromExpressionSetterGenerator*/
public static T Offset<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.Connector 
   => control._set(NodeEditor.Controls.Connector.OffsetProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Offset<T>(this T control,System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.Connector 
=> control._setEx(NodeEditor.Controls.Connector.OffsetProperty, ps, () => control.Offset = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Offset<T>(this T control, IBinding binding) where T : NodeEditor.Controls.Connector 
   => control._set(NodeEditor.Controls.Connector.OffsetProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Offset<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.Connector 
   => control._set(NodeEditor.Controls.Connector.OffsetProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Offset<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.Connector 
=> control._setEx(NodeEditor.Controls.Connector.OffsetProperty, ps, () => control.Offset = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // Orientation

/*BindFromExpressionSetterGenerator*/
public static T Orientation<T>(this T control, Func<NodeEditor.Model.ConnectorOrientation> func, Action<NodeEditor.Model.ConnectorOrientation>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.Connector 
   => control._set(NodeEditor.Controls.Connector.OrientationProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Orientation<T>(this T control,NodeEditor.Model.ConnectorOrientation value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.Connector 
=> control._setEx(NodeEditor.Controls.Connector.OrientationProperty, ps, () => control.Orientation = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Orientation<T>(this T control, IBinding binding) where T : NodeEditor.Controls.Connector 
   => control._set(NodeEditor.Controls.Connector.OrientationProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Orientation<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.Connector 
   => control._set(NodeEditor.Controls.Connector.OrientationProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Orientation<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, NodeEditor.Model.ConnectorOrientation> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.Connector 
=> control._setEx(NodeEditor.Controls.Connector.OrientationProperty, ps, () => control.Orientation = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Styles ======================//
 // ConnectorSource

/*ValueStyleSetterGenerator*/
public static Style<T> ConnectorSource<T>(this Style<T> style, NodeEditor.Model.IConnector value) where T : NodeEditor.Controls.Connector 
=> style._addSetter(NodeEditor.Controls.Connector.ConnectorSourceProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ConnectorSource<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.Connector 
=> style._addSetter(NodeEditor.Controls.Connector.ConnectorSourceProperty, binding);


 // StartPoint

/*ValueStyleSetterGenerator*/
public static Style<T> StartPoint<T>(this Style<T> style, Avalonia.Point value) where T : NodeEditor.Controls.Connector 
=> style._addSetter(NodeEditor.Controls.Connector.StartPointProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> StartPoint<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.Connector 
=> style._addSetter(NodeEditor.Controls.Connector.StartPointProperty, binding);


 // EndPoint

/*ValueStyleSetterGenerator*/
public static Style<T> EndPoint<T>(this Style<T> style, Avalonia.Point value) where T : NodeEditor.Controls.Connector 
=> style._addSetter(NodeEditor.Controls.Connector.EndPointProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> EndPoint<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.Connector 
=> style._addSetter(NodeEditor.Controls.Connector.EndPointProperty, binding);


 // Offset

/*ValueStyleSetterGenerator*/
public static Style<T> Offset<T>(this Style<T> style, System.Double value) where T : NodeEditor.Controls.Connector 
=> style._addSetter(NodeEditor.Controls.Connector.OffsetProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Offset<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.Connector 
=> style._addSetter(NodeEditor.Controls.Connector.OffsetProperty, binding);


 // Orientation

/*ValueStyleSetterGenerator*/
public static Style<T> Orientation<T>(this Style<T> style, NodeEditor.Model.ConnectorOrientation value) where T : NodeEditor.Controls.Connector 
=> style._addSetter(NodeEditor.Controls.Connector.OrientationProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Orientation<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.Connector 
=> style._addSetter(NodeEditor.Controls.Connector.OrientationProperty, binding);



}
