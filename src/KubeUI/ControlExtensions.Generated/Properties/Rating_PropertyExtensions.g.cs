#nullable enable
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Rating = Ursa.Controls.Rating;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class RatingExtensions
{
public static T Value<T>(this T control, IBinding binding) where T : Ursa.Controls.Rating
   => control._set(Ursa.Controls.Rating.ValueProperty, binding);
public static T Value<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Rating
   => control._set(Ursa.Controls.Rating.ValueProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Value<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Rating
   => control._set(Ursa.Controls.Rating.ValueProperty, func, onChanged, expression);
public static T Value<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Rating
=> control._setEx(Ursa.Controls.Rating.ValueProperty, ps, () => control.Value = value, bindingMode, converter, bindingSource);
public static T Value<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Rating
=> control._setEx(Ursa.Controls.Rating.ValueProperty, ps, () => control.Value = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T AllowClear<T>(this T control, IBinding binding) where T : Ursa.Controls.Rating
   => control._set(Ursa.Controls.Rating.AllowClearProperty, binding);
public static T AllowClear<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Rating
   => control._set(Ursa.Controls.Rating.AllowClearProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T AllowClear<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Rating
   => control._set(Ursa.Controls.Rating.AllowClearProperty, func, onChanged, expression);
public static T AllowClear<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Rating
=> control._setEx(Ursa.Controls.Rating.AllowClearProperty, ps, () => control.AllowClear = value, bindingMode, converter, bindingSource);
public static T AllowClear<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Rating
=> control._setEx(Ursa.Controls.Rating.AllowClearProperty, ps, () => control.AllowClear = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T AllowHalf<T>(this T control, IBinding binding) where T : Ursa.Controls.Rating
   => control._set(Ursa.Controls.Rating.AllowHalfProperty, binding);
public static T AllowHalf<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Rating
   => control._set(Ursa.Controls.Rating.AllowHalfProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T AllowHalf<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Rating
   => control._set(Ursa.Controls.Rating.AllowHalfProperty, func, onChanged, expression);
public static T AllowHalf<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Rating
=> control._setEx(Ursa.Controls.Rating.AllowHalfProperty, ps, () => control.AllowHalf = value, bindingMode, converter, bindingSource);
public static T AllowHalf<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Rating
=> control._setEx(Ursa.Controls.Rating.AllowHalfProperty, ps, () => control.AllowHalf = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Character<T>(this T control, IBinding binding) where T : Ursa.Controls.Rating
   => control._set(Ursa.Controls.Rating.CharacterProperty, binding);
public static T Character<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Rating
   => control._set(Ursa.Controls.Rating.CharacterProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Character<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Rating
   => control._set(Ursa.Controls.Rating.CharacterProperty, func, onChanged, expression);
public static T Character<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Rating
=> control._setEx(Ursa.Controls.Rating.CharacterProperty, ps, () => control.Character = value, bindingMode, converter, bindingSource);
public static T Character<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Rating
=> control._setEx(Ursa.Controls.Rating.CharacterProperty, ps, () => control.Character = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Count<T>(this T control, IBinding binding) where T : Ursa.Controls.Rating
   => control._set(Ursa.Controls.Rating.CountProperty, binding);
public static T Count<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Rating
   => control._set(Ursa.Controls.Rating.CountProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Count<T>(this T control, Func<System.Int32> func, Action<System.Int32>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Rating
   => control._set(Ursa.Controls.Rating.CountProperty, func, onChanged, expression);
public static T Count<T>(this T control, System.Int32 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Rating
=> control._setEx(Ursa.Controls.Rating.CountProperty, ps, () => control.Count = value, bindingMode, converter, bindingSource);
public static T Count<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Int32> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Rating
=> control._setEx(Ursa.Controls.Rating.CountProperty, ps, () => control.Count = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T DefaultValue<T>(this T control, IBinding binding) where T : Ursa.Controls.Rating
   => control._set(Ursa.Controls.Rating.DefaultValueProperty, binding);
public static T DefaultValue<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Rating
   => control._set(Ursa.Controls.Rating.DefaultValueProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T DefaultValue<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Rating
   => control._set(Ursa.Controls.Rating.DefaultValueProperty, func, onChanged, expression);
public static T DefaultValue<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Rating
=> control._setEx(Ursa.Controls.Rating.DefaultValueProperty, ps, () => control.DefaultValue = value, bindingMode, converter, bindingSource);
public static T DefaultValue<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Rating
=> control._setEx(Ursa.Controls.Rating.DefaultValueProperty, ps, () => control.DefaultValue = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Size<T>(this T control, IBinding binding) where T : Ursa.Controls.Rating
   => control._set(Ursa.Controls.Rating.SizeProperty, binding);
public static T Size<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Rating
   => control._set(Ursa.Controls.Rating.SizeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Size<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Rating
   => control._set(Ursa.Controls.Rating.SizeProperty, func, onChanged, expression);
public static T Size<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Rating
=> control._setEx(Ursa.Controls.Rating.SizeProperty, ps, () => control.Size = value, bindingMode, converter, bindingSource);
public static T Size<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Rating
=> control._setEx(Ursa.Controls.Rating.SizeProperty, ps, () => control.Size = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ItemTemplate<T>(this T control, IBinding binding) where T : Ursa.Controls.Rating
   => control._set(Ursa.Controls.Rating.ItemTemplateProperty, binding);
public static T ItemTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Rating
   => control._set(Ursa.Controls.Rating.ItemTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ItemTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Rating
   => control._set(Ursa.Controls.Rating.ItemTemplateProperty, func, onChanged, expression);
public static T ItemTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Rating
=> control._setEx(Ursa.Controls.Rating.ItemTemplateProperty, ps, () => control.ItemTemplate = value, bindingMode, converter, bindingSource);
public static T ItemTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Rating
=> control._setEx(Ursa.Controls.Rating.ItemTemplateProperty, ps, () => control.ItemTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

