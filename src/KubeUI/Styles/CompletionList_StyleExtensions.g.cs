using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml.Templates;
using AvaloniaEdit.CodeCompletion;
using CompletionList = AvaloniaEdit.CodeCompletion.CompletionList;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class CompletionListExtensions
{
public static Style<T> EmptyTemplate<T>(this Style<T> style, Avalonia.Markup.Xaml.Templates.ControlTemplate value) where T : AvaloniaEdit.CodeCompletion.CompletionList
=> style._addSetter(AvaloniaEdit.CodeCompletion.CompletionList.EmptyTemplateProperty, value);
public static Style<T> EmptyTemplate<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.CodeCompletion.CompletionList
=> style._addSetter(AvaloniaEdit.CodeCompletion.CompletionList.EmptyTemplateProperty, binding);
}

