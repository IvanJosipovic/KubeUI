#nullable enable
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
public static T EmptyTemplate<T>(this T control, IBinding binding) where T : AvaloniaEdit.CodeCompletion.CompletionList
   => control._set(AvaloniaEdit.CodeCompletion.CompletionList.EmptyTemplateProperty, binding);
public static T EmptyTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.CodeCompletion.CompletionList
   => control._set(AvaloniaEdit.CodeCompletion.CompletionList.EmptyTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T EmptyTemplate<T>(this T control, Func<Avalonia.Markup.Xaml.Templates.ControlTemplate> func, Action<Avalonia.Markup.Xaml.Templates.ControlTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.CodeCompletion.CompletionList
   => control._set(AvaloniaEdit.CodeCompletion.CompletionList.EmptyTemplateProperty, func, onChanged, expression);
public static T EmptyTemplate<T>(this T control, Avalonia.Markup.Xaml.Templates.ControlTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.CodeCompletion.CompletionList
=> control._setEx(AvaloniaEdit.CodeCompletion.CompletionList.EmptyTemplateProperty, ps, () => control.EmptyTemplate = value, bindingMode, converter, bindingSource);
public static T EmptyTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Markup.Xaml.Templates.ControlTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.CodeCompletion.CompletionList
=> control._setEx(AvaloniaEdit.CodeCompletion.CompletionList.EmptyTemplateProperty, ps, () => control.EmptyTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

