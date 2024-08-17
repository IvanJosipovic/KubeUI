#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TimePickerPresenter = Ursa.Controls.TimePickerPresenter;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class TimePickerPresenterExtensions
{
public static T NeedsConfirmation<T>(this T control, IBinding binding) where T : Ursa.Controls.TimePickerPresenter
   => control._set(Ursa.Controls.TimePickerPresenter.NeedsConfirmationProperty, binding);
public static T NeedsConfirmation<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimePickerPresenter
   => control._set(Ursa.Controls.TimePickerPresenter.NeedsConfirmationProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T NeedsConfirmation<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimePickerPresenter
   => control._set(Ursa.Controls.TimePickerPresenter.NeedsConfirmationProperty, func, onChanged, expression);
public static T NeedsConfirmation<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerPresenter
=> control._setEx(Ursa.Controls.TimePickerPresenter.NeedsConfirmationProperty, ps, () => control.NeedsConfirmation = value, bindingMode, converter, bindingSource);
public static T NeedsConfirmation<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerPresenter
=> control._setEx(Ursa.Controls.TimePickerPresenter.NeedsConfirmationProperty, ps, () => control.NeedsConfirmation = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T MinuteIncrement<T>(this T control, IBinding binding) where T : Ursa.Controls.TimePickerPresenter
   => control._set(Ursa.Controls.TimePickerPresenter.MinuteIncrementProperty, binding);
public static T MinuteIncrement<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimePickerPresenter
   => control._set(Ursa.Controls.TimePickerPresenter.MinuteIncrementProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T MinuteIncrement<T>(this T control, Func<System.Int32> func, Action<System.Int32>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimePickerPresenter
   => control._set(Ursa.Controls.TimePickerPresenter.MinuteIncrementProperty, func, onChanged, expression);
public static T MinuteIncrement<T>(this T control, System.Int32 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerPresenter
=> control._setEx(Ursa.Controls.TimePickerPresenter.MinuteIncrementProperty, ps, () => control.MinuteIncrement = value, bindingMode, converter, bindingSource);
public static T MinuteIncrement<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Int32> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerPresenter
=> control._setEx(Ursa.Controls.TimePickerPresenter.MinuteIncrementProperty, ps, () => control.MinuteIncrement = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SecondIncrement<T>(this T control, IBinding binding) where T : Ursa.Controls.TimePickerPresenter
   => control._set(Ursa.Controls.TimePickerPresenter.SecondIncrementProperty, binding);
public static T SecondIncrement<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimePickerPresenter
   => control._set(Ursa.Controls.TimePickerPresenter.SecondIncrementProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SecondIncrement<T>(this T control, Func<System.Int32> func, Action<System.Int32>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimePickerPresenter
   => control._set(Ursa.Controls.TimePickerPresenter.SecondIncrementProperty, func, onChanged, expression);
public static T SecondIncrement<T>(this T control, System.Int32 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerPresenter
=> control._setEx(Ursa.Controls.TimePickerPresenter.SecondIncrementProperty, ps, () => control.SecondIncrement = value, bindingMode, converter, bindingSource);
public static T SecondIncrement<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Int32> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerPresenter
=> control._setEx(Ursa.Controls.TimePickerPresenter.SecondIncrementProperty, ps, () => control.SecondIncrement = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Time<T>(this T control, IBinding binding) where T : Ursa.Controls.TimePickerPresenter
   => control._set(Ursa.Controls.TimePickerPresenter.TimeProperty, binding);
public static T Time<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimePickerPresenter
   => control._set(Ursa.Controls.TimePickerPresenter.TimeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Time<T>(this T control, Func<System.Nullable<System.TimeSpan>> func, Action<System.Nullable<System.TimeSpan>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimePickerPresenter
   => control._set(Ursa.Controls.TimePickerPresenter.TimeProperty, func, onChanged, expression);
public static T Time<T>(this T control, System.Nullable<System.TimeSpan> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerPresenter
=> control._setEx(Ursa.Controls.TimePickerPresenter.TimeProperty, ps, () => control.Time = value, bindingMode, converter, bindingSource);
public static T Time<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Nullable<System.TimeSpan>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerPresenter
=> control._setEx(Ursa.Controls.TimePickerPresenter.TimeProperty, ps, () => control.Time = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T PanelFormat<T>(this T control, IBinding binding) where T : Ursa.Controls.TimePickerPresenter
   => control._set(Ursa.Controls.TimePickerPresenter.PanelFormatProperty, binding);
public static T PanelFormat<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimePickerPresenter
   => control._set(Ursa.Controls.TimePickerPresenter.PanelFormatProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T PanelFormat<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimePickerPresenter
   => control._set(Ursa.Controls.TimePickerPresenter.PanelFormatProperty, func, onChanged, expression);
public static T PanelFormat<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerPresenter
=> control._setEx(Ursa.Controls.TimePickerPresenter.PanelFormatProperty, ps, () => control.PanelFormat = value, bindingMode, converter, bindingSource);
public static T PanelFormat<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerPresenter
=> control._setEx(Ursa.Controls.TimePickerPresenter.PanelFormatProperty, ps, () => control.PanelFormat = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

