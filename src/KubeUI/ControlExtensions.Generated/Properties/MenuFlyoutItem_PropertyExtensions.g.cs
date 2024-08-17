#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using FluentAvalonia.UI.Controls;
using MenuFlyoutItem = FluentAvalonia.UI.Controls.MenuFlyoutItem;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Avalonia.Markup.Declarative;
public static partial class MenuFlyoutItemExtensions
{
public static T Text<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutItem.TextProperty, binding);
public static T Text<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutItem.TextProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Text<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutItem.TextProperty, func, onChanged, expression);
public static T Text<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutItem.TextProperty, ps, () => control.Text = value, bindingMode, converter, bindingSource);
public static T Text<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutItem.TextProperty, ps, () => control.Text = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IconSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutItem.IconSourceProperty, binding);
public static T IconSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutItem.IconSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IconSource<T>(this T control, Func<FluentAvalonia.UI.Controls.IconSource> func, Action<FluentAvalonia.UI.Controls.IconSource>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutItem.IconSourceProperty, func, onChanged, expression);
public static T IconSource<T>(this T control, FluentAvalonia.UI.Controls.IconSource value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutItem.IconSourceProperty, ps, () => control.IconSource = value, bindingMode, converter, bindingSource);
public static T IconSource<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.IconSource> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutItem.IconSourceProperty, ps, () => control.IconSource = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Command<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutItem.CommandProperty, binding);
public static T Command<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutItem.CommandProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Command<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutItem.CommandProperty, func, onChanged, expression);
public static T Command<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutItem.CommandProperty, ps, () => control.Command = value, bindingMode, converter, bindingSource);
public static T Command<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutItem.CommandProperty, ps, () => control.Command = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T CommandParameter<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutItem.CommandParameterProperty, binding);
public static T CommandParameter<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutItem.CommandParameterProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CommandParameter<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutItem.CommandParameterProperty, func, onChanged, expression);
public static T CommandParameter<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutItem.CommandParameterProperty, ps, () => control.CommandParameter = value, bindingMode, converter, bindingSource);
public static T CommandParameter<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutItem.CommandParameterProperty, ps, () => control.CommandParameter = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T HotKey<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutItem.HotKeyProperty, binding);
public static T HotKey<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutItem.HotKeyProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T HotKey<T>(this T control, Func<Avalonia.Input.KeyGesture> func, Action<Avalonia.Input.KeyGesture>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutItem.HotKeyProperty, func, onChanged, expression);
public static T HotKey<T>(this T control, Avalonia.Input.KeyGesture value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutItem.HotKeyProperty, ps, () => control.HotKey = value, bindingMode, converter, bindingSource);
public static T HotKey<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Input.KeyGesture> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutItem.HotKeyProperty, ps, () => control.HotKey = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T InputGesture<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutItem.InputGestureProperty, binding);
public static T InputGesture<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutItem.InputGestureProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T InputGesture<T>(this T control, Func<Avalonia.Input.KeyGesture> func, Action<Avalonia.Input.KeyGesture>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutItem.InputGestureProperty, func, onChanged, expression);
public static T InputGesture<T>(this T control, Avalonia.Input.KeyGesture value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutItem.InputGestureProperty, ps, () => control.InputGesture = value, bindingMode, converter, bindingSource);
public static T InputGesture<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Input.KeyGesture> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutItem.InputGestureProperty, ps, () => control.InputGesture = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

