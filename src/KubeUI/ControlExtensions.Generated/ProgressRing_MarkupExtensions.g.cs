#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class ProgressRing_MarkupExtensions
{
//================= Properties ======================//
 // IsActiveProperty

/*BindFromExpressionSetterGenerator*/
public static T IsActive<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ProgressRing
   => control._set(FluentAvalonia.UI.Controls.ProgressRing.IsActiveProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsActive<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ProgressRing
=> control._setEx(FluentAvalonia.UI.Controls.ProgressRing.IsActiveProperty, ps, () => control.IsActive = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsActive<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ProgressRing
   => control._set(FluentAvalonia.UI.Controls.ProgressRing.IsActiveProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsActive<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ProgressRing
   => control._set(FluentAvalonia.UI.Controls.ProgressRing.IsActiveProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsActive<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ProgressRing
=> control._setEx(FluentAvalonia.UI.Controls.ProgressRing.IsActiveProperty, ps, () => control.IsActive = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsIndeterminateProperty

/*BindFromExpressionSetterGenerator*/
public static T IsIndeterminate<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ProgressRing
   => control._set(FluentAvalonia.UI.Controls.ProgressRing.IsIndeterminateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsIndeterminate<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ProgressRing
=> control._setEx(FluentAvalonia.UI.Controls.ProgressRing.IsIndeterminateProperty, ps, () => control.IsIndeterminate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsIndeterminate<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ProgressRing
   => control._set(FluentAvalonia.UI.Controls.ProgressRing.IsIndeterminateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsIndeterminate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ProgressRing
   => control._set(FluentAvalonia.UI.Controls.ProgressRing.IsIndeterminateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsIndeterminate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ProgressRing
=> control._setEx(FluentAvalonia.UI.Controls.ProgressRing.IsIndeterminateProperty, ps, () => control.IsIndeterminate = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // IsActiveProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsActive<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.ProgressRing
=> style._addSetter(FluentAvalonia.UI.Controls.ProgressRing.IsActiveProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsActive<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ProgressRing
=> style._addSetter(FluentAvalonia.UI.Controls.ProgressRing.IsActiveProperty, binding);


 // IsIndeterminateProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsIndeterminate<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.ProgressRing
=> style._addSetter(FluentAvalonia.UI.Controls.ProgressRing.IsIndeterminateProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsIndeterminate<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ProgressRing
=> style._addSetter(FluentAvalonia.UI.Controls.ProgressRing.IsIndeterminateProperty, binding);



}
