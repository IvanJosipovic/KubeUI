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
public static Style<T> CurrentPage<T>(this Style<T> style, System.Nullable<System.Int32> value) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.CurrentPageProperty, value);
public static Style<T> CurrentPage<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.CurrentPageProperty, binding);
public static Style<T> Command<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.CommandProperty, value);
public static Style<T> Command<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.CommandProperty, binding);
public static Style<T> CommandParameter<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.CommandParameterProperty, value);
public static Style<T> CommandParameter<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.CommandParameterProperty, binding);
public static Style<T> TotalCount<T>(this Style<T> style, System.Int32 value) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.TotalCountProperty, value);
public static Style<T> TotalCount<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.TotalCountProperty, binding);
public static Style<T> PageSize<T>(this Style<T> style, System.Int32 value) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.PageSizeProperty, value);
public static Style<T> PageSize<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.PageSizeProperty, binding);
public static Style<T> PageSizeOptions<T>(this Style<T> style, Avalonia.Collections.AvaloniaList<System.Int32> value) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.PageSizeOptionsProperty, value);
public static Style<T> PageSizeOptions<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.PageSizeOptionsProperty, binding);
public static Style<T> PageButtonTheme<T>(this Style<T> style, Avalonia.Styling.ControlTheme value) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.PageButtonThemeProperty, value);
public static Style<T> PageButtonTheme<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.PageButtonThemeProperty, binding);
public static Style<T> ShowPageSizeSelector<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.ShowPageSizeSelectorProperty, value);
public static Style<T> ShowPageSizeSelector<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.ShowPageSizeSelectorProperty, binding);
public static Style<T> ShowQuickJump<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.ShowQuickJumpProperty, value);
public static Style<T> ShowQuickJump<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.ShowQuickJumpProperty, binding);
public static Style<T> DisplayCurrentPageInQuickJumper<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.DisplayCurrentPageInQuickJumperProperty, value);
public static Style<T> DisplayCurrentPageInQuickJumper<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Pagination
=> style._addSetter(Ursa.Controls.Pagination.DisplayCurrentPageInQuickJumperProperty, binding);
}

