#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using AvaloniaEdit.Folding;
using FoldingMargin = AvaloniaEdit.Folding.FoldingMargin;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class FoldingMarginExtensions
{
public static T FoldingMarkerBrush<T>(this T control, IBinding binding) where T : AvaloniaEdit.Folding.FoldingMargin
   => control._set(AvaloniaEdit.Folding.FoldingMargin.FoldingMarkerBrushProperty, binding);
public static T FoldingMarkerBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Folding.FoldingMargin
   => control._set(AvaloniaEdit.Folding.FoldingMargin.FoldingMarkerBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T FoldingMarkerBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Folding.FoldingMargin
   => control._set(AvaloniaEdit.Folding.FoldingMargin.FoldingMarkerBrushProperty, func, onChanged, expression);
public static T FoldingMarkerBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Folding.FoldingMargin
=> control._setEx(AvaloniaEdit.Folding.FoldingMargin.FoldingMarkerBrushProperty, ps, () => control.FoldingMarkerBrush = value, bindingMode, converter, bindingSource);
public static T FoldingMarkerBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Folding.FoldingMargin
=> control._setEx(AvaloniaEdit.Folding.FoldingMargin.FoldingMarkerBrushProperty, ps, () => control.FoldingMarkerBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T FoldingMarkerBackgroundBrush<T>(this T control, IBinding binding) where T : AvaloniaEdit.Folding.FoldingMargin
   => control._set(AvaloniaEdit.Folding.FoldingMargin.FoldingMarkerBackgroundBrushProperty, binding);
public static T FoldingMarkerBackgroundBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Folding.FoldingMargin
   => control._set(AvaloniaEdit.Folding.FoldingMargin.FoldingMarkerBackgroundBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T FoldingMarkerBackgroundBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Folding.FoldingMargin
   => control._set(AvaloniaEdit.Folding.FoldingMargin.FoldingMarkerBackgroundBrushProperty, func, onChanged, expression);
public static T FoldingMarkerBackgroundBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Folding.FoldingMargin
=> control._setEx(AvaloniaEdit.Folding.FoldingMargin.FoldingMarkerBackgroundBrushProperty, ps, () => control.FoldingMarkerBackgroundBrush = value, bindingMode, converter, bindingSource);
public static T FoldingMarkerBackgroundBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Folding.FoldingMargin
=> control._setEx(AvaloniaEdit.Folding.FoldingMargin.FoldingMarkerBackgroundBrushProperty, ps, () => control.FoldingMarkerBackgroundBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SelectedFoldingMarkerBrush<T>(this T control, IBinding binding) where T : AvaloniaEdit.Folding.FoldingMargin
   => control._set(AvaloniaEdit.Folding.FoldingMargin.SelectedFoldingMarkerBrushProperty, binding);
public static T SelectedFoldingMarkerBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Folding.FoldingMargin
   => control._set(AvaloniaEdit.Folding.FoldingMargin.SelectedFoldingMarkerBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SelectedFoldingMarkerBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Folding.FoldingMargin
   => control._set(AvaloniaEdit.Folding.FoldingMargin.SelectedFoldingMarkerBrushProperty, func, onChanged, expression);
public static T SelectedFoldingMarkerBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Folding.FoldingMargin
=> control._setEx(AvaloniaEdit.Folding.FoldingMargin.SelectedFoldingMarkerBrushProperty, ps, () => control.SelectedFoldingMarkerBrush = value, bindingMode, converter, bindingSource);
public static T SelectedFoldingMarkerBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Folding.FoldingMargin
=> control._setEx(AvaloniaEdit.Folding.FoldingMargin.SelectedFoldingMarkerBrushProperty, ps, () => control.SelectedFoldingMarkerBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SelectedFoldingMarkerBackgroundBrush<T>(this T control, IBinding binding) where T : AvaloniaEdit.Folding.FoldingMargin
   => control._set(AvaloniaEdit.Folding.FoldingMargin.SelectedFoldingMarkerBackgroundBrushProperty, binding);
public static T SelectedFoldingMarkerBackgroundBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Folding.FoldingMargin
   => control._set(AvaloniaEdit.Folding.FoldingMargin.SelectedFoldingMarkerBackgroundBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SelectedFoldingMarkerBackgroundBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Folding.FoldingMargin
   => control._set(AvaloniaEdit.Folding.FoldingMargin.SelectedFoldingMarkerBackgroundBrushProperty, func, onChanged, expression);
public static T SelectedFoldingMarkerBackgroundBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Folding.FoldingMargin
=> control._setEx(AvaloniaEdit.Folding.FoldingMargin.SelectedFoldingMarkerBackgroundBrushProperty, ps, () => control.SelectedFoldingMarkerBackgroundBrush = value, bindingMode, converter, bindingSource);
public static T SelectedFoldingMarkerBackgroundBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Folding.FoldingMargin
=> control._setEx(AvaloniaEdit.Folding.FoldingMargin.SelectedFoldingMarkerBackgroundBrushProperty, ps, () => control.SelectedFoldingMarkerBackgroundBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

