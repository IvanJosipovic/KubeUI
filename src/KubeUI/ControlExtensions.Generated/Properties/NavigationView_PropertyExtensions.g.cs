#nullable enable
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using NavigationView = FluentAvalonia.UI.Controls.NavigationView;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class NavigationViewExtensions
{
public static T AlwaysShowHeader<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.AlwaysShowHeaderProperty, binding);
public static T AlwaysShowHeader<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.AlwaysShowHeaderProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T AlwaysShowHeader<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.AlwaysShowHeaderProperty, func, onChanged, expression);
public static T AlwaysShowHeader<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.AlwaysShowHeaderProperty, ps, () => control.AlwaysShowHeader = value, bindingMode, converter, bindingSource);
public static T AlwaysShowHeader<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.AlwaysShowHeaderProperty, ps, () => control.AlwaysShowHeader = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T AutoCompleteBox<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.AutoCompleteBoxProperty, binding);
public static T AutoCompleteBox<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.AutoCompleteBoxProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T AutoCompleteBox<T>(this T control, Func<Avalonia.Controls.AutoCompleteBox> func, Action<Avalonia.Controls.AutoCompleteBox>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.AutoCompleteBoxProperty, func, onChanged, expression);
public static T AutoCompleteBox<T>(this T control, Avalonia.Controls.AutoCompleteBox value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.AutoCompleteBoxProperty, ps, () => control.AutoCompleteBox = value, bindingMode, converter, bindingSource);
public static T AutoCompleteBox<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.AutoCompleteBox> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.AutoCompleteBoxProperty, ps, () => control.AutoCompleteBox = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T CompactModeThresholdWidth<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.CompactModeThresholdWidthProperty, binding);
public static T CompactModeThresholdWidth<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.CompactModeThresholdWidthProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CompactModeThresholdWidth<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.CompactModeThresholdWidthProperty, func, onChanged, expression);
public static T CompactModeThresholdWidth<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.CompactModeThresholdWidthProperty, ps, () => control.CompactModeThresholdWidth = value, bindingMode, converter, bindingSource);
public static T CompactModeThresholdWidth<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.CompactModeThresholdWidthProperty, ps, () => control.CompactModeThresholdWidth = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T CompactPaneLength<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.CompactPaneLengthProperty, binding);
public static T CompactPaneLength<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.CompactPaneLengthProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CompactPaneLength<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.CompactPaneLengthProperty, func, onChanged, expression);
public static T CompactPaneLength<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.CompactPaneLengthProperty, ps, () => control.CompactPaneLength = value, bindingMode, converter, bindingSource);
public static T CompactPaneLength<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.CompactPaneLengthProperty, ps, () => control.CompactPaneLength = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ContentOverlay<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.ContentOverlayProperty, binding);
public static T ContentOverlay<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.ContentOverlayProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ContentOverlay<T>(this T control, Func<Avalonia.Controls.Control> func, Action<Avalonia.Controls.Control>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.ContentOverlayProperty, func, onChanged, expression);
public static T ContentOverlay<T>(this T control, Avalonia.Controls.Control value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.ContentOverlayProperty, ps, () => control.ContentOverlay = value, bindingMode, converter, bindingSource);
public static T ContentOverlay<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Control> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.ContentOverlayProperty, ps, () => control.ContentOverlay = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ExpandedModeThresholdWidth<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.ExpandedModeThresholdWidthProperty, binding);
public static T ExpandedModeThresholdWidth<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.ExpandedModeThresholdWidthProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ExpandedModeThresholdWidth<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.ExpandedModeThresholdWidthProperty, func, onChanged, expression);
public static T ExpandedModeThresholdWidth<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.ExpandedModeThresholdWidthProperty, ps, () => control.ExpandedModeThresholdWidth = value, bindingMode, converter, bindingSource);
public static T ExpandedModeThresholdWidth<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.ExpandedModeThresholdWidthProperty, ps, () => control.ExpandedModeThresholdWidth = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T FooterMenuItemsSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.FooterMenuItemsSourceProperty, binding);
public static T FooterMenuItemsSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.FooterMenuItemsSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T FooterMenuItemsSource<T>(this T control, Func<System.Collections.IEnumerable> func, Action<System.Collections.IEnumerable>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.FooterMenuItemsSourceProperty, func, onChanged, expression);
public static T FooterMenuItemsSource<T>(this T control, System.Collections.IEnumerable value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.FooterMenuItemsSourceProperty, ps, () => control.FooterMenuItemsSource = value, bindingMode, converter, bindingSource);
public static T FooterMenuItemsSource<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.IEnumerable> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.FooterMenuItemsSourceProperty, ps, () => control.FooterMenuItemsSource = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsBackButtonVisible<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.IsBackButtonVisibleProperty, binding);
public static T IsBackButtonVisible<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.IsBackButtonVisibleProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsBackButtonVisible<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.IsBackButtonVisibleProperty, func, onChanged, expression);
public static T IsBackButtonVisible<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.IsBackButtonVisibleProperty, ps, () => control.IsBackButtonVisible = value, bindingMode, converter, bindingSource);
public static T IsBackButtonVisible<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.IsBackButtonVisibleProperty, ps, () => control.IsBackButtonVisible = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsBackEnabled<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.IsBackEnabledProperty, binding);
public static T IsBackEnabled<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.IsBackEnabledProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsBackEnabled<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.IsBackEnabledProperty, func, onChanged, expression);
public static T IsBackEnabled<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.IsBackEnabledProperty, ps, () => control.IsBackEnabled = value, bindingMode, converter, bindingSource);
public static T IsBackEnabled<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.IsBackEnabledProperty, ps, () => control.IsBackEnabled = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsPaneOpen<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.IsPaneOpenProperty, binding);
public static T IsPaneOpen<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.IsPaneOpenProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsPaneOpen<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.IsPaneOpenProperty, func, onChanged, expression);
public static T IsPaneOpen<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.IsPaneOpenProperty, ps, () => control.IsPaneOpen = value, bindingMode, converter, bindingSource);
public static T IsPaneOpen<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.IsPaneOpenProperty, ps, () => control.IsPaneOpen = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsPaneToggleButtonVisible<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.IsPaneToggleButtonVisibleProperty, binding);
public static T IsPaneToggleButtonVisible<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.IsPaneToggleButtonVisibleProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsPaneToggleButtonVisible<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.IsPaneToggleButtonVisibleProperty, func, onChanged, expression);
public static T IsPaneToggleButtonVisible<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.IsPaneToggleButtonVisibleProperty, ps, () => control.IsPaneToggleButtonVisible = value, bindingMode, converter, bindingSource);
public static T IsPaneToggleButtonVisible<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.IsPaneToggleButtonVisibleProperty, ps, () => control.IsPaneToggleButtonVisible = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsPaneVisible<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.IsPaneVisibleProperty, binding);
public static T IsPaneVisible<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.IsPaneVisibleProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsPaneVisible<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.IsPaneVisibleProperty, func, onChanged, expression);
public static T IsPaneVisible<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.IsPaneVisibleProperty, ps, () => control.IsPaneVisible = value, bindingMode, converter, bindingSource);
public static T IsPaneVisible<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.IsPaneVisibleProperty, ps, () => control.IsPaneVisible = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsSettingsVisible<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.IsSettingsVisibleProperty, binding);
public static T IsSettingsVisible<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.IsSettingsVisibleProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsSettingsVisible<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.IsSettingsVisibleProperty, func, onChanged, expression);
public static T IsSettingsVisible<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.IsSettingsVisibleProperty, ps, () => control.IsSettingsVisible = value, bindingMode, converter, bindingSource);
public static T IsSettingsVisible<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.IsSettingsVisibleProperty, ps, () => control.IsSettingsVisible = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T MenuItems<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.MenuItemsProperty, binding);
public static T MenuItems<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.MenuItemsProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T MenuItems<T>(this T control, Func<System.Collections.Generic.IList<System.Object>> func, Action<System.Collections.Generic.IList<System.Object>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.MenuItemsProperty, func, onChanged, expression);
public static T MenuItems<T>(this T control, System.Collections.Generic.IList<System.Object> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.MenuItemsProperty, ps, () => control.MenuItems = value, bindingMode, converter, bindingSource);
public static T MenuItems<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.Generic.IList<System.Object>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.MenuItemsProperty, ps, () => control.MenuItems = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T MenuItemsSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.MenuItemsSourceProperty, binding);
public static T MenuItemsSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.MenuItemsSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T MenuItemsSource<T>(this T control, Func<System.Collections.IEnumerable> func, Action<System.Collections.IEnumerable>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.MenuItemsSourceProperty, func, onChanged, expression);
public static T MenuItemsSource<T>(this T control, System.Collections.IEnumerable value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.MenuItemsSourceProperty, ps, () => control.MenuItemsSource = value, bindingMode, converter, bindingSource);
public static T MenuItemsSource<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.IEnumerable> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.MenuItemsSourceProperty, ps, () => control.MenuItemsSource = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T MenuItemTemplate<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.MenuItemTemplateProperty, binding);
public static T MenuItemTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.MenuItemTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T MenuItemTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.MenuItemTemplateProperty, func, onChanged, expression);
public static T MenuItemTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.MenuItemTemplateProperty, ps, () => control.MenuItemTemplate = value, bindingMode, converter, bindingSource);
public static T MenuItemTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.MenuItemTemplateProperty, ps, () => control.MenuItemTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T MenuItemTemplateSelector<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.MenuItemTemplateSelectorProperty, binding);
public static T MenuItemTemplateSelector<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.MenuItemTemplateSelectorProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T MenuItemTemplateSelector<T>(this T control, Func<FluentAvalonia.UI.Controls.DataTemplateSelector> func, Action<FluentAvalonia.UI.Controls.DataTemplateSelector>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.MenuItemTemplateSelectorProperty, func, onChanged, expression);
public static T MenuItemTemplateSelector<T>(this T control, FluentAvalonia.UI.Controls.DataTemplateSelector value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.MenuItemTemplateSelectorProperty, ps, () => control.MenuItemTemplateSelector = value, bindingMode, converter, bindingSource);
public static T MenuItemTemplateSelector<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.DataTemplateSelector> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.MenuItemTemplateSelectorProperty, ps, () => control.MenuItemTemplateSelector = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T OpenPaneLength<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.OpenPaneLengthProperty, binding);
public static T OpenPaneLength<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.OpenPaneLengthProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T OpenPaneLength<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.OpenPaneLengthProperty, func, onChanged, expression);
public static T OpenPaneLength<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.OpenPaneLengthProperty, ps, () => control.OpenPaneLength = value, bindingMode, converter, bindingSource);
public static T OpenPaneLength<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.OpenPaneLengthProperty, ps, () => control.OpenPaneLength = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T PaneCustomContent<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.PaneCustomContentProperty, binding);
public static T PaneCustomContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.PaneCustomContentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T PaneCustomContent<T>(this T control, Func<Avalonia.Controls.Control> func, Action<Avalonia.Controls.Control>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.PaneCustomContentProperty, func, onChanged, expression);
public static T PaneCustomContent<T>(this T control, Avalonia.Controls.Control value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.PaneCustomContentProperty, ps, () => control.PaneCustomContent = value, bindingMode, converter, bindingSource);
public static T PaneCustomContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Control> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.PaneCustomContentProperty, ps, () => control.PaneCustomContent = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T PaneDisplayMode<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.PaneDisplayModeProperty, binding);
public static T PaneDisplayMode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.PaneDisplayModeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T PaneDisplayMode<T>(this T control, Func<FluentAvalonia.UI.Controls.NavigationViewPaneDisplayMode> func, Action<FluentAvalonia.UI.Controls.NavigationViewPaneDisplayMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.PaneDisplayModeProperty, func, onChanged, expression);
public static T PaneDisplayMode<T>(this T control, FluentAvalonia.UI.Controls.NavigationViewPaneDisplayMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.PaneDisplayModeProperty, ps, () => control.PaneDisplayMode = value, bindingMode, converter, bindingSource);
public static T PaneDisplayMode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.NavigationViewPaneDisplayMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.PaneDisplayModeProperty, ps, () => control.PaneDisplayMode = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T PaneFooter<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.PaneFooterProperty, binding);
public static T PaneFooter<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.PaneFooterProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T PaneFooter<T>(this T control, Func<Avalonia.Controls.Control> func, Action<Avalonia.Controls.Control>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.PaneFooterProperty, func, onChanged, expression);
public static T PaneFooter<T>(this T control, Avalonia.Controls.Control value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.PaneFooterProperty, ps, () => control.PaneFooter = value, bindingMode, converter, bindingSource);
public static T PaneFooter<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Control> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.PaneFooterProperty, ps, () => control.PaneFooter = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T PaneHeader<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.PaneHeaderProperty, binding);
public static T PaneHeader<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.PaneHeaderProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T PaneHeader<T>(this T control, Func<Avalonia.Controls.Control> func, Action<Avalonia.Controls.Control>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.PaneHeaderProperty, func, onChanged, expression);
public static T PaneHeader<T>(this T control, Avalonia.Controls.Control value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.PaneHeaderProperty, ps, () => control.PaneHeader = value, bindingMode, converter, bindingSource);
public static T PaneHeader<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Control> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.PaneHeaderProperty, ps, () => control.PaneHeader = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T PaneTitle<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.PaneTitleProperty, binding);
public static T PaneTitle<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.PaneTitleProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T PaneTitle<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.PaneTitleProperty, func, onChanged, expression);
public static T PaneTitle<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.PaneTitleProperty, ps, () => control.PaneTitle = value, bindingMode, converter, bindingSource);
public static T PaneTitle<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.PaneTitleProperty, ps, () => control.PaneTitle = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SelectedItem<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.SelectedItemProperty, binding);
public static T SelectedItem<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.SelectedItemProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SelectedItem<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.SelectedItemProperty, func, onChanged, expression);
public static T SelectedItem<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.SelectedItemProperty, ps, () => control.SelectedItem = value, bindingMode, converter, bindingSource);
public static T SelectedItem<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.SelectedItemProperty, ps, () => control.SelectedItem = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SelectionFollowsFocus<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.SelectionFollowsFocusProperty, binding);
public static T SelectionFollowsFocus<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.SelectionFollowsFocusProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SelectionFollowsFocus<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.SelectionFollowsFocusProperty, func, onChanged, expression);
public static T SelectionFollowsFocus<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.SelectionFollowsFocusProperty, ps, () => control.SelectionFollowsFocus = value, bindingMode, converter, bindingSource);
public static T SelectionFollowsFocus<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.SelectionFollowsFocusProperty, ps, () => control.SelectionFollowsFocus = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

