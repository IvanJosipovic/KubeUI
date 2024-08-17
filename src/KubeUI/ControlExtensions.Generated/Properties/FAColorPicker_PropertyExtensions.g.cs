#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using FAColorPicker = FluentAvalonia.UI.Controls.FAColorPicker;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class FAColorPickerExtensions
{
public static T PreviousColor<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.PreviousColorProperty, binding);
public static T PreviousColor<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.PreviousColorProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T PreviousColor<T>(this T control, Func<FluentAvalonia.UI.Media.Color2> func, Action<FluentAvalonia.UI.Media.Color2>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.PreviousColorProperty, func, onChanged, expression);
public static T PreviousColor<T>(this T control, FluentAvalonia.UI.Media.Color2 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.PreviousColorProperty, ps, () => control.PreviousColor = value, bindingMode, converter, bindingSource);
public static T PreviousColor<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Media.Color2> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.PreviousColorProperty, ps, () => control.PreviousColor = converter.TryConvert(value), bindingMode, converter, bindingSource);

public static T PreviousColor<T>(this T control, Byte r = default, Byte g = default, Byte b = default, Byte a = default) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(() => control.PreviousColor = new FluentAvalonia.UI.Media.Color2(r, g, b, a));
public static T PreviousColor<T>(this T control, Color avColor = default) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(() => control.PreviousColor = new FluentAvalonia.UI.Media.Color2(avColor));
public static T Color<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.ColorProperty, binding);
public static T Color<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.ColorProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Color<T>(this T control, Func<FluentAvalonia.UI.Media.Color2> func, Action<FluentAvalonia.UI.Media.Color2>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.ColorProperty, func, onChanged, expression);
public static T Color<T>(this T control, FluentAvalonia.UI.Media.Color2 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.ColorProperty, ps, () => control.Color = value, bindingMode, converter, bindingSource);
public static T Color<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Media.Color2> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.ColorProperty, ps, () => control.Color = converter.TryConvert(value), bindingMode, converter, bindingSource);

public static T Color<T>(this T control, Byte r = default, Byte g = default, Byte b = default, Byte a = default) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(() => control.Color = new FluentAvalonia.UI.Media.Color2(r, g, b, a));
public static T Color<T>(this T control, Color avColor = default) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(() => control.Color = new FluentAvalonia.UI.Media.Color2(avColor));
public static T ColorTextType<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.ColorTextTypeProperty, binding);
public static T ColorTextType<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.ColorTextTypeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ColorTextType<T>(this T control, Func<FluentAvalonia.UI.Controls.ColorTextType> func, Action<FluentAvalonia.UI.Controls.ColorTextType>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.ColorTextTypeProperty, func, onChanged, expression);
public static T ColorTextType<T>(this T control, FluentAvalonia.UI.Controls.ColorTextType value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.ColorTextTypeProperty, ps, () => control.ColorTextType = value, bindingMode, converter, bindingSource);
public static T ColorTextType<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.ColorTextType> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.ColorTextTypeProperty, ps, () => control.ColorTextType = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Component<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.ComponentProperty, binding);
public static T Component<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.ComponentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Component<T>(this T control, Func<FluentAvalonia.UI.Controls.ColorSpectrumComponents> func, Action<FluentAvalonia.UI.Controls.ColorSpectrumComponents>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.ComponentProperty, func, onChanged, expression);
public static T Component<T>(this T control, FluentAvalonia.UI.Controls.ColorSpectrumComponents value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.ComponentProperty, ps, () => control.Component = value, bindingMode, converter, bindingSource);
public static T Component<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.ColorSpectrumComponents> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.ComponentProperty, ps, () => control.Component = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsMoreButtonVisible<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.IsMoreButtonVisibleProperty, binding);
public static T IsMoreButtonVisible<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.IsMoreButtonVisibleProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsMoreButtonVisible<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.IsMoreButtonVisibleProperty, func, onChanged, expression);
public static T IsMoreButtonVisible<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.IsMoreButtonVisibleProperty, ps, () => control.IsMoreButtonVisible = value, bindingMode, converter, bindingSource);
public static T IsMoreButtonVisible<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.IsMoreButtonVisibleProperty, ps, () => control.IsMoreButtonVisible = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsCompact<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.IsCompactProperty, binding);
public static T IsCompact<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.IsCompactProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsCompact<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.IsCompactProperty, func, onChanged, expression);
public static T IsCompact<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.IsCompactProperty, ps, () => control.IsCompact = value, bindingMode, converter, bindingSource);
public static T IsCompact<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.IsCompactProperty, ps, () => control.IsCompact = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsAlphaEnabled<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.IsAlphaEnabledProperty, binding);
public static T IsAlphaEnabled<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.IsAlphaEnabledProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsAlphaEnabled<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.IsAlphaEnabledProperty, func, onChanged, expression);
public static T IsAlphaEnabled<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.IsAlphaEnabledProperty, ps, () => control.IsAlphaEnabled = value, bindingMode, converter, bindingSource);
public static T IsAlphaEnabled<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.IsAlphaEnabledProperty, ps, () => control.IsAlphaEnabled = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T UseSpectrum<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.UseSpectrumProperty, binding);
public static T UseSpectrum<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.UseSpectrumProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T UseSpectrum<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.UseSpectrumProperty, func, onChanged, expression);
public static T UseSpectrum<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.UseSpectrumProperty, ps, () => control.UseSpectrum = value, bindingMode, converter, bindingSource);
public static T UseSpectrum<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.UseSpectrumProperty, ps, () => control.UseSpectrum = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T UseColorWheel<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.UseColorWheelProperty, binding);
public static T UseColorWheel<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.UseColorWheelProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T UseColorWheel<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.UseColorWheelProperty, func, onChanged, expression);
public static T UseColorWheel<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.UseColorWheelProperty, ps, () => control.UseColorWheel = value, bindingMode, converter, bindingSource);
public static T UseColorWheel<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.UseColorWheelProperty, ps, () => control.UseColorWheel = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T UseColorTriangle<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.UseColorTriangleProperty, binding);
public static T UseColorTriangle<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.UseColorTriangleProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T UseColorTriangle<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.UseColorTriangleProperty, func, onChanged, expression);
public static T UseColorTriangle<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.UseColorTriangleProperty, ps, () => control.UseColorTriangle = value, bindingMode, converter, bindingSource);
public static T UseColorTriangle<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.UseColorTriangleProperty, ps, () => control.UseColorTriangle = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T UseColorPalette<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.UseColorPaletteProperty, binding);
public static T UseColorPalette<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.UseColorPaletteProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T UseColorPalette<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.UseColorPaletteProperty, func, onChanged, expression);
public static T UseColorPalette<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.UseColorPaletteProperty, ps, () => control.UseColorPalette = value, bindingMode, converter, bindingSource);
public static T UseColorPalette<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.UseColorPaletteProperty, ps, () => control.UseColorPalette = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T CustomPaletteColors<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.CustomPaletteColorsProperty, binding);
public static T CustomPaletteColors<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.CustomPaletteColorsProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CustomPaletteColors<T>(this T control, Func<System.Collections.Generic.IEnumerable<Avalonia.Media.Color>> func, Action<System.Collections.Generic.IEnumerable<Avalonia.Media.Color>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.CustomPaletteColorsProperty, func, onChanged, expression);
public static T CustomPaletteColors<T>(this T control, System.Collections.Generic.IEnumerable<Avalonia.Media.Color> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.CustomPaletteColorsProperty, ps, () => control.CustomPaletteColors = value, bindingMode, converter, bindingSource);
public static T CustomPaletteColors<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.Generic.IEnumerable<Avalonia.Media.Color>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.CustomPaletteColorsProperty, ps, () => control.CustomPaletteColors = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T PaletteColumnCount<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.PaletteColumnCountProperty, binding);
public static T PaletteColumnCount<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.PaletteColumnCountProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T PaletteColumnCount<T>(this T control, Func<System.Int32> func, Action<System.Int32>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => control._set(FluentAvalonia.UI.Controls.FAColorPicker.PaletteColumnCountProperty, func, onChanged, expression);
public static T PaletteColumnCount<T>(this T control, System.Int32 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.PaletteColumnCountProperty, ps, () => control.PaletteColumnCount = value, bindingMode, converter, bindingSource);
public static T PaletteColumnCount<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Int32> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> control._setEx(FluentAvalonia.UI.Controls.FAColorPicker.PaletteColumnCountProperty, ps, () => control.PaletteColumnCount = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

