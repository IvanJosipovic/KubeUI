using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using IPv4Box = Ursa.Controls.IPv4Box;
using System;
using System.Linq.Expressions;
using System.Net;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class IPv4BoxExtensions
{
public static Style<T> IPAddress<T>(this Style<T> style, System.Net.IPAddress value) where T : Ursa.Controls.IPv4Box
=> style._addSetter(Ursa.Controls.IPv4Box.IPAddressProperty, value);
public static Style<T> IPAddress<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.IPv4Box
=> style._addSetter(Ursa.Controls.IPv4Box.IPAddressProperty, binding);
public static Style<T> TextAlignment<T>(this Style<T> style, Avalonia.Media.TextAlignment value) where T : Ursa.Controls.IPv4Box
=> style._addSetter(Ursa.Controls.IPv4Box.TextAlignmentProperty, value);
public static Style<T> TextAlignment<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.IPv4Box
=> style._addSetter(Ursa.Controls.IPv4Box.TextAlignmentProperty, binding);
public static Style<T> SelectionBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Ursa.Controls.IPv4Box
=> style._addSetter(Ursa.Controls.IPv4Box.SelectionBrushProperty, value);
public static Style<T> SelectionBrush<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.IPv4Box
=> style._addSetter(Ursa.Controls.IPv4Box.SelectionBrushProperty, binding);
public static Style<T> SelectionForegroundBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Ursa.Controls.IPv4Box
=> style._addSetter(Ursa.Controls.IPv4Box.SelectionForegroundBrushProperty, value);
public static Style<T> SelectionForegroundBrush<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.IPv4Box
=> style._addSetter(Ursa.Controls.IPv4Box.SelectionForegroundBrushProperty, binding);
public static Style<T> CaretBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Ursa.Controls.IPv4Box
=> style._addSetter(Ursa.Controls.IPv4Box.CaretBrushProperty, value);
public static Style<T> CaretBrush<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.IPv4Box
=> style._addSetter(Ursa.Controls.IPv4Box.CaretBrushProperty, binding);
public static Style<T> ShowLeadingZero<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.IPv4Box
=> style._addSetter(Ursa.Controls.IPv4Box.ShowLeadingZeroProperty, value);
public static Style<T> ShowLeadingZero<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.IPv4Box
=> style._addSetter(Ursa.Controls.IPv4Box.ShowLeadingZeroProperty, binding);
public static Style<T> InputMode<T>(this Style<T> style, Ursa.Controls.IPv4BoxInputMode value) where T : Ursa.Controls.IPv4Box
=> style._addSetter(Ursa.Controls.IPv4Box.InputModeProperty, value);
public static Style<T> InputMode<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.IPv4Box
=> style._addSetter(Ursa.Controls.IPv4Box.InputModeProperty, binding);
}

