#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
[global::System.CodeDom.Compiler.GeneratedCode("AvaloniaExtensionGenerator", "11.1.3.0")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static partial class NavigationView_MarkupExtensions
{
//================= Properties ======================//
 // AlwaysShowHeaderProperty

/*BindFromExpressionSetterGenerator*/
public static T AlwaysShowHeader<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.AlwaysShowHeaderProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T AlwaysShowHeader<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.AlwaysShowHeaderProperty, ps, () => control.AlwaysShowHeader = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T AlwaysShowHeader<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.AlwaysShowHeaderProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T AlwaysShowHeader<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.AlwaysShowHeaderProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T AlwaysShowHeader<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.AlwaysShowHeaderProperty, ps, () => control.AlwaysShowHeader = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // AutoCompleteBoxProperty

/*BindFromExpressionSetterGenerator*/
public static T AutoCompleteBox<T>(this T control, Func<Avalonia.Controls.AutoCompleteBox> func, Action<Avalonia.Controls.AutoCompleteBox>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.AutoCompleteBoxProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T AutoCompleteBox<T>(this T control, Avalonia.Controls.AutoCompleteBox value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.AutoCompleteBoxProperty, ps, () => control.AutoCompleteBox = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T AutoCompleteBox<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.AutoCompleteBoxProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T AutoCompleteBox<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.AutoCompleteBoxProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T AutoCompleteBox<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.AutoCompleteBox> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.AutoCompleteBoxProperty, ps, () => control.AutoCompleteBox = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CompactModeThresholdWidthProperty

/*BindFromExpressionSetterGenerator*/
public static T CompactModeThresholdWidth<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.CompactModeThresholdWidthProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CompactModeThresholdWidth<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.CompactModeThresholdWidthProperty, ps, () => control.CompactModeThresholdWidth = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CompactModeThresholdWidth<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.CompactModeThresholdWidthProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CompactModeThresholdWidth<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.CompactModeThresholdWidthProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CompactModeThresholdWidth<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.CompactModeThresholdWidthProperty, ps, () => control.CompactModeThresholdWidth = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CompactPaneLengthProperty

/*BindFromExpressionSetterGenerator*/
public static T CompactPaneLength<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.CompactPaneLengthProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CompactPaneLength<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.CompactPaneLengthProperty, ps, () => control.CompactPaneLength = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CompactPaneLength<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.CompactPaneLengthProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CompactPaneLength<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.CompactPaneLengthProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CompactPaneLength<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.CompactPaneLengthProperty, ps, () => control.CompactPaneLength = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ContentOverlayProperty

/*BindFromExpressionSetterGenerator*/
public static T ContentOverlay<T>(this T control, Func<Avalonia.Controls.Control> func, Action<Avalonia.Controls.Control>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.ContentOverlayProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ContentOverlay<T>(this T control, Avalonia.Controls.Control value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.ContentOverlayProperty, ps, () => control.ContentOverlay = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ContentOverlay<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.ContentOverlayProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ContentOverlay<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.ContentOverlayProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ContentOverlay<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Control> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.ContentOverlayProperty, ps, () => control.ContentOverlay = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ExpandedModeThresholdWidthProperty

/*BindFromExpressionSetterGenerator*/
public static T ExpandedModeThresholdWidth<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.ExpandedModeThresholdWidthProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ExpandedModeThresholdWidth<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.ExpandedModeThresholdWidthProperty, ps, () => control.ExpandedModeThresholdWidth = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ExpandedModeThresholdWidth<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.ExpandedModeThresholdWidthProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ExpandedModeThresholdWidth<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.ExpandedModeThresholdWidthProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ExpandedModeThresholdWidth<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.ExpandedModeThresholdWidthProperty, ps, () => control.ExpandedModeThresholdWidth = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // FooterMenuItemsSourceProperty

/*BindFromExpressionSetterGenerator*/
public static T FooterMenuItemsSource<T>(this T control, Func<System.Collections.IEnumerable> func, Action<System.Collections.IEnumerable>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.FooterMenuItemsSourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T FooterMenuItemsSource<T>(this T control, System.Collections.IEnumerable value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.FooterMenuItemsSourceProperty, ps, () => control.FooterMenuItemsSource = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T FooterMenuItemsSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.FooterMenuItemsSourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T FooterMenuItemsSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.FooterMenuItemsSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T FooterMenuItemsSource<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.IEnumerable> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.FooterMenuItemsSourceProperty, ps, () => control.FooterMenuItemsSource = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsBackButtonVisibleProperty

/*BindFromExpressionSetterGenerator*/
public static T IsBackButtonVisible<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.IsBackButtonVisibleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsBackButtonVisible<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.IsBackButtonVisibleProperty, ps, () => control.IsBackButtonVisible = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsBackButtonVisible<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.IsBackButtonVisibleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsBackButtonVisible<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.IsBackButtonVisibleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsBackButtonVisible<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.IsBackButtonVisibleProperty, ps, () => control.IsBackButtonVisible = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsBackEnabledProperty

/*BindFromExpressionSetterGenerator*/
public static T IsBackEnabled<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.IsBackEnabledProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsBackEnabled<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.IsBackEnabledProperty, ps, () => control.IsBackEnabled = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsBackEnabled<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.IsBackEnabledProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsBackEnabled<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.IsBackEnabledProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsBackEnabled<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.IsBackEnabledProperty, ps, () => control.IsBackEnabled = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsPaneOpenProperty

/*BindFromExpressionSetterGenerator*/
public static T IsPaneOpen<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.IsPaneOpenProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsPaneOpen<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.IsPaneOpenProperty, ps, () => control.IsPaneOpen = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsPaneOpen<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.IsPaneOpenProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsPaneOpen<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.IsPaneOpenProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsPaneOpen<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.IsPaneOpenProperty, ps, () => control.IsPaneOpen = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsPaneToggleButtonVisibleProperty

/*BindFromExpressionSetterGenerator*/
public static T IsPaneToggleButtonVisible<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.IsPaneToggleButtonVisibleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsPaneToggleButtonVisible<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.IsPaneToggleButtonVisibleProperty, ps, () => control.IsPaneToggleButtonVisible = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsPaneToggleButtonVisible<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.IsPaneToggleButtonVisibleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsPaneToggleButtonVisible<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.IsPaneToggleButtonVisibleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsPaneToggleButtonVisible<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.IsPaneToggleButtonVisibleProperty, ps, () => control.IsPaneToggleButtonVisible = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsPaneVisibleProperty

/*BindFromExpressionSetterGenerator*/
public static T IsPaneVisible<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.IsPaneVisibleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsPaneVisible<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.IsPaneVisibleProperty, ps, () => control.IsPaneVisible = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsPaneVisible<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.IsPaneVisibleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsPaneVisible<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.IsPaneVisibleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsPaneVisible<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.IsPaneVisibleProperty, ps, () => control.IsPaneVisible = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsSettingsVisibleProperty

/*BindFromExpressionSetterGenerator*/
public static T IsSettingsVisible<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.IsSettingsVisibleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsSettingsVisible<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.IsSettingsVisibleProperty, ps, () => control.IsSettingsVisible = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsSettingsVisible<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.IsSettingsVisibleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsSettingsVisible<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.IsSettingsVisibleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsSettingsVisible<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.IsSettingsVisibleProperty, ps, () => control.IsSettingsVisible = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // MenuItemsProperty

/*BindFromExpressionSetterGenerator*/
public static T MenuItems<T>(this T control, Func<System.Collections.Generic.IList<System.Object>> func, Action<System.Collections.Generic.IList<System.Object>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.MenuItemsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T MenuItems<T>(this T control, System.Collections.Generic.IList<System.Object> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.MenuItemsProperty, ps, () => control.MenuItems = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T MenuItems<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.MenuItemsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T MenuItems<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.MenuItemsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T MenuItems<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.Generic.IList<System.Object>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.MenuItemsProperty, ps, () => control.MenuItems = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // MenuItemsSourceProperty

/*BindFromExpressionSetterGenerator*/
public static T MenuItemsSource<T>(this T control, Func<System.Collections.IEnumerable> func, Action<System.Collections.IEnumerable>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.MenuItemsSourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T MenuItemsSource<T>(this T control, System.Collections.IEnumerable value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.MenuItemsSourceProperty, ps, () => control.MenuItemsSource = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T MenuItemsSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.MenuItemsSourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T MenuItemsSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.MenuItemsSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T MenuItemsSource<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.IEnumerable> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.MenuItemsSourceProperty, ps, () => control.MenuItemsSource = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // MenuItemTemplateProperty

/*BindFromExpressionSetterGenerator*/
public static T MenuItemTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.MenuItemTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T MenuItemTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.MenuItemTemplateProperty, ps, () => control.MenuItemTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T MenuItemTemplate<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.MenuItemTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T MenuItemTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.MenuItemTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T MenuItemTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.MenuItemTemplateProperty, ps, () => control.MenuItemTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // MenuItemTemplateSelectorProperty

/*BindFromExpressionSetterGenerator*/
public static T MenuItemTemplateSelector<T>(this T control, Func<FluentAvalonia.UI.Controls.DataTemplateSelector> func, Action<FluentAvalonia.UI.Controls.DataTemplateSelector>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.MenuItemTemplateSelectorProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T MenuItemTemplateSelector<T>(this T control, FluentAvalonia.UI.Controls.DataTemplateSelector value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.MenuItemTemplateSelectorProperty, ps, () => control.MenuItemTemplateSelector = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T MenuItemTemplateSelector<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.MenuItemTemplateSelectorProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T MenuItemTemplateSelector<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.MenuItemTemplateSelectorProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T MenuItemTemplateSelector<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.DataTemplateSelector> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.MenuItemTemplateSelectorProperty, ps, () => control.MenuItemTemplateSelector = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // OpenPaneLengthProperty

/*BindFromExpressionSetterGenerator*/
public static T OpenPaneLength<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.OpenPaneLengthProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T OpenPaneLength<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.OpenPaneLengthProperty, ps, () => control.OpenPaneLength = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T OpenPaneLength<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.OpenPaneLengthProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T OpenPaneLength<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.OpenPaneLengthProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T OpenPaneLength<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.OpenPaneLengthProperty, ps, () => control.OpenPaneLength = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // PaneCustomContentProperty

/*BindFromExpressionSetterGenerator*/
public static T PaneCustomContent<T>(this T control, Func<Avalonia.Controls.Control> func, Action<Avalonia.Controls.Control>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.PaneCustomContentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T PaneCustomContent<T>(this T control, Avalonia.Controls.Control value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.PaneCustomContentProperty, ps, () => control.PaneCustomContent = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T PaneCustomContent<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.PaneCustomContentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T PaneCustomContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.PaneCustomContentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T PaneCustomContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Control> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.PaneCustomContentProperty, ps, () => control.PaneCustomContent = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // PaneDisplayModeProperty

/*BindFromExpressionSetterGenerator*/
public static T PaneDisplayMode<T>(this T control, Func<FluentAvalonia.UI.Controls.NavigationViewPaneDisplayMode> func, Action<FluentAvalonia.UI.Controls.NavigationViewPaneDisplayMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.PaneDisplayModeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T PaneDisplayMode<T>(this T control, FluentAvalonia.UI.Controls.NavigationViewPaneDisplayMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.PaneDisplayModeProperty, ps, () => control.PaneDisplayMode = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T PaneDisplayMode<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.PaneDisplayModeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T PaneDisplayMode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.PaneDisplayModeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T PaneDisplayMode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.NavigationViewPaneDisplayMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.PaneDisplayModeProperty, ps, () => control.PaneDisplayMode = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // PaneFooterProperty

/*BindFromExpressionSetterGenerator*/
public static T PaneFooter<T>(this T control, Func<Avalonia.Controls.Control> func, Action<Avalonia.Controls.Control>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.PaneFooterProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T PaneFooter<T>(this T control, Avalonia.Controls.Control value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.PaneFooterProperty, ps, () => control.PaneFooter = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T PaneFooter<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.PaneFooterProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T PaneFooter<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.PaneFooterProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T PaneFooter<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Control> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.PaneFooterProperty, ps, () => control.PaneFooter = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // PaneHeaderProperty

/*BindFromExpressionSetterGenerator*/
public static T PaneHeader<T>(this T control, Func<Avalonia.Controls.Control> func, Action<Avalonia.Controls.Control>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.PaneHeaderProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T PaneHeader<T>(this T control, Avalonia.Controls.Control value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.PaneHeaderProperty, ps, () => control.PaneHeader = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T PaneHeader<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.PaneHeaderProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T PaneHeader<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.PaneHeaderProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T PaneHeader<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Control> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.PaneHeaderProperty, ps, () => control.PaneHeader = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // PaneTitleProperty

/*BindFromExpressionSetterGenerator*/
public static T PaneTitle<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.PaneTitleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T PaneTitle<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.PaneTitleProperty, ps, () => control.PaneTitle = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T PaneTitle<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.PaneTitleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T PaneTitle<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.PaneTitleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T PaneTitle<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.PaneTitleProperty, ps, () => control.PaneTitle = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SelectedItemProperty

/*BindFromExpressionSetterGenerator*/
public static T SelectedItem<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.SelectedItemProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SelectedItem<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.SelectedItemProperty, ps, () => control.SelectedItem = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SelectedItem<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.SelectedItemProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SelectedItem<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.SelectedItemProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SelectedItem<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.SelectedItemProperty, ps, () => control.SelectedItem = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SelectionFollowsFocusProperty

/*BindFromExpressionSetterGenerator*/
public static T SelectionFollowsFocus<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.SelectionFollowsFocusProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SelectionFollowsFocus<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.SelectionFollowsFocusProperty, ps, () => control.SelectionFollowsFocus = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SelectionFollowsFocus<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.SelectionFollowsFocusProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SelectionFollowsFocus<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationView
   => control._set(FluentAvalonia.UI.Controls.NavigationView.SelectionFollowsFocusProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SelectionFollowsFocus<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationView
=> control._setEx(FluentAvalonia.UI.Controls.NavigationView.SelectionFollowsFocusProperty, ps, () => control.SelectionFollowsFocus = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // PaneClosing

/*ActionToEventGenerator*/
    public static T OnPaneClosing<T>(this T control, Action<FluentAvalonia.UI.Controls.NavigationView, FluentAvalonia.UI.Controls.NavigationViewPaneClosingEventArgs> action) where T : FluentAvalonia.UI.Controls.NavigationView => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.NavigationView,FluentAvalonia.UI.Controls.NavigationViewPaneClosingEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.PaneClosing += h);


 // PaneClosed

/*ActionToEventGenerator*/
    public static T OnPaneClosed<T>(this T control, Action<FluentAvalonia.UI.Controls.NavigationView, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.NavigationView => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.NavigationView,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.PaneClosed += h);


 // PaneOpening

/*ActionToEventGenerator*/
    public static T OnPaneOpening<T>(this T control, Action<FluentAvalonia.UI.Controls.NavigationView, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.NavigationView => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.NavigationView,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.PaneOpening += h);


 // PaneOpened

/*ActionToEventGenerator*/
    public static T OnPaneOpened<T>(this T control, Action<FluentAvalonia.UI.Controls.NavigationView, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.NavigationView => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.NavigationView,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.PaneOpened += h);


 // BackRequested

/*ActionToEventGenerator*/
    public static T OnBackRequested<T>(this T control, Action<FluentAvalonia.UI.Controls.NavigationViewBackRequestedEventArgs> action) where T : FluentAvalonia.UI.Controls.NavigationView => 
        control._setEvent((System.EventHandler<FluentAvalonia.UI.Controls.NavigationViewBackRequestedEventArgs>) ((arg0, arg1) => action(arg1)), h => control.BackRequested += h);


 // SelectionChanged

/*ActionToEventGenerator*/
    public static T OnSelectionChanged<T>(this T control, Action<FluentAvalonia.UI.Controls.NavigationViewSelectionChangedEventArgs> action) where T : FluentAvalonia.UI.Controls.NavigationView => 
        control._setEvent((System.EventHandler<FluentAvalonia.UI.Controls.NavigationViewSelectionChangedEventArgs>) ((arg0, arg1) => action(arg1)), h => control.SelectionChanged += h);


 // ItemInvoked

/*ActionToEventGenerator*/
    public static T OnItemInvoked<T>(this T control, Action<FluentAvalonia.UI.Controls.NavigationViewItemInvokedEventArgs> action) where T : FluentAvalonia.UI.Controls.NavigationView => 
        control._setEvent((System.EventHandler<FluentAvalonia.UI.Controls.NavigationViewItemInvokedEventArgs>) ((arg0, arg1) => action(arg1)), h => control.ItemInvoked += h);


 // DisplayModeChanged

/*ActionToEventGenerator*/
    public static T OnDisplayModeChanged<T>(this T control, Action<FluentAvalonia.UI.Controls.NavigationViewDisplayModeChangedEventArgs> action) where T : FluentAvalonia.UI.Controls.NavigationView => 
        control._setEvent((System.EventHandler<FluentAvalonia.UI.Controls.NavigationViewDisplayModeChangedEventArgs>) ((arg0, arg1) => action(arg1)), h => control.DisplayModeChanged += h);


 // ItemExpanding

/*ActionToEventGenerator*/
    public static T OnItemExpanding<T>(this T control, Action<FluentAvalonia.UI.Controls.NavigationViewItemExpandingEventArgs> action) where T : FluentAvalonia.UI.Controls.NavigationView => 
        control._setEvent((System.EventHandler<FluentAvalonia.UI.Controls.NavigationViewItemExpandingEventArgs>) ((arg0, arg1) => action(arg1)), h => control.ItemExpanding += h);


 // ItemCollapsed

/*ActionToEventGenerator*/
    public static T OnItemCollapsed<T>(this T control, Action<FluentAvalonia.UI.Controls.NavigationViewItemCollapsedEventArgs> action) where T : FluentAvalonia.UI.Controls.NavigationView => 
        control._setEvent((System.EventHandler<FluentAvalonia.UI.Controls.NavigationViewItemCollapsedEventArgs>) ((arg0, arg1) => action(arg1)), h => control.ItemCollapsed += h);



//================= Styles ======================//
 // AlwaysShowHeaderProperty

/*ValueStyleSetterGenerator*/
public static Style<T> AlwaysShowHeader<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.AlwaysShowHeaderProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> AlwaysShowHeader<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.AlwaysShowHeaderProperty, binding);


 // AutoCompleteBoxProperty

/*ValueStyleSetterGenerator*/
public static Style<T> AutoCompleteBox<T>(this Style<T> style, Avalonia.Controls.AutoCompleteBox value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.AutoCompleteBoxProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> AutoCompleteBox<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.AutoCompleteBoxProperty, binding);


 // CompactModeThresholdWidthProperty

/*ValueStyleSetterGenerator*/
public static Style<T> CompactModeThresholdWidth<T>(this Style<T> style, System.Double value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.CompactModeThresholdWidthProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CompactModeThresholdWidth<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.CompactModeThresholdWidthProperty, binding);


 // CompactPaneLengthProperty

/*ValueStyleSetterGenerator*/
public static Style<T> CompactPaneLength<T>(this Style<T> style, System.Double value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.CompactPaneLengthProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CompactPaneLength<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.CompactPaneLengthProperty, binding);


 // ContentOverlayProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ContentOverlay<T>(this Style<T> style, Avalonia.Controls.Control value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.ContentOverlayProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ContentOverlay<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.ContentOverlayProperty, binding);


 // ExpandedModeThresholdWidthProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ExpandedModeThresholdWidth<T>(this Style<T> style, System.Double value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.ExpandedModeThresholdWidthProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ExpandedModeThresholdWidth<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.ExpandedModeThresholdWidthProperty, binding);


 // FooterMenuItemsSourceProperty

/*ValueStyleSetterGenerator*/
public static Style<T> FooterMenuItemsSource<T>(this Style<T> style, System.Collections.IEnumerable value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.FooterMenuItemsSourceProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> FooterMenuItemsSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.FooterMenuItemsSourceProperty, binding);


 // IsBackButtonVisibleProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsBackButtonVisible<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.IsBackButtonVisibleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsBackButtonVisible<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.IsBackButtonVisibleProperty, binding);


 // IsBackEnabledProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsBackEnabled<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.IsBackEnabledProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsBackEnabled<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.IsBackEnabledProperty, binding);


 // IsPaneOpenProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsPaneOpen<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.IsPaneOpenProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsPaneOpen<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.IsPaneOpenProperty, binding);


 // IsPaneToggleButtonVisibleProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsPaneToggleButtonVisible<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.IsPaneToggleButtonVisibleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsPaneToggleButtonVisible<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.IsPaneToggleButtonVisibleProperty, binding);


 // IsPaneVisibleProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsPaneVisible<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.IsPaneVisibleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsPaneVisible<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.IsPaneVisibleProperty, binding);


 // IsSettingsVisibleProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsSettingsVisible<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.IsSettingsVisibleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsSettingsVisible<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.IsSettingsVisibleProperty, binding);


 // MenuItemsSourceProperty

/*ValueStyleSetterGenerator*/
public static Style<T> MenuItemsSource<T>(this Style<T> style, System.Collections.IEnumerable value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.MenuItemsSourceProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> MenuItemsSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.MenuItemsSourceProperty, binding);


 // MenuItemTemplateProperty

/*ValueStyleSetterGenerator*/
public static Style<T> MenuItemTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.MenuItemTemplateProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> MenuItemTemplate<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.MenuItemTemplateProperty, binding);


 // MenuItemTemplateSelectorProperty

/*ValueStyleSetterGenerator*/
public static Style<T> MenuItemTemplateSelector<T>(this Style<T> style, FluentAvalonia.UI.Controls.DataTemplateSelector value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.MenuItemTemplateSelectorProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> MenuItemTemplateSelector<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.MenuItemTemplateSelectorProperty, binding);


 // OpenPaneLengthProperty

/*ValueStyleSetterGenerator*/
public static Style<T> OpenPaneLength<T>(this Style<T> style, System.Double value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.OpenPaneLengthProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> OpenPaneLength<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.OpenPaneLengthProperty, binding);


 // PaneCustomContentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> PaneCustomContent<T>(this Style<T> style, Avalonia.Controls.Control value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.PaneCustomContentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> PaneCustomContent<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.PaneCustomContentProperty, binding);


 // PaneDisplayModeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> PaneDisplayMode<T>(this Style<T> style, FluentAvalonia.UI.Controls.NavigationViewPaneDisplayMode value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.PaneDisplayModeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> PaneDisplayMode<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.PaneDisplayModeProperty, binding);


 // PaneFooterProperty

/*ValueStyleSetterGenerator*/
public static Style<T> PaneFooter<T>(this Style<T> style, Avalonia.Controls.Control value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.PaneFooterProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> PaneFooter<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.PaneFooterProperty, binding);


 // PaneHeaderProperty

/*ValueStyleSetterGenerator*/
public static Style<T> PaneHeader<T>(this Style<T> style, Avalonia.Controls.Control value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.PaneHeaderProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> PaneHeader<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.PaneHeaderProperty, binding);


 // PaneTitleProperty

/*ValueStyleSetterGenerator*/
public static Style<T> PaneTitle<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.PaneTitleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> PaneTitle<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.PaneTitleProperty, binding);


 // SelectionFollowsFocusProperty

/*ValueStyleSetterGenerator*/
public static Style<T> SelectionFollowsFocus<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.SelectionFollowsFocusProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SelectionFollowsFocus<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.SelectionFollowsFocusProperty, binding);



}
