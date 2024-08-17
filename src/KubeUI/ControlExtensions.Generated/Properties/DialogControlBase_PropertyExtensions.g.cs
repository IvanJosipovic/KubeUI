#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using DialogControlBase = Ursa.Controls.DialogControlBase;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class DialogControlBaseExtensions
{
public static T IsFullScreen<T>(this T control, IBinding binding) where T : Ursa.Controls.DialogControlBase
   => control._set(Ursa.Controls.DialogControlBase.IsFullScreenProperty, binding);
public static T IsFullScreen<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DialogControlBase
   => control._set(Ursa.Controls.DialogControlBase.IsFullScreenProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsFullScreen<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DialogControlBase
   => control._set(Ursa.Controls.DialogControlBase.IsFullScreenProperty, func, onChanged, expression);
public static T IsFullScreen<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DialogControlBase
=> control._setEx(Ursa.Controls.DialogControlBase.IsFullScreenProperty, ps, () => control.IsFullScreen = value, bindingMode, converter, bindingSource);
public static T IsFullScreen<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DialogControlBase
=> control._setEx(Ursa.Controls.DialogControlBase.IsFullScreenProperty, ps, () => control.IsFullScreen = converter.TryConvert(value), bindingMode, converter, bindingSource);
}
