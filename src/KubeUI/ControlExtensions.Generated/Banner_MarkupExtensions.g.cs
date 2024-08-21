#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class Banner_MarkupExtensions
{
//================= Properties ======================//
 // CanCloseProperty

/*BindFromExpressionSetterGenerator*/
public static T CanClose<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Banner
   => control._set(Ursa.Controls.Banner.CanCloseProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CanClose<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Banner
=> control._setEx(Ursa.Controls.Banner.CanCloseProperty, ps, () => control.CanClose = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CanClose<T>(this T control, IBinding binding) where T : Ursa.Controls.Banner
   => control._set(Ursa.Controls.Banner.CanCloseProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CanClose<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Banner
   => control._set(Ursa.Controls.Banner.CanCloseProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CanClose<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Banner
=> control._setEx(Ursa.Controls.Banner.CanCloseProperty, ps, () => control.CanClose = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ShowIconProperty

/*BindFromExpressionSetterGenerator*/
public static T ShowIcon<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Banner
   => control._set(Ursa.Controls.Banner.ShowIconProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ShowIcon<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Banner
=> control._setEx(Ursa.Controls.Banner.ShowIconProperty, ps, () => control.ShowIcon = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ShowIcon<T>(this T control, IBinding binding) where T : Ursa.Controls.Banner
   => control._set(Ursa.Controls.Banner.ShowIconProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ShowIcon<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Banner
   => control._set(Ursa.Controls.Banner.ShowIconProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ShowIcon<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Banner
=> control._setEx(Ursa.Controls.Banner.ShowIconProperty, ps, () => control.ShowIcon = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IconProperty

/*BindFromExpressionSetterGenerator*/
public static T Icon<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Banner
   => control._set(Ursa.Controls.Banner.IconProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Icon<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Banner
=> control._setEx(Ursa.Controls.Banner.IconProperty, ps, () => control.Icon = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Icon<T>(this T control, IBinding binding) where T : Ursa.Controls.Banner
   => control._set(Ursa.Controls.Banner.IconProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Icon<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Banner
   => control._set(Ursa.Controls.Banner.IconProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Icon<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Banner
=> control._setEx(Ursa.Controls.Banner.IconProperty, ps, () => control.Icon = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TypeProperty

/*BindFromExpressionSetterGenerator*/
public static T Type<T>(this T control, Func<Avalonia.Controls.Notifications.NotificationType> func, Action<Avalonia.Controls.Notifications.NotificationType>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Banner
   => control._set(Ursa.Controls.Banner.TypeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Type<T>(this T control, Avalonia.Controls.Notifications.NotificationType value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Banner
=> control._setEx(Ursa.Controls.Banner.TypeProperty, ps, () => control.Type = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Type<T>(this T control, IBinding binding) where T : Ursa.Controls.Banner
   => control._set(Ursa.Controls.Banner.TypeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Type<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Banner
   => control._set(Ursa.Controls.Banner.TypeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Type<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Notifications.NotificationType> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Banner
=> control._setEx(Ursa.Controls.Banner.TypeProperty, ps, () => control.Type = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // CanCloseProperty

/*ValueStyleSetterGenerator*/
public static Style<T> CanClose<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.Banner
=> style._addSetter(Ursa.Controls.Banner.CanCloseProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CanClose<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Banner
=> style._addSetter(Ursa.Controls.Banner.CanCloseProperty, binding);


 // ShowIconProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ShowIcon<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.Banner
=> style._addSetter(Ursa.Controls.Banner.ShowIconProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ShowIcon<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Banner
=> style._addSetter(Ursa.Controls.Banner.ShowIconProperty, binding);


 // IconProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Icon<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.Banner
=> style._addSetter(Ursa.Controls.Banner.IconProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Icon<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Banner
=> style._addSetter(Ursa.Controls.Banner.IconProperty, binding);


 // TypeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Type<T>(this Style<T> style, Avalonia.Controls.Notifications.NotificationType value) where T : Ursa.Controls.Banner
=> style._addSetter(Ursa.Controls.Banner.TypeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Type<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Banner
=> style._addSetter(Ursa.Controls.Banner.TypeProperty, binding);



}
