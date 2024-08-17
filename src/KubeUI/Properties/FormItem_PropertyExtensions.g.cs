#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using FormItem = Ursa.Controls.FormItem;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class FormItemExtensions
{
public static T LabelWidth<T>(this T control, IBinding binding) where T : Ursa.Controls.FormItem
   => control._set(Ursa.Controls.FormItem.LabelWidthProperty, binding);
public static T LabelWidth<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.FormItem
   => control._set(Ursa.Controls.FormItem.LabelWidthProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T LabelWidth<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.FormItem
   => control._set(Ursa.Controls.FormItem.LabelWidthProperty, func, onChanged, expression);
public static T LabelWidth<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.FormItem
=> control._setEx(Ursa.Controls.FormItem.LabelWidthProperty, ps, () => control.LabelWidth = value, bindingMode, converter, bindingSource);
public static T LabelWidth<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.FormItem
=> control._setEx(Ursa.Controls.FormItem.LabelWidthProperty, ps, () => control.LabelWidth = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T LabelAlignment<T>(this T control, IBinding binding) where T : Ursa.Controls.FormItem
   => control._set(Ursa.Controls.FormItem.LabelAlignmentProperty, binding);
public static T LabelAlignment<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.FormItem
   => control._set(Ursa.Controls.FormItem.LabelAlignmentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T LabelAlignment<T>(this T control, Func<Avalonia.Layout.HorizontalAlignment> func, Action<Avalonia.Layout.HorizontalAlignment>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.FormItem
   => control._set(Ursa.Controls.FormItem.LabelAlignmentProperty, func, onChanged, expression);
public static T LabelAlignment<T>(this T control, Avalonia.Layout.HorizontalAlignment value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.FormItem
=> control._setEx(Ursa.Controls.FormItem.LabelAlignmentProperty, ps, () => control.LabelAlignment = value, bindingMode, converter, bindingSource);
public static T LabelAlignment<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Layout.HorizontalAlignment> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.FormItem
=> control._setEx(Ursa.Controls.FormItem.LabelAlignmentProperty, ps, () => control.LabelAlignment = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

