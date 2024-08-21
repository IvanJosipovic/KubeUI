#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class TaskDialogCommandHost_MarkupExtensions
{
//================= Properties ======================//
 // DescriptionProperty

/*BindFromExpressionSetterGenerator*/
public static T Description<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.Primitives.TaskDialogCommandHost
   => control._set(FluentAvalonia.UI.Controls.Primitives.TaskDialogCommandHost.DescriptionProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Description<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Primitives.TaskDialogCommandHost
=> control._setEx(FluentAvalonia.UI.Controls.Primitives.TaskDialogCommandHost.DescriptionProperty, ps, () => control.Description = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Description<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.Primitives.TaskDialogCommandHost
   => control._set(FluentAvalonia.UI.Controls.Primitives.TaskDialogCommandHost.DescriptionProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Description<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.Primitives.TaskDialogCommandHost
   => control._set(FluentAvalonia.UI.Controls.Primitives.TaskDialogCommandHost.DescriptionProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Description<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Primitives.TaskDialogCommandHost
=> control._setEx(FluentAvalonia.UI.Controls.Primitives.TaskDialogCommandHost.DescriptionProperty, ps, () => control.Description = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // DescriptionProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Description<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.Primitives.TaskDialogCommandHost
=> style._addSetter(FluentAvalonia.UI.Controls.Primitives.TaskDialogCommandHost.DescriptionProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Description<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.Primitives.TaskDialogCommandHost
=> style._addSetter(FluentAvalonia.UI.Controls.Primitives.TaskDialogCommandHost.DescriptionProperty, binding);



}
