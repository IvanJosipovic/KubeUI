#nullable enable
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TabViewItem = FluentAvalonia.UI.Controls.TabViewItem;

namespace Avalonia.Markup.Declarative;
public static partial class TabViewItemExtensions
{
public static T Header<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabViewItem
   => control._set(FluentAvalonia.UI.Controls.TabViewItem.HeaderProperty, binding);
public static T Header<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabViewItem
   => control._set(FluentAvalonia.UI.Controls.TabViewItem.HeaderProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Header<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabViewItem
   => control._set(FluentAvalonia.UI.Controls.TabViewItem.HeaderProperty, func, onChanged, expression);
public static T Header<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabViewItem
=> control._setEx(FluentAvalonia.UI.Controls.TabViewItem.HeaderProperty, ps, () => control.Header = value, bindingMode, converter, bindingSource);
public static T Header<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabViewItem
=> control._setEx(FluentAvalonia.UI.Controls.TabViewItem.HeaderProperty, ps, () => control.Header = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T HeaderTemplate<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabViewItem
   => control._set(FluentAvalonia.UI.Controls.TabViewItem.HeaderTemplateProperty, binding);
public static T HeaderTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabViewItem
   => control._set(FluentAvalonia.UI.Controls.TabViewItem.HeaderTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T HeaderTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabViewItem
   => control._set(FluentAvalonia.UI.Controls.TabViewItem.HeaderTemplateProperty, func, onChanged, expression);
public static T HeaderTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabViewItem
=> control._setEx(FluentAvalonia.UI.Controls.TabViewItem.HeaderTemplateProperty, ps, () => control.HeaderTemplate = value, bindingMode, converter, bindingSource);
public static T HeaderTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabViewItem
=> control._setEx(FluentAvalonia.UI.Controls.TabViewItem.HeaderTemplateProperty, ps, () => control.HeaderTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IconSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabViewItem
   => control._set(FluentAvalonia.UI.Controls.TabViewItem.IconSourceProperty, binding);
public static T IconSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabViewItem
   => control._set(FluentAvalonia.UI.Controls.TabViewItem.IconSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IconSource<T>(this T control, Func<FluentAvalonia.UI.Controls.IconSource> func, Action<FluentAvalonia.UI.Controls.IconSource>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabViewItem
   => control._set(FluentAvalonia.UI.Controls.TabViewItem.IconSourceProperty, func, onChanged, expression);
public static T IconSource<T>(this T control, FluentAvalonia.UI.Controls.IconSource value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabViewItem
=> control._setEx(FluentAvalonia.UI.Controls.TabViewItem.IconSourceProperty, ps, () => control.IconSource = value, bindingMode, converter, bindingSource);
public static T IconSource<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.IconSource> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabViewItem
=> control._setEx(FluentAvalonia.UI.Controls.TabViewItem.IconSourceProperty, ps, () => control.IconSource = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsClosable<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabViewItem
   => control._set(FluentAvalonia.UI.Controls.TabViewItem.IsClosableProperty, binding);
public static T IsClosable<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabViewItem
   => control._set(FluentAvalonia.UI.Controls.TabViewItem.IsClosableProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsClosable<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabViewItem
   => control._set(FluentAvalonia.UI.Controls.TabViewItem.IsClosableProperty, func, onChanged, expression);
public static T IsClosable<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabViewItem
=> control._setEx(FluentAvalonia.UI.Controls.TabViewItem.IsClosableProperty, ps, () => control.IsClosable = value, bindingMode, converter, bindingSource);
public static T IsClosable<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabViewItem
=> control._setEx(FluentAvalonia.UI.Controls.TabViewItem.IsClosableProperty, ps, () => control.IsClosable = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

