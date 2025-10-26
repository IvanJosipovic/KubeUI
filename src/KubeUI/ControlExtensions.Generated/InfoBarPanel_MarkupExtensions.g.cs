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
public static partial class InfoBarPanel_MarkupExtensions
{
//================= Properties ======================//
 // HorizontalOrientationPadding

/*BindFromExpressionSetterGenerator*/
public static T HorizontalOrientationPadding<T>(this T control, Func<Avalonia.Thickness> func, Action<Avalonia.Thickness>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel 
   => control._set(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.HorizontalOrientationPaddingProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T HorizontalOrientationPadding<T>(this T control,Avalonia.Thickness value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel 
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
public static T HorizontalOrientationPadding<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Thickness> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel 
=> control._setEx(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.HorizontalOrientationPaddingProperty, ps, () => control.HorizontalOrientationPadding = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // VerticalOrientationPadding

/*BindFromExpressionSetterGenerator*/
public static T VerticalOrientationPadding<T>(this T control, Func<Avalonia.Thickness> func, Action<Avalonia.Thickness>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel 
   => control._set(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.VerticalOrientationPaddingProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T VerticalOrientationPadding<T>(this T control,Avalonia.Thickness value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel 
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
public static T VerticalOrientationPadding<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Thickness> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel 
=> control._setEx(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.VerticalOrientationPaddingProperty, ps, () => control.VerticalOrientationPadding = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Attached Properties ======================//
 // HorizontalOrientationMargin

/*AttachedPropertyMagicalSetterGenerator*/
public static T InfoBarPanel_HorizontalOrientationMargin<T>(this T control, Avalonia.Thickness value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.Control
 => control._setEx(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.HorizontalOrientationMarginProperty, ps, () => FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.SetHorizontalOrientationMargin(control, value), bindingMode, converter, bindingSource);

/*AttachedPropertyBindFromExpressionSetterGenerator*/
public static T InfoBarPanel_HorizontalOrientationMargin<T>(this T control, Func<Avalonia.Thickness> func, Action<Avalonia.Thickness>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.Control 
   => control._set(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.HorizontalOrientationMarginProperty, func, onChanged, expression);


 // VerticalOrientationMargin

/*AttachedPropertyMagicalSetterGenerator*/
public static T InfoBarPanel_VerticalOrientationMargin<T>(this T control, Avalonia.Thickness value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.Control
 => control._setEx(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.VerticalOrientationMarginProperty, ps, () => FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.SetVerticalOrientationMargin(control, value), bindingMode, converter, bindingSource);

/*AttachedPropertyBindFromExpressionSetterGenerator*/
public static T InfoBarPanel_VerticalOrientationMargin<T>(this T control, Func<Avalonia.Thickness> func, Action<Avalonia.Thickness>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.Control 
   => control._set(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.VerticalOrientationMarginProperty, func, onChanged, expression);



//================= Styles ======================//
 // HorizontalOrientationPadding

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


 // VerticalOrientationPadding

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
