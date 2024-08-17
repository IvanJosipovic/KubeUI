#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using SettingsExpanderTemplateSettings = FluentAvalonia.UI.Controls.SettingsExpanderTemplateSettings;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class SettingsExpanderTemplateSettingsExtensions
{
public static T Icon<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpanderTemplateSettings
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderTemplateSettings.IconProperty, binding);
public static T Icon<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderTemplateSettings
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderTemplateSettings.IconProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Icon<T>(this T control, Func<FluentAvalonia.UI.Controls.FAIconElement> func, Action<FluentAvalonia.UI.Controls.FAIconElement>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderTemplateSettings
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderTemplateSettings.IconProperty, func, onChanged, expression);
public static T Icon<T>(this T control, FluentAvalonia.UI.Controls.FAIconElement value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderTemplateSettings
=> control._setEx(FluentAvalonia.UI.Controls.SettingsExpanderTemplateSettings.IconProperty, ps, () => control.Icon = value, bindingMode, converter, bindingSource);
public static T Icon<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.FAIconElement> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderTemplateSettings
=> control._setEx(FluentAvalonia.UI.Controls.SettingsExpanderTemplateSettings.IconProperty, ps, () => control.Icon = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ActionIcon<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpanderTemplateSettings
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderTemplateSettings.ActionIconProperty, binding);
public static T ActionIcon<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderTemplateSettings
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderTemplateSettings.ActionIconProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ActionIcon<T>(this T control, Func<FluentAvalonia.UI.Controls.FAIconElement> func, Action<FluentAvalonia.UI.Controls.FAIconElement>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderTemplateSettings
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderTemplateSettings.ActionIconProperty, func, onChanged, expression);
public static T ActionIcon<T>(this T control, FluentAvalonia.UI.Controls.FAIconElement value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderTemplateSettings
=> control._setEx(FluentAvalonia.UI.Controls.SettingsExpanderTemplateSettings.ActionIconProperty, ps, () => control.ActionIcon = value, bindingMode, converter, bindingSource);
public static T ActionIcon<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.FAIconElement> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderTemplateSettings
=> control._setEx(FluentAvalonia.UI.Controls.SettingsExpanderTemplateSettings.ActionIconProperty, ps, () => control.ActionIcon = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

