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
public static partial class CommandBarToggleButton_MarkupExtensions
{
//================= Properties ======================//
 // IconSource

/*BindFromExpressionSetterGenerator*/
public static T IconSource<T>(this T control, Func<FluentAvalonia.UI.Controls.IconSource> func, Action<FluentAvalonia.UI.Controls.IconSource>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.CommandBarToggleButton 
   => control._set(FluentAvalonia.UI.Controls.CommandBarToggleButton.IconSourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IconSource<T>(this T control,FluentAvalonia.UI.Controls.IconSource value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBarToggleButton 
=> control._setEx(FluentAvalonia.UI.Controls.CommandBarToggleButton.IconSourceProperty, ps, () => control.IconSource = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IconSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBarToggleButton 
   => control._set(FluentAvalonia.UI.Controls.CommandBarToggleButton.IconSourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IconSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.CommandBarToggleButton 
   => control._set(FluentAvalonia.UI.Controls.CommandBarToggleButton.IconSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IconSource<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.IconSource> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBarToggleButton 
=> control._setEx(FluentAvalonia.UI.Controls.CommandBarToggleButton.IconSourceProperty, ps, () => control.IconSource = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // Label

/*BindFromExpressionSetterGenerator*/
public static T Label<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.CommandBarToggleButton 
   => control._set(FluentAvalonia.UI.Controls.CommandBarToggleButton.LabelProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Label<T>(this T control,System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBarToggleButton 
=> control._setEx(FluentAvalonia.UI.Controls.CommandBarToggleButton.LabelProperty, ps, () => control.Label = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Label<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBarToggleButton 
   => control._set(FluentAvalonia.UI.Controls.CommandBarToggleButton.LabelProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Label<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.CommandBarToggleButton 
   => control._set(FluentAvalonia.UI.Controls.CommandBarToggleButton.LabelProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Label<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBarToggleButton 
=> control._setEx(FluentAvalonia.UI.Controls.CommandBarToggleButton.LabelProperty, ps, () => control.Label = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // DynamicOverflowOrder

/*BindFromExpressionSetterGenerator*/
public static T DynamicOverflowOrder<T>(this T control, Func<System.Int32> func, Action<System.Int32>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.CommandBarToggleButton 
   => control._set(FluentAvalonia.UI.Controls.CommandBarToggleButton.DynamicOverflowOrderProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T DynamicOverflowOrder<T>(this T control,System.Int32 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBarToggleButton 
=> control._setEx(FluentAvalonia.UI.Controls.CommandBarToggleButton.DynamicOverflowOrderProperty, ps, () => control.DynamicOverflowOrder = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T DynamicOverflowOrder<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBarToggleButton 
   => control._set(FluentAvalonia.UI.Controls.CommandBarToggleButton.DynamicOverflowOrderProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T DynamicOverflowOrder<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.CommandBarToggleButton 
   => control._set(FluentAvalonia.UI.Controls.CommandBarToggleButton.DynamicOverflowOrderProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T DynamicOverflowOrder<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Int32> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBarToggleButton 
=> control._setEx(FluentAvalonia.UI.Controls.CommandBarToggleButton.DynamicOverflowOrderProperty, ps, () => control.DynamicOverflowOrder = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsCompact

/*BindFromExpressionSetterGenerator*/
public static T IsCompact<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.CommandBarToggleButton 
   => control._set(FluentAvalonia.UI.Controls.CommandBarToggleButton.IsCompactProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsCompact<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBarToggleButton 
=> control._setEx(FluentAvalonia.UI.Controls.CommandBarToggleButton.IsCompactProperty, ps, () => control.IsCompact = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsCompact<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBarToggleButton 
   => control._set(FluentAvalonia.UI.Controls.CommandBarToggleButton.IsCompactProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsCompact<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.CommandBarToggleButton 
   => control._set(FluentAvalonia.UI.Controls.CommandBarToggleButton.IsCompactProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsCompact<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBarToggleButton 
=> control._setEx(FluentAvalonia.UI.Controls.CommandBarToggleButton.IsCompactProperty, ps, () => control.IsCompact = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Styles ======================//
 // IconSource

/*ValueStyleSetterGenerator*/
public static Style<T> IconSource<T>(this Style<T> style, FluentAvalonia.UI.Controls.IconSource value) where T : FluentAvalonia.UI.Controls.CommandBarToggleButton 
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBarToggleButton.IconSourceProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IconSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBarToggleButton 
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBarToggleButton.IconSourceProperty, binding);


 // Label

/*ValueStyleSetterGenerator*/
public static Style<T> Label<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.CommandBarToggleButton 
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBarToggleButton.LabelProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Label<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBarToggleButton 
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBarToggleButton.LabelProperty, binding);


 // IsCompact

/*ValueStyleSetterGenerator*/
public static Style<T> IsCompact<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.CommandBarToggleButton 
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBarToggleButton.IsCompactProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsCompact<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBarToggleButton 
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBarToggleButton.IsCompactProperty, binding);



}
