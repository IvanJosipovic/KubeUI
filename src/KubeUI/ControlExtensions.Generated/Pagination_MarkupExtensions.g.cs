#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class Pagination_MarkupExtensions
{
//================= Properties ======================//
 // CurrentPageProperty

/*BindFromExpressionSetterGenerator*/
public static T CurrentPage<T>(this T control, Func<System.Nullable<System.Int32>> func, Action<System.Nullable<System.Int32>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.CurrentPageProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CurrentPage<T>(this T control, System.Nullable<System.Int32> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Pagination
=> control._setEx(Ursa.Controls.Pagination.CurrentPageProperty, ps, () => control.CurrentPage = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CurrentPage<T>(this T control, IBinding binding) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.CurrentPageProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CurrentPage<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.CurrentPageProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CurrentPage<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Nullable<System.Int32>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Pagination
=> control._setEx(Ursa.Controls.Pagination.CurrentPageProperty, ps, () => control.CurrentPage = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CommandProperty

/*BindFromExpressionSetterGenerator*/
public static T Command<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.CommandProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Command<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Pagination
=> control._setEx(Ursa.Controls.Pagination.CommandProperty, ps, () => control.Command = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Command<T>(this T control, IBinding binding) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.CommandProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Command<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.CommandProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Command<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Pagination
=> control._setEx(Ursa.Controls.Pagination.CommandProperty, ps, () => control.Command = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CommandParameterProperty

/*BindFromExpressionSetterGenerator*/
public static T CommandParameter<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.CommandParameterProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CommandParameter<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Pagination
=> control._setEx(Ursa.Controls.Pagination.CommandParameterProperty, ps, () => control.CommandParameter = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CommandParameter<T>(this T control, IBinding binding) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.CommandParameterProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CommandParameter<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.CommandParameterProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CommandParameter<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Pagination
=> control._setEx(Ursa.Controls.Pagination.CommandParameterProperty, ps, () => control.CommandParameter = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TotalCountProperty

/*BindFromExpressionSetterGenerator*/
public static T TotalCount<T>(this T control, Func<System.Int32> func, Action<System.Int32>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.TotalCountProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T TotalCount<T>(this T control, System.Int32 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Pagination
=> control._setEx(Ursa.Controls.Pagination.TotalCountProperty, ps, () => control.TotalCount = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TotalCount<T>(this T control, IBinding binding) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.TotalCountProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TotalCount<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.TotalCountProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T TotalCount<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Int32> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Pagination
=> control._setEx(Ursa.Controls.Pagination.TotalCountProperty, ps, () => control.TotalCount = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // PageSizeProperty

/*BindFromExpressionSetterGenerator*/
public static T PageSize<T>(this T control, Func<System.Int32> func, Action<System.Int32>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.PageSizeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T PageSize<T>(this T control, System.Int32 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Pagination
=> control._setEx(Ursa.Controls.Pagination.PageSizeProperty, ps, () => control.PageSize = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T PageSize<T>(this T control, IBinding binding) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.PageSizeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T PageSize<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.PageSizeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T PageSize<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Int32> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Pagination
=> control._setEx(Ursa.Controls.Pagination.PageSizeProperty, ps, () => control.PageSize = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // PageSizeOptionsProperty

/*BindFromExpressionSetterGenerator*/
public static T PageSizeOptions<T>(this T control, Func<Avalonia.Collections.AvaloniaList<System.Int32>> func, Action<Avalonia.Collections.AvaloniaList<System.Int32>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.PageSizeOptionsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T PageSizeOptions<T>(this T control, Avalonia.Collections.AvaloniaList<System.Int32> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Pagination
=> control._setEx(Ursa.Controls.Pagination.PageSizeOptionsProperty, ps, () => control.PageSizeOptions = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T PageSizeOptions<T>(this T control, IBinding binding) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.PageSizeOptionsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T PageSizeOptions<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.PageSizeOptionsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T PageSizeOptions<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Collections.AvaloniaList<System.Int32>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Pagination
=> control._setEx(Ursa.Controls.Pagination.PageSizeOptionsProperty, ps, () => control.PageSizeOptions = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // PageButtonThemeProperty

/*BindFromExpressionSetterGenerator*/
public static T PageButtonTheme<T>(this T control, Func<Avalonia.Styling.ControlTheme> func, Action<Avalonia.Styling.ControlTheme>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.PageButtonThemeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T PageButtonTheme<T>(this T control, Avalonia.Styling.ControlTheme value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Pagination
=> control._setEx(Ursa.Controls.Pagination.PageButtonThemeProperty, ps, () => control.PageButtonTheme = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T PageButtonTheme<T>(this T control, IBinding binding) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.PageButtonThemeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T PageButtonTheme<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.PageButtonThemeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T PageButtonTheme<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ControlTheme> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Pagination
=> control._setEx(Ursa.Controls.Pagination.PageButtonThemeProperty, ps, () => control.PageButtonTheme = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ShowPageSizeSelectorProperty

/*BindFromExpressionSetterGenerator*/
public static T ShowPageSizeSelector<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.ShowPageSizeSelectorProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ShowPageSizeSelector<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Pagination
=> control._setEx(Ursa.Controls.Pagination.ShowPageSizeSelectorProperty, ps, () => control.ShowPageSizeSelector = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ShowPageSizeSelector<T>(this T control, IBinding binding) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.ShowPageSizeSelectorProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ShowPageSizeSelector<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.ShowPageSizeSelectorProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ShowPageSizeSelector<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Pagination
=> control._setEx(Ursa.Controls.Pagination.ShowPageSizeSelectorProperty, ps, () => control.ShowPageSizeSelector = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ShowQuickJumpProperty

/*BindFromExpressionSetterGenerator*/
public static T ShowQuickJump<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.ShowQuickJumpProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ShowQuickJump<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Pagination
=> control._setEx(Ursa.Controls.Pagination.ShowQuickJumpProperty, ps, () => control.ShowQuickJump = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ShowQuickJump<T>(this T control, IBinding binding) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.ShowQuickJumpProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ShowQuickJump<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.ShowQuickJumpProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ShowQuickJump<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Pagination
=> control._setEx(Ursa.Controls.Pagination.ShowQuickJumpProperty, ps, () => control.ShowQuickJump = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // DisplayCurrentPageInQuickJumperProperty

/*BindFromExpressionSetterGenerator*/
public static T DisplayCurrentPageInQuickJumper<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.DisplayCurrentPageInQuickJumperProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T DisplayCurrentPageInQuickJumper<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Pagination
=> control._setEx(Ursa.Controls.Pagination.DisplayCurrentPageInQuickJumperProperty, ps, () => control.DisplayCurrentPageInQuickJumper = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T DisplayCurrentPageInQuickJumper<T>(this T control, IBinding binding) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.DisplayCurrentPageInQuickJumperProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T DisplayCurrentPageInQuickJumper<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.DisplayCurrentPageInQuickJumperProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T DisplayCurrentPageInQuickJumper<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Pagination
=> control._setEx(Ursa.Controls.Pagination.DisplayCurrentPageInQuickJumperProperty, ps, () => control.DisplayCurrentPageInQuickJumper = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // CurrentPageChanged

/*ActionToEventGenerator*/
    public static T OnCurrentPageChanged<T>(this T control, Action<System.Object, Ursa.Controls.ValueChangedEventArgs<System.Int32>> action) where T : Ursa.Controls.Pagination => 
        control._setEvent((System.EventHandler<Ursa.Controls.ValueChangedEventArgs<System.Int32>>) ((arg0, arg1) => action(arg0, arg1)), h => control.CurrentPageChanged += h);



//================= Styles ======================//
 // CurrentPageProperty

/*ValueStyleSetterGenerator*/
public static Style<T> CurrentPage<T>(this Style<T> style, System.Nullable<System.Int32> value) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.CurrentPageProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CurrentPage<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.CurrentPageProperty, binding);


 // CommandProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Command<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.CommandProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Command<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.CommandProperty, binding);


 // CommandParameterProperty

/*ValueStyleSetterGenerator*/
public static Style<T> CommandParameter<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.CommandParameterProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CommandParameter<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.CommandParameterProperty, binding);


 // TotalCountProperty

/*ValueStyleSetterGenerator*/
public static Style<T> TotalCount<T>(this Style<T> style, System.Int32 value) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.TotalCountProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> TotalCount<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.TotalCountProperty, binding);


 // PageSizeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> PageSize<T>(this Style<T> style, System.Int32 value) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.PageSizeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> PageSize<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.PageSizeProperty, binding);


 // PageSizeOptionsProperty

/*ValueStyleSetterGenerator*/
public static Style<T> PageSizeOptions<T>(this Style<T> style, Avalonia.Collections.AvaloniaList<System.Int32> value) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.PageSizeOptionsProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> PageSizeOptions<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.PageSizeOptionsProperty, binding);


 // PageButtonThemeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> PageButtonTheme<T>(this Style<T> style, Avalonia.Styling.ControlTheme value) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.PageButtonThemeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> PageButtonTheme<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.PageButtonThemeProperty, binding);


 // ShowPageSizeSelectorProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ShowPageSizeSelector<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.ShowPageSizeSelectorProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ShowPageSizeSelector<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.ShowPageSizeSelectorProperty, binding);


 // ShowQuickJumpProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ShowQuickJump<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.ShowQuickJumpProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ShowQuickJump<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.ShowQuickJumpProperty, binding);


 // DisplayCurrentPageInQuickJumperProperty

/*ValueStyleSetterGenerator*/
public static Style<T> DisplayCurrentPageInQuickJumper<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.DisplayCurrentPageInQuickJumperProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> DisplayCurrentPageInQuickJumper<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.DisplayCurrentPageInQuickJumperProperty, binding);



}
