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
public static partial class TaskDialogButtonsPanel_MarkupExtensions
{
//================= Properties ======================//
 // SpacingProperty

/*BindFromExpressionSetterGenerator*/
public static T Spacing<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.Primitives.TaskDialogButtonsPanel
   => control._set(FluentAvalonia.UI.Controls.Primitives.TaskDialogButtonsPanel.SpacingProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Spacing<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Primitives.TaskDialogButtonsPanel
=> control._setEx(FluentAvalonia.UI.Controls.Primitives.TaskDialogButtonsPanel.SpacingProperty, ps, () => control.Spacing = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Spacing<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.Primitives.TaskDialogButtonsPanel
   => control._set(FluentAvalonia.UI.Controls.Primitives.TaskDialogButtonsPanel.SpacingProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Spacing<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.Primitives.TaskDialogButtonsPanel
   => control._set(FluentAvalonia.UI.Controls.Primitives.TaskDialogButtonsPanel.SpacingProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Spacing<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Primitives.TaskDialogButtonsPanel
=> control._setEx(FluentAvalonia.UI.Controls.Primitives.TaskDialogButtonsPanel.SpacingProperty, ps, () => control.Spacing = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//

}
