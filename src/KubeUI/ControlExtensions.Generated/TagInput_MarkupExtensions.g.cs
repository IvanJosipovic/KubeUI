#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class TagInput_MarkupExtensions
{
//================= Properties ======================//
 // TagsProperty

/*BindFromExpressionSetterGenerator*/
public static T Tags<T>(this T control, Func<System.Collections.Generic.IList<System.String>> func, Action<System.Collections.Generic.IList<System.String>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.TagsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Tags<T>(this T control, System.Collections.Generic.IList<System.String> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.TagsProperty, ps, () => control.Tags = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Tags<T>(this T control, IBinding binding) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.TagsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Tags<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.TagsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Tags<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.Generic.IList<System.String>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.TagsProperty, ps, () => control.Tags = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // WatermarkProperty

/*BindFromExpressionSetterGenerator*/
public static T Watermark<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.WatermarkProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Watermark<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.WatermarkProperty, ps, () => control.Watermark = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Watermark<T>(this T control, IBinding binding) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.WatermarkProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Watermark<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.WatermarkProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Watermark<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.WatermarkProperty, ps, () => control.Watermark = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // AcceptsReturnProperty

/*BindFromExpressionSetterGenerator*/
public static T AcceptsReturn<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.AcceptsReturnProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T AcceptsReturn<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.AcceptsReturnProperty, ps, () => control.AcceptsReturn = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T AcceptsReturn<T>(this T control, IBinding binding) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.AcceptsReturnProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T AcceptsReturn<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.AcceptsReturnProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T AcceptsReturn<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.AcceptsReturnProperty, ps, () => control.AcceptsReturn = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // MaxCountProperty

/*BindFromExpressionSetterGenerator*/
public static T MaxCount<T>(this T control, Func<System.Int32> func, Action<System.Int32>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.MaxCountProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T MaxCount<T>(this T control, System.Int32 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.MaxCountProperty, ps, () => control.MaxCount = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T MaxCount<T>(this T control, IBinding binding) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.MaxCountProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T MaxCount<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.MaxCountProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T MaxCount<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Int32> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.MaxCountProperty, ps, () => control.MaxCount = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // InputThemeProperty

/*BindFromExpressionSetterGenerator*/
public static T InputTheme<T>(this T control, Func<Avalonia.Styling.ControlTheme> func, Action<Avalonia.Styling.ControlTheme>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.InputThemeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T InputTheme<T>(this T control, Avalonia.Styling.ControlTheme value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.InputThemeProperty, ps, () => control.InputTheme = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T InputTheme<T>(this T control, IBinding binding) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.InputThemeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T InputTheme<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.InputThemeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T InputTheme<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ControlTheme> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.InputThemeProperty, ps, () => control.InputTheme = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ItemTemplateProperty

/*BindFromExpressionSetterGenerator*/
public static T ItemTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.ItemTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ItemTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.ItemTemplateProperty, ps, () => control.ItemTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ItemTemplate<T>(this T control, IBinding binding) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.ItemTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ItemTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.ItemTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ItemTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.ItemTemplateProperty, ps, () => control.ItemTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SeparatorProperty

/*BindFromExpressionSetterGenerator*/
public static T Separator<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.SeparatorProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Separator<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.SeparatorProperty, ps, () => control.Separator = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Separator<T>(this T control, IBinding binding) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.SeparatorProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Separator<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.SeparatorProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Separator<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.SeparatorProperty, ps, () => control.Separator = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // LostFocusBehaviorProperty

/*BindFromExpressionSetterGenerator*/
public static T LostFocusBehavior<T>(this T control, Func<Ursa.Controls.LostFocusBehavior> func, Action<Ursa.Controls.LostFocusBehavior>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.LostFocusBehaviorProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T LostFocusBehavior<T>(this T control, Ursa.Controls.LostFocusBehavior value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.LostFocusBehaviorProperty, ps, () => control.LostFocusBehavior = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T LostFocusBehavior<T>(this T control, IBinding binding) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.LostFocusBehaviorProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T LostFocusBehavior<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.LostFocusBehaviorProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T LostFocusBehavior<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Ursa.Controls.LostFocusBehavior> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.LostFocusBehaviorProperty, ps, () => control.LostFocusBehavior = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // AllowDuplicatesProperty

/*BindFromExpressionSetterGenerator*/
public static T AllowDuplicates<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.AllowDuplicatesProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T AllowDuplicates<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.AllowDuplicatesProperty, ps, () => control.AllowDuplicates = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T AllowDuplicates<T>(this T control, IBinding binding) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.AllowDuplicatesProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T AllowDuplicates<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.AllowDuplicatesProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T AllowDuplicates<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.AllowDuplicatesProperty, ps, () => control.AllowDuplicates = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // InnerLeftContentProperty

/*BindFromExpressionSetterGenerator*/
public static T InnerLeftContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.InnerLeftContentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T InnerLeftContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.InnerLeftContentProperty, ps, () => control.InnerLeftContent = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T InnerLeftContent<T>(this T control, IBinding binding) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.InnerLeftContentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T InnerLeftContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.InnerLeftContentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T InnerLeftContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.InnerLeftContentProperty, ps, () => control.InnerLeftContent = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // InnerRightContentProperty

/*BindFromExpressionSetterGenerator*/
public static T InnerRightContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.InnerRightContentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T InnerRightContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.InnerRightContentProperty, ps, () => control.InnerRightContent = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T InnerRightContent<T>(this T control, IBinding binding) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.InnerRightContentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T InnerRightContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TagInput
   => control._set(Ursa.Controls.TagInput.InnerRightContentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T InnerRightContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TagInput
=> control._setEx(Ursa.Controls.TagInput.InnerRightContentProperty, ps, () => control.InnerRightContent = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // TagsProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Tags<T>(this Style<T> style, System.Collections.Generic.IList<System.String> value) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.TagsProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Tags<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.TagsProperty, binding);


 // WatermarkProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Watermark<T>(this Style<T> style, System.String value) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.WatermarkProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Watermark<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.WatermarkProperty, binding);


 // AcceptsReturnProperty

/*ValueStyleSetterGenerator*/
public static Style<T> AcceptsReturn<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.AcceptsReturnProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> AcceptsReturn<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.AcceptsReturnProperty, binding);


 // MaxCountProperty

/*ValueStyleSetterGenerator*/
public static Style<T> MaxCount<T>(this Style<T> style, System.Int32 value) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.MaxCountProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> MaxCount<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.MaxCountProperty, binding);


 // InputThemeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> InputTheme<T>(this Style<T> style, Avalonia.Styling.ControlTheme value) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.InputThemeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> InputTheme<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.InputThemeProperty, binding);


 // ItemTemplateProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ItemTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.ItemTemplateProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ItemTemplate<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.ItemTemplateProperty, binding);


 // SeparatorProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Separator<T>(this Style<T> style, System.String value) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.SeparatorProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Separator<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.SeparatorProperty, binding);


 // LostFocusBehaviorProperty

/*ValueStyleSetterGenerator*/
public static Style<T> LostFocusBehavior<T>(this Style<T> style, Ursa.Controls.LostFocusBehavior value) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.LostFocusBehaviorProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> LostFocusBehavior<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.LostFocusBehaviorProperty, binding);


 // AllowDuplicatesProperty

/*ValueStyleSetterGenerator*/
public static Style<T> AllowDuplicates<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.AllowDuplicatesProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> AllowDuplicates<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.AllowDuplicatesProperty, binding);


 // InnerLeftContentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> InnerLeftContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.InnerLeftContentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> InnerLeftContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.InnerLeftContentProperty, binding);


 // InnerRightContentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> InnerRightContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.InnerRightContentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> InnerRightContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.InnerRightContentProperty, binding);



}
