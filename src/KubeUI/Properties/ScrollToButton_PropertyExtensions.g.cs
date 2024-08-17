#nullable enable
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using ScrollToButton = Ursa.Controls.ScrollToButton;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Common;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class ScrollToButtonExtensions
{
public static T Target<T>(this T control, IBinding binding) where T : Ursa.Controls.ScrollToButton
   => control._set(Ursa.Controls.ScrollToButton.TargetProperty, binding);
public static T Target<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ScrollToButton
   => control._set(Ursa.Controls.ScrollToButton.TargetProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Target<T>(this T control, Func<Avalonia.Controls.Control> func, Action<Avalonia.Controls.Control>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ScrollToButton
   => control._set(Ursa.Controls.ScrollToButton.TargetProperty, func, onChanged, expression);
public static T Target<T>(this T control, Avalonia.Controls.Control value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ScrollToButton
=> control._setEx(Ursa.Controls.ScrollToButton.TargetProperty, ps, () => control.Target = value, bindingMode, converter, bindingSource);
public static T Target<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Control> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ScrollToButton
=> control._setEx(Ursa.Controls.ScrollToButton.TargetProperty, ps, () => control.Target = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Direction<T>(this T control, IBinding binding) where T : Ursa.Controls.ScrollToButton
   => control._set(Ursa.Controls.ScrollToButton.DirectionProperty, binding);
public static T Direction<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ScrollToButton
   => control._set(Ursa.Controls.ScrollToButton.DirectionProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Direction<T>(this T control, Func<Ursa.Common.Position> func, Action<Ursa.Common.Position>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ScrollToButton
   => control._set(Ursa.Controls.ScrollToButton.DirectionProperty, func, onChanged, expression);
public static T Direction<T>(this T control, Ursa.Common.Position value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ScrollToButton
=> control._setEx(Ursa.Controls.ScrollToButton.DirectionProperty, ps, () => control.Direction = value, bindingMode, converter, bindingSource);
public static T Direction<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Ursa.Common.Position> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ScrollToButton
=> control._setEx(Ursa.Controls.ScrollToButton.DirectionProperty, ps, () => control.Direction = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

