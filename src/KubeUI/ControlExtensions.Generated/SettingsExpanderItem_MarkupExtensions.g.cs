#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class SettingsExpanderItem_MarkupExtensions
{
//================= Properties ======================//
 // DescriptionProperty

/*BindFromExpressionSetterGenerator*/
public static T Description<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.DescriptionProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Description<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> control._setEx(FluentAvalonia.UI.Controls.SettingsExpanderItem.DescriptionProperty, ps, () => control.Description = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Description<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.DescriptionProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Description<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.DescriptionProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Description<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> control._setEx(FluentAvalonia.UI.Controls.SettingsExpanderItem.DescriptionProperty, ps, () => control.Description = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IconSourceProperty

/*BindFromExpressionSetterGenerator*/
public static T IconSource<T>(this T control, Func<FluentAvalonia.UI.Controls.IconSource> func, Action<FluentAvalonia.UI.Controls.IconSource>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.IconSourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IconSource<T>(this T control, FluentAvalonia.UI.Controls.IconSource value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> control._setEx(FluentAvalonia.UI.Controls.SettingsExpanderItem.IconSourceProperty, ps, () => control.IconSource = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IconSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.IconSourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IconSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.IconSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IconSource<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.IconSource> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> control._setEx(FluentAvalonia.UI.Controls.SettingsExpanderItem.IconSourceProperty, ps, () => control.IconSource = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // FooterProperty

/*BindFromExpressionSetterGenerator*/
public static T Footer<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.FooterProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Footer<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> control._setEx(FluentAvalonia.UI.Controls.SettingsExpanderItem.FooterProperty, ps, () => control.Footer = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Footer<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.FooterProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Footer<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.FooterProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Footer<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> control._setEx(FluentAvalonia.UI.Controls.SettingsExpanderItem.FooterProperty, ps, () => control.Footer = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // FooterTemplateProperty

/*BindFromExpressionSetterGenerator*/
public static T FooterTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.FooterTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T FooterTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> control._setEx(FluentAvalonia.UI.Controls.SettingsExpanderItem.FooterTemplateProperty, ps, () => control.FooterTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T FooterTemplate<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.FooterTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T FooterTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.FooterTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T FooterTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> control._setEx(FluentAvalonia.UI.Controls.SettingsExpanderItem.FooterTemplateProperty, ps, () => control.FooterTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ActionIconSourceProperty

/*BindFromExpressionSetterGenerator*/
public static T ActionIconSource<T>(this T control, Func<FluentAvalonia.UI.Controls.IconSource> func, Action<FluentAvalonia.UI.Controls.IconSource>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.ActionIconSourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ActionIconSource<T>(this T control, FluentAvalonia.UI.Controls.IconSource value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> control._setEx(FluentAvalonia.UI.Controls.SettingsExpanderItem.ActionIconSourceProperty, ps, () => control.ActionIconSource = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ActionIconSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.ActionIconSourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ActionIconSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.ActionIconSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ActionIconSource<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.IconSource> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> control._setEx(FluentAvalonia.UI.Controls.SettingsExpanderItem.ActionIconSourceProperty, ps, () => control.ActionIconSource = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsClickEnabledProperty

/*BindFromExpressionSetterGenerator*/
public static T IsClickEnabled<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.IsClickEnabledProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsClickEnabled<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> control._setEx(FluentAvalonia.UI.Controls.SettingsExpanderItem.IsClickEnabledProperty, ps, () => control.IsClickEnabled = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsClickEnabled<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.IsClickEnabledProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsClickEnabled<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.IsClickEnabledProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsClickEnabled<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> control._setEx(FluentAvalonia.UI.Controls.SettingsExpanderItem.IsClickEnabledProperty, ps, () => control.IsClickEnabled = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CommandProperty

/*BindFromExpressionSetterGenerator*/
public static T Command<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.CommandProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Command<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> control._setEx(FluentAvalonia.UI.Controls.SettingsExpanderItem.CommandProperty, ps, () => control.Command = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Command<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.CommandProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Command<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.CommandProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Command<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> control._setEx(FluentAvalonia.UI.Controls.SettingsExpanderItem.CommandProperty, ps, () => control.Command = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CommandParameterProperty

/*BindFromExpressionSetterGenerator*/
public static T CommandParameter<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.CommandParameterProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CommandParameter<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> control._setEx(FluentAvalonia.UI.Controls.SettingsExpanderItem.CommandParameterProperty, ps, () => control.CommandParameter = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CommandParameter<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.CommandParameterProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CommandParameter<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
   => control._set(FluentAvalonia.UI.Controls.SettingsExpanderItem.CommandParameterProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CommandParameter<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> control._setEx(FluentAvalonia.UI.Controls.SettingsExpanderItem.CommandParameterProperty, ps, () => control.CommandParameter = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // Click

/*ActionToEventGenerator*/
    public static T OnClick<T>(this T control, Action<Avalonia.Interactivity.RoutedEventArgs> action) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem => 
        control._setEvent((System.EventHandler<Avalonia.Interactivity.RoutedEventArgs>) ((arg0, arg1) => action(arg1)), h => control.Click += h);



//================= Styles ======================//
 // DescriptionProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Description<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpanderItem.DescriptionProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Description<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpanderItem.DescriptionProperty, binding);


 // IconSourceProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IconSource<T>(this Style<T> style, FluentAvalonia.UI.Controls.IconSource value) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpanderItem.IconSourceProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IconSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpanderItem.IconSourceProperty, binding);


 // FooterProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Footer<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpanderItem.FooterProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Footer<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpanderItem.FooterProperty, binding);


 // FooterTemplateProperty

/*ValueStyleSetterGenerator*/
public static Style<T> FooterTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpanderItem.FooterTemplateProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> FooterTemplate<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpanderItem.FooterTemplateProperty, binding);


 // ActionIconSourceProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ActionIconSource<T>(this Style<T> style, FluentAvalonia.UI.Controls.IconSource value) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpanderItem.ActionIconSourceProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ActionIconSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpanderItem.ActionIconSourceProperty, binding);


 // IsClickEnabledProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsClickEnabled<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpanderItem.IsClickEnabledProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsClickEnabled<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpanderItem.IsClickEnabledProperty, binding);


 // CommandProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Command<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpanderItem.CommandProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Command<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpanderItem.CommandProperty, binding);


 // CommandParameterProperty

/*ValueStyleSetterGenerator*/
public static Style<T> CommandParameter<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpanderItem.CommandParameterProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CommandParameter<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpanderItem.CommandParameterProperty, binding);



}
