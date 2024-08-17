#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using BitmapIcon = FluentAvalonia.UI.Controls.BitmapIcon;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class BitmapIconExtensions
{
public static T UriSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.BitmapIcon
   => control._set(FluentAvalonia.UI.Controls.BitmapIcon.UriSourceProperty, binding);
public static T UriSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.BitmapIcon
   => control._set(FluentAvalonia.UI.Controls.BitmapIcon.UriSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T UriSource<T>(this T control, Func<System.Uri> func, Action<System.Uri>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.BitmapIcon
   => control._set(FluentAvalonia.UI.Controls.BitmapIcon.UriSourceProperty, func, onChanged, expression);
public static T UriSource<T>(this T control, System.Uri value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.BitmapIcon
=> control._setEx(FluentAvalonia.UI.Controls.BitmapIcon.UriSourceProperty, ps, () => control.UriSource = value, bindingMode, converter, bindingSource);
public static T UriSource<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Uri> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.BitmapIcon
=> control._setEx(FluentAvalonia.UI.Controls.BitmapIcon.UriSourceProperty, ps, () => control.UriSource = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ShowAsMonochrome<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.BitmapIcon
   => control._set(FluentAvalonia.UI.Controls.BitmapIcon.ShowAsMonochromeProperty, binding);
public static T ShowAsMonochrome<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.BitmapIcon
   => control._set(FluentAvalonia.UI.Controls.BitmapIcon.ShowAsMonochromeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ShowAsMonochrome<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.BitmapIcon
   => control._set(FluentAvalonia.UI.Controls.BitmapIcon.ShowAsMonochromeProperty, func, onChanged, expression);
public static T ShowAsMonochrome<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.BitmapIcon
=> control._setEx(FluentAvalonia.UI.Controls.BitmapIcon.ShowAsMonochromeProperty, ps, () => control.ShowAsMonochrome = value, bindingMode, converter, bindingSource);
public static T ShowAsMonochrome<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.BitmapIcon
=> control._setEx(FluentAvalonia.UI.Controls.BitmapIcon.ShowAsMonochromeProperty, ps, () => control.ShowAsMonochrome = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

