#nullable enable
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using LoadingContainer = Ursa.Controls.LoadingContainer;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class LoadingContainerExtensions
{
public static T Indicator<T>(this T control, IBinding binding) where T : Ursa.Controls.LoadingContainer
   => control._set(Ursa.Controls.LoadingContainer.IndicatorProperty, binding);
public static T Indicator<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.LoadingContainer
   => control._set(Ursa.Controls.LoadingContainer.IndicatorProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Indicator<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.LoadingContainer
   => control._set(Ursa.Controls.LoadingContainer.IndicatorProperty, func, onChanged, expression);
public static T Indicator<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.LoadingContainer
=> control._setEx(Ursa.Controls.LoadingContainer.IndicatorProperty, ps, () => control.Indicator = value, bindingMode, converter, bindingSource);
public static T Indicator<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.LoadingContainer
=> control._setEx(Ursa.Controls.LoadingContainer.IndicatorProperty, ps, () => control.Indicator = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T LoadingMessage<T>(this T control, IBinding binding) where T : Ursa.Controls.LoadingContainer
   => control._set(Ursa.Controls.LoadingContainer.LoadingMessageProperty, binding);
public static T LoadingMessage<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.LoadingContainer
   => control._set(Ursa.Controls.LoadingContainer.LoadingMessageProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T LoadingMessage<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.LoadingContainer
   => control._set(Ursa.Controls.LoadingContainer.LoadingMessageProperty, func, onChanged, expression);
public static T LoadingMessage<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.LoadingContainer
=> control._setEx(Ursa.Controls.LoadingContainer.LoadingMessageProperty, ps, () => control.LoadingMessage = value, bindingMode, converter, bindingSource);
public static T LoadingMessage<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.LoadingContainer
=> control._setEx(Ursa.Controls.LoadingContainer.LoadingMessageProperty, ps, () => control.LoadingMessage = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T LoadingMessageTemplate<T>(this T control, IBinding binding) where T : Ursa.Controls.LoadingContainer
   => control._set(Ursa.Controls.LoadingContainer.LoadingMessageTemplateProperty, binding);
public static T LoadingMessageTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.LoadingContainer
   => control._set(Ursa.Controls.LoadingContainer.LoadingMessageTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T LoadingMessageTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.LoadingContainer
   => control._set(Ursa.Controls.LoadingContainer.LoadingMessageTemplateProperty, func, onChanged, expression);
public static T LoadingMessageTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.LoadingContainer
=> control._setEx(Ursa.Controls.LoadingContainer.LoadingMessageTemplateProperty, ps, () => control.LoadingMessageTemplate = value, bindingMode, converter, bindingSource);
public static T LoadingMessageTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.LoadingContainer
=> control._setEx(Ursa.Controls.LoadingContainer.LoadingMessageTemplateProperty, ps, () => control.LoadingMessageTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsLoading<T>(this T control, IBinding binding) where T : Ursa.Controls.LoadingContainer
   => control._set(Ursa.Controls.LoadingContainer.IsLoadingProperty, binding);
public static T IsLoading<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.LoadingContainer
   => control._set(Ursa.Controls.LoadingContainer.IsLoadingProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsLoading<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.LoadingContainer
   => control._set(Ursa.Controls.LoadingContainer.IsLoadingProperty, func, onChanged, expression);
public static T IsLoading<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.LoadingContainer
=> control._setEx(Ursa.Controls.LoadingContainer.IsLoadingProperty, ps, () => control.IsLoading = value, bindingMode, converter, bindingSource);
public static T IsLoading<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.LoadingContainer
=> control._setEx(Ursa.Controls.LoadingContainer.IsLoadingProperty, ps, () => control.IsLoading = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

