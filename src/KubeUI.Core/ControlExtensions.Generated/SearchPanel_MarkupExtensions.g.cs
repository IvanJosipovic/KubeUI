#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
[global::System.CodeDom.Compiler.GeneratedCode("AvaloniaExtensionGenerator", "11.1.3.0")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static partial class SearchPanel_MarkupExtensions
{
//================= Properties ======================//
 // UseRegexProperty

/*BindFromExpressionSetterGenerator*/
public static T UseRegex<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Search.SearchPanel
   => control._set(AvaloniaEdit.Search.SearchPanel.UseRegexProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T UseRegex<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Search.SearchPanel
=> control._setEx(AvaloniaEdit.Search.SearchPanel.UseRegexProperty, ps, () => control.UseRegex = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T UseRegex<T>(this T control, IBinding binding) where T : AvaloniaEdit.Search.SearchPanel
   => control._set(AvaloniaEdit.Search.SearchPanel.UseRegexProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T UseRegex<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Search.SearchPanel
   => control._set(AvaloniaEdit.Search.SearchPanel.UseRegexProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T UseRegex<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Search.SearchPanel
=> control._setEx(AvaloniaEdit.Search.SearchPanel.UseRegexProperty, ps, () => control.UseRegex = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // MatchCaseProperty

/*BindFromExpressionSetterGenerator*/
public static T MatchCase<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Search.SearchPanel
   => control._set(AvaloniaEdit.Search.SearchPanel.MatchCaseProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T MatchCase<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Search.SearchPanel
=> control._setEx(AvaloniaEdit.Search.SearchPanel.MatchCaseProperty, ps, () => control.MatchCase = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T MatchCase<T>(this T control, IBinding binding) where T : AvaloniaEdit.Search.SearchPanel
   => control._set(AvaloniaEdit.Search.SearchPanel.MatchCaseProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T MatchCase<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Search.SearchPanel
   => control._set(AvaloniaEdit.Search.SearchPanel.MatchCaseProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T MatchCase<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Search.SearchPanel
=> control._setEx(AvaloniaEdit.Search.SearchPanel.MatchCaseProperty, ps, () => control.MatchCase = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // WholeWordsProperty

/*BindFromExpressionSetterGenerator*/
public static T WholeWords<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Search.SearchPanel
   => control._set(AvaloniaEdit.Search.SearchPanel.WholeWordsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T WholeWords<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Search.SearchPanel
=> control._setEx(AvaloniaEdit.Search.SearchPanel.WholeWordsProperty, ps, () => control.WholeWords = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T WholeWords<T>(this T control, IBinding binding) where T : AvaloniaEdit.Search.SearchPanel
   => control._set(AvaloniaEdit.Search.SearchPanel.WholeWordsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T WholeWords<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Search.SearchPanel
   => control._set(AvaloniaEdit.Search.SearchPanel.WholeWordsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T WholeWords<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Search.SearchPanel
=> control._setEx(AvaloniaEdit.Search.SearchPanel.WholeWordsProperty, ps, () => control.WholeWords = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SearchPatternProperty

/*BindFromExpressionSetterGenerator*/
public static T SearchPattern<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Search.SearchPanel
   => control._set(AvaloniaEdit.Search.SearchPanel.SearchPatternProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SearchPattern<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Search.SearchPanel
=> control._setEx(AvaloniaEdit.Search.SearchPanel.SearchPatternProperty, ps, () => control.SearchPattern = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SearchPattern<T>(this T control, IBinding binding) where T : AvaloniaEdit.Search.SearchPanel
   => control._set(AvaloniaEdit.Search.SearchPanel.SearchPatternProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SearchPattern<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Search.SearchPanel
   => control._set(AvaloniaEdit.Search.SearchPanel.SearchPatternProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SearchPattern<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Search.SearchPanel
=> control._setEx(AvaloniaEdit.Search.SearchPanel.SearchPatternProperty, ps, () => control.SearchPattern = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsReplaceModeProperty

/*BindFromExpressionSetterGenerator*/
public static T IsReplaceMode<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Search.SearchPanel
   => control._set(AvaloniaEdit.Search.SearchPanel.IsReplaceModeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsReplaceMode<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Search.SearchPanel
=> control._setEx(AvaloniaEdit.Search.SearchPanel.IsReplaceModeProperty, ps, () => control.IsReplaceMode = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsReplaceMode<T>(this T control, IBinding binding) where T : AvaloniaEdit.Search.SearchPanel
   => control._set(AvaloniaEdit.Search.SearchPanel.IsReplaceModeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsReplaceMode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Search.SearchPanel
   => control._set(AvaloniaEdit.Search.SearchPanel.IsReplaceModeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsReplaceMode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Search.SearchPanel
=> control._setEx(AvaloniaEdit.Search.SearchPanel.IsReplaceModeProperty, ps, () => control.IsReplaceMode = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ReplacePatternProperty

/*BindFromExpressionSetterGenerator*/
public static T ReplacePattern<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Search.SearchPanel
   => control._set(AvaloniaEdit.Search.SearchPanel.ReplacePatternProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ReplacePattern<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Search.SearchPanel
=> control._setEx(AvaloniaEdit.Search.SearchPanel.ReplacePatternProperty, ps, () => control.ReplacePattern = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ReplacePattern<T>(this T control, IBinding binding) where T : AvaloniaEdit.Search.SearchPanel
   => control._set(AvaloniaEdit.Search.SearchPanel.ReplacePatternProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ReplacePattern<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Search.SearchPanel
   => control._set(AvaloniaEdit.Search.SearchPanel.ReplacePatternProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ReplacePattern<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Search.SearchPanel
=> control._setEx(AvaloniaEdit.Search.SearchPanel.ReplacePatternProperty, ps, () => control.ReplacePattern = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // SearchOptionsChanged

/*ActionToEventGenerator*/
    public static T OnSearchOptionsChanged<T>(this T control, Action<AvaloniaEdit.Search.SearchOptionsChangedEventArgs> action) where T : AvaloniaEdit.Search.SearchPanel => 
        control._setEvent((System.EventHandler<AvaloniaEdit.Search.SearchOptionsChangedEventArgs>) ((arg0, arg1) => action(arg1)), h => control.SearchOptionsChanged += h);



//================= Styles ======================//
 // UseRegexProperty

/*ValueStyleSetterGenerator*/
public static Style<T> UseRegex<T>(this Style<T> style, System.Boolean value) where T : AvaloniaEdit.Search.SearchPanel
=> style._addSetter(AvaloniaEdit.Search.SearchPanel.UseRegexProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> UseRegex<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Search.SearchPanel
=> style._addSetter(AvaloniaEdit.Search.SearchPanel.UseRegexProperty, binding);


 // MatchCaseProperty

/*ValueStyleSetterGenerator*/
public static Style<T> MatchCase<T>(this Style<T> style, System.Boolean value) where T : AvaloniaEdit.Search.SearchPanel
=> style._addSetter(AvaloniaEdit.Search.SearchPanel.MatchCaseProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> MatchCase<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Search.SearchPanel
=> style._addSetter(AvaloniaEdit.Search.SearchPanel.MatchCaseProperty, binding);


 // WholeWordsProperty

/*ValueStyleSetterGenerator*/
public static Style<T> WholeWords<T>(this Style<T> style, System.Boolean value) where T : AvaloniaEdit.Search.SearchPanel
=> style._addSetter(AvaloniaEdit.Search.SearchPanel.WholeWordsProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> WholeWords<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Search.SearchPanel
=> style._addSetter(AvaloniaEdit.Search.SearchPanel.WholeWordsProperty, binding);


 // SearchPatternProperty

/*ValueStyleSetterGenerator*/
public static Style<T> SearchPattern<T>(this Style<T> style, System.String value) where T : AvaloniaEdit.Search.SearchPanel
=> style._addSetter(AvaloniaEdit.Search.SearchPanel.SearchPatternProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SearchPattern<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Search.SearchPanel
=> style._addSetter(AvaloniaEdit.Search.SearchPanel.SearchPatternProperty, binding);


 // IsReplaceModeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsReplaceMode<T>(this Style<T> style, System.Boolean value) where T : AvaloniaEdit.Search.SearchPanel
=> style._addSetter(AvaloniaEdit.Search.SearchPanel.IsReplaceModeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsReplaceMode<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Search.SearchPanel
=> style._addSetter(AvaloniaEdit.Search.SearchPanel.IsReplaceModeProperty, binding);


 // ReplacePatternProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ReplacePattern<T>(this Style<T> style, System.String value) where T : AvaloniaEdit.Search.SearchPanel
=> style._addSetter(AvaloniaEdit.Search.SearchPanel.ReplacePatternProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ReplacePattern<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Search.SearchPanel
=> style._addSetter(AvaloniaEdit.Search.SearchPanel.ReplacePatternProperty, binding);



}
