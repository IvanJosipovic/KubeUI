#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Controls.Primitives;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TaskDialogButtonHost = FluentAvalonia.UI.Controls.Primitives.TaskDialogButtonHost;

namespace Avalonia.Markup.Declarative;
public static partial class TaskDialogButtonHostExtensions
{
public static T IconSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.Primitives.TaskDialogButtonHost
   => control._set(FluentAvalonia.UI.Controls.Primitives.TaskDialogButtonHost.IconSourceProperty, binding);
public static T IconSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.Primitives.TaskDialogButtonHost
   => control._set(FluentAvalonia.UI.Controls.Primitives.TaskDialogButtonHost.IconSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IconSource<T>(this T control, Func<FluentAvalonia.UI.Controls.IconSource> func, Action<FluentAvalonia.UI.Controls.IconSource>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.Primitives.TaskDialogButtonHost
   => control._set(FluentAvalonia.UI.Controls.Primitives.TaskDialogButtonHost.IconSourceProperty, func, onChanged, expression);
public static T IconSource<T>(this T control, FluentAvalonia.UI.Controls.IconSource value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Primitives.TaskDialogButtonHost
=> control._setEx(FluentAvalonia.UI.Controls.Primitives.TaskDialogButtonHost.IconSourceProperty, ps, () => control.IconSource = value, bindingMode, converter, bindingSource);
public static T IconSource<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.IconSource> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Primitives.TaskDialogButtonHost
=> control._setEx(FluentAvalonia.UI.Controls.Primitives.TaskDialogButtonHost.IconSourceProperty, ps, () => control.IconSource = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

