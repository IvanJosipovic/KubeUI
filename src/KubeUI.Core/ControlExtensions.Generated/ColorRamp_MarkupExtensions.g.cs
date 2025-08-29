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
public static partial class ColorRamp_MarkupExtensions
{
//================= Properties ======================//
 // Orientation

/*BindFromExpressionSetterGenerator*/
public static T Orientation<T>(this T control, Func<Avalonia.Layout.Orientation> func, Action<Avalonia.Layout.Orientation>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorRamp 
   => control._set(FluentAvalonia.UI.Controls.ColorRamp.OrientationProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Orientation<T>(this T control,Avalonia.Layout.Orientation value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorRamp 
=> control._setEx(FluentAvalonia.UI.Controls.ColorRamp.OrientationProperty, ps, () => control.Orientation = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Orientation<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorRamp 
   => control._set(FluentAvalonia.UI.Controls.ColorRamp.OrientationProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Orientation<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorRamp 
   => control._set(FluentAvalonia.UI.Controls.ColorRamp.OrientationProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Orientation<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Layout.Orientation> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorRamp 
=> control._setEx(FluentAvalonia.UI.Controls.ColorRamp.OrientationProperty, ps, () => control.Orientation = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // BorderBrush

/*BindFromExpressionSetterGenerator*/
public static T BorderBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorRamp 
   => control._set(FluentAvalonia.UI.Controls.ColorRamp.BorderBrushProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T BorderBrush<T>(this T control,Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorRamp 
=> control._setEx(FluentAvalonia.UI.Controls.ColorRamp.BorderBrushProperty, ps, () => control.BorderBrush = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T BorderBrush<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorRamp 
   => control._set(FluentAvalonia.UI.Controls.ColorRamp.BorderBrushProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T BorderBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorRamp 
   => control._set(FluentAvalonia.UI.Controls.ColorRamp.BorderBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T BorderBrush<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorRamp 
=> control._setEx(FluentAvalonia.UI.Controls.ColorRamp.BorderBrushProperty, ps, () => control.BorderBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // BorderThickness

/*BindFromExpressionSetterGenerator*/
public static T BorderThickness<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorRamp 
   => control._set(FluentAvalonia.UI.Controls.ColorRamp.BorderThicknessProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T BorderThickness<T>(this T control,System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorRamp 
=> control._setEx(FluentAvalonia.UI.Controls.ColorRamp.BorderThicknessProperty, ps, () => control.BorderThickness = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T BorderThickness<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorRamp 
   => control._set(FluentAvalonia.UI.Controls.ColorRamp.BorderThicknessProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T BorderThickness<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorRamp 
   => control._set(FluentAvalonia.UI.Controls.ColorRamp.BorderThicknessProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T BorderThickness<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorRamp 
=> control._setEx(FluentAvalonia.UI.Controls.ColorRamp.BorderThicknessProperty, ps, () => control.BorderThickness = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CornerRadius

/*BindFromExpressionSetterGenerator*/
public static T CornerRadius<T>(this T control, Func<Avalonia.CornerRadius> func, Action<Avalonia.CornerRadius>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorRamp 
   => control._set(FluentAvalonia.UI.Controls.ColorRamp.CornerRadiusProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CornerRadius<T>(this T control,Avalonia.CornerRadius value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorRamp 
=> control._setEx(FluentAvalonia.UI.Controls.ColorRamp.CornerRadiusProperty, ps, () => control.CornerRadius = value, bindingMode, converter, bindingSource);

/*ValueOverloadsSetterGenerator*/

public static T CornerRadius<T>(this T control, System.Double uniformRadius = default) where T : FluentAvalonia.UI.Controls.ColorRamp 
   => control._set(() => control.CornerRadius = new Avalonia.CornerRadius(uniformRadius));
public static T CornerRadius<T>(this T control, System.Double top = default, System.Double bottom = default) where T : FluentAvalonia.UI.Controls.ColorRamp 
   => control._set(() => control.CornerRadius = new Avalonia.CornerRadius(top, bottom));
public static T CornerRadius<T>(this T control, System.Double topLeft = default, System.Double topRight = default, System.Double bottomRight = default, System.Double bottomLeft = default) where T : FluentAvalonia.UI.Controls.ColorRamp 
   => control._set(() => control.CornerRadius = new Avalonia.CornerRadius(topLeft, topRight, bottomRight, bottomLeft));

/*BindSetterGenerator*/
public static T CornerRadius<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorRamp 
   => control._set(FluentAvalonia.UI.Controls.ColorRamp.CornerRadiusProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CornerRadius<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorRamp 
   => control._set(FluentAvalonia.UI.Controls.ColorRamp.CornerRadiusProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CornerRadius<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.CornerRadius> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorRamp 
=> control._setEx(FluentAvalonia.UI.Controls.ColorRamp.CornerRadiusProperty, ps, () => control.CornerRadius = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Styles ======================//
 // BorderBrush

/*ValueStyleSetterGenerator*/
public static Style<T> BorderBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : FluentAvalonia.UI.Controls.ColorRamp 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorRamp.BorderBrushProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> BorderBrush<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorRamp 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorRamp.BorderBrushProperty, binding);


 // BorderThickness

/*ValueStyleSetterGenerator*/
public static Style<T> BorderThickness<T>(this Style<T> style, System.Double value) where T : FluentAvalonia.UI.Controls.ColorRamp 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorRamp.BorderThicknessProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> BorderThickness<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorRamp 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorRamp.BorderThicknessProperty, binding);


 // CornerRadius

/*ValueStyleSetterGenerator*/
public static Style<T> CornerRadius<T>(this Style<T> style, Avalonia.CornerRadius value) where T : FluentAvalonia.UI.Controls.ColorRamp 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorRamp.CornerRadiusProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CornerRadius<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorRamp 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorRamp.CornerRadiusProperty, binding);

/*ValueOverloadsStyleSetterGenerator*/
public static Style<T> CornerRadius<T>(this Style<T> style, System.Double uniformRadius) where T : FluentAvalonia.UI.Controls.ColorRamp 
   => style._addSetter(FluentAvalonia.UI.Controls.ColorRamp.CornerRadiusProperty, new Avalonia.CornerRadius(uniformRadius));public static Style<T> CornerRadius<T>(this Style<T> style, System.Double top, System.Double bottom) where T : FluentAvalonia.UI.Controls.ColorRamp 
   => style._addSetter(FluentAvalonia.UI.Controls.ColorRamp.CornerRadiusProperty, new Avalonia.CornerRadius(top, bottom));public static Style<T> CornerRadius<T>(this Style<T> style, System.Double topLeft, System.Double topRight, System.Double bottomRight, System.Double bottomLeft) where T : FluentAvalonia.UI.Controls.ColorRamp 
   => style._addSetter(FluentAvalonia.UI.Controls.ColorRamp.CornerRadiusProperty, new Avalonia.CornerRadius(topLeft, topRight, bottomRight, bottomLeft));



}
