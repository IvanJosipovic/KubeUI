using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using ToolBarSeparator = Ursa.Controls.ToolBarSeparator;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class ToolBarSeparatorExtensions
{
public static Style<T> Orientation<T>(this Style<T> style, Avalonia.Layout.Orientation value) where T : Ursa.Controls.ToolBarSeparator
=> style._addSetter(Ursa.Controls.ToolBarSeparator.OrientationProperty, value);
public static Style<T> Orientation<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ToolBarSeparator
=> style._addSetter(Ursa.Controls.ToolBarSeparator.OrientationProperty, binding);
}

