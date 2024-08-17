#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using ThemeToggleButton = Ursa.Controls.ThemeToggleButton;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class ThemeToggleButtonExtensions
{
public static T IsThreeState<T>(this T control, IBinding binding) where T : Ursa.Controls.ThemeToggleButton
   => control._set(Ursa.Controls.ThemeToggleButton.IsThreeStateProperty, binding);
public static T IsThreeState<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ThemeToggleButton
   => control._set(Ursa.Controls.ThemeToggleButton.IsThreeStateProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsThreeState<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ThemeToggleButton
   => control._set(Ursa.Controls.ThemeToggleButton.IsThreeStateProperty, func, onChanged, expression);
public static T IsThreeState<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ThemeToggleButton
=> control._setEx(Ursa.Controls.ThemeToggleButton.IsThreeStateProperty, ps, () => control.IsThreeState = value, bindingMode, converter, bindingSource);
public static T IsThreeState<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ThemeToggleButton
=> control._setEx(Ursa.Controls.ThemeToggleButton.IsThreeStateProperty, ps, () => control.IsThreeState = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

