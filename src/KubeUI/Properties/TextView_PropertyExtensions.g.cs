#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TextView = AvaloniaEdit.Rendering.TextView;

namespace Avalonia.Markup.Declarative;
public static partial class TextViewExtensions
{
public static T Document<T>(this T control, IBinding binding) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.DocumentProperty, binding);
public static T Document<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.DocumentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Document<T>(this T control, Func<AvaloniaEdit.Document.TextDocument> func, Action<AvaloniaEdit.Document.TextDocument>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.DocumentProperty, func, onChanged, expression);
public static T Document<T>(this T control, AvaloniaEdit.Document.TextDocument value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Rendering.TextView
=> control._setEx(AvaloniaEdit.Rendering.TextView.DocumentProperty, ps, () => control.Document = value, bindingMode, converter, bindingSource);
public static T Document<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, AvaloniaEdit.Document.TextDocument> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Rendering.TextView
=> control._setEx(AvaloniaEdit.Rendering.TextView.DocumentProperty, ps, () => control.Document = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Options<T>(this T control, IBinding binding) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.OptionsProperty, binding);
public static T Options<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.OptionsProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Options<T>(this T control, Func<AvaloniaEdit.TextEditorOptions> func, Action<AvaloniaEdit.TextEditorOptions>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.OptionsProperty, func, onChanged, expression);
public static T Options<T>(this T control, AvaloniaEdit.TextEditorOptions value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Rendering.TextView
=> control._setEx(AvaloniaEdit.Rendering.TextView.OptionsProperty, ps, () => control.Options = value, bindingMode, converter, bindingSource);
public static T Options<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, AvaloniaEdit.TextEditorOptions> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Rendering.TextView
=> control._setEx(AvaloniaEdit.Rendering.TextView.OptionsProperty, ps, () => control.Options = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T NonPrintableCharacterBrush<T>(this T control, IBinding binding) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.NonPrintableCharacterBrushProperty, binding);
public static T NonPrintableCharacterBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.NonPrintableCharacterBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T NonPrintableCharacterBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.NonPrintableCharacterBrushProperty, func, onChanged, expression);
public static T NonPrintableCharacterBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Rendering.TextView
=> control._setEx(AvaloniaEdit.Rendering.TextView.NonPrintableCharacterBrushProperty, ps, () => control.NonPrintableCharacterBrush = value, bindingMode, converter, bindingSource);
public static T NonPrintableCharacterBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Rendering.TextView
=> control._setEx(AvaloniaEdit.Rendering.TextView.NonPrintableCharacterBrushProperty, ps, () => control.NonPrintableCharacterBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T LinkTextForegroundBrush<T>(this T control, IBinding binding) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.LinkTextForegroundBrushProperty, binding);
public static T LinkTextForegroundBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.LinkTextForegroundBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T LinkTextForegroundBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.LinkTextForegroundBrushProperty, func, onChanged, expression);
public static T LinkTextForegroundBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Rendering.TextView
=> control._setEx(AvaloniaEdit.Rendering.TextView.LinkTextForegroundBrushProperty, ps, () => control.LinkTextForegroundBrush = value, bindingMode, converter, bindingSource);
public static T LinkTextForegroundBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Rendering.TextView
=> control._setEx(AvaloniaEdit.Rendering.TextView.LinkTextForegroundBrushProperty, ps, () => control.LinkTextForegroundBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T LinkTextBackgroundBrush<T>(this T control, IBinding binding) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.LinkTextBackgroundBrushProperty, binding);
public static T LinkTextBackgroundBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.LinkTextBackgroundBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T LinkTextBackgroundBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.LinkTextBackgroundBrushProperty, func, onChanged, expression);
public static T LinkTextBackgroundBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Rendering.TextView
=> control._setEx(AvaloniaEdit.Rendering.TextView.LinkTextBackgroundBrushProperty, ps, () => control.LinkTextBackgroundBrush = value, bindingMode, converter, bindingSource);
public static T LinkTextBackgroundBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Rendering.TextView
=> control._setEx(AvaloniaEdit.Rendering.TextView.LinkTextBackgroundBrushProperty, ps, () => control.LinkTextBackgroundBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T LinkTextUnderline<T>(this T control, IBinding binding) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.LinkTextUnderlineProperty, binding);
public static T LinkTextUnderline<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.LinkTextUnderlineProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T LinkTextUnderline<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.LinkTextUnderlineProperty, func, onChanged, expression);
public static T LinkTextUnderline<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Rendering.TextView
=> control._setEx(AvaloniaEdit.Rendering.TextView.LinkTextUnderlineProperty, ps, () => control.LinkTextUnderline = value, bindingMode, converter, bindingSource);
public static T LinkTextUnderline<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Rendering.TextView
=> control._setEx(AvaloniaEdit.Rendering.TextView.LinkTextUnderlineProperty, ps, () => control.LinkTextUnderline = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ColumnRulerPen<T>(this T control, IBinding binding) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.ColumnRulerPenProperty, binding);
public static T ColumnRulerPen<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.ColumnRulerPenProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ColumnRulerPen<T>(this T control, Func<Avalonia.Media.IPen> func, Action<Avalonia.Media.IPen>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.ColumnRulerPenProperty, func, onChanged, expression);
public static T ColumnRulerPen<T>(this T control, Avalonia.Media.IPen value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Rendering.TextView
=> control._setEx(AvaloniaEdit.Rendering.TextView.ColumnRulerPenProperty, ps, () => control.ColumnRulerPen = value, bindingMode, converter, bindingSource);
public static T ColumnRulerPen<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IPen> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Rendering.TextView
=> control._setEx(AvaloniaEdit.Rendering.TextView.ColumnRulerPenProperty, ps, () => control.ColumnRulerPen = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T CurrentLineBackground<T>(this T control, IBinding binding) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.CurrentLineBackgroundProperty, binding);
public static T CurrentLineBackground<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.CurrentLineBackgroundProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CurrentLineBackground<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.CurrentLineBackgroundProperty, func, onChanged, expression);
public static T CurrentLineBackground<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Rendering.TextView
=> control._setEx(AvaloniaEdit.Rendering.TextView.CurrentLineBackgroundProperty, ps, () => control.CurrentLineBackground = value, bindingMode, converter, bindingSource);
public static T CurrentLineBackground<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Rendering.TextView
=> control._setEx(AvaloniaEdit.Rendering.TextView.CurrentLineBackgroundProperty, ps, () => control.CurrentLineBackground = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T CurrentLineBorder<T>(this T control, IBinding binding) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.CurrentLineBorderProperty, binding);
public static T CurrentLineBorder<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.CurrentLineBorderProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CurrentLineBorder<T>(this T control, Func<Avalonia.Media.IPen> func, Action<Avalonia.Media.IPen>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.CurrentLineBorderProperty, func, onChanged, expression);
public static T CurrentLineBorder<T>(this T control, Avalonia.Media.IPen value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Rendering.TextView
=> control._setEx(AvaloniaEdit.Rendering.TextView.CurrentLineBorderProperty, ps, () => control.CurrentLineBorder = value, bindingMode, converter, bindingSource);
public static T CurrentLineBorder<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IPen> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Rendering.TextView
=> control._setEx(AvaloniaEdit.Rendering.TextView.CurrentLineBorderProperty, ps, () => control.CurrentLineBorder = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

