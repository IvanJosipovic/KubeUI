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
public static partial class DataGridDetailsPresenter_MarkupExtensions
{
//================= Properties ======================//
 // ContentHeight

/*BindFromExpressionSetterGenerator*/
public static Avalonia.Controls.Primitives.DataGridDetailsPresenter ContentHeight(this Avalonia.Controls.Primitives.DataGridDetailsPresenter control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null)  
   => control._set(Avalonia.Controls.Primitives.DataGridDetailsPresenter.ContentHeightProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static Avalonia.Controls.Primitives.DataGridDetailsPresenter ContentHeight(this Avalonia.Controls.Primitives.DataGridDetailsPresenter control,System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null)  
=> control._setEx(Avalonia.Controls.Primitives.DataGridDetailsPresenter.ContentHeightProperty, ps, () => control.ContentHeight = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static Avalonia.Controls.Primitives.DataGridDetailsPresenter ContentHeight(this Avalonia.Controls.Primitives.DataGridDetailsPresenter control, IBinding binding)  
   => control._set(Avalonia.Controls.Primitives.DataGridDetailsPresenter.ContentHeightProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static Avalonia.Controls.Primitives.DataGridDetailsPresenter ContentHeight(this Avalonia.Controls.Primitives.DataGridDetailsPresenter control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null)  
   => control._set(Avalonia.Controls.Primitives.DataGridDetailsPresenter.ContentHeightProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static Avalonia.Controls.Primitives.DataGridDetailsPresenter ContentHeight<TValue>(this Avalonia.Controls.Primitives.DataGridDetailsPresenter control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null)  
=> control._setEx(Avalonia.Controls.Primitives.DataGridDetailsPresenter.ContentHeightProperty, ps, () => control.ContentHeight = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Styles ======================//
 // ContentHeight

/*ValueStyleSetterGenerator*/
public static Style<Avalonia.Controls.Primitives.DataGridDetailsPresenter> ContentHeight(this Style<Avalonia.Controls.Primitives.DataGridDetailsPresenter> style, System.Double value)  
=> style._addSetter(Avalonia.Controls.Primitives.DataGridDetailsPresenter.ContentHeightProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<Avalonia.Controls.Primitives.DataGridDetailsPresenter> ContentHeight(this Style<Avalonia.Controls.Primitives.DataGridDetailsPresenter> style, IBinding binding)  
=> style._addSetter(Avalonia.Controls.Primitives.DataGridDetailsPresenter.ContentHeightProperty, binding);



}
