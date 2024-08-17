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
public static Style<T> UseRegex<T>(this Style<T> style, System.Boolean value) where T : AvaloniaEdit.Search.SearchPanel
=> style._addSetter(AvaloniaEdit.Search.SearchPanel.UseRegexProperty, value);
public static Style<T> UseRegex<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Search.SearchPanel
=> style._addSetter(AvaloniaEdit.Search.SearchPanel.UseRegexProperty, binding);
public static Style<T> MatchCase<T>(this Style<T> style, System.Boolean value) where T : AvaloniaEdit.Search.SearchPanel
=> style._addSetter(AvaloniaEdit.Search.SearchPanel.MatchCaseProperty, value);
public static Style<T> MatchCase<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Search.SearchPanel
=> style._addSetter(AvaloniaEdit.Search.SearchPanel.MatchCaseProperty, binding);
public static Style<T> WholeWords<T>(this Style<T> style, System.Boolean value) where T : AvaloniaEdit.Search.SearchPanel
=> style._addSetter(AvaloniaEdit.Search.SearchPanel.WholeWordsProperty, value);
public static Style<T> WholeWords<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Search.SearchPanel
=> style._addSetter(AvaloniaEdit.Search.SearchPanel.WholeWordsProperty, binding);
public static Style<T> SearchPattern<T>(this Style<T> style, System.String value) where T : AvaloniaEdit.Search.SearchPanel
=> style._addSetter(AvaloniaEdit.Search.SearchPanel.SearchPatternProperty, value);
public static Style<T> SearchPattern<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Search.SearchPanel
=> style._addSetter(AvaloniaEdit.Search.SearchPanel.SearchPatternProperty, binding);
public static Style<T> IsReplaceMode<T>(this Style<T> style, System.Boolean value) where T : AvaloniaEdit.Search.SearchPanel
=> style._addSetter(AvaloniaEdit.Search.SearchPanel.IsReplaceModeProperty, value);
public static Style<T> IsReplaceMode<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Search.SearchPanel
=> style._addSetter(AvaloniaEdit.Search.SearchPanel.IsReplaceModeProperty, binding);
public static Style<T> ReplacePattern<T>(this Style<T> style, System.String value) where T : AvaloniaEdit.Search.SearchPanel
=> style._addSetter(AvaloniaEdit.Search.SearchPanel.ReplacePatternProperty, value);
public static Style<T> ReplacePattern<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Search.SearchPanel
=> style._addSetter(AvaloniaEdit.Search.SearchPanel.ReplacePatternProperty, binding);
}

