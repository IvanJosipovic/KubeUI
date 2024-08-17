#nullable enable
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using NavMenuItem = Ursa.Controls.NavMenuItem;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class NavMenuItemExtensions
{
public static T Icon<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.IconProperty, binding);
public static T Icon<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.IconProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Icon<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.IconProperty, func, onChanged, expression);
public static T Icon<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenuItem
=> control._setEx(Ursa.Controls.NavMenuItem.IconProperty, ps, () => control.Icon = value, bindingMode, converter, bindingSource);
public static T Icon<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenuItem
=> control._setEx(Ursa.Controls.NavMenuItem.IconProperty, ps, () => control.Icon = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IconTemplate<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.IconTemplateProperty, binding);
public static T IconTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.IconTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IconTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.IconTemplateProperty, func, onChanged, expression);
public static T IconTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenuItem
=> control._setEx(Ursa.Controls.NavMenuItem.IconTemplateProperty, ps, () => control.IconTemplate = value, bindingMode, converter, bindingSource);
public static T IconTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenuItem
=> control._setEx(Ursa.Controls.NavMenuItem.IconTemplateProperty, ps, () => control.IconTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Command<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.CommandProperty, binding);
public static T Command<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.CommandProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Command<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.CommandProperty, func, onChanged, expression);
public static T Command<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenuItem
=> control._setEx(Ursa.Controls.NavMenuItem.CommandProperty, ps, () => control.Command = value, bindingMode, converter, bindingSource);
public static T Command<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenuItem
=> control._setEx(Ursa.Controls.NavMenuItem.CommandProperty, ps, () => control.Command = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T CommandParameter<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.CommandParameterProperty, binding);
public static T CommandParameter<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.CommandParameterProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CommandParameter<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.CommandParameterProperty, func, onChanged, expression);
public static T CommandParameter<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenuItem
=> control._setEx(Ursa.Controls.NavMenuItem.CommandParameterProperty, ps, () => control.CommandParameter = value, bindingMode, converter, bindingSource);
public static T CommandParameter<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenuItem
=> control._setEx(Ursa.Controls.NavMenuItem.CommandParameterProperty, ps, () => control.CommandParameter = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsSelected<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.IsSelectedProperty, binding);
public static T IsSelected<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.IsSelectedProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsSelected<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.IsSelectedProperty, func, onChanged, expression);
public static T IsSelected<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenuItem
=> control._setEx(Ursa.Controls.NavMenuItem.IsSelectedProperty, ps, () => control.IsSelected = value, bindingMode, converter, bindingSource);
public static T IsSelected<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenuItem
=> control._setEx(Ursa.Controls.NavMenuItem.IsSelectedProperty, ps, () => control.IsSelected = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsHorizontalCollapsed<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.IsHorizontalCollapsedProperty, binding);
public static T IsHorizontalCollapsed<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.IsHorizontalCollapsedProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsHorizontalCollapsed<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.IsHorizontalCollapsedProperty, func, onChanged, expression);
public static T IsHorizontalCollapsed<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenuItem
=> control._setEx(Ursa.Controls.NavMenuItem.IsHorizontalCollapsedProperty, ps, () => control.IsHorizontalCollapsed = value, bindingMode, converter, bindingSource);
public static T IsHorizontalCollapsed<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenuItem
=> control._setEx(Ursa.Controls.NavMenuItem.IsHorizontalCollapsedProperty, ps, () => control.IsHorizontalCollapsed = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsVerticalCollapsed<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.IsVerticalCollapsedProperty, binding);
public static T IsVerticalCollapsed<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.IsVerticalCollapsedProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsVerticalCollapsed<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.IsVerticalCollapsedProperty, func, onChanged, expression);
public static T IsVerticalCollapsed<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenuItem
=> control._setEx(Ursa.Controls.NavMenuItem.IsVerticalCollapsedProperty, ps, () => control.IsVerticalCollapsed = value, bindingMode, converter, bindingSource);
public static T IsVerticalCollapsed<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenuItem
=> control._setEx(Ursa.Controls.NavMenuItem.IsVerticalCollapsedProperty, ps, () => control.IsVerticalCollapsed = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SubMenuIndent<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.SubMenuIndentProperty, binding);
public static T SubMenuIndent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.SubMenuIndentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SubMenuIndent<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.SubMenuIndentProperty, func, onChanged, expression);
public static T SubMenuIndent<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenuItem
=> control._setEx(Ursa.Controls.NavMenuItem.SubMenuIndentProperty, ps, () => control.SubMenuIndent = value, bindingMode, converter, bindingSource);
public static T SubMenuIndent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenuItem
=> control._setEx(Ursa.Controls.NavMenuItem.SubMenuIndentProperty, ps, () => control.SubMenuIndent = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsSeparator<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.IsSeparatorProperty, binding);
public static T IsSeparator<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.IsSeparatorProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsSeparator<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.IsSeparatorProperty, func, onChanged, expression);
public static T IsSeparator<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenuItem
=> control._setEx(Ursa.Controls.NavMenuItem.IsSeparatorProperty, ps, () => control.IsSeparator = value, bindingMode, converter, bindingSource);
public static T IsSeparator<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenuItem
=> control._setEx(Ursa.Controls.NavMenuItem.IsSeparatorProperty, ps, () => control.IsSeparator = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

