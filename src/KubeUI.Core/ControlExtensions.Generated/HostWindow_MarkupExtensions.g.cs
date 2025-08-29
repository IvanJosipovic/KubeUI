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
public static partial class HostWindow_MarkupExtensions
{
//================= Properties ======================//
 // IsToolWindow

/*BindFromExpressionSetterGenerator*/
public static T IsToolWindow<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.HostWindow 
   => control._set(Dock.Avalonia.Controls.HostWindow.IsToolWindowProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsToolWindow<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.HostWindow 
=> control._setEx(Dock.Avalonia.Controls.HostWindow.IsToolWindowProperty, ps, () => control.IsToolWindow = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsToolWindow<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.HostWindow 
   => control._set(Dock.Avalonia.Controls.HostWindow.IsToolWindowProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsToolWindow<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.HostWindow 
   => control._set(Dock.Avalonia.Controls.HostWindow.IsToolWindowProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsToolWindow<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.HostWindow 
=> control._setEx(Dock.Avalonia.Controls.HostWindow.IsToolWindowProperty, ps, () => control.IsToolWindow = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ToolChromeControlsWholeWindow

/*BindFromExpressionSetterGenerator*/
public static T ToolChromeControlsWholeWindow<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.HostWindow 
   => control._set(Dock.Avalonia.Controls.HostWindow.ToolChromeControlsWholeWindowProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ToolChromeControlsWholeWindow<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.HostWindow 
=> control._setEx(Dock.Avalonia.Controls.HostWindow.ToolChromeControlsWholeWindowProperty, ps, () => control.ToolChromeControlsWholeWindow = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ToolChromeControlsWholeWindow<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.HostWindow 
   => control._set(Dock.Avalonia.Controls.HostWindow.ToolChromeControlsWholeWindowProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ToolChromeControlsWholeWindow<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.HostWindow 
   => control._set(Dock.Avalonia.Controls.HostWindow.ToolChromeControlsWholeWindowProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ToolChromeControlsWholeWindow<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.HostWindow 
=> control._setEx(Dock.Avalonia.Controls.HostWindow.ToolChromeControlsWholeWindowProperty, ps, () => control.ToolChromeControlsWholeWindow = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // DocumentChromeControlsWholeWindow

/*ValueSetterGenerator*/
public static T DocumentChromeControlsWholeWindow<T>(this T control, System.Boolean value) where T : Dock.Avalonia.Controls.HostWindow 
=> control._set(() => control.DocumentChromeControlsWholeWindow = value!);

/*BindFromExpressionSetterGenerator*/
public static T DocumentChromeControlsWholeWindow<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : Dock.Avalonia.Controls.HostWindow 
   => control._set(Dock.Avalonia.Controls.HostWindow.DocumentChromeControlsWholeWindowProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T DocumentChromeControlsWholeWindow<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.HostWindow 
=> control._setEx(Dock.Avalonia.Controls.HostWindow.DocumentChromeControlsWholeWindowProperty, ps, () => control.DocumentChromeControlsWholeWindow = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T DocumentChromeControlsWholeWindow<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.HostWindow 
   => control._set(Dock.Avalonia.Controls.HostWindow.DocumentChromeControlsWholeWindowProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T DocumentChromeControlsWholeWindow<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.HostWindow 
   => control._set(Dock.Avalonia.Controls.HostWindow.DocumentChromeControlsWholeWindowProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T DocumentChromeControlsWholeWindow<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.HostWindow 
=> control._setEx(Dock.Avalonia.Controls.HostWindow.DocumentChromeControlsWholeWindowProperty, ps, () => control.DocumentChromeControlsWholeWindow = converter.TryConvert(value)!, bindingMode, converter, bindingSource);



//================= Styles ======================//
 // IsToolWindow

/*ValueStyleSetterGenerator*/
public static Style<T> IsToolWindow<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.HostWindow 
=> style._addSetter(Dock.Avalonia.Controls.HostWindow.IsToolWindowProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsToolWindow<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.HostWindow 
=> style._addSetter(Dock.Avalonia.Controls.HostWindow.IsToolWindowProperty, binding);


 // ToolChromeControlsWholeWindow

/*ValueStyleSetterGenerator*/
public static Style<T> ToolChromeControlsWholeWindow<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.HostWindow 
=> style._addSetter(Dock.Avalonia.Controls.HostWindow.ToolChromeControlsWholeWindowProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ToolChromeControlsWholeWindow<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.HostWindow 
=> style._addSetter(Dock.Avalonia.Controls.HostWindow.ToolChromeControlsWholeWindowProperty, binding);


 // DocumentChromeControlsWholeWindow

/*ValueStyleSetterGenerator*/
public static Style<T> DocumentChromeControlsWholeWindow<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.HostWindow 
=> style._addSetter(Dock.Avalonia.Controls.HostWindow.DocumentChromeControlsWholeWindowProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> DocumentChromeControlsWholeWindow<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.HostWindow 
=> style._addSetter(Dock.Avalonia.Controls.HostWindow.DocumentChromeControlsWholeWindowProperty, binding);



}
