#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class TabViewItem_MarkupExtensions
{
//================= Properties ======================//
 // HeaderProperty

/*BindFromExpressionSetterGenerator*/
public static T Header<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabViewItem
   => control._set(FluentAvalonia.UI.Controls.TabViewItem.HeaderProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Header<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabViewItem
=> control._setEx(FluentAvalonia.UI.Controls.TabViewItem.HeaderProperty, ps, () => control.Header = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Header<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabViewItem
   => control._set(FluentAvalonia.UI.Controls.TabViewItem.HeaderProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Header<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabViewItem
   => control._set(FluentAvalonia.UI.Controls.TabViewItem.HeaderProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Header<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabViewItem
=> control._setEx(FluentAvalonia.UI.Controls.TabViewItem.HeaderProperty, ps, () => control.Header = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // HeaderTemplateProperty

/*BindFromExpressionSetterGenerator*/
public static T HeaderTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabViewItem
   => control._set(FluentAvalonia.UI.Controls.TabViewItem.HeaderTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T HeaderTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabViewItem
=> control._setEx(FluentAvalonia.UI.Controls.TabViewItem.HeaderTemplateProperty, ps, () => control.HeaderTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T HeaderTemplate<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabViewItem
   => control._set(FluentAvalonia.UI.Controls.TabViewItem.HeaderTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T HeaderTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabViewItem
   => control._set(FluentAvalonia.UI.Controls.TabViewItem.HeaderTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T HeaderTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabViewItem
=> control._setEx(FluentAvalonia.UI.Controls.TabViewItem.HeaderTemplateProperty, ps, () => control.HeaderTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IconSourceProperty

/*BindFromExpressionSetterGenerator*/
public static T IconSource<T>(this T control, Func<FluentAvalonia.UI.Controls.IconSource> func, Action<FluentAvalonia.UI.Controls.IconSource>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabViewItem
   => control._set(FluentAvalonia.UI.Controls.TabViewItem.IconSourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IconSource<T>(this T control, FluentAvalonia.UI.Controls.IconSource value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabViewItem
=> control._setEx(FluentAvalonia.UI.Controls.TabViewItem.IconSourceProperty, ps, () => control.IconSource = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IconSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabViewItem
   => control._set(FluentAvalonia.UI.Controls.TabViewItem.IconSourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IconSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabViewItem
   => control._set(FluentAvalonia.UI.Controls.TabViewItem.IconSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IconSource<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.IconSource> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabViewItem
=> control._setEx(FluentAvalonia.UI.Controls.TabViewItem.IconSourceProperty, ps, () => control.IconSource = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsClosableProperty

/*BindFromExpressionSetterGenerator*/
public static T IsClosable<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TabViewItem
   => control._set(FluentAvalonia.UI.Controls.TabViewItem.IsClosableProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsClosable<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabViewItem
=> control._setEx(FluentAvalonia.UI.Controls.TabViewItem.IsClosableProperty, ps, () => control.IsClosable = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsClosable<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TabViewItem
   => control._set(FluentAvalonia.UI.Controls.TabViewItem.IsClosableProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsClosable<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TabViewItem
   => control._set(FluentAvalonia.UI.Controls.TabViewItem.IsClosableProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsClosable<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TabViewItem
=> control._setEx(FluentAvalonia.UI.Controls.TabViewItem.IsClosableProperty, ps, () => control.IsClosable = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // CloseRequested

/*ActionToEventGenerator*/
    public static T OnCloseRequested<T>(this T control, Action<FluentAvalonia.UI.Controls.TabViewItem, FluentAvalonia.UI.Controls.TabViewTabCloseRequestedEventArgs> action) where T : FluentAvalonia.UI.Controls.TabViewItem => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.TabViewItem,FluentAvalonia.UI.Controls.TabViewTabCloseRequestedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.CloseRequested += h);



//================= Styles ======================//
 // HeaderProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Header<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.TabViewItem
=> style._addSetter(FluentAvalonia.UI.Controls.TabViewItem.HeaderProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Header<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TabViewItem
=> style._addSetter(FluentAvalonia.UI.Controls.TabViewItem.HeaderProperty, binding);


 // HeaderTemplateProperty

/*ValueStyleSetterGenerator*/
public static Style<T> HeaderTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : FluentAvalonia.UI.Controls.TabViewItem
=> style._addSetter(FluentAvalonia.UI.Controls.TabViewItem.HeaderTemplateProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> HeaderTemplate<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TabViewItem
=> style._addSetter(FluentAvalonia.UI.Controls.TabViewItem.HeaderTemplateProperty, binding);


 // IconSourceProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IconSource<T>(this Style<T> style, FluentAvalonia.UI.Controls.IconSource value) where T : FluentAvalonia.UI.Controls.TabViewItem
=> style._addSetter(FluentAvalonia.UI.Controls.TabViewItem.IconSourceProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IconSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TabViewItem
=> style._addSetter(FluentAvalonia.UI.Controls.TabViewItem.IconSourceProperty, binding);


 // IsClosableProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsClosable<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.TabViewItem
=> style._addSetter(FluentAvalonia.UI.Controls.TabViewItem.IsClosableProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsClosable<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TabViewItem
=> style._addSetter(FluentAvalonia.UI.Controls.TabViewItem.IsClosableProperty, binding);



}
