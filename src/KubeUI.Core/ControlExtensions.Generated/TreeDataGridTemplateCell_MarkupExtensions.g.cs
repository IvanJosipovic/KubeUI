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
public static partial class TreeDataGridTemplateCell_MarkupExtensions
{
//================= Properties ======================//
 // ContentTemplate

/*BindFromExpressionSetterGenerator*/
public static T ContentTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.Primitives.TreeDataGridTemplateCell 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridTemplateCell.ContentTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ContentTemplate<T>(this T control,Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.Primitives.TreeDataGridTemplateCell 
=> control._setEx(Avalonia.Controls.Primitives.TreeDataGridTemplateCell.ContentTemplateProperty, ps, () => control.ContentTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ContentTemplate<T>(this T control, IBinding binding) where T : Avalonia.Controls.Primitives.TreeDataGridTemplateCell 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridTemplateCell.ContentTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ContentTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.Primitives.TreeDataGridTemplateCell 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridTemplateCell.ContentTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ContentTemplate<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.Primitives.TreeDataGridTemplateCell 
=> control._setEx(Avalonia.Controls.Primitives.TreeDataGridTemplateCell.ContentTemplateProperty, ps, () => control.ContentTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // EditingTemplate

/*BindFromExpressionSetterGenerator*/
public static T EditingTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.Primitives.TreeDataGridTemplateCell 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridTemplateCell.EditingTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T EditingTemplate<T>(this T control,Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.Primitives.TreeDataGridTemplateCell 
=> control._setEx(Avalonia.Controls.Primitives.TreeDataGridTemplateCell.EditingTemplateProperty, ps, () => control.EditingTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T EditingTemplate<T>(this T control, IBinding binding) where T : Avalonia.Controls.Primitives.TreeDataGridTemplateCell 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridTemplateCell.EditingTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T EditingTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.Primitives.TreeDataGridTemplateCell 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridTemplateCell.EditingTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T EditingTemplate<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.Primitives.TreeDataGridTemplateCell 
=> control._setEx(Avalonia.Controls.Primitives.TreeDataGridTemplateCell.EditingTemplateProperty, ps, () => control.EditingTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);



}
