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
public static partial class BitmapIcon_MarkupExtensions
{
//================= Properties ======================//
 // UriSource

/*BindFromExpressionSetterGenerator*/
public static T UriSource<T>(this T control, Func<System.Uri> func, Action<System.Uri>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.BitmapIcon 
   => control._set(FluentAvalonia.UI.Controls.BitmapIcon.UriSourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T UriSource<T>(this T control,System.Uri value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.BitmapIcon 
=> control._setEx(FluentAvalonia.UI.Controls.BitmapIcon.UriSourceProperty, ps, () => control.UriSource = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T UriSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.BitmapIcon 
   => control._set(FluentAvalonia.UI.Controls.BitmapIcon.UriSourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T UriSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.BitmapIcon 
   => control._set(FluentAvalonia.UI.Controls.BitmapIcon.UriSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T UriSource<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Uri> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.BitmapIcon 
=> control._setEx(FluentAvalonia.UI.Controls.BitmapIcon.UriSourceProperty, ps, () => control.UriSource = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ShowAsMonochrome

/*BindFromExpressionSetterGenerator*/
public static T ShowAsMonochrome<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.BitmapIcon 
   => control._set(FluentAvalonia.UI.Controls.BitmapIcon.ShowAsMonochromeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ShowAsMonochrome<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.BitmapIcon 
=> control._setEx(FluentAvalonia.UI.Controls.BitmapIcon.ShowAsMonochromeProperty, ps, () => control.ShowAsMonochrome = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ShowAsMonochrome<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.BitmapIcon 
   => control._set(FluentAvalonia.UI.Controls.BitmapIcon.ShowAsMonochromeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ShowAsMonochrome<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.BitmapIcon 
   => control._set(FluentAvalonia.UI.Controls.BitmapIcon.ShowAsMonochromeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ShowAsMonochrome<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.BitmapIcon 
=> control._setEx(FluentAvalonia.UI.Controls.BitmapIcon.ShowAsMonochromeProperty, ps, () => control.ShowAsMonochrome = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Styles ======================//
 // UriSource

/*ValueStyleSetterGenerator*/
public static Style<T> UriSource<T>(this Style<T> style, System.Uri value) where T : FluentAvalonia.UI.Controls.BitmapIcon 
=> style._addSetter(FluentAvalonia.UI.Controls.BitmapIcon.UriSourceProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> UriSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.BitmapIcon 
=> style._addSetter(FluentAvalonia.UI.Controls.BitmapIcon.UriSourceProperty, binding);


 // ShowAsMonochrome

/*ValueStyleSetterGenerator*/
public static Style<T> ShowAsMonochrome<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.BitmapIcon 
=> style._addSetter(FluentAvalonia.UI.Controls.BitmapIcon.ShowAsMonochromeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ShowAsMonochrome<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.BitmapIcon 
=> style._addSetter(FluentAvalonia.UI.Controls.BitmapIcon.ShowAsMonochromeProperty, binding);



}
