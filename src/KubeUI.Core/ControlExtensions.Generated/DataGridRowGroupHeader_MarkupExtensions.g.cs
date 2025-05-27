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
public static partial class DataGridRowGroupHeader_MarkupExtensions
{
//================= Properties ======================//
 // IsItemCountVisible

/*BindFromExpressionSetterGenerator*/
public static T IsItemCountVisible<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGridRowGroupHeader 
   => control._set(Avalonia.Controls.DataGridRowGroupHeader.IsItemCountVisibleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsItemCountVisible<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridRowGroupHeader 
=> control._setEx(Avalonia.Controls.DataGridRowGroupHeader.IsItemCountVisibleProperty, ps, () => control.IsItemCountVisible = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsItemCountVisible<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGridRowGroupHeader 
   => control._set(Avalonia.Controls.DataGridRowGroupHeader.IsItemCountVisibleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsItemCountVisible<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGridRowGroupHeader 
   => control._set(Avalonia.Controls.DataGridRowGroupHeader.IsItemCountVisibleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsItemCountVisible<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridRowGroupHeader 
=> control._setEx(Avalonia.Controls.DataGridRowGroupHeader.IsItemCountVisibleProperty, ps, () => control.IsItemCountVisible = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ItemCountFormat

/*BindFromExpressionSetterGenerator*/
public static T ItemCountFormat<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGridRowGroupHeader 
   => control._set(Avalonia.Controls.DataGridRowGroupHeader.ItemCountFormatProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ItemCountFormat<T>(this T control,System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridRowGroupHeader 
=> control._setEx(Avalonia.Controls.DataGridRowGroupHeader.ItemCountFormatProperty, ps, () => control.ItemCountFormat = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ItemCountFormat<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGridRowGroupHeader 
   => control._set(Avalonia.Controls.DataGridRowGroupHeader.ItemCountFormatProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ItemCountFormat<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGridRowGroupHeader 
   => control._set(Avalonia.Controls.DataGridRowGroupHeader.ItemCountFormatProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ItemCountFormat<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridRowGroupHeader 
=> control._setEx(Avalonia.Controls.DataGridRowGroupHeader.ItemCountFormatProperty, ps, () => control.ItemCountFormat = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // Name

/*BindFromExpressionSetterGenerator*/
public static T Name<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGridRowGroupHeader 
   => control._set(Avalonia.Controls.DataGridRowGroupHeader.PropertyNameProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Name<T>(this T control,System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridRowGroupHeader 
=> control._setEx(Avalonia.Controls.DataGridRowGroupHeader.PropertyNameProperty, ps, () => control.Name = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Name<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGridRowGroupHeader 
   => control._set(Avalonia.Controls.DataGridRowGroupHeader.PropertyNameProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Name<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGridRowGroupHeader 
   => control._set(Avalonia.Controls.DataGridRowGroupHeader.PropertyNameProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Name<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridRowGroupHeader 
=> control._setEx(Avalonia.Controls.DataGridRowGroupHeader.PropertyNameProperty, ps, () => control.Name = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SublevelIndent

/*BindFromExpressionSetterGenerator*/
public static T SublevelIndent<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGridRowGroupHeader 
   => control._set(Avalonia.Controls.DataGridRowGroupHeader.SublevelIndentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SublevelIndent<T>(this T control,System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridRowGroupHeader 
=> control._setEx(Avalonia.Controls.DataGridRowGroupHeader.SublevelIndentProperty, ps, () => control.SublevelIndent = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SublevelIndent<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGridRowGroupHeader 
   => control._set(Avalonia.Controls.DataGridRowGroupHeader.SublevelIndentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SublevelIndent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGridRowGroupHeader 
   => control._set(Avalonia.Controls.DataGridRowGroupHeader.SublevelIndentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SublevelIndent<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridRowGroupHeader 
=> control._setEx(Avalonia.Controls.DataGridRowGroupHeader.SublevelIndentProperty, ps, () => control.SublevelIndent = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Styles ======================//
 // IsItemCountVisible

/*ValueStyleSetterGenerator*/
public static Style<T> IsItemCountVisible<T>(this Style<T> style, System.Boolean value) where T : Avalonia.Controls.DataGridRowGroupHeader 
=> style._addSetter(Avalonia.Controls.DataGridRowGroupHeader.IsItemCountVisibleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsItemCountVisible<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGridRowGroupHeader 
=> style._addSetter(Avalonia.Controls.DataGridRowGroupHeader.IsItemCountVisibleProperty, binding);


 // ItemCountFormat

/*ValueStyleSetterGenerator*/
public static Style<T> ItemCountFormat<T>(this Style<T> style, System.String value) where T : Avalonia.Controls.DataGridRowGroupHeader 
=> style._addSetter(Avalonia.Controls.DataGridRowGroupHeader.ItemCountFormatProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ItemCountFormat<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGridRowGroupHeader 
=> style._addSetter(Avalonia.Controls.DataGridRowGroupHeader.ItemCountFormatProperty, binding);


 // Name

/*ValueStyleSetterGenerator*/
public static Style<T> Name<T>(this Style<T> style, System.String value) where T : Avalonia.Controls.DataGridRowGroupHeader 
=> style._addSetter(Avalonia.Controls.DataGridRowGroupHeader.NameProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Name<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGridRowGroupHeader 
=> style._addSetter(Avalonia.Controls.DataGridRowGroupHeader.NameProperty, binding);


 // SublevelIndent

/*ValueStyleSetterGenerator*/
public static Style<T> SublevelIndent<T>(this Style<T> style, System.Double value) where T : Avalonia.Controls.DataGridRowGroupHeader 
=> style._addSetter(Avalonia.Controls.DataGridRowGroupHeader.SublevelIndentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SublevelIndent<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGridRowGroupHeader 
=> style._addSetter(Avalonia.Controls.DataGridRowGroupHeader.SublevelIndentProperty, binding);



}
