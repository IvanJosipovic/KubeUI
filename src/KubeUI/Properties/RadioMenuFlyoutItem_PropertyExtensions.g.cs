#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using RadioMenuFlyoutItem = FluentAvalonia.UI.Controls.RadioMenuFlyoutItem;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class RadioMenuFlyoutItemExtensions
{
public static T GroupName<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.RadioMenuFlyoutItem
   => control._set(FluentAvalonia.UI.Controls.RadioMenuFlyoutItem.GroupNameProperty, binding);
public static T GroupName<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.RadioMenuFlyoutItem
   => control._set(FluentAvalonia.UI.Controls.RadioMenuFlyoutItem.GroupNameProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T GroupName<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.RadioMenuFlyoutItem
   => control._set(FluentAvalonia.UI.Controls.RadioMenuFlyoutItem.GroupNameProperty, func, onChanged, expression);
public static T GroupName<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.RadioMenuFlyoutItem
=> control._setEx(FluentAvalonia.UI.Controls.RadioMenuFlyoutItem.GroupNameProperty, ps, () => control.GroupName = value, bindingMode, converter, bindingSource);
public static T GroupName<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.RadioMenuFlyoutItem
=> control._setEx(FluentAvalonia.UI.Controls.RadioMenuFlyoutItem.GroupNameProperty, ps, () => control.GroupName = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsChecked<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.RadioMenuFlyoutItem
   => control._set(FluentAvalonia.UI.Controls.RadioMenuFlyoutItem.IsCheckedProperty, binding);
public static T IsChecked<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.RadioMenuFlyoutItem
   => control._set(FluentAvalonia.UI.Controls.RadioMenuFlyoutItem.IsCheckedProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsChecked<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.RadioMenuFlyoutItem
   => control._set(FluentAvalonia.UI.Controls.RadioMenuFlyoutItem.IsCheckedProperty, func, onChanged, expression);
public static T IsChecked<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.RadioMenuFlyoutItem
=> control._setEx(FluentAvalonia.UI.Controls.RadioMenuFlyoutItem.IsCheckedProperty, ps, () => control.IsChecked = value, bindingMode, converter, bindingSource);
public static T IsChecked<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.RadioMenuFlyoutItem
=> control._setEx(FluentAvalonia.UI.Controls.RadioMenuFlyoutItem.IsCheckedProperty, ps, () => control.IsChecked = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

