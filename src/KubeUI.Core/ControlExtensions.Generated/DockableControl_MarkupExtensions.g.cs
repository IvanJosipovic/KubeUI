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
public static partial class DockableControl_MarkupExtensions
{
//================= Properties ======================//
 // TrackingMode

/*BindFromExpressionSetterGenerator*/
public static T TrackingMode<T>(this T control, Func<Dock.Model.Core.TrackingMode> func, Action<Dock.Model.Core.TrackingMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.DockableControl 
   => control._set(Dock.Avalonia.Controls.DockableControl.TrackingModeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T TrackingMode<T>(this T control,Dock.Model.Core.TrackingMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DockableControl 
=> control._setEx(Dock.Avalonia.Controls.DockableControl.TrackingModeProperty, ps, () => control.TrackingMode = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TrackingMode<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.DockableControl 
   => control._set(Dock.Avalonia.Controls.DockableControl.TrackingModeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TrackingMode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.DockableControl 
   => control._set(Dock.Avalonia.Controls.DockableControl.TrackingModeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T TrackingMode<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Dock.Model.Core.TrackingMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DockableControl 
=> control._setEx(Dock.Avalonia.Controls.DockableControl.TrackingModeProperty, ps, () => control.TrackingMode = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Styles ======================//
 // TrackingMode

/*ValueStyleSetterGenerator*/
public static Style<T> TrackingMode<T>(this Style<T> style, Dock.Model.Core.TrackingMode value) where T : Dock.Avalonia.Controls.DockableControl 
=> style._addSetter(Dock.Avalonia.Controls.DockableControl.TrackingModeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> TrackingMode<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DockableControl 
=> style._addSetter(Dock.Avalonia.Controls.DockableControl.TrackingModeProperty, binding);



}
