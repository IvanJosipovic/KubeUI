#nullable enable
using AppWindowTemplateSettings = FluentAvalonia.UI.Windowing.AppWindowTemplateSettings;
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using FluentAvalonia.UI.Windowing;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class AppWindowTemplateSettingsExtensions
{
public static T TitleBarHeight<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Windowing.AppWindowTemplateSettings
   => control._set(FluentAvalonia.UI.Windowing.AppWindowTemplateSettings.TitleBarHeightProperty, binding);
public static T TitleBarHeight<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Windowing.AppWindowTemplateSettings
   => control._set(FluentAvalonia.UI.Windowing.AppWindowTemplateSettings.TitleBarHeightProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T TitleBarHeight<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Windowing.AppWindowTemplateSettings
   => control._set(FluentAvalonia.UI.Windowing.AppWindowTemplateSettings.TitleBarHeightProperty, func, onChanged, expression);
public static T TitleBarHeight<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Windowing.AppWindowTemplateSettings
=> control._setEx(FluentAvalonia.UI.Windowing.AppWindowTemplateSettings.TitleBarHeightProperty, ps, () => control.TitleBarHeight = value, bindingMode, converter, bindingSource);
public static T TitleBarHeight<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Windowing.AppWindowTemplateSettings
=> control._setEx(FluentAvalonia.UI.Windowing.AppWindowTemplateSettings.TitleBarHeightProperty, ps, () => control.TitleBarHeight = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ContentMargin<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Windowing.AppWindowTemplateSettings
   => control._set(FluentAvalonia.UI.Windowing.AppWindowTemplateSettings.ContentMarginProperty, binding);
public static T ContentMargin<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Windowing.AppWindowTemplateSettings
   => control._set(FluentAvalonia.UI.Windowing.AppWindowTemplateSettings.ContentMarginProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ContentMargin<T>(this T control, Func<Avalonia.Thickness> func, Action<Avalonia.Thickness>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Windowing.AppWindowTemplateSettings
   => control._set(FluentAvalonia.UI.Windowing.AppWindowTemplateSettings.ContentMarginProperty, func, onChanged, expression);
public static T ContentMargin<T>(this T control, Avalonia.Thickness value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Windowing.AppWindowTemplateSettings
=> control._setEx(FluentAvalonia.UI.Windowing.AppWindowTemplateSettings.ContentMarginProperty, ps, () => control.ContentMargin = value, bindingMode, converter, bindingSource);
public static T ContentMargin<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Thickness> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Windowing.AppWindowTemplateSettings
=> control._setEx(FluentAvalonia.UI.Windowing.AppWindowTemplateSettings.ContentMarginProperty, ps, () => control.ContentMargin = converter.TryConvert(value), bindingMode, converter, bindingSource);

public static T ContentMargin<T>(this T control, Double uniformLength = default) where T : FluentAvalonia.UI.Windowing.AppWindowTemplateSettings
   => control._set(() => control.ContentMargin = new Avalonia.Thickness(uniformLength));
public static T ContentMargin<T>(this T control, Double horizontal = default, Double vertical = default) where T : FluentAvalonia.UI.Windowing.AppWindowTemplateSettings
   => control._set(() => control.ContentMargin = new Avalonia.Thickness(horizontal, vertical));
public static T ContentMargin<T>(this T control, Double left = default, Double top = default, Double right = default, Double bottom = default) where T : FluentAvalonia.UI.Windowing.AppWindowTemplateSettings
   => control._set(() => control.ContentMargin = new Avalonia.Thickness(left, top, right, bottom));
public static T IsTitleBarContentVisible<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Windowing.AppWindowTemplateSettings
   => control._set(FluentAvalonia.UI.Windowing.AppWindowTemplateSettings.IsTitleBarContentVisibleProperty, binding);
public static T IsTitleBarContentVisible<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Windowing.AppWindowTemplateSettings
   => control._set(FluentAvalonia.UI.Windowing.AppWindowTemplateSettings.IsTitleBarContentVisibleProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsTitleBarContentVisible<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Windowing.AppWindowTemplateSettings
   => control._set(FluentAvalonia.UI.Windowing.AppWindowTemplateSettings.IsTitleBarContentVisibleProperty, func, onChanged, expression);
public static T IsTitleBarContentVisible<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Windowing.AppWindowTemplateSettings
=> control._setEx(FluentAvalonia.UI.Windowing.AppWindowTemplateSettings.IsTitleBarContentVisibleProperty, ps, () => control.IsTitleBarContentVisible = value, bindingMode, converter, bindingSource);
public static T IsTitleBarContentVisible<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Windowing.AppWindowTemplateSettings
=> control._setEx(FluentAvalonia.UI.Windowing.AppWindowTemplateSettings.IsTitleBarContentVisibleProperty, ps, () => control.IsTitleBarContentVisible = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T WindowIcon<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Windowing.AppWindowTemplateSettings
   => control._set(FluentAvalonia.UI.Windowing.AppWindowTemplateSettings.WindowIconProperty, binding);
public static T WindowIcon<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Windowing.AppWindowTemplateSettings
   => control._set(FluentAvalonia.UI.Windowing.AppWindowTemplateSettings.WindowIconProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T WindowIcon<T>(this T control, Func<Avalonia.Media.IImage> func, Action<Avalonia.Media.IImage>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Windowing.AppWindowTemplateSettings
   => control._set(FluentAvalonia.UI.Windowing.AppWindowTemplateSettings.WindowIconProperty, func, onChanged, expression);
public static T WindowIcon<T>(this T control, Avalonia.Media.IImage value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Windowing.AppWindowTemplateSettings
=> control._setEx(FluentAvalonia.UI.Windowing.AppWindowTemplateSettings.WindowIconProperty, ps, () => control.WindowIcon = value, bindingMode, converter, bindingSource);
public static T WindowIcon<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IImage> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Windowing.AppWindowTemplateSettings
=> control._setEx(FluentAvalonia.UI.Windowing.AppWindowTemplateSettings.WindowIconProperty, ps, () => control.WindowIcon = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

