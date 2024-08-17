#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Layout;
using KeyGestureInput = Ursa.Controls.KeyGestureInput;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class KeyGestureInputExtensions
{
public static T Gesture<T>(this T control, IBinding binding) where T : Ursa.Controls.KeyGestureInput
   => control._set(Ursa.Controls.KeyGestureInput.GestureProperty, binding);
public static T Gesture<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.KeyGestureInput
   => control._set(Ursa.Controls.KeyGestureInput.GestureProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Gesture<T>(this T control, Func<Avalonia.Input.KeyGesture> func, Action<Avalonia.Input.KeyGesture>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.KeyGestureInput
   => control._set(Ursa.Controls.KeyGestureInput.GestureProperty, func, onChanged, expression);
public static T Gesture<T>(this T control, Avalonia.Input.KeyGesture value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.KeyGestureInput
=> control._setEx(Ursa.Controls.KeyGestureInput.GestureProperty, ps, () => control.Gesture = value, bindingMode, converter, bindingSource);
public static T Gesture<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Input.KeyGesture> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.KeyGestureInput
=> control._setEx(Ursa.Controls.KeyGestureInput.GestureProperty, ps, () => control.Gesture = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T AcceptableKeys<T>(this T control, IBinding binding) where T : Ursa.Controls.KeyGestureInput
   => control._set(Ursa.Controls.KeyGestureInput.AcceptableKeysProperty, binding);
public static T AcceptableKeys<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.KeyGestureInput
   => control._set(Ursa.Controls.KeyGestureInput.AcceptableKeysProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T AcceptableKeys<T>(this T control, Func<System.Collections.Generic.IList<Avalonia.Input.Key>> func, Action<System.Collections.Generic.IList<Avalonia.Input.Key>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.KeyGestureInput
   => control._set(Ursa.Controls.KeyGestureInput.AcceptableKeysProperty, func, onChanged, expression);
public static T AcceptableKeys<T>(this T control, System.Collections.Generic.IList<Avalonia.Input.Key> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.KeyGestureInput
=> control._setEx(Ursa.Controls.KeyGestureInput.AcceptableKeysProperty, ps, () => control.AcceptableKeys = value, bindingMode, converter, bindingSource);
public static T AcceptableKeys<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.Generic.IList<Avalonia.Input.Key>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.KeyGestureInput
=> control._setEx(Ursa.Controls.KeyGestureInput.AcceptableKeysProperty, ps, () => control.AcceptableKeys = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ConsiderKeyModifiers<T>(this T control, IBinding binding) where T : Ursa.Controls.KeyGestureInput
   => control._set(Ursa.Controls.KeyGestureInput.ConsiderKeyModifiersProperty, binding);
public static T ConsiderKeyModifiers<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.KeyGestureInput
   => control._set(Ursa.Controls.KeyGestureInput.ConsiderKeyModifiersProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ConsiderKeyModifiers<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.KeyGestureInput
   => control._set(Ursa.Controls.KeyGestureInput.ConsiderKeyModifiersProperty, func, onChanged, expression);
public static T ConsiderKeyModifiers<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.KeyGestureInput
=> control._setEx(Ursa.Controls.KeyGestureInput.ConsiderKeyModifiersProperty, ps, () => control.ConsiderKeyModifiers = value, bindingMode, converter, bindingSource);
public static T ConsiderKeyModifiers<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.KeyGestureInput
=> control._setEx(Ursa.Controls.KeyGestureInput.ConsiderKeyModifiersProperty, ps, () => control.ConsiderKeyModifiers = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T HorizontalContentAlignment<T>(this T control, IBinding binding) where T : Ursa.Controls.KeyGestureInput
   => control._set(Ursa.Controls.KeyGestureInput.HorizontalContentAlignmentProperty, binding);
public static T HorizontalContentAlignment<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.KeyGestureInput
   => control._set(Ursa.Controls.KeyGestureInput.HorizontalContentAlignmentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T HorizontalContentAlignment<T>(this T control, Func<Avalonia.Layout.HorizontalAlignment> func, Action<Avalonia.Layout.HorizontalAlignment>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.KeyGestureInput
   => control._set(Ursa.Controls.KeyGestureInput.HorizontalContentAlignmentProperty, func, onChanged, expression);
public static T HorizontalContentAlignment<T>(this T control, Avalonia.Layout.HorizontalAlignment value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.KeyGestureInput
=> control._setEx(Ursa.Controls.KeyGestureInput.HorizontalContentAlignmentProperty, ps, () => control.HorizontalContentAlignment = value, bindingMode, converter, bindingSource);
public static T HorizontalContentAlignment<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Layout.HorizontalAlignment> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.KeyGestureInput
=> control._setEx(Ursa.Controls.KeyGestureInput.HorizontalContentAlignmentProperty, ps, () => control.HorizontalContentAlignment = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T VerticalContentAlignment<T>(this T control, IBinding binding) where T : Ursa.Controls.KeyGestureInput
   => control._set(Ursa.Controls.KeyGestureInput.VerticalContentAlignmentProperty, binding);
public static T VerticalContentAlignment<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.KeyGestureInput
   => control._set(Ursa.Controls.KeyGestureInput.VerticalContentAlignmentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T VerticalContentAlignment<T>(this T control, Func<Avalonia.Layout.VerticalAlignment> func, Action<Avalonia.Layout.VerticalAlignment>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.KeyGestureInput
   => control._set(Ursa.Controls.KeyGestureInput.VerticalContentAlignmentProperty, func, onChanged, expression);
public static T VerticalContentAlignment<T>(this T control, Avalonia.Layout.VerticalAlignment value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.KeyGestureInput
=> control._setEx(Ursa.Controls.KeyGestureInput.VerticalContentAlignmentProperty, ps, () => control.VerticalContentAlignment = value, bindingMode, converter, bindingSource);
public static T VerticalContentAlignment<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Layout.VerticalAlignment> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.KeyGestureInput
=> control._setEx(Ursa.Controls.KeyGestureInput.VerticalContentAlignmentProperty, ps, () => control.VerticalContentAlignment = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T InnerLeftContent<T>(this T control, IBinding binding) where T : Ursa.Controls.KeyGestureInput
   => control._set(Ursa.Controls.KeyGestureInput.InnerLeftContentProperty, binding);
public static T InnerLeftContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.KeyGestureInput
   => control._set(Ursa.Controls.KeyGestureInput.InnerLeftContentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T InnerLeftContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.KeyGestureInput
   => control._set(Ursa.Controls.KeyGestureInput.InnerLeftContentProperty, func, onChanged, expression);
public static T InnerLeftContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.KeyGestureInput
=> control._setEx(Ursa.Controls.KeyGestureInput.InnerLeftContentProperty, ps, () => control.InnerLeftContent = value, bindingMode, converter, bindingSource);
public static T InnerLeftContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.KeyGestureInput
=> control._setEx(Ursa.Controls.KeyGestureInput.InnerLeftContentProperty, ps, () => control.InnerLeftContent = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T InnerRightContent<T>(this T control, IBinding binding) where T : Ursa.Controls.KeyGestureInput
   => control._set(Ursa.Controls.KeyGestureInput.InnerRightContentProperty, binding);
public static T InnerRightContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.KeyGestureInput
   => control._set(Ursa.Controls.KeyGestureInput.InnerRightContentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T InnerRightContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.KeyGestureInput
   => control._set(Ursa.Controls.KeyGestureInput.InnerRightContentProperty, func, onChanged, expression);
public static T InnerRightContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.KeyGestureInput
=> control._setEx(Ursa.Controls.KeyGestureInput.InnerRightContentProperty, ps, () => control.InnerRightContent = value, bindingMode, converter, bindingSource);
public static T InnerRightContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.KeyGestureInput
=> control._setEx(Ursa.Controls.KeyGestureInput.InnerRightContentProperty, ps, () => control.InnerRightContent = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

