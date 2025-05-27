#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
[global::System.CodeDom.Compiler.GeneratedCode("AvaloniaExtensionGenerator", "1.0.0.0")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static partial class LineNumberMargin_MarkupExtensions
{
//================= Properties ======================//
 // MinWidthInDigits

/*ValueSetterGenerator*/
public static T MinWidthInDigits<T>(this T control, System.Int32 value) where T : AvaloniaEdit.Editing.LineNumberMargin 
=> control._set(() => control.MinWidthInDigits = value!);

/*BindFromExpressionSetterGenerator*/
public static T MinWidthInDigits<T>(this T control, Func<System.Int32> func, Action<System.Int32>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : AvaloniaEdit.Editing.LineNumberMargin 
   => control._set(AvaloniaEdit.Editing.LineNumberMargin.MinWidthInDigitsProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T MinWidthInDigits<T>(this T control,System.Int32 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : AvaloniaEdit.Editing.LineNumberMargin 
=> control._setEx(AvaloniaEdit.Editing.LineNumberMargin.MinWidthInDigitsProperty, ps, () => control.MinWidthInDigits = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T MinWidthInDigits<T>(this T control, IBinding binding) where T : AvaloniaEdit.Editing.LineNumberMargin 
   => control._set(AvaloniaEdit.Editing.LineNumberMargin.MinWidthInDigitsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T MinWidthInDigits<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Editing.LineNumberMargin 
   => control._set(AvaloniaEdit.Editing.LineNumberMargin.MinWidthInDigitsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T MinWidthInDigits<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Int32> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : AvaloniaEdit.Editing.LineNumberMargin 
=> control._setEx(AvaloniaEdit.Editing.LineNumberMargin.MinWidthInDigitsProperty, ps, () => control.MinWidthInDigits = converter.TryConvert(value)!, bindingMode, converter, bindingSource);



//================= Styles ======================//
 // MinWidthInDigits

/*ValueStyleSetterGenerator*/
public static Style<T> MinWidthInDigits<T>(this Style<T> style, System.Int32 value) where T : AvaloniaEdit.Editing.LineNumberMargin 
=> style._addSetter(AvaloniaEdit.Editing.LineNumberMargin.MinWidthInDigitsProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> MinWidthInDigits<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Editing.LineNumberMargin 
=> style._addSetter(AvaloniaEdit.Editing.LineNumberMargin.MinWidthInDigitsProperty, binding);



}
