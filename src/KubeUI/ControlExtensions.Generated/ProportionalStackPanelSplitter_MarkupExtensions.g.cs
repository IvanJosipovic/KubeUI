#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class ProportionalStackPanelSplitter_MarkupExtensions
{
//================= Properties ======================//
 // ThicknessProperty

/*BindFromExpressionSetterGenerator*/
public static T Thickness<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.ProportionalStackPanelSplitter
   => control._set(Dock.Avalonia.Controls.ProportionalStackPanelSplitter.ThicknessProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Thickness<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.ProportionalStackPanelSplitter
=> control._setEx(Dock.Avalonia.Controls.ProportionalStackPanelSplitter.ThicknessProperty, ps, () => control.Thickness = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Thickness<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.ProportionalStackPanelSplitter
   => control._set(Dock.Avalonia.Controls.ProportionalStackPanelSplitter.ThicknessProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Thickness<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.ProportionalStackPanelSplitter
   => control._set(Dock.Avalonia.Controls.ProportionalStackPanelSplitter.ThicknessProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Thickness<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.ProportionalStackPanelSplitter
=> control._setEx(Dock.Avalonia.Controls.ProportionalStackPanelSplitter.ThicknessProperty, ps, () => control.Thickness = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // ThicknessProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Thickness<T>(this Style<T> style, System.Double value) where T : Dock.Avalonia.Controls.ProportionalStackPanelSplitter
=> style._addSetter(Dock.Avalonia.Controls.ProportionalStackPanelSplitter.ThicknessProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Thickness<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.ProportionalStackPanelSplitter
=> style._addSetter(Dock.Avalonia.Controls.ProportionalStackPanelSplitter.ThicknessProperty, binding);



}
