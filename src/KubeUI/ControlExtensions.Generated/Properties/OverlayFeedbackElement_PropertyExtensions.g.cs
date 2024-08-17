#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using OverlayFeedbackElement = Ursa.Controls.OverlayShared.OverlayFeedbackElement;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls.OverlayShared;

namespace Avalonia.Markup.Declarative;
public static partial class OverlayFeedbackElementExtensions
{
public static T IsClosed<T>(this T control, IBinding binding) where T : Ursa.Controls.OverlayShared.OverlayFeedbackElement
   => control._set(Ursa.Controls.OverlayShared.OverlayFeedbackElement.IsClosedProperty, binding);
public static T IsClosed<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.OverlayShared.OverlayFeedbackElement
   => control._set(Ursa.Controls.OverlayShared.OverlayFeedbackElement.IsClosedProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsClosed<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.OverlayShared.OverlayFeedbackElement
   => control._set(Ursa.Controls.OverlayShared.OverlayFeedbackElement.IsClosedProperty, func, onChanged, expression);
public static T IsClosed<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.OverlayShared.OverlayFeedbackElement
=> control._setEx(Ursa.Controls.OverlayShared.OverlayFeedbackElement.IsClosedProperty, ps, () => control.IsClosed = value, bindingMode, converter, bindingSource);
public static T IsClosed<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.OverlayShared.OverlayFeedbackElement
=> control._setEx(Ursa.Controls.OverlayShared.OverlayFeedbackElement.IsClosedProperty, ps, () => control.IsClosed = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

