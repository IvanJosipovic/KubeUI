#nullable enable
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using DataGridTemplateColumn = Avalonia.Controls.DataGridTemplateColumn;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class DataGridTemplateColumnExtensions
{
public static T CellTemplate<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGridTemplateColumn
   => control._set(Avalonia.Controls.DataGridTemplateColumn.CellTemplateProperty, binding);
public static T CellTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGridTemplateColumn
   => control._set(Avalonia.Controls.DataGridTemplateColumn.CellTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CellTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGridTemplateColumn
   => control._set(Avalonia.Controls.DataGridTemplateColumn.CellTemplateProperty, func, onChanged, expression);
public static T CellTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridTemplateColumn
=> control._setEx(Avalonia.Controls.DataGridTemplateColumn.CellTemplateProperty, ps, () => control.CellTemplate = value, bindingMode, converter, bindingSource);
public static T CellTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridTemplateColumn
=> control._setEx(Avalonia.Controls.DataGridTemplateColumn.CellTemplateProperty, ps, () => control.CellTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T CellEditingTemplate<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGridTemplateColumn
   => control._set(Avalonia.Controls.DataGridTemplateColumn.CellEditingTemplateProperty, binding);
public static T CellEditingTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGridTemplateColumn
   => control._set(Avalonia.Controls.DataGridTemplateColumn.CellEditingTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CellEditingTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGridTemplateColumn
   => control._set(Avalonia.Controls.DataGridTemplateColumn.CellEditingTemplateProperty, func, onChanged, expression);
public static T CellEditingTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridTemplateColumn
=> control._setEx(Avalonia.Controls.DataGridTemplateColumn.CellEditingTemplateProperty, ps, () => control.CellEditingTemplate = value, bindingMode, converter, bindingSource);
public static T CellEditingTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridTemplateColumn
=> control._setEx(Avalonia.Controls.DataGridTemplateColumn.CellEditingTemplateProperty, ps, () => control.CellEditingTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

