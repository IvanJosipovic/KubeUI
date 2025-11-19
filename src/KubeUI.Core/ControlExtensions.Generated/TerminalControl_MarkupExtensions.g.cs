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
public static partial class TerminalControl_MarkupExtensions
{
//================= Properties ======================//
 // FontName

/*ValueSetterGenerator*/
public static T FontName<T>(this T control, System.String value) where T : AvaloniaTerminal.TerminalControl 
=> control._set(() => control.FontName = value!);

/*BindFromExpressionSetterGenerator*/
public static T FontName<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : AvaloniaTerminal.TerminalControl 
   => control._set(AvaloniaTerminal.TerminalControl.FontNameProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T FontName<T>(this T control,System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : AvaloniaTerminal.TerminalControl 
=> control._setEx(AvaloniaTerminal.TerminalControl.FontNameProperty, ps, () => control.FontName = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T FontName<T>(this T control, IBinding binding) where T : AvaloniaTerminal.TerminalControl 
   => control._set(AvaloniaTerminal.TerminalControl.FontNameProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T FontName<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaTerminal.TerminalControl 
   => control._set(AvaloniaTerminal.TerminalControl.FontNameProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T FontName<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : AvaloniaTerminal.TerminalControl 
=> control._setEx(AvaloniaTerminal.TerminalControl.FontNameProperty, ps, () => control.FontName = converter.TryConvert(value)!, bindingMode, converter, bindingSource);


 // FontSize

/*ValueSetterGenerator*/
public static T FontSize<T>(this T control, System.Double value) where T : AvaloniaTerminal.TerminalControl 
=> control._set(() => control.FontSize = value!);

/*BindFromExpressionSetterGenerator*/
public static T FontSize<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : AvaloniaTerminal.TerminalControl 
   => control._set(AvaloniaTerminal.TerminalControl.FontSizeProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T FontSize<T>(this T control,System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : AvaloniaTerminal.TerminalControl 
=> control._setEx(AvaloniaTerminal.TerminalControl.FontSizeProperty, ps, () => control.FontSize = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T FontSize<T>(this T control, IBinding binding) where T : AvaloniaTerminal.TerminalControl 
   => control._set(AvaloniaTerminal.TerminalControl.FontSizeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T FontSize<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaTerminal.TerminalControl 
   => control._set(AvaloniaTerminal.TerminalControl.FontSizeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T FontSize<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : AvaloniaTerminal.TerminalControl 
=> control._setEx(AvaloniaTerminal.TerminalControl.FontSizeProperty, ps, () => control.FontSize = converter.TryConvert(value)!, bindingMode, converter, bindingSource);



//================= Styles ======================//
 // FontName

/*ValueStyleSetterGenerator*/
public static Style<T> FontName<T>(this Style<T> style, System.String value) where T : AvaloniaTerminal.TerminalControl 
=> style._addSetter(AvaloniaTerminal.TerminalControl.FontNameProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> FontName<T>(this Style<T> style, IBinding binding) where T : AvaloniaTerminal.TerminalControl 
=> style._addSetter(AvaloniaTerminal.TerminalControl.FontNameProperty, binding);


 // FontSize

/*ValueStyleSetterGenerator*/
public static Style<T> FontSize<T>(this Style<T> style, System.Double value) where T : AvaloniaTerminal.TerminalControl 
=> style._addSetter(AvaloniaTerminal.TerminalControl.FontSizeProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> FontSize<T>(this Style<T> style, IBinding binding) where T : AvaloniaTerminal.TerminalControl 
=> style._addSetter(AvaloniaTerminal.TerminalControl.FontSizeProperty, binding);



}
