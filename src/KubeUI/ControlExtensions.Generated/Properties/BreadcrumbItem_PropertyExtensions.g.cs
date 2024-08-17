#nullable enable
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using BreadcrumbItem = Ursa.Controls.BreadcrumbItem;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class BreadcrumbItemExtensions
{
public static T Separator<T>(this T control, IBinding binding) where T : Ursa.Controls.BreadcrumbItem
   => control._set(Ursa.Controls.BreadcrumbItem.SeparatorProperty, binding);
public static T Separator<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.BreadcrumbItem
   => control._set(Ursa.Controls.BreadcrumbItem.SeparatorProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Separator<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.BreadcrumbItem
   => control._set(Ursa.Controls.BreadcrumbItem.SeparatorProperty, func, onChanged, expression);
public static T Separator<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.BreadcrumbItem
=> control._setEx(Ursa.Controls.BreadcrumbItem.SeparatorProperty, ps, () => control.Separator = value, bindingMode, converter, bindingSource);
public static T Separator<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.BreadcrumbItem
=> control._setEx(Ursa.Controls.BreadcrumbItem.SeparatorProperty, ps, () => control.Separator = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Icon<T>(this T control, IBinding binding) where T : Ursa.Controls.BreadcrumbItem
   => control._set(Ursa.Controls.BreadcrumbItem.IconProperty, binding);
public static T Icon<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.BreadcrumbItem
   => control._set(Ursa.Controls.BreadcrumbItem.IconProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Icon<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.BreadcrumbItem
   => control._set(Ursa.Controls.BreadcrumbItem.IconProperty, func, onChanged, expression);
public static T Icon<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.BreadcrumbItem
=> control._setEx(Ursa.Controls.BreadcrumbItem.IconProperty, ps, () => control.Icon = value, bindingMode, converter, bindingSource);
public static T Icon<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.BreadcrumbItem
=> control._setEx(Ursa.Controls.BreadcrumbItem.IconProperty, ps, () => control.Icon = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Command<T>(this T control, IBinding binding) where T : Ursa.Controls.BreadcrumbItem
   => control._set(Ursa.Controls.BreadcrumbItem.CommandProperty, binding);
public static T Command<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.BreadcrumbItem
   => control._set(Ursa.Controls.BreadcrumbItem.CommandProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Command<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.BreadcrumbItem
   => control._set(Ursa.Controls.BreadcrumbItem.CommandProperty, func, onChanged, expression);
public static T Command<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.BreadcrumbItem
=> control._setEx(Ursa.Controls.BreadcrumbItem.CommandProperty, ps, () => control.Command = value, bindingMode, converter, bindingSource);
public static T Command<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.BreadcrumbItem
=> control._setEx(Ursa.Controls.BreadcrumbItem.CommandProperty, ps, () => control.Command = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IconTemplate<T>(this T control, IBinding binding) where T : Ursa.Controls.BreadcrumbItem
   => control._set(Ursa.Controls.BreadcrumbItem.IconTemplateProperty, binding);
public static T IconTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.BreadcrumbItem
   => control._set(Ursa.Controls.BreadcrumbItem.IconTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IconTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.BreadcrumbItem
   => control._set(Ursa.Controls.BreadcrumbItem.IconTemplateProperty, func, onChanged, expression);
public static T IconTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.BreadcrumbItem
=> control._setEx(Ursa.Controls.BreadcrumbItem.IconTemplateProperty, ps, () => control.IconTemplate = value, bindingMode, converter, bindingSource);
public static T IconTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.BreadcrumbItem
=> control._setEx(Ursa.Controls.BreadcrumbItem.IconTemplateProperty, ps, () => control.IconTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsReadOnly<T>(this T control, IBinding binding) where T : Ursa.Controls.BreadcrumbItem
   => control._set(Ursa.Controls.BreadcrumbItem.IsReadOnlyProperty, binding);
public static T IsReadOnly<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.BreadcrumbItem
   => control._set(Ursa.Controls.BreadcrumbItem.IsReadOnlyProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsReadOnly<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.BreadcrumbItem
   => control._set(Ursa.Controls.BreadcrumbItem.IsReadOnlyProperty, func, onChanged, expression);
public static T IsReadOnly<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.BreadcrumbItem
=> control._setEx(Ursa.Controls.BreadcrumbItem.IsReadOnlyProperty, ps, () => control.IsReadOnly = value, bindingMode, converter, bindingSource);
public static T IsReadOnly<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.BreadcrumbItem
=> control._setEx(Ursa.Controls.BreadcrumbItem.IsReadOnlyProperty, ps, () => control.IsReadOnly = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

