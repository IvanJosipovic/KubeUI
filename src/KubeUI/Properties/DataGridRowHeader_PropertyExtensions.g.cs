#nullable enable
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
public static T SeparatorBrush<T>(this T control, IBinding binding) where T : Avalonia.Controls.Primitives.DataGridRowHeader
   => control._set(Avalonia.Controls.Primitives.DataGridRowHeader.SeparatorBrushProperty, binding);
public static T SeparatorBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.Primitives.DataGridRowHeader
   => control._set(Avalonia.Controls.Primitives.DataGridRowHeader.SeparatorBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SeparatorBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.Primitives.DataGridRowHeader
   => control._set(Avalonia.Controls.Primitives.DataGridRowHeader.SeparatorBrushProperty, func, onChanged, expression);
public static T SeparatorBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.Primitives.DataGridRowHeader
=> control._setEx(Avalonia.Controls.Primitives.DataGridRowHeader.SeparatorBrushProperty, ps, () => control.SeparatorBrush = value, bindingMode, converter, bindingSource);
public static T SeparatorBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.Primitives.DataGridRowHeader
=> control._setEx(Avalonia.Controls.Primitives.DataGridRowHeader.SeparatorBrushProperty, ps, () => control.SeparatorBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T AreSeparatorsVisible<T>(this T control, IBinding binding) where T : Avalonia.Controls.Primitives.DataGridRowHeader
   => control._set(Avalonia.Controls.Primitives.DataGridRowHeader.AreSeparatorsVisibleProperty, binding);
public static T AreSeparatorsVisible<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.Primitives.DataGridRowHeader
   => control._set(Avalonia.Controls.Primitives.DataGridRowHeader.AreSeparatorsVisibleProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T AreSeparatorsVisible<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.Primitives.DataGridRowHeader
   => control._set(Avalonia.Controls.Primitives.DataGridRowHeader.AreSeparatorsVisibleProperty, func, onChanged, expression);
public static T AreSeparatorsVisible<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.Primitives.DataGridRowHeader
=> control._setEx(Avalonia.Controls.Primitives.DataGridRowHeader.AreSeparatorsVisibleProperty, ps, () => control.AreSeparatorsVisible = value, bindingMode, converter, bindingSource);
public static T AreSeparatorsVisible<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.Primitives.DataGridRowHeader
=> control._setEx(Avalonia.Controls.Primitives.DataGridRowHeader.AreSeparatorsVisibleProperty, ps, () => control.AreSeparatorsVisible = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

