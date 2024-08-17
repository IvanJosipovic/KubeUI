#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using ToggleMenuFlyoutItem = FluentAvalonia.UI.Controls.ToggleMenuFlyoutItem;

namespace Avalonia.Markup.Declarative;
public static partial class ToggleMenuFlyoutItemExtensions
{
public static T IsChecked<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ToggleMenuFlyoutItem
   => control._set(FluentAvalonia.UI.Controls.ToggleMenuFlyoutItem.IsCheckedProperty, binding);
public static T IsChecked<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ToggleMenuFlyoutItem
   => control._set(FluentAvalonia.UI.Controls.ToggleMenuFlyoutItem.IsCheckedProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsChecked<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ToggleMenuFlyoutItem
   => control._set(FluentAvalonia.UI.Controls.ToggleMenuFlyoutItem.IsCheckedProperty, func, onChanged, expression);
public static T IsChecked<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ToggleMenuFlyoutItem
=> control._setEx(FluentAvalonia.UI.Controls.ToggleMenuFlyoutItem.IsCheckedProperty, ps, () => control.IsChecked = value, bindingMode, converter, bindingSource);
public static T IsChecked<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ToggleMenuFlyoutItem
=> control._setEx(FluentAvalonia.UI.Controls.ToggleMenuFlyoutItem.IsCheckedProperty, ps, () => control.IsChecked = converter.TryConvert(value), bindingMode, converter, bindingSource);
}
