#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using ElasticWrapPanel = Ursa.Controls.ElasticWrapPanel;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class ElasticWrapPanelExtensions
{
public static T IsFillHorizontal<T>(this T control, IBinding binding) where T : Ursa.Controls.ElasticWrapPanel
   => control._set(Ursa.Controls.ElasticWrapPanel.IsFillHorizontalProperty, binding);
public static T IsFillHorizontal<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ElasticWrapPanel
   => control._set(Ursa.Controls.ElasticWrapPanel.IsFillHorizontalProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsFillHorizontal<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ElasticWrapPanel
   => control._set(Ursa.Controls.ElasticWrapPanel.IsFillHorizontalProperty, func, onChanged, expression);
public static T IsFillHorizontal<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ElasticWrapPanel
=> control._setEx(Ursa.Controls.ElasticWrapPanel.IsFillHorizontalProperty, ps, () => control.IsFillHorizontal = value, bindingMode, converter, bindingSource);
public static T IsFillHorizontal<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ElasticWrapPanel
=> control._setEx(Ursa.Controls.ElasticWrapPanel.IsFillHorizontalProperty, ps, () => control.IsFillHorizontal = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsFillVertical<T>(this T control, IBinding binding) where T : Ursa.Controls.ElasticWrapPanel
   => control._set(Ursa.Controls.ElasticWrapPanel.IsFillVerticalProperty, binding);
public static T IsFillVertical<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ElasticWrapPanel
   => control._set(Ursa.Controls.ElasticWrapPanel.IsFillVerticalProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsFillVertical<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ElasticWrapPanel
   => control._set(Ursa.Controls.ElasticWrapPanel.IsFillVerticalProperty, func, onChanged, expression);
public static T IsFillVertical<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ElasticWrapPanel
=> control._setEx(Ursa.Controls.ElasticWrapPanel.IsFillVerticalProperty, ps, () => control.IsFillVertical = value, bindingMode, converter, bindingSource);
public static T IsFillVertical<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ElasticWrapPanel
=> control._setEx(Ursa.Controls.ElasticWrapPanel.IsFillVerticalProperty, ps, () => control.IsFillVertical = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

