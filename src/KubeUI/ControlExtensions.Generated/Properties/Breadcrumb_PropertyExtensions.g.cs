#nullable enable
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Breadcrumb = Ursa.Controls.Breadcrumb;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class BreadcrumbExtensions
{
public static T IconBinding<T>(this T control, IBinding binding) where T : Ursa.Controls.Breadcrumb
   => control._set(Ursa.Controls.Breadcrumb.IconBindingProperty, binding);
public static T IconBinding<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Breadcrumb
   => control._set(Ursa.Controls.Breadcrumb.IconBindingProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IconBinding<T>(this T control, Func<Avalonia.Data.IBinding> func, Action<Avalonia.Data.IBinding>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Breadcrumb
   => control._set(Ursa.Controls.Breadcrumb.IconBindingProperty, func, onChanged, expression);
public static T IconBinding<T>(this T control, Avalonia.Data.IBinding value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Breadcrumb
=> control._setEx(Ursa.Controls.Breadcrumb.IconBindingProperty, ps, () => control.IconBinding = value, bindingMode, converter, bindingSource);
public static T IconBinding<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Data.IBinding> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Breadcrumb
=> control._setEx(Ursa.Controls.Breadcrumb.IconBindingProperty, ps, () => control.IconBinding = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T CommandBinding<T>(this T control, IBinding binding) where T : Ursa.Controls.Breadcrumb
   => control._set(Ursa.Controls.Breadcrumb.CommandBindingProperty, binding);
public static T CommandBinding<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Breadcrumb
   => control._set(Ursa.Controls.Breadcrumb.CommandBindingProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CommandBinding<T>(this T control, Func<Avalonia.Data.IBinding> func, Action<Avalonia.Data.IBinding>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Breadcrumb
   => control._set(Ursa.Controls.Breadcrumb.CommandBindingProperty, func, onChanged, expression);
public static T CommandBinding<T>(this T control, Avalonia.Data.IBinding value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Breadcrumb
=> control._setEx(Ursa.Controls.Breadcrumb.CommandBindingProperty, ps, () => control.CommandBinding = value, bindingMode, converter, bindingSource);
public static T CommandBinding<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Data.IBinding> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Breadcrumb
=> control._setEx(Ursa.Controls.Breadcrumb.CommandBindingProperty, ps, () => control.CommandBinding = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Separator<T>(this T control, IBinding binding) where T : Ursa.Controls.Breadcrumb
   => control._set(Ursa.Controls.Breadcrumb.SeparatorProperty, binding);
public static T Separator<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Breadcrumb
   => control._set(Ursa.Controls.Breadcrumb.SeparatorProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Separator<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Breadcrumb
   => control._set(Ursa.Controls.Breadcrumb.SeparatorProperty, func, onChanged, expression);
public static T Separator<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Breadcrumb
=> control._setEx(Ursa.Controls.Breadcrumb.SeparatorProperty, ps, () => control.Separator = value, bindingMode, converter, bindingSource);
public static T Separator<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Breadcrumb
=> control._setEx(Ursa.Controls.Breadcrumb.SeparatorProperty, ps, () => control.Separator = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IconTemplate<T>(this T control, IBinding binding) where T : Ursa.Controls.Breadcrumb
   => control._set(Ursa.Controls.Breadcrumb.IconTemplateProperty, binding);
public static T IconTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Breadcrumb
   => control._set(Ursa.Controls.Breadcrumb.IconTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IconTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Breadcrumb
   => control._set(Ursa.Controls.Breadcrumb.IconTemplateProperty, func, onChanged, expression);
public static T IconTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Breadcrumb
=> control._setEx(Ursa.Controls.Breadcrumb.IconTemplateProperty, ps, () => control.IconTemplate = value, bindingMode, converter, bindingSource);
public static T IconTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Breadcrumb
=> control._setEx(Ursa.Controls.Breadcrumb.IconTemplateProperty, ps, () => control.IconTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

