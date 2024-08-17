#nullable enable
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using InfoBar = FluentAvalonia.UI.Controls.InfoBar;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Avalonia.Markup.Declarative;
public static partial class InfoBarExtensions
{
public static T IsOpen<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar
   => control._set(FluentAvalonia.UI.Controls.InfoBar.IsOpenProperty, binding);
public static T IsOpen<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.InfoBar
   => control._set(FluentAvalonia.UI.Controls.InfoBar.IsOpenProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsOpen<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.InfoBar
   => control._set(FluentAvalonia.UI.Controls.InfoBar.IsOpenProperty, func, onChanged, expression);
public static T IsOpen<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.IsOpenProperty, ps, () => control.IsOpen = value, bindingMode, converter, bindingSource);
public static T IsOpen<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.IsOpenProperty, ps, () => control.IsOpen = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Title<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar
   => control._set(FluentAvalonia.UI.Controls.InfoBar.TitleProperty, binding);
public static T Title<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.InfoBar
   => control._set(FluentAvalonia.UI.Controls.InfoBar.TitleProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Title<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.InfoBar
   => control._set(FluentAvalonia.UI.Controls.InfoBar.TitleProperty, func, onChanged, expression);
public static T Title<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.TitleProperty, ps, () => control.Title = value, bindingMode, converter, bindingSource);
public static T Title<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.TitleProperty, ps, () => control.Title = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Message<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar
   => control._set(FluentAvalonia.UI.Controls.InfoBar.MessageProperty, binding);
public static T Message<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.InfoBar
   => control._set(FluentAvalonia.UI.Controls.InfoBar.MessageProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Message<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.InfoBar
   => control._set(FluentAvalonia.UI.Controls.InfoBar.MessageProperty, func, onChanged, expression);
public static T Message<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.MessageProperty, ps, () => control.Message = value, bindingMode, converter, bindingSource);
public static T Message<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.MessageProperty, ps, () => control.Message = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Severity<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar
   => control._set(FluentAvalonia.UI.Controls.InfoBar.SeverityProperty, binding);
public static T Severity<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.InfoBar
   => control._set(FluentAvalonia.UI.Controls.InfoBar.SeverityProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Severity<T>(this T control, Func<FluentAvalonia.UI.Controls.InfoBarSeverity> func, Action<FluentAvalonia.UI.Controls.InfoBarSeverity>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.InfoBar
   => control._set(FluentAvalonia.UI.Controls.InfoBar.SeverityProperty, func, onChanged, expression);
public static T Severity<T>(this T control, FluentAvalonia.UI.Controls.InfoBarSeverity value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.SeverityProperty, ps, () => control.Severity = value, bindingMode, converter, bindingSource);
public static T Severity<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.InfoBarSeverity> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.SeverityProperty, ps, () => control.Severity = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IconSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar
   => control._set(FluentAvalonia.UI.Controls.InfoBar.IconSourceProperty, binding);
public static T IconSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.InfoBar
   => control._set(FluentAvalonia.UI.Controls.InfoBar.IconSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IconSource<T>(this T control, Func<FluentAvalonia.UI.Controls.IconSource> func, Action<FluentAvalonia.UI.Controls.IconSource>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.InfoBar
   => control._set(FluentAvalonia.UI.Controls.InfoBar.IconSourceProperty, func, onChanged, expression);
public static T IconSource<T>(this T control, FluentAvalonia.UI.Controls.IconSource value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.IconSourceProperty, ps, () => control.IconSource = value, bindingMode, converter, bindingSource);
public static T IconSource<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.IconSource> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.IconSourceProperty, ps, () => control.IconSource = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsIconVisible<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar
   => control._set(FluentAvalonia.UI.Controls.InfoBar.IsIconVisibleProperty, binding);
public static T IsIconVisible<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.InfoBar
   => control._set(FluentAvalonia.UI.Controls.InfoBar.IsIconVisibleProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsIconVisible<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.InfoBar
   => control._set(FluentAvalonia.UI.Controls.InfoBar.IsIconVisibleProperty, func, onChanged, expression);
public static T IsIconVisible<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.IsIconVisibleProperty, ps, () => control.IsIconVisible = value, bindingMode, converter, bindingSource);
public static T IsIconVisible<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.IsIconVisibleProperty, ps, () => control.IsIconVisible = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsClosable<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar
   => control._set(FluentAvalonia.UI.Controls.InfoBar.IsClosableProperty, binding);
public static T IsClosable<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.InfoBar
   => control._set(FluentAvalonia.UI.Controls.InfoBar.IsClosableProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsClosable<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.InfoBar
   => control._set(FluentAvalonia.UI.Controls.InfoBar.IsClosableProperty, func, onChanged, expression);
public static T IsClosable<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.IsClosableProperty, ps, () => control.IsClosable = value, bindingMode, converter, bindingSource);
public static T IsClosable<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.IsClosableProperty, ps, () => control.IsClosable = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T CloseButtonCommand<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar
   => control._set(FluentAvalonia.UI.Controls.InfoBar.CloseButtonCommandProperty, binding);
public static T CloseButtonCommand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.InfoBar
   => control._set(FluentAvalonia.UI.Controls.InfoBar.CloseButtonCommandProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CloseButtonCommand<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.InfoBar
   => control._set(FluentAvalonia.UI.Controls.InfoBar.CloseButtonCommandProperty, func, onChanged, expression);
public static T CloseButtonCommand<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.CloseButtonCommandProperty, ps, () => control.CloseButtonCommand = value, bindingMode, converter, bindingSource);
public static T CloseButtonCommand<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.CloseButtonCommandProperty, ps, () => control.CloseButtonCommand = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T CloseButtonCommandParameter<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar
   => control._set(FluentAvalonia.UI.Controls.InfoBar.CloseButtonCommandParameterProperty, binding);
public static T CloseButtonCommandParameter<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.InfoBar
   => control._set(FluentAvalonia.UI.Controls.InfoBar.CloseButtonCommandParameterProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CloseButtonCommandParameter<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.InfoBar
   => control._set(FluentAvalonia.UI.Controls.InfoBar.CloseButtonCommandParameterProperty, func, onChanged, expression);
public static T CloseButtonCommandParameter<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.CloseButtonCommandParameterProperty, ps, () => control.CloseButtonCommandParameter = value, bindingMode, converter, bindingSource);
public static T CloseButtonCommandParameter<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.CloseButtonCommandParameterProperty, ps, () => control.CloseButtonCommandParameter = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ActionButton<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar
   => control._set(FluentAvalonia.UI.Controls.InfoBar.ActionButtonProperty, binding);
public static T ActionButton<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.InfoBar
   => control._set(FluentAvalonia.UI.Controls.InfoBar.ActionButtonProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ActionButton<T>(this T control, Func<Avalonia.Controls.Control> func, Action<Avalonia.Controls.Control>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.InfoBar
   => control._set(FluentAvalonia.UI.Controls.InfoBar.ActionButtonProperty, func, onChanged, expression);
public static T ActionButton<T>(this T control, Avalonia.Controls.Control value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.ActionButtonProperty, ps, () => control.ActionButton = value, bindingMode, converter, bindingSource);
public static T ActionButton<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Control> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.ActionButtonProperty, ps, () => control.ActionButton = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

