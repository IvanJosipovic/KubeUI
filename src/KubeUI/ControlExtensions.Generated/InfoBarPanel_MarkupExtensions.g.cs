#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class InfoBarPanel_MarkupExtensions
{
//================= Properties ======================//
 // HorizontalOrientationPaddingProperty

/*BindFromExpressionSetterGenerator*/
public static T HorizontalOrientationPadding<T>(this T control, Func<Avalonia.Thickness> func, Action<Avalonia.Thickness>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
   => control._set(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.HorizontalOrientationPaddingProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T HorizontalOrientationPadding<T>(this T control, Avalonia.Thickness value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
=> control._setEx(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.HorizontalOrientationPaddingProperty, ps, () => control.HorizontalOrientationPadding = value, bindingMode, converter, bindingSource);

/*ValueOverloadsSetterGenerator*/

public static T HorizontalOrientationPadding<T>(this T control, System.Double uniformLength = default) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
   => control._set(() => control.HorizontalOrientationPadding = new Avalonia.Thickness(uniformLength));
public static T HorizontalOrientationPadding<T>(this T control, System.Double horizontal = default, System.Double vertical = default) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
   => control._set(() => control.HorizontalOrientationPadding = new Avalonia.Thickness(horizontal, vertical));
public static T HorizontalOrientationPadding<T>(this T control, System.Double left = default, System.Double top = default, System.Double right = default, System.Double bottom = default) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
   => control._set(() => control.HorizontalOrientationPadding = new Avalonia.Thickness(left, top, right, bottom));

/*BindSetterGenerator*/
public static T HorizontalOrientationPadding<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
   => control._set(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.HorizontalOrientationPaddingProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T HorizontalOrientationPadding<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
   => control._set(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.HorizontalOrientationPaddingProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T HorizontalOrientationPadding<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Thickness> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
=> control._setEx(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.HorizontalOrientationPaddingProperty, ps, () => control.HorizontalOrientationPadding = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // VerticalOrientationPaddingProperty

/*BindFromExpressionSetterGenerator*/
public static T VerticalOrientationPadding<T>(this T control, Func<Avalonia.Thickness> func, Action<Avalonia.Thickness>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
   => control._set(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.VerticalOrientationPaddingProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T VerticalOrientationPadding<T>(this T control, Avalonia.Thickness value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
=> control._setEx(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.VerticalOrientationPaddingProperty, ps, () => control.VerticalOrientationPadding = value, bindingMode, converter, bindingSource);

/*ValueOverloadsSetterGenerator*/

public static T VerticalOrientationPadding<T>(this T control, System.Double uniformLength = default) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
   => control._set(() => control.VerticalOrientationPadding = new Avalonia.Thickness(uniformLength));
public static T VerticalOrientationPadding<T>(this T control, System.Double horizontal = default, System.Double vertical = default) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
   => control._set(() => control.VerticalOrientationPadding = new Avalonia.Thickness(horizontal, vertical));
public static T VerticalOrientationPadding<T>(this T control, System.Double left = default, System.Double top = default, System.Double right = default, System.Double bottom = default) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
   => control._set(() => control.VerticalOrientationPadding = new Avalonia.Thickness(left, top, right, bottom));

/*BindSetterGenerator*/
public static T VerticalOrientationPadding<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
   => control._set(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.VerticalOrientationPaddingProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T VerticalOrientationPadding<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
   => control._set(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.VerticalOrientationPaddingProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T VerticalOrientationPadding<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Thickness> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
=> control._setEx(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.VerticalOrientationPaddingProperty, ps, () => control.VerticalOrientationPadding = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // HorizontalOrientationPaddingProperty

/*ValueStyleSetterGenerator*/
public static Style<T> HorizontalOrientationPadding<T>(this Style<T> style, Avalonia.Thickness value) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
=> style._addSetter(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.HorizontalOrientationPaddingProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> HorizontalOrientationPadding<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
=> style._addSetter(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.HorizontalOrientationPaddingProperty, binding);

/*ValueOverloadsStyleSetterGenerator*/
public static Style<T> HorizontalOrientationPadding<T>(this Style<T> style, System.Double uniformLength) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
   => style._addSetter(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.HorizontalOrientationPaddingProperty, new Avalonia.Thickness(uniformLength));public static Style<T> HorizontalOrientationPadding<T>(this Style<T> style, System.Double horizontal, System.Double vertical) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
   => style._addSetter(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.HorizontalOrientationPaddingProperty, new Avalonia.Thickness(horizontal, vertical));public static Style<T> HorizontalOrientationPadding<T>(this Style<T> style, System.Double left, System.Double top, System.Double right, System.Double bottom) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
   => style._addSetter(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.HorizontalOrientationPaddingProperty, new Avalonia.Thickness(left, top, right, bottom));


 // VerticalOrientationPaddingProperty

/*ValueStyleSetterGenerator*/
public static Style<T> VerticalOrientationPadding<T>(this Style<T> style, Avalonia.Thickness value) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
=> style._addSetter(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.VerticalOrientationPaddingProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> VerticalOrientationPadding<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
=> style._addSetter(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.VerticalOrientationPaddingProperty, binding);

/*ValueOverloadsStyleSetterGenerator*/
public static Style<T> VerticalOrientationPadding<T>(this Style<T> style, System.Double uniformLength) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
   => style._addSetter(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.VerticalOrientationPaddingProperty, new Avalonia.Thickness(uniformLength));public static Style<T> VerticalOrientationPadding<T>(this Style<T> style, System.Double horizontal, System.Double vertical) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
   => style._addSetter(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.VerticalOrientationPaddingProperty, new Avalonia.Thickness(horizontal, vertical));public static Style<T> VerticalOrientationPadding<T>(this Style<T> style, System.Double left, System.Double top, System.Double right, System.Double bottom) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
   => style._addSetter(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.VerticalOrientationPaddingProperty, new Avalonia.Thickness(left, top, right, bottom));



}
