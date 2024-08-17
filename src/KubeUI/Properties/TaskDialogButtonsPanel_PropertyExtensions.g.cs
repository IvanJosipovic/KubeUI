#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls.Primitives;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TaskDialogButtonsPanel = FluentAvalonia.UI.Controls.Primitives.TaskDialogButtonsPanel;

namespace Avalonia.Markup.Declarative;
public static partial class TaskDialogButtonsPanelExtensions
{
public static T Spacing<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.Primitives.TaskDialogButtonsPanel
   => control._set(FluentAvalonia.UI.Controls.Primitives.TaskDialogButtonsPanel.SpacingProperty, binding);
public static T Spacing<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.Primitives.TaskDialogButtonsPanel
   => control._set(FluentAvalonia.UI.Controls.Primitives.TaskDialogButtonsPanel.SpacingProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Spacing<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.Primitives.TaskDialogButtonsPanel
   => control._set(FluentAvalonia.UI.Controls.Primitives.TaskDialogButtonsPanel.SpacingProperty, func, onChanged, expression);
public static T Spacing<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Primitives.TaskDialogButtonsPanel
=> control._setEx(FluentAvalonia.UI.Controls.Primitives.TaskDialogButtonsPanel.SpacingProperty, ps, () => control.Spacing = value, bindingMode, converter, bindingSource);
public static T Spacing<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Primitives.TaskDialogButtonsPanel
=> control._setEx(FluentAvalonia.UI.Controls.Primitives.TaskDialogButtonsPanel.SpacingProperty, ps, () => control.Spacing = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

