using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using ColorSpectrum = FluentAvalonia.UI.Controls.ColorSpectrum;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class ColorSpectrumExtensions
{
public static Style<T> BorderBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : FluentAvalonia.UI.Controls.ColorSpectrum
=> style._addSetter(FluentAvalonia.UI.Controls.ColorSpectrum.BorderBrushProperty, value);
public static Style<T> BorderBrush<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorSpectrum
=> style._addSetter(FluentAvalonia.UI.Controls.ColorSpectrum.BorderBrushProperty, binding);
public static Style<T> BorderThickness<T>(this Style<T> style, System.Double value) where T : FluentAvalonia.UI.Controls.ColorSpectrum
=> style._addSetter(FluentAvalonia.UI.Controls.ColorSpectrum.BorderThicknessProperty, value);
public static Style<T> BorderThickness<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorSpectrum
=> style._addSetter(FluentAvalonia.UI.Controls.ColorSpectrum.BorderThicknessProperty, binding);
}

