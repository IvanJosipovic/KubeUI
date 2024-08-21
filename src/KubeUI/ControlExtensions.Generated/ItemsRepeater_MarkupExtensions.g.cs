#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class ItemsRepeater_MarkupExtensions
{
//================= Properties ======================//
 // VerticalCacheLengthProperty

/*BindFromExpressionSetterGenerator*/
public static T VerticalCacheLength<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
   => control._set(FluentAvalonia.UI.Controls.ItemsRepeater.VerticalCacheLengthProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T VerticalCacheLength<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> control._setEx(FluentAvalonia.UI.Controls.ItemsRepeater.VerticalCacheLengthProperty, ps, () => control.VerticalCacheLength = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T VerticalCacheLength<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ItemsRepeater
   => control._set(FluentAvalonia.UI.Controls.ItemsRepeater.VerticalCacheLengthProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T VerticalCacheLength<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
   => control._set(FluentAvalonia.UI.Controls.ItemsRepeater.VerticalCacheLengthProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T VerticalCacheLength<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> control._setEx(FluentAvalonia.UI.Controls.ItemsRepeater.VerticalCacheLengthProperty, ps, () => control.VerticalCacheLength = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // HorizontalCacheLengthProperty

/*BindFromExpressionSetterGenerator*/
public static T HorizontalCacheLength<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
   => control._set(FluentAvalonia.UI.Controls.ItemsRepeater.HorizontalCacheLengthProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T HorizontalCacheLength<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> control._setEx(FluentAvalonia.UI.Controls.ItemsRepeater.HorizontalCacheLengthProperty, ps, () => control.HorizontalCacheLength = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T HorizontalCacheLength<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ItemsRepeater
   => control._set(FluentAvalonia.UI.Controls.ItemsRepeater.HorizontalCacheLengthProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T HorizontalCacheLength<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
   => control._set(FluentAvalonia.UI.Controls.ItemsRepeater.HorizontalCacheLengthProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T HorizontalCacheLength<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> control._setEx(FluentAvalonia.UI.Controls.ItemsRepeater.HorizontalCacheLengthProperty, ps, () => control.HorizontalCacheLength = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // LayoutProperty

/*BindFromExpressionSetterGenerator*/
public static T Layout<T>(this T control, Func<FluentAvalonia.UI.Controls.Layout> func, Action<FluentAvalonia.UI.Controls.Layout>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
   => control._set(FluentAvalonia.UI.Controls.ItemsRepeater.LayoutProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Layout<T>(this T control, FluentAvalonia.UI.Controls.Layout value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> control._setEx(FluentAvalonia.UI.Controls.ItemsRepeater.LayoutProperty, ps, () => control.Layout = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Layout<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ItemsRepeater
   => control._set(FluentAvalonia.UI.Controls.ItemsRepeater.LayoutProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Layout<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
   => control._set(FluentAvalonia.UI.Controls.ItemsRepeater.LayoutProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Layout<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.Layout> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> control._setEx(FluentAvalonia.UI.Controls.ItemsRepeater.LayoutProperty, ps, () => control.Layout = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ItemsSourceProperty

/*BindFromExpressionSetterGenerator*/
public static T ItemsSource<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
   => control._set(FluentAvalonia.UI.Controls.ItemsRepeater.ItemsSourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ItemsSource<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> control._setEx(FluentAvalonia.UI.Controls.ItemsRepeater.ItemsSourceProperty, ps, () => control.ItemsSource = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ItemsSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ItemsRepeater
   => control._set(FluentAvalonia.UI.Controls.ItemsRepeater.ItemsSourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ItemsSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
   => control._set(FluentAvalonia.UI.Controls.ItemsRepeater.ItemsSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ItemsSource<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> control._setEx(FluentAvalonia.UI.Controls.ItemsRepeater.ItemsSourceProperty, ps, () => control.ItemsSource = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ItemTemplateProperty

/*BindFromExpressionSetterGenerator*/
public static T ItemTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
   => control._set(FluentAvalonia.UI.Controls.ItemsRepeater.ItemTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ItemTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> control._setEx(FluentAvalonia.UI.Controls.ItemsRepeater.ItemTemplateProperty, ps, () => control.ItemTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ItemTemplate<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ItemsRepeater
   => control._set(FluentAvalonia.UI.Controls.ItemsRepeater.ItemTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ItemTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
   => control._set(FluentAvalonia.UI.Controls.ItemsRepeater.ItemTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ItemTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> control._setEx(FluentAvalonia.UI.Controls.ItemsRepeater.ItemTemplateProperty, ps, () => control.ItemTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ItemTransitionProviderProperty

/*BindFromExpressionSetterGenerator*/
public static T ItemTransitionProvider<T>(this T control, Func<FluentAvalonia.UI.Controls.ItemCollectionTransitionProvider> func, Action<FluentAvalonia.UI.Controls.ItemCollectionTransitionProvider>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
   => control._set(FluentAvalonia.UI.Controls.ItemsRepeater.ItemTransitionProviderProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ItemTransitionProvider<T>(this T control, FluentAvalonia.UI.Controls.ItemCollectionTransitionProvider value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> control._setEx(FluentAvalonia.UI.Controls.ItemsRepeater.ItemTransitionProviderProperty, ps, () => control.ItemTransitionProvider = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ItemTransitionProvider<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ItemsRepeater
   => control._set(FluentAvalonia.UI.Controls.ItemsRepeater.ItemTransitionProviderProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ItemTransitionProvider<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
   => control._set(FluentAvalonia.UI.Controls.ItemsRepeater.ItemTransitionProviderProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ItemTransitionProvider<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.ItemCollectionTransitionProvider> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> control._setEx(FluentAvalonia.UI.Controls.ItemsRepeater.ItemTransitionProviderProperty, ps, () => control.ItemTransitionProvider = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // ElementPrepared

/*ActionToEventGenerator*/
    public static T OnElementPrepared<T>(this T control, Action<FluentAvalonia.UI.Controls.ItemsRepeater, FluentAvalonia.UI.Controls.ItemsRepeaterElementPreparedEventArgs> action) where T : FluentAvalonia.UI.Controls.ItemsRepeater => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.ItemsRepeater,FluentAvalonia.UI.Controls.ItemsRepeaterElementPreparedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.ElementPrepared += h);


 // ElementClearing

/*ActionToEventGenerator*/
    public static T OnElementClearing<T>(this T control, Action<FluentAvalonia.UI.Controls.ItemsRepeater, FluentAvalonia.UI.Controls.ItemsRepeaterElementClearingEventArgs> action) where T : FluentAvalonia.UI.Controls.ItemsRepeater => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.ItemsRepeater,FluentAvalonia.UI.Controls.ItemsRepeaterElementClearingEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.ElementClearing += h);


 // ElementIndexChanged

/*ActionToEventGenerator*/
    public static T OnElementIndexChanged<T>(this T control, Action<FluentAvalonia.UI.Controls.ItemsRepeater, FluentAvalonia.UI.Controls.ItemsRepeaterElementIndexChangedEventArgs> action) where T : FluentAvalonia.UI.Controls.ItemsRepeater => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.ItemsRepeater,FluentAvalonia.UI.Controls.ItemsRepeaterElementIndexChangedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.ElementIndexChanged += h);


 // ContainerContentChanging

/*ActionToEventGenerator*/
    public static T OnContainerContentChanging<T>(this T control, Action<FluentAvalonia.UI.Controls.ItemsRepeater, FluentAvalonia.UI.Controls.ContainerContentChangingEventArgs> action) where T : FluentAvalonia.UI.Controls.ItemsRepeater => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.ItemsRepeater,FluentAvalonia.UI.Controls.ContainerContentChangingEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.ContainerContentChanging += h);



//================= Styles ======================//
 // VerticalCacheLengthProperty

/*ValueStyleSetterGenerator*/
public static Style<T> VerticalCacheLength<T>(this Style<T> style, System.Double value) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> style._addSetter(FluentAvalonia.UI.Controls.ItemsRepeater.VerticalCacheLengthProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> VerticalCacheLength<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> style._addSetter(FluentAvalonia.UI.Controls.ItemsRepeater.VerticalCacheLengthProperty, binding);


 // HorizontalCacheLengthProperty

/*ValueStyleSetterGenerator*/
public static Style<T> HorizontalCacheLength<T>(this Style<T> style, System.Double value) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> style._addSetter(FluentAvalonia.UI.Controls.ItemsRepeater.HorizontalCacheLengthProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> HorizontalCacheLength<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> style._addSetter(FluentAvalonia.UI.Controls.ItemsRepeater.HorizontalCacheLengthProperty, binding);


 // LayoutProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Layout<T>(this Style<T> style, FluentAvalonia.UI.Controls.Layout value) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> style._addSetter(FluentAvalonia.UI.Controls.ItemsRepeater.LayoutProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Layout<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> style._addSetter(FluentAvalonia.UI.Controls.ItemsRepeater.LayoutProperty, binding);


 // ItemsSourceProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ItemsSource<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> style._addSetter(FluentAvalonia.UI.Controls.ItemsRepeater.ItemsSourceProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ItemsSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> style._addSetter(FluentAvalonia.UI.Controls.ItemsRepeater.ItemsSourceProperty, binding);


 // ItemTemplateProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ItemTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> style._addSetter(FluentAvalonia.UI.Controls.ItemsRepeater.ItemTemplateProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ItemTemplate<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> style._addSetter(FluentAvalonia.UI.Controls.ItemsRepeater.ItemTemplateProperty, binding);


 // ItemTransitionProviderProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ItemTransitionProvider<T>(this Style<T> style, FluentAvalonia.UI.Controls.ItemCollectionTransitionProvider value) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> style._addSetter(FluentAvalonia.UI.Controls.ItemsRepeater.ItemTransitionProviderProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ItemTransitionProvider<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> style._addSetter(FluentAvalonia.UI.Controls.ItemsRepeater.ItemTransitionProviderProperty, binding);



}
