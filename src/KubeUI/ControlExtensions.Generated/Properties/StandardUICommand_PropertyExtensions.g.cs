#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Input;
using StandardUICommand = FluentAvalonia.UI.Input.StandardUICommand;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class StandardUICommandExtensions
{
public static T Kind<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Input.StandardUICommand
   => control._set(FluentAvalonia.UI.Input.StandardUICommand.KindProperty, binding);
public static T Kind<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Input.StandardUICommand
   => control._set(FluentAvalonia.UI.Input.StandardUICommand.KindProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Kind<T>(this T control, Func<FluentAvalonia.UI.Input.StandardUICommandKind> func, Action<FluentAvalonia.UI.Input.StandardUICommandKind>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Input.StandardUICommand
   => control._set(FluentAvalonia.UI.Input.StandardUICommand.KindProperty, func, onChanged, expression);
public static T Kind<T>(this T control, FluentAvalonia.UI.Input.StandardUICommandKind value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Input.StandardUICommand
=> control._setEx(FluentAvalonia.UI.Input.StandardUICommand.KindProperty, ps, () => control.Kind = value, bindingMode, converter, bindingSource);
public static T Kind<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Input.StandardUICommandKind> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Input.StandardUICommand
=> control._setEx(FluentAvalonia.UI.Input.StandardUICommand.KindProperty, ps, () => control.Kind = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

