#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using CommandBarFlyout = FluentAvalonia.UI.Controls.CommandBarFlyout;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class CommandBarFlyoutExtensions
{
public static T AlwaysExpanded<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBarFlyout
   => control._set(FluentAvalonia.UI.Controls.CommandBarFlyout.AlwaysExpandedProperty, binding);
public static T AlwaysExpanded<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.CommandBarFlyout
   => control._set(FluentAvalonia.UI.Controls.CommandBarFlyout.AlwaysExpandedProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T AlwaysExpanded<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.CommandBarFlyout
   => control._set(FluentAvalonia.UI.Controls.CommandBarFlyout.AlwaysExpandedProperty, func, onChanged, expression);
public static T AlwaysExpanded<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBarFlyout
=> control._setEx(FluentAvalonia.UI.Controls.CommandBarFlyout.AlwaysExpandedProperty, ps, () => control.AlwaysExpanded = value, bindingMode, converter, bindingSource);
public static T AlwaysExpanded<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBarFlyout
=> control._setEx(FluentAvalonia.UI.Controls.CommandBarFlyout.AlwaysExpandedProperty, ps, () => control.AlwaysExpanded = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

