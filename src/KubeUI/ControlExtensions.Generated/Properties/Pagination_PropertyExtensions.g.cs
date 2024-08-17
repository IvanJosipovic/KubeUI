#nullable enable
using Avalonia.Collections;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Styling;
using Pagination = Ursa.Controls.Pagination;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class PaginationExtensions
{
public static T CurrentPage<T>(this T control, IBinding binding) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.CurrentPageProperty, binding);
public static T CurrentPage<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.CurrentPageProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CurrentPage<T>(this T control, Func<System.Nullable<System.Int32>> func, Action<System.Nullable<System.Int32>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.CurrentPageProperty, func, onChanged, expression);
public static T CurrentPage<T>(this T control, System.Nullable<System.Int32> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Pagination
=> control._setEx(Ursa.Controls.Pagination.CurrentPageProperty, ps, () => control.CurrentPage = value, bindingMode, converter, bindingSource);
public static T CurrentPage<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Nullable<System.Int32>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Pagination
=> control._setEx(Ursa.Controls.Pagination.CurrentPageProperty, ps, () => control.CurrentPage = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Command<T>(this T control, IBinding binding) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.CommandProperty, binding);
public static T Command<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.CommandProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Command<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.CommandProperty, func, onChanged, expression);
public static T Command<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Pagination
=> control._setEx(Ursa.Controls.Pagination.CommandProperty, ps, () => control.Command = value, bindingMode, converter, bindingSource);
public static T Command<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Pagination
=> control._setEx(Ursa.Controls.Pagination.CommandProperty, ps, () => control.Command = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T CommandParameter<T>(this T control, IBinding binding) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.CommandParameterProperty, binding);
public static T CommandParameter<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.CommandParameterProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CommandParameter<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.CommandParameterProperty, func, onChanged, expression);
public static T CommandParameter<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Pagination
=> control._setEx(Ursa.Controls.Pagination.CommandParameterProperty, ps, () => control.CommandParameter = value, bindingMode, converter, bindingSource);
public static T CommandParameter<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Pagination
=> control._setEx(Ursa.Controls.Pagination.CommandParameterProperty, ps, () => control.CommandParameter = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T TotalCount<T>(this T control, IBinding binding) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.TotalCountProperty, binding);
public static T TotalCount<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.TotalCountProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T TotalCount<T>(this T control, Func<System.Int32> func, Action<System.Int32>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.TotalCountProperty, func, onChanged, expression);
public static T TotalCount<T>(this T control, System.Int32 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Pagination
=> control._setEx(Ursa.Controls.Pagination.TotalCountProperty, ps, () => control.TotalCount = value, bindingMode, converter, bindingSource);
public static T TotalCount<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Int32> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Pagination
=> control._setEx(Ursa.Controls.Pagination.TotalCountProperty, ps, () => control.TotalCount = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T PageSize<T>(this T control, IBinding binding) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.PageSizeProperty, binding);
public static T PageSize<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.PageSizeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T PageSize<T>(this T control, Func<System.Int32> func, Action<System.Int32>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.PageSizeProperty, func, onChanged, expression);
public static T PageSize<T>(this T control, System.Int32 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Pagination
=> control._setEx(Ursa.Controls.Pagination.PageSizeProperty, ps, () => control.PageSize = value, bindingMode, converter, bindingSource);
public static T PageSize<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Int32> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Pagination
=> control._setEx(Ursa.Controls.Pagination.PageSizeProperty, ps, () => control.PageSize = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T PageSizeOptions<T>(this T control, IBinding binding) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.PageSizeOptionsProperty, binding);
public static T PageSizeOptions<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.PageSizeOptionsProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T PageSizeOptions<T>(this T control, Func<Avalonia.Collections.AvaloniaList<System.Int32>> func, Action<Avalonia.Collections.AvaloniaList<System.Int32>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.PageSizeOptionsProperty, func, onChanged, expression);
public static T PageSizeOptions<T>(this T control, Avalonia.Collections.AvaloniaList<System.Int32> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Pagination
=> control._setEx(Ursa.Controls.Pagination.PageSizeOptionsProperty, ps, () => control.PageSizeOptions = value, bindingMode, converter, bindingSource);
public static T PageSizeOptions<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Collections.AvaloniaList<System.Int32>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Pagination
=> control._setEx(Ursa.Controls.Pagination.PageSizeOptionsProperty, ps, () => control.PageSizeOptions = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T PageButtonTheme<T>(this T control, IBinding binding) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.PageButtonThemeProperty, binding);
public static T PageButtonTheme<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.PageButtonThemeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T PageButtonTheme<T>(this T control, Func<Avalonia.Styling.ControlTheme> func, Action<Avalonia.Styling.ControlTheme>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.PageButtonThemeProperty, func, onChanged, expression);
public static T PageButtonTheme<T>(this T control, Avalonia.Styling.ControlTheme value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Pagination
=> control._setEx(Ursa.Controls.Pagination.PageButtonThemeProperty, ps, () => control.PageButtonTheme = value, bindingMode, converter, bindingSource);
public static T PageButtonTheme<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ControlTheme> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Pagination
=> control._setEx(Ursa.Controls.Pagination.PageButtonThemeProperty, ps, () => control.PageButtonTheme = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ShowPageSizeSelector<T>(this T control, IBinding binding) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.ShowPageSizeSelectorProperty, binding);
public static T ShowPageSizeSelector<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.ShowPageSizeSelectorProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ShowPageSizeSelector<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.ShowPageSizeSelectorProperty, func, onChanged, expression);
public static T ShowPageSizeSelector<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Pagination
=> control._setEx(Ursa.Controls.Pagination.ShowPageSizeSelectorProperty, ps, () => control.ShowPageSizeSelector = value, bindingMode, converter, bindingSource);
public static T ShowPageSizeSelector<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Pagination
=> control._setEx(Ursa.Controls.Pagination.ShowPageSizeSelectorProperty, ps, () => control.ShowPageSizeSelector = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ShowQuickJump<T>(this T control, IBinding binding) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.ShowQuickJumpProperty, binding);
public static T ShowQuickJump<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.ShowQuickJumpProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ShowQuickJump<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Pagination
   => control._set(Ursa.Controls.Pagination.ShowQuickJumpProperty, func, onChanged, expression);
public static T ShowQuickJump<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Pagination
=> control._setEx(Ursa.Controls.Pagination.ShowQuickJumpProperty, ps, () => control.ShowQuickJump = value, bindingMode, converter, bindingSource);
public static T ShowQuickJump<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Pagination
=> control._setEx(Ursa.Controls.Pagination.ShowQuickJumpProperty, ps, () => control.ShowQuickJump = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

