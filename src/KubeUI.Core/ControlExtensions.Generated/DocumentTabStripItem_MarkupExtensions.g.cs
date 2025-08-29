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
public static partial class DocumentTabStripItem_MarkupExtensions
{
//================= Properties ======================//
 // IsActive

/*BindFromExpressionSetterGenerator*/
public static T IsActive<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.DocumentTabStripItem 
   => control._set(Dock.Avalonia.Controls.DocumentTabStripItem.IsActiveProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsActive<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DocumentTabStripItem 
=> control._setEx(Dock.Avalonia.Controls.DocumentTabStripItem.IsActiveProperty, ps, () => control.IsActive = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsActive<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.DocumentTabStripItem 
   => control._set(Dock.Avalonia.Controls.DocumentTabStripItem.IsActiveProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsActive<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.DocumentTabStripItem 
   => control._set(Dock.Avalonia.Controls.DocumentTabStripItem.IsActiveProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsActive<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DocumentTabStripItem 
=> control._setEx(Dock.Avalonia.Controls.DocumentTabStripItem.IsActiveProperty, ps, () => control.IsActive = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // DocumentContextMenu

/*ValueSetterGenerator*/
public static T DocumentContextMenu<T>(this T control, Avalonia.Controls.ContextMenu value) where T : Dock.Avalonia.Controls.DocumentTabStripItem 
=> control._set(() => control.DocumentContextMenu = value!);

/*BindFromExpressionSetterGenerator*/
public static T DocumentContextMenu<T>(this T control, Func<Avalonia.Controls.ContextMenu> func, Action<Avalonia.Controls.ContextMenu>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : Dock.Avalonia.Controls.DocumentTabStripItem 
   => control._set(Dock.Avalonia.Controls.DocumentTabStripItem.DocumentContextMenuProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T DocumentContextMenu<T>(this T control,Avalonia.Controls.ContextMenu value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.DocumentTabStripItem 
=> control._setEx(Dock.Avalonia.Controls.DocumentTabStripItem.DocumentContextMenuProperty, ps, () => control.DocumentContextMenu = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T DocumentContextMenu<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.DocumentTabStripItem 
   => control._set(Dock.Avalonia.Controls.DocumentTabStripItem.DocumentContextMenuProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T DocumentContextMenu<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.DocumentTabStripItem 
   => control._set(Dock.Avalonia.Controls.DocumentTabStripItem.DocumentContextMenuProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T DocumentContextMenu<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.ContextMenu> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.DocumentTabStripItem 
=> control._setEx(Dock.Avalonia.Controls.DocumentTabStripItem.DocumentContextMenuProperty, ps, () => control.DocumentContextMenu = converter.TryConvert(value)!, bindingMode, converter, bindingSource);



//================= Styles ======================//
 // IsActive

/*ValueStyleSetterGenerator*/
public static Style<T> IsActive<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.DocumentTabStripItem 
=> style._addSetter(Dock.Avalonia.Controls.DocumentTabStripItem.IsActiveProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsActive<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DocumentTabStripItem 
=> style._addSetter(Dock.Avalonia.Controls.DocumentTabStripItem.IsActiveProperty, binding);


 // DocumentContextMenu

/*ValueStyleSetterGenerator*/
public static Style<T> DocumentContextMenu<T>(this Style<T> style, Avalonia.Controls.ContextMenu value) where T : Dock.Avalonia.Controls.DocumentTabStripItem 
=> style._addSetter(Dock.Avalonia.Controls.DocumentTabStripItem.DocumentContextMenuProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> DocumentContextMenu<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DocumentTabStripItem 
=> style._addSetter(Dock.Avalonia.Controls.DocumentTabStripItem.DocumentContextMenuProperty, binding);



}
