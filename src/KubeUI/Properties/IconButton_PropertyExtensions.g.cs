#nullable enable
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using IconButton = Ursa.Controls.IconButton;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Common;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class IconButtonExtensions
{
public static T Icon<T>(this T control, IBinding binding) where T : Ursa.Controls.IconButton
   => control._set(Ursa.Controls.IconButton.IconProperty, binding);
public static T Icon<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.IconButton
   => control._set(Ursa.Controls.IconButton.IconProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Icon<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.IconButton
   => control._set(Ursa.Controls.IconButton.IconProperty, func, onChanged, expression);
public static T Icon<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.IconButton
=> control._setEx(Ursa.Controls.IconButton.IconProperty, ps, () => control.Icon = value, bindingMode, converter, bindingSource);
public static T Icon<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.IconButton
=> control._setEx(Ursa.Controls.IconButton.IconProperty, ps, () => control.Icon = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IconTemplate<T>(this T control, IBinding binding) where T : Ursa.Controls.IconButton
   => control._set(Ursa.Controls.IconButton.IconTemplateProperty, binding);
public static T IconTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.IconButton
   => control._set(Ursa.Controls.IconButton.IconTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IconTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.IconButton
   => control._set(Ursa.Controls.IconButton.IconTemplateProperty, func, onChanged, expression);
public static T IconTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.IconButton
=> control._setEx(Ursa.Controls.IconButton.IconTemplateProperty, ps, () => control.IconTemplate = value, bindingMode, converter, bindingSource);
public static T IconTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.IconButton
=> control._setEx(Ursa.Controls.IconButton.IconTemplateProperty, ps, () => control.IconTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsLoading<T>(this T control, IBinding binding) where T : Ursa.Controls.IconButton
   => control._set(Ursa.Controls.IconButton.IsLoadingProperty, binding);
public static T IsLoading<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.IconButton
   => control._set(Ursa.Controls.IconButton.IsLoadingProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsLoading<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.IconButton
   => control._set(Ursa.Controls.IconButton.IsLoadingProperty, func, onChanged, expression);
public static T IsLoading<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.IconButton
=> control._setEx(Ursa.Controls.IconButton.IsLoadingProperty, ps, () => control.IsLoading = value, bindingMode, converter, bindingSource);
public static T IsLoading<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.IconButton
=> control._setEx(Ursa.Controls.IconButton.IsLoadingProperty, ps, () => control.IsLoading = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IconPlacement<T>(this T control, IBinding binding) where T : Ursa.Controls.IconButton
   => control._set(Ursa.Controls.IconButton.IconPlacementProperty, binding);
public static T IconPlacement<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.IconButton
   => control._set(Ursa.Controls.IconButton.IconPlacementProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IconPlacement<T>(this T control, Func<Ursa.Common.Position> func, Action<Ursa.Common.Position>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.IconButton
   => control._set(Ursa.Controls.IconButton.IconPlacementProperty, func, onChanged, expression);
public static T IconPlacement<T>(this T control, Ursa.Common.Position value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.IconButton
=> control._setEx(Ursa.Controls.IconButton.IconPlacementProperty, ps, () => control.IconPlacement = value, bindingMode, converter, bindingSource);
public static T IconPlacement<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Ursa.Common.Position> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.IconButton
=> control._setEx(Ursa.Controls.IconButton.IconPlacementProperty, ps, () => control.IconPlacement = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

