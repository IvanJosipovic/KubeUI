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
public static partial class AbstractMargin_MarkupExtensions
{
//================= Properties ======================//
 // TextViewProperty

/*BindFromExpressionSetterGenerator*/
public static T TextView<T>(this T control, Func<AvaloniaEdit.Rendering.TextView> func, Action<AvaloniaEdit.Rendering.TextView>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Editing.AbstractMargin
   => control._set(AvaloniaEdit.Editing.AbstractMargin.TextViewProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T TextView<T>(this T control, AvaloniaEdit.Rendering.TextView value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Editing.AbstractMargin
=> control._setEx(AvaloniaEdit.Editing.AbstractMargin.TextViewProperty, ps, () => control.TextView = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TextView<T>(this T control, IBinding binding) where T : AvaloniaEdit.Editing.AbstractMargin
   => control._set(AvaloniaEdit.Editing.AbstractMargin.TextViewProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TextView<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Editing.AbstractMargin
   => control._set(AvaloniaEdit.Editing.AbstractMargin.TextViewProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T TextView<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, AvaloniaEdit.Rendering.TextView> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Editing.AbstractMargin
=> control._setEx(AvaloniaEdit.Editing.AbstractMargin.TextViewProperty, ps, () => control.TextView = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // TextViewProperty

/*ValueStyleSetterGenerator*/
public static Style<T> TextView<T>(this Style<T> style, AvaloniaEdit.Rendering.TextView value) where T : AvaloniaEdit.Editing.AbstractMargin
=> style._addSetter(AvaloniaEdit.Editing.AbstractMargin.TextViewProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> TextView<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Editing.AbstractMargin
=> style._addSetter(AvaloniaEdit.Editing.AbstractMargin.TextViewProperty, binding);



}
