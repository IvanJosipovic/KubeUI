#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class NumberDisplayerBase_MarkupExtensions
{
//================= Properties ======================//
 // InternalTextProperty

/*BindFromExpressionSetterGenerator*/
public static T InternalText<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumberDisplayerBase
   => control._set(Ursa.Controls.NumberDisplayerBase.InternalTextProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T InternalText<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumberDisplayerBase
=> control._setEx(Ursa.Controls.NumberDisplayerBase.InternalTextProperty, ps, () => control.InternalText = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T InternalText<T>(this T control, IBinding binding) where T : Ursa.Controls.NumberDisplayerBase
   => control._set(Ursa.Controls.NumberDisplayerBase.InternalTextProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T InternalText<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumberDisplayerBase
   => control._set(Ursa.Controls.NumberDisplayerBase.InternalTextProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T InternalText<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumberDisplayerBase
=> control._setEx(Ursa.Controls.NumberDisplayerBase.InternalTextProperty, ps, () => control.InternalText = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // DurationProperty

/*BindFromExpressionSetterGenerator*/
public static T Duration<T>(this T control, Func<System.TimeSpan> func, Action<System.TimeSpan>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumberDisplayerBase
   => control._set(Ursa.Controls.NumberDisplayerBase.DurationProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Duration<T>(this T control, System.TimeSpan value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumberDisplayerBase
=> control._setEx(Ursa.Controls.NumberDisplayerBase.DurationProperty, ps, () => control.Duration = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Duration<T>(this T control, IBinding binding) where T : Ursa.Controls.NumberDisplayerBase
   => control._set(Ursa.Controls.NumberDisplayerBase.DurationProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Duration<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumberDisplayerBase
   => control._set(Ursa.Controls.NumberDisplayerBase.DurationProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Duration<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.TimeSpan> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumberDisplayerBase
=> control._setEx(Ursa.Controls.NumberDisplayerBase.DurationProperty, ps, () => control.Duration = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // StringFormatProperty

/*BindFromExpressionSetterGenerator*/
public static T StringFormat<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumberDisplayerBase
   => control._set(Ursa.Controls.NumberDisplayerBase.StringFormatProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T StringFormat<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumberDisplayerBase
=> control._setEx(Ursa.Controls.NumberDisplayerBase.StringFormatProperty, ps, () => control.StringFormat = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T StringFormat<T>(this T control, IBinding binding) where T : Ursa.Controls.NumberDisplayerBase
   => control._set(Ursa.Controls.NumberDisplayerBase.StringFormatProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T StringFormat<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumberDisplayerBase
   => control._set(Ursa.Controls.NumberDisplayerBase.StringFormatProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T StringFormat<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumberDisplayerBase
=> control._setEx(Ursa.Controls.NumberDisplayerBase.StringFormatProperty, ps, () => control.StringFormat = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsSelectableProperty

/*BindFromExpressionSetterGenerator*/
public static T IsSelectable<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NumberDisplayerBase
   => control._set(Ursa.Controls.NumberDisplayerBase.IsSelectableProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsSelectable<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumberDisplayerBase
=> control._setEx(Ursa.Controls.NumberDisplayerBase.IsSelectableProperty, ps, () => control.IsSelectable = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsSelectable<T>(this T control, IBinding binding) where T : Ursa.Controls.NumberDisplayerBase
   => control._set(Ursa.Controls.NumberDisplayerBase.IsSelectableProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsSelectable<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NumberDisplayerBase
   => control._set(Ursa.Controls.NumberDisplayerBase.IsSelectableProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsSelectable<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NumberDisplayerBase
=> control._setEx(Ursa.Controls.NumberDisplayerBase.IsSelectableProperty, ps, () => control.IsSelectable = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // DurationProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Duration<T>(this Style<T> style, System.TimeSpan value) where T : Ursa.Controls.NumberDisplayerBase
=> style._addSetter(Ursa.Controls.NumberDisplayerBase.DurationProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Duration<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumberDisplayerBase
=> style._addSetter(Ursa.Controls.NumberDisplayerBase.DurationProperty, binding);


 // StringFormatProperty

/*ValueStyleSetterGenerator*/
public static Style<T> StringFormat<T>(this Style<T> style, System.String value) where T : Ursa.Controls.NumberDisplayerBase
=> style._addSetter(Ursa.Controls.NumberDisplayerBase.StringFormatProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> StringFormat<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumberDisplayerBase
=> style._addSetter(Ursa.Controls.NumberDisplayerBase.StringFormatProperty, binding);


 // IsSelectableProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsSelectable<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.NumberDisplayerBase
=> style._addSetter(Ursa.Controls.NumberDisplayerBase.IsSelectableProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsSelectable<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumberDisplayerBase
=> style._addSetter(Ursa.Controls.NumberDisplayerBase.IsSelectableProperty, binding);



}
