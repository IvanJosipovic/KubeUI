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
 // ContentTemplate

/*ValueSetterGenerator*/
public static T ContentTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value) where T : Dock.Avalonia.Controls.DragPreviewControl 
=> control._set(() => control.ContentTemplate = value!);

/*BindFromExpressionSetterGenerator*/
public static T ContentTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : Dock.Avalonia.Controls.DragPreviewControl 
   => control._set(Dock.Avalonia.Controls.DragPreviewControl.ContentTemplateProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T ContentTemplate<T>(this T control,Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.DragPreviewControl 
=> control._setEx(Dock.Avalonia.Controls.DragPreviewControl.ContentTemplateProperty, ps, () => control.ContentTemplate = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ContentTemplate<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.DragPreviewControl 
   => control._set(Dock.Avalonia.Controls.DragPreviewControl.ContentTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ContentTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.DragPreviewControl 
   => control._set(Dock.Avalonia.Controls.DragPreviewControl.ContentTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T ContentTemplate<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.DragPreviewControl 
=> control._setEx(Dock.Avalonia.Controls.DragPreviewControl.ContentTemplateProperty, ps, () => control.ContentTemplate = converter.TryConvert(value)!, bindingMode, converter, bindingSource);


 // Status

/*ValueSetterGenerator*/
public static T Status<T>(this T control, System.String value) where T : Dock.Avalonia.Controls.DragPreviewControl 
=> control._set(() => control.Status = value!);

/*BindFromExpressionSetterGenerator*/
public static T Status<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : Dock.Avalonia.Controls.DragPreviewControl 
   => control._set(Dock.Avalonia.Controls.DragPreviewControl.StatusProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T Status<T>(this T control,System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.DragPreviewControl 
=> control._setEx(Dock.Avalonia.Controls.DragPreviewControl.StatusProperty, ps, () => control.Status = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Status<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.DragPreviewControl 
   => control._set(Dock.Avalonia.Controls.DragPreviewControl.StatusProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Status<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.DragPreviewControl 
   => control._set(Dock.Avalonia.Controls.DragPreviewControl.StatusProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T Status<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.DragPreviewControl 
=> control._setEx(Dock.Avalonia.Controls.DragPreviewControl.StatusProperty, ps, () => control.Status = converter.TryConvert(value)!, bindingMode, converter, bindingSource);



//================= Styles ======================//
 // ContentTemplate

/*ValueStyleSetterGenerator*/
public static Style<T> ContentTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : Dock.Avalonia.Controls.DragPreviewControl 
=> style._addSetter(Dock.Avalonia.Controls.DragPreviewControl.ContentTemplateProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> ContentTemplate<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DragPreviewControl 
=> style._addSetter(Dock.Avalonia.Controls.DragPreviewControl.ContentTemplateProperty, binding);


 // Status

/*ValueStyleSetterGenerator*/
public static Style<T> Status<T>(this Style<T> style, System.String value) where T : Dock.Avalonia.Controls.DragPreviewControl 
=> style._addSetter(Dock.Avalonia.Controls.DragPreviewControl.StatusProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> Status<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DragPreviewControl 
=> style._addSetter(Dock.Avalonia.Controls.DragPreviewControl.StatusProperty, binding);



}
