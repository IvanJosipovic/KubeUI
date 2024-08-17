#nullable enable
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using System;
using System.Collections;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TabView = FluentAvalonia.UI.Controls.TabView;

namespace Avalonia.Markup.Declarative;
public static partial class TabViewExtensions
{
public static T TabWidthMode<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabWidthModeProperty, binding);
public static T TabWidthMode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabWidthModeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T TabWidthMode<T>(this T control, Func<FluentAvalonia.UI.Controls.TabViewWidthMode> func, Action<FluentAvalonia.UI.Controls.TabViewWidthMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabWidthModeProperty, func, onChanged, expression);
public static T TabWidthMode<T>(this T control, FluentAvalonia.UI.Controls.TabViewWidthMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.TabWidthModeProperty, ps, () => control.TabWidthMode = value, bindingMode, converter, bindingSource);
public static T TabWidthMode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.TabViewWidthMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.TabWidthModeProperty, ps, () => control.TabWidthMode = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T CloseButtonOverlayMode<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.CloseButtonOverlayModeProperty, binding);
public static T CloseButtonOverlayMode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.CloseButtonOverlayModeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CloseButtonOverlayMode<T>(this T control, Func<FluentAvalonia.UI.Controls.TabViewCloseButtonOverlayMode> func, Action<FluentAvalonia.UI.Controls.TabViewCloseButtonOverlayMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.CloseButtonOverlayModeProperty, func, onChanged, expression);
public static T CloseButtonOverlayMode<T>(this T control, FluentAvalonia.UI.Controls.TabViewCloseButtonOverlayMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.CloseButtonOverlayModeProperty, ps, () => control.CloseButtonOverlayMode = value, bindingMode, converter, bindingSource);
public static T CloseButtonOverlayMode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.TabViewCloseButtonOverlayMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.CloseButtonOverlayModeProperty, ps, () => control.CloseButtonOverlayMode = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T TabStripHeader<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabStripHeaderProperty, binding);
public static T TabStripHeader<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabStripHeaderProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T TabStripHeader<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabStripHeaderProperty, func, onChanged, expression);
public static T TabStripHeader<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.TabStripHeaderProperty, ps, () => control.TabStripHeader = value, bindingMode, converter, bindingSource);
public static T TabStripHeader<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.TabStripHeaderProperty, ps, () => control.TabStripHeader = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T TabStripHeaderTemplate<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabStripHeaderTemplateProperty, binding);
public static T TabStripHeaderTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabStripHeaderTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T TabStripHeaderTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabStripHeaderTemplateProperty, func, onChanged, expression);
public static T TabStripHeaderTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.TabStripHeaderTemplateProperty, ps, () => control.TabStripHeaderTemplate = value, bindingMode, converter, bindingSource);
public static T TabStripHeaderTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.TabStripHeaderTemplateProperty, ps, () => control.TabStripHeaderTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T TabStripFooter<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabStripFooterProperty, binding);
public static T TabStripFooter<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabStripFooterProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T TabStripFooter<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabStripFooterProperty, func, onChanged, expression);
public static T TabStripFooter<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.TabStripFooterProperty, ps, () => control.TabStripFooter = value, bindingMode, converter, bindingSource);
public static T TabStripFooter<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.TabStripFooterProperty, ps, () => control.TabStripFooter = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T TabStripFooterTemplate<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabStripFooterTemplateProperty, binding);
public static T TabStripFooterTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabStripFooterTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T TabStripFooterTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabStripFooterTemplateProperty, func, onChanged, expression);
public static T TabStripFooterTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.TabStripFooterTemplateProperty, ps, () => control.TabStripFooterTemplate = value, bindingMode, converter, bindingSource);
public static T TabStripFooterTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.TabStripFooterTemplateProperty, ps, () => control.TabStripFooterTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsAddTabButtonVisible<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.IsAddTabButtonVisibleProperty, binding);
public static T IsAddTabButtonVisible<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.IsAddTabButtonVisibleProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsAddTabButtonVisible<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.IsAddTabButtonVisibleProperty, func, onChanged, expression);
public static T IsAddTabButtonVisible<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.IsAddTabButtonVisibleProperty, ps, () => control.IsAddTabButtonVisible = value, bindingMode, converter, bindingSource);
public static T IsAddTabButtonVisible<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.IsAddTabButtonVisibleProperty, ps, () => control.IsAddTabButtonVisible = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T AddTabButtonCommand<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.AddTabButtonCommandProperty, binding);
public static T AddTabButtonCommand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.AddTabButtonCommandProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T AddTabButtonCommand<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.AddTabButtonCommandProperty, func, onChanged, expression);
public static T AddTabButtonCommand<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.AddTabButtonCommandProperty, ps, () => control.AddTabButtonCommand = value, bindingMode, converter, bindingSource);
public static T AddTabButtonCommand<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.AddTabButtonCommandProperty, ps, () => control.AddTabButtonCommand = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T AddTabButtonCommandParameter<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.AddTabButtonCommandParameterProperty, binding);
public static T AddTabButtonCommandParameter<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.AddTabButtonCommandParameterProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T AddTabButtonCommandParameter<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.AddTabButtonCommandParameterProperty, func, onChanged, expression);
public static T AddTabButtonCommandParameter<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.AddTabButtonCommandParameterProperty, ps, () => control.AddTabButtonCommandParameter = value, bindingMode, converter, bindingSource);
public static T AddTabButtonCommandParameter<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.AddTabButtonCommandParameterProperty, ps, () => control.AddTabButtonCommandParameter = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T TabItems<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabItemsProperty, binding);
public static T TabItems<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabItemsProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T TabItems<T>(this T control, Func<System.Collections.IEnumerable> func, Action<System.Collections.IEnumerable>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabItemsProperty, func, onChanged, expression);
public static T TabItems<T>(this T control, System.Collections.IEnumerable value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.TabItemsProperty, ps, () => control.TabItems = value, bindingMode, converter, bindingSource);
public static T TabItems<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.IEnumerable> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.TabItemsProperty, ps, () => control.TabItems = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T TabItemTemplate<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabItemTemplateProperty, binding);
public static T TabItemTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabItemTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T TabItemTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabItemTemplateProperty, func, onChanged, expression);
public static T TabItemTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.TabItemTemplateProperty, ps, () => control.TabItemTemplate = value, bindingMode, converter, bindingSource);
public static T TabItemTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.TabItemTemplateProperty, ps, () => control.TabItemTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T CanDragTabs<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.CanDragTabsProperty, binding);
public static T CanDragTabs<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.CanDragTabsProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CanDragTabs<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.CanDragTabsProperty, func, onChanged, expression);
public static T CanDragTabs<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.CanDragTabsProperty, ps, () => control.CanDragTabs = value, bindingMode, converter, bindingSource);
public static T CanDragTabs<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.CanDragTabsProperty, ps, () => control.CanDragTabs = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T CanReorderTabs<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.CanReorderTabsProperty, binding);
public static T CanReorderTabs<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.CanReorderTabsProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CanReorderTabs<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.CanReorderTabsProperty, func, onChanged, expression);
public static T CanReorderTabs<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.CanReorderTabsProperty, ps, () => control.CanReorderTabs = value, bindingMode, converter, bindingSource);
public static T CanReorderTabs<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.CanReorderTabsProperty, ps, () => control.CanReorderTabs = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T AllowDropTabs<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.AllowDropTabsProperty, binding);
public static T AllowDropTabs<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.AllowDropTabsProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T AllowDropTabs<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.AllowDropTabsProperty, func, onChanged, expression);
public static T AllowDropTabs<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.AllowDropTabsProperty, ps, () => control.AllowDropTabs = value, bindingMode, converter, bindingSource);
public static T AllowDropTabs<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.AllowDropTabsProperty, ps, () => control.AllowDropTabs = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SelectedIndex<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.SelectedIndexProperty, binding);
public static T SelectedIndex<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.SelectedIndexProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SelectedIndex<T>(this T control, Func<System.Int32> func, Action<System.Int32>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.SelectedIndexProperty, func, onChanged, expression);
public static T SelectedIndex<T>(this T control, System.Int32 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.SelectedIndexProperty, ps, () => control.SelectedIndex = value, bindingMode, converter, bindingSource);
public static T SelectedIndex<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Int32> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.SelectedIndexProperty, ps, () => control.SelectedIndex = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SelectedItem<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.SelectedItemProperty, binding);
public static T SelectedItem<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.SelectedItemProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SelectedItem<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.SelectedItemProperty, func, onChanged, expression);
public static T SelectedItem<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.SelectedItemProperty, ps, () => control.SelectedItem = value, bindingMode, converter, bindingSource);
public static T SelectedItem<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.SelectedItemProperty, ps, () => control.SelectedItem = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

