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
public static partial class ToolTabStripItem_MarkupExtensions
{
//================= Properties ======================//
 // TabContextMenu

/*ValueSetterGenerator*/
public static T TabContextMenu<T>(this T control, Avalonia.Controls.ContextMenu value) where T : Dock.Avalonia.Controls.ToolTabStripItem 
=> control._set(() => control.TabContextMenu = value!);

/*BindFromExpressionSetterGenerator*/
public static T TabContextMenu<T>(this T control, Func<Avalonia.Controls.ContextMenu> func, Action<Avalonia.Controls.ContextMenu>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : Dock.Avalonia.Controls.ToolTabStripItem 
   => control._set(Dock.Avalonia.Controls.ToolTabStripItem.TabContextMenuProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T TabContextMenu<T>(this T control,Avalonia.Controls.ContextMenu value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.ToolTabStripItem 
=> control._setEx(Dock.Avalonia.Controls.ToolTabStripItem.TabContextMenuProperty, ps, () => control.TabContextMenu = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TabContextMenu<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.ToolTabStripItem 
   => control._set(Dock.Avalonia.Controls.ToolTabStripItem.TabContextMenuProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TabContextMenu<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.ToolTabStripItem 
   => control._set(Dock.Avalonia.Controls.ToolTabStripItem.TabContextMenuProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T TabContextMenu<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.ContextMenu> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.ToolTabStripItem 
=> control._setEx(Dock.Avalonia.Controls.ToolTabStripItem.TabContextMenuProperty, ps, () => control.TabContextMenu = converter.TryConvert(value)!, bindingMode, converter, bindingSource);



//================= Styles ======================//
 // TabContextMenu

/*ValueStyleSetterGenerator*/
public static Style<T> TabContextMenu<T>(this Style<T> style, Avalonia.Controls.ContextMenu value) where T : Dock.Avalonia.Controls.ToolTabStripItem 
=> style._addSetter(Dock.Avalonia.Controls.ToolTabStripItem.TabContextMenuProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> TabContextMenu<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.ToolTabStripItem 
=> style._addSetter(Dock.Avalonia.Controls.ToolTabStripItem.TabContextMenuProperty, binding);



}
