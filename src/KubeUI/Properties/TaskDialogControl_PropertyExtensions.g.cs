#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TaskDialogControl = FluentAvalonia.UI.Controls.TaskDialogControl;

namespace Avalonia.Markup.Declarative;
public static partial class TaskDialogControlExtensions
{
public static T Text<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialogControl
   => control._set(FluentAvalonia.UI.Controls.TaskDialogControl.TextProperty, binding);
public static T Text<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TaskDialogControl
   => control._set(FluentAvalonia.UI.Controls.TaskDialogControl.TextProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Text<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TaskDialogControl
   => control._set(FluentAvalonia.UI.Controls.TaskDialogControl.TextProperty, func, onChanged, expression);
public static T Text<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialogControl
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialogControl.TextProperty, ps, () => control.Text = value, bindingMode, converter, bindingSource);
public static T Text<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialogControl
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialogControl.TextProperty, ps, () => control.Text = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T DialogResult<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialogControl
   => control._set(FluentAvalonia.UI.Controls.TaskDialogControl.DialogResultProperty, binding);
public static T DialogResult<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TaskDialogControl
   => control._set(FluentAvalonia.UI.Controls.TaskDialogControl.DialogResultProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T DialogResult<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TaskDialogControl
   => control._set(FluentAvalonia.UI.Controls.TaskDialogControl.DialogResultProperty, func, onChanged, expression);
public static T DialogResult<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialogControl
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialogControl.DialogResultProperty, ps, () => control.DialogResult = value, bindingMode, converter, bindingSource);
public static T DialogResult<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialogControl
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialogControl.DialogResultProperty, ps, () => control.DialogResult = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsEnabled<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialogControl
   => control._set(FluentAvalonia.UI.Controls.TaskDialogControl.IsEnabledProperty, binding);
public static T IsEnabled<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TaskDialogControl
   => control._set(FluentAvalonia.UI.Controls.TaskDialogControl.IsEnabledProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsEnabled<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TaskDialogControl
   => control._set(FluentAvalonia.UI.Controls.TaskDialogControl.IsEnabledProperty, func, onChanged, expression);
public static T IsEnabled<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialogControl
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialogControl.IsEnabledProperty, ps, () => control.IsEnabled = value, bindingMode, converter, bindingSource);
public static T IsEnabled<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialogControl
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialogControl.IsEnabledProperty, ps, () => control.IsEnabled = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsDefault<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialogControl
   => control._set(FluentAvalonia.UI.Controls.TaskDialogControl.IsDefaultProperty, binding);
public static T IsDefault<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TaskDialogControl
   => control._set(FluentAvalonia.UI.Controls.TaskDialogControl.IsDefaultProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsDefault<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TaskDialogControl
   => control._set(FluentAvalonia.UI.Controls.TaskDialogControl.IsDefaultProperty, func, onChanged, expression);
public static T IsDefault<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialogControl
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialogControl.IsDefaultProperty, ps, () => control.IsDefault = value, bindingMode, converter, bindingSource);
public static T IsDefault<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialogControl
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialogControl.IsDefaultProperty, ps, () => control.IsDefault = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

