#nullable enable
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Form = Ursa.Controls.Form;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Common;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class FormExtensions
{
public static T LabelWidth<T>(this T control, IBinding binding) where T : Ursa.Controls.Form
   => control._set(Ursa.Controls.Form.LabelWidthProperty, binding);
public static T LabelWidth<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Form
   => control._set(Ursa.Controls.Form.LabelWidthProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T LabelWidth<T>(this T control, Func<Avalonia.Controls.GridLength> func, Action<Avalonia.Controls.GridLength>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Form
   => control._set(Ursa.Controls.Form.LabelWidthProperty, func, onChanged, expression);
public static T LabelWidth<T>(this T control, Avalonia.Controls.GridLength value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Form
=> control._setEx(Ursa.Controls.Form.LabelWidthProperty, ps, () => control.LabelWidth = value, bindingMode, converter, bindingSource);
public static T LabelWidth<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.GridLength> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Form
=> control._setEx(Ursa.Controls.Form.LabelWidthProperty, ps, () => control.LabelWidth = converter.TryConvert(value), bindingMode, converter, bindingSource);

public static T LabelWidth<T>(this T control, Double value = default) where T : Ursa.Controls.Form
   => control._set(() => control.LabelWidth = new Avalonia.Controls.GridLength(value));
public static T LabelWidth<T>(this T control, Double value = default, GridUnitType type = default) where T : Ursa.Controls.Form
   => control._set(() => control.LabelWidth = new Avalonia.Controls.GridLength(value, type));
public static T LabelPosition<T>(this T control, IBinding binding) where T : Ursa.Controls.Form
   => control._set(Ursa.Controls.Form.LabelPositionProperty, binding);
public static T LabelPosition<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Form
   => control._set(Ursa.Controls.Form.LabelPositionProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T LabelPosition<T>(this T control, Func<Ursa.Common.Position> func, Action<Ursa.Common.Position>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Form
   => control._set(Ursa.Controls.Form.LabelPositionProperty, func, onChanged, expression);
public static T LabelPosition<T>(this T control, Ursa.Common.Position value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Form
=> control._setEx(Ursa.Controls.Form.LabelPositionProperty, ps, () => control.LabelPosition = value, bindingMode, converter, bindingSource);
public static T LabelPosition<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Ursa.Common.Position> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Form
=> control._setEx(Ursa.Controls.Form.LabelPositionProperty, ps, () => control.LabelPosition = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T LabelAlignment<T>(this T control, IBinding binding) where T : Ursa.Controls.Form
   => control._set(Ursa.Controls.Form.LabelAlignmentProperty, binding);
public static T LabelAlignment<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Form
   => control._set(Ursa.Controls.Form.LabelAlignmentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T LabelAlignment<T>(this T control, Func<Avalonia.Layout.HorizontalAlignment> func, Action<Avalonia.Layout.HorizontalAlignment>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Form
   => control._set(Ursa.Controls.Form.LabelAlignmentProperty, func, onChanged, expression);
public static T LabelAlignment<T>(this T control, Avalonia.Layout.HorizontalAlignment value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Form
=> control._setEx(Ursa.Controls.Form.LabelAlignmentProperty, ps, () => control.LabelAlignment = value, bindingMode, converter, bindingSource);
public static T LabelAlignment<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Layout.HorizontalAlignment> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Form
=> control._setEx(Ursa.Controls.Form.LabelAlignmentProperty, ps, () => control.LabelAlignment = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

