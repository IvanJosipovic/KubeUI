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
public static partial class WindowNotificationManager_MarkupExtensions
{
//================= Properties ======================//
 // PositionProperty

/*BindFromExpressionSetterGenerator*/
public static T Position<T>(this T control, Func<Avalonia.Controls.Notifications.NotificationPosition> func, Action<Avalonia.Controls.Notifications.NotificationPosition>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.WindowNotificationManager
   => control._set(Ursa.Controls.WindowNotificationManager.PositionProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Position<T>(this T control, Avalonia.Controls.Notifications.NotificationPosition value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.WindowNotificationManager
=> control._setEx(Ursa.Controls.WindowNotificationManager.PositionProperty, ps, () => control.Position = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Position<T>(this T control, IBinding binding) where T : Ursa.Controls.WindowNotificationManager
   => control._set(Ursa.Controls.WindowNotificationManager.PositionProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Position<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.WindowNotificationManager
   => control._set(Ursa.Controls.WindowNotificationManager.PositionProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Position<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Notifications.NotificationPosition> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.WindowNotificationManager
=> control._setEx(Ursa.Controls.WindowNotificationManager.PositionProperty, ps, () => control.Position = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // PositionProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Position<T>(this Style<T> style, Avalonia.Controls.Notifications.NotificationPosition value) where T : Ursa.Controls.WindowNotificationManager
=> style._addSetter(Ursa.Controls.WindowNotificationManager.PositionProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Position<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.WindowNotificationManager
=> style._addSetter(Ursa.Controls.WindowNotificationManager.PositionProperty, binding);



}
