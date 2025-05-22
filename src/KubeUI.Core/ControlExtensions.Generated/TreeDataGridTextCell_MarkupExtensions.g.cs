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
public static partial class TreeDataGridTextCell_MarkupExtensions
{
//================= Properties ======================//
 // TextTrimming

/*BindFromExpressionSetterGenerator*/
public static T TextTrimming<T>(this T control, Func<Avalonia.Media.TextTrimming> func, Action<Avalonia.Media.TextTrimming>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.Primitives.TreeDataGridTextCell 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridTextCell.TextTrimmingProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T TextTrimming<T>(this T control,Avalonia.Media.TextTrimming value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.Primitives.TreeDataGridTextCell 
=> control._setEx(Avalonia.Controls.Primitives.TreeDataGridTextCell.TextTrimmingProperty, ps, () => control.TextTrimming = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TextTrimming<T>(this T control, IBinding binding) where T : Avalonia.Controls.Primitives.TreeDataGridTextCell 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridTextCell.TextTrimmingProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TextTrimming<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.Primitives.TreeDataGridTextCell 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridTextCell.TextTrimmingProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T TextTrimming<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.TextTrimming> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.Primitives.TreeDataGridTextCell 
=> control._setEx(Avalonia.Controls.Primitives.TreeDataGridTextCell.TextTrimmingProperty, ps, () => control.TextTrimming = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TextWrapping

/*BindFromExpressionSetterGenerator*/
public static T TextWrapping<T>(this T control, Func<Avalonia.Media.TextWrapping> func, Action<Avalonia.Media.TextWrapping>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.Primitives.TreeDataGridTextCell 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridTextCell.TextWrappingProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T TextWrapping<T>(this T control,Avalonia.Media.TextWrapping value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.Primitives.TreeDataGridTextCell 
=> control._setEx(Avalonia.Controls.Primitives.TreeDataGridTextCell.TextWrappingProperty, ps, () => control.TextWrapping = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TextWrapping<T>(this T control, IBinding binding) where T : Avalonia.Controls.Primitives.TreeDataGridTextCell 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridTextCell.TextWrappingProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TextWrapping<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.Primitives.TreeDataGridTextCell 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridTextCell.TextWrappingProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T TextWrapping<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.TextWrapping> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.Primitives.TreeDataGridTextCell 
=> control._setEx(Avalonia.Controls.Primitives.TreeDataGridTextCell.TextWrappingProperty, ps, () => control.TextWrapping = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // Value

/*BindFromExpressionSetterGenerator*/
public static T Value<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.Primitives.TreeDataGridTextCell 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridTextCell.ValueProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Value<T>(this T control,System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.Primitives.TreeDataGridTextCell 
=> control._setEx(Avalonia.Controls.Primitives.TreeDataGridTextCell.ValueProperty, ps, () => control.Value = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Value<T>(this T control, IBinding binding) where T : Avalonia.Controls.Primitives.TreeDataGridTextCell 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridTextCell.ValueProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Value<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.Primitives.TreeDataGridTextCell 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridTextCell.ValueProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Value<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.Primitives.TreeDataGridTextCell 
=> control._setEx(Avalonia.Controls.Primitives.TreeDataGridTextCell.ValueProperty, ps, () => control.Value = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TextAlignment

/*BindFromExpressionSetterGenerator*/
public static T TextAlignment<T>(this T control, Func<Avalonia.Media.TextAlignment> func, Action<Avalonia.Media.TextAlignment>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.Primitives.TreeDataGridTextCell 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridTextCell.TextAlignmentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T TextAlignment<T>(this T control,Avalonia.Media.TextAlignment value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.Primitives.TreeDataGridTextCell 
=> control._setEx(Avalonia.Controls.Primitives.TreeDataGridTextCell.TextAlignmentProperty, ps, () => control.TextAlignment = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TextAlignment<T>(this T control, IBinding binding) where T : Avalonia.Controls.Primitives.TreeDataGridTextCell 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridTextCell.TextAlignmentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TextAlignment<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.Primitives.TreeDataGridTextCell 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridTextCell.TextAlignmentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T TextAlignment<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.TextAlignment> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.Primitives.TreeDataGridTextCell 
=> control._setEx(Avalonia.Controls.Primitives.TreeDataGridTextCell.TextAlignmentProperty, ps, () => control.TextAlignment = converter.TryConvert(value), bindingMode, converter, bindingSource);



}
