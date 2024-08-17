using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using SymbolIcon = FluentAvalonia.UI.Controls.SymbolIcon;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class SymbolIconExtensions
{
public static Style<T> Symbol<T>(this Style<T> style, FluentAvalonia.UI.Controls.Symbol value) where T : FluentAvalonia.UI.Controls.SymbolIcon
=> style._addSetter(FluentAvalonia.UI.Controls.SymbolIcon.SymbolProperty, value);
public static Style<T> Symbol<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.SymbolIcon
=> style._addSetter(FluentAvalonia.UI.Controls.SymbolIcon.SymbolProperty, binding);
public static Style<T> FontSize<T>(this Style<T> style, System.Double value) where T : FluentAvalonia.UI.Controls.SymbolIcon
=> style._addSetter(FluentAvalonia.UI.Controls.SymbolIcon.FontSizeProperty, value);
public static Style<T> FontSize<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.SymbolIcon
=> style._addSetter(FluentAvalonia.UI.Controls.SymbolIcon.FontSizeProperty, binding);
}

