using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using DataGridRowGroupHeader = Avalonia.Controls.DataGridRowGroupHeader;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class DataGridRowGroupHeaderExtensions
{
public static Style<T> IsItemCountVisible<T>(this Style<T> style, System.Boolean value) where T : Avalonia.Controls.DataGridRowGroupHeader
=> style._addSetter(Avalonia.Controls.DataGridRowGroupHeader.IsItemCountVisibleProperty, value);
public static Style<T> IsItemCountVisible<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGridRowGroupHeader
=> style._addSetter(Avalonia.Controls.DataGridRowGroupHeader.IsItemCountVisibleProperty, binding);
public static Style<T> ItemCountFormat<T>(this Style<T> style, System.String value) where T : Avalonia.Controls.DataGridRowGroupHeader
=> style._addSetter(Avalonia.Controls.DataGridRowGroupHeader.ItemCountFormatProperty, value);
public static Style<T> ItemCountFormat<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGridRowGroupHeader
=> style._addSetter(Avalonia.Controls.DataGridRowGroupHeader.ItemCountFormatProperty, binding);
public static Style<T> Name<T>(this Style<T> style, System.String value) where T : Avalonia.Controls.DataGridRowGroupHeader
=> style._addSetter(Avalonia.Controls.DataGridRowGroupHeader.NameProperty, value);
public static Style<T> Name<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGridRowGroupHeader
=> style._addSetter(Avalonia.Controls.DataGridRowGroupHeader.NameProperty, binding);
public static Style<T> SublevelIndent<T>(this Style<T> style, System.Double value) where T : Avalonia.Controls.DataGridRowGroupHeader
=> style._addSetter(Avalonia.Controls.DataGridRowGroupHeader.SublevelIndentProperty, value);
public static Style<T> SublevelIndent<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGridRowGroupHeader
=> style._addSetter(Avalonia.Controls.DataGridRowGroupHeader.SublevelIndentProperty, binding);
}

