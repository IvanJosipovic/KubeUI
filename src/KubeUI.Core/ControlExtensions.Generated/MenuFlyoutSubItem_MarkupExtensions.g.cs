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
public static partial class MenuFlyoutSubItem_MarkupExtensions
{
//================= Properties ======================//
 // Text

/*BindFromExpressionSetterGenerator*/
public static T Text<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem 
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.TextProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Text<T>(this T control,System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem 
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.TextProperty, ps, () => control.Text = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Text<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem 
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.TextProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Text<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem 
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.TextProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Text<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem 
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.TextProperty, ps, () => control.Text = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IconSource

/*BindFromExpressionSetterGenerator*/
public static T IconSource<T>(this T control, Func<FluentAvalonia.UI.Controls.IconSource> func, Action<FluentAvalonia.UI.Controls.IconSource>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem 
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.IconSourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IconSource<T>(this T control,FluentAvalonia.UI.Controls.IconSource value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem 
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.IconSourceProperty, ps, () => control.IconSource = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IconSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem 
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.IconSourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IconSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem 
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.IconSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IconSource<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.IconSource> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem 
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.IconSourceProperty, ps, () => control.IconSource = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ItemsSource

/*BindFromExpressionSetterGenerator*/
public static T ItemsSource<T>(this T control, Func<System.Collections.IEnumerable> func, Action<System.Collections.IEnumerable>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem 
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemsSourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ItemsSource<T>(this T control,System.Collections.IEnumerable value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem 
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemsSourceProperty, ps, () => control.ItemsSource = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ItemsSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem 
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemsSourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ItemsSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem 
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemsSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ItemsSource<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.IEnumerable> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem 
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemsSourceProperty, ps, () => control.ItemsSource = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ItemTemplate

/*BindFromExpressionSetterGenerator*/
public static T ItemTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem 
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ItemTemplate<T>(this T control,Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem 
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemTemplateProperty, ps, () => control.ItemTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ItemTemplate<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem 
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ItemTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem 
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ItemTemplate<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem 
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemTemplateProperty, ps, () => control.ItemTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ItemContainerTheme

/*BindFromExpressionSetterGenerator*/
public static T ItemContainerTheme<T>(this T control, Func<Avalonia.Styling.ControlTheme> func, Action<Avalonia.Styling.ControlTheme>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem 
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemContainerThemeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ItemContainerTheme<T>(this T control,Avalonia.Styling.ControlTheme value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem 
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemContainerThemeProperty, ps, () => control.ItemContainerTheme = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ItemContainerTheme<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem 
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemContainerThemeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ItemContainerTheme<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem 
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemContainerThemeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ItemContainerTheme<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ControlTheme> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem 
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemContainerThemeProperty, ps, () => control.ItemContainerTheme = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Styles ======================//
 // Text

/*ValueStyleSetterGenerator*/
public static Style<T> Text<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem 
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.TextProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Text<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem 
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.TextProperty, binding);


 // IconSource

/*ValueStyleSetterGenerator*/
public static Style<T> IconSource<T>(this Style<T> style, FluentAvalonia.UI.Controls.IconSource value) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem 
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.IconSourceProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IconSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem 
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.IconSourceProperty, binding);


 // ItemsSource

/*ValueStyleSetterGenerator*/
public static Style<T> ItemsSource<T>(this Style<T> style, System.Collections.IEnumerable value) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem 
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemsSourceProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ItemsSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem 
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemsSourceProperty, binding);


 // ItemTemplate

/*ValueStyleSetterGenerator*/
public static Style<T> ItemTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem 
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemTemplateProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ItemTemplate<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem 
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemTemplateProperty, binding);


 // ItemContainerTheme

/*ValueStyleSetterGenerator*/
public static Style<T> ItemContainerTheme<T>(this Style<T> style, Avalonia.Styling.ControlTheme value) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem 
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemContainerThemeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ItemContainerTheme<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutSubItem 
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutSubItem.ItemContainerThemeProperty, binding);



}
