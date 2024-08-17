#nullable enable
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
public static T IsItemCountVisible<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGridRowGroupHeader
   => control._set(Avalonia.Controls.DataGridRowGroupHeader.IsItemCountVisibleProperty, binding);
public static T IsItemCountVisible<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGridRowGroupHeader
   => control._set(Avalonia.Controls.DataGridRowGroupHeader.IsItemCountVisibleProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsItemCountVisible<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGridRowGroupHeader
   => control._set(Avalonia.Controls.DataGridRowGroupHeader.IsItemCountVisibleProperty, func, onChanged, expression);
public static T IsItemCountVisible<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridRowGroupHeader
=> control._setEx(Avalonia.Controls.DataGridRowGroupHeader.IsItemCountVisibleProperty, ps, () => control.IsItemCountVisible = value, bindingMode, converter, bindingSource);
public static T IsItemCountVisible<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridRowGroupHeader
=> control._setEx(Avalonia.Controls.DataGridRowGroupHeader.IsItemCountVisibleProperty, ps, () => control.IsItemCountVisible = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ItemCountFormat<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGridRowGroupHeader
   => control._set(Avalonia.Controls.DataGridRowGroupHeader.ItemCountFormatProperty, binding);
public static T ItemCountFormat<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGridRowGroupHeader
   => control._set(Avalonia.Controls.DataGridRowGroupHeader.ItemCountFormatProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ItemCountFormat<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGridRowGroupHeader
   => control._set(Avalonia.Controls.DataGridRowGroupHeader.ItemCountFormatProperty, func, onChanged, expression);
public static T ItemCountFormat<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridRowGroupHeader
=> control._setEx(Avalonia.Controls.DataGridRowGroupHeader.ItemCountFormatProperty, ps, () => control.ItemCountFormat = value, bindingMode, converter, bindingSource);
public static T ItemCountFormat<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridRowGroupHeader
=> control._setEx(Avalonia.Controls.DataGridRowGroupHeader.ItemCountFormatProperty, ps, () => control.ItemCountFormat = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Name<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGridRowGroupHeader
   => control._set(Avalonia.Controls.DataGridRowGroupHeader.PropertyNameProperty, binding);
public static T Name<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGridRowGroupHeader
   => control._set(Avalonia.Controls.DataGridRowGroupHeader.PropertyNameProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Name<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGridRowGroupHeader
   => control._set(Avalonia.Controls.DataGridRowGroupHeader.PropertyNameProperty, func, onChanged, expression);
public static T Name<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridRowGroupHeader
=> control._setEx(Avalonia.Controls.DataGridRowGroupHeader.PropertyNameProperty, ps, () => control.Name = value, bindingMode, converter, bindingSource);
public static T Name<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridRowGroupHeader
=> control._setEx(Avalonia.Controls.DataGridRowGroupHeader.PropertyNameProperty, ps, () => control.Name = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SublevelIndent<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGridRowGroupHeader
   => control._set(Avalonia.Controls.DataGridRowGroupHeader.SublevelIndentProperty, binding);
public static T SublevelIndent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGridRowGroupHeader
   => control._set(Avalonia.Controls.DataGridRowGroupHeader.SublevelIndentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SublevelIndent<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGridRowGroupHeader
   => control._set(Avalonia.Controls.DataGridRowGroupHeader.SublevelIndentProperty, func, onChanged, expression);
public static T SublevelIndent<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridRowGroupHeader
=> control._setEx(Avalonia.Controls.DataGridRowGroupHeader.SublevelIndentProperty, ps, () => control.SublevelIndent = value, bindingMode, converter, bindingSource);
public static T SublevelIndent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridRowGroupHeader
=> control._setEx(Avalonia.Controls.DataGridRowGroupHeader.SublevelIndentProperty, ps, () => control.SublevelIndent = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

