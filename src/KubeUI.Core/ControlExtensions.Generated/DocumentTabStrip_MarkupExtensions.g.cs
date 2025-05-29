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
public static partial class DocumentTabStrip_MarkupExtensions
{
//================= Properties ======================//
 // CanCreateItem

/*BindFromExpressionSetterGenerator*/
public static T CanCreateItem<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.DocumentTabStrip 
   => control._set(Dock.Avalonia.Controls.DocumentTabStrip.CanCreateItemProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CanCreateItem<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DocumentTabStrip 
=> control._setEx(Dock.Avalonia.Controls.DocumentTabStrip.CanCreateItemProperty, ps, () => control.CanCreateItem = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CanCreateItem<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.DocumentTabStrip 
   => control._set(Dock.Avalonia.Controls.DocumentTabStrip.CanCreateItemProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CanCreateItem<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.DocumentTabStrip 
   => control._set(Dock.Avalonia.Controls.DocumentTabStrip.CanCreateItemProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CanCreateItem<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DocumentTabStrip 
=> control._setEx(Dock.Avalonia.Controls.DocumentTabStrip.CanCreateItemProperty, ps, () => control.CanCreateItem = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsActive

/*BindFromExpressionSetterGenerator*/
public static T IsActive<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.DocumentTabStrip 
   => control._set(Dock.Avalonia.Controls.DocumentTabStrip.IsActiveProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsActive<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DocumentTabStrip 
=> control._setEx(Dock.Avalonia.Controls.DocumentTabStrip.IsActiveProperty, ps, () => control.IsActive = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsActive<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.DocumentTabStrip 
   => control._set(Dock.Avalonia.Controls.DocumentTabStrip.IsActiveProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsActive<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.DocumentTabStrip 
   => control._set(Dock.Avalonia.Controls.DocumentTabStrip.IsActiveProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsActive<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DocumentTabStrip 
=> control._setEx(Dock.Avalonia.Controls.DocumentTabStrip.IsActiveProperty, ps, () => control.IsActive = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Styles ======================//
 // CanCreateItem

/*ValueStyleSetterGenerator*/
public static Style<T> CanCreateItem<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.DocumentTabStrip 
=> style._addSetter(Dock.Avalonia.Controls.DocumentTabStrip.CanCreateItemProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CanCreateItem<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DocumentTabStrip 
=> style._addSetter(Dock.Avalonia.Controls.DocumentTabStrip.CanCreateItemProperty, binding);


 // IsActive

/*ValueStyleSetterGenerator*/
public static Style<T> IsActive<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.DocumentTabStrip 
=> style._addSetter(Dock.Avalonia.Controls.DocumentTabStrip.IsActiveProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsActive<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DocumentTabStrip 
=> style._addSetter(Dock.Avalonia.Controls.DocumentTabStrip.IsActiveProperty, binding);



}
