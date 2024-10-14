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
public static partial class AppWindow_MarkupExtensions
{
//================= Properties ======================//
 // Icon

/*BindFromExpressionSetterGenerator*/
public static T Icon<T>(this T control, Func<Avalonia.Media.IImage> func, Action<Avalonia.Media.IImage>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Windowing.AppWindow 
   => control._set(FluentAvalonia.UI.Windowing.AppWindow.IconProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Icon<T>(this T control,Avalonia.Media.IImage value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Windowing.AppWindow 
=> control._setEx(FluentAvalonia.UI.Windowing.AppWindow.IconProperty, ps, () => control.Icon = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Icon<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Windowing.AppWindow 
   => control._set(FluentAvalonia.UI.Windowing.AppWindow.IconProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Icon<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Windowing.AppWindow 
   => control._set(FluentAvalonia.UI.Windowing.AppWindow.IconProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Icon<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IImage> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Windowing.AppWindow 
=> control._setEx(FluentAvalonia.UI.Windowing.AppWindow.IconProperty, ps, () => control.Icon = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Attached Properties ======================//
 // AllowInteractionInTitleBar

/*AttachedPropertyMagicalSetterGenerator*/
public static T AppWindow_AllowInteractionInTitleBar<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.Control
 => control._setEx(FluentAvalonia.UI.Windowing.AppWindow.AllowInteractionInTitleBarProperty, ps, () => FluentAvalonia.UI.Windowing.AppWindow.SetAllowInteractionInTitleBar(control, value), bindingMode, converter, bindingSource);

/*AttachedPropertyBindFromExpressionSetterGenerator*/
public static T AppWindow_AllowInteractionInTitleBar<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.Control 
   => control._set(FluentAvalonia.UI.Windowing.AppWindow.AllowInteractionInTitleBarProperty, func, onChanged, expression);



//================= Styles ======================//
 // Icon

/*ValueStyleSetterGenerator*/
public static Style<T> Icon<T>(this Style<T> style, Avalonia.Media.IImage value) where T : FluentAvalonia.UI.Windowing.AppWindow 
=> style._addSetter(FluentAvalonia.UI.Windowing.AppWindow.IconProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Icon<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Windowing.AppWindow 
=> style._addSetter(FluentAvalonia.UI.Windowing.AppWindow.IconProperty, binding);



}
