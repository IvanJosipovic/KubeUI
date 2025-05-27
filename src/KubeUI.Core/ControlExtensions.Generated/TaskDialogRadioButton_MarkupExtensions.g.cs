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
public static partial class TaskDialogRadioButton_MarkupExtensions
{
//================= Properties ======================//
 // IsChecked

/*ValueSetterGenerator*/
public static T IsChecked<T>(this T control, System.Nullable<System.Boolean> value) where T : FluentAvalonia.UI.Controls.TaskDialogRadioButton 
=> control._set(() => control.IsChecked = value!);

/*BindFromExpressionSetterGenerator*/
public static T IsChecked<T>(this T control, Func<System.Nullable<System.Boolean>> func, Action<System.Nullable<System.Boolean>>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : FluentAvalonia.UI.Controls.TaskDialogRadioButton 
   => control._set(FluentAvalonia.UI.Controls.TaskDialogRadioButton.IsCheckedProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T IsChecked<T>(this T control,System.Nullable<System.Boolean> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialogRadioButton 
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialogRadioButton.IsCheckedProperty, ps, () => control.IsChecked = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsChecked<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialogRadioButton 
   => control._set(FluentAvalonia.UI.Controls.TaskDialogRadioButton.IsCheckedProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsChecked<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TaskDialogRadioButton 
   => control._set(FluentAvalonia.UI.Controls.TaskDialogRadioButton.IsCheckedProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T IsChecked<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Nullable<System.Boolean>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : FluentAvalonia.UI.Controls.TaskDialogRadioButton 
=> control._setEx(FluentAvalonia.UI.Controls.TaskDialogRadioButton.IsCheckedProperty, ps, () => control.IsChecked = converter.TryConvert(value)!, bindingMode, converter, bindingSource);



}
