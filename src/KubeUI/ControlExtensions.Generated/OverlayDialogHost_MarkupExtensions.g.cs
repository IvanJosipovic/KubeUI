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
public static partial class OverlayDialogHost_MarkupExtensions
{
//================= Properties ======================//
 // IsInModalStatusProperty

/*BindFromExpressionSetterGenerator*/
public static T IsInModalStatus<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.OverlayDialogHost
   => control._set(Ursa.Controls.OverlayDialogHost.IsInModalStatusProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsInModalStatus<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.OverlayDialogHost
=> control._setEx(Ursa.Controls.OverlayDialogHost.IsInModalStatusProperty, ps, () => control.IsInModalStatus = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsInModalStatus<T>(this T control, IBinding binding) where T : Ursa.Controls.OverlayDialogHost
   => control._set(Ursa.Controls.OverlayDialogHost.IsInModalStatusProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsInModalStatus<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.OverlayDialogHost
   => control._set(Ursa.Controls.OverlayDialogHost.IsInModalStatusProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsInModalStatus<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.OverlayDialogHost
=> control._setEx(Ursa.Controls.OverlayDialogHost.IsInModalStatusProperty, ps, () => control.IsInModalStatus = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsModalStatusReporterProperty

/*BindFromExpressionSetterGenerator*/
public static T IsModalStatusReporter<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.OverlayDialogHost
   => control._set(Ursa.Controls.OverlayDialogHost.IsModalStatusReporterProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsModalStatusReporter<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.OverlayDialogHost
=> control._setEx(Ursa.Controls.OverlayDialogHost.IsModalStatusReporterProperty, ps, () => control.IsModalStatusReporter = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsModalStatusReporter<T>(this T control, IBinding binding) where T : Ursa.Controls.OverlayDialogHost
   => control._set(Ursa.Controls.OverlayDialogHost.IsModalStatusReporterProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsModalStatusReporter<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.OverlayDialogHost
   => control._set(Ursa.Controls.OverlayDialogHost.IsModalStatusReporterProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsModalStatusReporter<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.OverlayDialogHost
=> control._setEx(Ursa.Controls.OverlayDialogHost.IsModalStatusReporterProperty, ps, () => control.IsModalStatusReporter = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // OverlayMaskBrushProperty

/*BindFromExpressionSetterGenerator*/
public static T OverlayMaskBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.OverlayDialogHost
   => control._set(Ursa.Controls.OverlayDialogHost.OverlayMaskBrushProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T OverlayMaskBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.OverlayDialogHost
=> control._setEx(Ursa.Controls.OverlayDialogHost.OverlayMaskBrushProperty, ps, () => control.OverlayMaskBrush = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T OverlayMaskBrush<T>(this T control, IBinding binding) where T : Ursa.Controls.OverlayDialogHost
   => control._set(Ursa.Controls.OverlayDialogHost.OverlayMaskBrushProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T OverlayMaskBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.OverlayDialogHost
   => control._set(Ursa.Controls.OverlayDialogHost.OverlayMaskBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T OverlayMaskBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.OverlayDialogHost
=> control._setEx(Ursa.Controls.OverlayDialogHost.OverlayMaskBrushProperty, ps, () => control.OverlayMaskBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // IsInModalStatusProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsInModalStatus<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.OverlayDialogHost
=> style._addSetter(Ursa.Controls.OverlayDialogHost.IsInModalStatusProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsInModalStatus<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.OverlayDialogHost
=> style._addSetter(Ursa.Controls.OverlayDialogHost.IsInModalStatusProperty, binding);


 // IsModalStatusReporterProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsModalStatusReporter<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.OverlayDialogHost
=> style._addSetter(Ursa.Controls.OverlayDialogHost.IsModalStatusReporterProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsModalStatusReporter<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.OverlayDialogHost
=> style._addSetter(Ursa.Controls.OverlayDialogHost.IsModalStatusReporterProperty, binding);


 // OverlayMaskBrushProperty

/*ValueStyleSetterGenerator*/
public static Style<T> OverlayMaskBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Ursa.Controls.OverlayDialogHost
=> style._addSetter(Ursa.Controls.OverlayDialogHost.OverlayMaskBrushProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> OverlayMaskBrush<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.OverlayDialogHost
=> style._addSetter(Ursa.Controls.OverlayDialogHost.OverlayMaskBrushProperty, binding);



}
