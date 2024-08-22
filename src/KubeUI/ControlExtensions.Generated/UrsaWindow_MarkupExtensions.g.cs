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
public static partial class UrsaWindow_MarkupExtensions
{
//================= Properties ======================//
 // IsFullScreenButtonVisibleProperty

/*BindFromExpressionSetterGenerator*/
public static T IsFullScreenButtonVisible<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.IsFullScreenButtonVisibleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsFullScreenButtonVisible<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaWindow
=> control._setEx(Ursa.Controls.UrsaWindow.IsFullScreenButtonVisibleProperty, ps, () => control.IsFullScreenButtonVisible = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsFullScreenButtonVisible<T>(this T control, IBinding binding) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.IsFullScreenButtonVisibleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsFullScreenButtonVisible<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.IsFullScreenButtonVisibleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsFullScreenButtonVisible<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaWindow
=> control._setEx(Ursa.Controls.UrsaWindow.IsFullScreenButtonVisibleProperty, ps, () => control.IsFullScreenButtonVisible = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsMinimizeButtonVisibleProperty

/*BindFromExpressionSetterGenerator*/
public static T IsMinimizeButtonVisible<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.IsMinimizeButtonVisibleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsMinimizeButtonVisible<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaWindow
=> control._setEx(Ursa.Controls.UrsaWindow.IsMinimizeButtonVisibleProperty, ps, () => control.IsMinimizeButtonVisible = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsMinimizeButtonVisible<T>(this T control, IBinding binding) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.IsMinimizeButtonVisibleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsMinimizeButtonVisible<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.IsMinimizeButtonVisibleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsMinimizeButtonVisible<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaWindow
=> control._setEx(Ursa.Controls.UrsaWindow.IsMinimizeButtonVisibleProperty, ps, () => control.IsMinimizeButtonVisible = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsRestoreButtonVisibleProperty

/*BindFromExpressionSetterGenerator*/
public static T IsRestoreButtonVisible<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.IsRestoreButtonVisibleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsRestoreButtonVisible<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaWindow
=> control._setEx(Ursa.Controls.UrsaWindow.IsRestoreButtonVisibleProperty, ps, () => control.IsRestoreButtonVisible = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsRestoreButtonVisible<T>(this T control, IBinding binding) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.IsRestoreButtonVisibleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsRestoreButtonVisible<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.IsRestoreButtonVisibleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsRestoreButtonVisible<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaWindow
=> control._setEx(Ursa.Controls.UrsaWindow.IsRestoreButtonVisibleProperty, ps, () => control.IsRestoreButtonVisible = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsCloseButtonVisibleProperty

/*BindFromExpressionSetterGenerator*/
public static T IsCloseButtonVisible<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.IsCloseButtonVisibleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsCloseButtonVisible<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaWindow
=> control._setEx(Ursa.Controls.UrsaWindow.IsCloseButtonVisibleProperty, ps, () => control.IsCloseButtonVisible = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsCloseButtonVisible<T>(this T control, IBinding binding) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.IsCloseButtonVisibleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsCloseButtonVisible<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.IsCloseButtonVisibleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsCloseButtonVisible<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaWindow
=> control._setEx(Ursa.Controls.UrsaWindow.IsCloseButtonVisibleProperty, ps, () => control.IsCloseButtonVisible = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsTitleBarVisibleProperty

/*BindFromExpressionSetterGenerator*/
public static T IsTitleBarVisible<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.IsTitleBarVisibleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsTitleBarVisible<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaWindow
=> control._setEx(Ursa.Controls.UrsaWindow.IsTitleBarVisibleProperty, ps, () => control.IsTitleBarVisible = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsTitleBarVisible<T>(this T control, IBinding binding) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.IsTitleBarVisibleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsTitleBarVisible<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.IsTitleBarVisibleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsTitleBarVisible<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaWindow
=> control._setEx(Ursa.Controls.UrsaWindow.IsTitleBarVisibleProperty, ps, () => control.IsTitleBarVisible = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TitleBarContentProperty

/*BindFromExpressionSetterGenerator*/
public static T TitleBarContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.TitleBarContentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T TitleBarContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaWindow
=> control._setEx(Ursa.Controls.UrsaWindow.TitleBarContentProperty, ps, () => control.TitleBarContent = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TitleBarContent<T>(this T control, IBinding binding) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.TitleBarContentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TitleBarContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.TitleBarContentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T TitleBarContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaWindow
=> control._setEx(Ursa.Controls.UrsaWindow.TitleBarContentProperty, ps, () => control.TitleBarContent = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // LeftContentProperty

/*BindFromExpressionSetterGenerator*/
public static T LeftContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.LeftContentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T LeftContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaWindow
=> control._setEx(Ursa.Controls.UrsaWindow.LeftContentProperty, ps, () => control.LeftContent = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T LeftContent<T>(this T control, IBinding binding) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.LeftContentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T LeftContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.LeftContentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T LeftContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaWindow
=> control._setEx(Ursa.Controls.UrsaWindow.LeftContentProperty, ps, () => control.LeftContent = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // RightContentProperty

/*BindFromExpressionSetterGenerator*/
public static T RightContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.RightContentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T RightContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaWindow
=> control._setEx(Ursa.Controls.UrsaWindow.RightContentProperty, ps, () => control.RightContent = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T RightContent<T>(this T control, IBinding binding) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.RightContentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T RightContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.RightContentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T RightContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaWindow
=> control._setEx(Ursa.Controls.UrsaWindow.RightContentProperty, ps, () => control.RightContent = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TitleBarMarginProperty

/*BindFromExpressionSetterGenerator*/
public static T TitleBarMargin<T>(this T control, Func<Avalonia.Thickness> func, Action<Avalonia.Thickness>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.TitleBarMarginProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T TitleBarMargin<T>(this T control, Avalonia.Thickness value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaWindow
=> control._setEx(Ursa.Controls.UrsaWindow.TitleBarMarginProperty, ps, () => control.TitleBarMargin = value, bindingMode, converter, bindingSource);

/*ValueOverloadsSetterGenerator*/

public static T TitleBarMargin<T>(this T control, System.Double uniformLength = default) where T : Ursa.Controls.UrsaWindow
   => control._set(() => control.TitleBarMargin = new Avalonia.Thickness(uniformLength));
public static T TitleBarMargin<T>(this T control, System.Double horizontal = default, System.Double vertical = default) where T : Ursa.Controls.UrsaWindow
   => control._set(() => control.TitleBarMargin = new Avalonia.Thickness(horizontal, vertical));
public static T TitleBarMargin<T>(this T control, System.Double left = default, System.Double top = default, System.Double right = default, System.Double bottom = default) where T : Ursa.Controls.UrsaWindow
   => control._set(() => control.TitleBarMargin = new Avalonia.Thickness(left, top, right, bottom));

/*BindSetterGenerator*/
public static T TitleBarMargin<T>(this T control, IBinding binding) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.TitleBarMarginProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TitleBarMargin<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.UrsaWindow
   => control._set(Ursa.Controls.UrsaWindow.TitleBarMarginProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T TitleBarMargin<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Thickness> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.UrsaWindow
=> control._setEx(Ursa.Controls.UrsaWindow.TitleBarMarginProperty, ps, () => control.TitleBarMargin = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // IsFullScreenButtonVisibleProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsFullScreenButtonVisible<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.UrsaWindow
=> style._addSetter(Ursa.Controls.UrsaWindow.IsFullScreenButtonVisibleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsFullScreenButtonVisible<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.UrsaWindow
=> style._addSetter(Ursa.Controls.UrsaWindow.IsFullScreenButtonVisibleProperty, binding);


 // IsMinimizeButtonVisibleProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsMinimizeButtonVisible<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.UrsaWindow
=> style._addSetter(Ursa.Controls.UrsaWindow.IsMinimizeButtonVisibleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsMinimizeButtonVisible<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.UrsaWindow
=> style._addSetter(Ursa.Controls.UrsaWindow.IsMinimizeButtonVisibleProperty, binding);


 // IsRestoreButtonVisibleProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsRestoreButtonVisible<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.UrsaWindow
=> style._addSetter(Ursa.Controls.UrsaWindow.IsRestoreButtonVisibleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsRestoreButtonVisible<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.UrsaWindow
=> style._addSetter(Ursa.Controls.UrsaWindow.IsRestoreButtonVisibleProperty, binding);


 // IsCloseButtonVisibleProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsCloseButtonVisible<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.UrsaWindow
=> style._addSetter(Ursa.Controls.UrsaWindow.IsCloseButtonVisibleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsCloseButtonVisible<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.UrsaWindow
=> style._addSetter(Ursa.Controls.UrsaWindow.IsCloseButtonVisibleProperty, binding);


 // IsTitleBarVisibleProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsTitleBarVisible<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.UrsaWindow
=> style._addSetter(Ursa.Controls.UrsaWindow.IsTitleBarVisibleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsTitleBarVisible<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.UrsaWindow
=> style._addSetter(Ursa.Controls.UrsaWindow.IsTitleBarVisibleProperty, binding);


 // TitleBarContentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> TitleBarContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.UrsaWindow
=> style._addSetter(Ursa.Controls.UrsaWindow.TitleBarContentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> TitleBarContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.UrsaWindow
=> style._addSetter(Ursa.Controls.UrsaWindow.TitleBarContentProperty, binding);


 // LeftContentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> LeftContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.UrsaWindow
=> style._addSetter(Ursa.Controls.UrsaWindow.LeftContentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> LeftContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.UrsaWindow
=> style._addSetter(Ursa.Controls.UrsaWindow.LeftContentProperty, binding);


 // RightContentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> RightContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.UrsaWindow
=> style._addSetter(Ursa.Controls.UrsaWindow.RightContentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> RightContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.UrsaWindow
=> style._addSetter(Ursa.Controls.UrsaWindow.RightContentProperty, binding);


 // TitleBarMarginProperty

/*ValueStyleSetterGenerator*/
public static Style<T> TitleBarMargin<T>(this Style<T> style, Avalonia.Thickness value) where T : Ursa.Controls.UrsaWindow
=> style._addSetter(Ursa.Controls.UrsaWindow.TitleBarMarginProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> TitleBarMargin<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.UrsaWindow
=> style._addSetter(Ursa.Controls.UrsaWindow.TitleBarMarginProperty, binding);

/*ValueOverloadsStyleSetterGenerator*/
public static Style<T> TitleBarMargin<T>(this Style<T> style, System.Double uniformLength) where T : Ursa.Controls.UrsaWindow
   => style._addSetter(Ursa.Controls.UrsaWindow.TitleBarMarginProperty, new Avalonia.Thickness(uniformLength));public static Style<T> TitleBarMargin<T>(this Style<T> style, System.Double horizontal, System.Double vertical) where T : Ursa.Controls.UrsaWindow
   => style._addSetter(Ursa.Controls.UrsaWindow.TitleBarMarginProperty, new Avalonia.Thickness(horizontal, vertical));public static Style<T> TitleBarMargin<T>(this Style<T> style, System.Double left, System.Double top, System.Double right, System.Double bottom) where T : Ursa.Controls.UrsaWindow
   => style._addSetter(Ursa.Controls.UrsaWindow.TitleBarMarginProperty, new Avalonia.Thickness(left, top, right, bottom));



}
