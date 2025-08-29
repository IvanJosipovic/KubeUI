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
public static partial class ColorSpectrum_MarkupExtensions
{
//================= Properties ======================//
 // Shape

/*BindFromExpressionSetterGenerator*/
public static T Shape<T>(this T control, Func<FluentAvalonia.UI.Controls.ColorSpectrumShape> func, Action<FluentAvalonia.UI.Controls.ColorSpectrumShape>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorSpectrum 
   => control._set(FluentAvalonia.UI.Controls.ColorSpectrum.ShapeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Shape<T>(this T control,FluentAvalonia.UI.Controls.ColorSpectrumShape value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorSpectrum 
=> control._setEx(FluentAvalonia.UI.Controls.ColorSpectrum.ShapeProperty, ps, () => control.Shape = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Shape<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorSpectrum 
   => control._set(FluentAvalonia.UI.Controls.ColorSpectrum.ShapeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Shape<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorSpectrum 
   => control._set(FluentAvalonia.UI.Controls.ColorSpectrum.ShapeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Shape<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.ColorSpectrumShape> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorSpectrum 
=> control._setEx(FluentAvalonia.UI.Controls.ColorSpectrum.ShapeProperty, ps, () => control.Shape = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // BorderBrush

/*BindFromExpressionSetterGenerator*/
public static T BorderBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorSpectrum 
   => control._set(FluentAvalonia.UI.Controls.ColorSpectrum.BorderBrushProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T BorderBrush<T>(this T control,Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorSpectrum 
=> control._setEx(FluentAvalonia.UI.Controls.ColorSpectrum.BorderBrushProperty, ps, () => control.BorderBrush = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T BorderBrush<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorSpectrum 
   => control._set(FluentAvalonia.UI.Controls.ColorSpectrum.BorderBrushProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T BorderBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorSpectrum 
   => control._set(FluentAvalonia.UI.Controls.ColorSpectrum.BorderBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T BorderBrush<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorSpectrum 
=> control._setEx(FluentAvalonia.UI.Controls.ColorSpectrum.BorderBrushProperty, ps, () => control.BorderBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // BorderThickness

/*BindFromExpressionSetterGenerator*/
public static T BorderThickness<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorSpectrum 
   => control._set(FluentAvalonia.UI.Controls.ColorSpectrum.BorderThicknessProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T BorderThickness<T>(this T control,System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorSpectrum 
=> control._setEx(FluentAvalonia.UI.Controls.ColorSpectrum.BorderThicknessProperty, ps, () => control.BorderThickness = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T BorderThickness<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorSpectrum 
   => control._set(FluentAvalonia.UI.Controls.ColorSpectrum.BorderThicknessProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T BorderThickness<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorSpectrum 
   => control._set(FluentAvalonia.UI.Controls.ColorSpectrum.BorderThicknessProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T BorderThickness<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorSpectrum 
=> control._setEx(FluentAvalonia.UI.Controls.ColorSpectrum.BorderThicknessProperty, ps, () => control.BorderThickness = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Styles ======================//
 // BorderBrush

/*ValueStyleSetterGenerator*/
public static Style<T> BorderBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : FluentAvalonia.UI.Controls.ColorSpectrum 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorSpectrum.BorderBrushProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> BorderBrush<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorSpectrum 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorSpectrum.BorderBrushProperty, binding);


 // BorderThickness

/*ValueStyleSetterGenerator*/
public static Style<T> BorderThickness<T>(this Style<T> style, System.Double value) where T : FluentAvalonia.UI.Controls.ColorSpectrum 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorSpectrum.BorderThicknessProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> BorderThickness<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorSpectrum 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorSpectrum.BorderThicknessProperty, binding);



}
