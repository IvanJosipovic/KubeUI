#nullable enable
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TimelineItem = Ursa.Controls.TimelineItem;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class TimelineItemExtensions
{
public static T Icon<T>(this T control, IBinding binding) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.IconProperty, binding);
public static T Icon<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.IconProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Icon<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.IconProperty, func, onChanged, expression);
public static T Icon<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelineItem
=> control._setEx(Ursa.Controls.TimelineItem.IconProperty, ps, () => control.Icon = value, bindingMode, converter, bindingSource);
public static T Icon<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelineItem
=> control._setEx(Ursa.Controls.TimelineItem.IconProperty, ps, () => control.Icon = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IconTemplate<T>(this T control, IBinding binding) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.IconTemplateProperty, binding);
public static T IconTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.IconTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IconTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.IconTemplateProperty, func, onChanged, expression);
public static T IconTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelineItem
=> control._setEx(Ursa.Controls.TimelineItem.IconTemplateProperty, ps, () => control.IconTemplate = value, bindingMode, converter, bindingSource);
public static T IconTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelineItem
=> control._setEx(Ursa.Controls.TimelineItem.IconTemplateProperty, ps, () => control.IconTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Type<T>(this T control, IBinding binding) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.TypeProperty, binding);
public static T Type<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.TypeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Type<T>(this T control, Func<Ursa.Controls.TimelineItemType> func, Action<Ursa.Controls.TimelineItemType>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.TypeProperty, func, onChanged, expression);
public static T Type<T>(this T control, Ursa.Controls.TimelineItemType value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelineItem
=> control._setEx(Ursa.Controls.TimelineItem.TypeProperty, ps, () => control.Type = value, bindingMode, converter, bindingSource);
public static T Type<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Ursa.Controls.TimelineItemType> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelineItem
=> control._setEx(Ursa.Controls.TimelineItem.TypeProperty, ps, () => control.Type = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Position<T>(this T control, IBinding binding) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.PositionProperty, binding);
public static T Position<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.PositionProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Position<T>(this T control, Func<Ursa.Controls.TimelineItemPosition> func, Action<Ursa.Controls.TimelineItemPosition>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.PositionProperty, func, onChanged, expression);
public static T Position<T>(this T control, Ursa.Controls.TimelineItemPosition value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelineItem
=> control._setEx(Ursa.Controls.TimelineItem.PositionProperty, ps, () => control.Position = value, bindingMode, converter, bindingSource);
public static T Position<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Ursa.Controls.TimelineItemPosition> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelineItem
=> control._setEx(Ursa.Controls.TimelineItem.PositionProperty, ps, () => control.Position = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T LeftWidth<T>(this T control, IBinding binding) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.LeftWidthProperty, binding);
public static T LeftWidth<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.LeftWidthProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T LeftWidth<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.LeftWidthProperty, func, onChanged, expression);
public static T LeftWidth<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelineItem
=> control._setEx(Ursa.Controls.TimelineItem.LeftWidthProperty, ps, () => control.LeftWidth = value, bindingMode, converter, bindingSource);
public static T LeftWidth<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelineItem
=> control._setEx(Ursa.Controls.TimelineItem.LeftWidthProperty, ps, () => control.LeftWidth = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IconWidth<T>(this T control, IBinding binding) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.IconWidthProperty, binding);
public static T IconWidth<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.IconWidthProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IconWidth<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.IconWidthProperty, func, onChanged, expression);
public static T IconWidth<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelineItem
=> control._setEx(Ursa.Controls.TimelineItem.IconWidthProperty, ps, () => control.IconWidth = value, bindingMode, converter, bindingSource);
public static T IconWidth<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelineItem
=> control._setEx(Ursa.Controls.TimelineItem.IconWidthProperty, ps, () => control.IconWidth = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T RightWidth<T>(this T control, IBinding binding) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.RightWidthProperty, binding);
public static T RightWidth<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.RightWidthProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T RightWidth<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.RightWidthProperty, func, onChanged, expression);
public static T RightWidth<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelineItem
=> control._setEx(Ursa.Controls.TimelineItem.RightWidthProperty, ps, () => control.RightWidth = value, bindingMode, converter, bindingSource);
public static T RightWidth<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelineItem
=> control._setEx(Ursa.Controls.TimelineItem.RightWidthProperty, ps, () => control.RightWidth = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Time<T>(this T control, IBinding binding) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.TimeProperty, binding);
public static T Time<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.TimeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Time<T>(this T control, Func<System.DateTime> func, Action<System.DateTime>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.TimeProperty, func, onChanged, expression);
public static T Time<T>(this T control, System.DateTime value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelineItem
=> control._setEx(Ursa.Controls.TimelineItem.TimeProperty, ps, () => control.Time = value, bindingMode, converter, bindingSource);
public static T Time<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.DateTime> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelineItem
=> control._setEx(Ursa.Controls.TimelineItem.TimeProperty, ps, () => control.Time = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T TimeFormat<T>(this T control, IBinding binding) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.TimeFormatProperty, binding);
public static T TimeFormat<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.TimeFormatProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T TimeFormat<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.TimeFormatProperty, func, onChanged, expression);
public static T TimeFormat<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelineItem
=> control._setEx(Ursa.Controls.TimelineItem.TimeFormatProperty, ps, () => control.TimeFormat = value, bindingMode, converter, bindingSource);
public static T TimeFormat<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelineItem
=> control._setEx(Ursa.Controls.TimelineItem.TimeFormatProperty, ps, () => control.TimeFormat = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

