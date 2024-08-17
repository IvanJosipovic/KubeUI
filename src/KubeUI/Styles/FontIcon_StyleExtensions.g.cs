using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using FontIcon = FluentAvalonia.UI.Controls.FontIcon;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class FontIconExtensions
{
public static Style<T> FontFamily<T>(this Style<T> style, Avalonia.Media.FontFamily value) where T : FluentAvalonia.UI.Controls.FontIcon
=> style._addSetter(FluentAvalonia.UI.Controls.FontIcon.FontFamilyProperty, value);
public static Style<T> FontFamily<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FontIcon
=> style._addSetter(FluentAvalonia.UI.Controls.FontIcon.FontFamilyProperty, binding);
public static Style<T> FontSize<T>(this Style<T> style, System.Double value) where T : FluentAvalonia.UI.Controls.FontIcon
=> style._addSetter(FluentAvalonia.UI.Controls.FontIcon.FontSizeProperty, value);
public static Style<T> FontSize<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FontIcon
=> style._addSetter(FluentAvalonia.UI.Controls.FontIcon.FontSizeProperty, binding);
public static Style<T> FontWeight<T>(this Style<T> style, Avalonia.Media.FontWeight value) where T : FluentAvalonia.UI.Controls.FontIcon
=> style._addSetter(FluentAvalonia.UI.Controls.FontIcon.FontWeightProperty, value);
public static Style<T> FontWeight<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FontIcon
=> style._addSetter(FluentAvalonia.UI.Controls.FontIcon.FontWeightProperty, binding);
public static Style<T> FontStyle<T>(this Style<T> style, Avalonia.Media.FontStyle value) where T : FluentAvalonia.UI.Controls.FontIcon
=> style._addSetter(FluentAvalonia.UI.Controls.FontIcon.FontStyleProperty, value);
public static Style<T> FontStyle<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FontIcon
=> style._addSetter(FluentAvalonia.UI.Controls.FontIcon.FontStyleProperty, binding);
public static Style<T> Glyph<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.FontIcon
=> style._addSetter(FluentAvalonia.UI.Controls.FontIcon.GlyphProperty, value);
public static Style<T> Glyph<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FontIcon
=> style._addSetter(FluentAvalonia.UI.Controls.FontIcon.GlyphProperty, binding);
}

