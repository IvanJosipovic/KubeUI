#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using NumberDisplayerBase = Ursa.Controls.NumberDisplayerBase;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class NumberDisplayerBaseExtensions
{
public static T InternalText<T>(this T control, IBinding binding) where T : Ursa.Controls.NumberDisplayerBase
   => control._set(Ursa.Controls.NumberDisplayerBase.InternalTextProperty, binding);
public static T InternalText<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumberDisplayerBase
   => control._set(Ursa.Controls.NumberDisplayerBase.InternalTextProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T InternalText<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumberDisplayerBase
   => control._set(Ursa.Controls.NumberDisplayerBase.InternalTextProperty, func, onChanged, expression);
public static T InternalText<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumberDisplayerBase
=> control._setEx(Ursa.Controls.NumberDisplayerBase.InternalTextProperty, ps, () => control.InternalText = value, bindingMode, converter, bindingSource);
public static T InternalText<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumberDisplayerBase
=> control._setEx(Ursa.Controls.NumberDisplayerBase.InternalTextProperty, ps, () => control.InternalText = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Duration<T>(this T control, IBinding binding) where T : Ursa.Controls.NumberDisplayerBase
   => control._set(Ursa.Controls.NumberDisplayerBase.DurationProperty, binding);
public static T Duration<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumberDisplayerBase
   => control._set(Ursa.Controls.NumberDisplayerBase.DurationProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Duration<T>(this T control, Func<System.TimeSpan> func, Action<System.TimeSpan>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumberDisplayerBase
   => control._set(Ursa.Controls.NumberDisplayerBase.DurationProperty, func, onChanged, expression);
public static T Duration<T>(this T control, System.TimeSpan value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumberDisplayerBase
=> control._setEx(Ursa.Controls.NumberDisplayerBase.DurationProperty, ps, () => control.Duration = value, bindingMode, converter, bindingSource);
public static T Duration<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.TimeSpan> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumberDisplayerBase
=> control._setEx(Ursa.Controls.NumberDisplayerBase.DurationProperty, ps, () => control.Duration = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T StringFormat<T>(this T control, IBinding binding) where T : Ursa.Controls.NumberDisplayerBase
   => control._set(Ursa.Controls.NumberDisplayerBase.StringFormatProperty, binding);
public static T StringFormat<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumberDisplayerBase
   => control._set(Ursa.Controls.NumberDisplayerBase.StringFormatProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T StringFormat<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumberDisplayerBase
   => control._set(Ursa.Controls.NumberDisplayerBase.StringFormatProperty, func, onChanged, expression);
public static T StringFormat<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumberDisplayerBase
=> control._setEx(Ursa.Controls.NumberDisplayerBase.StringFormatProperty, ps, () => control.StringFormat = value, bindingMode, converter, bindingSource);
public static T StringFormat<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumberDisplayerBase
=> control._setEx(Ursa.Controls.NumberDisplayerBase.StringFormatProperty, ps, () => control.StringFormat = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsSelectable<T>(this T control, IBinding binding) where T : Ursa.Controls.NumberDisplayerBase
   => control._set(Ursa.Controls.NumberDisplayerBase.IsSelectableProperty, binding);
public static T IsSelectable<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumberDisplayerBase
   => control._set(Ursa.Controls.NumberDisplayerBase.IsSelectableProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsSelectable<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumberDisplayerBase
   => control._set(Ursa.Controls.NumberDisplayerBase.IsSelectableProperty, func, onChanged, expression);
public static T IsSelectable<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumberDisplayerBase
=> control._setEx(Ursa.Controls.NumberDisplayerBase.IsSelectableProperty, ps, () => control.IsSelectable = value, bindingMode, converter, bindingSource);
public static T IsSelectable<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumberDisplayerBase
=> control._setEx(Ursa.Controls.NumberDisplayerBase.IsSelectableProperty, ps, () => control.IsSelectable = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

