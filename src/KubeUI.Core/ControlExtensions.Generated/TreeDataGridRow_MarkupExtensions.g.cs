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
public static partial class TreeDataGridRow_MarkupExtensions
{
//================= Properties ======================//
 // ElementFactory

/*BindFromExpressionSetterGenerator*/
public static T ElementFactory<T>(this T control, Func<Avalonia.Controls.Primitives.TreeDataGridElementFactory> func, Action<Avalonia.Controls.Primitives.TreeDataGridElementFactory>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.Primitives.TreeDataGridRow 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridRow.ElementFactoryProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ElementFactory<T>(this T control,Avalonia.Controls.Primitives.TreeDataGridElementFactory value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.Primitives.TreeDataGridRow 
=> control._setEx(Avalonia.Controls.Primitives.TreeDataGridRow.ElementFactoryProperty, ps, () => control.ElementFactory = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ElementFactory<T>(this T control, IBinding binding) where T : Avalonia.Controls.Primitives.TreeDataGridRow 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridRow.ElementFactoryProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ElementFactory<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.Primitives.TreeDataGridRow 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridRow.ElementFactoryProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ElementFactory<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Primitives.TreeDataGridElementFactory> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.Primitives.TreeDataGridRow 
=> control._setEx(Avalonia.Controls.Primitives.TreeDataGridRow.ElementFactoryProperty, ps, () => control.ElementFactory = converter.TryConvert(value), bindingMode, converter, bindingSource);



}
