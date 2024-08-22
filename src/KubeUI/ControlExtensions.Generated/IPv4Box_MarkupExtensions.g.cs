#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class IPv4Box_MarkupExtensions
{
//================= Properties ======================//
 // IPAddressProperty

/*BindFromExpressionSetterGenerator*/
public static T IPAddress<T>(this T control, Func<System.Net.IPAddress> func, Action<System.Net.IPAddress>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.IPAddressProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IPAddress<T>(this T control, System.Net.IPAddress value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.IPv4Box
=> control._setEx(Ursa.Controls.IPv4Box.IPAddressProperty, ps, () => control.IPAddress = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IPAddress<T>(this T control, IBinding binding) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.IPAddressProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IPAddress<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.IPAddressProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IPAddress<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Net.IPAddress> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.IPv4Box
=> control._setEx(Ursa.Controls.IPv4Box.IPAddressProperty, ps, () => control.IPAddress = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TextAlignmentProperty

/*BindFromExpressionSetterGenerator*/
public static T TextAlignment<T>(this T control, Func<Avalonia.Media.TextAlignment> func, Action<Avalonia.Media.TextAlignment>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.TextAlignmentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T TextAlignment<T>(this T control, Avalonia.Media.TextAlignment value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.IPv4Box
=> control._setEx(Ursa.Controls.IPv4Box.TextAlignmentProperty, ps, () => control.TextAlignment = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TextAlignment<T>(this T control, IBinding binding) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.TextAlignmentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TextAlignment<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.TextAlignmentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T TextAlignment<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.TextAlignment> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.IPv4Box
=> control._setEx(Ursa.Controls.IPv4Box.TextAlignmentProperty, ps, () => control.TextAlignment = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SelectionBrushProperty

/*BindFromExpressionSetterGenerator*/
public static T SelectionBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.SelectionBrushProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SelectionBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.IPv4Box
=> control._setEx(Ursa.Controls.IPv4Box.SelectionBrushProperty, ps, () => control.SelectionBrush = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SelectionBrush<T>(this T control, IBinding binding) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.SelectionBrushProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SelectionBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.SelectionBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SelectionBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.IPv4Box
=> control._setEx(Ursa.Controls.IPv4Box.SelectionBrushProperty, ps, () => control.SelectionBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SelectionForegroundBrushProperty

/*BindFromExpressionSetterGenerator*/
public static T SelectionForegroundBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.SelectionForegroundBrushProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SelectionForegroundBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.IPv4Box
=> control._setEx(Ursa.Controls.IPv4Box.SelectionForegroundBrushProperty, ps, () => control.SelectionForegroundBrush = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SelectionForegroundBrush<T>(this T control, IBinding binding) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.SelectionForegroundBrushProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SelectionForegroundBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.SelectionForegroundBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SelectionForegroundBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.IPv4Box
=> control._setEx(Ursa.Controls.IPv4Box.SelectionForegroundBrushProperty, ps, () => control.SelectionForegroundBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CaretBrushProperty

/*BindFromExpressionSetterGenerator*/
public static T CaretBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.CaretBrushProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CaretBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.IPv4Box
=> control._setEx(Ursa.Controls.IPv4Box.CaretBrushProperty, ps, () => control.CaretBrush = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CaretBrush<T>(this T control, IBinding binding) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.CaretBrushProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CaretBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.CaretBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CaretBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.IPv4Box
=> control._setEx(Ursa.Controls.IPv4Box.CaretBrushProperty, ps, () => control.CaretBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ShowLeadingZeroProperty

/*BindFromExpressionSetterGenerator*/
public static T ShowLeadingZero<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.ShowLeadingZeroProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ShowLeadingZero<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.IPv4Box
=> control._setEx(Ursa.Controls.IPv4Box.ShowLeadingZeroProperty, ps, () => control.ShowLeadingZero = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ShowLeadingZero<T>(this T control, IBinding binding) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.ShowLeadingZeroProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ShowLeadingZero<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.ShowLeadingZeroProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ShowLeadingZero<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.IPv4Box
=> control._setEx(Ursa.Controls.IPv4Box.ShowLeadingZeroProperty, ps, () => control.ShowLeadingZero = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // InputModeProperty

/*BindFromExpressionSetterGenerator*/
public static T InputMode<T>(this T control, Func<Ursa.Controls.IPv4BoxInputMode> func, Action<Ursa.Controls.IPv4BoxInputMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.InputModeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T InputMode<T>(this T control, Ursa.Controls.IPv4BoxInputMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.IPv4Box
=> control._setEx(Ursa.Controls.IPv4Box.InputModeProperty, ps, () => control.InputMode = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T InputMode<T>(this T control, IBinding binding) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.InputModeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T InputMode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.IPv4Box
   => control._set(Ursa.Controls.IPv4Box.InputModeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T InputMode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Ursa.Controls.IPv4BoxInputMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.IPv4Box
=> control._setEx(Ursa.Controls.IPv4Box.InputModeProperty, ps, () => control.InputMode = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // IPAddressProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IPAddress<T>(this Style<T> style, System.Net.IPAddress value) where T : Ursa.Controls.IPv4Box
=> style._addSetter(Ursa.Controls.IPv4Box.IPAddressProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IPAddress<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.IPv4Box
=> style._addSetter(Ursa.Controls.IPv4Box.IPAddressProperty, binding);


 // TextAlignmentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> TextAlignment<T>(this Style<T> style, Avalonia.Media.TextAlignment value) where T : Ursa.Controls.IPv4Box
=> style._addSetter(Ursa.Controls.IPv4Box.TextAlignmentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> TextAlignment<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.IPv4Box
=> style._addSetter(Ursa.Controls.IPv4Box.TextAlignmentProperty, binding);


 // SelectionBrushProperty

/*ValueStyleSetterGenerator*/
public static Style<T> SelectionBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Ursa.Controls.IPv4Box
=> style._addSetter(Ursa.Controls.IPv4Box.SelectionBrushProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SelectionBrush<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.IPv4Box
=> style._addSetter(Ursa.Controls.IPv4Box.SelectionBrushProperty, binding);


 // SelectionForegroundBrushProperty

/*ValueStyleSetterGenerator*/
public static Style<T> SelectionForegroundBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Ursa.Controls.IPv4Box
=> style._addSetter(Ursa.Controls.IPv4Box.SelectionForegroundBrushProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SelectionForegroundBrush<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.IPv4Box
=> style._addSetter(Ursa.Controls.IPv4Box.SelectionForegroundBrushProperty, binding);


 // CaretBrushProperty

/*ValueStyleSetterGenerator*/
public static Style<T> CaretBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Ursa.Controls.IPv4Box
=> style._addSetter(Ursa.Controls.IPv4Box.CaretBrushProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CaretBrush<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.IPv4Box
=> style._addSetter(Ursa.Controls.IPv4Box.CaretBrushProperty, binding);


 // ShowLeadingZeroProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ShowLeadingZero<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.IPv4Box
=> style._addSetter(Ursa.Controls.IPv4Box.ShowLeadingZeroProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ShowLeadingZero<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.IPv4Box
=> style._addSetter(Ursa.Controls.IPv4Box.ShowLeadingZeroProperty, binding);


 // InputModeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> InputMode<T>(this Style<T> style, Ursa.Controls.IPv4BoxInputMode value) where T : Ursa.Controls.IPv4Box
=> style._addSetter(Ursa.Controls.IPv4Box.InputModeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> InputMode<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.IPv4Box
=> style._addSetter(Ursa.Controls.IPv4Box.InputModeProperty, binding);



}
