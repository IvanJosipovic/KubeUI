#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class NavMenu_MarkupExtensions
{
//================= Properties ======================//
 // SelectedItemProperty

/*BindFromExpressionSetterGenerator*/
public static T SelectedItem<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.SelectedItemProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SelectedItem<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.SelectedItemProperty, ps, () => control.SelectedItem = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SelectedItem<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.SelectedItemProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SelectedItem<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.SelectedItemProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SelectedItem<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.SelectedItemProperty, ps, () => control.SelectedItem = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IconBindingProperty

/*BindFromExpressionSetterGenerator*/
public static T IconBinding<T>(this T control, Func<Avalonia.Data.IBinding> func, Action<Avalonia.Data.IBinding>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.IconBindingProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IconBinding<T>(this T control, Avalonia.Data.IBinding value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.IconBindingProperty, ps, () => control.IconBinding = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IconBinding<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.IconBindingProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IconBinding<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.IconBindingProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IconBinding<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Data.IBinding> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.IconBindingProperty, ps, () => control.IconBinding = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // HeaderBindingProperty

/*BindFromExpressionSetterGenerator*/
public static T HeaderBinding<T>(this T control, Func<Avalonia.Data.IBinding> func, Action<Avalonia.Data.IBinding>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.HeaderBindingProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T HeaderBinding<T>(this T control, Avalonia.Data.IBinding value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.HeaderBindingProperty, ps, () => control.HeaderBinding = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T HeaderBinding<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.HeaderBindingProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T HeaderBinding<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.HeaderBindingProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T HeaderBinding<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Data.IBinding> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.HeaderBindingProperty, ps, () => control.HeaderBinding = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SubMenuBindingProperty

/*BindFromExpressionSetterGenerator*/
public static T SubMenuBinding<T>(this T control, Func<Avalonia.Data.IBinding> func, Action<Avalonia.Data.IBinding>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.SubMenuBindingProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SubMenuBinding<T>(this T control, Avalonia.Data.IBinding value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.SubMenuBindingProperty, ps, () => control.SubMenuBinding = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SubMenuBinding<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.SubMenuBindingProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SubMenuBinding<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.SubMenuBindingProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SubMenuBinding<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Data.IBinding> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.SubMenuBindingProperty, ps, () => control.SubMenuBinding = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CommandBindingProperty

/*BindFromExpressionSetterGenerator*/
public static T CommandBinding<T>(this T control, Func<Avalonia.Data.IBinding> func, Action<Avalonia.Data.IBinding>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.CommandBindingProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CommandBinding<T>(this T control, Avalonia.Data.IBinding value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.CommandBindingProperty, ps, () => control.CommandBinding = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CommandBinding<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.CommandBindingProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CommandBinding<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.CommandBindingProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CommandBinding<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Data.IBinding> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.CommandBindingProperty, ps, () => control.CommandBinding = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // HeaderTemplateProperty

/*BindFromExpressionSetterGenerator*/
public static T HeaderTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.HeaderTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T HeaderTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.HeaderTemplateProperty, ps, () => control.HeaderTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T HeaderTemplate<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.HeaderTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T HeaderTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.HeaderTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T HeaderTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.HeaderTemplateProperty, ps, () => control.HeaderTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IconTemplateProperty

/*BindFromExpressionSetterGenerator*/
public static T IconTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.IconTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IconTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.IconTemplateProperty, ps, () => control.IconTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IconTemplate<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.IconTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IconTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.IconTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IconTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.IconTemplateProperty, ps, () => control.IconTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SubMenuIndentProperty

/*BindFromExpressionSetterGenerator*/
public static T SubMenuIndent<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.SubMenuIndentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SubMenuIndent<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.SubMenuIndentProperty, ps, () => control.SubMenuIndent = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SubMenuIndent<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.SubMenuIndentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SubMenuIndent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.SubMenuIndentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SubMenuIndent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.SubMenuIndentProperty, ps, () => control.SubMenuIndent = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsHorizontalCollapsedProperty

/*BindFromExpressionSetterGenerator*/
public static T IsHorizontalCollapsed<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.IsHorizontalCollapsedProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsHorizontalCollapsed<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.IsHorizontalCollapsedProperty, ps, () => control.IsHorizontalCollapsed = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsHorizontalCollapsed<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.IsHorizontalCollapsedProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsHorizontalCollapsed<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.IsHorizontalCollapsedProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsHorizontalCollapsed<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.IsHorizontalCollapsedProperty, ps, () => control.IsHorizontalCollapsed = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // HeaderProperty

/*BindFromExpressionSetterGenerator*/
public static T Header<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.HeaderProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Header<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.HeaderProperty, ps, () => control.Header = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Header<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.HeaderProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Header<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.HeaderProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Header<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.HeaderProperty, ps, () => control.Header = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // FooterProperty

/*BindFromExpressionSetterGenerator*/
public static T Footer<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.FooterProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Footer<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.FooterProperty, ps, () => control.Footer = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Footer<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.FooterProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Footer<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.FooterProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Footer<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.FooterProperty, ps, () => control.Footer = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ExpandWidthProperty

/*BindFromExpressionSetterGenerator*/
public static T ExpandWidth<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.ExpandWidthProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ExpandWidth<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.ExpandWidthProperty, ps, () => control.ExpandWidth = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ExpandWidth<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.ExpandWidthProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ExpandWidth<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.ExpandWidthProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ExpandWidth<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.ExpandWidthProperty, ps, () => control.ExpandWidth = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CollapseWidthProperty

/*BindFromExpressionSetterGenerator*/
public static T CollapseWidth<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.CollapseWidthProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CollapseWidth<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.CollapseWidthProperty, ps, () => control.CollapseWidth = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CollapseWidth<T>(this T control, IBinding binding) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.CollapseWidthProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CollapseWidth<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.NavMenu
   => control._set(Ursa.Controls.NavMenu.CollapseWidthProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CollapseWidth<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.NavMenu
=> control._setEx(Ursa.Controls.NavMenu.CollapseWidthProperty, ps, () => control.CollapseWidth = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // SelectionChanged

/*ActionToEventGenerator*/
    public static T OnSelectionChanged<T>(this T control, Action<Avalonia.Controls.SelectionChangedEventArgs> action) where T : Ursa.Controls.NavMenu => 
        control._setEvent((System.EventHandler<Avalonia.Controls.SelectionChangedEventArgs>) ((arg0, arg1) => action(arg1)), h => control.SelectionChanged += h);



//================= Styles ======================//
 // SelectedItemProperty

/*ValueStyleSetterGenerator*/
public static Style<T> SelectedItem<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.SelectedItemProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SelectedItem<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.SelectedItemProperty, binding);


 // IconBindingProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IconBinding<T>(this Style<T> style, Avalonia.Data.IBinding value) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.IconBindingProperty, value);

/*BindingStyleSetterGenerator*/
//Skipped IconBinding because already exist in value setters


 // HeaderBindingProperty

/*ValueStyleSetterGenerator*/
public static Style<T> HeaderBinding<T>(this Style<T> style, Avalonia.Data.IBinding value) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.HeaderBindingProperty, value);

/*BindingStyleSetterGenerator*/
//Skipped HeaderBinding because already exist in value setters


 // SubMenuBindingProperty

/*ValueStyleSetterGenerator*/
public static Style<T> SubMenuBinding<T>(this Style<T> style, Avalonia.Data.IBinding value) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.SubMenuBindingProperty, value);

/*BindingStyleSetterGenerator*/
//Skipped SubMenuBinding because already exist in value setters


 // CommandBindingProperty

/*ValueStyleSetterGenerator*/
public static Style<T> CommandBinding<T>(this Style<T> style, Avalonia.Data.IBinding value) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.CommandBindingProperty, value);

/*BindingStyleSetterGenerator*/
//Skipped CommandBinding because already exist in value setters


 // HeaderTemplateProperty

/*ValueStyleSetterGenerator*/
public static Style<T> HeaderTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.HeaderTemplateProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> HeaderTemplate<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.HeaderTemplateProperty, binding);


 // IconTemplateProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IconTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.IconTemplateProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IconTemplate<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.IconTemplateProperty, binding);


 // SubMenuIndentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> SubMenuIndent<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.SubMenuIndentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SubMenuIndent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.SubMenuIndentProperty, binding);


 // IsHorizontalCollapsedProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsHorizontalCollapsed<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.IsHorizontalCollapsedProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsHorizontalCollapsed<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.IsHorizontalCollapsedProperty, binding);


 // HeaderProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Header<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.HeaderProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Header<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.HeaderProperty, binding);


 // FooterProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Footer<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.FooterProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Footer<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.FooterProperty, binding);


 // ExpandWidthProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ExpandWidth<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.ExpandWidthProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ExpandWidth<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.ExpandWidthProperty, binding);


 // CollapseWidthProperty

/*ValueStyleSetterGenerator*/
public static Style<T> CollapseWidth<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.CollapseWidthProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CollapseWidth<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.CollapseWidthProperty, binding);



}
