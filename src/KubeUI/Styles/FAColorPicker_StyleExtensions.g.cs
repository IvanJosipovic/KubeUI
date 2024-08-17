using Avalonia.Data;
using Avalonia.Data.Converters;
using FAColorPicker = FluentAvalonia.UI.Controls.FAColorPicker;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class FAColorPickerExtensions
{
public static Style<T> PreviousColor<T>(this Style<T> style, FluentAvalonia.UI.Media.Color2 value) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.PreviousColorProperty, value);
public static Style<T> PreviousColor<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.PreviousColorProperty, binding);

public static Style<T> PreviousColor<T>(this Style<T> style, Byte r, Byte g, Byte b, Byte a) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.PreviousColorProperty, new FluentAvalonia.UI.Media.Color2(r, g, b, a));
public static Style<T> PreviousColor<T>(this Style<T> style, Color avColor) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.PreviousColorProperty, new FluentAvalonia.UI.Media.Color2(avColor));
public static Style<T> Color<T>(this Style<T> style, FluentAvalonia.UI.Media.Color2 value) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.ColorProperty, value);
public static Style<T> Color<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.ColorProperty, binding);

public static Style<T> Color<T>(this Style<T> style, Byte r, Byte g, Byte b, Byte a) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.ColorProperty, new FluentAvalonia.UI.Media.Color2(r, g, b, a));
public static Style<T> Color<T>(this Style<T> style, Color avColor) where T : FluentAvalonia.UI.Controls.FAColorPicker
   => style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.ColorProperty, new FluentAvalonia.UI.Media.Color2(avColor));
public static Style<T> ColorTextType<T>(this Style<T> style, FluentAvalonia.UI.Controls.ColorTextType value) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.ColorTextTypeProperty, value);
public static Style<T> ColorTextType<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.ColorTextTypeProperty, binding);
public static Style<T> Component<T>(this Style<T> style, FluentAvalonia.UI.Controls.ColorSpectrumComponents value) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.ComponentProperty, value);
public static Style<T> Component<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.ComponentProperty, binding);
public static Style<T> IsMoreButtonVisible<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.IsMoreButtonVisibleProperty, value);
public static Style<T> IsMoreButtonVisible<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.IsMoreButtonVisibleProperty, binding);
public static Style<T> IsCompact<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.IsCompactProperty, value);
public static Style<T> IsCompact<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.IsCompactProperty, binding);
public static Style<T> IsAlphaEnabled<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.IsAlphaEnabledProperty, value);
public static Style<T> IsAlphaEnabled<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.IsAlphaEnabledProperty, binding);
public static Style<T> UseSpectrum<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.UseSpectrumProperty, value);
public static Style<T> UseSpectrum<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.UseSpectrumProperty, binding);
public static Style<T> UseColorWheel<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.UseColorWheelProperty, value);
public static Style<T> UseColorWheel<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.UseColorWheelProperty, binding);
public static Style<T> UseColorTriangle<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.UseColorTriangleProperty, value);
public static Style<T> UseColorTriangle<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.UseColorTriangleProperty, binding);
public static Style<T> UseColorPalette<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.UseColorPaletteProperty, value);
public static Style<T> UseColorPalette<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.UseColorPaletteProperty, binding);
public static Style<T> PaletteColumnCount<T>(this Style<T> style, System.Int32 value) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.PaletteColumnCountProperty, value);
public static Style<T> PaletteColumnCount<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAColorPicker
=> style._addSetter(FluentAvalonia.UI.Controls.FAColorPicker.PaletteColumnCountProperty, binding);
}

