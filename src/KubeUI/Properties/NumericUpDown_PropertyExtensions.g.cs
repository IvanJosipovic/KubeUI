#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using NumericUpDown = Ursa.Controls.NumericUpDown;
using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class NumericUpDownExtensions
{
public static T AllowDrag<T>(this T control, IBinding binding) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.AllowDragProperty, binding);
public static T AllowDrag<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.AllowDragProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T AllowDrag<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.AllowDragProperty, func, onChanged, expression);
public static T AllowDrag<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.AllowDragProperty, ps, () => control.AllowDrag = value, bindingMode, converter, bindingSource);
public static T AllowDrag<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.AllowDragProperty, ps, () => control.AllowDrag = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsReadOnly<T>(this T control, IBinding binding) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.IsReadOnlyProperty, binding);
public static T IsReadOnly<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.IsReadOnlyProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsReadOnly<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.IsReadOnlyProperty, func, onChanged, expression);
public static T IsReadOnly<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.IsReadOnlyProperty, ps, () => control.IsReadOnly = value, bindingMode, converter, bindingSource);
public static T IsReadOnly<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.IsReadOnlyProperty, ps, () => control.IsReadOnly = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T HorizontalContentAlignment<T>(this T control, IBinding binding) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.HorizontalContentAlignmentProperty, binding);
public static T HorizontalContentAlignment<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.HorizontalContentAlignmentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T HorizontalContentAlignment<T>(this T control, Func<Avalonia.Layout.HorizontalAlignment> func, Action<Avalonia.Layout.HorizontalAlignment>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.HorizontalContentAlignmentProperty, func, onChanged, expression);
public static T HorizontalContentAlignment<T>(this T control, Avalonia.Layout.HorizontalAlignment value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.HorizontalContentAlignmentProperty, ps, () => control.HorizontalContentAlignment = value, bindingMode, converter, bindingSource);
public static T HorizontalContentAlignment<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Layout.HorizontalAlignment> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.HorizontalContentAlignmentProperty, ps, () => control.HorizontalContentAlignment = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T InnerLeftContent<T>(this T control, IBinding binding) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.InnerLeftContentProperty, binding);
public static T InnerLeftContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.InnerLeftContentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T InnerLeftContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.InnerLeftContentProperty, func, onChanged, expression);
public static T InnerLeftContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.InnerLeftContentProperty, ps, () => control.InnerLeftContent = value, bindingMode, converter, bindingSource);
public static T InnerLeftContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.InnerLeftContentProperty, ps, () => control.InnerLeftContent = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Watermark<T>(this T control, IBinding binding) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.WatermarkProperty, binding);
public static T Watermark<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.WatermarkProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Watermark<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.WatermarkProperty, func, onChanged, expression);
public static T Watermark<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.WatermarkProperty, ps, () => control.Watermark = value, bindingMode, converter, bindingSource);
public static T Watermark<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.WatermarkProperty, ps, () => control.Watermark = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T NumberFormat<T>(this T control, IBinding binding) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.NumberFormatProperty, binding);
public static T NumberFormat<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.NumberFormatProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T NumberFormat<T>(this T control, Func<System.Globalization.NumberFormatInfo> func, Action<System.Globalization.NumberFormatInfo>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.NumberFormatProperty, func, onChanged, expression);
public static T NumberFormat<T>(this T control, System.Globalization.NumberFormatInfo value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.NumberFormatProperty, ps, () => control.NumberFormat = value, bindingMode, converter, bindingSource);
public static T NumberFormat<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Globalization.NumberFormatInfo> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.NumberFormatProperty, ps, () => control.NumberFormat = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T FormatString<T>(this T control, IBinding binding) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.FormatStringProperty, binding);
public static T FormatString<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.FormatStringProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T FormatString<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.FormatStringProperty, func, onChanged, expression);
public static T FormatString<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.FormatStringProperty, ps, () => control.FormatString = value, bindingMode, converter, bindingSource);
public static T FormatString<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.FormatStringProperty, ps, () => control.FormatString = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ParsingNumberStyle<T>(this T control, IBinding binding) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.ParsingNumberStyleProperty, binding);
public static T ParsingNumberStyle<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.ParsingNumberStyleProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ParsingNumberStyle<T>(this T control, Func<System.Globalization.NumberStyles> func, Action<System.Globalization.NumberStyles>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.ParsingNumberStyleProperty, func, onChanged, expression);
public static T ParsingNumberStyle<T>(this T control, System.Globalization.NumberStyles value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.ParsingNumberStyleProperty, ps, () => control.ParsingNumberStyle = value, bindingMode, converter, bindingSource);
public static T ParsingNumberStyle<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Globalization.NumberStyles> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.ParsingNumberStyleProperty, ps, () => control.ParsingNumberStyle = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T TextConverter<T>(this T control, IBinding binding) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.TextConverterProperty, binding);
public static T TextConverter<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.TextConverterProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T TextConverter<T>(this T control, Func<Avalonia.Data.Converters.IValueConverter> func, Action<Avalonia.Data.Converters.IValueConverter>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.TextConverterProperty, func, onChanged, expression);
public static T TextConverter<T>(this T control, Avalonia.Data.Converters.IValueConverter value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.TextConverterProperty, ps, () => control.TextConverter = value, bindingMode, converter, bindingSource);
public static T TextConverter<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Data.Converters.IValueConverter> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.TextConverterProperty, ps, () => control.TextConverter = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T AllowSpin<T>(this T control, IBinding binding) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.AllowSpinProperty, binding);
public static T AllowSpin<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.AllowSpinProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T AllowSpin<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.AllowSpinProperty, func, onChanged, expression);
public static T AllowSpin<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.AllowSpinProperty, ps, () => control.AllowSpin = value, bindingMode, converter, bindingSource);
public static T AllowSpin<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.AllowSpinProperty, ps, () => control.AllowSpin = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ShowButtonSpinner<T>(this T control, IBinding binding) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.ShowButtonSpinnerProperty, binding);
public static T ShowButtonSpinner<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.ShowButtonSpinnerProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ShowButtonSpinner<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.ShowButtonSpinnerProperty, func, onChanged, expression);
public static T ShowButtonSpinner<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.ShowButtonSpinnerProperty, ps, () => control.ShowButtonSpinner = value, bindingMode, converter, bindingSource);
public static T ShowButtonSpinner<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.ShowButtonSpinnerProperty, ps, () => control.ShowButtonSpinner = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

