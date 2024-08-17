#nullable enable
using AppWindow = FluentAvalonia.UI.Windowing.AppWindow;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using FluentAvalonia.UI.Windowing;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class AppWindowExtensions
{
public static T Icon<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Windowing.AppWindow
   => control._set(FluentAvalonia.UI.Windowing.AppWindow.IconProperty, binding);
public static T Icon<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Windowing.AppWindow
   => control._set(FluentAvalonia.UI.Windowing.AppWindow.IconProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Icon<T>(this T control, Func<Avalonia.Media.IImage> func, Action<Avalonia.Media.IImage>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Windowing.AppWindow
   => control._set(FluentAvalonia.UI.Windowing.AppWindow.IconProperty, func, onChanged, expression);
public static T Icon<T>(this T control, Avalonia.Media.IImage value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Windowing.AppWindow
=> control._setEx(FluentAvalonia.UI.Windowing.AppWindow.IconProperty, ps, () => control.Icon = value, bindingMode, converter, bindingSource);
public static T Icon<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IImage> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Windowing.AppWindow
=> control._setEx(FluentAvalonia.UI.Windowing.AppWindow.IconProperty, ps, () => control.Icon = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

