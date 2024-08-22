#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class ThemeSelectorBase_MarkupExtensions
{
//================= Properties ======================//
 // SelectedThemeProperty

/*BindFromExpressionSetterGenerator*/
public static T SelectedTheme<T>(this T control, Func<Avalonia.Styling.ThemeVariant> func, Action<Avalonia.Styling.ThemeVariant>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ThemeSelectorBase
   => control._set(Ursa.Controls.ThemeSelectorBase.SelectedThemeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SelectedTheme<T>(this T control, Avalonia.Styling.ThemeVariant value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ThemeSelectorBase
=> control._setEx(Ursa.Controls.ThemeSelectorBase.SelectedThemeProperty, ps, () => control.SelectedTheme = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SelectedTheme<T>(this T control, IBinding binding) where T : Ursa.Controls.ThemeSelectorBase
   => control._set(Ursa.Controls.ThemeSelectorBase.SelectedThemeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SelectedTheme<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ThemeSelectorBase
   => control._set(Ursa.Controls.ThemeSelectorBase.SelectedThemeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SelectedTheme<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ThemeVariant> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ThemeSelectorBase
=> control._setEx(Ursa.Controls.ThemeSelectorBase.SelectedThemeProperty, ps, () => control.SelectedTheme = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ModeProperty

/*BindFromExpressionSetterGenerator*/
public static T Mode<T>(this T control, Func<Ursa.Controls.ThemeSelectorMode> func, Action<Ursa.Controls.ThemeSelectorMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ThemeSelectorBase
   => control._set(Ursa.Controls.ThemeSelectorBase.ModeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Mode<T>(this T control, Ursa.Controls.ThemeSelectorMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ThemeSelectorBase
=> control._setEx(Ursa.Controls.ThemeSelectorBase.ModeProperty, ps, () => control.Mode = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Mode<T>(this T control, IBinding binding) where T : Ursa.Controls.ThemeSelectorBase
   => control._set(Ursa.Controls.ThemeSelectorBase.ModeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Mode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ThemeSelectorBase
   => control._set(Ursa.Controls.ThemeSelectorBase.ModeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Mode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Ursa.Controls.ThemeSelectorMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ThemeSelectorBase
=> control._setEx(Ursa.Controls.ThemeSelectorBase.ModeProperty, ps, () => control.Mode = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TargetScopeProperty

/*BindFromExpressionSetterGenerator*/
public static T TargetScope<T>(this T control, Func<Avalonia.Controls.ThemeVariantScope> func, Action<Avalonia.Controls.ThemeVariantScope>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ThemeSelectorBase
   => control._set(Ursa.Controls.ThemeSelectorBase.TargetScopeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T TargetScope<T>(this T control, Avalonia.Controls.ThemeVariantScope value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ThemeSelectorBase
=> control._setEx(Ursa.Controls.ThemeSelectorBase.TargetScopeProperty, ps, () => control.TargetScope = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TargetScope<T>(this T control, IBinding binding) where T : Ursa.Controls.ThemeSelectorBase
   => control._set(Ursa.Controls.ThemeSelectorBase.TargetScopeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TargetScope<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ThemeSelectorBase
   => control._set(Ursa.Controls.ThemeSelectorBase.TargetScopeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T TargetScope<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.ThemeVariantScope> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ThemeSelectorBase
=> control._setEx(Ursa.Controls.ThemeSelectorBase.TargetScopeProperty, ps, () => control.TargetScope = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // SelectedThemeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> SelectedTheme<T>(this Style<T> style, Avalonia.Styling.ThemeVariant value) where T : Ursa.Controls.ThemeSelectorBase
=> style._addSetter(Ursa.Controls.ThemeSelectorBase.SelectedThemeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SelectedTheme<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ThemeSelectorBase
=> style._addSetter(Ursa.Controls.ThemeSelectorBase.SelectedThemeProperty, binding);


 // ModeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Mode<T>(this Style<T> style, Ursa.Controls.ThemeSelectorMode value) where T : Ursa.Controls.ThemeSelectorBase
=> style._addSetter(Ursa.Controls.ThemeSelectorBase.ModeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Mode<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ThemeSelectorBase
=> style._addSetter(Ursa.Controls.ThemeSelectorBase.ModeProperty, binding);


 // TargetScopeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> TargetScope<T>(this Style<T> style, Avalonia.Controls.ThemeVariantScope value) where T : Ursa.Controls.ThemeSelectorBase
=> style._addSetter(Ursa.Controls.ThemeSelectorBase.TargetScopeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> TargetScope<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ThemeSelectorBase
=> style._addSetter(Ursa.Controls.ThemeSelectorBase.TargetScopeProperty, binding);



}
