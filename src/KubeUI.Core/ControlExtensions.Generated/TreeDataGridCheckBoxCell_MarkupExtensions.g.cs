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
public static partial class TreeDataGridCheckBoxCell_MarkupExtensions
{
//================= Properties ======================//
 // IsReadOnly

/*BindFromExpressionSetterGenerator*/
public static T IsReadOnly<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.Primitives.TreeDataGridCheckBoxCell 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridCheckBoxCell.IsReadOnlyProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsReadOnly<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.Primitives.TreeDataGridCheckBoxCell 
=> control._setEx(Avalonia.Controls.Primitives.TreeDataGridCheckBoxCell.IsReadOnlyProperty, ps, () => control.IsReadOnly = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsReadOnly<T>(this T control, IBinding binding) where T : Avalonia.Controls.Primitives.TreeDataGridCheckBoxCell 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridCheckBoxCell.IsReadOnlyProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsReadOnly<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.Primitives.TreeDataGridCheckBoxCell 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridCheckBoxCell.IsReadOnlyProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsReadOnly<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.Primitives.TreeDataGridCheckBoxCell 
=> control._setEx(Avalonia.Controls.Primitives.TreeDataGridCheckBoxCell.IsReadOnlyProperty, ps, () => control.IsReadOnly = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsThreeState

/*BindFromExpressionSetterGenerator*/
public static T IsThreeState<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.Primitives.TreeDataGridCheckBoxCell 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridCheckBoxCell.IsThreeStateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsThreeState<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.Primitives.TreeDataGridCheckBoxCell 
=> control._setEx(Avalonia.Controls.Primitives.TreeDataGridCheckBoxCell.IsThreeStateProperty, ps, () => control.IsThreeState = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsThreeState<T>(this T control, IBinding binding) where T : Avalonia.Controls.Primitives.TreeDataGridCheckBoxCell 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridCheckBoxCell.IsThreeStateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsThreeState<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.Primitives.TreeDataGridCheckBoxCell 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridCheckBoxCell.IsThreeStateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsThreeState<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.Primitives.TreeDataGridCheckBoxCell 
=> control._setEx(Avalonia.Controls.Primitives.TreeDataGridCheckBoxCell.IsThreeStateProperty, ps, () => control.IsThreeState = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // Value

/*BindFromExpressionSetterGenerator*/
public static T Value<T>(this T control, Func<System.Nullable<System.Boolean>> func, Action<System.Nullable<System.Boolean>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.Primitives.TreeDataGridCheckBoxCell 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridCheckBoxCell.ValueProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Value<T>(this T control,System.Nullable<System.Boolean> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.Primitives.TreeDataGridCheckBoxCell 
=> control._setEx(Avalonia.Controls.Primitives.TreeDataGridCheckBoxCell.ValueProperty, ps, () => control.Value = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Value<T>(this T control, IBinding binding) where T : Avalonia.Controls.Primitives.TreeDataGridCheckBoxCell 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridCheckBoxCell.ValueProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Value<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.Primitives.TreeDataGridCheckBoxCell 
   => control._set(Avalonia.Controls.Primitives.TreeDataGridCheckBoxCell.ValueProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Value<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Nullable<System.Boolean>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.Primitives.TreeDataGridCheckBoxCell 
=> control._setEx(Avalonia.Controls.Primitives.TreeDataGridCheckBoxCell.ValueProperty, ps, () => control.Value = converter.TryConvert(value), bindingMode, converter, bindingSource);



}
