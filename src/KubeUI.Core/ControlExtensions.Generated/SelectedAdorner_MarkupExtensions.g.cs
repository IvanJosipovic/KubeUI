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
public static partial class SelectedAdorner_MarkupExtensions
{
//================= Properties ======================//
 // RectProperty

/*BindFromExpressionSetterGenerator*/
public static T Rect<T>(this T control, Func<Avalonia.Rect> func, Action<Avalonia.Rect>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.SelectedAdorner
   => control._set(NodeEditor.Controls.SelectedAdorner.RectProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Rect<T>(this T control, Avalonia.Rect value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.SelectedAdorner
=> control._setEx(NodeEditor.Controls.SelectedAdorner.RectProperty, ps, () => control.Rect = value, bindingMode, converter, bindingSource);

/*ValueOverloadsSetterGenerator*/

public static T Rect<T>(this T control, System.Double x = default, System.Double y = default, System.Double width = default, System.Double height = default) where T : NodeEditor.Controls.SelectedAdorner
   => control._set(() => control.Rect = new Avalonia.Rect(x, y, width, height));
public static T Rect<T>(this T control, Avalonia.Size size = default) where T : NodeEditor.Controls.SelectedAdorner
   => control._set(() => control.Rect = new Avalonia.Rect(size));
public static T Rect<T>(this T control, Avalonia.Point position = default, Avalonia.Size size = default) where T : NodeEditor.Controls.SelectedAdorner
   => control._set(() => control.Rect = new Avalonia.Rect(position, size));
public static T Rect<T>(this T control, Avalonia.Point topLeft = default, Avalonia.Point bottomRight = default) where T : NodeEditor.Controls.SelectedAdorner
   => control._set(() => control.Rect = new Avalonia.Rect(topLeft, bottomRight));

/*BindSetterGenerator*/
public static T Rect<T>(this T control, IBinding binding) where T : NodeEditor.Controls.SelectedAdorner
   => control._set(NodeEditor.Controls.SelectedAdorner.RectProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Rect<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.SelectedAdorner
   => control._set(NodeEditor.Controls.SelectedAdorner.RectProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Rect<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Rect> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.SelectedAdorner
=> control._setEx(NodeEditor.Controls.SelectedAdorner.RectProperty, ps, () => control.Rect = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // RectProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Rect<T>(this Style<T> style, Avalonia.Rect value) where T : NodeEditor.Controls.SelectedAdorner
=> style._addSetter(NodeEditor.Controls.SelectedAdorner.RectProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Rect<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.SelectedAdorner
=> style._addSetter(NodeEditor.Controls.SelectedAdorner.RectProperty, binding);

/*ValueOverloadsStyleSetterGenerator*/
public static Style<T> Rect<T>(this Style<T> style, System.Double x, System.Double y, System.Double width, System.Double height) where T : NodeEditor.Controls.SelectedAdorner
   => style._addSetter(NodeEditor.Controls.SelectedAdorner.RectProperty, new Avalonia.Rect(x, y, width, height));public static Style<T> Rect<T>(this Style<T> style, Avalonia.Size size) where T : NodeEditor.Controls.SelectedAdorner
   => style._addSetter(NodeEditor.Controls.SelectedAdorner.RectProperty, new Avalonia.Rect(size));public static Style<T> Rect<T>(this Style<T> style, Avalonia.Point position, Avalonia.Size size) where T : NodeEditor.Controls.SelectedAdorner
   => style._addSetter(NodeEditor.Controls.SelectedAdorner.RectProperty, new Avalonia.Rect(position, size));public static Style<T> Rect<T>(this Style<T> style, Avalonia.Point topLeft, Avalonia.Point bottomRight) where T : NodeEditor.Controls.SelectedAdorner
   => style._addSetter(NodeEditor.Controls.SelectedAdorner.RectProperty, new Avalonia.Rect(topLeft, bottomRight));



}
