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
public static partial class TimelineItem_MarkupExtensions
{
//================= Properties ======================//
 // IconProperty

/*BindFromExpressionSetterGenerator*/
public static T Icon<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.IconProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Icon<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelineItem
=> control._setEx(Ursa.Controls.TimelineItem.IconProperty, ps, () => control.Icon = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Icon<T>(this T control, IBinding binding) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.IconProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Icon<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.IconProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Icon<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelineItem
=> control._setEx(Ursa.Controls.TimelineItem.IconProperty, ps, () => control.Icon = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IconTemplateProperty

/*BindFromExpressionSetterGenerator*/
public static T IconTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.IconTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IconTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelineItem
=> control._setEx(Ursa.Controls.TimelineItem.IconTemplateProperty, ps, () => control.IconTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IconTemplate<T>(this T control, IBinding binding) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.IconTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IconTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.IconTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IconTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelineItem
=> control._setEx(Ursa.Controls.TimelineItem.IconTemplateProperty, ps, () => control.IconTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TypeProperty

/*BindFromExpressionSetterGenerator*/
public static T Type<T>(this T control, Func<Ursa.Controls.TimelineItemType> func, Action<Ursa.Controls.TimelineItemType>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.TypeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Type<T>(this T control, Ursa.Controls.TimelineItemType value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelineItem
=> control._setEx(Ursa.Controls.TimelineItem.TypeProperty, ps, () => control.Type = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Type<T>(this T control, IBinding binding) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.TypeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Type<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.TypeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Type<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Ursa.Controls.TimelineItemType> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelineItem
=> control._setEx(Ursa.Controls.TimelineItem.TypeProperty, ps, () => control.Type = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // PositionProperty

/*BindFromExpressionSetterGenerator*/
public static T Position<T>(this T control, Func<Ursa.Controls.TimelineItemPosition> func, Action<Ursa.Controls.TimelineItemPosition>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.PositionProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Position<T>(this T control, Ursa.Controls.TimelineItemPosition value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelineItem
=> control._setEx(Ursa.Controls.TimelineItem.PositionProperty, ps, () => control.Position = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Position<T>(this T control, IBinding binding) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.PositionProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Position<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.PositionProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Position<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Ursa.Controls.TimelineItemPosition> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelineItem
=> control._setEx(Ursa.Controls.TimelineItem.PositionProperty, ps, () => control.Position = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // LeftWidthProperty

/*BindFromExpressionSetterGenerator*/
public static T LeftWidth<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.LeftWidthProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T LeftWidth<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelineItem
=> control._setEx(Ursa.Controls.TimelineItem.LeftWidthProperty, ps, () => control.LeftWidth = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T LeftWidth<T>(this T control, IBinding binding) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.LeftWidthProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T LeftWidth<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.LeftWidthProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T LeftWidth<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelineItem
=> control._setEx(Ursa.Controls.TimelineItem.LeftWidthProperty, ps, () => control.LeftWidth = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IconWidthProperty

/*BindFromExpressionSetterGenerator*/
public static T IconWidth<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.IconWidthProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IconWidth<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelineItem
=> control._setEx(Ursa.Controls.TimelineItem.IconWidthProperty, ps, () => control.IconWidth = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IconWidth<T>(this T control, IBinding binding) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.IconWidthProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IconWidth<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.IconWidthProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IconWidth<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelineItem
=> control._setEx(Ursa.Controls.TimelineItem.IconWidthProperty, ps, () => control.IconWidth = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // RightWidthProperty

/*BindFromExpressionSetterGenerator*/
public static T RightWidth<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.RightWidthProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T RightWidth<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelineItem
=> control._setEx(Ursa.Controls.TimelineItem.RightWidthProperty, ps, () => control.RightWidth = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T RightWidth<T>(this T control, IBinding binding) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.RightWidthProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T RightWidth<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.RightWidthProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T RightWidth<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelineItem
=> control._setEx(Ursa.Controls.TimelineItem.RightWidthProperty, ps, () => control.RightWidth = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TimeProperty

/*BindFromExpressionSetterGenerator*/
public static T Time<T>(this T control, Func<System.DateTime> func, Action<System.DateTime>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.TimeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Time<T>(this T control, System.DateTime value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelineItem
=> control._setEx(Ursa.Controls.TimelineItem.TimeProperty, ps, () => control.Time = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Time<T>(this T control, IBinding binding) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.TimeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Time<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.TimeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Time<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.DateTime> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelineItem
=> control._setEx(Ursa.Controls.TimelineItem.TimeProperty, ps, () => control.Time = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TimeFormatProperty

/*BindFromExpressionSetterGenerator*/
public static T TimeFormat<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.TimeFormatProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T TimeFormat<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelineItem
=> control._setEx(Ursa.Controls.TimelineItem.TimeFormatProperty, ps, () => control.TimeFormat = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TimeFormat<T>(this T control, IBinding binding) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.TimeFormatProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TimeFormat<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimelineItem
   => control._set(Ursa.Controls.TimelineItem.TimeFormatProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T TimeFormat<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelineItem
=> control._setEx(Ursa.Controls.TimelineItem.TimeFormatProperty, ps, () => control.TimeFormat = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // IconProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Icon<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.TimelineItem
=> style._addSetter(Ursa.Controls.TimelineItem.IconProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Icon<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimelineItem
=> style._addSetter(Ursa.Controls.TimelineItem.IconProperty, binding);


 // IconTemplateProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IconTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : Ursa.Controls.TimelineItem
=> style._addSetter(Ursa.Controls.TimelineItem.IconTemplateProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IconTemplate<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimelineItem
=> style._addSetter(Ursa.Controls.TimelineItem.IconTemplateProperty, binding);


 // TypeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Type<T>(this Style<T> style, Ursa.Controls.TimelineItemType value) where T : Ursa.Controls.TimelineItem
=> style._addSetter(Ursa.Controls.TimelineItem.TypeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Type<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimelineItem
=> style._addSetter(Ursa.Controls.TimelineItem.TypeProperty, binding);


 // PositionProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Position<T>(this Style<T> style, Ursa.Controls.TimelineItemPosition value) where T : Ursa.Controls.TimelineItem
=> style._addSetter(Ursa.Controls.TimelineItem.PositionProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Position<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimelineItem
=> style._addSetter(Ursa.Controls.TimelineItem.PositionProperty, binding);


 // TimeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Time<T>(this Style<T> style, System.DateTime value) where T : Ursa.Controls.TimelineItem
=> style._addSetter(Ursa.Controls.TimelineItem.TimeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Time<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimelineItem
=> style._addSetter(Ursa.Controls.TimelineItem.TimeProperty, binding);


 // TimeFormatProperty

/*ValueStyleSetterGenerator*/
public static Style<T> TimeFormat<T>(this Style<T> style, System.String value) where T : Ursa.Controls.TimelineItem
=> style._addSetter(Ursa.Controls.TimelineItem.TimeFormatProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> TimeFormat<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimelineItem
=> style._addSetter(Ursa.Controls.TimelineItem.TimeFormatProperty, binding);



}
