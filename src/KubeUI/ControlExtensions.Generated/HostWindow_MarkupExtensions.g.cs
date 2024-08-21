#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class HostWindow_MarkupExtensions
{
//================= Properties ======================//
 // IsToolWindowProperty

/*BindFromExpressionSetterGenerator*/
public static T IsToolWindow<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.HostWindow
   => control._set(Dock.Avalonia.Controls.HostWindow.IsToolWindowProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsToolWindow<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.HostWindow
=> control._setEx(Dock.Avalonia.Controls.HostWindow.IsToolWindowProperty, ps, () => control.IsToolWindow = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsToolWindow<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.HostWindow
   => control._set(Dock.Avalonia.Controls.HostWindow.IsToolWindowProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsToolWindow<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.HostWindow
   => control._set(Dock.Avalonia.Controls.HostWindow.IsToolWindowProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsToolWindow<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.HostWindow
=> control._setEx(Dock.Avalonia.Controls.HostWindow.IsToolWindowProperty, ps, () => control.IsToolWindow = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ToolChromeControlsWholeWindowProperty

/*BindFromExpressionSetterGenerator*/
public static T ToolChromeControlsWholeWindow<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.HostWindow
   => control._set(Dock.Avalonia.Controls.HostWindow.ToolChromeControlsWholeWindowProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ToolChromeControlsWholeWindow<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.HostWindow
=> control._setEx(Dock.Avalonia.Controls.HostWindow.ToolChromeControlsWholeWindowProperty, ps, () => control.ToolChromeControlsWholeWindow = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ToolChromeControlsWholeWindow<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.HostWindow
   => control._set(Dock.Avalonia.Controls.HostWindow.ToolChromeControlsWholeWindowProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ToolChromeControlsWholeWindow<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.HostWindow
   => control._set(Dock.Avalonia.Controls.HostWindow.ToolChromeControlsWholeWindowProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ToolChromeControlsWholeWindow<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.HostWindow
=> control._setEx(Dock.Avalonia.Controls.HostWindow.ToolChromeControlsWholeWindowProperty, ps, () => control.ToolChromeControlsWholeWindow = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // IsToolWindowProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsToolWindow<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.HostWindow
=> style._addSetter(Dock.Avalonia.Controls.HostWindow.IsToolWindowProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsToolWindow<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.HostWindow
=> style._addSetter(Dock.Avalonia.Controls.HostWindow.IsToolWindowProperty, binding);


 // ToolChromeControlsWholeWindowProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ToolChromeControlsWholeWindow<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.HostWindow
=> style._addSetter(Dock.Avalonia.Controls.HostWindow.ToolChromeControlsWholeWindowProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ToolChromeControlsWholeWindow<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.HostWindow
=> style._addSetter(Dock.Avalonia.Controls.HostWindow.ToolChromeControlsWholeWindowProperty, binding);



}
