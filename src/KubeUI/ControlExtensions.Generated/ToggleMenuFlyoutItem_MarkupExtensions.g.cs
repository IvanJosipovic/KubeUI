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
public static partial class ToggleMenuFlyoutItem_MarkupExtensions
{
//================= Properties ======================//
 // IsCheckedProperty

/*BindFromExpressionSetterGenerator*/
public static T IsChecked<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ToggleMenuFlyoutItem
   => control._set(FluentAvalonia.UI.Controls.ToggleMenuFlyoutItem.IsCheckedProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsChecked<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ToggleMenuFlyoutItem
=> control._setEx(FluentAvalonia.UI.Controls.ToggleMenuFlyoutItem.IsCheckedProperty, ps, () => control.IsChecked = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsChecked<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ToggleMenuFlyoutItem
   => control._set(FluentAvalonia.UI.Controls.ToggleMenuFlyoutItem.IsCheckedProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsChecked<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ToggleMenuFlyoutItem
   => control._set(FluentAvalonia.UI.Controls.ToggleMenuFlyoutItem.IsCheckedProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsChecked<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ToggleMenuFlyoutItem
=> control._setEx(FluentAvalonia.UI.Controls.ToggleMenuFlyoutItem.IsCheckedProperty, ps, () => control.IsChecked = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // IsCheckedProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsChecked<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.ToggleMenuFlyoutItem
=> style._addSetter(FluentAvalonia.UI.Controls.ToggleMenuFlyoutItem.IsCheckedProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsChecked<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ToggleMenuFlyoutItem
=> style._addSetter(FluentAvalonia.UI.Controls.ToggleMenuFlyoutItem.IsCheckedProperty, binding);



}
