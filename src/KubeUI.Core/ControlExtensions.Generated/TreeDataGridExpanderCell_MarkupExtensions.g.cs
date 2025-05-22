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
public static partial class TreeDataGridExpanderCell_MarkupExtensions
{
//================= Properties ======================//
 // IsExpanded

/*BindFromExpressionSetterGenerator*/
public static T IsExpanded<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.Primitives.TreeDataGridExpanderCell 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridExpanderCell.IsExpandedProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsExpanded<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.Primitives.TreeDataGridExpanderCell 
=> control._setEx(Avalonia.Controls.Primitives.TreeDataGridExpanderCell.IsExpandedProperty, ps, () => control.IsExpanded = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsExpanded<T>(this T control, IBinding binding) where T : Avalonia.Controls.Primitives.TreeDataGridExpanderCell 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridExpanderCell.IsExpandedProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsExpanded<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.Primitives.TreeDataGridExpanderCell 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridExpanderCell.IsExpandedProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsExpanded<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.Primitives.TreeDataGridExpanderCell 
=> control._setEx(Avalonia.Controls.Primitives.TreeDataGridExpanderCell.IsExpandedProperty, ps, () => control.IsExpanded = converter.TryConvert(value), bindingMode, converter, bindingSource);



}
