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
public static partial class StackLayout_MarkupExtensions
{
//================= Properties ======================//
 // Spacing

/*BindFromExpressionSetterGenerator*/
public static T Spacing<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.StackLayout 
   => control._set(FluentAvalonia.UI.Controls.StackLayout.SpacingProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Spacing<T>(this T control,System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.StackLayout 
=> control._setEx(FluentAvalonia.UI.Controls.StackLayout.SpacingProperty, ps, () => control.Spacing = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Spacing<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.StackLayout 
   => control._set(FluentAvalonia.UI.Controls.StackLayout.SpacingProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Spacing<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.StackLayout 
   => control._set(FluentAvalonia.UI.Controls.StackLayout.SpacingProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Spacing<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.StackLayout 
=> control._setEx(FluentAvalonia.UI.Controls.StackLayout.SpacingProperty, ps, () => control.Spacing = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // Orientation

/*BindFromExpressionSetterGenerator*/
public static T Orientation<T>(this T control, Func<Avalonia.Layout.Orientation> func, Action<Avalonia.Layout.Orientation>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.StackLayout 
   => control._set(FluentAvalonia.UI.Controls.StackLayout.OrientationProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Orientation<T>(this T control,Avalonia.Layout.Orientation value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.StackLayout 
=> control._setEx(FluentAvalonia.UI.Controls.StackLayout.OrientationProperty, ps, () => control.Orientation = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Orientation<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.StackLayout 
   => control._set(FluentAvalonia.UI.Controls.StackLayout.OrientationProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Orientation<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.StackLayout 
   => control._set(FluentAvalonia.UI.Controls.StackLayout.OrientationProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Orientation<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Layout.Orientation> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.StackLayout 
=> control._setEx(FluentAvalonia.UI.Controls.StackLayout.OrientationProperty, ps, () => control.Orientation = converter.TryConvert(value), bindingMode, converter, bindingSource);



}
