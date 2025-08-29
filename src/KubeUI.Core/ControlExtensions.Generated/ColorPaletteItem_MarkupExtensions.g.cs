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
public static partial class ColorPaletteItem_MarkupExtensions
{
//================= Properties ======================//
 // Color

/*BindFromExpressionSetterGenerator*/
public static T Color<T>(this T control, Func<Avalonia.Media.Color> func, Action<Avalonia.Media.Color>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.ColorProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Color<T>(this T control,Avalonia.Media.Color value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPaletteItem.ColorProperty, ps, () => control.Color = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Color<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.ColorProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Color<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.ColorProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Color<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.Color> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPaletteItem.ColorProperty, ps, () => control.Color = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // BorderBrush

/*BindFromExpressionSetterGenerator*/
public static T BorderBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T BorderBrush<T>(this T control,Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushProperty, ps, () => control.BorderBrush = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T BorderBrush<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T BorderBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T BorderBrush<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushProperty, ps, () => control.BorderBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // BorderBrushPointerOver

/*BindFromExpressionSetterGenerator*/
public static T BorderBrushPointerOver<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushPointerOverProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T BorderBrushPointerOver<T>(this T control,Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushPointerOverProperty, ps, () => control.BorderBrushPointerOver = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T BorderBrushPointerOver<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushPointerOverProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T BorderBrushPointerOver<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushPointerOverProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T BorderBrushPointerOver<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushPointerOverProperty, ps, () => control.BorderBrushPointerOver = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // BorderBrushPressed

/*BindFromExpressionSetterGenerator*/
public static T BorderBrushPressed<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushPressedProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T BorderBrushPressed<T>(this T control,Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushPressedProperty, ps, () => control.BorderBrushPressed = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T BorderBrushPressed<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushPressedProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T BorderBrushPressed<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushPressedProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T BorderBrushPressed<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushPressedProperty, ps, () => control.BorderBrushPressed = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // BorderThickness

/*BindFromExpressionSetterGenerator*/
public static T BorderThickness<T>(this T control, Func<Avalonia.Thickness> func, Action<Avalonia.Thickness>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T BorderThickness<T>(this T control,Avalonia.Thickness value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessProperty, ps, () => control.BorderThickness = value, bindingMode, converter, bindingSource);

/*ValueOverloadsSetterGenerator*/

public static T BorderThickness<T>(this T control, System.Double uniformLength = default) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => control._set(() => control.BorderThickness = new Avalonia.Thickness(uniformLength));
public static T BorderThickness<T>(this T control, System.Double horizontal = default, System.Double vertical = default) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => control._set(() => control.BorderThickness = new Avalonia.Thickness(horizontal, vertical));
public static T BorderThickness<T>(this T control, System.Double left = default, System.Double top = default, System.Double right = default, System.Double bottom = default) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => control._set(() => control.BorderThickness = new Avalonia.Thickness(left, top, right, bottom));

/*BindSetterGenerator*/
public static T BorderThickness<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T BorderThickness<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T BorderThickness<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Thickness> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessProperty, ps, () => control.BorderThickness = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // BorderThicknessPointerOver

/*BindFromExpressionSetterGenerator*/
public static T BorderThicknessPointerOver<T>(this T control, Func<Avalonia.Thickness> func, Action<Avalonia.Thickness>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPointerOverProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T BorderThicknessPointerOver<T>(this T control,Avalonia.Thickness value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPointerOverProperty, ps, () => control.BorderThicknessPointerOver = value, bindingMode, converter, bindingSource);

/*ValueOverloadsSetterGenerator*/

public static T BorderThicknessPointerOver<T>(this T control, System.Double uniformLength = default) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => control._set(() => control.BorderThicknessPointerOver = new Avalonia.Thickness(uniformLength));
public static T BorderThicknessPointerOver<T>(this T control, System.Double horizontal = default, System.Double vertical = default) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => control._set(() => control.BorderThicknessPointerOver = new Avalonia.Thickness(horizontal, vertical));
public static T BorderThicknessPointerOver<T>(this T control, System.Double left = default, System.Double top = default, System.Double right = default, System.Double bottom = default) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => control._set(() => control.BorderThicknessPointerOver = new Avalonia.Thickness(left, top, right, bottom));

/*BindSetterGenerator*/
public static T BorderThicknessPointerOver<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPointerOverProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T BorderThicknessPointerOver<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPointerOverProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T BorderThicknessPointerOver<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Thickness> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPointerOverProperty, ps, () => control.BorderThicknessPointerOver = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // BorderThicknessPressed

/*BindFromExpressionSetterGenerator*/
public static T BorderThicknessPressed<T>(this T control, Func<Avalonia.Thickness> func, Action<Avalonia.Thickness>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPressedProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T BorderThicknessPressed<T>(this T control,Avalonia.Thickness value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPressedProperty, ps, () => control.BorderThicknessPressed = value, bindingMode, converter, bindingSource);

/*ValueOverloadsSetterGenerator*/

public static T BorderThicknessPressed<T>(this T control, System.Double uniformLength = default) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => control._set(() => control.BorderThicknessPressed = new Avalonia.Thickness(uniformLength));
public static T BorderThicknessPressed<T>(this T control, System.Double horizontal = default, System.Double vertical = default) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => control._set(() => control.BorderThicknessPressed = new Avalonia.Thickness(horizontal, vertical));
public static T BorderThicknessPressed<T>(this T control, System.Double left = default, System.Double top = default, System.Double right = default, System.Double bottom = default) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => control._set(() => control.BorderThicknessPressed = new Avalonia.Thickness(left, top, right, bottom));

/*BindSetterGenerator*/
public static T BorderThicknessPressed<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPressedProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T BorderThicknessPressed<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPressedProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T BorderThicknessPressed<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Thickness> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPressedProperty, ps, () => control.BorderThicknessPressed = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CornerRadius

/*BindFromExpressionSetterGenerator*/
public static T CornerRadius<T>(this T control, Func<Avalonia.CornerRadius> func, Action<Avalonia.CornerRadius>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.CornerRadiusProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CornerRadius<T>(this T control,Avalonia.CornerRadius value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPaletteItem.CornerRadiusProperty, ps, () => control.CornerRadius = value, bindingMode, converter, bindingSource);

/*ValueOverloadsSetterGenerator*/

public static T CornerRadius<T>(this T control, System.Double uniformRadius = default) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => control._set(() => control.CornerRadius = new Avalonia.CornerRadius(uniformRadius));
public static T CornerRadius<T>(this T control, System.Double top = default, System.Double bottom = default) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => control._set(() => control.CornerRadius = new Avalonia.CornerRadius(top, bottom));
public static T CornerRadius<T>(this T control, System.Double topLeft = default, System.Double topRight = default, System.Double bottomRight = default, System.Double bottomLeft = default) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => control._set(() => control.CornerRadius = new Avalonia.CornerRadius(topLeft, topRight, bottomRight, bottomLeft));

/*BindSetterGenerator*/
public static T CornerRadius<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.CornerRadiusProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CornerRadius<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => control._set(FluentAvalonia.UI.Controls.ColorPaletteItem.CornerRadiusProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CornerRadius<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.CornerRadius> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPaletteItem.CornerRadiusProperty, ps, () => control.CornerRadius = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Styles ======================//
 // BorderBrush

/*ValueStyleSetterGenerator*/
public static Style<T> BorderBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> BorderBrush<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushProperty, binding);


 // BorderBrushPointerOver

/*ValueStyleSetterGenerator*/
public static Style<T> BorderBrushPointerOver<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushPointerOverProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> BorderBrushPointerOver<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushPointerOverProperty, binding);


 // BorderBrushPressed

/*ValueStyleSetterGenerator*/
public static Style<T> BorderBrushPressed<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushPressedProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> BorderBrushPressed<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderBrushPressedProperty, binding);


 // BorderThickness

/*ValueStyleSetterGenerator*/
public static Style<T> BorderThickness<T>(this Style<T> style, Avalonia.Thickness value) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> BorderThickness<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessProperty, binding);

/*ValueOverloadsStyleSetterGenerator*/
public static Style<T> BorderThickness<T>(this Style<T> style, System.Double uniformLength) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessProperty, new Avalonia.Thickness(uniformLength));public static Style<T> BorderThickness<T>(this Style<T> style, System.Double horizontal, System.Double vertical) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessProperty, new Avalonia.Thickness(horizontal, vertical));public static Style<T> BorderThickness<T>(this Style<T> style, System.Double left, System.Double top, System.Double right, System.Double bottom) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessProperty, new Avalonia.Thickness(left, top, right, bottom));


 // BorderThicknessPointerOver

/*ValueStyleSetterGenerator*/
public static Style<T> BorderThicknessPointerOver<T>(this Style<T> style, Avalonia.Thickness value) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPointerOverProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> BorderThicknessPointerOver<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPointerOverProperty, binding);

/*ValueOverloadsStyleSetterGenerator*/
public static Style<T> BorderThicknessPointerOver<T>(this Style<T> style, System.Double uniformLength) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPointerOverProperty, new Avalonia.Thickness(uniformLength));public static Style<T> BorderThicknessPointerOver<T>(this Style<T> style, System.Double horizontal, System.Double vertical) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPointerOverProperty, new Avalonia.Thickness(horizontal, vertical));public static Style<T> BorderThicknessPointerOver<T>(this Style<T> style, System.Double left, System.Double top, System.Double right, System.Double bottom) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPointerOverProperty, new Avalonia.Thickness(left, top, right, bottom));


 // BorderThicknessPressed

/*ValueStyleSetterGenerator*/
public static Style<T> BorderThicknessPressed<T>(this Style<T> style, Avalonia.Thickness value) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPressedProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> BorderThicknessPressed<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPressedProperty, binding);

/*ValueOverloadsStyleSetterGenerator*/
public static Style<T> BorderThicknessPressed<T>(this Style<T> style, System.Double uniformLength) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPressedProperty, new Avalonia.Thickness(uniformLength));public static Style<T> BorderThicknessPressed<T>(this Style<T> style, System.Double horizontal, System.Double vertical) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPressedProperty, new Avalonia.Thickness(horizontal, vertical));public static Style<T> BorderThicknessPressed<T>(this Style<T> style, System.Double left, System.Double top, System.Double right, System.Double bottom) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.BorderThicknessPressedProperty, new Avalonia.Thickness(left, top, right, bottom));


 // CornerRadius

/*ValueStyleSetterGenerator*/
public static Style<T> CornerRadius<T>(this Style<T> style, Avalonia.CornerRadius value) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.CornerRadiusProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CornerRadius<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
=> style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.CornerRadiusProperty, binding);

/*ValueOverloadsStyleSetterGenerator*/
public static Style<T> CornerRadius<T>(this Style<T> style, System.Double uniformRadius) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.CornerRadiusProperty, new Avalonia.CornerRadius(uniformRadius));public static Style<T> CornerRadius<T>(this Style<T> style, System.Double top, System.Double bottom) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.CornerRadiusProperty, new Avalonia.CornerRadius(top, bottom));public static Style<T> CornerRadius<T>(this Style<T> style, System.Double topLeft, System.Double topRight, System.Double bottomRight, System.Double bottomLeft) where T : FluentAvalonia.UI.Controls.ColorPaletteItem 
   => style._addSetter(FluentAvalonia.UI.Controls.ColorPaletteItem.CornerRadiusProperty, new Avalonia.CornerRadius(topLeft, topRight, bottomRight, bottomLeft));



}
