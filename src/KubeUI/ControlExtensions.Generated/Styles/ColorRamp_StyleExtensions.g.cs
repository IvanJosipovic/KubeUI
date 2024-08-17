using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using ColorRamp = FluentAvalonia.UI.Controls.ColorRamp;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class ColorRampExtensions
{
public static Style<T> BorderBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : FluentAvalonia.UI.Controls.ColorRamp
=> style._addSetter(FluentAvalonia.UI.Controls.ColorRamp.BorderBrushProperty, value);
public static Style<T> BorderBrush<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorRamp
=> style._addSetter(FluentAvalonia.UI.Controls.ColorRamp.BorderBrushProperty, binding);
public static Style<T> BorderThickness<T>(this Style<T> style, System.Double value) where T : FluentAvalonia.UI.Controls.ColorRamp
=> style._addSetter(FluentAvalonia.UI.Controls.ColorRamp.BorderThicknessProperty, value);
public static Style<T> BorderThickness<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorRamp
=> style._addSetter(FluentAvalonia.UI.Controls.ColorRamp.BorderThicknessProperty, binding);
public static Style<T> CornerRadius<T>(this Style<T> style, Avalonia.CornerRadius value) where T : FluentAvalonia.UI.Controls.ColorRamp
=> style._addSetter(FluentAvalonia.UI.Controls.ColorRamp.CornerRadiusProperty, value);
public static Style<T> CornerRadius<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorRamp
=> style._addSetter(FluentAvalonia.UI.Controls.ColorRamp.CornerRadiusProperty, binding);

public static Style<T> CornerRadius<T>(this Style<T> style, Double uniformRadius) where T : FluentAvalonia.UI.Controls.ColorRamp
   => style._addSetter(FluentAvalonia.UI.Controls.ColorRamp.CornerRadiusProperty, new Avalonia.CornerRadius(uniformRadius));
public static Style<T> CornerRadius<T>(this Style<T> style, Double top, Double bottom) where T : FluentAvalonia.UI.Controls.ColorRamp
   => style._addSetter(FluentAvalonia.UI.Controls.ColorRamp.CornerRadiusProperty, new Avalonia.CornerRadius(top, bottom));
public static Style<T> CornerRadius<T>(this Style<T> style, Double topLeft, Double topRight, Double bottomRight, Double bottomLeft) where T : FluentAvalonia.UI.Controls.ColorRamp
   => style._addSetter(FluentAvalonia.UI.Controls.ColorRamp.CornerRadiusProperty, new Avalonia.CornerRadius(topLeft, topRight, bottomRight, bottomLeft));
}

