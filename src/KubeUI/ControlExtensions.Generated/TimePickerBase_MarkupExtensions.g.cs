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
public static partial class TimePickerBase_MarkupExtensions
{
//================= Properties ======================//
 // DisplayFormatProperty

/*BindFromExpressionSetterGenerator*/
public static T DisplayFormat<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.DisplayFormatProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T DisplayFormat<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerBase
=> control._setEx(Ursa.Controls.TimePickerBase.DisplayFormatProperty, ps, () => control.DisplayFormat = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T DisplayFormat<T>(this T control, IBinding binding) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.DisplayFormatProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T DisplayFormat<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.DisplayFormatProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T DisplayFormat<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerBase
=> control._setEx(Ursa.Controls.TimePickerBase.DisplayFormatProperty, ps, () => control.DisplayFormat = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // PanelFormatProperty

/*BindFromExpressionSetterGenerator*/
public static T PanelFormat<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.PanelFormatProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T PanelFormat<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerBase
=> control._setEx(Ursa.Controls.TimePickerBase.PanelFormatProperty, ps, () => control.PanelFormat = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T PanelFormat<T>(this T control, IBinding binding) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.PanelFormatProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T PanelFormat<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.PanelFormatProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T PanelFormat<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerBase
=> control._setEx(Ursa.Controls.TimePickerBase.PanelFormatProperty, ps, () => control.PanelFormat = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // NeedConfirmationProperty

/*BindFromExpressionSetterGenerator*/
public static T NeedConfirmation<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.NeedConfirmationProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T NeedConfirmation<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerBase
=> control._setEx(Ursa.Controls.TimePickerBase.NeedConfirmationProperty, ps, () => control.NeedConfirmation = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T NeedConfirmation<T>(this T control, IBinding binding) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.NeedConfirmationProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T NeedConfirmation<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.NeedConfirmationProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T NeedConfirmation<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerBase
=> control._setEx(Ursa.Controls.TimePickerBase.NeedConfirmationProperty, ps, () => control.NeedConfirmation = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // InnerLeftContentProperty

/*BindFromExpressionSetterGenerator*/
public static T InnerLeftContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.InnerLeftContentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T InnerLeftContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerBase
=> control._setEx(Ursa.Controls.TimePickerBase.InnerLeftContentProperty, ps, () => control.InnerLeftContent = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T InnerLeftContent<T>(this T control, IBinding binding) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.InnerLeftContentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T InnerLeftContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.InnerLeftContentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T InnerLeftContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerBase
=> control._setEx(Ursa.Controls.TimePickerBase.InnerLeftContentProperty, ps, () => control.InnerLeftContent = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // InnerRightContentProperty

/*BindFromExpressionSetterGenerator*/
public static T InnerRightContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.InnerRightContentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T InnerRightContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerBase
=> control._setEx(Ursa.Controls.TimePickerBase.InnerRightContentProperty, ps, () => control.InnerRightContent = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T InnerRightContent<T>(this T control, IBinding binding) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.InnerRightContentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T InnerRightContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.InnerRightContentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T InnerRightContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerBase
=> control._setEx(Ursa.Controls.TimePickerBase.InnerRightContentProperty, ps, () => control.InnerRightContent = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // PopupInnerTopContentProperty

/*BindFromExpressionSetterGenerator*/
public static T PopupInnerTopContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.PopupInnerTopContentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T PopupInnerTopContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerBase
=> control._setEx(Ursa.Controls.TimePickerBase.PopupInnerTopContentProperty, ps, () => control.PopupInnerTopContent = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T PopupInnerTopContent<T>(this T control, IBinding binding) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.PopupInnerTopContentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T PopupInnerTopContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.PopupInnerTopContentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T PopupInnerTopContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerBase
=> control._setEx(Ursa.Controls.TimePickerBase.PopupInnerTopContentProperty, ps, () => control.PopupInnerTopContent = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // PopupInnerBottomContentProperty

/*BindFromExpressionSetterGenerator*/
public static T PopupInnerBottomContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.PopupInnerBottomContentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T PopupInnerBottomContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerBase
=> control._setEx(Ursa.Controls.TimePickerBase.PopupInnerBottomContentProperty, ps, () => control.PopupInnerBottomContent = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T PopupInnerBottomContent<T>(this T control, IBinding binding) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.PopupInnerBottomContentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T PopupInnerBottomContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.PopupInnerBottomContentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T PopupInnerBottomContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerBase
=> control._setEx(Ursa.Controls.TimePickerBase.PopupInnerBottomContentProperty, ps, () => control.PopupInnerBottomContent = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsDropdownOpenProperty

/*BindFromExpressionSetterGenerator*/
public static T IsDropdownOpen<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.IsDropdownOpenProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsDropdownOpen<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerBase
=> control._setEx(Ursa.Controls.TimePickerBase.IsDropdownOpenProperty, ps, () => control.IsDropdownOpen = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsDropdownOpen<T>(this T control, IBinding binding) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.IsDropdownOpenProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsDropdownOpen<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.IsDropdownOpenProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsDropdownOpen<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerBase
=> control._setEx(Ursa.Controls.TimePickerBase.IsDropdownOpenProperty, ps, () => control.IsDropdownOpen = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsReadonlyProperty

/*BindFromExpressionSetterGenerator*/
public static T IsReadonly<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.IsReadonlyProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsReadonly<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerBase
=> control._setEx(Ursa.Controls.TimePickerBase.IsReadonlyProperty, ps, () => control.IsReadonly = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsReadonly<T>(this T control, IBinding binding) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.IsReadonlyProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsReadonly<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimePickerBase
   => control._set(Ursa.Controls.TimePickerBase.IsReadonlyProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsReadonly<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimePickerBase
=> control._setEx(Ursa.Controls.TimePickerBase.IsReadonlyProperty, ps, () => control.IsReadonly = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // DisplayFormatProperty

/*ValueStyleSetterGenerator*/
public static Style<T> DisplayFormat<T>(this Style<T> style, System.String value) where T : Ursa.Controls.TimePickerBase
=> style._addSetter(Ursa.Controls.TimePickerBase.DisplayFormatProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> DisplayFormat<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimePickerBase
=> style._addSetter(Ursa.Controls.TimePickerBase.DisplayFormatProperty, binding);


 // PanelFormatProperty

/*ValueStyleSetterGenerator*/
public static Style<T> PanelFormat<T>(this Style<T> style, System.String value) where T : Ursa.Controls.TimePickerBase
=> style._addSetter(Ursa.Controls.TimePickerBase.PanelFormatProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> PanelFormat<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimePickerBase
=> style._addSetter(Ursa.Controls.TimePickerBase.PanelFormatProperty, binding);


 // NeedConfirmationProperty

/*ValueStyleSetterGenerator*/
public static Style<T> NeedConfirmation<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.TimePickerBase
=> style._addSetter(Ursa.Controls.TimePickerBase.NeedConfirmationProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> NeedConfirmation<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimePickerBase
=> style._addSetter(Ursa.Controls.TimePickerBase.NeedConfirmationProperty, binding);


 // InnerLeftContentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> InnerLeftContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.TimePickerBase
=> style._addSetter(Ursa.Controls.TimePickerBase.InnerLeftContentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> InnerLeftContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimePickerBase
=> style._addSetter(Ursa.Controls.TimePickerBase.InnerLeftContentProperty, binding);


 // InnerRightContentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> InnerRightContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.TimePickerBase
=> style._addSetter(Ursa.Controls.TimePickerBase.InnerRightContentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> InnerRightContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimePickerBase
=> style._addSetter(Ursa.Controls.TimePickerBase.InnerRightContentProperty, binding);


 // PopupInnerTopContentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> PopupInnerTopContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.TimePickerBase
=> style._addSetter(Ursa.Controls.TimePickerBase.PopupInnerTopContentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> PopupInnerTopContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimePickerBase
=> style._addSetter(Ursa.Controls.TimePickerBase.PopupInnerTopContentProperty, binding);


 // PopupInnerBottomContentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> PopupInnerBottomContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.TimePickerBase
=> style._addSetter(Ursa.Controls.TimePickerBase.PopupInnerBottomContentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> PopupInnerBottomContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimePickerBase
=> style._addSetter(Ursa.Controls.TimePickerBase.PopupInnerBottomContentProperty, binding);


 // IsDropdownOpenProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsDropdownOpen<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.TimePickerBase
=> style._addSetter(Ursa.Controls.TimePickerBase.IsDropdownOpenProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsDropdownOpen<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimePickerBase
=> style._addSetter(Ursa.Controls.TimePickerBase.IsDropdownOpenProperty, binding);


 // IsReadonlyProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsReadonly<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.TimePickerBase
=> style._addSetter(Ursa.Controls.TimePickerBase.IsReadonlyProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsReadonly<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimePickerBase
=> style._addSetter(Ursa.Controls.TimePickerBase.IsReadonlyProperty, binding);



}
