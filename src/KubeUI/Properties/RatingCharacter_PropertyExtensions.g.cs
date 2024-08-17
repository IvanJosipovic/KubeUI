#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using RatingCharacter = Ursa.Controls.RatingCharacter;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class RatingCharacterExtensions
{
public static T AllowHalf<T>(this T control, IBinding binding) where T : Ursa.Controls.RatingCharacter
   => control._set(Ursa.Controls.RatingCharacter.AllowHalfProperty, binding);
public static T AllowHalf<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.RatingCharacter
   => control._set(Ursa.Controls.RatingCharacter.AllowHalfProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T AllowHalf<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.RatingCharacter
   => control._set(Ursa.Controls.RatingCharacter.AllowHalfProperty, func, onChanged, expression);
public static T AllowHalf<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RatingCharacter
=> control._setEx(Ursa.Controls.RatingCharacter.AllowHalfProperty, ps, () => control.AllowHalf = value, bindingMode, converter, bindingSource);
public static T AllowHalf<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RatingCharacter
=> control._setEx(Ursa.Controls.RatingCharacter.AllowHalfProperty, ps, () => control.AllowHalf = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Character<T>(this T control, IBinding binding) where T : Ursa.Controls.RatingCharacter
   => control._set(Ursa.Controls.RatingCharacter.CharacterProperty, binding);
public static T Character<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.RatingCharacter
   => control._set(Ursa.Controls.RatingCharacter.CharacterProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Character<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.RatingCharacter
   => control._set(Ursa.Controls.RatingCharacter.CharacterProperty, func, onChanged, expression);
public static T Character<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RatingCharacter
=> control._setEx(Ursa.Controls.RatingCharacter.CharacterProperty, ps, () => control.Character = value, bindingMode, converter, bindingSource);
public static T Character<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RatingCharacter
=> control._setEx(Ursa.Controls.RatingCharacter.CharacterProperty, ps, () => control.Character = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Size<T>(this T control, IBinding binding) where T : Ursa.Controls.RatingCharacter
   => control._set(Ursa.Controls.RatingCharacter.SizeProperty, binding);
public static T Size<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.RatingCharacter
   => control._set(Ursa.Controls.RatingCharacter.SizeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Size<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.RatingCharacter
   => control._set(Ursa.Controls.RatingCharacter.SizeProperty, func, onChanged, expression);
public static T Size<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RatingCharacter
=> control._setEx(Ursa.Controls.RatingCharacter.SizeProperty, ps, () => control.Size = value, bindingMode, converter, bindingSource);
public static T Size<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.RatingCharacter
=> control._setEx(Ursa.Controls.RatingCharacter.SizeProperty, ps, () => control.Size = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

