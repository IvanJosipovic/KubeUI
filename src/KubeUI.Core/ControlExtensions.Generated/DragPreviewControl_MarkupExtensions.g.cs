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
public static partial class DragPreviewControl_MarkupExtensions
{
//================= Properties ======================//
 // Title

/*BindFromExpressionSetterGenerator*/
public static T Title<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.DragPreviewControl 
   => control._set(Dock.Avalonia.Controls.DragPreviewControl.TitleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Title<T>(this T control,System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DragPreviewControl 
=> control._setEx(Dock.Avalonia.Controls.DragPreviewControl.TitleProperty, ps, () => control.Title = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Title<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.DragPreviewControl 
   => control._set(Dock.Avalonia.Controls.DragPreviewControl.TitleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Title<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.DragPreviewControl 
   => control._set(Dock.Avalonia.Controls.DragPreviewControl.TitleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Title<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DragPreviewControl 
=> control._setEx(Dock.Avalonia.Controls.DragPreviewControl.TitleProperty, ps, () => control.Title = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // Status

/*BindFromExpressionSetterGenerator*/
public static T Status<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.DragPreviewControl 
   => control._set(Dock.Avalonia.Controls.DragPreviewControl.StatusProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Status<T>(this T control,System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DragPreviewControl 
=> control._setEx(Dock.Avalonia.Controls.DragPreviewControl.StatusProperty, ps, () => control.Status = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Status<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.DragPreviewControl 
   => control._set(Dock.Avalonia.Controls.DragPreviewControl.StatusProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Status<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.DragPreviewControl 
   => control._set(Dock.Avalonia.Controls.DragPreviewControl.StatusProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Status<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DragPreviewControl 
=> control._setEx(Dock.Avalonia.Controls.DragPreviewControl.StatusProperty, ps, () => control.Status = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Styles ======================//
 // Title

/*ValueStyleSetterGenerator*/
public static Style<T> Title<T>(this Style<T> style, System.String value) where T : Dock.Avalonia.Controls.DragPreviewControl 
=> style._addSetter(Dock.Avalonia.Controls.DragPreviewControl.TitleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Title<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DragPreviewControl 
=> style._addSetter(Dock.Avalonia.Controls.DragPreviewControl.TitleProperty, binding);


 // Status

/*ValueStyleSetterGenerator*/
public static Style<T> Status<T>(this Style<T> style, System.String value) where T : Dock.Avalonia.Controls.DragPreviewControl 
=> style._addSetter(Dock.Avalonia.Controls.DragPreviewControl.StatusProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Status<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DragPreviewControl 
=> style._addSetter(Dock.Avalonia.Controls.DragPreviewControl.StatusProperty, binding);



}
