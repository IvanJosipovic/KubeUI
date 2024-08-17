#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using AvaloniaEdit.Search;
using SearchPanel = AvaloniaEdit.Search.SearchPanel;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class SearchPanelExtensions
{
public static T UseRegex<T>(this T control, IBinding binding) where T : AvaloniaEdit.Search.SearchPanel
   => control._set(AvaloniaEdit.Search.SearchPanel.UseRegexProperty, binding);
public static T UseRegex<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Search.SearchPanel
   => control._set(AvaloniaEdit.Search.SearchPanel.UseRegexProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T UseRegex<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Search.SearchPanel
   => control._set(AvaloniaEdit.Search.SearchPanel.UseRegexProperty, func, onChanged, expression);
public static T UseRegex<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Search.SearchPanel
=> control._setEx(AvaloniaEdit.Search.SearchPanel.UseRegexProperty, ps, () => control.UseRegex = value, bindingMode, converter, bindingSource);
public static T UseRegex<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Search.SearchPanel
=> control._setEx(AvaloniaEdit.Search.SearchPanel.UseRegexProperty, ps, () => control.UseRegex = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T MatchCase<T>(this T control, IBinding binding) where T : AvaloniaEdit.Search.SearchPanel
   => control._set(AvaloniaEdit.Search.SearchPanel.MatchCaseProperty, binding);
public static T MatchCase<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Search.SearchPanel
   => control._set(AvaloniaEdit.Search.SearchPanel.MatchCaseProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T MatchCase<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Search.SearchPanel
   => control._set(AvaloniaEdit.Search.SearchPanel.MatchCaseProperty, func, onChanged, expression);
public static T MatchCase<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Search.SearchPanel
=> control._setEx(AvaloniaEdit.Search.SearchPanel.MatchCaseProperty, ps, () => control.MatchCase = value, bindingMode, converter, bindingSource);
public static T MatchCase<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Search.SearchPanel
=> control._setEx(AvaloniaEdit.Search.SearchPanel.MatchCaseProperty, ps, () => control.MatchCase = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T WholeWords<T>(this T control, IBinding binding) where T : AvaloniaEdit.Search.SearchPanel
   => control._set(AvaloniaEdit.Search.SearchPanel.WholeWordsProperty, binding);
public static T WholeWords<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Search.SearchPanel
   => control._set(AvaloniaEdit.Search.SearchPanel.WholeWordsProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T WholeWords<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Search.SearchPanel
   => control._set(AvaloniaEdit.Search.SearchPanel.WholeWordsProperty, func, onChanged, expression);
public static T WholeWords<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Search.SearchPanel
=> control._setEx(AvaloniaEdit.Search.SearchPanel.WholeWordsProperty, ps, () => control.WholeWords = value, bindingMode, converter, bindingSource);
public static T WholeWords<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Search.SearchPanel
=> control._setEx(AvaloniaEdit.Search.SearchPanel.WholeWordsProperty, ps, () => control.WholeWords = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SearchPattern<T>(this T control, IBinding binding) where T : AvaloniaEdit.Search.SearchPanel
   => control._set(AvaloniaEdit.Search.SearchPanel.SearchPatternProperty, binding);
public static T SearchPattern<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Search.SearchPanel
   => control._set(AvaloniaEdit.Search.SearchPanel.SearchPatternProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SearchPattern<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Search.SearchPanel
   => control._set(AvaloniaEdit.Search.SearchPanel.SearchPatternProperty, func, onChanged, expression);
public static T SearchPattern<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Search.SearchPanel
=> control._setEx(AvaloniaEdit.Search.SearchPanel.SearchPatternProperty, ps, () => control.SearchPattern = value, bindingMode, converter, bindingSource);
public static T SearchPattern<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Search.SearchPanel
=> control._setEx(AvaloniaEdit.Search.SearchPanel.SearchPatternProperty, ps, () => control.SearchPattern = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsReplaceMode<T>(this T control, IBinding binding) where T : AvaloniaEdit.Search.SearchPanel
   => control._set(AvaloniaEdit.Search.SearchPanel.IsReplaceModeProperty, binding);
public static T IsReplaceMode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Search.SearchPanel
   => control._set(AvaloniaEdit.Search.SearchPanel.IsReplaceModeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsReplaceMode<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Search.SearchPanel
   => control._set(AvaloniaEdit.Search.SearchPanel.IsReplaceModeProperty, func, onChanged, expression);
public static T IsReplaceMode<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Search.SearchPanel
=> control._setEx(AvaloniaEdit.Search.SearchPanel.IsReplaceModeProperty, ps, () => control.IsReplaceMode = value, bindingMode, converter, bindingSource);
public static T IsReplaceMode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Search.SearchPanel
=> control._setEx(AvaloniaEdit.Search.SearchPanel.IsReplaceModeProperty, ps, () => control.IsReplaceMode = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ReplacePattern<T>(this T control, IBinding binding) where T : AvaloniaEdit.Search.SearchPanel
   => control._set(AvaloniaEdit.Search.SearchPanel.ReplacePatternProperty, binding);
public static T ReplacePattern<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Search.SearchPanel
   => control._set(AvaloniaEdit.Search.SearchPanel.ReplacePatternProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ReplacePattern<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Search.SearchPanel
   => control._set(AvaloniaEdit.Search.SearchPanel.ReplacePatternProperty, func, onChanged, expression);
public static T ReplacePattern<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Search.SearchPanel
=> control._setEx(AvaloniaEdit.Search.SearchPanel.ReplacePatternProperty, ps, () => control.ReplacePattern = value, bindingMode, converter, bindingSource);
public static T ReplacePattern<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Search.SearchPanel
=> control._setEx(AvaloniaEdit.Search.SearchPanel.ReplacePatternProperty, ps, () => control.ReplacePattern = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

