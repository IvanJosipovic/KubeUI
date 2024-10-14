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
public static partial class FlowLayout_MarkupExtensions
{
//================= Properties ======================//
 // LineAlignmentProperty

/*BindFromExpressionSetterGenerator*/
public static T LineAlignment<T>(this T control, Func<FluentAvalonia.UI.Controls.FlowLayoutLineAlignment> func, Action<FluentAvalonia.UI.Controls.FlowLayoutLineAlignment>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FlowLayout
   => control._set(FluentAvalonia.UI.Controls.FlowLayout.LineAlignmentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T LineAlignment<T>(this T control, FluentAvalonia.UI.Controls.FlowLayoutLineAlignment value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FlowLayout
=> control._setEx(FluentAvalonia.UI.Controls.FlowLayout.LineAlignmentProperty, ps, () => control.LineAlignment = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T LineAlignment<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FlowLayout
   => control._set(FluentAvalonia.UI.Controls.FlowLayout.LineAlignmentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T LineAlignment<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FlowLayout
   => control._set(FluentAvalonia.UI.Controls.FlowLayout.LineAlignmentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T LineAlignment<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.FlowLayoutLineAlignment> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FlowLayout
=> control._setEx(FluentAvalonia.UI.Controls.FlowLayout.LineAlignmentProperty, ps, () => control.LineAlignment = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // MinColumnSpacingProperty

/*BindFromExpressionSetterGenerator*/
public static T MinColumnSpacing<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FlowLayout
   => control._set(FluentAvalonia.UI.Controls.FlowLayout.MinColumnSpacingProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T MinColumnSpacing<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FlowLayout
=> control._setEx(FluentAvalonia.UI.Controls.FlowLayout.MinColumnSpacingProperty, ps, () => control.MinColumnSpacing = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T MinColumnSpacing<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FlowLayout
   => control._set(FluentAvalonia.UI.Controls.FlowLayout.MinColumnSpacingProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T MinColumnSpacing<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FlowLayout
   => control._set(FluentAvalonia.UI.Controls.FlowLayout.MinColumnSpacingProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T MinColumnSpacing<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FlowLayout
=> control._setEx(FluentAvalonia.UI.Controls.FlowLayout.MinColumnSpacingProperty, ps, () => control.MinColumnSpacing = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // MinRowSpacingProperty

/*BindFromExpressionSetterGenerator*/
public static T MinRowSpacing<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FlowLayout
   => control._set(FluentAvalonia.UI.Controls.FlowLayout.MinRowSpacingProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T MinRowSpacing<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FlowLayout
=> control._setEx(FluentAvalonia.UI.Controls.FlowLayout.MinRowSpacingProperty, ps, () => control.MinRowSpacing = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T MinRowSpacing<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FlowLayout
   => control._set(FluentAvalonia.UI.Controls.FlowLayout.MinRowSpacingProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T MinRowSpacing<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FlowLayout
   => control._set(FluentAvalonia.UI.Controls.FlowLayout.MinRowSpacingProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T MinRowSpacing<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FlowLayout
=> control._setEx(FluentAvalonia.UI.Controls.FlowLayout.MinRowSpacingProperty, ps, () => control.MinRowSpacing = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // OrientationProperty

/*BindFromExpressionSetterGenerator*/
public static T Orientation<T>(this T control, Func<Avalonia.Layout.Orientation> func, Action<Avalonia.Layout.Orientation>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FlowLayout
   => control._set(FluentAvalonia.UI.Controls.FlowLayout.OrientationProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Orientation<T>(this T control, Avalonia.Layout.Orientation value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FlowLayout
=> control._setEx(FluentAvalonia.UI.Controls.FlowLayout.OrientationProperty, ps, () => control.Orientation = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Orientation<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FlowLayout
   => control._set(FluentAvalonia.UI.Controls.FlowLayout.OrientationProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Orientation<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FlowLayout
   => control._set(FluentAvalonia.UI.Controls.FlowLayout.OrientationProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Orientation<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Layout.Orientation> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FlowLayout
=> control._setEx(FluentAvalonia.UI.Controls.FlowLayout.OrientationProperty, ps, () => control.Orientation = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//

}
