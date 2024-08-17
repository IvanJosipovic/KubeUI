using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using DataGridRow = Avalonia.Controls.DataGridRow;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class DataGridRowExtensions
{
public static Style<T> Header<T>(this Style<T> style, System.Object value) where T : Avalonia.Controls.DataGridRow
=> style._addSetter(Avalonia.Controls.DataGridRow.HeaderProperty, value);
public static Style<T> Header<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGridRow
=> style._addSetter(Avalonia.Controls.DataGridRow.HeaderProperty, binding);
public static Style<T> DetailsTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : Avalonia.Controls.DataGridRow
=> style._addSetter(Avalonia.Controls.DataGridRow.DetailsTemplateProperty, value);
public static Style<T> DetailsTemplate<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGridRow
=> style._addSetter(Avalonia.Controls.DataGridRow.DetailsTemplateProperty, binding);
public static Style<T> AreDetailsVisible<T>(this Style<T> style, System.Boolean value) where T : Avalonia.Controls.DataGridRow
=> style._addSetter(Avalonia.Controls.DataGridRow.AreDetailsVisibleProperty, value);
public static Style<T> AreDetailsVisible<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGridRow
=> style._addSetter(Avalonia.Controls.DataGridRow.AreDetailsVisibleProperty, binding);
}

