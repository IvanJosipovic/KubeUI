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
public static partial class ToolTabStrip_MarkupExtensions
{
//================= Properties ======================//
 // CanCreateItemProperty

/*BindFromExpressionSetterGenerator*/
public static T CanCreateItem<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.ToolTabStrip
   => control._set(Dock.Avalonia.Controls.ToolTabStrip.CanCreateItemProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CanCreateItem<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.ToolTabStrip
=> control._setEx(Dock.Avalonia.Controls.ToolTabStrip.CanCreateItemProperty, ps, () => control.CanCreateItem = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CanCreateItem<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.ToolTabStrip
   => control._set(Dock.Avalonia.Controls.ToolTabStrip.CanCreateItemProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CanCreateItem<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.ToolTabStrip
   => control._set(Dock.Avalonia.Controls.ToolTabStrip.CanCreateItemProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CanCreateItem<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.ToolTabStrip
=> control._setEx(Dock.Avalonia.Controls.ToolTabStrip.CanCreateItemProperty, ps, () => control.CanCreateItem = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // CanCreateItemProperty

/*ValueStyleSetterGenerator*/
public static Style<T> CanCreateItem<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.ToolTabStrip
=> style._addSetter(Dock.Avalonia.Controls.ToolTabStrip.CanCreateItemProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CanCreateItem<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.ToolTabStrip
=> style._addSetter(Dock.Avalonia.Controls.ToolTabStrip.CanCreateItemProperty, binding);



}
