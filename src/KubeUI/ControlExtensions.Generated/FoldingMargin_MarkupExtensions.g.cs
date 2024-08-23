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
public static partial class FoldingMargin_MarkupExtensions
{
//================= Properties ======================//
 // FoldingMarkerBrushProperty

/*BindFromExpressionSetterGenerator*/
public static T FoldingMarkerBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Folding.FoldingMargin
   => control._set(AvaloniaEdit.Folding.FoldingMargin.FoldingMarkerBrushProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T FoldingMarkerBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Folding.FoldingMargin
=> control._setEx(AvaloniaEdit.Folding.FoldingMargin.FoldingMarkerBrushProperty, ps, () => control.FoldingMarkerBrush = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T FoldingMarkerBrush<T>(this T control, IBinding binding) where T : AvaloniaEdit.Folding.FoldingMargin
   => control._set(AvaloniaEdit.Folding.FoldingMargin.FoldingMarkerBrushProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T FoldingMarkerBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Folding.FoldingMargin
   => control._set(AvaloniaEdit.Folding.FoldingMargin.FoldingMarkerBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T FoldingMarkerBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Folding.FoldingMargin
=> control._setEx(AvaloniaEdit.Folding.FoldingMargin.FoldingMarkerBrushProperty, ps, () => control.FoldingMarkerBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // FoldingMarkerBackgroundBrushProperty

/*BindFromExpressionSetterGenerator*/
public static T FoldingMarkerBackgroundBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Folding.FoldingMargin
   => control._set(AvaloniaEdit.Folding.FoldingMargin.FoldingMarkerBackgroundBrushProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T FoldingMarkerBackgroundBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Folding.FoldingMargin
=> control._setEx(AvaloniaEdit.Folding.FoldingMargin.FoldingMarkerBackgroundBrushProperty, ps, () => control.FoldingMarkerBackgroundBrush = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T FoldingMarkerBackgroundBrush<T>(this T control, IBinding binding) where T : AvaloniaEdit.Folding.FoldingMargin
   => control._set(AvaloniaEdit.Folding.FoldingMargin.FoldingMarkerBackgroundBrushProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T FoldingMarkerBackgroundBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Folding.FoldingMargin
   => control._set(AvaloniaEdit.Folding.FoldingMargin.FoldingMarkerBackgroundBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T FoldingMarkerBackgroundBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Folding.FoldingMargin
=> control._setEx(AvaloniaEdit.Folding.FoldingMargin.FoldingMarkerBackgroundBrushProperty, ps, () => control.FoldingMarkerBackgroundBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SelectedFoldingMarkerBrushProperty

/*BindFromExpressionSetterGenerator*/
public static T SelectedFoldingMarkerBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Folding.FoldingMargin
   => control._set(AvaloniaEdit.Folding.FoldingMargin.SelectedFoldingMarkerBrushProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SelectedFoldingMarkerBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Folding.FoldingMargin
=> control._setEx(AvaloniaEdit.Folding.FoldingMargin.SelectedFoldingMarkerBrushProperty, ps, () => control.SelectedFoldingMarkerBrush = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SelectedFoldingMarkerBrush<T>(this T control, IBinding binding) where T : AvaloniaEdit.Folding.FoldingMargin
   => control._set(AvaloniaEdit.Folding.FoldingMargin.SelectedFoldingMarkerBrushProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SelectedFoldingMarkerBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Folding.FoldingMargin
   => control._set(AvaloniaEdit.Folding.FoldingMargin.SelectedFoldingMarkerBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SelectedFoldingMarkerBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Folding.FoldingMargin
=> control._setEx(AvaloniaEdit.Folding.FoldingMargin.SelectedFoldingMarkerBrushProperty, ps, () => control.SelectedFoldingMarkerBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SelectedFoldingMarkerBackgroundBrushProperty

/*BindFromExpressionSetterGenerator*/
public static T SelectedFoldingMarkerBackgroundBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Folding.FoldingMargin
   => control._set(AvaloniaEdit.Folding.FoldingMargin.SelectedFoldingMarkerBackgroundBrushProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SelectedFoldingMarkerBackgroundBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Folding.FoldingMargin
=> control._setEx(AvaloniaEdit.Folding.FoldingMargin.SelectedFoldingMarkerBackgroundBrushProperty, ps, () => control.SelectedFoldingMarkerBackgroundBrush = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SelectedFoldingMarkerBackgroundBrush<T>(this T control, IBinding binding) where T : AvaloniaEdit.Folding.FoldingMargin
   => control._set(AvaloniaEdit.Folding.FoldingMargin.SelectedFoldingMarkerBackgroundBrushProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SelectedFoldingMarkerBackgroundBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Folding.FoldingMargin
   => control._set(AvaloniaEdit.Folding.FoldingMargin.SelectedFoldingMarkerBackgroundBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SelectedFoldingMarkerBackgroundBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Folding.FoldingMargin
=> control._setEx(AvaloniaEdit.Folding.FoldingMargin.SelectedFoldingMarkerBackgroundBrushProperty, ps, () => control.SelectedFoldingMarkerBackgroundBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // FoldingMarkerBrushProperty

/*ValueStyleSetterGenerator*/
public static Style<T> FoldingMarkerBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : AvaloniaEdit.Folding.FoldingMargin
=> style._addSetter(AvaloniaEdit.Folding.FoldingMargin.FoldingMarkerBrushProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> FoldingMarkerBrush<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Folding.FoldingMargin
=> style._addSetter(AvaloniaEdit.Folding.FoldingMargin.FoldingMarkerBrushProperty, binding);


 // FoldingMarkerBackgroundBrushProperty

/*ValueStyleSetterGenerator*/
public static Style<T> FoldingMarkerBackgroundBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : AvaloniaEdit.Folding.FoldingMargin
=> style._addSetter(AvaloniaEdit.Folding.FoldingMargin.FoldingMarkerBackgroundBrushProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> FoldingMarkerBackgroundBrush<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Folding.FoldingMargin
=> style._addSetter(AvaloniaEdit.Folding.FoldingMargin.FoldingMarkerBackgroundBrushProperty, binding);


 // SelectedFoldingMarkerBrushProperty

/*ValueStyleSetterGenerator*/
public static Style<T> SelectedFoldingMarkerBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : AvaloniaEdit.Folding.FoldingMargin
=> style._addSetter(AvaloniaEdit.Folding.FoldingMargin.SelectedFoldingMarkerBrushProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SelectedFoldingMarkerBrush<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Folding.FoldingMargin
=> style._addSetter(AvaloniaEdit.Folding.FoldingMargin.SelectedFoldingMarkerBrushProperty, binding);


 // SelectedFoldingMarkerBackgroundBrushProperty

/*ValueStyleSetterGenerator*/
public static Style<T> SelectedFoldingMarkerBackgroundBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : AvaloniaEdit.Folding.FoldingMargin
=> style._addSetter(AvaloniaEdit.Folding.FoldingMargin.SelectedFoldingMarkerBackgroundBrushProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SelectedFoldingMarkerBackgroundBrush<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Folding.FoldingMargin
=> style._addSetter(AvaloniaEdit.Folding.FoldingMargin.SelectedFoldingMarkerBackgroundBrushProperty, binding);



}
