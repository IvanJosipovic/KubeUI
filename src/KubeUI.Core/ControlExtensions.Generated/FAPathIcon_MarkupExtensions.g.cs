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
public static partial class FAPathIcon_MarkupExtensions
{
//================= Properties ======================//
 // Data

/*BindFromExpressionSetterGenerator*/
public static T Data<T>(this T control, Func<Avalonia.Media.Geometry> func, Action<Avalonia.Media.Geometry>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAPathIcon 
   => control._set(FluentAvalonia.UI.Controls.FAPathIcon.DataProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Data<T>(this T control,Avalonia.Media.Geometry value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAPathIcon 
=> control._setEx(FluentAvalonia.UI.Controls.FAPathIcon.DataProperty, ps, () => control.Data = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Data<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAPathIcon 
   => control._set(FluentAvalonia.UI.Controls.FAPathIcon.DataProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Data<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAPathIcon 
   => control._set(FluentAvalonia.UI.Controls.FAPathIcon.DataProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Data<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.Geometry> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAPathIcon 
=> control._setEx(FluentAvalonia.UI.Controls.FAPathIcon.DataProperty, ps, () => control.Data = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // Stretch

/*BindFromExpressionSetterGenerator*/
public static T Stretch<T>(this T control, Func<Avalonia.Media.Stretch> func, Action<Avalonia.Media.Stretch>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAPathIcon 
   => control._set(FluentAvalonia.UI.Controls.FAPathIcon.StretchProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Stretch<T>(this T control,Avalonia.Media.Stretch value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAPathIcon 
=> control._setEx(FluentAvalonia.UI.Controls.FAPathIcon.StretchProperty, ps, () => control.Stretch = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Stretch<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAPathIcon 
   => control._set(FluentAvalonia.UI.Controls.FAPathIcon.StretchProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Stretch<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAPathIcon 
   => control._set(FluentAvalonia.UI.Controls.FAPathIcon.StretchProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Stretch<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.Stretch> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAPathIcon 
=> control._setEx(FluentAvalonia.UI.Controls.FAPathIcon.StretchProperty, ps, () => control.Stretch = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // StretchDirection

/*BindFromExpressionSetterGenerator*/
public static T StretchDirection<T>(this T control, Func<Avalonia.Media.StretchDirection> func, Action<Avalonia.Media.StretchDirection>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.FAPathIcon 
   => control._set(FluentAvalonia.UI.Controls.FAPathIcon.StretchDirectionProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T StretchDirection<T>(this T control,Avalonia.Media.StretchDirection value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAPathIcon 
=> control._setEx(FluentAvalonia.UI.Controls.FAPathIcon.StretchDirectionProperty, ps, () => control.StretchDirection = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T StretchDirection<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.FAPathIcon 
   => control._set(FluentAvalonia.UI.Controls.FAPathIcon.StretchDirectionProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T StretchDirection<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.FAPathIcon 
   => control._set(FluentAvalonia.UI.Controls.FAPathIcon.StretchDirectionProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T StretchDirection<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.StretchDirection> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.FAPathIcon 
=> control._setEx(FluentAvalonia.UI.Controls.FAPathIcon.StretchDirectionProperty, ps, () => control.StretchDirection = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Styles ======================//
 // Data

/*ValueStyleSetterGenerator*/
public static Style<T> Data<T>(this Style<T> style, Avalonia.Media.Geometry value) where T : FluentAvalonia.UI.Controls.FAPathIcon 
=> style._addSetter(FluentAvalonia.UI.Controls.FAPathIcon.DataProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Data<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAPathIcon 
=> style._addSetter(FluentAvalonia.UI.Controls.FAPathIcon.DataProperty, binding);


 // Stretch

/*ValueStyleSetterGenerator*/
public static Style<T> Stretch<T>(this Style<T> style, Avalonia.Media.Stretch value) where T : FluentAvalonia.UI.Controls.FAPathIcon 
=> style._addSetter(FluentAvalonia.UI.Controls.FAPathIcon.StretchProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Stretch<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAPathIcon 
=> style._addSetter(FluentAvalonia.UI.Controls.FAPathIcon.StretchProperty, binding);


 // StretchDirection

/*ValueStyleSetterGenerator*/
public static Style<T> StretchDirection<T>(this Style<T> style, Avalonia.Media.StretchDirection value) where T : FluentAvalonia.UI.Controls.FAPathIcon 
=> style._addSetter(FluentAvalonia.UI.Controls.FAPathIcon.StretchDirectionProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> StretchDirection<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAPathIcon 
=> style._addSetter(FluentAvalonia.UI.Controls.FAPathIcon.StretchDirectionProperty, binding);



}
