#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TaskDialogCommand = FluentAvalonia.UI.Controls.TaskDialogCommand;

namespace Avalonia.Markup.Declarative;
public static partial class TaskDialogCommandExtensions
{
public static T Description<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialogCommand
   => control._set(FluentAvalonia.UI.Controls.TaskDialogCommand.DescriptionProperty, binding);
public static T Description<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TaskDialogCommand
   => control._set(FluentAvalonia.UI.Controls.TaskDialogCommand.DescriptionProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Description<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TaskDialogCommand
   => control._set(FluentAvalonia.UI.Controls.TaskDialogCommand.DescriptionProperty, func, onChanged, expression);
public static T Description<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialogCommand
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialogCommand.DescriptionProperty, ps, () => control.Description = value, bindingMode, converter, bindingSource);
public static T Description<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialogCommand
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialogCommand.DescriptionProperty, ps, () => control.Description = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

