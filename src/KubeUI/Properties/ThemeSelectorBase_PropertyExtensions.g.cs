#nullable enable
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Styling;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using ThemeSelectorBase = Ursa.Controls.ThemeSelectorBase;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class ThemeSelectorBaseExtensions
{
public static T SelectedTheme<T>(this T control, IBinding binding) where T : Ursa.Controls.ThemeSelectorBase
   => control._set(Ursa.Controls.ThemeSelectorBase.SelectedThemeProperty, binding);
public static T SelectedTheme<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ThemeSelectorBase
   => control._set(Ursa.Controls.ThemeSelectorBase.SelectedThemeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SelectedTheme<T>(this T control, Func<Avalonia.Styling.ThemeVariant> func, Action<Avalonia.Styling.ThemeVariant>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ThemeSelectorBase
   => control._set(Ursa.Controls.ThemeSelectorBase.SelectedThemeProperty, func, onChanged, expression);
public static T SelectedTheme<T>(this T control, Avalonia.Styling.ThemeVariant value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ThemeSelectorBase
=> control._setEx(Ursa.Controls.ThemeSelectorBase.SelectedThemeProperty, ps, () => control.SelectedTheme = value, bindingMode, converter, bindingSource);
public static T SelectedTheme<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ThemeVariant> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ThemeSelectorBase
=> control._setEx(Ursa.Controls.ThemeSelectorBase.SelectedThemeProperty, ps, () => control.SelectedTheme = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Mode<T>(this T control, IBinding binding) where T : Ursa.Controls.ThemeSelectorBase
   => control._set(Ursa.Controls.ThemeSelectorBase.ModeProperty, binding);
public static T Mode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ThemeSelectorBase
   => control._set(Ursa.Controls.ThemeSelectorBase.ModeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Mode<T>(this T control, Func<Ursa.Controls.ThemeSelectorMode> func, Action<Ursa.Controls.ThemeSelectorMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ThemeSelectorBase
   => control._set(Ursa.Controls.ThemeSelectorBase.ModeProperty, func, onChanged, expression);
public static T Mode<T>(this T control, Ursa.Controls.ThemeSelectorMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ThemeSelectorBase
=> control._setEx(Ursa.Controls.ThemeSelectorBase.ModeProperty, ps, () => control.Mode = value, bindingMode, converter, bindingSource);
public static T Mode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Ursa.Controls.ThemeSelectorMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ThemeSelectorBase
=> control._setEx(Ursa.Controls.ThemeSelectorBase.ModeProperty, ps, () => control.Mode = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T TargetScope<T>(this T control, IBinding binding) where T : Ursa.Controls.ThemeSelectorBase
   => control._set(Ursa.Controls.ThemeSelectorBase.TargetScopeProperty, binding);
public static T TargetScope<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ThemeSelectorBase
   => control._set(Ursa.Controls.ThemeSelectorBase.TargetScopeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T TargetScope<T>(this T control, Func<Avalonia.Controls.ThemeVariantScope> func, Action<Avalonia.Controls.ThemeVariantScope>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ThemeSelectorBase
   => control._set(Ursa.Controls.ThemeSelectorBase.TargetScopeProperty, func, onChanged, expression);
public static T TargetScope<T>(this T control, Avalonia.Controls.ThemeVariantScope value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ThemeSelectorBase
=> control._setEx(Ursa.Controls.ThemeSelectorBase.TargetScopeProperty, ps, () => control.TargetScope = value, bindingMode, converter, bindingSource);
public static T TargetScope<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.ThemeVariantScope> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ThemeSelectorBase
=> control._setEx(Ursa.Controls.ThemeSelectorBase.TargetScopeProperty, ps, () => control.TargetScope = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

