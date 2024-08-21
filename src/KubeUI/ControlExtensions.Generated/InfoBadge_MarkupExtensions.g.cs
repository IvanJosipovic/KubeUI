#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class InfoBadge_MarkupExtensions
{
//================= Properties ======================//
 // ValueProperty

/*BindFromExpressionSetterGenerator*/
public static T Value<T>(this T control, Func<System.Int32> func, Action<System.Int32>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.InfoBadge
   => control._set(FluentAvalonia.UI.Controls.InfoBadge.ValueProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Value<T>(this T control, System.Int32 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBadge
=> control._setEx(FluentAvalonia.UI.Controls.InfoBadge.ValueProperty, ps, () => control.Value = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Value<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBadge
   => control._set(FluentAvalonia.UI.Controls.InfoBadge.ValueProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Value<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.InfoBadge
   => control._set(FluentAvalonia.UI.Controls.InfoBadge.ValueProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Value<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Int32> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBadge
=> control._setEx(FluentAvalonia.UI.Controls.InfoBadge.ValueProperty, ps, () => control.Value = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IconSourceProperty

/*BindFromExpressionSetterGenerator*/
public static T IconSource<T>(this T control, Func<FluentAvalonia.UI.Controls.IconSource> func, Action<FluentAvalonia.UI.Controls.IconSource>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.InfoBadge
   => control._set(FluentAvalonia.UI.Controls.InfoBadge.IconSourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IconSource<T>(this T control, FluentAvalonia.UI.Controls.IconSource value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBadge
=> control._setEx(FluentAvalonia.UI.Controls.InfoBadge.IconSourceProperty, ps, () => control.IconSource = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IconSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBadge
   => control._set(FluentAvalonia.UI.Controls.InfoBadge.IconSourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IconSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.InfoBadge
   => control._set(FluentAvalonia.UI.Controls.InfoBadge.IconSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IconSource<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.IconSource> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBadge
=> control._setEx(FluentAvalonia.UI.Controls.InfoBadge.IconSourceProperty, ps, () => control.IconSource = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // ValueProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Value<T>(this Style<T> style, System.Int32 value) where T : FluentAvalonia.UI.Controls.InfoBadge
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBadge.ValueProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Value<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBadge
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBadge.ValueProperty, binding);


 // IconSourceProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IconSource<T>(this Style<T> style, FluentAvalonia.UI.Controls.IconSource value) where T : FluentAvalonia.UI.Controls.InfoBadge
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBadge.IconSourceProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IconSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBadge
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBadge.IconSourceProperty, binding);



}
