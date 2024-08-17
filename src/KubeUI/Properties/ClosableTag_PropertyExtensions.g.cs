#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using ClosableTag = Ursa.Controls.ClosableTag;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class ClosableTagExtensions
{
public static T Command<T>(this T control, IBinding binding) where T : Ursa.Controls.ClosableTag
   => control._set(Ursa.Controls.ClosableTag.CommandProperty, binding);
public static T Command<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.ClosableTag
   => control._set(Ursa.Controls.ClosableTag.CommandProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Command<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.ClosableTag
   => control._set(Ursa.Controls.ClosableTag.CommandProperty, func, onChanged, expression);
public static T Command<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ClosableTag
=> control._setEx(Ursa.Controls.ClosableTag.CommandProperty, ps, () => control.Command = value, bindingMode, converter, bindingSource);
public static T Command<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.ClosableTag
=> control._setEx(Ursa.Controls.ClosableTag.CommandProperty, ps, () => control.Command = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

