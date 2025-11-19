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
public static partial class BreadcrumbBar_MarkupExtensions
{
//================= Properties ======================//
 // ItemsSource

/*ValueSetterGenerator*/
public static T ItemsSource<T>(this T control, System.Collections.IEnumerable value) where T : FluentAvalonia.UI.Controls.BreadcrumbBar 
=> control._set(() => control.ItemsSource = value!);

/*BindFromExpressionSetterGenerator*/
public static T ItemsSource<T>(this T control, Func<System.Collections.IEnumerable> func, Action<System.Collections.IEnumerable>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : FluentAvalonia.UI.Controls.BreadcrumbBar 
   => control._set(FluentAvalonia.UI.Controls.BreadcrumbBar.ItemsSourceProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T ItemsSource<T>(this T control,System.Collections.IEnumerable value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : FluentAvalonia.UI.Controls.BreadcrumbBar 
=> control._setEx(FluentAvalonia.UI.Controls.BreadcrumbBar.ItemsSourceProperty, ps, () => control.ItemsSource = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ItemsSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.BreadcrumbBar 
   => control._set(FluentAvalonia.UI.Controls.BreadcrumbBar.ItemsSourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ItemsSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.BreadcrumbBar 
   => control._set(FluentAvalonia.UI.Controls.BreadcrumbBar.ItemsSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T ItemsSource<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.IEnumerable> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : FluentAvalonia.UI.Controls.BreadcrumbBar 
=> control._setEx(FluentAvalonia.UI.Controls.BreadcrumbBar.ItemsSourceProperty, ps, () => control.ItemsSource = converter.TryConvert(value)!, bindingMode, converter, bindingSource);


 // ItemTemplate

/*ValueSetterGenerator*/
public static T ItemTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value) where T : FluentAvalonia.UI.Controls.BreadcrumbBar 
=> control._set(() => control.ItemTemplate = value!);

/*BindFromExpressionSetterGenerator*/
public static T ItemTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : FluentAvalonia.UI.Controls.BreadcrumbBar 
   => control._set(FluentAvalonia.UI.Controls.BreadcrumbBar.ItemTemplateProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T ItemTemplate<T>(this T control,Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : FluentAvalonia.UI.Controls.BreadcrumbBar 
=> control._setEx(FluentAvalonia.UI.Controls.BreadcrumbBar.ItemTemplateProperty, ps, () => control.ItemTemplate = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ItemTemplate<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.BreadcrumbBar 
   => control._set(FluentAvalonia.UI.Controls.BreadcrumbBar.ItemTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ItemTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.BreadcrumbBar 
   => control._set(FluentAvalonia.UI.Controls.BreadcrumbBar.ItemTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T ItemTemplate<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : FluentAvalonia.UI.Controls.BreadcrumbBar 
=> control._setEx(FluentAvalonia.UI.Controls.BreadcrumbBar.ItemTemplateProperty, ps, () => control.ItemTemplate = converter.TryConvert(value)!, bindingMode, converter, bindingSource);


 // IsLastItemClickEnabled

/*ValueSetterGenerator*/
public static T IsLastItemClickEnabled<T>(this T control, System.Boolean value) where T : FluentAvalonia.UI.Controls.BreadcrumbBar 
=> control._set(() => control.IsLastItemClickEnabled = value!);

/*BindFromExpressionSetterGenerator*/
public static T IsLastItemClickEnabled<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : FluentAvalonia.UI.Controls.BreadcrumbBar 
   => control._set(FluentAvalonia.UI.Controls.BreadcrumbBar.IsLastItemClickEnabledProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T IsLastItemClickEnabled<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : FluentAvalonia.UI.Controls.BreadcrumbBar 
=> control._setEx(FluentAvalonia.UI.Controls.BreadcrumbBar.IsLastItemClickEnabledProperty, ps, () => control.IsLastItemClickEnabled = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsLastItemClickEnabled<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.BreadcrumbBar 
   => control._set(FluentAvalonia.UI.Controls.BreadcrumbBar.IsLastItemClickEnabledProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsLastItemClickEnabled<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.BreadcrumbBar 
   => control._set(FluentAvalonia.UI.Controls.BreadcrumbBar.IsLastItemClickEnabledProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T IsLastItemClickEnabled<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : FluentAvalonia.UI.Controls.BreadcrumbBar 
=> control._setEx(FluentAvalonia.UI.Controls.BreadcrumbBar.IsLastItemClickEnabledProperty, ps, () => control.IsLastItemClickEnabled = converter.TryConvert(value)!, bindingMode, converter, bindingSource);



//================= Events ======================//
 // ItemClicked

/*ActionToEventGenerator*/
public static T OnItemClicked<T>(this T control, Action<FluentAvalonia.UI.Controls.BreadcrumbBar, FluentAvalonia.UI.Controls.BreadcrumbBarItemClickedEventArgs> action) where T : FluentAvalonia.UI.Controls.BreadcrumbBar  => 
 control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.BreadcrumbBar,FluentAvalonia.UI.Controls.BreadcrumbBarItemClickedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.ItemClicked += h);



//================= Styles ======================//
 // ItemsSource

/*ValueStyleSetterGenerator*/
public static Style<T> ItemsSource<T>(this Style<T> style, System.Collections.IEnumerable value) where T : FluentAvalonia.UI.Controls.BreadcrumbBar 
=> style._addSetter(FluentAvalonia.UI.Controls.BreadcrumbBar.ItemsSourceProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> ItemsSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.BreadcrumbBar 
=> style._addSetter(FluentAvalonia.UI.Controls.BreadcrumbBar.ItemsSourceProperty, binding);


 // ItemTemplate

/*ValueStyleSetterGenerator*/
public static Style<T> ItemTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : FluentAvalonia.UI.Controls.BreadcrumbBar 
=> style._addSetter(FluentAvalonia.UI.Controls.BreadcrumbBar.ItemTemplateProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> ItemTemplate<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.BreadcrumbBar 
=> style._addSetter(FluentAvalonia.UI.Controls.BreadcrumbBar.ItemTemplateProperty, binding);


 // IsLastItemClickEnabled

/*ValueStyleSetterGenerator*/
public static Style<T> IsLastItemClickEnabled<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.BreadcrumbBar 
=> style._addSetter(FluentAvalonia.UI.Controls.BreadcrumbBar.IsLastItemClickEnabledProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> IsLastItemClickEnabled<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.BreadcrumbBar 
=> style._addSetter(FluentAvalonia.UI.Controls.BreadcrumbBar.IsLastItemClickEnabledProperty, binding);



}
