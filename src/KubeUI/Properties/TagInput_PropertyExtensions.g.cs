#nullable enable
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TagInput = Ursa.Controls.TagInput;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class TagInputExtensions
{
public static T Tags<T>(this T control, IBinding binding) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.TagsProperty, binding);
public static T Tags<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.TagsProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Tags<T>(this T control, Func<System.Collections.Generic.IList<System.String>> func, Action<System.Collections.Generic.IList<System.String>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.TagsProperty, func, onChanged, expression);
public static T Tags<T>(this T control, System.Collections.Generic.IList<System.String> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.TagsProperty, ps, () => control.Tags = value, bindingMode, converter, bindingSource);
public static T Tags<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.Generic.IList<System.String>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.TagsProperty, ps, () => control.Tags = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Watermark<T>(this T control, IBinding binding) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.WatermarkProperty, binding);
public static T Watermark<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.WatermarkProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Watermark<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.WatermarkProperty, func, onChanged, expression);
public static T Watermark<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.WatermarkProperty, ps, () => control.Watermark = value, bindingMode, converter, bindingSource);
public static T Watermark<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.WatermarkProperty, ps, () => control.Watermark = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T AcceptsReturn<T>(this T control, IBinding binding) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.AcceptsReturnProperty, binding);
public static T AcceptsReturn<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.AcceptsReturnProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T AcceptsReturn<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.AcceptsReturnProperty, func, onChanged, expression);
public static T AcceptsReturn<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.AcceptsReturnProperty, ps, () => control.AcceptsReturn = value, bindingMode, converter, bindingSource);
public static T AcceptsReturn<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.AcceptsReturnProperty, ps, () => control.AcceptsReturn = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T MaxCount<T>(this T control, IBinding binding) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.MaxCountProperty, binding);
public static T MaxCount<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.MaxCountProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T MaxCount<T>(this T control, Func<System.Int32> func, Action<System.Int32>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.MaxCountProperty, func, onChanged, expression);
public static T MaxCount<T>(this T control, System.Int32 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.MaxCountProperty, ps, () => control.MaxCount = value, bindingMode, converter, bindingSource);
public static T MaxCount<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Int32> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.MaxCountProperty, ps, () => control.MaxCount = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T InputTheme<T>(this T control, IBinding binding) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.InputThemeProperty, binding);
public static T InputTheme<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.InputThemeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T InputTheme<T>(this T control, Func<Avalonia.Styling.ControlTheme> func, Action<Avalonia.Styling.ControlTheme>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.InputThemeProperty, func, onChanged, expression);
public static T InputTheme<T>(this T control, Avalonia.Styling.ControlTheme value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.InputThemeProperty, ps, () => control.InputTheme = value, bindingMode, converter, bindingSource);
public static T InputTheme<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ControlTheme> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.InputThemeProperty, ps, () => control.InputTheme = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ItemTemplate<T>(this T control, IBinding binding) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.ItemTemplateProperty, binding);
public static T ItemTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.ItemTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ItemTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.ItemTemplateProperty, func, onChanged, expression);
public static T ItemTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.ItemTemplateProperty, ps, () => control.ItemTemplate = value, bindingMode, converter, bindingSource);
public static T ItemTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.ItemTemplateProperty, ps, () => control.ItemTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Separator<T>(this T control, IBinding binding) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.SeparatorProperty, binding);
public static T Separator<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.SeparatorProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Separator<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.SeparatorProperty, func, onChanged, expression);
public static T Separator<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.SeparatorProperty, ps, () => control.Separator = value, bindingMode, converter, bindingSource);
public static T Separator<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.SeparatorProperty, ps, () => control.Separator = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T LostFocusBehavior<T>(this T control, IBinding binding) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.LostFocusBehaviorProperty, binding);
public static T LostFocusBehavior<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.LostFocusBehaviorProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T LostFocusBehavior<T>(this T control, Func<Ursa.Controls.LostFocusBehavior> func, Action<Ursa.Controls.LostFocusBehavior>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.LostFocusBehaviorProperty, func, onChanged, expression);
public static T LostFocusBehavior<T>(this T control, Ursa.Controls.LostFocusBehavior value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.LostFocusBehaviorProperty, ps, () => control.LostFocusBehavior = value, bindingMode, converter, bindingSource);
public static T LostFocusBehavior<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Ursa.Controls.LostFocusBehavior> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.LostFocusBehaviorProperty, ps, () => control.LostFocusBehavior = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T AllowDuplicates<T>(this T control, IBinding binding) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.AllowDuplicatesProperty, binding);
public static T AllowDuplicates<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.AllowDuplicatesProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T AllowDuplicates<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.AllowDuplicatesProperty, func, onChanged, expression);
public static T AllowDuplicates<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.AllowDuplicatesProperty, ps, () => control.AllowDuplicates = value, bindingMode, converter, bindingSource);
public static T AllowDuplicates<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.AllowDuplicatesProperty, ps, () => control.AllowDuplicates = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T InnerLeftContent<T>(this T control, IBinding binding) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.InnerLeftContentProperty, binding);
public static T InnerLeftContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.InnerLeftContentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T InnerLeftContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.InnerLeftContentProperty, func, onChanged, expression);
public static T InnerLeftContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.InnerLeftContentProperty, ps, () => control.InnerLeftContent = value, bindingMode, converter, bindingSource);
public static T InnerLeftContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.InnerLeftContentProperty, ps, () => control.InnerLeftContent = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T InnerRightContent<T>(this T control, IBinding binding) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.InnerRightContentProperty, binding);
public static T InnerRightContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.InnerRightContentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T InnerRightContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.InnerRightContentProperty, func, onChanged, expression);
public static T InnerRightContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.InnerRightContentProperty, ps, () => control.InnerRightContent = value, bindingMode, converter, bindingSource);
public static T InnerRightContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.InnerRightContentProperty, ps, () => control.InnerRightContent = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

