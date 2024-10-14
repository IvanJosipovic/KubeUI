#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
[global::System.CodeDom.Compiler.GeneratedCode("AvaloniaExtensionGenerator", "1.0.0.0")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static partial class FAMenuFlyout_MarkupExtensions
{
//================= Properties ======================//
 // ItemsSource

/*BindFromExpressionSetterGenerator*/
public static T ItemsSource<T>(this T control, Func<System.Collections.IEnumerable> func, Action<System.Collections.IEnumerable>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAMenuFlyout 
   => control._set(FluentAvalonia.UI.Controls.FAMenuFlyout.ItemsSourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ItemsSource<T>(this T control,System.Collections.IEnumerable value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAMenuFlyout 
=> control._setEx(FluentAvalonia.UI.Controls.FAMenuFlyout.ItemsSourceProperty, ps, () => control.ItemsSource = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ItemsSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAMenuFlyout 
   => control._set(FluentAvalonia.UI.Controls.FAMenuFlyout.ItemsSourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ItemsSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAMenuFlyout 
   => control._set(FluentAvalonia.UI.Controls.FAMenuFlyout.ItemsSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ItemsSource<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.IEnumerable> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAMenuFlyout 
=> control._setEx(FluentAvalonia.UI.Controls.FAMenuFlyout.ItemsSourceProperty, ps, () => control.ItemsSource = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ItemTemplate

/*BindFromExpressionSetterGenerator*/
public static T ItemTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAMenuFlyout 
   => control._set(FluentAvalonia.UI.Controls.FAMenuFlyout.ItemTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ItemTemplate<T>(this T control,Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAMenuFlyout 
=> control._setEx(FluentAvalonia.UI.Controls.FAMenuFlyout.ItemTemplateProperty, ps, () => control.ItemTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ItemTemplate<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAMenuFlyout 
   => control._set(FluentAvalonia.UI.Controls.FAMenuFlyout.ItemTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ItemTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAMenuFlyout 
   => control._set(FluentAvalonia.UI.Controls.FAMenuFlyout.ItemTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ItemTemplate<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAMenuFlyout 
=> control._setEx(FluentAvalonia.UI.Controls.FAMenuFlyout.ItemTemplateProperty, ps, () => control.ItemTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ItemContainerTheme

/*BindFromExpressionSetterGenerator*/
public static T ItemContainerTheme<T>(this T control, Func<Avalonia.Styling.ControlTheme> func, Action<Avalonia.Styling.ControlTheme>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAMenuFlyout 
   => control._set(FluentAvalonia.UI.Controls.FAMenuFlyout.ItemContainerThemeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ItemContainerTheme<T>(this T control,Avalonia.Styling.ControlTheme value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAMenuFlyout 
=> control._setEx(FluentAvalonia.UI.Controls.FAMenuFlyout.ItemContainerThemeProperty, ps, () => control.ItemContainerTheme = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ItemContainerTheme<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAMenuFlyout 
   => control._set(FluentAvalonia.UI.Controls.FAMenuFlyout.ItemContainerThemeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ItemContainerTheme<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAMenuFlyout 
   => control._set(FluentAvalonia.UI.Controls.FAMenuFlyout.ItemContainerThemeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ItemContainerTheme<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ControlTheme> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAMenuFlyout 
=> control._setEx(FluentAvalonia.UI.Controls.FAMenuFlyout.ItemContainerThemeProperty, ps, () => control.ItemContainerTheme = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // FlyoutPresenterTheme

/*BindFromExpressionSetterGenerator*/
public static T FlyoutPresenterTheme<T>(this T control, Func<Avalonia.Styling.ControlTheme> func, Action<Avalonia.Styling.ControlTheme>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAMenuFlyout 
   => control._set(FluentAvalonia.UI.Controls.FAMenuFlyout.FlyoutPresenterThemeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T FlyoutPresenterTheme<T>(this T control,Avalonia.Styling.ControlTheme value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAMenuFlyout 
=> control._setEx(FluentAvalonia.UI.Controls.FAMenuFlyout.FlyoutPresenterThemeProperty, ps, () => control.FlyoutPresenterTheme = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T FlyoutPresenterTheme<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAMenuFlyout 
   => control._set(FluentAvalonia.UI.Controls.FAMenuFlyout.FlyoutPresenterThemeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T FlyoutPresenterTheme<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAMenuFlyout 
   => control._set(FluentAvalonia.UI.Controls.FAMenuFlyout.FlyoutPresenterThemeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T FlyoutPresenterTheme<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ControlTheme> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAMenuFlyout 
=> control._setEx(FluentAvalonia.UI.Controls.FAMenuFlyout.FlyoutPresenterThemeProperty, ps, () => control.FlyoutPresenterTheme = converter.TryConvert(value), bindingMode, converter, bindingSource);



}
