using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Svg.Skia;
using Svg = Avalonia.Svg.Skia.Svg;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class SvgExtensions
{
public static Style<T> Path<T>(this Style<T> style, System.String value) where T : Avalonia.Svg.Skia.Svg
=> style._addSetter(Avalonia.Svg.Skia.Svg.PathProperty, value);
public static Style<T> Path<T>(this Style<T> style, IBinding binding) where T : Avalonia.Svg.Skia.Svg
=> style._addSetter(Avalonia.Svg.Skia.Svg.PathProperty, binding);
public static Style<T> Source<T>(this Style<T> style, System.String value) where T : Avalonia.Svg.Skia.Svg
=> style._addSetter(Avalonia.Svg.Skia.Svg.SourceProperty, value);
public static Style<T> Source<T>(this Style<T> style, IBinding binding) where T : Avalonia.Svg.Skia.Svg
=> style._addSetter(Avalonia.Svg.Skia.Svg.SourceProperty, binding);
public static Style<T> Stretch<T>(this Style<T> style, Avalonia.Media.Stretch value) where T : Avalonia.Svg.Skia.Svg
=> style._addSetter(Avalonia.Svg.Skia.Svg.StretchProperty, value);
public static Style<T> Stretch<T>(this Style<T> style, IBinding binding) where T : Avalonia.Svg.Skia.Svg
=> style._addSetter(Avalonia.Svg.Skia.Svg.StretchProperty, binding);
public static Style<T> StretchDirection<T>(this Style<T> style, Avalonia.Media.StretchDirection value) where T : Avalonia.Svg.Skia.Svg
=> style._addSetter(Avalonia.Svg.Skia.Svg.StretchDirectionProperty, value);
public static Style<T> StretchDirection<T>(this Style<T> style, IBinding binding) where T : Avalonia.Svg.Skia.Svg
=> style._addSetter(Avalonia.Svg.Skia.Svg.StretchDirectionProperty, binding);
}

