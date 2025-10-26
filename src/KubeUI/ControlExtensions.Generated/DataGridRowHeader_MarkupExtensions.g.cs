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
public static partial class DataGridRowHeader_MarkupExtensions
{
//================= Properties ======================//
 // SeparatorBrush

/*BindFromExpressionSetterGenerator*/
public static T SeparatorBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.Primitives.DataGridRowHeader 
   => control._set(Avalonia.Controls.Primitives.DataGridRowHeader.SeparatorBrushProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SeparatorBrush<T>(this T control,Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.Primitives.DataGridRowHeader 
=> control._setEx(Avalonia.Controls.Primitives.DataGridRowHeader.SeparatorBrushProperty, ps, () => control.SeparatorBrush = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SeparatorBrush<T>(this T control, IBinding binding) where T : Avalonia.Controls.Primitives.DataGridRowHeader 
   => control._set(Avalonia.Controls.Primitives.DataGridRowHeader.SeparatorBrushProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SeparatorBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.Primitives.DataGridRowHeader 
   => control._set(Avalonia.Controls.Primitives.DataGridRowHeader.SeparatorBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SeparatorBrush<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.Primitives.DataGridRowHeader 
=> control._setEx(Avalonia.Controls.Primitives.DataGridRowHeader.SeparatorBrushProperty, ps, () => control.SeparatorBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // AreSeparatorsVisible

/*BindFromExpressionSetterGenerator*/
public static T AreSeparatorsVisible<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.Primitives.DataGridRowHeader 
   => control._set(Avalonia.Controls.Primitives.DataGridRowHeader.AreSeparatorsVisibleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T AreSeparatorsVisible<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.Primitives.DataGridRowHeader 
=> control._setEx(Avalonia.Controls.Primitives.DataGridRowHeader.AreSeparatorsVisibleProperty, ps, () => control.AreSeparatorsVisible = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T AreSeparatorsVisible<T>(this T control, IBinding binding) where T : Avalonia.Controls.Primitives.DataGridRowHeader 
   => control._set(Avalonia.Controls.Primitives.DataGridRowHeader.AreSeparatorsVisibleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T AreSeparatorsVisible<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.Primitives.DataGridRowHeader 
   => control._set(Avalonia.Controls.Primitives.DataGridRowHeader.AreSeparatorsVisibleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T AreSeparatorsVisible<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.Primitives.DataGridRowHeader 
=> control._setEx(Avalonia.Controls.Primitives.DataGridRowHeader.AreSeparatorsVisibleProperty, ps, () => control.AreSeparatorsVisible = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Styles ======================//
 // SeparatorBrush

/*ValueStyleSetterGenerator*/
public static Style<T> SeparatorBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Avalonia.Controls.Primitives.DataGridRowHeader 
=> style._addSetter(Avalonia.Controls.Primitives.DataGridRowHeader.SeparatorBrushProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SeparatorBrush<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.Primitives.DataGridRowHeader 
=> style._addSetter(Avalonia.Controls.Primitives.DataGridRowHeader.SeparatorBrushProperty, binding);


 // AreSeparatorsVisible

/*ValueStyleSetterGenerator*/
public static Style<T> AreSeparatorsVisible<T>(this Style<T> style, System.Boolean value) where T : Avalonia.Controls.Primitives.DataGridRowHeader 
=> style._addSetter(Avalonia.Controls.Primitives.DataGridRowHeader.AreSeparatorsVisibleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> AreSeparatorsVisible<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.Primitives.DataGridRowHeader 
=> style._addSetter(Avalonia.Controls.Primitives.DataGridRowHeader.AreSeparatorsVisibleProperty, binding);



}
