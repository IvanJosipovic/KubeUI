#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Indentation;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TextArea = AvaloniaEdit.Editing.TextArea;

namespace Avalonia.Markup.Declarative;
public static partial class TextAreaExtensions
{
public static T Document<T>(this T control, IBinding binding) where T : AvaloniaEdit.Editing.TextArea
   => control._set(AvaloniaEdit.Editing.TextArea.DocumentProperty, binding);
public static T Document<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Editing.TextArea
   => control._set(AvaloniaEdit.Editing.TextArea.DocumentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Document<T>(this T control, Func<AvaloniaEdit.Document.TextDocument> func, Action<AvaloniaEdit.Document.TextDocument>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Editing.TextArea
   => control._set(AvaloniaEdit.Editing.TextArea.DocumentProperty, func, onChanged, expression);
public static T Document<T>(this T control, AvaloniaEdit.Document.TextDocument value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Editing.TextArea
=> control._setEx(AvaloniaEdit.Editing.TextArea.DocumentProperty, ps, () => control.Document = value, bindingMode, converter, bindingSource);
public static T Document<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, AvaloniaEdit.Document.TextDocument> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Editing.TextArea
=> control._setEx(AvaloniaEdit.Editing.TextArea.DocumentProperty, ps, () => control.Document = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Options<T>(this T control, IBinding binding) where T : AvaloniaEdit.Editing.TextArea
   => control._set(AvaloniaEdit.Editing.TextArea.OptionsProperty, binding);
public static T Options<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Editing.TextArea
   => control._set(AvaloniaEdit.Editing.TextArea.OptionsProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Options<T>(this T control, Func<AvaloniaEdit.TextEditorOptions> func, Action<AvaloniaEdit.TextEditorOptions>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Editing.TextArea
   => control._set(AvaloniaEdit.Editing.TextArea.OptionsProperty, func, onChanged, expression);
public static T Options<T>(this T control, AvaloniaEdit.TextEditorOptions value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Editing.TextArea
=> control._setEx(AvaloniaEdit.Editing.TextArea.OptionsProperty, ps, () => control.Options = value, bindingMode, converter, bindingSource);
public static T Options<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, AvaloniaEdit.TextEditorOptions> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Editing.TextArea
=> control._setEx(AvaloniaEdit.Editing.TextArea.OptionsProperty, ps, () => control.Options = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SelectionBrush<T>(this T control, IBinding binding) where T : AvaloniaEdit.Editing.TextArea
   => control._set(AvaloniaEdit.Editing.TextArea.SelectionBrushProperty, binding);
public static T SelectionBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Editing.TextArea
   => control._set(AvaloniaEdit.Editing.TextArea.SelectionBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SelectionBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Editing.TextArea
   => control._set(AvaloniaEdit.Editing.TextArea.SelectionBrushProperty, func, onChanged, expression);
public static T SelectionBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Editing.TextArea
=> control._setEx(AvaloniaEdit.Editing.TextArea.SelectionBrushProperty, ps, () => control.SelectionBrush = value, bindingMode, converter, bindingSource);
public static T SelectionBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Editing.TextArea
=> control._setEx(AvaloniaEdit.Editing.TextArea.SelectionBrushProperty, ps, () => control.SelectionBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SelectionForeground<T>(this T control, IBinding binding) where T : AvaloniaEdit.Editing.TextArea
   => control._set(AvaloniaEdit.Editing.TextArea.SelectionForegroundProperty, binding);
public static T SelectionForeground<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Editing.TextArea
   => control._set(AvaloniaEdit.Editing.TextArea.SelectionForegroundProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SelectionForeground<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Editing.TextArea
   => control._set(AvaloniaEdit.Editing.TextArea.SelectionForegroundProperty, func, onChanged, expression);
public static T SelectionForeground<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Editing.TextArea
=> control._setEx(AvaloniaEdit.Editing.TextArea.SelectionForegroundProperty, ps, () => control.SelectionForeground = value, bindingMode, converter, bindingSource);
public static T SelectionForeground<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Editing.TextArea
=> control._setEx(AvaloniaEdit.Editing.TextArea.SelectionForegroundProperty, ps, () => control.SelectionForeground = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SelectionBorder<T>(this T control, IBinding binding) where T : AvaloniaEdit.Editing.TextArea
   => control._set(AvaloniaEdit.Editing.TextArea.SelectionBorderProperty, binding);
public static T SelectionBorder<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Editing.TextArea
   => control._set(AvaloniaEdit.Editing.TextArea.SelectionBorderProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SelectionBorder<T>(this T control, Func<Avalonia.Media.Pen> func, Action<Avalonia.Media.Pen>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Editing.TextArea
   => control._set(AvaloniaEdit.Editing.TextArea.SelectionBorderProperty, func, onChanged, expression);
public static T SelectionBorder<T>(this T control, Avalonia.Media.Pen value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Editing.TextArea
=> control._setEx(AvaloniaEdit.Editing.TextArea.SelectionBorderProperty, ps, () => control.SelectionBorder = value, bindingMode, converter, bindingSource);
public static T SelectionBorder<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.Pen> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Editing.TextArea
=> control._setEx(AvaloniaEdit.Editing.TextArea.SelectionBorderProperty, ps, () => control.SelectionBorder = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SelectionCornerRadius<T>(this T control, IBinding binding) where T : AvaloniaEdit.Editing.TextArea
   => control._set(AvaloniaEdit.Editing.TextArea.SelectionCornerRadiusProperty, binding);
public static T SelectionCornerRadius<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Editing.TextArea
   => control._set(AvaloniaEdit.Editing.TextArea.SelectionCornerRadiusProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SelectionCornerRadius<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Editing.TextArea
   => control._set(AvaloniaEdit.Editing.TextArea.SelectionCornerRadiusProperty, func, onChanged, expression);
public static T SelectionCornerRadius<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Editing.TextArea
=> control._setEx(AvaloniaEdit.Editing.TextArea.SelectionCornerRadiusProperty, ps, () => control.SelectionCornerRadius = value, bindingMode, converter, bindingSource);
public static T SelectionCornerRadius<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Editing.TextArea
=> control._setEx(AvaloniaEdit.Editing.TextArea.SelectionCornerRadiusProperty, ps, () => control.SelectionCornerRadius = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T RightClickMovesCaret<T>(this T control, IBinding binding) where T : AvaloniaEdit.Editing.TextArea
   => control._set(AvaloniaEdit.Editing.TextArea.RightClickMovesCaretProperty, binding);
public static T RightClickMovesCaret<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Editing.TextArea
   => control._set(AvaloniaEdit.Editing.TextArea.RightClickMovesCaretProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T RightClickMovesCaret<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Editing.TextArea
   => control._set(AvaloniaEdit.Editing.TextArea.RightClickMovesCaretProperty, func, onChanged, expression);
public static T RightClickMovesCaret<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Editing.TextArea
=> control._setEx(AvaloniaEdit.Editing.TextArea.RightClickMovesCaretProperty, ps, () => control.RightClickMovesCaret = value, bindingMode, converter, bindingSource);
public static T RightClickMovesCaret<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Editing.TextArea
=> control._setEx(AvaloniaEdit.Editing.TextArea.RightClickMovesCaretProperty, ps, () => control.RightClickMovesCaret = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IndentationStrategy<T>(this T control, IBinding binding) where T : AvaloniaEdit.Editing.TextArea
   => control._set(AvaloniaEdit.Editing.TextArea.IndentationStrategyProperty, binding);
public static T IndentationStrategy<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Editing.TextArea
   => control._set(AvaloniaEdit.Editing.TextArea.IndentationStrategyProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IndentationStrategy<T>(this T control, Func<AvaloniaEdit.Indentation.IIndentationStrategy> func, Action<AvaloniaEdit.Indentation.IIndentationStrategy>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Editing.TextArea
   => control._set(AvaloniaEdit.Editing.TextArea.IndentationStrategyProperty, func, onChanged, expression);
public static T IndentationStrategy<T>(this T control, AvaloniaEdit.Indentation.IIndentationStrategy value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Editing.TextArea
=> control._setEx(AvaloniaEdit.Editing.TextArea.IndentationStrategyProperty, ps, () => control.IndentationStrategy = value, bindingMode, converter, bindingSource);
public static T IndentationStrategy<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, AvaloniaEdit.Indentation.IIndentationStrategy> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Editing.TextArea
=> control._setEx(AvaloniaEdit.Editing.TextArea.IndentationStrategyProperty, ps, () => control.IndentationStrategy = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T OverstrikeMode<T>(this T control, IBinding binding) where T : AvaloniaEdit.Editing.TextArea
   => control._set(AvaloniaEdit.Editing.TextArea.OverstrikeModeProperty, binding);
public static T OverstrikeMode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Editing.TextArea
   => control._set(AvaloniaEdit.Editing.TextArea.OverstrikeModeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T OverstrikeMode<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Editing.TextArea
   => control._set(AvaloniaEdit.Editing.TextArea.OverstrikeModeProperty, func, onChanged, expression);
public static T OverstrikeMode<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Editing.TextArea
=> control._setEx(AvaloniaEdit.Editing.TextArea.OverstrikeModeProperty, ps, () => control.OverstrikeMode = value, bindingMode, converter, bindingSource);
public static T OverstrikeMode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Editing.TextArea
=> control._setEx(AvaloniaEdit.Editing.TextArea.OverstrikeModeProperty, ps, () => control.OverstrikeMode = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

