#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
[global::System.CodeDom.Compiler.GeneratedCode("AvaloniaExtensionGenerator", "11.1.3.0")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static partial class ImageViewer_MarkupExtensions
{
//================= Properties ======================//
 // OverlayerProperty

/*BindFromExpressionSetterGenerator*/
public static T Overlayer<T>(this T control, Func<Avalonia.Controls.Control> func, Action<Avalonia.Controls.Control>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ImageViewer
   => control._set(Ursa.Controls.ImageViewer.OverlayerProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Overlayer<T>(this T control, Avalonia.Controls.Control value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ImageViewer
=> control._setEx(Ursa.Controls.ImageViewer.OverlayerProperty, ps, () => control.Overlayer = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Overlayer<T>(this T control, IBinding binding) where T : Ursa.Controls.ImageViewer
   => control._set(Ursa.Controls.ImageViewer.OverlayerProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Overlayer<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ImageViewer
   => control._set(Ursa.Controls.ImageViewer.OverlayerProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Overlayer<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Control> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ImageViewer
=> control._setEx(Ursa.Controls.ImageViewer.OverlayerProperty, ps, () => control.Overlayer = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SourceProperty

/*BindFromExpressionSetterGenerator*/
public static T Source<T>(this T control, Func<Avalonia.Media.IImage> func, Action<Avalonia.Media.IImage>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ImageViewer
   => control._set(Ursa.Controls.ImageViewer.SourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Source<T>(this T control, Avalonia.Media.IImage value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ImageViewer
=> control._setEx(Ursa.Controls.ImageViewer.SourceProperty, ps, () => control.Source = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Source<T>(this T control, IBinding binding) where T : Ursa.Controls.ImageViewer
   => control._set(Ursa.Controls.ImageViewer.SourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Source<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ImageViewer
   => control._set(Ursa.Controls.ImageViewer.SourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Source<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IImage> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ImageViewer
=> control._setEx(Ursa.Controls.ImageViewer.SourceProperty, ps, () => control.Source = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ScaleProperty

/*BindFromExpressionSetterGenerator*/
public static T Scale<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ImageViewer
   => control._set(Ursa.Controls.ImageViewer.ScaleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Scale<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ImageViewer
=> control._setEx(Ursa.Controls.ImageViewer.ScaleProperty, ps, () => control.Scale = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Scale<T>(this T control, IBinding binding) where T : Ursa.Controls.ImageViewer
   => control._set(Ursa.Controls.ImageViewer.ScaleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Scale<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ImageViewer
   => control._set(Ursa.Controls.ImageViewer.ScaleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Scale<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ImageViewer
=> control._setEx(Ursa.Controls.ImageViewer.ScaleProperty, ps, () => control.Scale = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // MinScaleProperty

/*BindFromExpressionSetterGenerator*/
public static T MinScale<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ImageViewer
   => control._set(Ursa.Controls.ImageViewer.MinScaleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T MinScale<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ImageViewer
=> control._setEx(Ursa.Controls.ImageViewer.MinScaleProperty, ps, () => control.MinScale = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T MinScale<T>(this T control, IBinding binding) where T : Ursa.Controls.ImageViewer
   => control._set(Ursa.Controls.ImageViewer.MinScaleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T MinScale<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ImageViewer
   => control._set(Ursa.Controls.ImageViewer.MinScaleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T MinScale<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ImageViewer
=> control._setEx(Ursa.Controls.ImageViewer.MinScaleProperty, ps, () => control.MinScale = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TranslateXProperty

/*BindFromExpressionSetterGenerator*/
public static T TranslateX<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ImageViewer
   => control._set(Ursa.Controls.ImageViewer.TranslateXProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T TranslateX<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ImageViewer
=> control._setEx(Ursa.Controls.ImageViewer.TranslateXProperty, ps, () => control.TranslateX = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TranslateX<T>(this T control, IBinding binding) where T : Ursa.Controls.ImageViewer
   => control._set(Ursa.Controls.ImageViewer.TranslateXProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TranslateX<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ImageViewer
   => control._set(Ursa.Controls.ImageViewer.TranslateXProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T TranslateX<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ImageViewer
=> control._setEx(Ursa.Controls.ImageViewer.TranslateXProperty, ps, () => control.TranslateX = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TranslateYProperty

/*BindFromExpressionSetterGenerator*/
public static T TranslateY<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ImageViewer
   => control._set(Ursa.Controls.ImageViewer.TranslateYProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T TranslateY<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ImageViewer
=> control._setEx(Ursa.Controls.ImageViewer.TranslateYProperty, ps, () => control.TranslateY = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TranslateY<T>(this T control, IBinding binding) where T : Ursa.Controls.ImageViewer
   => control._set(Ursa.Controls.ImageViewer.TranslateYProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TranslateY<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ImageViewer
   => control._set(Ursa.Controls.ImageViewer.TranslateYProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T TranslateY<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ImageViewer
=> control._setEx(Ursa.Controls.ImageViewer.TranslateYProperty, ps, () => control.TranslateY = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SmallChangeProperty

/*BindFromExpressionSetterGenerator*/
public static T SmallChange<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ImageViewer
   => control._set(Ursa.Controls.ImageViewer.SmallChangeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SmallChange<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ImageViewer
=> control._setEx(Ursa.Controls.ImageViewer.SmallChangeProperty, ps, () => control.SmallChange = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SmallChange<T>(this T control, IBinding binding) where T : Ursa.Controls.ImageViewer
   => control._set(Ursa.Controls.ImageViewer.SmallChangeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SmallChange<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ImageViewer
   => control._set(Ursa.Controls.ImageViewer.SmallChangeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SmallChange<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ImageViewer
=> control._setEx(Ursa.Controls.ImageViewer.SmallChangeProperty, ps, () => control.SmallChange = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // LargeChangeProperty

/*BindFromExpressionSetterGenerator*/
public static T LargeChange<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ImageViewer
   => control._set(Ursa.Controls.ImageViewer.LargeChangeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T LargeChange<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ImageViewer
=> control._setEx(Ursa.Controls.ImageViewer.LargeChangeProperty, ps, () => control.LargeChange = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T LargeChange<T>(this T control, IBinding binding) where T : Ursa.Controls.ImageViewer
   => control._set(Ursa.Controls.ImageViewer.LargeChangeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T LargeChange<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ImageViewer
   => control._set(Ursa.Controls.ImageViewer.LargeChangeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T LargeChange<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ImageViewer
=> control._setEx(Ursa.Controls.ImageViewer.LargeChangeProperty, ps, () => control.LargeChange = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // StretchProperty

/*BindFromExpressionSetterGenerator*/
public static T Stretch<T>(this T control, Func<Avalonia.Media.Stretch> func, Action<Avalonia.Media.Stretch>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ImageViewer
   => control._set(Ursa.Controls.ImageViewer.StretchProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Stretch<T>(this T control, Avalonia.Media.Stretch value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ImageViewer
=> control._setEx(Ursa.Controls.ImageViewer.StretchProperty, ps, () => control.Stretch = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Stretch<T>(this T control, IBinding binding) where T : Ursa.Controls.ImageViewer
   => control._set(Ursa.Controls.ImageViewer.StretchProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Stretch<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ImageViewer
   => control._set(Ursa.Controls.ImageViewer.StretchProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Stretch<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.Stretch> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ImageViewer
=> control._setEx(Ursa.Controls.ImageViewer.StretchProperty, ps, () => control.Stretch = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // OverlayerProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Overlayer<T>(this Style<T> style, Avalonia.Controls.Control value) where T : Ursa.Controls.ImageViewer
=> style._addSetter(Ursa.Controls.ImageViewer.OverlayerProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Overlayer<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ImageViewer
=> style._addSetter(Ursa.Controls.ImageViewer.OverlayerProperty, binding);


 // SourceProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Source<T>(this Style<T> style, Avalonia.Media.IImage value) where T : Ursa.Controls.ImageViewer
=> style._addSetter(Ursa.Controls.ImageViewer.SourceProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Source<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ImageViewer
=> style._addSetter(Ursa.Controls.ImageViewer.SourceProperty, binding);


 // SmallChangeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> SmallChange<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.ImageViewer
=> style._addSetter(Ursa.Controls.ImageViewer.SmallChangeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SmallChange<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ImageViewer
=> style._addSetter(Ursa.Controls.ImageViewer.SmallChangeProperty, binding);


 // LargeChangeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> LargeChange<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.ImageViewer
=> style._addSetter(Ursa.Controls.ImageViewer.LargeChangeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> LargeChange<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ImageViewer
=> style._addSetter(Ursa.Controls.ImageViewer.LargeChangeProperty, binding);


 // StretchProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Stretch<T>(this Style<T> style, Avalonia.Media.Stretch value) where T : Ursa.Controls.ImageViewer
=> style._addSetter(Ursa.Controls.ImageViewer.StretchProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Stretch<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ImageViewer
=> style._addSetter(Ursa.Controls.ImageViewer.StretchProperty, binding);



}
