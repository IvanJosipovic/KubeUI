#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class RadioMenuFlyoutItem_MarkupExtensions
{
//================= Properties ======================//
 // GroupNameProperty

/*BindFromExpressionSetterGenerator*/
public static T GroupName<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.RadioMenuFlyoutItem
   => control._set(FluentAvalonia.UI.Controls.RadioMenuFlyoutItem.GroupNameProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T GroupName<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.RadioMenuFlyoutItem
=> control._setEx(FluentAvalonia.UI.Controls.RadioMenuFlyoutItem.GroupNameProperty, ps, () => control.GroupName = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T GroupName<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.RadioMenuFlyoutItem
   => control._set(FluentAvalonia.UI.Controls.RadioMenuFlyoutItem.GroupNameProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T GroupName<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.RadioMenuFlyoutItem
   => control._set(FluentAvalonia.UI.Controls.RadioMenuFlyoutItem.GroupNameProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T GroupName<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.RadioMenuFlyoutItem
=> control._setEx(FluentAvalonia.UI.Controls.RadioMenuFlyoutItem.GroupNameProperty, ps, () => control.GroupName = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsCheckedProperty

/*BindFromExpressionSetterGenerator*/
public static T IsChecked<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.RadioMenuFlyoutItem
   => control._set(FluentAvalonia.UI.Controls.RadioMenuFlyoutItem.IsCheckedProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsChecked<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.RadioMenuFlyoutItem
=> control._setEx(FluentAvalonia.UI.Controls.RadioMenuFlyoutItem.IsCheckedProperty, ps, () => control.IsChecked = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsChecked<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.RadioMenuFlyoutItem
   => control._set(FluentAvalonia.UI.Controls.RadioMenuFlyoutItem.IsCheckedProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsChecked<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.RadioMenuFlyoutItem
   => control._set(FluentAvalonia.UI.Controls.RadioMenuFlyoutItem.IsCheckedProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsChecked<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.RadioMenuFlyoutItem
=> control._setEx(FluentAvalonia.UI.Controls.RadioMenuFlyoutItem.IsCheckedProperty, ps, () => control.IsChecked = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // GroupNameProperty

/*ValueStyleSetterGenerator*/
public static Style<T> GroupName<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.RadioMenuFlyoutItem
=> style._addSetter(FluentAvalonia.UI.Controls.RadioMenuFlyoutItem.GroupNameProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> GroupName<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.RadioMenuFlyoutItem
=> style._addSetter(FluentAvalonia.UI.Controls.RadioMenuFlyoutItem.GroupNameProperty, binding);


 // IsCheckedProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsChecked<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.RadioMenuFlyoutItem
=> style._addSetter(FluentAvalonia.UI.Controls.RadioMenuFlyoutItem.IsCheckedProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsChecked<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.RadioMenuFlyoutItem
=> style._addSetter(FluentAvalonia.UI.Controls.RadioMenuFlyoutItem.IsCheckedProperty, binding);



}
