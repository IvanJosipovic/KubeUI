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
public static partial class ToolChromeControl_MarkupExtensions
{
//================= Properties ======================//
 // IsActiveProperty

/*BindFromExpressionSetterGenerator*/
public static T IsActive<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.ToolChromeControl
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.IsActiveProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsActive<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.ToolChromeControl
=> control._setEx(Dock.Avalonia.Controls.ToolChromeControl.IsActiveProperty, ps, () => control.IsActive = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsActive<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.ToolChromeControl
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.IsActiveProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsActive<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.ToolChromeControl
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.IsActiveProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsActive<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.ToolChromeControl
=> control._setEx(Dock.Avalonia.Controls.ToolChromeControl.IsActiveProperty, ps, () => control.IsActive = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsPinnedProperty

/*BindFromExpressionSetterGenerator*/
public static T IsPinned<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.ToolChromeControl
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.IsPinnedProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsPinned<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.ToolChromeControl
=> control._setEx(Dock.Avalonia.Controls.ToolChromeControl.IsPinnedProperty, ps, () => control.IsPinned = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsPinned<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.ToolChromeControl
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.IsPinnedProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsPinned<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.ToolChromeControl
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.IsPinnedProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsPinned<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.ToolChromeControl
=> control._setEx(Dock.Avalonia.Controls.ToolChromeControl.IsPinnedProperty, ps, () => control.IsPinned = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsFloatingProperty

/*BindFromExpressionSetterGenerator*/
public static T IsFloating<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.ToolChromeControl
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.IsFloatingProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsFloating<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.ToolChromeControl
=> control._setEx(Dock.Avalonia.Controls.ToolChromeControl.IsFloatingProperty, ps, () => control.IsFloating = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsFloating<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.ToolChromeControl
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.IsFloatingProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsFloating<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.ToolChromeControl
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.IsFloatingProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsFloating<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.ToolChromeControl
=> control._setEx(Dock.Avalonia.Controls.ToolChromeControl.IsFloatingProperty, ps, () => control.IsFloating = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsMaximizedProperty

/*BindFromExpressionSetterGenerator*/
public static T IsMaximized<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.ToolChromeControl
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.IsMaximizedProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsMaximized<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.ToolChromeControl
=> control._setEx(Dock.Avalonia.Controls.ToolChromeControl.IsMaximizedProperty, ps, () => control.IsMaximized = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsMaximized<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.ToolChromeControl
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.IsMaximizedProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsMaximized<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.ToolChromeControl
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.IsMaximizedProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsMaximized<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.ToolChromeControl
=> control._setEx(Dock.Avalonia.Controls.ToolChromeControl.IsMaximizedProperty, ps, () => control.IsMaximized = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // IsActiveProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsActive<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.ToolChromeControl
=> style._addSetter(Dock.Avalonia.Controls.ToolChromeControl.IsActiveProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsActive<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.ToolChromeControl
=> style._addSetter(Dock.Avalonia.Controls.ToolChromeControl.IsActiveProperty, binding);


 // IsPinnedProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsPinned<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.ToolChromeControl
=> style._addSetter(Dock.Avalonia.Controls.ToolChromeControl.IsPinnedProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsPinned<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.ToolChromeControl
=> style._addSetter(Dock.Avalonia.Controls.ToolChromeControl.IsPinnedProperty, binding);


 // IsFloatingProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsFloating<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.ToolChromeControl
=> style._addSetter(Dock.Avalonia.Controls.ToolChromeControl.IsFloatingProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsFloating<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.ToolChromeControl
=> style._addSetter(Dock.Avalonia.Controls.ToolChromeControl.IsFloatingProperty, binding);


 // IsMaximizedProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsMaximized<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.ToolChromeControl
=> style._addSetter(Dock.Avalonia.Controls.ToolChromeControl.IsMaximizedProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsMaximized<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.ToolChromeControl
=> style._addSetter(Dock.Avalonia.Controls.ToolChromeControl.IsMaximizedProperty, binding);



}
