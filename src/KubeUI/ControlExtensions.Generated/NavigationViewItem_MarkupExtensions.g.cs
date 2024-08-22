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
public static partial class NavigationViewItem_MarkupExtensions
{
//================= Properties ======================//
 // HasUnrealizedChildrenProperty

/*BindFromExpressionSetterGenerator*/
public static T HasUnrealizedChildren<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.HasUnrealizedChildrenProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T HasUnrealizedChildren<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> control._setEx(FluentAvalonia.UI.Controls.NavigationViewItem.HasUnrealizedChildrenProperty, ps, () => control.HasUnrealizedChildren = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T HasUnrealizedChildren<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.HasUnrealizedChildrenProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T HasUnrealizedChildren<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.HasUnrealizedChildrenProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T HasUnrealizedChildren<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> control._setEx(FluentAvalonia.UI.Controls.NavigationViewItem.HasUnrealizedChildrenProperty, ps, () => control.HasUnrealizedChildren = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IconSourceProperty

/*BindFromExpressionSetterGenerator*/
public static T IconSource<T>(this T control, Func<FluentAvalonia.UI.Controls.IconSource> func, Action<FluentAvalonia.UI.Controls.IconSource>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.IconSourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IconSource<T>(this T control, FluentAvalonia.UI.Controls.IconSource value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> control._setEx(FluentAvalonia.UI.Controls.NavigationViewItem.IconSourceProperty, ps, () => control.IconSource = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IconSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.IconSourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IconSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.IconSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IconSource<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.IconSource> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> control._setEx(FluentAvalonia.UI.Controls.NavigationViewItem.IconSourceProperty, ps, () => control.IconSource = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsChildSelectedProperty

/*BindFromExpressionSetterGenerator*/
public static T IsChildSelected<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.IsChildSelectedProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsChildSelected<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> control._setEx(FluentAvalonia.UI.Controls.NavigationViewItem.IsChildSelectedProperty, ps, () => control.IsChildSelected = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsChildSelected<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.IsChildSelectedProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsChildSelected<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.IsChildSelectedProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsChildSelected<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> control._setEx(FluentAvalonia.UI.Controls.NavigationViewItem.IsChildSelectedProperty, ps, () => control.IsChildSelected = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsExpandedProperty

/*BindFromExpressionSetterGenerator*/
public static T IsExpanded<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.IsExpandedProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsExpanded<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> control._setEx(FluentAvalonia.UI.Controls.NavigationViewItem.IsExpandedProperty, ps, () => control.IsExpanded = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsExpanded<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.IsExpandedProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsExpanded<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.IsExpandedProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsExpanded<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> control._setEx(FluentAvalonia.UI.Controls.NavigationViewItem.IsExpandedProperty, ps, () => control.IsExpanded = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // MenuItemsSourceProperty

/*BindFromExpressionSetterGenerator*/
public static T MenuItemsSource<T>(this T control, Func<System.Collections.IEnumerable> func, Action<System.Collections.IEnumerable>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.MenuItemsSourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T MenuItemsSource<T>(this T control, System.Collections.IEnumerable value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> control._setEx(FluentAvalonia.UI.Controls.NavigationViewItem.MenuItemsSourceProperty, ps, () => control.MenuItemsSource = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T MenuItemsSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.MenuItemsSourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T MenuItemsSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.MenuItemsSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T MenuItemsSource<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.IEnumerable> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> control._setEx(FluentAvalonia.UI.Controls.NavigationViewItem.MenuItemsSourceProperty, ps, () => control.MenuItemsSource = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SelectsOnInvokedProperty

/*BindFromExpressionSetterGenerator*/
public static T SelectsOnInvoked<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.SelectsOnInvokedProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SelectsOnInvoked<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> control._setEx(FluentAvalonia.UI.Controls.NavigationViewItem.SelectsOnInvokedProperty, ps, () => control.SelectsOnInvoked = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SelectsOnInvoked<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.SelectsOnInvokedProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SelectsOnInvoked<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.SelectsOnInvokedProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SelectsOnInvoked<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> control._setEx(FluentAvalonia.UI.Controls.NavigationViewItem.SelectsOnInvokedProperty, ps, () => control.SelectsOnInvoked = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // InfoBadgeProperty

/*BindFromExpressionSetterGenerator*/
public static T InfoBadge<T>(this T control, Func<FluentAvalonia.UI.Controls.InfoBadge> func, Action<FluentAvalonia.UI.Controls.InfoBadge>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.InfoBadgeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T InfoBadge<T>(this T control, FluentAvalonia.UI.Controls.InfoBadge value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> control._setEx(FluentAvalonia.UI.Controls.NavigationViewItem.InfoBadgeProperty, ps, () => control.InfoBadge = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T InfoBadge<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.InfoBadgeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T InfoBadge<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
   => control._set(FluentAvalonia.UI.Controls.NavigationViewItem.InfoBadgeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T InfoBadge<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.InfoBadge> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> control._setEx(FluentAvalonia.UI.Controls.NavigationViewItem.InfoBadgeProperty, ps, () => control.InfoBadge = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // IconSourceProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IconSource<T>(this Style<T> style, FluentAvalonia.UI.Controls.IconSource value) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationViewItem.IconSourceProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IconSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationViewItem.IconSourceProperty, binding);


 // MenuItemsSourceProperty

/*ValueStyleSetterGenerator*/
public static Style<T> MenuItemsSource<T>(this Style<T> style, System.Collections.IEnumerable value) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationViewItem.MenuItemsSourceProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> MenuItemsSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationViewItem.MenuItemsSourceProperty, binding);


 // InfoBadgeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> InfoBadge<T>(this Style<T> style, FluentAvalonia.UI.Controls.InfoBadge value) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationViewItem.InfoBadgeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> InfoBadge<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationViewItem
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationViewItem.InfoBadgeProperty, binding);



}
