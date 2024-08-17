#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using NavigationViewItem = FluentAvalonia.UI.Controls.NavigationViewItem;
using System;
using System.Collections;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class NavigationViewItemExtensions
{
public static T HasUnrealizedChildren<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.HasUnrealizedChildrenProperty, binding);
public static T HasUnrealizedChildren<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.HasUnrealizedChildrenProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T HasUnrealizedChildren<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.HasUnrealizedChildrenProperty, func, onChanged, expression);
public static T HasUnrealizedChildren<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> control._setEx(FluentAvalonia.UI.Controls.NavigationViewItem.HasUnrealizedChildrenProperty, ps, () => control.HasUnrealizedChildren = value, bindingMode, converter, bindingSource);
public static T HasUnrealizedChildren<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> control._setEx(FluentAvalonia.UI.Controls.NavigationViewItem.HasUnrealizedChildrenProperty, ps, () => control.HasUnrealizedChildren = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IconSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.IconSourceProperty, binding);
public static T IconSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.IconSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IconSource<T>(this T control, Func<FluentAvalonia.UI.Controls.IconSource> func, Action<FluentAvalonia.UI.Controls.IconSource>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.IconSourceProperty, func, onChanged, expression);
public static T IconSource<T>(this T control, FluentAvalonia.UI.Controls.IconSource value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> control._setEx(FluentAvalonia.UI.Controls.NavigationViewItem.IconSourceProperty, ps, () => control.IconSource = value, bindingMode, converter, bindingSource);
public static T IconSource<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.IconSource> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> control._setEx(FluentAvalonia.UI.Controls.NavigationViewItem.IconSourceProperty, ps, () => control.IconSource = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsChildSelected<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.IsChildSelectedProperty, binding);
public static T IsChildSelected<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.IsChildSelectedProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsChildSelected<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.IsChildSelectedProperty, func, onChanged, expression);
public static T IsChildSelected<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> control._setEx(FluentAvalonia.UI.Controls.NavigationViewItem.IsChildSelectedProperty, ps, () => control.IsChildSelected = value, bindingMode, converter, bindingSource);
public static T IsChildSelected<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> control._setEx(FluentAvalonia.UI.Controls.NavigationViewItem.IsChildSelectedProperty, ps, () => control.IsChildSelected = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsExpanded<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.IsExpandedProperty, binding);
public static T IsExpanded<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.IsExpandedProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsExpanded<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.IsExpandedProperty, func, onChanged, expression);
public static T IsExpanded<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> control._setEx(FluentAvalonia.UI.Controls.NavigationViewItem.IsExpandedProperty, ps, () => control.IsExpanded = value, bindingMode, converter, bindingSource);
public static T IsExpanded<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> control._setEx(FluentAvalonia.UI.Controls.NavigationViewItem.IsExpandedProperty, ps, () => control.IsExpanded = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T MenuItemsSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.MenuItemsSourceProperty, binding);
public static T MenuItemsSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.MenuItemsSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T MenuItemsSource<T>(this T control, Func<System.Collections.IEnumerable> func, Action<System.Collections.IEnumerable>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.MenuItemsSourceProperty, func, onChanged, expression);
public static T MenuItemsSource<T>(this T control, System.Collections.IEnumerable value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> control._setEx(FluentAvalonia.UI.Controls.NavigationViewItem.MenuItemsSourceProperty, ps, () => control.MenuItemsSource = value, bindingMode, converter, bindingSource);
public static T MenuItemsSource<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.IEnumerable> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> control._setEx(FluentAvalonia.UI.Controls.NavigationViewItem.MenuItemsSourceProperty, ps, () => control.MenuItemsSource = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SelectsOnInvoked<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.SelectsOnInvokedProperty, binding);
public static T SelectsOnInvoked<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.SelectsOnInvokedProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SelectsOnInvoked<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.SelectsOnInvokedProperty, func, onChanged, expression);
public static T SelectsOnInvoked<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> control._setEx(FluentAvalonia.UI.Controls.NavigationViewItem.SelectsOnInvokedProperty, ps, () => control.SelectsOnInvoked = value, bindingMode, converter, bindingSource);
public static T SelectsOnInvoked<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> control._setEx(FluentAvalonia.UI.Controls.NavigationViewItem.SelectsOnInvokedProperty, ps, () => control.SelectsOnInvoked = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T InfoBadge<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.InfoBadgeProperty, binding);
public static T InfoBadge<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.InfoBadgeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T InfoBadge<T>(this T control, Func<FluentAvalonia.UI.Controls.InfoBadge> func, Action<FluentAvalonia.UI.Controls.InfoBadge>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.InfoBadgeProperty, func, onChanged, expression);
public static T InfoBadge<T>(this T control, FluentAvalonia.UI.Controls.InfoBadge value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> control._setEx(FluentAvalonia.UI.Controls.NavigationViewItem.InfoBadgeProperty, ps, () => control.InfoBadge = value, bindingMode, converter, bindingSource);
public static T InfoBadge<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.InfoBadge> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> control._setEx(FluentAvalonia.UI.Controls.NavigationViewItem.InfoBadgeProperty, ps, () => control.InfoBadge = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

