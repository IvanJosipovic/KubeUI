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
public static partial class CommandBarSeparator_MarkupExtensions
{
//================= Properties ======================//
 // DynamicOverflowOrder

/*ValueSetterGenerator*/
public static T DynamicOverflowOrder<T>(this T control, System.Int32 value) where T : FluentAvalonia.UI.Controls.CommandBarSeparator 
=> control._set(() => control.DynamicOverflowOrder = value!);

/*BindFromExpressionSetterGenerator*/
public static T DynamicOverflowOrder<T>(this T control, Func<System.Int32> func, Action<System.Int32>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : FluentAvalonia.UI.Controls.CommandBarSeparator 
   => control._set(FluentAvalonia.UI.Controls.CommandBarSeparator.DynamicOverflowOrderProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T DynamicOverflowOrder<T>(this T control,System.Int32 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBarSeparator 
=> control._setEx(FluentAvalonia.UI.Controls.CommandBarSeparator.DynamicOverflowOrderProperty, ps, () => control.DynamicOverflowOrder = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T DynamicOverflowOrder<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBarSeparator 
   => control._set(FluentAvalonia.UI.Controls.CommandBarSeparator.DynamicOverflowOrderProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T DynamicOverflowOrder<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.CommandBarSeparator 
   => control._set(FluentAvalonia.UI.Controls.CommandBarSeparator.DynamicOverflowOrderProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T DynamicOverflowOrder<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Int32> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBarSeparator 
=> control._setEx(FluentAvalonia.UI.Controls.CommandBarSeparator.DynamicOverflowOrderProperty, ps, () => control.DynamicOverflowOrder = converter.TryConvert(value)!, bindingMode, converter, bindingSource);


 // IsCompact

/*ValueSetterGenerator*/
public static T IsCompact<T>(this T control, System.Boolean value) where T : FluentAvalonia.UI.Controls.CommandBarSeparator 
=> control._set(() => control.IsCompact = value!);

/*BindFromExpressionSetterGenerator*/
public static T IsCompact<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : FluentAvalonia.UI.Controls.CommandBarSeparator 
   => control._set(FluentAvalonia.UI.Controls.CommandBarSeparator.IsCompactProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T IsCompact<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBarSeparator 
=> control._setEx(FluentAvalonia.UI.Controls.CommandBarSeparator.IsCompactProperty, ps, () => control.IsCompact = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsCompact<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBarSeparator 
   => control._set(FluentAvalonia.UI.Controls.CommandBarSeparator.IsCompactProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsCompact<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.CommandBarSeparator 
   => control._set(FluentAvalonia.UI.Controls.CommandBarSeparator.IsCompactProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T IsCompact<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBarSeparator 
=> control._setEx(FluentAvalonia.UI.Controls.CommandBarSeparator.IsCompactProperty, ps, () => control.IsCompact = converter.TryConvert(value)!, bindingMode, converter, bindingSource);



//================= Styles ======================//
 // IsCompact

/*ValueStyleSetterGenerator*/
public static Style<T> IsCompact<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.CommandBarSeparator 
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBarSeparator.IsCompactProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> IsCompact<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBarSeparator 
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBarSeparator.IsCompactProperty, binding);



}
