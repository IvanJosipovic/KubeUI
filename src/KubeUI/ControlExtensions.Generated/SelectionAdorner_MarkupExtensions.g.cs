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
public static partial class SelectionAdorner_MarkupExtensions
{
//================= Properties ======================//
 // TopLeft

/*BindFromExpressionSetterGenerator*/
public static T TopLeft<T>(this T control, Func<Avalonia.Point> func, Action<Avalonia.Point>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.SelectionAdorner 
   => control._set(NodeEditor.Controls.SelectionAdorner.TopLeftProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T TopLeft<T>(this T control,Avalonia.Point value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.SelectionAdorner 
=> control._setEx(NodeEditor.Controls.SelectionAdorner.TopLeftProperty, ps, () => control.TopLeft = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TopLeft<T>(this T control, IBinding binding) where T : NodeEditor.Controls.SelectionAdorner 
   => control._set(NodeEditor.Controls.SelectionAdorner.TopLeftProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TopLeft<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.SelectionAdorner 
   => control._set(NodeEditor.Controls.SelectionAdorner.TopLeftProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T TopLeft<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Point> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.SelectionAdorner 
=> control._setEx(NodeEditor.Controls.SelectionAdorner.TopLeftProperty, ps, () => control.TopLeft = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // BottomRight

/*BindFromExpressionSetterGenerator*/
public static T BottomRight<T>(this T control, Func<Avalonia.Point> func, Action<Avalonia.Point>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.SelectionAdorner 
   => control._set(NodeEditor.Controls.SelectionAdorner.BottomRightProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T BottomRight<T>(this T control,Avalonia.Point value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.SelectionAdorner 
=> control._setEx(NodeEditor.Controls.SelectionAdorner.BottomRightProperty, ps, () => control.BottomRight = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T BottomRight<T>(this T control, IBinding binding) where T : NodeEditor.Controls.SelectionAdorner 
   => control._set(NodeEditor.Controls.SelectionAdorner.BottomRightProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T BottomRight<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.SelectionAdorner 
   => control._set(NodeEditor.Controls.SelectionAdorner.BottomRightProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T BottomRight<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Point> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.SelectionAdorner 
=> control._setEx(NodeEditor.Controls.SelectionAdorner.BottomRightProperty, ps, () => control.BottomRight = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Styles ======================//
 // TopLeft

/*ValueStyleSetterGenerator*/
public static Style<T> TopLeft<T>(this Style<T> style, Avalonia.Point value) where T : NodeEditor.Controls.SelectionAdorner 
=> style._addSetter(NodeEditor.Controls.SelectionAdorner.TopLeftProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> TopLeft<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.SelectionAdorner 
=> style._addSetter(NodeEditor.Controls.SelectionAdorner.TopLeftProperty, binding);


 // BottomRight

/*ValueStyleSetterGenerator*/
public static Style<T> BottomRight<T>(this Style<T> style, Avalonia.Point value) where T : NodeEditor.Controls.SelectionAdorner 
=> style._addSetter(NodeEditor.Controls.SelectionAdorner.BottomRightProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> BottomRight<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.SelectionAdorner 
=> style._addSetter(NodeEditor.Controls.SelectionAdorner.BottomRightProperty, binding);



}
