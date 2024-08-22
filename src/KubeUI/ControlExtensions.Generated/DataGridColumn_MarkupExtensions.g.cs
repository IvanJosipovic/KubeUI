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
public static partial class DataGridColumn_MarkupExtensions
{
//================= Properties ======================//
 // IsVisibleProperty

/*BindFromExpressionSetterGenerator*/
public static T IsVisible<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGridColumn
   => control._set(Avalonia.Controls.DataGridColumn.IsVisibleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsVisible<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridColumn
=> control._setEx(Avalonia.Controls.DataGridColumn.IsVisibleProperty, ps, () => control.IsVisible = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsVisible<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGridColumn
   => control._set(Avalonia.Controls.DataGridColumn.IsVisibleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsVisible<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGridColumn
   => control._set(Avalonia.Controls.DataGridColumn.IsVisibleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsVisible<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridColumn
=> control._setEx(Avalonia.Controls.DataGridColumn.IsVisibleProperty, ps, () => control.IsVisible = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CellThemeProperty

/*BindFromExpressionSetterGenerator*/
public static T CellTheme<T>(this T control, Func<Avalonia.Styling.ControlTheme> func, Action<Avalonia.Styling.ControlTheme>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGridColumn
   => control._set(Avalonia.Controls.DataGridColumn.CellThemeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CellTheme<T>(this T control, Avalonia.Styling.ControlTheme value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridColumn
=> control._setEx(Avalonia.Controls.DataGridColumn.CellThemeProperty, ps, () => control.CellTheme = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CellTheme<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGridColumn
   => control._set(Avalonia.Controls.DataGridColumn.CellThemeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CellTheme<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGridColumn
   => control._set(Avalonia.Controls.DataGridColumn.CellThemeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CellTheme<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ControlTheme> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridColumn
=> control._setEx(Avalonia.Controls.DataGridColumn.CellThemeProperty, ps, () => control.CellTheme = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // HeaderProperty

/*BindFromExpressionSetterGenerator*/
public static T Header<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGridColumn
   => control._set(Avalonia.Controls.DataGridColumn.HeaderProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Header<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridColumn
=> control._setEx(Avalonia.Controls.DataGridColumn.HeaderProperty, ps, () => control.Header = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Header<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGridColumn
   => control._set(Avalonia.Controls.DataGridColumn.HeaderProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Header<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGridColumn
   => control._set(Avalonia.Controls.DataGridColumn.HeaderProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Header<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridColumn
=> control._setEx(Avalonia.Controls.DataGridColumn.HeaderProperty, ps, () => control.Header = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // HeaderTemplateProperty

/*BindFromExpressionSetterGenerator*/
public static T HeaderTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGridColumn
   => control._set(Avalonia.Controls.DataGridColumn.HeaderTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T HeaderTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridColumn
=> control._setEx(Avalonia.Controls.DataGridColumn.HeaderTemplateProperty, ps, () => control.HeaderTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T HeaderTemplate<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGridColumn
   => control._set(Avalonia.Controls.DataGridColumn.HeaderTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T HeaderTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGridColumn
   => control._set(Avalonia.Controls.DataGridColumn.HeaderTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T HeaderTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridColumn
=> control._setEx(Avalonia.Controls.DataGridColumn.HeaderTemplateProperty, ps, () => control.HeaderTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // WidthProperty

/*BindFromExpressionSetterGenerator*/
public static T Width<T>(this T control, Func<Avalonia.Controls.DataGridLength> func, Action<Avalonia.Controls.DataGridLength>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGridColumn
   => control._set(Avalonia.Controls.DataGridColumn.WidthProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Width<T>(this T control, Avalonia.Controls.DataGridLength value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridColumn
=> control._setEx(Avalonia.Controls.DataGridColumn.WidthProperty, ps, () => control.Width = value, bindingMode, converter, bindingSource);

/*ValueOverloadsSetterGenerator*/

public static T Width<T>(this T control, System.Double value = default) where T : Avalonia.Controls.DataGridColumn
   => control._set(() => control.Width = new Avalonia.Controls.DataGridLength(value));
public static T Width<T>(this T control, System.Double value = default, Avalonia.Controls.DataGridLengthUnitType type = default) where T : Avalonia.Controls.DataGridColumn
   => control._set(() => control.Width = new Avalonia.Controls.DataGridLength(value, type));
public static T Width<T>(this T control, System.Double value = default, Avalonia.Controls.DataGridLengthUnitType type = default, System.Double desiredValue = default, System.Double displayValue = default) where T : Avalonia.Controls.DataGridColumn
   => control._set(() => control.Width = new Avalonia.Controls.DataGridLength(value, type, desiredValue, displayValue));

/*BindSetterGenerator*/
public static T Width<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGridColumn
   => control._set(Avalonia.Controls.DataGridColumn.WidthProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Width<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGridColumn
   => control._set(Avalonia.Controls.DataGridColumn.WidthProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Width<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.DataGridLength> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridColumn
=> control._setEx(Avalonia.Controls.DataGridColumn.WidthProperty, ps, () => control.Width = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // HeaderPointerPressed

/*ActionToEventGenerator*/
    public static T OnHeaderPointerPressed<T>(this T control, Action<Avalonia.Input.PointerPressedEventArgs> action) where T : Avalonia.Controls.DataGridColumn => 
        control._setEvent((System.EventHandler<Avalonia.Input.PointerPressedEventArgs>) ((arg0, arg1) => action(arg1)), h => control.HeaderPointerPressed += h);


 // HeaderPointerReleased

/*ActionToEventGenerator*/
    public static T OnHeaderPointerReleased<T>(this T control, Action<Avalonia.Input.PointerReleasedEventArgs> action) where T : Avalonia.Controls.DataGridColumn => 
        control._setEvent((System.EventHandler<Avalonia.Input.PointerReleasedEventArgs>) ((arg0, arg1) => action(arg1)), h => control.HeaderPointerReleased += h);



//================= Styles ======================//

}
