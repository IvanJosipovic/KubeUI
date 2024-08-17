#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using FAIconElement = FluentAvalonia.UI.Controls.FAIconElement;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class FAIconElementExtensions
{
public static T Foreground<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAIconElement
   => control._set(FluentAvalonia.UI.Controls.FAIconElement.ForegroundProperty, binding);
public static T Foreground<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAIconElement
   => control._set(FluentAvalonia.UI.Controls.FAIconElement.ForegroundProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Foreground<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAIconElement
   => control._set(FluentAvalonia.UI.Controls.FAIconElement.ForegroundProperty, func, onChanged, expression);
public static T Foreground<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAIconElement
=> control._setEx(FluentAvalonia.UI.Controls.FAIconElement.ForegroundProperty, ps, () => control.Foreground = value, bindingMode, converter, bindingSource);
public static T Foreground<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAIconElement
=> control._setEx(FluentAvalonia.UI.Controls.FAIconElement.ForegroundProperty, ps, () => control.Foreground = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

