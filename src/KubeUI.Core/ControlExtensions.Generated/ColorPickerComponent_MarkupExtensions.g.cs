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
public static partial class ColorPickerComponent_MarkupExtensions
{
//================= Properties ======================//
 // Color

/*BindFromExpressionSetterGenerator*/
public static T Color<T>(this T control, Func<FluentAvalonia.UI.Media.Color2> func, Action<FluentAvalonia.UI.Media.Color2>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPickerComponent 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerComponent.ColorProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Color<T>(this T control,FluentAvalonia.UI.Media.Color2 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerComponent 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerComponent.ColorProperty, ps, () => control.Color = value, bindingMode, converter, bindingSource);

/*ValueOverloadsSetterGenerator*/

public static T Color<T>(this T control, System.Byte r = default, System.Byte g = default, System.Byte b = default, System.Byte a = default) where T : FluentAvalonia.UI.Controls.ColorPickerComponent 
   => control._set(() => control.Color = new FluentAvalonia.UI.Media.Color2(r, g, b, a));
public static T Color<T>(this T control, Avalonia.Media.Color avColor = default) where T : FluentAvalonia.UI.Controls.ColorPickerComponent 
   => control._set(() => control.Color = new FluentAvalonia.UI.Media.Color2(avColor));

/*BindSetterGenerator*/
public static T Color<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerComponent 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerComponent.ColorProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Color<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPickerComponent 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerComponent.ColorProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Color<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Media.Color2> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerComponent 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerComponent.ColorProperty, ps, () => control.Color = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // Component

/*BindFromExpressionSetterGenerator*/
public static T Component<T>(this T control, Func<FluentAvalonia.UI.Controls.ColorComponent> func, Action<FluentAvalonia.UI.Controls.ColorComponent>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPickerComponent 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerComponent.ComponentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Component<T>(this T control,FluentAvalonia.UI.Controls.ColorComponent value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerComponent 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerComponent.ComponentProperty, ps, () => control.Component = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Component<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerComponent 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerComponent.ComponentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Component<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPickerComponent 
   => control._set(FluentAvalonia.UI.Controls.ColorPickerComponent.ComponentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Component<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.ColorComponent> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerComponent 
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerComponent.ComponentProperty, ps, () => control.Component = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // ColorChanged

/*ActionToEventGenerator*/
public static T OnColorChanged<T>(this T control, Action<FluentAvalonia.UI.Controls.ColorPickerComponent, FluentAvalonia.UI.Controls.ColorChangedEventArgs> action) where T : FluentAvalonia.UI.Controls.ColorPickerComponent  => 
 control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.ColorPickerComponent,FluentAvalonia.UI.Controls.ColorChangedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.ColorChanged += h);



}
