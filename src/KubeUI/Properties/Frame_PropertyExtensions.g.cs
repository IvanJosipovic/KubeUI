#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using Frame = FluentAvalonia.UI.Controls.Frame;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class FrameExtensions
{
public static T SourcePageType<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.Frame
   => control._set(FluentAvalonia.UI.Controls.Frame.SourcePageTypeProperty, binding);
public static T SourcePageType<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.Frame
   => control._set(FluentAvalonia.UI.Controls.Frame.SourcePageTypeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SourcePageType<T>(this T control, Func<System.Type> func, Action<System.Type>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.Frame
   => control._set(FluentAvalonia.UI.Controls.Frame.SourcePageTypeProperty, func, onChanged, expression);
public static T SourcePageType<T>(this T control, System.Type value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Frame
=> control._setEx(FluentAvalonia.UI.Controls.Frame.SourcePageTypeProperty, ps, () => control.SourcePageType = value, bindingMode, converter, bindingSource);
public static T SourcePageType<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Type> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Frame
=> control._setEx(FluentAvalonia.UI.Controls.Frame.SourcePageTypeProperty, ps, () => control.SourcePageType = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T CacheSize<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.Frame
   => control._set(FluentAvalonia.UI.Controls.Frame.CacheSizeProperty, binding);
public static T CacheSize<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.Frame
   => control._set(FluentAvalonia.UI.Controls.Frame.CacheSizeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CacheSize<T>(this T control, Func<System.Int32> func, Action<System.Int32>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.Frame
   => control._set(FluentAvalonia.UI.Controls.Frame.CacheSizeProperty, func, onChanged, expression);
public static T CacheSize<T>(this T control, System.Int32 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Frame
=> control._setEx(FluentAvalonia.UI.Controls.Frame.CacheSizeProperty, ps, () => control.CacheSize = value, bindingMode, converter, bindingSource);
public static T CacheSize<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Int32> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Frame
=> control._setEx(FluentAvalonia.UI.Controls.Frame.CacheSizeProperty, ps, () => control.CacheSize = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsNavigationStackEnabled<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.Frame
   => control._set(FluentAvalonia.UI.Controls.Frame.IsNavigationStackEnabledProperty, binding);
public static T IsNavigationStackEnabled<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.Frame
   => control._set(FluentAvalonia.UI.Controls.Frame.IsNavigationStackEnabledProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsNavigationStackEnabled<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.Frame
   => control._set(FluentAvalonia.UI.Controls.Frame.IsNavigationStackEnabledProperty, func, onChanged, expression);
public static T IsNavigationStackEnabled<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Frame
=> control._setEx(FluentAvalonia.UI.Controls.Frame.IsNavigationStackEnabledProperty, ps, () => control.IsNavigationStackEnabled = value, bindingMode, converter, bindingSource);
public static T IsNavigationStackEnabled<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Frame
=> control._setEx(FluentAvalonia.UI.Controls.Frame.IsNavigationStackEnabledProperty, ps, () => control.IsNavigationStackEnabled = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T NavigationPageFactory<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.Frame
   => control._set(FluentAvalonia.UI.Controls.Frame.NavigationPageFactoryProperty, binding);
public static T NavigationPageFactory<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.Frame
   => control._set(FluentAvalonia.UI.Controls.Frame.NavigationPageFactoryProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T NavigationPageFactory<T>(this T control, Func<FluentAvalonia.UI.Controls.INavigationPageFactory> func, Action<FluentAvalonia.UI.Controls.INavigationPageFactory>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.Frame
   => control._set(FluentAvalonia.UI.Controls.Frame.NavigationPageFactoryProperty, func, onChanged, expression);
public static T NavigationPageFactory<T>(this T control, FluentAvalonia.UI.Controls.INavigationPageFactory value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Frame
=> control._setEx(FluentAvalonia.UI.Controls.Frame.NavigationPageFactoryProperty, ps, () => control.NavigationPageFactory = value, bindingMode, converter, bindingSource);
public static T NavigationPageFactory<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.INavigationPageFactory> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Frame
=> control._setEx(FluentAvalonia.UI.Controls.Frame.NavigationPageFactoryProperty, ps, () => control.NavigationPageFactory = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

