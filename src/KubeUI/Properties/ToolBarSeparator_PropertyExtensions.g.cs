#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using ToolBarSeparator = Ursa.Controls.ToolBarSeparator;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class ToolBarSeparatorExtensions
{
public static T Orientation<T>(this T control, IBinding binding) where T : Ursa.Controls.ToolBarSeparator
   => control._set(Ursa.Controls.ToolBarSeparator.OrientationProperty, binding);
public static T Orientation<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ToolBarSeparator
   => control._set(Ursa.Controls.ToolBarSeparator.OrientationProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Orientation<T>(this T control, Func<Avalonia.Layout.Orientation> func, Action<Avalonia.Layout.Orientation>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ToolBarSeparator
   => control._set(Ursa.Controls.ToolBarSeparator.OrientationProperty, func, onChanged, expression);
public static T Orientation<T>(this T control, Avalonia.Layout.Orientation value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ToolBarSeparator
=> control._setEx(Ursa.Controls.ToolBarSeparator.OrientationProperty, ps, () => control.Orientation = value, bindingMode, converter, bindingSource);
public static T Orientation<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Layout.Orientation> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ToolBarSeparator
=> control._setEx(Ursa.Controls.ToolBarSeparator.OrientationProperty, ps, () => control.Orientation = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

