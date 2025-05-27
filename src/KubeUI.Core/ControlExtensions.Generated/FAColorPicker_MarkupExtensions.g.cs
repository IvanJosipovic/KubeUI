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
public static partial class FAColorPicker_MarkupExtensions
{
//================= Properties ======================//
 // PreviousColor

/*BindFromExpressionSetterGenerator*/
public static T PreviousColor<T>(this T control, Func<FluentAvalonia.UI.Media.Color2> func, Action<FluentAvalonia.UI.Media.Color2>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.PreviousColorProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T PreviousColor<T>(this T control,FluentAvalonia.UI.Media.Color2 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.PreviousColorProperty, ps, () => control.PreviousColor = value, bindingMode, converter, bindingSource);

/*ValueOverloadsSetterGenerator*/

public static T PreviousColor<T>(this T control, System.Byte r = default, System.Byte g = default, System.Byte b = default, System.Byte a = default) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(() => control.PreviousColor = new FluentAvalonia.UI.Media.Color2(r, g, b, a));
public static T PreviousColor<T>(this T control, Avalonia.Media.Color avColor = default) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(() => control.PreviousColor = new FluentAvalonia.UI.Media.Color2(avColor));

/*BindSetterGenerator*/
public static T PreviousColor<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.PreviousColorProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T PreviousColor<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.PreviousColorProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T PreviousColor<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Media.Color2> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.PreviousColorProperty, ps, () => control.PreviousColor = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // Color

/*BindFromExpressionSetterGenerator*/
public static T Color<T>(this T control, Func<FluentAvalonia.UI.Media.Color2> func, Action<FluentAvalonia.UI.Media.Color2>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.ColorProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Color<T>(this T control,FluentAvalonia.UI.Media.Color2 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.ColorProperty, ps, () => control.Color = value, bindingMode, converter, bindingSource);

/*ValueOverloadsSetterGenerator*/

public static T Color<T>(this T control, System.Byte r = default, System.Byte g = default, System.Byte b = default, System.Byte a = default) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(() => control.Color = new FluentAvalonia.UI.Media.Color2(r, g, b, a));
public static T Color<T>(this T control, Avalonia.Media.Color avColor = default) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(() => control.Color = new FluentAvalonia.UI.Media.Color2(avColor));

/*BindSetterGenerator*/
public static T Color<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.ColorProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Color<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.ColorProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Color<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Media.Color2> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.ColorProperty, ps, () => control.Color = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ColorTextType

/*BindFromExpressionSetterGenerator*/
public static T ColorTextType<T>(this T control, Func<FluentAvalonia.UI.Controls.ColorTextType> func, Action<FluentAvalonia.UI.Controls.ColorTextType>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.ColorTextTypeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ColorTextType<T>(this T control,FluentAvalonia.UI.Controls.ColorTextType value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.ColorTextTypeProperty, ps, () => control.ColorTextType = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ColorTextType<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.ColorTextTypeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ColorTextType<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.ColorTextTypeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ColorTextType<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.ColorTextType> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.ColorTextTypeProperty, ps, () => control.ColorTextType = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // Component

/*BindFromExpressionSetterGenerator*/
public static T Component<T>(this T control, Func<FluentAvalonia.UI.Controls.ColorSpectrumComponents> func, Action<FluentAvalonia.UI.Controls.ColorSpectrumComponents>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.ComponentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Component<T>(this T control,FluentAvalonia.UI.Controls.ColorSpectrumComponents value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.ComponentProperty, ps, () => control.Component = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Component<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.ComponentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Component<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.ComponentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Component<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.ColorSpectrumComponents> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.ComponentProperty, ps, () => control.Component = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsMoreButtonVisible

/*BindFromExpressionSetterGenerator*/
public static T IsMoreButtonVisible<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.IsMoreButtonVisibleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsMoreButtonVisible<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.IsMoreButtonVisibleProperty, ps, () => control.IsMoreButtonVisible = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsMoreButtonVisible<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.IsMoreButtonVisibleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsMoreButtonVisible<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.IsMoreButtonVisibleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsMoreButtonVisible<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.IsMoreButtonVisibleProperty, ps, () => control.IsMoreButtonVisible = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsCompact

/*BindFromExpressionSetterGenerator*/
public static T IsCompact<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.IsCompactProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsCompact<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.IsCompactProperty, ps, () => control.IsCompact = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsCompact<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.IsCompactProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsCompact<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.IsCompactProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsCompact<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.IsCompactProperty, ps, () => control.IsCompact = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsAlphaEnabled

/*BindFromExpressionSetterGenerator*/
public static T IsAlphaEnabled<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.IsAlphaEnabledProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsAlphaEnabled<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.IsAlphaEnabledProperty, ps, () => control.IsAlphaEnabled = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsAlphaEnabled<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.IsAlphaEnabledProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsAlphaEnabled<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.IsAlphaEnabledProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsAlphaEnabled<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.IsAlphaEnabledProperty, ps, () => control.IsAlphaEnabled = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // UseSpectrum

/*BindFromExpressionSetterGenerator*/
public static T UseSpectrum<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.UseSpectrumProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T UseSpectrum<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.UseSpectrumProperty, ps, () => control.UseSpectrum = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T UseSpectrum<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.UseSpectrumProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T UseSpectrum<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.UseSpectrumProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T UseSpectrum<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.UseSpectrumProperty, ps, () => control.UseSpectrum = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // UseColorWheel

/*BindFromExpressionSetterGenerator*/
public static T UseColorWheel<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.UseColorWheelProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T UseColorWheel<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.UseColorWheelProperty, ps, () => control.UseColorWheel = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T UseColorWheel<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.UseColorWheelProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T UseColorWheel<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.UseColorWheelProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T UseColorWheel<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.UseColorWheelProperty, ps, () => control.UseColorWheel = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // UseColorTriangle

/*BindFromExpressionSetterGenerator*/
public static T UseColorTriangle<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.UseColorTriangleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T UseColorTriangle<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.UseColorTriangleProperty, ps, () => control.UseColorTriangle = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T UseColorTriangle<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.UseColorTriangleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T UseColorTriangle<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.UseColorTriangleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T UseColorTriangle<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.UseColorTriangleProperty, ps, () => control.UseColorTriangle = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // UseColorPalette

/*BindFromExpressionSetterGenerator*/
public static T UseColorPalette<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.UseColorPaletteProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T UseColorPalette<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.UseColorPaletteProperty, ps, () => control.UseColorPalette = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T UseColorPalette<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.UseColorPaletteProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T UseColorPalette<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.UseColorPaletteProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T UseColorPalette<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.UseColorPaletteProperty, ps, () => control.UseColorPalette = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CustomPaletteColors

/*BindFromExpressionSetterGenerator*/
public static T CustomPaletteColors<T>(this T control, Func<System.Collections.Generic.IEnumerable<Avalonia.Media.Color>> func, Action<System.Collections.Generic.IEnumerable<Avalonia.Media.Color>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.CustomPaletteColorsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CustomPaletteColors<T>(this T control,System.Collections.Generic.IEnumerable<Avalonia.Media.Color> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.CustomPaletteColorsProperty, ps, () => control.CustomPaletteColors = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CustomPaletteColors<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.CustomPaletteColorsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CustomPaletteColors<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.CustomPaletteColorsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CustomPaletteColors<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.Generic.IEnumerable<Avalonia.Media.Color>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.CustomPaletteColorsProperty, ps, () => control.CustomPaletteColors = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // PaletteColumnCount

/*BindFromExpressionSetterGenerator*/
public static T PaletteColumnCount<T>(this T control, Func<System.Int32> func, Action<System.Int32>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.PaletteColumnCountProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T PaletteColumnCount<T>(this T control,System.Int32 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.PaletteColumnCountProperty, ps, () => control.PaletteColumnCount = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T PaletteColumnCount<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.PaletteColumnCountProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T PaletteColumnCount<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.PaletteColumnCountProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T PaletteColumnCount<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Int32> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.PaletteColumnCountProperty, ps, () => control.PaletteColumnCount = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // ColorChanged

/*ActionToEventGenerator*/
public static T OnColorChanged<T>(this T control, Action<FluentAvalonia.UI.Controls.FAColorPicker, FluentAvalonia.UI.Controls.ColorChangedEventArgs> action) where T : FluentAvalonia.UI.Controls.FAColorPicker  => 
 control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.FAColorPicker,FluentAvalonia.UI.Controls.ColorChangedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.ColorChanged += h);



//================= Styles ======================//
 // PreviousColor

/*ValueStyleSetterGenerator*/
public static Style<T> PreviousColor<T>(this Style<T> style, FluentAvalonia.UI.Media.Color2 value) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.PreviousColorProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> PreviousColor<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.PreviousColorProperty, binding);

/*ValueOverloadsStyleSetterGenerator*/
public static Style<T> PreviousColor<T>(this Style<T> style, System.Byte r, System.Byte g, System.Byte b, System.Byte a) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.PreviousColorProperty, new FluentAvalonia.UI.Media.Color2(r, g, b, a));public static Style<T> PreviousColor<T>(this Style<T> style, Avalonia.Media.Color avColor) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.PreviousColorProperty, new FluentAvalonia.UI.Media.Color2(avColor));


 // Color

/*ValueStyleSetterGenerator*/
public static Style<T> Color<T>(this Style<T> style, FluentAvalonia.UI.Media.Color2 value) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.ColorProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Color<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.ColorProperty, binding);

/*ValueOverloadsStyleSetterGenerator*/
public static Style<T> Color<T>(this Style<T> style, System.Byte r, System.Byte g, System.Byte b, System.Byte a) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.ColorProperty, new FluentAvalonia.UI.Media.Color2(r, g, b, a));public static Style<T> Color<T>(this Style<T> style, Avalonia.Media.Color avColor) where T : FluentAvalonia.UI.Controls.FAColorPicker 
   => style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.ColorProperty, new FluentAvalonia.UI.Media.Color2(avColor));


 // ColorTextType

/*ValueStyleSetterGenerator*/
public static Style<T> ColorTextType<T>(this Style<T> style, FluentAvalonia.UI.Controls.ColorTextType value) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.ColorTextTypeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ColorTextType<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.ColorTextTypeProperty, binding);


 // Component

/*ValueStyleSetterGenerator*/
public static Style<T> Component<T>(this Style<T> style, FluentAvalonia.UI.Controls.ColorSpectrumComponents value) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.ComponentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Component<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.ComponentProperty, binding);


 // IsMoreButtonVisible

/*ValueStyleSetterGenerator*/
public static Style<T> IsMoreButtonVisible<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.IsMoreButtonVisibleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsMoreButtonVisible<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.IsMoreButtonVisibleProperty, binding);


 // IsCompact

/*ValueStyleSetterGenerator*/
public static Style<T> IsCompact<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.IsCompactProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsCompact<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.IsCompactProperty, binding);


 // IsAlphaEnabled

/*ValueStyleSetterGenerator*/
public static Style<T> IsAlphaEnabled<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.IsAlphaEnabledProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsAlphaEnabled<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.IsAlphaEnabledProperty, binding);


 // UseSpectrum

/*ValueStyleSetterGenerator*/
public static Style<T> UseSpectrum<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.UseSpectrumProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> UseSpectrum<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.UseSpectrumProperty, binding);


 // UseColorWheel

/*ValueStyleSetterGenerator*/
public static Style<T> UseColorWheel<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.UseColorWheelProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> UseColorWheel<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.UseColorWheelProperty, binding);


 // UseColorTriangle

/*ValueStyleSetterGenerator*/
public static Style<T> UseColorTriangle<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.UseColorTriangleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> UseColorTriangle<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.UseColorTriangleProperty, binding);


 // UseColorPalette

/*ValueStyleSetterGenerator*/
public static Style<T> UseColorPalette<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.UseColorPaletteProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> UseColorPalette<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.UseColorPaletteProperty, binding);


 // PaletteColumnCount

/*ValueStyleSetterGenerator*/
public static Style<T> PaletteColumnCount<T>(this Style<T> style, System.Int32 value) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.PaletteColumnCountProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> PaletteColumnCount<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker 
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.PaletteColumnCountProperty, binding);



}
