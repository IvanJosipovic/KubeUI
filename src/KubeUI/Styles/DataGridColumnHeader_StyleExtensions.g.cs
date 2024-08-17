using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using DataGridColumnHeader = Avalonia.Controls.DataGridColumnHeader;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class DataGridColumnHeaderExtensions
{
public static Style<T> SeparatorBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Avalonia.Controls.DataGridColumnHeader
=> style._addSetter(Avalonia.Controls.DataGridColumnHeader.SeparatorBrushProperty, value);
public static Style<T> SeparatorBrush<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGridColumnHeader
=> style._addSetter(Avalonia.Controls.DataGridColumnHeader.SeparatorBrushProperty, binding);
public static Style<T> AreSeparatorsVisible<T>(this Style<T> style, System.Boolean value) where T : Avalonia.Controls.DataGridColumnHeader
=> style._addSetter(Avalonia.Controls.DataGridColumnHeader.AreSeparatorsVisibleProperty, value);
public static Style<T> AreSeparatorsVisible<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGridColumnHeader
=> style._addSetter(Avalonia.Controls.DataGridColumnHeader.AreSeparatorsVisibleProperty, binding);
}

