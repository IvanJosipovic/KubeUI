#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Styling;
using Badge = Ursa.Controls.Badge;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Common;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class BadgeExtensions
{
public static T BadgeTheme<T>(this T control, IBinding binding) where T : Ursa.Controls.Badge
   => control._set(Ursa.Controls.Badge.BadgeThemeProperty, binding);
public static T BadgeTheme<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Badge
   => control._set(Ursa.Controls.Badge.BadgeThemeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T BadgeTheme<T>(this T control, Func<Avalonia.Styling.ControlTheme> func, Action<Avalonia.Styling.ControlTheme>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Badge
   => control._set(Ursa.Controls.Badge.BadgeThemeProperty, func, onChanged, expression);
public static T BadgeTheme<T>(this T control, Avalonia.Styling.ControlTheme value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Badge
=> control._setEx(Ursa.Controls.Badge.BadgeThemeProperty, ps, () => control.BadgeTheme = value, bindingMode, converter, bindingSource);
public static T BadgeTheme<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ControlTheme> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Badge
=> control._setEx(Ursa.Controls.Badge.BadgeThemeProperty, ps, () => control.BadgeTheme = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Dot<T>(this T control, IBinding binding) where T : Ursa.Controls.Badge
   => control._set(Ursa.Controls.Badge.DotProperty, binding);
public static T Dot<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Badge
   => control._set(Ursa.Controls.Badge.DotProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Dot<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Badge
   => control._set(Ursa.Controls.Badge.DotProperty, func, onChanged, expression);
public static T Dot<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Badge
=> control._setEx(Ursa.Controls.Badge.DotProperty, ps, () => control.Dot = value, bindingMode, converter, bindingSource);
public static T Dot<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Badge
=> control._setEx(Ursa.Controls.Badge.DotProperty, ps, () => control.Dot = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T CornerPosition<T>(this T control, IBinding binding) where T : Ursa.Controls.Badge
   => control._set(Ursa.Controls.Badge.CornerPositionProperty, binding);
public static T CornerPosition<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Badge
   => control._set(Ursa.Controls.Badge.CornerPositionProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CornerPosition<T>(this T control, Func<Ursa.Common.CornerPosition> func, Action<Ursa.Common.CornerPosition>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Badge
   => control._set(Ursa.Controls.Badge.CornerPositionProperty, func, onChanged, expression);
public static T CornerPosition<T>(this T control, Ursa.Common.CornerPosition value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Badge
=> control._setEx(Ursa.Controls.Badge.CornerPositionProperty, ps, () => control.CornerPosition = value, bindingMode, converter, bindingSource);
public static T CornerPosition<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Ursa.Common.CornerPosition> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Badge
=> control._setEx(Ursa.Controls.Badge.CornerPositionProperty, ps, () => control.CornerPosition = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T OverflowCount<T>(this T control, IBinding binding) where T : Ursa.Controls.Badge
   => control._set(Ursa.Controls.Badge.OverflowCountProperty, binding);
public static T OverflowCount<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Badge
   => control._set(Ursa.Controls.Badge.OverflowCountProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T OverflowCount<T>(this T control, Func<System.Int32> func, Action<System.Int32>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Badge
   => control._set(Ursa.Controls.Badge.OverflowCountProperty, func, onChanged, expression);
public static T OverflowCount<T>(this T control, System.Int32 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Badge
=> control._setEx(Ursa.Controls.Badge.OverflowCountProperty, ps, () => control.OverflowCount = value, bindingMode, converter, bindingSource);
public static T OverflowCount<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Int32> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Badge
=> control._setEx(Ursa.Controls.Badge.OverflowCountProperty, ps, () => control.OverflowCount = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T BadgeFontSize<T>(this T control, IBinding binding) where T : Ursa.Controls.Badge
   => control._set(Ursa.Controls.Badge.BadgeFontSizeProperty, binding);
public static T BadgeFontSize<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Badge
   => control._set(Ursa.Controls.Badge.BadgeFontSizeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T BadgeFontSize<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Badge
   => control._set(Ursa.Controls.Badge.BadgeFontSizeProperty, func, onChanged, expression);
public static T BadgeFontSize<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Badge
=> control._setEx(Ursa.Controls.Badge.BadgeFontSizeProperty, ps, () => control.BadgeFontSize = value, bindingMode, converter, bindingSource);
public static T BadgeFontSize<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Badge
=> control._setEx(Ursa.Controls.Badge.BadgeFontSizeProperty, ps, () => control.BadgeFontSize = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

