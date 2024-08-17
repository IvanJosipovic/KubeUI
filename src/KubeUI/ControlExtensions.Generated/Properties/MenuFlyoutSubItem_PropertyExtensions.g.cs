#nullable enable
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Styling;
using FluentAvalonia.UI.Controls;
using MenuFlyoutSubItem = FluentAvalonia.UI.Controls.MenuFlyoutSubItem;
using System;
using System.Collections;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class MenuFlyoutSubItemExtensions
{
public static T Text<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.TextProperty, binding);
public static T Text<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.TextProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Text<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.TextProperty, func, onChanged, expression);
public static T Text<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.TextProperty, ps, () => control.Text = value, bindingMode, converter, bindingSource);
public static T Text<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.TextProperty, ps, () => control.Text = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IconSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.IconSourceProperty, binding);
public static T IconSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.IconSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IconSource<T>(this T control, Func<FluentAvalonia.UI.Controls.IconSource> func, Action<FluentAvalonia.UI.Controls.IconSource>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.IconSourceProperty, func, onChanged, expression);
public static T IconSource<T>(this T control, FluentAvalonia.UI.Controls.IconSource value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.IconSourceProperty, ps, () => control.IconSource = value, bindingMode, converter, bindingSource);
public static T IconSource<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.IconSource> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.IconSourceProperty, ps, () => control.IconSource = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ItemsSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemsSourceProperty, binding);
public static T ItemsSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemsSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ItemsSource<T>(this T control, Func<System.Collections.IEnumerable> func, Action<System.Collections.IEnumerable>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemsSourceProperty, func, onChanged, expression);
public static T ItemsSource<T>(this T control, System.Collections.IEnumerable value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemsSourceProperty, ps, () => control.ItemsSource = value, bindingMode, converter, bindingSource);
public static T ItemsSource<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.IEnumerable> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemsSourceProperty, ps, () => control.ItemsSource = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ItemTemplate<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemTemplateProperty, binding);
public static T ItemTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ItemTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemTemplateProperty, func, onChanged, expression);
public static T ItemTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemTemplateProperty, ps, () => control.ItemTemplate = value, bindingMode, converter, bindingSource);
public static T ItemTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemTemplateProperty, ps, () => control.ItemTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ItemContainerTheme<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemContainerThemeProperty, binding);
public static T ItemContainerTheme<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemContainerThemeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ItemContainerTheme<T>(this T control, Func<Avalonia.Styling.ControlTheme> func, Action<Avalonia.Styling.ControlTheme>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemContainerThemeProperty, func, onChanged, expression);
public static T ItemContainerTheme<T>(this T control, Avalonia.Styling.ControlTheme value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemContainerThemeProperty, ps, () => control.ItemContainerTheme = value, bindingMode, converter, bindingSource);
public static T ItemContainerTheme<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ControlTheme> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemContainerThemeProperty, ps, () => control.ItemContainerTheme = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

