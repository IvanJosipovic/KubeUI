using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using ColorPaletteItem = FluentAvalonia.UI.Controls.ColorPaletteItem;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class ColorPaletteItemExtensions
{
public static Style<T> BorderBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushProperty, value);
public static Style<T> BorderBrush<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushProperty, binding);
public static Style<T> BorderBrushPointerOver<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushPointerOverProperty, value);
public static Style<T> BorderBrushPointerOver<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushPointerOverProperty, binding);
public static Style<T> BorderBrushPressed<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushPressedProperty, value);
public static Style<T> BorderBrushPressed<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushPressedProperty, binding);
public static Style<T> BorderThickness<T>(this Style<T> style, Avalonia.Thickness value) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessProperty, value);
public static Style<T> BorderThickness<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessProperty, binding);

public static Style<T> BorderThickness<T>(this Style<T> style, Double uniformLength) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessProperty, new Avalonia.Thickness(uniformLength));
public static Style<T> BorderThickness<T>(this Style<T> style, Double horizontal, Double vertical) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessProperty, new Avalonia.Thickness(horizontal, vertical));
public static Style<T> BorderThickness<T>(this Style<T> style, Double left, Double top, Double right, Double bottom) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessProperty, new Avalonia.Thickness(left, top, right, bottom));
public static Style<T> BorderThicknessPointerOver<T>(this Style<T> style, Avalonia.Thickness value) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPointerOverProperty, value);
public static Style<T> BorderThicknessPointerOver<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPointerOverProperty, binding);

public static Style<T> BorderThicknessPointerOver<T>(this Style<T> style, Double uniformLength) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPointerOverProperty, new Avalonia.Thickness(uniformLength));
public static Style<T> BorderThicknessPointerOver<T>(this Style<T> style, Double horizontal, Double vertical) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPointerOverProperty, new Avalonia.Thickness(horizontal, vertical));
public static Style<T> BorderThicknessPointerOver<T>(this Style<T> style, Double left, Double top, Double right, Double bottom) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPointerOverProperty, new Avalonia.Thickness(left, top, right, bottom));
public static Style<T> BorderThicknessPressed<T>(this Style<T> style, Avalonia.Thickness value) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPressedProperty, value);
public static Style<T> BorderThicknessPressed<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPressedProperty, binding);

public static Style<T> BorderThicknessPressed<T>(this Style<T> style, Double uniformLength) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPressedProperty, new Avalonia.Thickness(uniformLength));
public static Style<T> BorderThicknessPressed<T>(this Style<T> style, Double horizontal, Double vertical) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPressedProperty, new Avalonia.Thickness(horizontal, vertical));
public static Style<T> BorderThicknessPressed<T>(this Style<T> style, Double left, Double top, Double right, Double bottom) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPressedProperty, new Avalonia.Thickness(left, top, right, bottom));
public static Style<T> CornerRadius<T>(this Style<T> style, Avalonia.CornerRadius value) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.CornerRadiusProperty, value);
public static Style<T> CornerRadius<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.CornerRadiusProperty, binding);

public static Style<T> CornerRadius<T>(this Style<T> style, Double uniformRadius) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.CornerRadiusProperty, new Avalonia.CornerRadius(uniformRadius));
public static Style<T> CornerRadius<T>(this Style<T> style, Double top, Double bottom) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.CornerRadiusProperty, new Avalonia.CornerRadius(top, bottom));
public static Style<T> CornerRadius<T>(this Style<T> style, Double topLeft, Double topRight, Double bottomRight, Double bottomLeft) where T : FluentAvalonia.UI.Controls.ColorPaletteItem
   => style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.CornerRadiusProperty, new Avalonia.CornerRadius(topLeft, topRight, bottomRight, bottomLeft));
}

