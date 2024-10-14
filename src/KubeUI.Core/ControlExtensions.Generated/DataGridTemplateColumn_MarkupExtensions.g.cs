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
public static partial class DataGridTemplateColumn_MarkupExtensions
{
//================= Properties ======================//
 // CellTemplate

/*BindFromExpressionSetterGenerator*/
public static T CellTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGridTemplateColumn 
   => control._set(Avalonia.Controls.DataGridTemplateColumn.CellTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CellTemplate<T>(this T control,Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridTemplateColumn 
=> control._setEx(Avalonia.Controls.DataGridTemplateColumn.CellTemplateProperty, ps, () => control.CellTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CellTemplate<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGridTemplateColumn 
   => control._set(Avalonia.Controls.DataGridTemplateColumn.CellTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CellTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGridTemplateColumn 
   => control._set(Avalonia.Controls.DataGridTemplateColumn.CellTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CellTemplate<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridTemplateColumn 
=> control._setEx(Avalonia.Controls.DataGridTemplateColumn.CellTemplateProperty, ps, () => control.CellTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CellEditingTemplate

/*BindFromExpressionSetterGenerator*/
public static T CellEditingTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGridTemplateColumn 
   => control._set(Avalonia.Controls.DataGridTemplateColumn.CellEditingTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CellEditingTemplate<T>(this T control,Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridTemplateColumn 
=> control._setEx(Avalonia.Controls.DataGridTemplateColumn.CellEditingTemplateProperty, ps, () => control.CellEditingTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CellEditingTemplate<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGridTemplateColumn 
   => control._set(Avalonia.Controls.DataGridTemplateColumn.CellEditingTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CellEditingTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGridTemplateColumn 
   => control._set(Avalonia.Controls.DataGridTemplateColumn.CellEditingTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CellEditingTemplate<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridTemplateColumn 
=> control._setEx(Avalonia.Controls.DataGridTemplateColumn.CellEditingTemplateProperty, ps, () => control.CellEditingTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);



}
