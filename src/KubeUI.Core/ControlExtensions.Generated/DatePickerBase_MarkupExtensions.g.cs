#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
[global::System.CodeDom.Compiler.GeneratedCode("AvaloniaExtensionGenerator", "1.0.0.0")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static partial class DatePickerBase_MarkupExtensions
{
//================= Properties ======================//
 // DisplayFormat

/*BindFromExpressionSetterGenerator*/
public static T DisplayFormat<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DatePickerBase 
   => control._set(Ursa.Controls.DatePickerBase.DisplayFormatProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T DisplayFormat<T>(this T control,System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase 
=> control._setEx(Ursa.Controls.DatePickerBase.DisplayFormatProperty, ps, () => control.DisplayFormat = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T DisplayFormat<T>(this T control, IBinding binding) where T : Ursa.Controls.DatePickerBase 
   => control._set(Ursa.Controls.DatePickerBase.DisplayFormatProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T DisplayFormat<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DatePickerBase 
   => control._set(Ursa.Controls.DatePickerBase.DisplayFormatProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T DisplayFormat<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase 
=> control._setEx(Ursa.Controls.DatePickerBase.DisplayFormatProperty, ps, () => control.DisplayFormat = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // BlackoutDates

/*BindFromExpressionSetterGenerator*/
public static T BlackoutDates<T>(this T control, Func<Avalonia.Collections.AvaloniaList<Ursa.Controls.DateRange>> func, Action<Avalonia.Collections.AvaloniaList<Ursa.Controls.DateRange>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DatePickerBase 
   => control._set(Ursa.Controls.DatePickerBase.BlackoutDatesProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T BlackoutDates<T>(this T control,Avalonia.Collections.AvaloniaList<Ursa.Controls.DateRange> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase 
=> control._setEx(Ursa.Controls.DatePickerBase.BlackoutDatesProperty, ps, () => control.BlackoutDates = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T BlackoutDates<T>(this T control, IBinding binding) where T : Ursa.Controls.DatePickerBase 
   => control._set(Ursa.Controls.DatePickerBase.BlackoutDatesProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T BlackoutDates<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DatePickerBase 
   => control._set(Ursa.Controls.DatePickerBase.BlackoutDatesProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T BlackoutDates<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Collections.AvaloniaList<Ursa.Controls.DateRange>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase 
=> control._setEx(Ursa.Controls.DatePickerBase.BlackoutDatesProperty, ps, () => control.BlackoutDates = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // BlackoutDateRule

/*BindFromExpressionSetterGenerator*/
public static T BlackoutDateRule<T>(this T control, Func<Ursa.Controls.IDateSelector> func, Action<Ursa.Controls.IDateSelector>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DatePickerBase 
   => control._set(Ursa.Controls.DatePickerBase.BlackoutDateRuleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T BlackoutDateRule<T>(this T control,Ursa.Controls.IDateSelector value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase 
=> control._setEx(Ursa.Controls.DatePickerBase.BlackoutDateRuleProperty, ps, () => control.BlackoutDateRule = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T BlackoutDateRule<T>(this T control, IBinding binding) where T : Ursa.Controls.DatePickerBase 
   => control._set(Ursa.Controls.DatePickerBase.BlackoutDateRuleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T BlackoutDateRule<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DatePickerBase 
   => control._set(Ursa.Controls.DatePickerBase.BlackoutDateRuleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T BlackoutDateRule<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Ursa.Controls.IDateSelector> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase 
=> control._setEx(Ursa.Controls.DatePickerBase.BlackoutDateRuleProperty, ps, () => control.BlackoutDateRule = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // FirstDayOfWeek

/*BindFromExpressionSetterGenerator*/
public static T FirstDayOfWeek<T>(this T control, Func<System.DayOfWeek> func, Action<System.DayOfWeek>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DatePickerBase 
   => control._set(Ursa.Controls.DatePickerBase.FirstDayOfWeekProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T FirstDayOfWeek<T>(this T control,System.DayOfWeek value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase 
=> control._setEx(Ursa.Controls.DatePickerBase.FirstDayOfWeekProperty, ps, () => control.FirstDayOfWeek = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T FirstDayOfWeek<T>(this T control, IBinding binding) where T : Ursa.Controls.DatePickerBase 
   => control._set(Ursa.Controls.DatePickerBase.FirstDayOfWeekProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T FirstDayOfWeek<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DatePickerBase 
   => control._set(Ursa.Controls.DatePickerBase.FirstDayOfWeekProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T FirstDayOfWeek<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.DayOfWeek> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase 
=> control._setEx(Ursa.Controls.DatePickerBase.FirstDayOfWeekProperty, ps, () => control.FirstDayOfWeek = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsTodayHighlighted

/*BindFromExpressionSetterGenerator*/
public static T IsTodayHighlighted<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DatePickerBase 
   => control._set(Ursa.Controls.DatePickerBase.IsTodayHighlightedProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsTodayHighlighted<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase 
=> control._setEx(Ursa.Controls.DatePickerBase.IsTodayHighlightedProperty, ps, () => control.IsTodayHighlighted = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsTodayHighlighted<T>(this T control, IBinding binding) where T : Ursa.Controls.DatePickerBase 
   => control._set(Ursa.Controls.DatePickerBase.IsTodayHighlightedProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsTodayHighlighted<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DatePickerBase 
   => control._set(Ursa.Controls.DatePickerBase.IsTodayHighlightedProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsTodayHighlighted<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase 
=> control._setEx(Ursa.Controls.DatePickerBase.IsTodayHighlightedProperty, ps, () => control.IsTodayHighlighted = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // InnerLeftContent

/*BindFromExpressionSetterGenerator*/
public static T InnerLeftContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DatePickerBase 
   => control._set(Ursa.Controls.DatePickerBase.InnerLeftContentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T InnerLeftContent<T>(this T control,System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase 
=> control._setEx(Ursa.Controls.DatePickerBase.InnerLeftContentProperty, ps, () => control.InnerLeftContent = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T InnerLeftContent<T>(this T control, IBinding binding) where T : Ursa.Controls.DatePickerBase 
   => control._set(Ursa.Controls.DatePickerBase.InnerLeftContentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T InnerLeftContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DatePickerBase 
   => control._set(Ursa.Controls.DatePickerBase.InnerLeftContentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T InnerLeftContent<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase 
=> control._setEx(Ursa.Controls.DatePickerBase.InnerLeftContentProperty, ps, () => control.InnerLeftContent = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // InnerRightContent

/*BindFromExpressionSetterGenerator*/
public static T InnerRightContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DatePickerBase 
   => control._set(Ursa.Controls.DatePickerBase.InnerRightContentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T InnerRightContent<T>(this T control,System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase 
=> control._setEx(Ursa.Controls.DatePickerBase.InnerRightContentProperty, ps, () => control.InnerRightContent = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T InnerRightContent<T>(this T control, IBinding binding) where T : Ursa.Controls.DatePickerBase 
   => control._set(Ursa.Controls.DatePickerBase.InnerRightContentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T InnerRightContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DatePickerBase 
   => control._set(Ursa.Controls.DatePickerBase.InnerRightContentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T InnerRightContent<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase 
=> control._setEx(Ursa.Controls.DatePickerBase.InnerRightContentProperty, ps, () => control.InnerRightContent = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // PopupInnerTopContent

/*BindFromExpressionSetterGenerator*/
public static T PopupInnerTopContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DatePickerBase 
   => control._set(Ursa.Controls.DatePickerBase.PopupInnerTopContentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T PopupInnerTopContent<T>(this T control,System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase 
=> control._setEx(Ursa.Controls.DatePickerBase.PopupInnerTopContentProperty, ps, () => control.PopupInnerTopContent = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T PopupInnerTopContent<T>(this T control, IBinding binding) where T : Ursa.Controls.DatePickerBase 
   => control._set(Ursa.Controls.DatePickerBase.PopupInnerTopContentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T PopupInnerTopContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DatePickerBase 
   => control._set(Ursa.Controls.DatePickerBase.PopupInnerTopContentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T PopupInnerTopContent<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase 
=> control._setEx(Ursa.Controls.DatePickerBase.PopupInnerTopContentProperty, ps, () => control.PopupInnerTopContent = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // PopupInnerBottomContent

/*BindFromExpressionSetterGenerator*/
public static T PopupInnerBottomContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DatePickerBase 
   => control._set(Ursa.Controls.DatePickerBase.PopupInnerBottomContentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T PopupInnerBottomContent<T>(this T control,System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase 
=> control._setEx(Ursa.Controls.DatePickerBase.PopupInnerBottomContentProperty, ps, () => control.PopupInnerBottomContent = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T PopupInnerBottomContent<T>(this T control, IBinding binding) where T : Ursa.Controls.DatePickerBase 
   => control._set(Ursa.Controls.DatePickerBase.PopupInnerBottomContentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T PopupInnerBottomContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DatePickerBase 
   => control._set(Ursa.Controls.DatePickerBase.PopupInnerBottomContentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T PopupInnerBottomContent<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase 
=> control._setEx(Ursa.Controls.DatePickerBase.PopupInnerBottomContentProperty, ps, () => control.PopupInnerBottomContent = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsDropdownOpen

/*BindFromExpressionSetterGenerator*/
public static T IsDropdownOpen<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DatePickerBase 
   => control._set(Ursa.Controls.DatePickerBase.IsDropdownOpenProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsDropdownOpen<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase 
=> control._setEx(Ursa.Controls.DatePickerBase.IsDropdownOpenProperty, ps, () => control.IsDropdownOpen = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsDropdownOpen<T>(this T control, IBinding binding) where T : Ursa.Controls.DatePickerBase 
   => control._set(Ursa.Controls.DatePickerBase.IsDropdownOpenProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsDropdownOpen<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DatePickerBase 
   => control._set(Ursa.Controls.DatePickerBase.IsDropdownOpenProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsDropdownOpen<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase 
=> control._setEx(Ursa.Controls.DatePickerBase.IsDropdownOpenProperty, ps, () => control.IsDropdownOpen = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsReadonly

/*BindFromExpressionSetterGenerator*/
public static T IsReadonly<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DatePickerBase 
   => control._set(Ursa.Controls.DatePickerBase.IsReadonlyProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsReadonly<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase 
=> control._setEx(Ursa.Controls.DatePickerBase.IsReadonlyProperty, ps, () => control.IsReadonly = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsReadonly<T>(this T control, IBinding binding) where T : Ursa.Controls.DatePickerBase 
   => control._set(Ursa.Controls.DatePickerBase.IsReadonlyProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsReadonly<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DatePickerBase 
   => control._set(Ursa.Controls.DatePickerBase.IsReadonlyProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsReadonly<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DatePickerBase 
=> control._setEx(Ursa.Controls.DatePickerBase.IsReadonlyProperty, ps, () => control.IsReadonly = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Styles ======================//
 // DisplayFormat

/*ValueStyleSetterGenerator*/
public static Style<T> DisplayFormat<T>(this Style<T> style, System.String value) where T : Ursa.Controls.DatePickerBase 
=> style._addSetter(Ursa.Controls.DatePickerBase.DisplayFormatProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> DisplayFormat<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DatePickerBase 
=> style._addSetter(Ursa.Controls.DatePickerBase.DisplayFormatProperty, binding);


 // BlackoutDates

/*ValueStyleSetterGenerator*/
public static Style<T> BlackoutDates<T>(this Style<T> style, Avalonia.Collections.AvaloniaList<Ursa.Controls.DateRange> value) where T : Ursa.Controls.DatePickerBase 
=> style._addSetter(Ursa.Controls.DatePickerBase.BlackoutDatesProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> BlackoutDates<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DatePickerBase 
=> style._addSetter(Ursa.Controls.DatePickerBase.BlackoutDatesProperty, binding);


 // BlackoutDateRule

/*ValueStyleSetterGenerator*/
public static Style<T> BlackoutDateRule<T>(this Style<T> style, Ursa.Controls.IDateSelector value) where T : Ursa.Controls.DatePickerBase 
=> style._addSetter(Ursa.Controls.DatePickerBase.BlackoutDateRuleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> BlackoutDateRule<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DatePickerBase 
=> style._addSetter(Ursa.Controls.DatePickerBase.BlackoutDateRuleProperty, binding);


 // FirstDayOfWeek

/*ValueStyleSetterGenerator*/
public static Style<T> FirstDayOfWeek<T>(this Style<T> style, System.DayOfWeek value) where T : Ursa.Controls.DatePickerBase 
=> style._addSetter(Ursa.Controls.DatePickerBase.FirstDayOfWeekProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> FirstDayOfWeek<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DatePickerBase 
=> style._addSetter(Ursa.Controls.DatePickerBase.FirstDayOfWeekProperty, binding);


 // IsTodayHighlighted

/*ValueStyleSetterGenerator*/
public static Style<T> IsTodayHighlighted<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.DatePickerBase 
=> style._addSetter(Ursa.Controls.DatePickerBase.IsTodayHighlightedProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsTodayHighlighted<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DatePickerBase 
=> style._addSetter(Ursa.Controls.DatePickerBase.IsTodayHighlightedProperty, binding);


 // InnerLeftContent

/*ValueStyleSetterGenerator*/
public static Style<T> InnerLeftContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.DatePickerBase 
=> style._addSetter(Ursa.Controls.DatePickerBase.InnerLeftContentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> InnerLeftContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DatePickerBase 
=> style._addSetter(Ursa.Controls.DatePickerBase.InnerLeftContentProperty, binding);


 // InnerRightContent

/*ValueStyleSetterGenerator*/
public static Style<T> InnerRightContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.DatePickerBase 
=> style._addSetter(Ursa.Controls.DatePickerBase.InnerRightContentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> InnerRightContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DatePickerBase 
=> style._addSetter(Ursa.Controls.DatePickerBase.InnerRightContentProperty, binding);


 // PopupInnerTopContent

/*ValueStyleSetterGenerator*/
public static Style<T> PopupInnerTopContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.DatePickerBase 
=> style._addSetter(Ursa.Controls.DatePickerBase.PopupInnerTopContentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> PopupInnerTopContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DatePickerBase 
=> style._addSetter(Ursa.Controls.DatePickerBase.PopupInnerTopContentProperty, binding);


 // PopupInnerBottomContent

/*ValueStyleSetterGenerator*/
public static Style<T> PopupInnerBottomContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.DatePickerBase 
=> style._addSetter(Ursa.Controls.DatePickerBase.PopupInnerBottomContentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> PopupInnerBottomContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DatePickerBase 
=> style._addSetter(Ursa.Controls.DatePickerBase.PopupInnerBottomContentProperty, binding);


 // IsDropdownOpen

/*ValueStyleSetterGenerator*/
public static Style<T> IsDropdownOpen<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.DatePickerBase 
=> style._addSetter(Ursa.Controls.DatePickerBase.IsDropdownOpenProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsDropdownOpen<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DatePickerBase 
=> style._addSetter(Ursa.Controls.DatePickerBase.IsDropdownOpenProperty, binding);


 // IsReadonly

/*ValueStyleSetterGenerator*/
public static Style<T> IsReadonly<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.DatePickerBase 
=> style._addSetter(Ursa.Controls.DatePickerBase.IsReadonlyProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsReadonly<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DatePickerBase 
=> style._addSetter(Ursa.Controls.DatePickerBase.IsReadonlyProperty, binding);



}
