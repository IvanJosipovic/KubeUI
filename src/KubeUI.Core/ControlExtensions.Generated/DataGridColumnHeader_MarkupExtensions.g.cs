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
public static partial class DataGridColumnHeader_MarkupExtensions
{
//================= Properties ======================//
 // SeparatorBrushProperty

/*BindFromExpressionSetterGenerator*/
public static T SeparatorBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGridColumnHeader
   => control._set(Avalonia.Controls.DataGridColumnHeader.SeparatorBrushProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SeparatorBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridColumnHeader
=> control._setEx(Avalonia.Controls.DataGridColumnHeader.SeparatorBrushProperty, ps, () => control.SeparatorBrush = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SeparatorBrush<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGridColumnHeader
   => control._set(Avalonia.Controls.DataGridColumnHeader.SeparatorBrushProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SeparatorBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGridColumnHeader
   => control._set(Avalonia.Controls.DataGridColumnHeader.SeparatorBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SeparatorBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridColumnHeader
=> control._setEx(Avalonia.Controls.DataGridColumnHeader.SeparatorBrushProperty, ps, () => control.SeparatorBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // AreSeparatorsVisibleProperty

/*BindFromExpressionSetterGenerator*/
public static T AreSeparatorsVisible<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGridColumnHeader
   => control._set(Avalonia.Controls.DataGridColumnHeader.AreSeparatorsVisibleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T AreSeparatorsVisible<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridColumnHeader
=> control._setEx(Avalonia.Controls.DataGridColumnHeader.AreSeparatorsVisibleProperty, ps, () => control.AreSeparatorsVisible = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T AreSeparatorsVisible<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGridColumnHeader
   => control._set(Avalonia.Controls.DataGridColumnHeader.AreSeparatorsVisibleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T AreSeparatorsVisible<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGridColumnHeader
   => control._set(Avalonia.Controls.DataGridColumnHeader.AreSeparatorsVisibleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T AreSeparatorsVisible<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridColumnHeader
=> control._setEx(Avalonia.Controls.DataGridColumnHeader.AreSeparatorsVisibleProperty, ps, () => control.AreSeparatorsVisible = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // LeftClick

/*ActionToEventGenerator*/
    public static T OnLeftClick<T>(this T control, Action<System.Object, Avalonia.Input.KeyModifiers> action) where T : Avalonia.Controls.DataGridColumnHeader => 
        control._setEvent((System.EventHandler<Avalonia.Input.KeyModifiers>) ((arg0, arg1) => action(arg0, arg1)), h => control.LeftClick += h);



//================= Styles ======================//
 // SeparatorBrushProperty

/*ValueStyleSetterGenerator*/
public static Style<T> SeparatorBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Avalonia.Controls.DataGridColumnHeader
=> style._addSetter(Avalonia.Controls.DataGridColumnHeader.SeparatorBrushProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SeparatorBrush<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGridColumnHeader
=> style._addSetter(Avalonia.Controls.DataGridColumnHeader.SeparatorBrushProperty, binding);


 // AreSeparatorsVisibleProperty

/*ValueStyleSetterGenerator*/
public static Style<T> AreSeparatorsVisible<T>(this Style<T> style, System.Boolean value) where T : Avalonia.Controls.DataGridColumnHeader
=> style._addSetter(Avalonia.Controls.DataGridColumnHeader.AreSeparatorsVisibleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> AreSeparatorsVisible<T>(this Style<T> style, IBinding binding) where T : Avalonia.Controls.DataGridColumnHeader
=> style._addSetter(Avalonia.Controls.DataGridColumnHeader.AreSeparatorsVisibleProperty, binding);



}
