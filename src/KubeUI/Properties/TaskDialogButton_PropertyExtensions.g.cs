#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TaskDialogButton = FluentAvalonia.UI.Controls.TaskDialogButton;

namespace Avalonia.Markup.Declarative;
public static partial class TaskDialogButtonExtensions
{
public static T IconSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialogButton
   => control._set(FluentAvalonia.UI.Controls.TaskDialogButton.IconSourceProperty, binding);
public static T IconSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TaskDialogButton
   => control._set(FluentAvalonia.UI.Controls.TaskDialogButton.IconSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IconSource<T>(this T control, Func<FluentAvalonia.UI.Controls.IconSource> func, Action<FluentAvalonia.UI.Controls.IconSource>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TaskDialogButton
   => control._set(FluentAvalonia.UI.Controls.TaskDialogButton.IconSourceProperty, func, onChanged, expression);
public static T IconSource<T>(this T control, FluentAvalonia.UI.Controls.IconSource value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialogButton
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialogButton.IconSourceProperty, ps, () => control.IconSource = value, bindingMode, converter, bindingSource);
public static T IconSource<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.IconSource> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialogButton
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialogButton.IconSourceProperty, ps, () => control.IconSource = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Command<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialogButton
   => control._set(FluentAvalonia.UI.Controls.TaskDialogButton.CommandProperty, binding);
public static T Command<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TaskDialogButton
   => control._set(FluentAvalonia.UI.Controls.TaskDialogButton.CommandProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Command<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TaskDialogButton
   => control._set(FluentAvalonia.UI.Controls.TaskDialogButton.CommandProperty, func, onChanged, expression);
public static T Command<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialogButton
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialogButton.CommandProperty, ps, () => control.Command = value, bindingMode, converter, bindingSource);
public static T Command<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialogButton
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialogButton.CommandProperty, ps, () => control.Command = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T CommandParameter<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialogButton
   => control._set(FluentAvalonia.UI.Controls.TaskDialogButton.CommandParameterProperty, binding);
public static T CommandParameter<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TaskDialogButton
   => control._set(FluentAvalonia.UI.Controls.TaskDialogButton.CommandParameterProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CommandParameter<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TaskDialogButton
   => control._set(FluentAvalonia.UI.Controls.TaskDialogButton.CommandParameterProperty, func, onChanged, expression);
public static T CommandParameter<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialogButton
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialogButton.CommandParameterProperty, ps, () => control.CommandParameter = value, bindingMode, converter, bindingSource);
public static T CommandParameter<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialogButton
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialogButton.CommandParameterProperty, ps, () => control.CommandParameter = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

