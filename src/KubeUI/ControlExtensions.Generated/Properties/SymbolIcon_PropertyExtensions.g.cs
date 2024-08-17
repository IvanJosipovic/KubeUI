#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using SymbolIcon = FluentAvalonia.UI.Controls.SymbolIcon;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class SymbolIconExtensions
{
public static T Symbol<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.SymbolIcon
   => control._set(FluentAvalonia.UI.Controls.SymbolIcon.SymbolProperty, binding);
public static T Symbol<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.SymbolIcon
   => control._set(FluentAvalonia.UI.Controls.SymbolIcon.SymbolProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Symbol<T>(this T control, Func<FluentAvalonia.UI.Controls.Symbol> func, Action<FluentAvalonia.UI.Controls.Symbol>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.SymbolIcon
   => control._set(FluentAvalonia.UI.Controls.SymbolIcon.SymbolProperty, func, onChanged, expression);
public static T Symbol<T>(this T control, FluentAvalonia.UI.Controls.Symbol value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SymbolIcon
=> control._setEx(FluentAvalonia.UI.Controls.SymbolIcon.SymbolProperty, ps, () => control.Symbol = value, bindingMode, converter, bindingSource);
public static T Symbol<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.Symbol> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SymbolIcon
=> control._setEx(FluentAvalonia.UI.Controls.SymbolIcon.SymbolProperty, ps, () => control.Symbol = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T FontSize<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.SymbolIcon
   => control._set(FluentAvalonia.UI.Controls.SymbolIcon.FontSizeProperty, binding);
public static T FontSize<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.SymbolIcon
   => control._set(FluentAvalonia.UI.Controls.SymbolIcon.FontSizeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T FontSize<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.SymbolIcon
   => control._set(FluentAvalonia.UI.Controls.SymbolIcon.FontSizeProperty, func, onChanged, expression);
public static T FontSize<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SymbolIcon
=> control._setEx(FluentAvalonia.UI.Controls.SymbolIcon.FontSizeProperty, ps, () => control.FontSize = value, bindingMode, converter, bindingSource);
public static T FontSize<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SymbolIcon
=> control._setEx(FluentAvalonia.UI.Controls.SymbolIcon.FontSizeProperty, ps, () => control.FontSize = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

