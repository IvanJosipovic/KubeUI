#nullable enable
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using SettingsExpanderItem = FluentAvalonia.UI.Controls.SettingsExpanderItem;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Avalonia.Markup.Declarative;
public static partial class SettingsExpanderItemExtensions
{
public static T Description<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.DescriptionProperty, binding);
public static T Description<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.DescriptionProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Description<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.DescriptionProperty, func, onChanged, expression);
public static T Description<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> control._setEx(FluentAvalonia.UI.Controls.SettingsExpanderItem.DescriptionProperty, ps, () => control.Description = value, bindingMode, converter, bindingSource);
public static T Description<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> control._setEx(FluentAvalonia.UI.Controls.SettingsExpanderItem.DescriptionProperty, ps, () => control.Description = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IconSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.IconSourceProperty, binding);
public static T IconSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.IconSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IconSource<T>(this T control, Func<FluentAvalonia.UI.Controls.IconSource> func, Action<FluentAvalonia.UI.Controls.IconSource>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.IconSourceProperty, func, onChanged, expression);
public static T IconSource<T>(this T control, FluentAvalonia.UI.Controls.IconSource value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> control._setEx(FluentAvalonia.UI.Controls.SettingsExpanderItem.IconSourceProperty, ps, () => control.IconSource = value, bindingMode, converter, bindingSource);
public static T IconSource<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.IconSource> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> control._setEx(FluentAvalonia.UI.Controls.SettingsExpanderItem.IconSourceProperty, ps, () => control.IconSource = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Footer<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.FooterProperty, binding);
public static T Footer<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.FooterProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Footer<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.FooterProperty, func, onChanged, expression);
public static T Footer<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> control._setEx(FluentAvalonia.UI.Controls.SettingsExpanderItem.FooterProperty, ps, () => control.Footer = value, bindingMode, converter, bindingSource);
public static T Footer<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> control._setEx(FluentAvalonia.UI.Controls.SettingsExpanderItem.FooterProperty, ps, () => control.Footer = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T FooterTemplate<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.FooterTemplateProperty, binding);
public static T FooterTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.FooterTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T FooterTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.FooterTemplateProperty, func, onChanged, expression);
public static T FooterTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> control._setEx(FluentAvalonia.UI.Controls.SettingsExpanderItem.FooterTemplateProperty, ps, () => control.FooterTemplate = value, bindingMode, converter, bindingSource);
public static T FooterTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> control._setEx(FluentAvalonia.UI.Controls.SettingsExpanderItem.FooterTemplateProperty, ps, () => control.FooterTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ActionIconSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.ActionIconSourceProperty, binding);
public static T ActionIconSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.ActionIconSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ActionIconSource<T>(this T control, Func<FluentAvalonia.UI.Controls.IconSource> func, Action<FluentAvalonia.UI.Controls.IconSource>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.ActionIconSourceProperty, func, onChanged, expression);
public static T ActionIconSource<T>(this T control, FluentAvalonia.UI.Controls.IconSource value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> control._setEx(FluentAvalonia.UI.Controls.SettingsExpanderItem.ActionIconSourceProperty, ps, () => control.ActionIconSource = value, bindingMode, converter, bindingSource);
public static T ActionIconSource<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.IconSource> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> control._setEx(FluentAvalonia.UI.Controls.SettingsExpanderItem.ActionIconSourceProperty, ps, () => control.ActionIconSource = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsClickEnabled<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.IsClickEnabledProperty, binding);
public static T IsClickEnabled<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.IsClickEnabledProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsClickEnabled<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.IsClickEnabledProperty, func, onChanged, expression);
public static T IsClickEnabled<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> control._setEx(FluentAvalonia.UI.Controls.SettingsExpanderItem.IsClickEnabledProperty, ps, () => control.IsClickEnabled = value, bindingMode, converter, bindingSource);
public static T IsClickEnabled<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> control._setEx(FluentAvalonia.UI.Controls.SettingsExpanderItem.IsClickEnabledProperty, ps, () => control.IsClickEnabled = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Command<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.CommandProperty, binding);
public static T Command<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.CommandProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Command<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.CommandProperty, func, onChanged, expression);
public static T Command<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> control._setEx(FluentAvalonia.UI.Controls.SettingsExpanderItem.CommandProperty, ps, () => control.Command = value, bindingMode, converter, bindingSource);
public static T Command<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> control._setEx(FluentAvalonia.UI.Controls.SettingsExpanderItem.CommandProperty, ps, () => control.Command = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T CommandParameter<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.CommandParameterProperty, binding);
public static T CommandParameter<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.CommandParameterProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CommandParameter<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.CommandParameterProperty, func, onChanged, expression);
public static T CommandParameter<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> control._setEx(FluentAvalonia.UI.Controls.SettingsExpanderItem.CommandParameterProperty, ps, () => control.CommandParameter = value, bindingMode, converter, bindingSource);
public static T CommandParameter<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> control._setEx(FluentAvalonia.UI.Controls.SettingsExpanderItem.CommandParameterProperty, ps, () => control.CommandParameter = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

