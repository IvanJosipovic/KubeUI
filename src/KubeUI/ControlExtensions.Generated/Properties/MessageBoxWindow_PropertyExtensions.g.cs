#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using MessageBoxWindow = Ursa.Controls.MessageBoxWindow;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class MessageBoxWindowExtensions
{
public static T MessageIcon<T>(this T control, IBinding binding) where T : Ursa.Controls.MessageBoxWindow
   => control._set(Ursa.Controls.MessageBoxWindow.MessageIconProperty, binding);
public static T MessageIcon<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.MessageBoxWindow
   => control._set(Ursa.Controls.MessageBoxWindow.MessageIconProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T MessageIcon<T>(this T control, Func<Ursa.Controls.MessageBoxIcon> func, Action<Ursa.Controls.MessageBoxIcon>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.MessageBoxWindow
   => control._set(Ursa.Controls.MessageBoxWindow.MessageIconProperty, func, onChanged, expression);
public static T MessageIcon<T>(this T control, Ursa.Controls.MessageBoxIcon value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MessageBoxWindow
=> control._setEx(Ursa.Controls.MessageBoxWindow.MessageIconProperty, ps, () => control.MessageIcon = value, bindingMode, converter, bindingSource);
public static T MessageIcon<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Ursa.Controls.MessageBoxIcon> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MessageBoxWindow
=> control._setEx(Ursa.Controls.MessageBoxWindow.MessageIconProperty, ps, () => control.MessageIcon = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

