using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using ColorPickerButton = FluentAvalonia.UI.Controls.ColorPickerButton;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class ColorPickerButtonExtensions
{
public static Style<T> Color<T>(this Style<T> style, System.Nullable<Avalonia.Media.Color> value) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.ColorProperty, value);
public static Style<T> Color<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.ColorProperty, binding);
public static Style<T> IsMoreButtonVisible<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.IsMoreButtonVisibleProperty, value);
public static Style<T> IsMoreButtonVisible<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.IsMoreButtonVisibleProperty, binding);
public static Style<T> IsCompact<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.IsCompactProperty, value);
public static Style<T> IsCompact<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.IsCompactProperty, binding);
public static Style<T> IsAlphaEnabled<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.IsAlphaEnabledProperty, value);
public static Style<T> IsAlphaEnabled<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.IsAlphaEnabledProperty, binding);
public static Style<T> UseSpectrum<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.UseSpectrumProperty, value);
public static Style<T> UseSpectrum<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.UseSpectrumProperty, binding);
public static Style<T> UseColorWheel<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorWheelProperty, value);
public static Style<T> UseColorWheel<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorWheelProperty, binding);
public static Style<T> UseColorTriangle<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorTriangleProperty, value);
public static Style<T> UseColorTriangle<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorTriangleProperty, binding);
public static Style<T> UseColorPalette<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorPaletteProperty, value);
public static Style<T> UseColorPalette<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.UseColorPaletteProperty, binding);
public static Style<T> PaletteColumnCount<T>(this Style<T> style, System.Int32 value) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.PaletteColumnCountProperty, value);
public static Style<T> PaletteColumnCount<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.PaletteColumnCountProperty, binding);
public static Style<T> ShowAcceptDismissButtons<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.ShowAcceptDismissButtonsProperty, value);
public static Style<T> ShowAcceptDismissButtons<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.ShowAcceptDismissButtonsProperty, binding);
public static Style<T> FlyoutPlacement<T>(this Style<T> style, Avalonia.Controls.PlacementMode value) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.FlyoutPlacementProperty, value);
public static Style<T> FlyoutPlacement<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerButton
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPickerButton.FlyoutPlacementProperty, binding);
}

