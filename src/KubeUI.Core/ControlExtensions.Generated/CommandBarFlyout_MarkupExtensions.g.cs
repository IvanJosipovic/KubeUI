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
public static partial class CommandBarFlyout_MarkupExtensions
{
//================= Properties ======================//
 // AlwaysExpanded

/*ValueSetterGenerator*/
public static T AlwaysExpanded<T>(this T control, System.Boolean value) where T : FluentAvalonia.UI.Controls.CommandBarFlyout 
=> control._set(() => control.AlwaysExpanded = value!);

/*BindFromExpressionSetterGenerator*/
public static T AlwaysExpanded<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : FluentAvalonia.UI.Controls.CommandBarFlyout 
   => control._set(FluentAvalonia.UI.Controls.CommandBarFlyout.AlwaysExpandedProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T AlwaysExpanded<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBarFlyout 
=> control._setEx(FluentAvalonia.UI.Controls.CommandBarFlyout.AlwaysExpandedProperty, ps, () => control.AlwaysExpanded = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T AlwaysExpanded<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBarFlyout 
   => control._set(FluentAvalonia.UI.Controls.CommandBarFlyout.AlwaysExpandedProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T AlwaysExpanded<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.CommandBarFlyout 
   => control._set(FluentAvalonia.UI.Controls.CommandBarFlyout.AlwaysExpandedProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T AlwaysExpanded<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBarFlyout 
=> control._setEx(FluentAvalonia.UI.Controls.CommandBarFlyout.AlwaysExpandedProperty, ps, () => control.AlwaysExpanded = converter.TryConvert(value)!, bindingMode, converter, bindingSource);



}
