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
public static partial class UniformGridLayout_MarkupExtensions
{
//================= Properties ======================//
 // Orientation

/*ValueSetterGenerator*/
public static T Orientation<T>(this T control, Avalonia.Layout.Orientation value) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
=> control._set(() => control.Orientation = value!);

/*BindFromExpressionSetterGenerator*/
public static T Orientation<T>(this T control, Func<Avalonia.Layout.Orientation> func, Action<Avalonia.Layout.Orientation>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.OrientationProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T Orientation<T>(this T control,Avalonia.Layout.Orientation value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
=> control._setEx(FluentAvalonia.UI.Controls.UniformGridLayout.OrientationProperty, ps, () => control.Orientation = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Orientation<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.OrientationProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Orientation<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.OrientationProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T Orientation<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Layout.Orientation> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
=> control._setEx(FluentAvalonia.UI.Controls.UniformGridLayout.OrientationProperty, ps, () => control.Orientation = converter.TryConvert(value)!, bindingMode, converter, bindingSource);


 // MinItemWidth

/*ValueSetterGenerator*/
public static T MinItemWidth<T>(this T control, System.Double value) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
=> control._set(() => control.MinItemWidth = value!);

/*BindFromExpressionSetterGenerator*/
public static T MinItemWidth<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.MinItemWidthProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T MinItemWidth<T>(this T control,System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
=> control._setEx(FluentAvalonia.UI.Controls.UniformGridLayout.MinItemWidthProperty, ps, () => control.MinItemWidth = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T MinItemWidth<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.MinItemWidthProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T MinItemWidth<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.MinItemWidthProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T MinItemWidth<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
=> control._setEx(FluentAvalonia.UI.Controls.UniformGridLayout.MinItemWidthProperty, ps, () => control.MinItemWidth = converter.TryConvert(value)!, bindingMode, converter, bindingSource);


 // MinItemHeight

/*ValueSetterGenerator*/
public static T MinItemHeight<T>(this T control, System.Double value) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
=> control._set(() => control.MinItemHeight = value!);

/*BindFromExpressionSetterGenerator*/
public static T MinItemHeight<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.MinItemHeightProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T MinItemHeight<T>(this T control,System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
=> control._setEx(FluentAvalonia.UI.Controls.UniformGridLayout.MinItemHeightProperty, ps, () => control.MinItemHeight = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T MinItemHeight<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.MinItemHeightProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T MinItemHeight<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.MinItemHeightProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T MinItemHeight<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
=> control._setEx(FluentAvalonia.UI.Controls.UniformGridLayout.MinItemHeightProperty, ps, () => control.MinItemHeight = converter.TryConvert(value)!, bindingMode, converter, bindingSource);


 // MinRowSpacing

/*ValueSetterGenerator*/
public static T MinRowSpacing<T>(this T control, System.Double value) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
=> control._set(() => control.MinRowSpacing = value!);

/*BindFromExpressionSetterGenerator*/
public static T MinRowSpacing<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.MinRowSpacingProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T MinRowSpacing<T>(this T control,System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
=> control._setEx(FluentAvalonia.UI.Controls.UniformGridLayout.MinRowSpacingProperty, ps, () => control.MinRowSpacing = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T MinRowSpacing<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.MinRowSpacingProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T MinRowSpacing<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.MinRowSpacingProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T MinRowSpacing<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
=> control._setEx(FluentAvalonia.UI.Controls.UniformGridLayout.MinRowSpacingProperty, ps, () => control.MinRowSpacing = converter.TryConvert(value)!, bindingMode, converter, bindingSource);


 // MinColumnSpacing

/*ValueSetterGenerator*/
public static T MinColumnSpacing<T>(this T control, System.Double value) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
=> control._set(() => control.MinColumnSpacing = value!);

/*BindFromExpressionSetterGenerator*/
public static T MinColumnSpacing<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.MinColumnSpacingProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T MinColumnSpacing<T>(this T control,System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
=> control._setEx(FluentAvalonia.UI.Controls.UniformGridLayout.MinColumnSpacingProperty, ps, () => control.MinColumnSpacing = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T MinColumnSpacing<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.MinColumnSpacingProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T MinColumnSpacing<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.MinColumnSpacingProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T MinColumnSpacing<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
=> control._setEx(FluentAvalonia.UI.Controls.UniformGridLayout.MinColumnSpacingProperty, ps, () => control.MinColumnSpacing = converter.TryConvert(value)!, bindingMode, converter, bindingSource);


 // ItemsJustification

/*ValueSetterGenerator*/
public static T ItemsJustification<T>(this T control, FluentAvalonia.UI.Controls.UniformGridLayoutItemsJustification value) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
=> control._set(() => control.ItemsJustification = value!);

/*BindFromExpressionSetterGenerator*/
public static T ItemsJustification<T>(this T control, Func<FluentAvalonia.UI.Controls.UniformGridLayoutItemsJustification> func, Action<FluentAvalonia.UI.Controls.UniformGridLayoutItemsJustification>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.ItemsJustificationProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T ItemsJustification<T>(this T control,FluentAvalonia.UI.Controls.UniformGridLayoutItemsJustification value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
=> control._setEx(FluentAvalonia.UI.Controls.UniformGridLayout.ItemsJustificationProperty, ps, () => control.ItemsJustification = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ItemsJustification<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.ItemsJustificationProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ItemsJustification<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.ItemsJustificationProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T ItemsJustification<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.UniformGridLayoutItemsJustification> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
=> control._setEx(FluentAvalonia.UI.Controls.UniformGridLayout.ItemsJustificationProperty, ps, () => control.ItemsJustification = converter.TryConvert(value)!, bindingMode, converter, bindingSource);


 // ItemsStretch

/*ValueSetterGenerator*/
public static T ItemsStretch<T>(this T control, FluentAvalonia.UI.Controls.UniformGridLayoutItemsStretch value) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
=> control._set(() => control.ItemsStretch = value!);

/*BindFromExpressionSetterGenerator*/
public static T ItemsStretch<T>(this T control, Func<FluentAvalonia.UI.Controls.UniformGridLayoutItemsStretch> func, Action<FluentAvalonia.UI.Controls.UniformGridLayoutItemsStretch>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.ItemsStretchProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T ItemsStretch<T>(this T control,FluentAvalonia.UI.Controls.UniformGridLayoutItemsStretch value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
=> control._setEx(FluentAvalonia.UI.Controls.UniformGridLayout.ItemsStretchProperty, ps, () => control.ItemsStretch = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ItemsStretch<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.ItemsStretchProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ItemsStretch<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.ItemsStretchProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T ItemsStretch<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.UniformGridLayoutItemsStretch> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
=> control._setEx(FluentAvalonia.UI.Controls.UniformGridLayout.ItemsStretchProperty, ps, () => control.ItemsStretch = converter.TryConvert(value)!, bindingMode, converter, bindingSource);


 // MaximumRowsOrColumns

/*ValueSetterGenerator*/
public static T MaximumRowsOrColumns<T>(this T control, System.Int32 value) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
=> control._set(() => control.MaximumRowsOrColumns = value!);

/*BindFromExpressionSetterGenerator*/
public static T MaximumRowsOrColumns<T>(this T control, Func<System.Int32> func, Action<System.Int32>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.MaximumRowsOrColumnsProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T MaximumRowsOrColumns<T>(this T control,System.Int32 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
=> control._setEx(FluentAvalonia.UI.Controls.UniformGridLayout.MaximumRowsOrColumnsProperty, ps, () => control.MaximumRowsOrColumns = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T MaximumRowsOrColumns<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.MaximumRowsOrColumnsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T MaximumRowsOrColumns<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
   => control._set(FluentAvalonia.UI.Controls.UniformGridLayout.MaximumRowsOrColumnsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T MaximumRowsOrColumns<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Int32> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : FluentAvalonia.UI.Controls.UniformGridLayout 
=> control._setEx(FluentAvalonia.UI.Controls.UniformGridLayout.MaximumRowsOrColumnsProperty, ps, () => control.MaximumRowsOrColumns = converter.TryConvert(value)!, bindingMode, converter, bindingSource);



}
