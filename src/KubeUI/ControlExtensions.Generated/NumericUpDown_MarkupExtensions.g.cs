#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
[global::System.CodeDom.Compiler.GeneratedCode("AvaloniaExtensionGenerator", "11.1.3.0")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static partial class NumericUpDown_MarkupExtensions
{
//================= Properties ======================//
 // AllowDragProperty

/*BindFromExpressionSetterGenerator*/
public static T AllowDrag<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.AllowDragProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T AllowDrag<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.AllowDragProperty, ps, () => control.AllowDrag = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T AllowDrag<T>(this T control, IBinding binding) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.AllowDragProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T AllowDrag<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.AllowDragProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T AllowDrag<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.AllowDragProperty, ps, () => control.AllowDrag = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsReadOnlyProperty

/*BindFromExpressionSetterGenerator*/
public static T IsReadOnly<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.IsReadOnlyProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsReadOnly<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.IsReadOnlyProperty, ps, () => control.IsReadOnly = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsReadOnly<T>(this T control, IBinding binding) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.IsReadOnlyProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsReadOnly<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.IsReadOnlyProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsReadOnly<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.IsReadOnlyProperty, ps, () => control.IsReadOnly = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // HorizontalContentAlignmentProperty

/*BindFromExpressionSetterGenerator*/
public static T HorizontalContentAlignment<T>(this T control, Func<Avalonia.Layout.HorizontalAlignment> func, Action<Avalonia.Layout.HorizontalAlignment>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.HorizontalContentAlignmentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T HorizontalContentAlignment<T>(this T control, Avalonia.Layout.HorizontalAlignment value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.HorizontalContentAlignmentProperty, ps, () => control.HorizontalContentAlignment = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T HorizontalContentAlignment<T>(this T control, IBinding binding) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.HorizontalContentAlignmentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T HorizontalContentAlignment<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.HorizontalContentAlignmentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T HorizontalContentAlignment<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Layout.HorizontalAlignment> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.HorizontalContentAlignmentProperty, ps, () => control.HorizontalContentAlignment = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // InnerLeftContentProperty

/*BindFromExpressionSetterGenerator*/
public static T InnerLeftContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.InnerLeftContentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T InnerLeftContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.InnerLeftContentProperty, ps, () => control.InnerLeftContent = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T InnerLeftContent<T>(this T control, IBinding binding) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.InnerLeftContentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T InnerLeftContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.InnerLeftContentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T InnerLeftContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.InnerLeftContentProperty, ps, () => control.InnerLeftContent = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // InnerRightContentProperty

/*BindFromExpressionSetterGenerator*/
public static T InnerRightContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.InnerRightContentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T InnerRightContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.InnerRightContentProperty, ps, () => control.InnerRightContent = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T InnerRightContent<T>(this T control, IBinding binding) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.InnerRightContentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T InnerRightContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.InnerRightContentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T InnerRightContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.InnerRightContentProperty, ps, () => control.InnerRightContent = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // WatermarkProperty

/*BindFromExpressionSetterGenerator*/
public static T Watermark<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.WatermarkProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Watermark<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.WatermarkProperty, ps, () => control.Watermark = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Watermark<T>(this T control, IBinding binding) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.WatermarkProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Watermark<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.WatermarkProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Watermark<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.WatermarkProperty, ps, () => control.Watermark = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // NumberFormatProperty

/*BindFromExpressionSetterGenerator*/
public static T NumberFormat<T>(this T control, Func<System.Globalization.NumberFormatInfo> func, Action<System.Globalization.NumberFormatInfo>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.NumberFormatProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T NumberFormat<T>(this T control, System.Globalization.NumberFormatInfo value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.NumberFormatProperty, ps, () => control.NumberFormat = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T NumberFormat<T>(this T control, IBinding binding) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.NumberFormatProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T NumberFormat<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.NumberFormatProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T NumberFormat<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Globalization.NumberFormatInfo> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.NumberFormatProperty, ps, () => control.NumberFormat = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // FormatStringProperty

/*BindFromExpressionSetterGenerator*/
public static T FormatString<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.FormatStringProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T FormatString<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.FormatStringProperty, ps, () => control.FormatString = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T FormatString<T>(this T control, IBinding binding) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.FormatStringProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T FormatString<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.FormatStringProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T FormatString<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.FormatStringProperty, ps, () => control.FormatString = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ParsingNumberStyleProperty

/*BindFromExpressionSetterGenerator*/
public static T ParsingNumberStyle<T>(this T control, Func<System.Globalization.NumberStyles> func, Action<System.Globalization.NumberStyles>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.ParsingNumberStyleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ParsingNumberStyle<T>(this T control, System.Globalization.NumberStyles value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.ParsingNumberStyleProperty, ps, () => control.ParsingNumberStyle = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ParsingNumberStyle<T>(this T control, IBinding binding) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.ParsingNumberStyleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ParsingNumberStyle<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.ParsingNumberStyleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ParsingNumberStyle<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Globalization.NumberStyles> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.ParsingNumberStyleProperty, ps, () => control.ParsingNumberStyle = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TextConverterProperty

/*BindFromExpressionSetterGenerator*/
public static T TextConverter<T>(this T control, Func<Avalonia.Data.Converters.IValueConverter> func, Action<Avalonia.Data.Converters.IValueConverter>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.TextConverterProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T TextConverter<T>(this T control, Avalonia.Data.Converters.IValueConverter value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.TextConverterProperty, ps, () => control.TextConverter = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TextConverter<T>(this T control, IBinding binding) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.TextConverterProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TextConverter<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.TextConverterProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T TextConverter<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Data.Converters.IValueConverter> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.TextConverterProperty, ps, () => control.TextConverter = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // AllowSpinProperty

/*BindFromExpressionSetterGenerator*/
public static T AllowSpin<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.AllowSpinProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T AllowSpin<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.AllowSpinProperty, ps, () => control.AllowSpin = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T AllowSpin<T>(this T control, IBinding binding) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.AllowSpinProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T AllowSpin<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.AllowSpinProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T AllowSpin<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.AllowSpinProperty, ps, () => control.AllowSpin = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ShowButtonSpinnerProperty

/*BindFromExpressionSetterGenerator*/
public static T ShowButtonSpinner<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.ShowButtonSpinnerProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ShowButtonSpinner<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.ShowButtonSpinnerProperty, ps, () => control.ShowButtonSpinner = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ShowButtonSpinner<T>(this T control, IBinding binding) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.ShowButtonSpinnerProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ShowButtonSpinner<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumericUpDown
   => control._set(Ursa.Controls.NumericUpDown.ShowButtonSpinnerProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ShowButtonSpinner<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumericUpDown
=> control._setEx(Ursa.Controls.NumericUpDown.ShowButtonSpinnerProperty, ps, () => control.ShowButtonSpinner = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // Spinned

/*ActionToEventGenerator*/
    public static T OnSpinned<T>(this T control, Action<Avalonia.Controls.SpinEventArgs> action) where T : Ursa.Controls.NumericUpDown => 
        control._setEvent((System.EventHandler<Avalonia.Controls.SpinEventArgs>) ((arg0, arg1) => action(arg1)), h => control.Spinned += h);



//================= Styles ======================//
 // AllowDragProperty

/*ValueStyleSetterGenerator*/
public static Style<T> AllowDrag<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.AllowDragProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> AllowDrag<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.AllowDragProperty, binding);


 // IsReadOnlyProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsReadOnly<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.IsReadOnlyProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsReadOnly<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.IsReadOnlyProperty, binding);


 // HorizontalContentAlignmentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> HorizontalContentAlignment<T>(this Style<T> style, Avalonia.Layout.HorizontalAlignment value) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.HorizontalContentAlignmentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> HorizontalContentAlignment<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.HorizontalContentAlignmentProperty, binding);


 // InnerLeftContentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> InnerLeftContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.InnerLeftContentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> InnerLeftContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.InnerLeftContentProperty, binding);


 // InnerRightContentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> InnerRightContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.InnerRightContentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> InnerRightContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.InnerRightContentProperty, binding);


 // WatermarkProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Watermark<T>(this Style<T> style, System.String value) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.WatermarkProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Watermark<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.WatermarkProperty, binding);


 // NumberFormatProperty

/*ValueStyleSetterGenerator*/
public static Style<T> NumberFormat<T>(this Style<T> style, System.Globalization.NumberFormatInfo value) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.NumberFormatProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> NumberFormat<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.NumberFormatProperty, binding);


 // FormatStringProperty

/*ValueStyleSetterGenerator*/
public static Style<T> FormatString<T>(this Style<T> style, System.String value) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.FormatStringProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> FormatString<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.FormatStringProperty, binding);


 // ParsingNumberStyleProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ParsingNumberStyle<T>(this Style<T> style, System.Globalization.NumberStyles value) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.ParsingNumberStyleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ParsingNumberStyle<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.ParsingNumberStyleProperty, binding);


 // TextConverterProperty

/*ValueStyleSetterGenerator*/
public static Style<T> TextConverter<T>(this Style<T> style, Avalonia.Data.Converters.IValueConverter value) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.TextConverterProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> TextConverter<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.TextConverterProperty, binding);


 // AllowSpinProperty

/*ValueStyleSetterGenerator*/
public static Style<T> AllowSpin<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.AllowSpinProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> AllowSpin<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.AllowSpinProperty, binding);


 // ShowButtonSpinnerProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ShowButtonSpinner<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.ShowButtonSpinnerProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ShowButtonSpinner<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.ShowButtonSpinnerProperty, binding);



}
