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
public static partial class ToolTabStrip_MarkupExtensions
{
//================= Properties ======================//
 // DockAdornerHost

/*ValueSetterGenerator*/
public static T DockAdornerHost<T>(this T control, Avalonia.Controls.Control value) where T : Dock.Avalonia.Controls.ToolTabStrip 
=> control._set(() => control.DockAdornerHost = value!);

/*ValueSetterGenerator*/
public static T CanCreateItem<T>(this T control, System.Boolean value) where T : Dock.Avalonia.Controls.ToolTabStrip 
=> control._set(() => control.CanCreateItem = value!);

/*BindFromExpressionSetterGenerator*/
<<<<<<< HEAD
public static T DockAdornerHost<T>(this T control, Func<Avalonia.Controls.Control> func, Action<Avalonia.Controls.Control>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : Dock.Avalonia.Controls.ToolTabStrip 
   => control._set(Dock.Avalonia.Controls.ToolTabStrip.DockAdornerHostProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T DockAdornerHost<T>(this T control,Avalonia.Controls.Control value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.ToolTabStrip 
=> control._setEx(Dock.Avalonia.Controls.ToolTabStrip.DockAdornerHostProperty, ps, () => control.DockAdornerHost = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T DockAdornerHost<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.ToolTabStrip 
   => control._set(Dock.Avalonia.Controls.ToolTabStrip.DockAdornerHostProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T DockAdornerHost<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.ToolTabStrip 
   => control._set(Dock.Avalonia.Controls.ToolTabStrip.DockAdornerHostProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T DockAdornerHost<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Control> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.ToolTabStrip 
=> control._setEx(Dock.Avalonia.Controls.ToolTabStrip.DockAdornerHostProperty, ps, () => control.DockAdornerHost = converter.TryConvert(value)!, bindingMode, converter, bindingSource);


 // CanCreateItem

/*ValueSetterGenerator*/
public static T CanCreateItem<T>(this T control, System.Boolean value) where T : Dock.Avalonia.Controls.ToolTabStrip 
=> control._set(() => control.CanCreateItem = value!);

/*BindFromExpressionSetterGenerator*/
public static T CanCreateItem<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : Dock.Avalonia.Controls.ToolTabStrip 
   => control._set(Dock.Avalonia.Controls.ToolTabStrip.CanCreateItemProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
=======
public static T CanCreateItem<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : Dock.Avalonia.Controls.ToolTabStrip 
   => control._set(Dock.Avalonia.Controls.ToolTabStrip.CanCreateItemProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
>>>>>>> alpha
public static T CanCreateItem<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.ToolTabStrip 
=> control._setEx(Dock.Avalonia.Controls.ToolTabStrip.CanCreateItemProperty, ps, () => control.CanCreateItem = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CanCreateItem<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.ToolTabStrip 
   => control._set(Dock.Avalonia.Controls.ToolTabStrip.CanCreateItemProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CanCreateItem<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.ToolTabStrip 
   => control._set(Dock.Avalonia.Controls.ToolTabStrip.CanCreateItemProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T CanCreateItem<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.ToolTabStrip 
=> control._setEx(Dock.Avalonia.Controls.ToolTabStrip.CanCreateItemProperty, ps, () => control.CanCreateItem = converter.TryConvert(value)!, bindingMode, converter, bindingSource);



//================= Styles ======================//
 // DockAdornerHost

/*ValueStyleSetterGenerator*/
public static Style<T> DockAdornerHost<T>(this Style<T> style, Avalonia.Controls.Control value) where T : Dock.Avalonia.Controls.ToolTabStrip 
=> style._addSetter(Dock.Avalonia.Controls.ToolTabStrip.DockAdornerHostProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> DockAdornerHost<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.ToolTabStrip 
=> style._addSetter(Dock.Avalonia.Controls.ToolTabStrip.DockAdornerHostProperty, binding);


 // CanCreateItem

/*ValueStyleSetterGenerator*/
public static Style<T> CanCreateItem<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.ToolTabStrip 
=> style._addSetter(Dock.Avalonia.Controls.ToolTabStrip.CanCreateItemProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> CanCreateItem<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.ToolTabStrip 
=> style._addSetter(Dock.Avalonia.Controls.ToolTabStrip.CanCreateItemProperty, binding);



}
