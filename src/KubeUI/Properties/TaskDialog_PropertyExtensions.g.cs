#nullable enable
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TaskDialog = FluentAvalonia.UI.Controls.TaskDialog;

namespace Avalonia.Markup.Declarative;
public static partial class TaskDialogExtensions
{
public static T Title<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.TitleProperty, binding);
public static T Title<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.TitleProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Title<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.TitleProperty, func, onChanged, expression);
public static T Title<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.TitleProperty, ps, () => control.Title = value, bindingMode, converter, bindingSource);
public static T Title<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.TitleProperty, ps, () => control.Title = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Header<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.HeaderProperty, binding);
public static T Header<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.HeaderProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Header<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.HeaderProperty, func, onChanged, expression);
public static T Header<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.HeaderProperty, ps, () => control.Header = value, bindingMode, converter, bindingSource);
public static T Header<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.HeaderProperty, ps, () => control.Header = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SubHeader<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.SubHeaderProperty, binding);
public static T SubHeader<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.SubHeaderProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SubHeader<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.SubHeaderProperty, func, onChanged, expression);
public static T SubHeader<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.SubHeaderProperty, ps, () => control.SubHeader = value, bindingMode, converter, bindingSource);
public static T SubHeader<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.SubHeaderProperty, ps, () => control.SubHeader = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IconSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.IconSourceProperty, binding);
public static T IconSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.IconSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IconSource<T>(this T control, Func<FluentAvalonia.UI.Controls.IconSource> func, Action<FluentAvalonia.UI.Controls.IconSource>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.IconSourceProperty, func, onChanged, expression);
public static T IconSource<T>(this T control, FluentAvalonia.UI.Controls.IconSource value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.IconSourceProperty, ps, () => control.IconSource = value, bindingMode, converter, bindingSource);
public static T IconSource<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.IconSource> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.IconSourceProperty, ps, () => control.IconSource = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Buttons<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.ButtonsProperty, binding);
public static T Buttons<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.ButtonsProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Buttons<T>(this T control, Func<System.Collections.Generic.IList<FluentAvalonia.UI.Controls.TaskDialogButton>> func, Action<System.Collections.Generic.IList<FluentAvalonia.UI.Controls.TaskDialogButton>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.ButtonsProperty, func, onChanged, expression);
public static T Buttons<T>(this T control, System.Collections.Generic.IList<FluentAvalonia.UI.Controls.TaskDialogButton> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.ButtonsProperty, ps, () => control.Buttons = value, bindingMode, converter, bindingSource);
public static T Buttons<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.Generic.IList<FluentAvalonia.UI.Controls.TaskDialogButton>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.ButtonsProperty, ps, () => control.Buttons = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Commands<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.CommandsProperty, binding);
public static T Commands<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.CommandsProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Commands<T>(this T control, Func<System.Collections.Generic.IList<FluentAvalonia.UI.Controls.TaskDialogCommand>> func, Action<System.Collections.Generic.IList<FluentAvalonia.UI.Controls.TaskDialogCommand>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.CommandsProperty, func, onChanged, expression);
public static T Commands<T>(this T control, System.Collections.Generic.IList<FluentAvalonia.UI.Controls.TaskDialogCommand> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.CommandsProperty, ps, () => control.Commands = value, bindingMode, converter, bindingSource);
public static T Commands<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.Generic.IList<FluentAvalonia.UI.Controls.TaskDialogCommand>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.CommandsProperty, ps, () => control.Commands = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T FooterVisibility<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.FooterVisibilityProperty, binding);
public static T FooterVisibility<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.FooterVisibilityProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T FooterVisibility<T>(this T control, Func<FluentAvalonia.UI.Controls.TaskDialogFooterVisibility> func, Action<FluentAvalonia.UI.Controls.TaskDialogFooterVisibility>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.FooterVisibilityProperty, func, onChanged, expression);
public static T FooterVisibility<T>(this T control, FluentAvalonia.UI.Controls.TaskDialogFooterVisibility value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.FooterVisibilityProperty, ps, () => control.FooterVisibility = value, bindingMode, converter, bindingSource);
public static T FooterVisibility<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.TaskDialogFooterVisibility> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.FooterVisibilityProperty, ps, () => control.FooterVisibility = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsFooterExpanded<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.IsFooterExpandedProperty, binding);
public static T IsFooterExpanded<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.IsFooterExpandedProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsFooterExpanded<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.IsFooterExpandedProperty, func, onChanged, expression);
public static T IsFooterExpanded<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.IsFooterExpandedProperty, ps, () => control.IsFooterExpanded = value, bindingMode, converter, bindingSource);
public static T IsFooterExpanded<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.IsFooterExpandedProperty, ps, () => control.IsFooterExpanded = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Footer<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.FooterProperty, binding);
public static T Footer<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.FooterProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Footer<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.FooterProperty, func, onChanged, expression);
public static T Footer<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.FooterProperty, ps, () => control.Footer = value, bindingMode, converter, bindingSource);
public static T Footer<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.FooterProperty, ps, () => control.Footer = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T FooterTemplate<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.FooterTemplateProperty, binding);
public static T FooterTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.FooterTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T FooterTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.FooterTemplateProperty, func, onChanged, expression);
public static T FooterTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.FooterTemplateProperty, ps, () => control.FooterTemplate = value, bindingMode, converter, bindingSource);
public static T FooterTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.FooterTemplateProperty, ps, () => control.FooterTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ShowProgressBar<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.ShowProgressBarProperty, binding);
public static T ShowProgressBar<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.ShowProgressBarProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ShowProgressBar<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.ShowProgressBarProperty, func, onChanged, expression);
public static T ShowProgressBar<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.ShowProgressBarProperty, ps, () => control.ShowProgressBar = value, bindingMode, converter, bindingSource);
public static T ShowProgressBar<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.ShowProgressBarProperty, ps, () => control.ShowProgressBar = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T HeaderBackground<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.HeaderBackgroundProperty, binding);
public static T HeaderBackground<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.HeaderBackgroundProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T HeaderBackground<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.HeaderBackgroundProperty, func, onChanged, expression);
public static T HeaderBackground<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.HeaderBackgroundProperty, ps, () => control.HeaderBackground = value, bindingMode, converter, bindingSource);
public static T HeaderBackground<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.HeaderBackgroundProperty, ps, () => control.HeaderBackground = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T HeaderForeground<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.HeaderForegroundProperty, binding);
public static T HeaderForeground<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.HeaderForegroundProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T HeaderForeground<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.HeaderForegroundProperty, func, onChanged, expression);
public static T HeaderForeground<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.HeaderForegroundProperty, ps, () => control.HeaderForeground = value, bindingMode, converter, bindingSource);
public static T HeaderForeground<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.HeaderForegroundProperty, ps, () => control.HeaderForeground = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IconForeground<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.IconForegroundProperty, binding);
public static T IconForeground<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.IconForegroundProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IconForeground<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TaskDialog
   => control._set(FluentAvalonia.UI.Controls.TaskDialog.IconForegroundProperty, func, onChanged, expression);
public static T IconForeground<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.IconForegroundProperty, ps, () => control.IconForeground = value, bindingMode, converter, bindingSource);
public static T IconForeground<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialog
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialog.IconForegroundProperty, ps, () => control.IconForeground = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

