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
public static partial class Toolbox_MarkupExtensions
{
//================= Properties ======================//
 // TemplatesSource

/*BindFromExpressionSetterGenerator*/
public static T TemplatesSource<T>(this T control, Func<System.Collections.Generic.IEnumerable<NodeEditor.Model.INodeTemplate>> func, Action<System.Collections.Generic.IEnumerable<NodeEditor.Model.INodeTemplate>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : NodeEditor.Controls.Toolbox 
   => control._set(NodeEditor.Controls.Toolbox.TemplatesSourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T TemplatesSource<T>(this T control,System.Collections.Generic.IEnumerable<NodeEditor.Model.INodeTemplate> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.Toolbox 
=> control._setEx(NodeEditor.Controls.Toolbox.TemplatesSourceProperty, ps, () => control.TemplatesSource = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TemplatesSource<T>(this T control, IBinding binding) where T : NodeEditor.Controls.Toolbox 
   => control._set(NodeEditor.Controls.Toolbox.TemplatesSourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TemplatesSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : NodeEditor.Controls.Toolbox 
   => control._set(NodeEditor.Controls.Toolbox.TemplatesSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T TemplatesSource<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.Generic.IEnumerable<NodeEditor.Model.INodeTemplate>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : NodeEditor.Controls.Toolbox 
=> control._setEx(NodeEditor.Controls.Toolbox.TemplatesSourceProperty, ps, () => control.TemplatesSource = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Styles ======================//
 // TemplatesSource

/*ValueStyleSetterGenerator*/
public static Style<T> TemplatesSource<T>(this Style<T> style, System.Collections.Generic.IEnumerable<NodeEditor.Model.INodeTemplate> value) where T : NodeEditor.Controls.Toolbox 
=> style._addSetter(NodeEditor.Controls.Toolbox.TemplatesSourceProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> TemplatesSource<T>(this Style<T> style, IBinding binding) where T : NodeEditor.Controls.Toolbox 
=> style._addSetter(NodeEditor.Controls.Toolbox.TemplatesSourceProperty, binding);



}
