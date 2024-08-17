#nullable enable
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using DataGridCheckBoxColumn = Avalonia.Controls.DataGridCheckBoxColumn;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class DataGridCheckBoxColumnExtensions
{
public static T IsThreeState<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGridCheckBoxColumn
   => control._set(Avalonia.Controls.DataGridCheckBoxColumn.IsThreeStateProperty, binding);
public static T IsThreeState<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGridCheckBoxColumn
   => control._set(Avalonia.Controls.DataGridCheckBoxColumn.IsThreeStateProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsThreeState<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGridCheckBoxColumn
   => control._set(Avalonia.Controls.DataGridCheckBoxColumn.IsThreeStateProperty, func, onChanged, expression);
public static T IsThreeState<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridCheckBoxColumn
=> control._setEx(Avalonia.Controls.DataGridCheckBoxColumn.IsThreeStateProperty, ps, () => control.IsThreeState = value, bindingMode, converter, bindingSource);
public static T IsThreeState<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridCheckBoxColumn
=> control._setEx(Avalonia.Controls.DataGridCheckBoxColumn.IsThreeStateProperty, ps, () => control.IsThreeState = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

