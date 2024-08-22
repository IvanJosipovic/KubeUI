#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class AppWindow_MarkupExtensions
{
//================= Properties ======================//
 // IconProperty

/*BindFromExpressionSetterGenerator*/
public static T Icon<T>(this T control, Func<Avalonia.Media.IImage> func, Action<Avalonia.Media.IImage>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Windowing.AppWindow
   => control._set(FluentAvalonia.UI.Windowing.AppWindow.IconProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Icon<T>(this T control, Avalonia.Media.IImage value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Windowing.AppWindow
=> control._setEx(FluentAvalonia.UI.Windowing.AppWindow.IconProperty, ps, () => control.Icon = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Icon<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Windowing.AppWindow
   => control._set(FluentAvalonia.UI.Windowing.AppWindow.IconProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Icon<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Windowing.AppWindow
   => control._set(FluentAvalonia.UI.Windowing.AppWindow.IconProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Icon<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IImage> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Windowing.AppWindow
=> control._setEx(FluentAvalonia.UI.Windowing.AppWindow.IconProperty, ps, () => control.Icon = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // IconProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Icon<T>(this Style<T> style, Avalonia.Media.IImage value) where T : FluentAvalonia.UI.Windowing.AppWindow
=> style._addSetter(FluentAvalonia.UI.Windowing.AppWindow.IconProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Icon<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Windowing.AppWindow
=> style._addSetter(FluentAvalonia.UI.Windowing.AppWindow.IconProperty, binding);



}
