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
public static partial class TaskDialogCommand_MarkupExtensions
{
//================= Properties ======================//
 // Description

/*BindFromExpressionSetterGenerator*/
public static T Description<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TaskDialogCommand 
   => control._set(FluentAvalonia.UI.Controls.TaskDialogCommand.DescriptionProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Description<T>(this T control,System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialogCommand 
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialogCommand.DescriptionProperty, ps, () => control.Description = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Description<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialogCommand 
   => control._set(FluentAvalonia.UI.Controls.TaskDialogCommand.DescriptionProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Description<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TaskDialogCommand 
   => control._set(FluentAvalonia.UI.Controls.TaskDialogCommand.DescriptionProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Description<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialogCommand 
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialogCommand.DescriptionProperty, ps, () => control.Description = converter.TryConvert(value), bindingMode, converter, bindingSource);



}
