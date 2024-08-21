#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class BitmapIconSource_MarkupExtensions
{
//================= Properties ======================//
 // UriSourceProperty

/*BindFromExpressionSetterGenerator*/
public static T UriSource<T>(this T control, Func<System.Uri> func, Action<System.Uri>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.BitmapIconSource
   => control._set(FluentAvalonia.UI.Controls.BitmapIconSource.UriSourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T UriSource<T>(this T control, System.Uri value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.BitmapIconSource
=> control._setEx(FluentAvalonia.UI.Controls.BitmapIconSource.UriSourceProperty, ps, () => control.UriSource = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T UriSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.BitmapIconSource
   => control._set(FluentAvalonia.UI.Controls.BitmapIconSource.UriSourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T UriSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.BitmapIconSource
   => control._set(FluentAvalonia.UI.Controls.BitmapIconSource.UriSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T UriSource<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Uri> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.BitmapIconSource
=> control._setEx(FluentAvalonia.UI.Controls.BitmapIconSource.UriSourceProperty, ps, () => control.UriSource = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ShowAsMonochromeProperty

/*BindFromExpressionSetterGenerator*/
public static T ShowAsMonochrome<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.BitmapIconSource
   => control._set(FluentAvalonia.UI.Controls.BitmapIconSource.ShowAsMonochromeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ShowAsMonochrome<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.BitmapIconSource
=> control._setEx(FluentAvalonia.UI.Controls.BitmapIconSource.ShowAsMonochromeProperty, ps, () => control.ShowAsMonochrome = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ShowAsMonochrome<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.BitmapIconSource
   => control._set(FluentAvalonia.UI.Controls.BitmapIconSource.ShowAsMonochromeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ShowAsMonochrome<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.BitmapIconSource
   => control._set(FluentAvalonia.UI.Controls.BitmapIconSource.ShowAsMonochromeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ShowAsMonochrome<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.BitmapIconSource
=> control._setEx(FluentAvalonia.UI.Controls.BitmapIconSource.ShowAsMonochromeProperty, ps, () => control.ShowAsMonochrome = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // OnBitmapChanged

/*ActionToEventGenerator*/
    public static T OnOnBitmapChanged<T>(this T control, Action<System.Object, System.Object> action) where T : FluentAvalonia.UI.Controls.BitmapIconSource => 
        control._setEvent((System.EventHandler<System.Object>) ((arg0, arg1) => action(arg0, arg1)), h => control.OnBitmapChanged += h);



//================= Styles ======================//

}
