#nullable enable
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using ColorPickerButton = FluentAvalonia.UI.Controls.ColorPickerButton;
using FluentAvalonia.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class ColorPickerButtonExtensions
{
public static T Color<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.ColorProperty, binding);
public static T Color<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.ColorProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Color<T>(this T control, Func<System.Nullable<Avalonia.Media.Color>> func, Action<System.Nullable<Avalonia.Media.Color>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.ColorProperty, func, onChanged, expression);
public static T Color<T>(this T control, System.Nullable<Avalonia.Media.Color> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.ColorProperty, ps, () => control.Color = value, bindingMode, converter, bindingSource);
public static T Color<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Nullable<Avalonia.Media.Color>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.ColorProperty, ps, () => control.Color = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsMoreButtonVisible<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.IsMoreButtonVisibleProperty, binding);
public static T IsMoreButtonVisible<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.IsMoreButtonVisibleProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsMoreButtonVisible<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.IsMoreButtonVisibleProperty, func, onChanged, expression);
public static T IsMoreButtonVisible<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.IsMoreButtonVisibleProperty, ps, () => control.IsMoreButtonVisible = value, bindingMode, converter, bindingSource);
public static T IsMoreButtonVisible<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.IsMoreButtonVisibleProperty, ps, () => control.IsMoreButtonVisible = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsCompact<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.IsCompactProperty, binding);
public static T IsCompact<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.IsCompactProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsCompact<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.IsCompactProperty, func, onChanged, expression);
public static T IsCompact<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.IsCompactProperty, ps, () => control.IsCompact = value, bindingMode, converter, bindingSource);
public static T IsCompact<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.IsCompactProperty, ps, () => control.IsCompact = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsAlphaEnabled<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.IsAlphaEnabledProperty, binding);
public static T IsAlphaEnabled<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.IsAlphaEnabledProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsAlphaEnabled<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.IsAlphaEnabledProperty, func, onChanged, expression);
public static T IsAlphaEnabled<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.IsAlphaEnabledProperty, ps, () => control.IsAlphaEnabled = value, bindingMode, converter, bindingSource);
public static T IsAlphaEnabled<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.IsAlphaEnabledProperty, ps, () => control.IsAlphaEnabled = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T UseSpectrum<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.UseSpectrumProperty, binding);
public static T UseSpectrum<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.UseSpectrumProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T UseSpectrum<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.UseSpectrumProperty, func, onChanged, expression);
public static T UseSpectrum<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.UseSpectrumProperty, ps, () => control.UseSpectrum = value, bindingMode, converter, bindingSource);
public static T UseSpectrum<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.UseSpectrumProperty, ps, () => control.UseSpectrum = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T UseColorWheel<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorWheelProperty, binding);
public static T UseColorWheel<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorWheelProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T UseColorWheel<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorWheelProperty, func, onChanged, expression);
public static T UseColorWheel<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorWheelProperty, ps, () => control.UseColorWheel = value, bindingMode, converter, bindingSource);
public static T UseColorWheel<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorWheelProperty, ps, () => control.UseColorWheel = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T UseColorTriangle<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorTriangleProperty, binding);
public static T UseColorTriangle<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorTriangleProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T UseColorTriangle<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorTriangleProperty, func, onChanged, expression);
public static T UseColorTriangle<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorTriangleProperty, ps, () => control.UseColorTriangle = value, bindingMode, converter, bindingSource);
public static T UseColorTriangle<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorTriangleProperty, ps, () => control.UseColorTriangle = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T UseColorPalette<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorPaletteProperty, binding);
public static T UseColorPalette<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorPaletteProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T UseColorPalette<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorPaletteProperty, func, onChanged, expression);
public static T UseColorPalette<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorPaletteProperty, ps, () => control.UseColorPalette = value, bindingMode, converter, bindingSource);
public static T UseColorPalette<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorPaletteProperty, ps, () => control.UseColorPalette = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T CustomPaletteColors<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.CustomPaletteColorsProperty, binding);
public static T CustomPaletteColors<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.CustomPaletteColorsProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CustomPaletteColors<T>(this T control, Func<System.Collections.Generic.IEnumerable<Avalonia.Media.Color>> func, Action<System.Collections.Generic.IEnumerable<Avalonia.Media.Color>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.CustomPaletteColorsProperty, func, onChanged, expression);
public static T CustomPaletteColors<T>(this T control, System.Collections.Generic.IEnumerable<Avalonia.Media.Color> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.CustomPaletteColorsProperty, ps, () => control.CustomPaletteColors = value, bindingMode, converter, bindingSource);
public static T CustomPaletteColors<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.Generic.IEnumerable<Avalonia.Media.Color>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.CustomPaletteColorsProperty, ps, () => control.CustomPaletteColors = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T PaletteColumnCount<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.PaletteColumnCountProperty, binding);
public static T PaletteColumnCount<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.PaletteColumnCountProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T PaletteColumnCount<T>(this T control, Func<System.Int32> func, Action<System.Int32>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.PaletteColumnCountProperty, func, onChanged, expression);
public static T PaletteColumnCount<T>(this T control, System.Int32 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.PaletteColumnCountProperty, ps, () => control.PaletteColumnCount = value, bindingMode, converter, bindingSource);
public static T PaletteColumnCount<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Int32> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.PaletteColumnCountProperty, ps, () => control.PaletteColumnCount = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ShowAcceptDismissButtons<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.ShowAcceptDismissButtonsProperty, binding);
public static T ShowAcceptDismissButtons<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.ShowAcceptDismissButtonsProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ShowAcceptDismissButtons<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.ShowAcceptDismissButtonsProperty, func, onChanged, expression);
public static T ShowAcceptDismissButtons<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.ShowAcceptDismissButtonsProperty, ps, () => control.ShowAcceptDismissButtons = value, bindingMode, converter, bindingSource);
public static T ShowAcceptDismissButtons<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.ShowAcceptDismissButtonsProperty, ps, () => control.ShowAcceptDismissButtons = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T FlyoutPlacement<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.FlyoutPlacementProperty, binding);
public static T FlyoutPlacement<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.FlyoutPlacementProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T FlyoutPlacement<T>(this T control, Func<Avalonia.Controls.PlacementMode> func, Action<Avalonia.Controls.PlacementMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
   => control._set(FluentAvalonia.UI.Controls.ColorPickerButton.FlyoutPlacementProperty, func, onChanged, expression);
public static T FlyoutPlacement<T>(this T control, Avalonia.Controls.PlacementMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.FlyoutPlacementProperty, ps, () => control.FlyoutPlacement = value, bindingMode, converter, bindingSource);
public static T FlyoutPlacement<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.PlacementMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerButton.FlyoutPlacementProperty, ps, () => control.FlyoutPlacement = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

