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
public static partial class ColorPickerButton_MarkupExtensions
{
//================= Properties ======================//
 // Color

/*BindFromExpressionSetterGenerator*/
public static T Color<T>(this T control, Func<System.Nullable<Avalonia.Media.Color>> func, Action<System.Nullable<Avalonia.Media.Color>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.ColorProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Color<T>(this T control,System.Nullable<Avalonia.Media.Color> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.ColorProperty, ps, () => control.Color = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Color<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.ColorProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Color<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.ColorProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Color<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Nullable<Avalonia.Media.Color>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.ColorProperty, ps, () => control.Color = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsMoreButtonVisible

/*BindFromExpressionSetterGenerator*/
public static T IsMoreButtonVisible<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.IsMoreButtonVisibleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsMoreButtonVisible<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.IsMoreButtonVisibleProperty, ps, () => control.IsMoreButtonVisible = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsMoreButtonVisible<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.IsMoreButtonVisibleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsMoreButtonVisible<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.IsMoreButtonVisibleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsMoreButtonVisible<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.IsMoreButtonVisibleProperty, ps, () => control.IsMoreButtonVisible = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsCompact

/*BindFromExpressionSetterGenerator*/
public static T IsCompact<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.IsCompactProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsCompact<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.IsCompactProperty, ps, () => control.IsCompact = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsCompact<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.IsCompactProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsCompact<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.IsCompactProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsCompact<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.IsCompactProperty, ps, () => control.IsCompact = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsAlphaEnabled

/*BindFromExpressionSetterGenerator*/
public static T IsAlphaEnabled<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.IsAlphaEnabledProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsAlphaEnabled<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.IsAlphaEnabledProperty, ps, () => control.IsAlphaEnabled = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsAlphaEnabled<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.IsAlphaEnabledProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsAlphaEnabled<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.IsAlphaEnabledProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsAlphaEnabled<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.IsAlphaEnabledProperty, ps, () => control.IsAlphaEnabled = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // UseSpectrum

/*BindFromExpressionSetterGenerator*/
public static T UseSpectrum<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.UseSpectrumProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T UseSpectrum<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.UseSpectrumProperty, ps, () => control.UseSpectrum = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T UseSpectrum<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.UseSpectrumProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T UseSpectrum<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.UseSpectrumProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T UseSpectrum<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.UseSpectrumProperty, ps, () => control.UseSpectrum = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // UseColorWheel

/*BindFromExpressionSetterGenerator*/
public static T UseColorWheel<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorWheelProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T UseColorWheel<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorWheelProperty, ps, () => control.UseColorWheel = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T UseColorWheel<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorWheelProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T UseColorWheel<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorWheelProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T UseColorWheel<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorWheelProperty, ps, () => control.UseColorWheel = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // UseColorTriangle

/*BindFromExpressionSetterGenerator*/
public static T UseColorTriangle<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorTriangleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T UseColorTriangle<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorTriangleProperty, ps, () => control.UseColorTriangle = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T UseColorTriangle<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorTriangleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T UseColorTriangle<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorTriangleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T UseColorTriangle<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorTriangleProperty, ps, () => control.UseColorTriangle = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // UseColorPalette

/*BindFromExpressionSetterGenerator*/
public static T UseColorPalette<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorPaletteProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T UseColorPalette<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorPaletteProperty, ps, () => control.UseColorPalette = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T UseColorPalette<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorPaletteProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T UseColorPalette<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorPaletteProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T UseColorPalette<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorPaletteProperty, ps, () => control.UseColorPalette = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CustomPaletteColors

/*BindFromExpressionSetterGenerator*/
public static T CustomPaletteColors<T>(this T control, Func<System.Collections.Generic.IEnumerable<Avalonia.Media.Color>> func, Action<System.Collections.Generic.IEnumerable<Avalonia.Media.Color>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.CustomPaletteColorsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CustomPaletteColors<T>(this T control,System.Collections.Generic.IEnumerable<Avalonia.Media.Color> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.CustomPaletteColorsProperty, ps, () => control.CustomPaletteColors = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CustomPaletteColors<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.CustomPaletteColorsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CustomPaletteColors<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.CustomPaletteColorsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CustomPaletteColors<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.Generic.IEnumerable<Avalonia.Media.Color>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.CustomPaletteColorsProperty, ps, () => control.CustomPaletteColors = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // PaletteColumnCount

/*BindFromExpressionSetterGenerator*/
public static T PaletteColumnCount<T>(this T control, Func<System.Int32> func, Action<System.Int32>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.PaletteColumnCountProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T PaletteColumnCount<T>(this T control,System.Int32 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.PaletteColumnCountProperty, ps, () => control.PaletteColumnCount = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T PaletteColumnCount<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.PaletteColumnCountProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T PaletteColumnCount<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.PaletteColumnCountProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T PaletteColumnCount<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Int32> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.PaletteColumnCountProperty, ps, () => control.PaletteColumnCount = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ShowAcceptDismissButtons

/*BindFromExpressionSetterGenerator*/
public static T ShowAcceptDismissButtons<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.ShowAcceptDismissButtonsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ShowAcceptDismissButtons<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.ShowAcceptDismissButtonsProperty, ps, () => control.ShowAcceptDismissButtons = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ShowAcceptDismissButtons<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.ShowAcceptDismissButtonsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ShowAcceptDismissButtons<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.ShowAcceptDismissButtonsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ShowAcceptDismissButtons<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.ShowAcceptDismissButtonsProperty, ps, () => control.ShowAcceptDismissButtons = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // FlyoutPlacement

/*BindFromExpressionSetterGenerator*/
public static T FlyoutPlacement<T>(this T control, Func<Avalonia.Controls.PlacementMode> func, Action<Avalonia.Controls.PlacementMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.FlyoutPlacementProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T FlyoutPlacement<T>(this T control,Avalonia.Controls.PlacementMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.FlyoutPlacementProperty, ps, () => control.FlyoutPlacement = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T FlyoutPlacement<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.FlyoutPlacementProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T FlyoutPlacement<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.FlyoutPlacementProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T FlyoutPlacement<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.PlacementMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.FlyoutPlacementProperty, ps, () => control.FlyoutPlacement = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // FlyoutConfirmed

/*ActionToEventGenerator*/
public static T OnFlyoutConfirmed<T>(this T control, Action<FluentAvalonia.UI.Controls.ColorPickerButton, FluentAvalonia.UI.Controls.ColorButtonColorChangedEventArgs> action) where T : FluentAvalonia.UI.Controls.ColorPickerButton  => 
 control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.ColorPickerButton,FluentAvalonia.UI.Controls.ColorButtonColorChangedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.FlyoutConfirmed += h);


 // FlyoutDismissed

/*ActionToEventGenerator*/
public static T OnFlyoutDismissed<T>(this T control, Action<FluentAvalonia.UI.Controls.ColorPickerButton, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.ColorPickerButton  => 
 control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.ColorPickerButton,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.FlyoutDismissed += h);


 // FlyoutOpened

/*ActionToEventGenerator*/
public static T OnFlyoutOpened<T>(this T control, Action<FluentAvalonia.UI.Controls.ColorPickerButton, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.ColorPickerButton  => 
 control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.ColorPickerButton,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.FlyoutOpened += h);


 // FlyoutClosed

/*ActionToEventGenerator*/
public static T OnFlyoutClosed<T>(this T control, Action<FluentAvalonia.UI.Controls.ColorPickerButton, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.ColorPickerButton  => 
 control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.ColorPickerButton,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.FlyoutClosed += h);


 // ColorChanged

/*ActionToEventGenerator*/
public static T OnColorChanged<T>(this T control, Action<FluentAvalonia.UI.Controls.ColorPickerButton, FluentAvalonia.UI.Controls.ColorButtonColorChangedEventArgs> action) where T : FluentAvalonia.UI.Controls.ColorPickerButton  => 
 control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.ColorPickerButton,FluentAvalonia.UI.Controls.ColorButtonColorChangedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.ColorChanged += h);



//================= Styles ======================//
 // Color

/*ValueStyleSetterGenerator*/
public static Style<T> Color<T>(this Style<T> style, System.Nullable<Avalonia.Media.Color> value) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.ColorProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Color<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.ColorProperty, binding);


 // IsMoreButtonVisible

/*ValueStyleSetterGenerator*/
public static Style<T> IsMoreButtonVisible<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.IsMoreButtonVisibleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsMoreButtonVisible<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.IsMoreButtonVisibleProperty, binding);


 // IsCompact

/*ValueStyleSetterGenerator*/
public static Style<T> IsCompact<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.IsCompactProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsCompact<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.IsCompactProperty, binding);


 // IsAlphaEnabled

/*ValueStyleSetterGenerator*/
public static Style<T> IsAlphaEnabled<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.IsAlphaEnabledProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsAlphaEnabled<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.IsAlphaEnabledProperty, binding);


 // UseSpectrum

/*ValueStyleSetterGenerator*/
public static Style<T> UseSpectrum<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.UseSpectrumProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> UseSpectrum<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.UseSpectrumProperty, binding);


 // UseColorWheel

/*ValueStyleSetterGenerator*/
public static Style<T> UseColorWheel<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorWheelProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> UseColorWheel<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorWheelProperty, binding);


 // UseColorTriangle

/*ValueStyleSetterGenerator*/
public static Style<T> UseColorTriangle<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorTriangleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> UseColorTriangle<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorTriangleProperty, binding);


 // UseColorPalette

/*ValueStyleSetterGenerator*/
public static Style<T> UseColorPalette<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorPaletteProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> UseColorPalette<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorPaletteProperty, binding);


 // PaletteColumnCount

/*ValueStyleSetterGenerator*/
public static Style<T> PaletteColumnCount<T>(this Style<T> style, System.Int32 value) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.PaletteColumnCountProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> PaletteColumnCount<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.PaletteColumnCountProperty, binding);


 // ShowAcceptDismissButtons

/*ValueStyleSetterGenerator*/
public static Style<T> ShowAcceptDismissButtons<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.ShowAcceptDismissButtonsProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ShowAcceptDismissButtons<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.ShowAcceptDismissButtonsProperty, binding);


 // FlyoutPlacement

/*ValueStyleSetterGenerator*/
public static Style<T> FlyoutPlacement<T>(this Style<T> style, Avalonia.Controls.PlacementMode value) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.FlyoutPlacementProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> FlyoutPlacement<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.FlyoutPlacementProperty, binding);



}
