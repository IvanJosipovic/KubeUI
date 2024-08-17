using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using DataGridRowHeader = Avalonia.Controls.Primitives.DataGridRowHeader;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class DataGridRowHeaderExtensions
{
public static Style<T> SeparatorBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Avalonia.Controls.Primitives.DataGridRowHeader
=> style._addSetter(Avalonia.Controls.Primitives.DataGridRowHeader.SeparatorBrushProperty, value);
public static Style<T> SeparatorBrush<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.Primitives.DataGridRowHeader
=> style._addSetter(Avalonia.Controls.Primitives.DataGridRowHeader.SeparatorBrushProperty, binding);
public static Style<T> AreSeparatorsVisible<T>(this Style<T> style, System.Boolean value) where T : Avalonia.Controls.Primitives.DataGridRowHeader
=> style._addSetter(Avalonia.Controls.Primitives.DataGridRowHeader.AreSeparatorsVisibleProperty, value);
public static Style<T> AreSeparatorsVisible<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.Primitives.DataGridRowHeader
=> style._addSetter(Avalonia.Controls.Primitives.DataGridRowHeader.AreSeparatorsVisibleProperty, binding);
}

