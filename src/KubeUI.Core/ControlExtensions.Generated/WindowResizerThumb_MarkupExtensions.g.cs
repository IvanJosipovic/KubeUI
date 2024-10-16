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
public static partial class WindowResizerThumb_MarkupExtensions
{
//================= Properties ======================//
 // ResizeDirection

/*BindFromExpressionSetterGenerator*/
public static T ResizeDirection<T>(this T control, Func<Ursa.Controls.ResizeDirection> func, Action<Ursa.Controls.ResizeDirection>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.WindowResizerThumb 
   => control._set(Ursa.Controls.WindowResizerThumb.ResizeDirectionProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ResizeDirection<T>(this T control,Ursa.Controls.ResizeDirection value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.WindowResizerThumb 
=> control._setEx(Ursa.Controls.WindowResizerThumb.ResizeDirectionProperty, ps, () => control.ResizeDirection = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ResizeDirection<T>(this T control, IBinding binding) where T : Ursa.Controls.WindowResizerThumb 
   => control._set(Ursa.Controls.WindowResizerThumb.ResizeDirectionProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ResizeDirection<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.WindowResizerThumb 
   => control._set(Ursa.Controls.WindowResizerThumb.ResizeDirectionProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ResizeDirection<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Ursa.Controls.ResizeDirection> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.WindowResizerThumb 
=> control._setEx(Ursa.Controls.WindowResizerThumb.ResizeDirectionProperty, ps, () => control.ResizeDirection = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Styles ======================//
 // ResizeDirection

/*ValueStyleSetterGenerator*/
public static Style<T> ResizeDirection<T>(this Style<T> style, Ursa.Controls.ResizeDirection value) where T : Ursa.Controls.WindowResizerThumb 
=> style._addSetter(Ursa.Controls.WindowResizerThumb.ResizeDirectionProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ResizeDirection<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.WindowResizerThumb 
=> style._addSetter(Ursa.Controls.WindowResizerThumb.ResizeDirectionProperty, binding);



}