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
public static partial class CompletionList_MarkupExtensions
{
//================= Properties ======================//
 // EmptyTemplateProperty

/*BindFromExpressionSetterGenerator*/
public static T EmptyTemplate<T>(this T control, Func<Avalonia.Markup.Xaml.Templates.ControlTemplate> func, Action<Avalonia.Markup.Xaml.Templates.ControlTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.CodeCompletion.CompletionList
   => control._set(AvaloniaEdit.CodeCompletion.CompletionList.EmptyTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T EmptyTemplate<T>(this T control, Avalonia.Markup.Xaml.Templates.ControlTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.CodeCompletion.CompletionList
=> control._setEx(AvaloniaEdit.CodeCompletion.CompletionList.EmptyTemplateProperty, ps, () => control.EmptyTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T EmptyTemplate<T>(this T control, IBinding binding) where T : AvaloniaEdit.CodeCompletion.CompletionList
   => control._set(AvaloniaEdit.CodeCompletion.CompletionList.EmptyTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T EmptyTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.CodeCompletion.CompletionList
   => control._set(AvaloniaEdit.CodeCompletion.CompletionList.EmptyTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T EmptyTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Markup.Xaml.Templates.ControlTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.CodeCompletion.CompletionList
=> control._setEx(AvaloniaEdit.CodeCompletion.CompletionList.EmptyTemplateProperty, ps, () => control.EmptyTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // InsertionRequested

/*ActionToEventGenerator*/
    public static T OnInsertionRequested<T>(this T control, Action<System.EventArgs> action) where T : AvaloniaEdit.CodeCompletion.CompletionList => 
        control._setEvent((System.EventHandler) ((arg0, arg1) => action(arg1)), h => control.InsertionRequested += h);


 // SelectionChanged

/*ActionToEventGenerator*/
    public static T OnSelectionChanged<T>(this T control, Action<Avalonia.Controls.SelectionChangedEventArgs> action) where T : AvaloniaEdit.CodeCompletion.CompletionList => 
        control._setEvent((System.EventHandler<Avalonia.Controls.SelectionChangedEventArgs>) ((arg0, arg1) => action(arg1)), h => control.SelectionChanged += h);



//================= Styles ======================//
 // EmptyTemplateProperty

/*ValueStyleSetterGenerator*/
public static Style<T> EmptyTemplate<T>(this Style<T> style, Avalonia.Markup.Xaml.Templates.ControlTemplate value) where T : AvaloniaEdit.CodeCompletion.CompletionList
=> style._addSetter(AvaloniaEdit.CodeCompletion.CompletionList.EmptyTemplateProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> EmptyTemplate<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.CodeCompletion.CompletionList
=> style._addSetter(AvaloniaEdit.CodeCompletion.CompletionList.EmptyTemplateProperty, binding);



}
