#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using CollectionViewSource = FluentAvalonia.UI.Data.CollectionViewSource;
using FluentAvalonia.UI.Data;
using System;
using System.Collections;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class CollectionViewSourceExtensions
{
public static T IsSourceGrouped<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Data.CollectionViewSource
   => control._set(FluentAvalonia.UI.Data.CollectionViewSource.IsSourceGroupedProperty, binding);
public static T IsSourceGrouped<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
   => control._set(FluentAvalonia.UI.Data.CollectionViewSource.IsSourceGroupedProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsSourceGrouped<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
   => control._set(FluentAvalonia.UI.Data.CollectionViewSource.IsSourceGroupedProperty, func, onChanged, expression);
public static T IsSourceGrouped<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
=> control._setEx(FluentAvalonia.UI.Data.CollectionViewSource.IsSourceGroupedProperty, ps, () => control.IsSourceGrouped = value, bindingMode, converter, bindingSource);
public static T IsSourceGrouped<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
=> control._setEx(FluentAvalonia.UI.Data.CollectionViewSource.IsSourceGroupedProperty, ps, () => control.IsSourceGrouped = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ItemsBinding<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Data.CollectionViewSource
   => control._set(FluentAvalonia.UI.Data.CollectionViewSource.ItemsBindingProperty, binding);
public static T ItemsBinding<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
   => control._set(FluentAvalonia.UI.Data.CollectionViewSource.ItemsBindingProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ItemsBinding<T>(this T control, Func<Avalonia.Data.IBinding> func, Action<Avalonia.Data.IBinding>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
   => control._set(FluentAvalonia.UI.Data.CollectionViewSource.ItemsBindingProperty, func, onChanged, expression);
public static T ItemsBinding<T>(this T control, Avalonia.Data.IBinding value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
=> control._setEx(FluentAvalonia.UI.Data.CollectionViewSource.ItemsBindingProperty, ps, () => control.ItemsBinding = value, bindingMode, converter, bindingSource);
public static T ItemsBinding<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Data.IBinding> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
=> control._setEx(FluentAvalonia.UI.Data.CollectionViewSource.ItemsBindingProperty, ps, () => control.ItemsBinding = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Source<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Data.CollectionViewSource
   => control._set(FluentAvalonia.UI.Data.CollectionViewSource.SourceProperty, binding);
public static T Source<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
   => control._set(FluentAvalonia.UI.Data.CollectionViewSource.SourceProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Source<T>(this T control, Func<System.Collections.IEnumerable> func, Action<System.Collections.IEnumerable>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
   => control._set(FluentAvalonia.UI.Data.CollectionViewSource.SourceProperty, func, onChanged, expression);
public static T Source<T>(this T control, System.Collections.IEnumerable value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
=> control._setEx(FluentAvalonia.UI.Data.CollectionViewSource.SourceProperty, ps, () => control.Source = value, bindingMode, converter, bindingSource);
public static T Source<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.IEnumerable> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
=> control._setEx(FluentAvalonia.UI.Data.CollectionViewSource.SourceProperty, ps, () => control.Source = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Filter<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Data.CollectionViewSource
   => control._set(FluentAvalonia.UI.Data.CollectionViewSource.FilterProperty, binding);
public static T Filter<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
   => control._set(FluentAvalonia.UI.Data.CollectionViewSource.FilterProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Filter<T>(this T control, Func<System.Predicate<System.Object>> func, Action<System.Predicate<System.Object>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
   => control._set(FluentAvalonia.UI.Data.CollectionViewSource.FilterProperty, func, onChanged, expression);
public static T Filter<T>(this T control, System.Predicate<System.Object> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
=> control._setEx(FluentAvalonia.UI.Data.CollectionViewSource.FilterProperty, ps, () => control.Filter = value, bindingMode, converter, bindingSource);
public static T Filter<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Predicate<System.Object>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
=> control._setEx(FluentAvalonia.UI.Data.CollectionViewSource.FilterProperty, ps, () => control.Filter = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsLiveShapingEnabled<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Data.CollectionViewSource
   => control._set(FluentAvalonia.UI.Data.CollectionViewSource.IsLiveShapingEnabledProperty, binding);
public static T IsLiveShapingEnabled<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
   => control._set(FluentAvalonia.UI.Data.CollectionViewSource.IsLiveShapingEnabledProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsLiveShapingEnabled<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
   => control._set(FluentAvalonia.UI.Data.CollectionViewSource.IsLiveShapingEnabledProperty, func, onChanged, expression);
public static T IsLiveShapingEnabled<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
=> control._setEx(FluentAvalonia.UI.Data.CollectionViewSource.IsLiveShapingEnabledProperty, ps, () => control.IsLiveShapingEnabled = value, bindingMode, converter, bindingSource);
public static T IsLiveShapingEnabled<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
=> control._setEx(FluentAvalonia.UI.Data.CollectionViewSource.IsLiveShapingEnabledProperty, ps, () => control.IsLiveShapingEnabled = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

