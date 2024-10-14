#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
[global::System.CodeDom.Compiler.GeneratedCode("AvaloniaExtensionGenerator", "11.1.3.0")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static partial class Frame_MarkupExtensions
{
//================= Properties ======================//
 // SourcePageTypeProperty

/*BindFromExpressionSetterGenerator*/
public static T SourcePageType<T>(this T control, Func<System.Type> func, Action<System.Type>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.Frame
   => control._set(FluentAvalonia.UI.Controls.Frame.SourcePageTypeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SourcePageType<T>(this T control, System.Type value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Frame
=> control._setEx(FluentAvalonia.UI.Controls.Frame.SourcePageTypeProperty, ps, () => control.SourcePageType = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SourcePageType<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.Frame
   => control._set(FluentAvalonia.UI.Controls.Frame.SourcePageTypeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SourcePageType<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.Frame
   => control._set(FluentAvalonia.UI.Controls.Frame.SourcePageTypeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SourcePageType<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Type> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Frame
=> control._setEx(FluentAvalonia.UI.Controls.Frame.SourcePageTypeProperty, ps, () => control.SourcePageType = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CacheSizeProperty

/*BindFromExpressionSetterGenerator*/
public static T CacheSize<T>(this T control, Func<System.Int32> func, Action<System.Int32>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.Frame
   => control._set(FluentAvalonia.UI.Controls.Frame.CacheSizeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CacheSize<T>(this T control, System.Int32 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Frame
=> control._setEx(FluentAvalonia.UI.Controls.Frame.CacheSizeProperty, ps, () => control.CacheSize = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CacheSize<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.Frame
   => control._set(FluentAvalonia.UI.Controls.Frame.CacheSizeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CacheSize<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.Frame
   => control._set(FluentAvalonia.UI.Controls.Frame.CacheSizeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CacheSize<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Int32> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Frame
=> control._setEx(FluentAvalonia.UI.Controls.Frame.CacheSizeProperty, ps, () => control.CacheSize = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsNavigationStackEnabledProperty

/*BindFromExpressionSetterGenerator*/
public static T IsNavigationStackEnabled<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.Frame
   => control._set(FluentAvalonia.UI.Controls.Frame.IsNavigationStackEnabledProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsNavigationStackEnabled<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Frame
=> control._setEx(FluentAvalonia.UI.Controls.Frame.IsNavigationStackEnabledProperty, ps, () => control.IsNavigationStackEnabled = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsNavigationStackEnabled<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.Frame
   => control._set(FluentAvalonia.UI.Controls.Frame.IsNavigationStackEnabledProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsNavigationStackEnabled<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.Frame
   => control._set(FluentAvalonia.UI.Controls.Frame.IsNavigationStackEnabledProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsNavigationStackEnabled<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Frame
=> control._setEx(FluentAvalonia.UI.Controls.Frame.IsNavigationStackEnabledProperty, ps, () => control.IsNavigationStackEnabled = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // NavigationPageFactoryProperty

/*BindFromExpressionSetterGenerator*/
public static T NavigationPageFactory<T>(this T control, Func<FluentAvalonia.UI.Controls.INavigationPageFactory> func, Action<FluentAvalonia.UI.Controls.INavigationPageFactory>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.Frame
   => control._set(FluentAvalonia.UI.Controls.Frame.NavigationPageFactoryProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T NavigationPageFactory<T>(this T control, FluentAvalonia.UI.Controls.INavigationPageFactory value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Frame
=> control._setEx(FluentAvalonia.UI.Controls.Frame.NavigationPageFactoryProperty, ps, () => control.NavigationPageFactory = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T NavigationPageFactory<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.Frame
   => control._set(FluentAvalonia.UI.Controls.Frame.NavigationPageFactoryProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T NavigationPageFactory<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.Frame
   => control._set(FluentAvalonia.UI.Controls.Frame.NavigationPageFactoryProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T NavigationPageFactory<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.INavigationPageFactory> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Frame
=> control._setEx(FluentAvalonia.UI.Controls.Frame.NavigationPageFactoryProperty, ps, () => control.NavigationPageFactory = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // Navigated

/*ActionToEventGenerator*/
    public static T OnNavigated<T>(this T control, Action<FluentAvalonia.UI.Navigation.NavigationEventArgs> action) where T : FluentAvalonia.UI.Controls.Frame => 
        control._setEvent((FluentAvalonia.UI.Navigation.NavigatedEventHandler) ((arg0, arg1) => action(arg1)), h => control.Navigated += h);


 // Navigating

/*ActionToEventGenerator*/
    public static T OnNavigating<T>(this T control, Action<FluentAvalonia.UI.Navigation.NavigatingCancelEventArgs> action) where T : FluentAvalonia.UI.Controls.Frame => 
        control._setEvent((FluentAvalonia.UI.Navigation.NavigatingCancelEventHandler) ((arg0, arg1) => action(arg1)), h => control.Navigating += h);


 // NavigationFailed

/*ActionToEventGenerator*/
    public static T OnNavigationFailed<T>(this T control, Action<FluentAvalonia.UI.Navigation.NavigationFailedEventArgs> action) where T : FluentAvalonia.UI.Controls.Frame => 
        control._setEvent((FluentAvalonia.UI.Navigation.NavigationFailedEventHandler) ((arg0, arg1) => action(arg1)), h => control.NavigationFailed += h);


 // NavigationStopped

/*ActionToEventGenerator*/
    public static T OnNavigationStopped<T>(this T control, Action<FluentAvalonia.UI.Navigation.NavigationEventArgs> action) where T : FluentAvalonia.UI.Controls.Frame => 
        control._setEvent((FluentAvalonia.UI.Navigation.NavigationStoppedEventHandler) ((arg0, arg1) => action(arg1)), h => control.NavigationStopped += h);



//================= Styles ======================//
 // SourcePageTypeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> SourcePageType<T>(this Style<T> style, System.Type value) where T : FluentAvalonia.UI.Controls.Frame
=> style._addSetter(FluentAvalonia.UI.Controls.Frame.SourcePageTypeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SourcePageType<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.Frame
=> style._addSetter(FluentAvalonia.UI.Controls.Frame.SourcePageTypeProperty, binding);


 // CacheSizeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> CacheSize<T>(this Style<T> style, System.Int32 value) where T : FluentAvalonia.UI.Controls.Frame
=> style._addSetter(FluentAvalonia.UI.Controls.Frame.CacheSizeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CacheSize<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.Frame
=> style._addSetter(FluentAvalonia.UI.Controls.Frame.CacheSizeProperty, binding);


 // IsNavigationStackEnabledProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsNavigationStackEnabled<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.Frame
=> style._addSetter(FluentAvalonia.UI.Controls.Frame.IsNavigationStackEnabledProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsNavigationStackEnabled<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.Frame
=> style._addSetter(FluentAvalonia.UI.Controls.Frame.IsNavigationStackEnabledProperty, binding);



}
