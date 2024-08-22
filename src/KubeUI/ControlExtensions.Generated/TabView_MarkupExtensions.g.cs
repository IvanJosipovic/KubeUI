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
public static partial class TabView_MarkupExtensions
{
//================= Properties ======================//
 // TabWidthModeProperty

/*BindFromExpressionSetterGenerator*/
public static T TabWidthMode<T>(this T control, Func<FluentAvalonia.UI.Controls.TabViewWidthMode> func, Action<FluentAvalonia.UI.Controls.TabViewWidthMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabWidthModeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T TabWidthMode<T>(this T control, FluentAvalonia.UI.Controls.TabViewWidthMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.TabWidthModeProperty, ps, () => control.TabWidthMode = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TabWidthMode<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabWidthModeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TabWidthMode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabWidthModeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T TabWidthMode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.TabViewWidthMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.TabWidthModeProperty, ps, () => control.TabWidthMode = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CloseButtonOverlayModeProperty

/*BindFromExpressionSetterGenerator*/
public static T CloseButtonOverlayMode<T>(this T control, Func<FluentAvalonia.UI.Controls.TabViewCloseButtonOverlayMode> func, Action<FluentAvalonia.UI.Controls.TabViewCloseButtonOverlayMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.CloseButtonOverlayModeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CloseButtonOverlayMode<T>(this T control, FluentAvalonia.UI.Controls.TabViewCloseButtonOverlayMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.CloseButtonOverlayModeProperty, ps, () => control.CloseButtonOverlayMode = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CloseButtonOverlayMode<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.CloseButtonOverlayModeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CloseButtonOverlayMode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.CloseButtonOverlayModeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CloseButtonOverlayMode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.TabViewCloseButtonOverlayMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.CloseButtonOverlayModeProperty, ps, () => control.CloseButtonOverlayMode = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TabStripHeaderProperty

/*BindFromExpressionSetterGenerator*/
public static T TabStripHeader<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabStripHeaderProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T TabStripHeader<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.TabStripHeaderProperty, ps, () => control.TabStripHeader = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TabStripHeader<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabStripHeaderProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TabStripHeader<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabStripHeaderProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T TabStripHeader<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.TabStripHeaderProperty, ps, () => control.TabStripHeader = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TabStripHeaderTemplateProperty

/*BindFromExpressionSetterGenerator*/
public static T TabStripHeaderTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabStripHeaderTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T TabStripHeaderTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.TabStripHeaderTemplateProperty, ps, () => control.TabStripHeaderTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TabStripHeaderTemplate<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabStripHeaderTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TabStripHeaderTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabStripHeaderTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T TabStripHeaderTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.TabStripHeaderTemplateProperty, ps, () => control.TabStripHeaderTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TabStripFooterProperty

/*BindFromExpressionSetterGenerator*/
public static T TabStripFooter<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabStripFooterProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T TabStripFooter<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.TabStripFooterProperty, ps, () => control.TabStripFooter = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TabStripFooter<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabStripFooterProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TabStripFooter<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabStripFooterProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T TabStripFooter<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.TabStripFooterProperty, ps, () => control.TabStripFooter = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TabStripFooterTemplateProperty

/*BindFromExpressionSetterGenerator*/
public static T TabStripFooterTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabStripFooterTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T TabStripFooterTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.TabStripFooterTemplateProperty, ps, () => control.TabStripFooterTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TabStripFooterTemplate<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabStripFooterTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TabStripFooterTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabStripFooterTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T TabStripFooterTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.TabStripFooterTemplateProperty, ps, () => control.TabStripFooterTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsAddTabButtonVisibleProperty

/*BindFromExpressionSetterGenerator*/
public static T IsAddTabButtonVisible<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.IsAddTabButtonVisibleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsAddTabButtonVisible<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.IsAddTabButtonVisibleProperty, ps, () => control.IsAddTabButtonVisible = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsAddTabButtonVisible<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.IsAddTabButtonVisibleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsAddTabButtonVisible<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.IsAddTabButtonVisibleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsAddTabButtonVisible<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.IsAddTabButtonVisibleProperty, ps, () => control.IsAddTabButtonVisible = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // AddTabButtonCommandProperty

/*BindFromExpressionSetterGenerator*/
public static T AddTabButtonCommand<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.AddTabButtonCommandProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T AddTabButtonCommand<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.AddTabButtonCommandProperty, ps, () => control.AddTabButtonCommand = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T AddTabButtonCommand<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.AddTabButtonCommandProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T AddTabButtonCommand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.AddTabButtonCommandProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T AddTabButtonCommand<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.AddTabButtonCommandProperty, ps, () => control.AddTabButtonCommand = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // AddTabButtonCommandParameterProperty

/*BindFromExpressionSetterGenerator*/
public static T AddTabButtonCommandParameter<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.AddTabButtonCommandParameterProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T AddTabButtonCommandParameter<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.AddTabButtonCommandParameterProperty, ps, () => control.AddTabButtonCommandParameter = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T AddTabButtonCommandParameter<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.AddTabButtonCommandParameterProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T AddTabButtonCommandParameter<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.AddTabButtonCommandParameterProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T AddTabButtonCommandParameter<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.AddTabButtonCommandParameterProperty, ps, () => control.AddTabButtonCommandParameter = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TabItemsProperty

/*BindFromExpressionSetterGenerator*/
public static T TabItems<T>(this T control, Func<System.Collections.IEnumerable> func, Action<System.Collections.IEnumerable>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabItemsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T TabItems<T>(this T control, System.Collections.IEnumerable value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.TabItemsProperty, ps, () => control.TabItems = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TabItems<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabItemsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TabItems<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabItemsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T TabItems<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.IEnumerable> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.TabItemsProperty, ps, () => control.TabItems = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TabItemTemplateProperty

/*BindFromExpressionSetterGenerator*/
public static T TabItemTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabItemTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T TabItemTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.TabItemTemplateProperty, ps, () => control.TabItemTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TabItemTemplate<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabItemTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TabItemTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.TabItemTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T TabItemTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.TabItemTemplateProperty, ps, () => control.TabItemTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CanDragTabsProperty

/*BindFromExpressionSetterGenerator*/
public static T CanDragTabs<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.CanDragTabsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CanDragTabs<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.CanDragTabsProperty, ps, () => control.CanDragTabs = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CanDragTabs<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.CanDragTabsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CanDragTabs<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.CanDragTabsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CanDragTabs<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.CanDragTabsProperty, ps, () => control.CanDragTabs = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CanReorderTabsProperty

/*BindFromExpressionSetterGenerator*/
public static T CanReorderTabs<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.CanReorderTabsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CanReorderTabs<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.CanReorderTabsProperty, ps, () => control.CanReorderTabs = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CanReorderTabs<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.CanReorderTabsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CanReorderTabs<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.CanReorderTabsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CanReorderTabs<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.CanReorderTabsProperty, ps, () => control.CanReorderTabs = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // AllowDropTabsProperty

/*BindFromExpressionSetterGenerator*/
public static T AllowDropTabs<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.AllowDropTabsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T AllowDropTabs<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.AllowDropTabsProperty, ps, () => control.AllowDropTabs = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T AllowDropTabs<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.AllowDropTabsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T AllowDropTabs<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.AllowDropTabsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T AllowDropTabs<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.AllowDropTabsProperty, ps, () => control.AllowDropTabs = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SelectedIndexProperty

/*BindFromExpressionSetterGenerator*/
public static T SelectedIndex<T>(this T control, Func<System.Int32> func, Action<System.Int32>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.SelectedIndexProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SelectedIndex<T>(this T control, System.Int32 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.SelectedIndexProperty, ps, () => control.SelectedIndex = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SelectedIndex<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.SelectedIndexProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SelectedIndex<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.SelectedIndexProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SelectedIndex<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Int32> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.SelectedIndexProperty, ps, () => control.SelectedIndex = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SelectedItemProperty

/*BindFromExpressionSetterGenerator*/
public static T SelectedItem<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.SelectedItemProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SelectedItem<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.SelectedItemProperty, ps, () => control.SelectedItem = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SelectedItem<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.SelectedItemProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SelectedItem<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabView
   => control._set(FluentAvalonia.UI.Controls.TabView.SelectedItemProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SelectedItem<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabView
=> control._setEx(FluentAvalonia.UI.Controls.TabView.SelectedItemProperty, ps, () => control.SelectedItem = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // TabCloseRequested

/*ActionToEventGenerator*/
    public static T OnTabCloseRequested<T>(this T control, Action<FluentAvalonia.UI.Controls.TabView, FluentAvalonia.UI.Controls.TabViewTabCloseRequestedEventArgs> action) where T : FluentAvalonia.UI.Controls.TabView => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.TabView,FluentAvalonia.UI.Controls.TabViewTabCloseRequestedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.TabCloseRequested += h);


 // TabDroppedOutside

/*ActionToEventGenerator*/
    public static T OnTabDroppedOutside<T>(this T control, Action<FluentAvalonia.UI.Controls.TabView, FluentAvalonia.UI.Controls.TabViewTabDroppedOutsideEventArgs> action) where T : FluentAvalonia.UI.Controls.TabView => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.TabView,FluentAvalonia.UI.Controls.TabViewTabDroppedOutsideEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.TabDroppedOutside += h);


 // AddTabButtonClick

/*ActionToEventGenerator*/
    public static T OnAddTabButtonClick<T>(this T control, Action<FluentAvalonia.UI.Controls.TabView, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.TabView => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.TabView,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.AddTabButtonClick += h);


 // TabItemsChanged

/*ActionToEventGenerator*/
    public static T OnTabItemsChanged<T>(this T control, Action<FluentAvalonia.UI.Controls.TabView, System.Collections.Specialized.NotifyCollectionChangedEventArgs> action) where T : FluentAvalonia.UI.Controls.TabView => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.TabView,System.Collections.Specialized.NotifyCollectionChangedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.TabItemsChanged += h);


 // SelectionChanged

/*ActionToEventGenerator*/
    public static T OnSelectionChanged<T>(this T control, Action<Avalonia.Controls.SelectionChangedEventArgs> action) where T : FluentAvalonia.UI.Controls.TabView => 
        control._setEvent((FluentAvalonia.Core.SelectionChangedEventHandler) ((arg0, arg1) => action(arg1)), h => control.SelectionChanged += h);


 // TabDragStarting

/*ActionToEventGenerator*/
    public static T OnTabDragStarting<T>(this T control, Action<FluentAvalonia.UI.Controls.TabView, FluentAvalonia.UI.Controls.TabViewTabDragStartingEventArgs> action) where T : FluentAvalonia.UI.Controls.TabView => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.TabView,FluentAvalonia.UI.Controls.TabViewTabDragStartingEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.TabDragStarting += h);


 // TabDragCompleted

/*ActionToEventGenerator*/
    public static T OnTabDragCompleted<T>(this T control, Action<FluentAvalonia.UI.Controls.TabView, FluentAvalonia.UI.Controls.TabViewTabDragCompletedEventArgs> action) where T : FluentAvalonia.UI.Controls.TabView => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.TabView,FluentAvalonia.UI.Controls.TabViewTabDragCompletedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.TabDragCompleted += h);


 // TabStripDragOver

/*ActionToEventGenerator*/
    public static T OnTabStripDragOver<T>(this T control, Action<Avalonia.Input.DragEventArgs> action) where T : FluentAvalonia.UI.Controls.TabView => 
        control._setEvent((System.EventHandler<Avalonia.Input.DragEventArgs>) ((arg0, arg1) => action(arg1)), h => control.TabStripDragOver += h);


 // TabStripDrop

/*ActionToEventGenerator*/
    public static T OnTabStripDrop<T>(this T control, Action<Avalonia.Input.DragEventArgs> action) where T : FluentAvalonia.UI.Controls.TabView => 
        control._setEvent((System.EventHandler<Avalonia.Input.DragEventArgs>) ((arg0, arg1) => action(arg1)), h => control.TabStripDrop += h);



//================= Styles ======================//
 // TabWidthModeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> TabWidthMode<T>(this Style<T> style, FluentAvalonia.UI.Controls.TabViewWidthMode value) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.TabWidthModeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> TabWidthMode<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.TabWidthModeProperty, binding);


 // CloseButtonOverlayModeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> CloseButtonOverlayMode<T>(this Style<T> style, FluentAvalonia.UI.Controls.TabViewCloseButtonOverlayMode value) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.CloseButtonOverlayModeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CloseButtonOverlayMode<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.CloseButtonOverlayModeProperty, binding);


 // TabStripHeaderProperty

/*ValueStyleSetterGenerator*/
public static Style<T> TabStripHeader<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.TabStripHeaderProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> TabStripHeader<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.TabStripHeaderProperty, binding);


 // TabStripHeaderTemplateProperty

/*ValueStyleSetterGenerator*/
public static Style<T> TabStripHeaderTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.TabStripHeaderTemplateProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> TabStripHeaderTemplate<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.TabStripHeaderTemplateProperty, binding);


 // TabStripFooterProperty

/*ValueStyleSetterGenerator*/
public static Style<T> TabStripFooter<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.TabStripFooterProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> TabStripFooter<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.TabStripFooterProperty, binding);


 // TabStripFooterTemplateProperty

/*ValueStyleSetterGenerator*/
public static Style<T> TabStripFooterTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.TabStripFooterTemplateProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> TabStripFooterTemplate<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.TabStripFooterTemplateProperty, binding);


 // IsAddTabButtonVisibleProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsAddTabButtonVisible<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.IsAddTabButtonVisibleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsAddTabButtonVisible<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.IsAddTabButtonVisibleProperty, binding);


 // AddTabButtonCommandProperty

/*ValueStyleSetterGenerator*/
public static Style<T> AddTabButtonCommand<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.AddTabButtonCommandProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> AddTabButtonCommand<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.AddTabButtonCommandProperty, binding);


 // AddTabButtonCommandParameterProperty

/*ValueStyleSetterGenerator*/
public static Style<T> AddTabButtonCommandParameter<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.AddTabButtonCommandParameterProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> AddTabButtonCommandParameter<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.AddTabButtonCommandParameterProperty, binding);


 // TabItemTemplateProperty

/*ValueStyleSetterGenerator*/
public static Style<T> TabItemTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.TabItemTemplateProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> TabItemTemplate<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.TabItemTemplateProperty, binding);


 // CanDragTabsProperty

/*ValueStyleSetterGenerator*/
public static Style<T> CanDragTabs<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.CanDragTabsProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CanDragTabs<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.CanDragTabsProperty, binding);


 // CanReorderTabsProperty

/*ValueStyleSetterGenerator*/
public static Style<T> CanReorderTabs<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.CanReorderTabsProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CanReorderTabs<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.CanReorderTabsProperty, binding);


 // AllowDropTabsProperty

/*ValueStyleSetterGenerator*/
public static Style<T> AllowDropTabs<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.AllowDropTabsProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> AllowDropTabs<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.AllowDropTabsProperty, binding);



}
