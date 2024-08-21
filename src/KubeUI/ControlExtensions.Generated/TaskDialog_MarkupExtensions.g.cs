#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class TaskDialog_MarkupExtensions
{
//================= Properties ======================//
 // TitleProperty

/*BindFromExpressionSetterGenerator*/
public static T Title<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.TitleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Title<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.TitleProperty, ps, () => control.Title = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Title<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.TitleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Title<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.TitleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Title<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.TitleProperty, ps, () => control.Title = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // HeaderProperty

/*BindFromExpressionSetterGenerator*/
public static T Header<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.HeaderProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Header<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.HeaderProperty, ps, () => control.Header = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Header<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.HeaderProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Header<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.HeaderProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Header<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.HeaderProperty, ps, () => control.Header = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SubHeaderProperty

/*BindFromExpressionSetterGenerator*/
public static T SubHeader<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.SubHeaderProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SubHeader<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.SubHeaderProperty, ps, () => control.SubHeader = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SubHeader<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.SubHeaderProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SubHeader<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.SubHeaderProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SubHeader<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.SubHeaderProperty, ps, () => control.SubHeader = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IconSourceProperty

/*BindFromExpressionSetterGenerator*/
public static T IconSource<T>(this T control, Func<FluentAvalonia.UI.Controls.IconSource> func, Action<FluentAvalonia.UI.Controls.IconSource>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.IconSourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IconSource<T>(this T control, FluentAvalonia.UI.Controls.IconSource value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.IconSourceProperty, ps, () => control.IconSource = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IconSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.IconSourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IconSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.IconSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IconSource<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.IconSource> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.IconSourceProperty, ps, () => control.IconSource = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ButtonsProperty

/*BindFromExpressionSetterGenerator*/
public static T Buttons<T>(this T control, Func<System.Collections.Generic.IList<FluentAvalonia.UI.Controls.TaskDialogButton>> func, Action<System.Collections.Generic.IList<FluentAvalonia.UI.Controls.TaskDialogButton>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.ButtonsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Buttons<T>(this T control, System.Collections.Generic.IList<FluentAvalonia.UI.Controls.TaskDialogButton> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.ButtonsProperty, ps, () => control.Buttons = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Buttons<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.ButtonsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Buttons<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.ButtonsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Buttons<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.Generic.IList<FluentAvalonia.UI.Controls.TaskDialogButton>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.ButtonsProperty, ps, () => control.Buttons = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CommandsProperty

/*BindFromExpressionSetterGenerator*/
public static T Commands<T>(this T control, Func<System.Collections.Generic.IList<FluentAvalonia.UI.Controls.TaskDialogCommand>> func, Action<System.Collections.Generic.IList<FluentAvalonia.UI.Controls.TaskDialogCommand>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.CommandsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Commands<T>(this T control, System.Collections.Generic.IList<FluentAvalonia.UI.Controls.TaskDialogCommand> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.CommandsProperty, ps, () => control.Commands = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Commands<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.CommandsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Commands<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.CommandsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Commands<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.Generic.IList<FluentAvalonia.UI.Controls.TaskDialogCommand>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.CommandsProperty, ps, () => control.Commands = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // FooterVisibilityProperty

/*BindFromExpressionSetterGenerator*/
public static T FooterVisibility<T>(this T control, Func<FluentAvalonia.UI.Controls.TaskDialogFooterVisibility> func, Action<FluentAvalonia.UI.Controls.TaskDialogFooterVisibility>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.FooterVisibilityProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T FooterVisibility<T>(this T control, FluentAvalonia.UI.Controls.TaskDialogFooterVisibility value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.FooterVisibilityProperty, ps, () => control.FooterVisibility = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T FooterVisibility<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.FooterVisibilityProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T FooterVisibility<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.FooterVisibilityProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T FooterVisibility<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.TaskDialogFooterVisibility> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.FooterVisibilityProperty, ps, () => control.FooterVisibility = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsFooterExpandedProperty

/*BindFromExpressionSetterGenerator*/
public static T IsFooterExpanded<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.IsFooterExpandedProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsFooterExpanded<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.IsFooterExpandedProperty, ps, () => control.IsFooterExpanded = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsFooterExpanded<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.IsFooterExpandedProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsFooterExpanded<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.IsFooterExpandedProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsFooterExpanded<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.IsFooterExpandedProperty, ps, () => control.IsFooterExpanded = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // FooterProperty

/*BindFromExpressionSetterGenerator*/
public static T Footer<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.FooterProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Footer<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.FooterProperty, ps, () => control.Footer = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Footer<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.FooterProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Footer<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.FooterProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Footer<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.FooterProperty, ps, () => control.Footer = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // FooterTemplateProperty

/*BindFromExpressionSetterGenerator*/
public static T FooterTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.FooterTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T FooterTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.FooterTemplateProperty, ps, () => control.FooterTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T FooterTemplate<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.FooterTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T FooterTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.FooterTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T FooterTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.FooterTemplateProperty, ps, () => control.FooterTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ShowProgressBarProperty

/*BindFromExpressionSetterGenerator*/
public static T ShowProgressBar<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.ShowProgressBarProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ShowProgressBar<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.ShowProgressBarProperty, ps, () => control.ShowProgressBar = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ShowProgressBar<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.ShowProgressBarProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ShowProgressBar<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.ShowProgressBarProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ShowProgressBar<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.ShowProgressBarProperty, ps, () => control.ShowProgressBar = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // HeaderBackgroundProperty

/*BindFromExpressionSetterGenerator*/
public static T HeaderBackground<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.HeaderBackgroundProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T HeaderBackground<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.HeaderBackgroundProperty, ps, () => control.HeaderBackground = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T HeaderBackground<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.HeaderBackgroundProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T HeaderBackground<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.HeaderBackgroundProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T HeaderBackground<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.HeaderBackgroundProperty, ps, () => control.HeaderBackground = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // HeaderForegroundProperty

/*BindFromExpressionSetterGenerator*/
public static T HeaderForeground<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.HeaderForegroundProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T HeaderForeground<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.HeaderForegroundProperty, ps, () => control.HeaderForeground = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T HeaderForeground<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.HeaderForegroundProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T HeaderForeground<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.HeaderForegroundProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T HeaderForeground<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.HeaderForegroundProperty, ps, () => control.HeaderForeground = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IconForegroundProperty

/*BindFromExpressionSetterGenerator*/
public static T IconForeground<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.IconForegroundProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IconForeground<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.IconForegroundProperty, ps, () => control.IconForeground = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IconForeground<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.IconForegroundProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IconForeground<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.IconForegroundProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IconForeground<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.IconForegroundProperty, ps, () => control.IconForeground = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // Opening

/*ActionToEventGenerator*/
    public static T OnOpening<T>(this T control, Action<FluentAvalonia.UI.Controls.TaskDialog, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.TaskDialog => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.TaskDialog,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Opening += h);


 // Opened

/*ActionToEventGenerator*/
    public static T OnOpened<T>(this T control, Action<FluentAvalonia.UI.Controls.TaskDialog, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.TaskDialog => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.TaskDialog,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Opened += h);


 // Closing

/*ActionToEventGenerator*/
    public static T OnClosing<T>(this T control, Action<FluentAvalonia.UI.Controls.TaskDialog, FluentAvalonia.UI.Controls.TaskDialogClosingEventArgs> action) where T : FluentAvalonia.UI.Controls.TaskDialog => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.TaskDialog,FluentAvalonia.UI.Controls.TaskDialogClosingEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Closing += h);


 // Closed

/*ActionToEventGenerator*/
    public static T OnClosed<T>(this T control, Action<FluentAvalonia.UI.Controls.TaskDialog, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.TaskDialog => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.TaskDialog,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Closed += h);



//================= Styles ======================//
 // TitleProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Title<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.TitleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Title<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.TitleProperty, binding);


 // HeaderProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Header<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.HeaderProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Header<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.HeaderProperty, binding);


 // SubHeaderProperty

/*ValueStyleSetterGenerator*/
public static Style<T> SubHeader<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.SubHeaderProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SubHeader<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.SubHeaderProperty, binding);


 // IconSourceProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IconSource<T>(this Style<T> style, FluentAvalonia.UI.Controls.IconSource value) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.IconSourceProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IconSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.IconSourceProperty, binding);


 // FooterVisibilityProperty

/*ValueStyleSetterGenerator*/
public static Style<T> FooterVisibility<T>(this Style<T> style, FluentAvalonia.UI.Controls.TaskDialogFooterVisibility value) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.FooterVisibilityProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> FooterVisibility<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.FooterVisibilityProperty, binding);


 // IsFooterExpandedProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsFooterExpanded<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.IsFooterExpandedProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsFooterExpanded<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.IsFooterExpandedProperty, binding);


 // FooterProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Footer<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.FooterProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Footer<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.FooterProperty, binding);


 // FooterTemplateProperty

/*ValueStyleSetterGenerator*/
public static Style<T> FooterTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.FooterTemplateProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> FooterTemplate<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.FooterTemplateProperty, binding);


 // ShowProgressBarProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ShowProgressBar<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.ShowProgressBarProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ShowProgressBar<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.ShowProgressBarProperty, binding);


 // HeaderBackgroundProperty

/*ValueStyleSetterGenerator*/
public static Style<T> HeaderBackground<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.HeaderBackgroundProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> HeaderBackground<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.HeaderBackgroundProperty, binding);


 // HeaderForegroundProperty

/*ValueStyleSetterGenerator*/
public static Style<T> HeaderForeground<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.HeaderForegroundProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> HeaderForeground<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.HeaderForegroundProperty, binding);


 // IconForegroundProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IconForeground<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.IconForegroundProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IconForeground<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.IconForegroundProperty, binding);



}
