#nullable enable
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using ItemsRepeater = FluentAvalonia.UI.Controls.ItemsRepeater;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class ItemsRepeaterExtensions
{
public static T VerticalCacheLength<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ItemsRepeater
   => control._set(FluentAvalonia.UI.Controls.ItemsRepeater.VerticalCacheLengthProperty, binding);
public static T VerticalCacheLength<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
   => control._set(FluentAvalonia.UI.Controls.ItemsRepeater.VerticalCacheLengthProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T VerticalCacheLength<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
   => control._set(FluentAvalonia.UI.Controls.ItemsRepeater.VerticalCacheLengthProperty, func, onChanged, expression);
public static T VerticalCacheLength<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> control._setEx(FluentAvalonia.UI.Controls.ItemsRepeater.VerticalCacheLengthProperty, ps, () => control.VerticalCacheLength = value, bindingMode, converter, bindingSource);
public static T VerticalCacheLength<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> control._setEx(FluentAvalonia.UI.Controls.ItemsRepeater.VerticalCacheLengthProperty, ps, () => control.VerticalCacheLength = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T HorizontalCacheLength<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ItemsRepeater
   => control._set(FluentAvalonia.UI.Controls.ItemsRepeater.HorizontalCacheLengthProperty, binding);
public static T HorizontalCacheLength<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
   => control._set(FluentAvalonia.UI.Controls.ItemsRepeater.HorizontalCacheLengthProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T HorizontalCacheLength<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
   => control._set(FluentAvalonia.UI.Controls.ItemsRepeater.HorizontalCacheLengthProperty, func, onChanged, expression);
public static T HorizontalCacheLength<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> control._setEx(FluentAvalonia.UI.Controls.ItemsRepeater.HorizontalCacheLengthProperty, ps, () => control.HorizontalCacheLength = value, bindingMode, converter, bindingSource);
public static T HorizontalCacheLength<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> control._setEx(FluentAvalonia.UI.Controls.ItemsRepeater.HorizontalCacheLengthProperty, ps, () => control.HorizontalCacheLength = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Layout<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ItemsRepeater
   => control._set(FluentAvalonia.UI.Controls.ItemsRepeater.LayoutProperty, binding);
public static T Layout<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
   => control._set(FluentAvalonia.UI.Controls.ItemsRepeater.LayoutProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Layout<T>(this T control, Func<FluentAvalonia.UI.Controls.Layout> func, Action<FluentAvalonia.UI.Controls.Layout>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
   => control._set(FluentAvalonia.UI.Controls.ItemsRepeater.LayoutProperty, func, onChanged, expression);
public static T Layout<T>(this T control, FluentAvalonia.UI.Controls.Layout value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> control._setEx(FluentAvalonia.UI.Controls.ItemsRepeater.LayoutProperty, ps, () => control.Layout = value, bindingMode, converter, bindingSource);
public static T Layout<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.Layout> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> control._setEx(FluentAvalonia.UI.Controls.ItemsRepeater.LayoutProperty, ps, () => control.Layout = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ItemsSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ItemsRepeater
   => control._set(FluentAvalonia.UI.Controls.ItemsRepeater.ItemsSourceProperty, binding);
public static T ItemsSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
   => control._set(FluentAvalonia.UI.Controls.ItemsRepeater.ItemsSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ItemsSource<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
   => control._set(FluentAvalonia.UI.Controls.ItemsRepeater.ItemsSourceProperty, func, onChanged, expression);
public static T ItemsSource<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> control._setEx(FluentAvalonia.UI.Controls.ItemsRepeater.ItemsSourceProperty, ps, () => control.ItemsSource = value, bindingMode, converter, bindingSource);
public static T ItemsSource<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> control._setEx(FluentAvalonia.UI.Controls.ItemsRepeater.ItemsSourceProperty, ps, () => control.ItemsSource = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ItemTemplate<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ItemsRepeater
   => control._set(FluentAvalonia.UI.Controls.ItemsRepeater.ItemTemplateProperty, binding);
public static T ItemTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
   => control._set(FluentAvalonia.UI.Controls.ItemsRepeater.ItemTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ItemTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
   => control._set(FluentAvalonia.UI.Controls.ItemsRepeater.ItemTemplateProperty, func, onChanged, expression);
public static T ItemTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> control._setEx(FluentAvalonia.UI.Controls.ItemsRepeater.ItemTemplateProperty, ps, () => control.ItemTemplate = value, bindingMode, converter, bindingSource);
public static T ItemTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> control._setEx(FluentAvalonia.UI.Controls.ItemsRepeater.ItemTemplateProperty, ps, () => control.ItemTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ItemTransitionProvider<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ItemsRepeater
   => control._set(FluentAvalonia.UI.Controls.ItemsRepeater.ItemTransitionProviderProperty, binding);
public static T ItemTransitionProvider<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
   => control._set(FluentAvalonia.UI.Controls.ItemsRepeater.ItemTransitionProviderProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ItemTransitionProvider<T>(this T control, Func<FluentAvalonia.UI.Controls.ItemCollectionTransitionProvider> func, Action<FluentAvalonia.UI.Controls.ItemCollectionTransitionProvider>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
   => control._set(FluentAvalonia.UI.Controls.ItemsRepeater.ItemTransitionProviderProperty, func, onChanged, expression);
public static T ItemTransitionProvider<T>(this T control, FluentAvalonia.UI.Controls.ItemCollectionTransitionProvider value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> control._setEx(FluentAvalonia.UI.Controls.ItemsRepeater.ItemTransitionProviderProperty, ps, () => control.ItemTransitionProvider = value, bindingMode, converter, bindingSource);
public static T ItemTransitionProvider<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.ItemCollectionTransitionProvider> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> control._setEx(FluentAvalonia.UI.Controls.ItemsRepeater.ItemTransitionProviderProperty, ps, () => control.ItemTransitionProvider = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

