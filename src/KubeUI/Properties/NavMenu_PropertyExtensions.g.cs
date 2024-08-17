#nullable enable
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using NavMenu = Ursa.Controls.NavMenu;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class NavMenuExtensions
{
public static T SelectedItem<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.SelectedItemProperty, binding);
public static T SelectedItem<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.SelectedItemProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SelectedItem<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.SelectedItemProperty, func, onChanged, expression);
public static T SelectedItem<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.SelectedItemProperty, ps, () => control.SelectedItem = value, bindingMode, converter, bindingSource);
public static T SelectedItem<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.SelectedItemProperty, ps, () => control.SelectedItem = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IconBinding<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.IconBindingProperty, binding);
public static T IconBinding<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.IconBindingProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IconBinding<T>(this T control, Func<Avalonia.Data.IBinding> func, Action<Avalonia.Data.IBinding>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.IconBindingProperty, func, onChanged, expression);
public static T IconBinding<T>(this T control, Avalonia.Data.IBinding value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.IconBindingProperty, ps, () => control.IconBinding = value, bindingMode, converter, bindingSource);
public static T IconBinding<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Data.IBinding> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.IconBindingProperty, ps, () => control.IconBinding = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T HeaderBinding<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.HeaderBindingProperty, binding);
public static T HeaderBinding<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.HeaderBindingProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T HeaderBinding<T>(this T control, Func<Avalonia.Data.IBinding> func, Action<Avalonia.Data.IBinding>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.HeaderBindingProperty, func, onChanged, expression);
public static T HeaderBinding<T>(this T control, Avalonia.Data.IBinding value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.HeaderBindingProperty, ps, () => control.HeaderBinding = value, bindingMode, converter, bindingSource);
public static T HeaderBinding<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Data.IBinding> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.HeaderBindingProperty, ps, () => control.HeaderBinding = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SubMenuBinding<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.SubMenuBindingProperty, binding);
public static T SubMenuBinding<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.SubMenuBindingProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SubMenuBinding<T>(this T control, Func<Avalonia.Data.IBinding> func, Action<Avalonia.Data.IBinding>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.SubMenuBindingProperty, func, onChanged, expression);
public static T SubMenuBinding<T>(this T control, Avalonia.Data.IBinding value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.SubMenuBindingProperty, ps, () => control.SubMenuBinding = value, bindingMode, converter, bindingSource);
public static T SubMenuBinding<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Data.IBinding> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.SubMenuBindingProperty, ps, () => control.SubMenuBinding = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T CommandBinding<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.CommandBindingProperty, binding);
public static T CommandBinding<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.CommandBindingProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CommandBinding<T>(this T control, Func<Avalonia.Data.IBinding> func, Action<Avalonia.Data.IBinding>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.CommandBindingProperty, func, onChanged, expression);
public static T CommandBinding<T>(this T control, Avalonia.Data.IBinding value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.CommandBindingProperty, ps, () => control.CommandBinding = value, bindingMode, converter, bindingSource);
public static T CommandBinding<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Data.IBinding> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.CommandBindingProperty, ps, () => control.CommandBinding = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T HeaderTemplate<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.HeaderTemplateProperty, binding);
public static T HeaderTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.HeaderTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T HeaderTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.HeaderTemplateProperty, func, onChanged, expression);
public static T HeaderTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.HeaderTemplateProperty, ps, () => control.HeaderTemplate = value, bindingMode, converter, bindingSource);
public static T HeaderTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.HeaderTemplateProperty, ps, () => control.HeaderTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IconTemplate<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.IconTemplateProperty, binding);
public static T IconTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.IconTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IconTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.IconTemplateProperty, func, onChanged, expression);
public static T IconTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.IconTemplateProperty, ps, () => control.IconTemplate = value, bindingMode, converter, bindingSource);
public static T IconTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.IconTemplateProperty, ps, () => control.IconTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SubMenuIndent<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.SubMenuIndentProperty, binding);
public static T SubMenuIndent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.SubMenuIndentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SubMenuIndent<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.SubMenuIndentProperty, func, onChanged, expression);
public static T SubMenuIndent<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.SubMenuIndentProperty, ps, () => control.SubMenuIndent = value, bindingMode, converter, bindingSource);
public static T SubMenuIndent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.SubMenuIndentProperty, ps, () => control.SubMenuIndent = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsHorizontalCollapsed<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.IsHorizontalCollapsedProperty, binding);
public static T IsHorizontalCollapsed<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.IsHorizontalCollapsedProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsHorizontalCollapsed<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.IsHorizontalCollapsedProperty, func, onChanged, expression);
public static T IsHorizontalCollapsed<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.IsHorizontalCollapsedProperty, ps, () => control.IsHorizontalCollapsed = value, bindingMode, converter, bindingSource);
public static T IsHorizontalCollapsed<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.IsHorizontalCollapsedProperty, ps, () => control.IsHorizontalCollapsed = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Header<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.HeaderProperty, binding);
public static T Header<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.HeaderProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Header<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.HeaderProperty, func, onChanged, expression);
public static T Header<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.HeaderProperty, ps, () => control.Header = value, bindingMode, converter, bindingSource);
public static T Header<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.HeaderProperty, ps, () => control.Header = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Footer<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.FooterProperty, binding);
public static T Footer<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.FooterProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Footer<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.FooterProperty, func, onChanged, expression);
public static T Footer<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.FooterProperty, ps, () => control.Footer = value, bindingMode, converter, bindingSource);
public static T Footer<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.FooterProperty, ps, () => control.Footer = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ExpandWidth<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.ExpandWidthProperty, binding);
public static T ExpandWidth<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.ExpandWidthProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ExpandWidth<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.ExpandWidthProperty, func, onChanged, expression);
public static T ExpandWidth<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.ExpandWidthProperty, ps, () => control.ExpandWidth = value, bindingMode, converter, bindingSource);
public static T ExpandWidth<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.ExpandWidthProperty, ps, () => control.ExpandWidth = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T CollapseWidth<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.CollapseWidthProperty, binding);
public static T CollapseWidth<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.CollapseWidthProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CollapseWidth<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.CollapseWidthProperty, func, onChanged, expression);
public static T CollapseWidth<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.CollapseWidthProperty, ps, () => control.CollapseWidth = value, bindingMode, converter, bindingSource);
public static T CollapseWidth<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.CollapseWidthProperty, ps, () => control.CollapseWidth = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

