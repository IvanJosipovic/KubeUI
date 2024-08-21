#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class NavMenuItem_MarkupExtensions
{
//================= Properties ======================//
 // IconProperty

/*BindFromExpressionSetterGenerator*/
public static T Icon<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.IconProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Icon<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenuItem
=> control._setEx(Ursa.Controls.NavMenuItem.IconProperty, ps, () => control.Icon = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Icon<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.IconProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Icon<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.IconProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Icon<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenuItem
=> control._setEx(Ursa.Controls.NavMenuItem.IconProperty, ps, () => control.Icon = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IconTemplateProperty

/*BindFromExpressionSetterGenerator*/
public static T IconTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.IconTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IconTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenuItem
=> control._setEx(Ursa.Controls.NavMenuItem.IconTemplateProperty, ps, () => control.IconTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IconTemplate<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.IconTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IconTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.IconTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IconTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenuItem
=> control._setEx(Ursa.Controls.NavMenuItem.IconTemplateProperty, ps, () => control.IconTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CommandProperty

/*BindFromExpressionSetterGenerator*/
public static T Command<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.CommandProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Command<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenuItem
=> control._setEx(Ursa.Controls.NavMenuItem.CommandProperty, ps, () => control.Command = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Command<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.CommandProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Command<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.CommandProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Command<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenuItem
=> control._setEx(Ursa.Controls.NavMenuItem.CommandProperty, ps, () => control.Command = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CommandParameterProperty

/*BindFromExpressionSetterGenerator*/
public static T CommandParameter<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.CommandParameterProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CommandParameter<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenuItem
=> control._setEx(Ursa.Controls.NavMenuItem.CommandParameterProperty, ps, () => control.CommandParameter = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CommandParameter<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.CommandParameterProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CommandParameter<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.CommandParameterProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CommandParameter<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenuItem
=> control._setEx(Ursa.Controls.NavMenuItem.CommandParameterProperty, ps, () => control.CommandParameter = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsSelectedProperty

/*BindFromExpressionSetterGenerator*/
public static T IsSelected<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.IsSelectedProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsSelected<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenuItem
=> control._setEx(Ursa.Controls.NavMenuItem.IsSelectedProperty, ps, () => control.IsSelected = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsSelected<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.IsSelectedProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsSelected<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.IsSelectedProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsSelected<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenuItem
=> control._setEx(Ursa.Controls.NavMenuItem.IsSelectedProperty, ps, () => control.IsSelected = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsHorizontalCollapsedProperty

/*BindFromExpressionSetterGenerator*/
public static T IsHorizontalCollapsed<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.IsHorizontalCollapsedProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsHorizontalCollapsed<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenuItem
=> control._setEx(Ursa.Controls.NavMenuItem.IsHorizontalCollapsedProperty, ps, () => control.IsHorizontalCollapsed = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsHorizontalCollapsed<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.IsHorizontalCollapsedProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsHorizontalCollapsed<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.IsHorizontalCollapsedProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsHorizontalCollapsed<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenuItem
=> control._setEx(Ursa.Controls.NavMenuItem.IsHorizontalCollapsedProperty, ps, () => control.IsHorizontalCollapsed = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsVerticalCollapsedProperty

/*BindFromExpressionSetterGenerator*/
public static T IsVerticalCollapsed<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.IsVerticalCollapsedProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsVerticalCollapsed<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenuItem
=> control._setEx(Ursa.Controls.NavMenuItem.IsVerticalCollapsedProperty, ps, () => control.IsVerticalCollapsed = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsVerticalCollapsed<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.IsVerticalCollapsedProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsVerticalCollapsed<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.IsVerticalCollapsedProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsVerticalCollapsed<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenuItem
=> control._setEx(Ursa.Controls.NavMenuItem.IsVerticalCollapsedProperty, ps, () => control.IsVerticalCollapsed = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SubMenuIndentProperty

/*BindFromExpressionSetterGenerator*/
public static T SubMenuIndent<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.SubMenuIndentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SubMenuIndent<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenuItem
=> control._setEx(Ursa.Controls.NavMenuItem.SubMenuIndentProperty, ps, () => control.SubMenuIndent = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SubMenuIndent<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.SubMenuIndentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SubMenuIndent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.SubMenuIndentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SubMenuIndent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenuItem
=> control._setEx(Ursa.Controls.NavMenuItem.SubMenuIndentProperty, ps, () => control.SubMenuIndent = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsSeparatorProperty

/*BindFromExpressionSetterGenerator*/
public static T IsSeparator<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.IsSeparatorProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsSeparator<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenuItem
=> control._setEx(Ursa.Controls.NavMenuItem.IsSeparatorProperty, ps, () => control.IsSeparator = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsSeparator<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.IsSeparatorProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsSeparator<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenuItem
   => control._set(Ursa.Controls.NavMenuItem.IsSeparatorProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsSeparator<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenuItem
=> control._setEx(Ursa.Controls.NavMenuItem.IsSeparatorProperty, ps, () => control.IsSeparator = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // IconProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Icon<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.NavMenuItem
=> style._addSetter(Ursa.Controls.NavMenuItem.IconProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Icon<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NavMenuItem
=> style._addSetter(Ursa.Controls.NavMenuItem.IconProperty, binding);


 // IconTemplateProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IconTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : Ursa.Controls.NavMenuItem
=> style._addSetter(Ursa.Controls.NavMenuItem.IconTemplateProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IconTemplate<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NavMenuItem
=> style._addSetter(Ursa.Controls.NavMenuItem.IconTemplateProperty, binding);


 // CommandProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Command<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : Ursa.Controls.NavMenuItem
=> style._addSetter(Ursa.Controls.NavMenuItem.CommandProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Command<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NavMenuItem
=> style._addSetter(Ursa.Controls.NavMenuItem.CommandProperty, binding);


 // CommandParameterProperty

/*ValueStyleSetterGenerator*/
public static Style<T> CommandParameter<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.NavMenuItem
=> style._addSetter(Ursa.Controls.NavMenuItem.CommandParameterProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CommandParameter<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NavMenuItem
=> style._addSetter(Ursa.Controls.NavMenuItem.CommandParameterProperty, binding);


 // IsSelectedProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsSelected<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.NavMenuItem
=> style._addSetter(Ursa.Controls.NavMenuItem.IsSelectedProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsSelected<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NavMenuItem
=> style._addSetter(Ursa.Controls.NavMenuItem.IsSelectedProperty, binding);


 // IsHorizontalCollapsedProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsHorizontalCollapsed<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.NavMenuItem
=> style._addSetter(Ursa.Controls.NavMenuItem.IsHorizontalCollapsedProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsHorizontalCollapsed<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NavMenuItem
=> style._addSetter(Ursa.Controls.NavMenuItem.IsHorizontalCollapsedProperty, binding);


 // IsVerticalCollapsedProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsVerticalCollapsed<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.NavMenuItem
=> style._addSetter(Ursa.Controls.NavMenuItem.IsVerticalCollapsedProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsVerticalCollapsed<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NavMenuItem
=> style._addSetter(Ursa.Controls.NavMenuItem.IsVerticalCollapsedProperty, binding);


 // SubMenuIndentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> SubMenuIndent<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.NavMenuItem
=> style._addSetter(Ursa.Controls.NavMenuItem.SubMenuIndentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SubMenuIndent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NavMenuItem
=> style._addSetter(Ursa.Controls.NavMenuItem.SubMenuIndentProperty, binding);


 // IsSeparatorProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsSeparator<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.NavMenuItem
=> style._addSetter(Ursa.Controls.NavMenuItem.IsSeparatorProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsSeparator<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NavMenuItem
=> style._addSetter(Ursa.Controls.NavMenuItem.IsSeparatorProperty, binding);



}
