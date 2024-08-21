#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class TextView_MarkupExtensions
{
//================= Properties ======================//
 // DocumentProperty

/*BindFromExpressionSetterGenerator*/
public static T Document<T>(this T control, Func<AvaloniaEdit.Document.TextDocument> func, Action<AvaloniaEdit.Document.TextDocument>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.DocumentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Document<T>(this T control, AvaloniaEdit.Document.TextDocument value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Rendering.TextView
=> control._setEx(AvaloniaEdit.Rendering.TextView.DocumentProperty, ps, () => control.Document = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Document<T>(this T control, IBinding binding) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.DocumentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Document<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.DocumentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Document<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, AvaloniaEdit.Document.TextDocument> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Rendering.TextView
=> control._setEx(AvaloniaEdit.Rendering.TextView.DocumentProperty, ps, () => control.Document = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // OptionsProperty

/*BindFromExpressionSetterGenerator*/
public static T Options<T>(this T control, Func<AvaloniaEdit.TextEditorOptions> func, Action<AvaloniaEdit.TextEditorOptions>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.OptionsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Options<T>(this T control, AvaloniaEdit.TextEditorOptions value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Rendering.TextView
=> control._setEx(AvaloniaEdit.Rendering.TextView.OptionsProperty, ps, () => control.Options = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Options<T>(this T control, IBinding binding) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.OptionsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Options<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.OptionsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Options<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, AvaloniaEdit.TextEditorOptions> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Rendering.TextView
=> control._setEx(AvaloniaEdit.Rendering.TextView.OptionsProperty, ps, () => control.Options = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // NonPrintableCharacterBrushProperty

/*BindFromExpressionSetterGenerator*/
public static T NonPrintableCharacterBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.NonPrintableCharacterBrushProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T NonPrintableCharacterBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Rendering.TextView
=> control._setEx(AvaloniaEdit.Rendering.TextView.NonPrintableCharacterBrushProperty, ps, () => control.NonPrintableCharacterBrush = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T NonPrintableCharacterBrush<T>(this T control, IBinding binding) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.NonPrintableCharacterBrushProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T NonPrintableCharacterBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.NonPrintableCharacterBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T NonPrintableCharacterBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Rendering.TextView
=> control._setEx(AvaloniaEdit.Rendering.TextView.NonPrintableCharacterBrushProperty, ps, () => control.NonPrintableCharacterBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // LinkTextForegroundBrushProperty

/*BindFromExpressionSetterGenerator*/
public static T LinkTextForegroundBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.LinkTextForegroundBrushProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T LinkTextForegroundBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Rendering.TextView
=> control._setEx(AvaloniaEdit.Rendering.TextView.LinkTextForegroundBrushProperty, ps, () => control.LinkTextForegroundBrush = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T LinkTextForegroundBrush<T>(this T control, IBinding binding) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.LinkTextForegroundBrushProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T LinkTextForegroundBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.LinkTextForegroundBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T LinkTextForegroundBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Rendering.TextView
=> control._setEx(AvaloniaEdit.Rendering.TextView.LinkTextForegroundBrushProperty, ps, () => control.LinkTextForegroundBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // LinkTextBackgroundBrushProperty

/*BindFromExpressionSetterGenerator*/
public static T LinkTextBackgroundBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.LinkTextBackgroundBrushProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T LinkTextBackgroundBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Rendering.TextView
=> control._setEx(AvaloniaEdit.Rendering.TextView.LinkTextBackgroundBrushProperty, ps, () => control.LinkTextBackgroundBrush = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T LinkTextBackgroundBrush<T>(this T control, IBinding binding) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.LinkTextBackgroundBrushProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T LinkTextBackgroundBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.LinkTextBackgroundBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T LinkTextBackgroundBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Rendering.TextView
=> control._setEx(AvaloniaEdit.Rendering.TextView.LinkTextBackgroundBrushProperty, ps, () => control.LinkTextBackgroundBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // LinkTextUnderlineProperty

/*BindFromExpressionSetterGenerator*/
public static T LinkTextUnderline<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.LinkTextUnderlineProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T LinkTextUnderline<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Rendering.TextView
=> control._setEx(AvaloniaEdit.Rendering.TextView.LinkTextUnderlineProperty, ps, () => control.LinkTextUnderline = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T LinkTextUnderline<T>(this T control, IBinding binding) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.LinkTextUnderlineProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T LinkTextUnderline<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.LinkTextUnderlineProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T LinkTextUnderline<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Rendering.TextView
=> control._setEx(AvaloniaEdit.Rendering.TextView.LinkTextUnderlineProperty, ps, () => control.LinkTextUnderline = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ColumnRulerPenProperty

/*BindFromExpressionSetterGenerator*/
public static T ColumnRulerPen<T>(this T control, Func<Avalonia.Media.IPen> func, Action<Avalonia.Media.IPen>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.ColumnRulerPenProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ColumnRulerPen<T>(this T control, Avalonia.Media.IPen value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Rendering.TextView
=> control._setEx(AvaloniaEdit.Rendering.TextView.ColumnRulerPenProperty, ps, () => control.ColumnRulerPen = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ColumnRulerPen<T>(this T control, IBinding binding) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.ColumnRulerPenProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ColumnRulerPen<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.ColumnRulerPenProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ColumnRulerPen<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IPen> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Rendering.TextView
=> control._setEx(AvaloniaEdit.Rendering.TextView.ColumnRulerPenProperty, ps, () => control.ColumnRulerPen = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CurrentLineBackgroundProperty

/*BindFromExpressionSetterGenerator*/
public static T CurrentLineBackground<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.CurrentLineBackgroundProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CurrentLineBackground<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Rendering.TextView
=> control._setEx(AvaloniaEdit.Rendering.TextView.CurrentLineBackgroundProperty, ps, () => control.CurrentLineBackground = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CurrentLineBackground<T>(this T control, IBinding binding) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.CurrentLineBackgroundProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CurrentLineBackground<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.CurrentLineBackgroundProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CurrentLineBackground<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Rendering.TextView
=> control._setEx(AvaloniaEdit.Rendering.TextView.CurrentLineBackgroundProperty, ps, () => control.CurrentLineBackground = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CurrentLineBorderProperty

/*BindFromExpressionSetterGenerator*/
public static T CurrentLineBorder<T>(this T control, Func<Avalonia.Media.IPen> func, Action<Avalonia.Media.IPen>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.CurrentLineBorderProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CurrentLineBorder<T>(this T control, Avalonia.Media.IPen value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Rendering.TextView
=> control._setEx(AvaloniaEdit.Rendering.TextView.CurrentLineBorderProperty, ps, () => control.CurrentLineBorder = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CurrentLineBorder<T>(this T control, IBinding binding) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.CurrentLineBorderProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CurrentLineBorder<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Rendering.TextView
   => control._set(AvaloniaEdit.Rendering.TextView.CurrentLineBorderProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CurrentLineBorder<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IPen> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Rendering.TextView
=> control._setEx(AvaloniaEdit.Rendering.TextView.CurrentLineBorderProperty, ps, () => control.CurrentLineBorder = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // DocumentChanged

/*ActionToEventGenerator*/
    public static T OnDocumentChanged<T>(this T control, Action<AvaloniaEdit.Document.DocumentChangedEventArgs> action) where T : AvaloniaEdit.Rendering.TextView => 
        control._setEvent((System.EventHandler<AvaloniaEdit.Document.DocumentChangedEventArgs>) ((arg0, arg1) => action(arg1)), h => control.DocumentChanged += h);


 // OptionChanged

/*ActionToEventGenerator*/
    public static T OnOptionChanged<T>(this T control, Action<System.ComponentModel.PropertyChangedEventArgs> action) where T : AvaloniaEdit.Rendering.TextView => 
        control._setEvent((System.ComponentModel.PropertyChangedEventHandler) ((arg0, arg1) => action(arg1)), h => control.OptionChanged += h);


 // VisualLineConstructionStarting

/*ActionToEventGenerator*/
    public static T OnVisualLineConstructionStarting<T>(this T control, Action<AvaloniaEdit.Rendering.VisualLineConstructionStartEventArgs> action) where T : AvaloniaEdit.Rendering.TextView => 
        control._setEvent((System.EventHandler<AvaloniaEdit.Rendering.VisualLineConstructionStartEventArgs>) ((arg0, arg1) => action(arg1)), h => control.VisualLineConstructionStarting += h);


 // VisualLinesChanged

/*ActionToEventGenerator*/
    public static T OnVisualLinesChanged<T>(this T control, Action<System.EventArgs> action) where T : AvaloniaEdit.Rendering.TextView => 
        control._setEvent((System.EventHandler) ((arg0, arg1) => action(arg1)), h => control.VisualLinesChanged += h);


 // ScrollOffsetChanged

/*ActionToEventGenerator*/
    public static T OnScrollOffsetChanged<T>(this T control, Action<System.EventArgs> action) where T : AvaloniaEdit.Rendering.TextView => 
        control._setEvent((System.EventHandler) ((arg0, arg1) => action(arg1)), h => control.ScrollOffsetChanged += h);


 // PreviewPointerHover

/*ActionToEventGenerator*/
    public static T OnPreviewPointerHover<T>(this T control, Action<Avalonia.Input.PointerEventArgs> action) where T : AvaloniaEdit.Rendering.TextView => 
        control._setEvent((System.EventHandler<Avalonia.Input.PointerEventArgs>) ((arg0, arg1) => action(arg1)), h => control.PreviewPointerHover += h);


 // PointerHover

/*ActionToEventGenerator*/
    public static T OnPointerHover<T>(this T control, Action<Avalonia.Input.PointerEventArgs> action) where T : AvaloniaEdit.Rendering.TextView => 
        control._setEvent((System.EventHandler<Avalonia.Input.PointerEventArgs>) ((arg0, arg1) => action(arg1)), h => control.PointerHover += h);


 // PreviewPointerHoverStopped

/*ActionToEventGenerator*/
    public static T OnPreviewPointerHoverStopped<T>(this T control, Action<Avalonia.Input.PointerEventArgs> action) where T : AvaloniaEdit.Rendering.TextView => 
        control._setEvent((System.EventHandler<Avalonia.Input.PointerEventArgs>) ((arg0, arg1) => action(arg1)), h => control.PreviewPointerHoverStopped += h);


 // PointerHoverStopped

/*ActionToEventGenerator*/
    public static T OnPointerHoverStopped<T>(this T control, Action<Avalonia.Input.PointerEventArgs> action) where T : AvaloniaEdit.Rendering.TextView => 
        control._setEvent((System.EventHandler<Avalonia.Input.PointerEventArgs>) ((arg0, arg1) => action(arg1)), h => control.PointerHoverStopped += h);



//================= Styles ======================//
 // DocumentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Document<T>(this Style<T> style, AvaloniaEdit.Document.TextDocument value) where T : AvaloniaEdit.Rendering.TextView
=> style._addSetter(AvaloniaEdit.Rendering.TextView.DocumentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Document<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Rendering.TextView
=> style._addSetter(AvaloniaEdit.Rendering.TextView.DocumentProperty, binding);


 // OptionsProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Options<T>(this Style<T> style, AvaloniaEdit.TextEditorOptions value) where T : AvaloniaEdit.Rendering.TextView
=> style._addSetter(AvaloniaEdit.Rendering.TextView.OptionsProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Options<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Rendering.TextView
=> style._addSetter(AvaloniaEdit.Rendering.TextView.OptionsProperty, binding);


 // NonPrintableCharacterBrushProperty

/*ValueStyleSetterGenerator*/
public static Style<T> NonPrintableCharacterBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : AvaloniaEdit.Rendering.TextView
=> style._addSetter(AvaloniaEdit.Rendering.TextView.NonPrintableCharacterBrushProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> NonPrintableCharacterBrush<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Rendering.TextView
=> style._addSetter(AvaloniaEdit.Rendering.TextView.NonPrintableCharacterBrushProperty, binding);


 // LinkTextForegroundBrushProperty

/*ValueStyleSetterGenerator*/
public static Style<T> LinkTextForegroundBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : AvaloniaEdit.Rendering.TextView
=> style._addSetter(AvaloniaEdit.Rendering.TextView.LinkTextForegroundBrushProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> LinkTextForegroundBrush<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Rendering.TextView
=> style._addSetter(AvaloniaEdit.Rendering.TextView.LinkTextForegroundBrushProperty, binding);


 // LinkTextBackgroundBrushProperty

/*ValueStyleSetterGenerator*/
public static Style<T> LinkTextBackgroundBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : AvaloniaEdit.Rendering.TextView
=> style._addSetter(AvaloniaEdit.Rendering.TextView.LinkTextBackgroundBrushProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> LinkTextBackgroundBrush<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Rendering.TextView
=> style._addSetter(AvaloniaEdit.Rendering.TextView.LinkTextBackgroundBrushProperty, binding);


 // LinkTextUnderlineProperty

/*ValueStyleSetterGenerator*/
public static Style<T> LinkTextUnderline<T>(this Style<T> style, System.Boolean value) where T : AvaloniaEdit.Rendering.TextView
=> style._addSetter(AvaloniaEdit.Rendering.TextView.LinkTextUnderlineProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> LinkTextUnderline<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Rendering.TextView
=> style._addSetter(AvaloniaEdit.Rendering.TextView.LinkTextUnderlineProperty, binding);


 // ColumnRulerPenProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ColumnRulerPen<T>(this Style<T> style, Avalonia.Media.IPen value) where T : AvaloniaEdit.Rendering.TextView
=> style._addSetter(AvaloniaEdit.Rendering.TextView.ColumnRulerPenProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ColumnRulerPen<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Rendering.TextView
=> style._addSetter(AvaloniaEdit.Rendering.TextView.ColumnRulerPenProperty, binding);


 // CurrentLineBackgroundProperty

/*ValueStyleSetterGenerator*/
public static Style<T> CurrentLineBackground<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : AvaloniaEdit.Rendering.TextView
=> style._addSetter(AvaloniaEdit.Rendering.TextView.CurrentLineBackgroundProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CurrentLineBackground<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Rendering.TextView
=> style._addSetter(AvaloniaEdit.Rendering.TextView.CurrentLineBackgroundProperty, binding);


 // CurrentLineBorderProperty

/*ValueStyleSetterGenerator*/
public static Style<T> CurrentLineBorder<T>(this Style<T> style, Avalonia.Media.IPen value) where T : AvaloniaEdit.Rendering.TextView
=> style._addSetter(AvaloniaEdit.Rendering.TextView.CurrentLineBorderProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CurrentLineBorder<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Rendering.TextView
=> style._addSetter(AvaloniaEdit.Rendering.TextView.CurrentLineBorderProperty, binding);



}
