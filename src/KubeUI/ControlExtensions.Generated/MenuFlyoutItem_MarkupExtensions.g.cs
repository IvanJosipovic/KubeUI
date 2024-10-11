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
public static partial class MenuFlyoutItem_MarkupExtensions
{
//================= Properties ======================//
 // Text

/*BindFromExpressionSetterGenerator*/
public static T Text<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutItem.TextProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Text<T>(this T control,System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutItem.TextProperty, ps, () => control.Text = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Text<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutItem.TextProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Text<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutItem.TextProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Text<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutItem.TextProperty, ps, () => control.Text = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IconSource

/*BindFromExpressionSetterGenerator*/
public static T IconSource<T>(this T control, Func<FluentAvalonia.UI.Controls.IconSource> func, Action<FluentAvalonia.UI.Controls.IconSource>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutItem.IconSourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IconSource<T>(this T control,FluentAvalonia.UI.Controls.IconSource value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutItem.IconSourceProperty, ps, () => control.IconSource = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IconSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutItem.IconSourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IconSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutItem.IconSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IconSource<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.IconSource> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutItem.IconSourceProperty, ps, () => control.IconSource = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // Command

/*BindFromExpressionSetterGenerator*/
public static T Command<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutItem.CommandProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Command<T>(this T control,System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutItem.CommandProperty, ps, () => control.Command = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Command<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutItem.CommandProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Command<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutItem.CommandProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Command<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutItem.CommandProperty, ps, () => control.Command = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CommandParameter

/*BindFromExpressionSetterGenerator*/
public static T CommandParameter<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutItem.CommandParameterProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CommandParameter<T>(this T control,System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutItem.CommandParameterProperty, ps, () => control.CommandParameter = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CommandParameter<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutItem.CommandParameterProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CommandParameter<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutItem.CommandParameterProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CommandParameter<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutItem.CommandParameterProperty, ps, () => control.CommandParameter = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // HotKey

/*BindFromExpressionSetterGenerator*/
public static T HotKey<T>(this T control, Func<Avalonia.Input.KeyGesture> func, Action<Avalonia.Input.KeyGesture>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutItem.HotKeyProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T HotKey<T>(this T control,Avalonia.Input.KeyGesture value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutItem.HotKeyProperty, ps, () => control.HotKey = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T HotKey<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutItem.HotKeyProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T HotKey<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutItem.HotKeyProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T HotKey<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Input.KeyGesture> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutItem.HotKeyProperty, ps, () => control.HotKey = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // InputGesture

/*BindFromExpressionSetterGenerator*/
public static T InputGesture<T>(this T control, Func<Avalonia.Input.KeyGesture> func, Action<Avalonia.Input.KeyGesture>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutItem.InputGestureProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T InputGesture<T>(this T control,Avalonia.Input.KeyGesture value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutItem.InputGestureProperty, ps, () => control.InputGesture = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T InputGesture<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutItem.InputGestureProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T InputGesture<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
   => control._set(FluentAvalonia.UI.Controls.MenuFlyoutItem.InputGestureProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T InputGesture<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Input.KeyGesture> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
=> control._setEx(FluentAvalonia.UI.Controls.MenuFlyoutItem.InputGestureProperty, ps, () => control.InputGesture = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // Click

/*ActionToEventGenerator*/
public static T OnClick<T>(this T control, Action<Avalonia.Interactivity.RoutedEventArgs> action, Avalonia.Interactivity.RoutingStrategies routes = Avalonia.Interactivity.RoutingStrategies.Tunnel | Avalonia.Interactivity.RoutingStrategies.Bubble) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
{
  control.AddHandler(FluentAvalonia.UI.Controls.MenuFlyoutItem.ClickEvent, (_, args) => action(args), routes);
  return control; 
}




//================= Styles ======================//
 // Text

/*ValueStyleSetterGenerator*/
public static Style<T> Text<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutItem.TextProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Text<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutItem.TextProperty, binding);


 // IconSource

/*ValueStyleSetterGenerator*/
public static Style<T> IconSource<T>(this Style<T> style, FluentAvalonia.UI.Controls.IconSource value) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutItem.IconSourceProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IconSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutItem.IconSourceProperty, binding);


 // Command

/*ValueStyleSetterGenerator*/
public static Style<T> Command<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutItem.CommandProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Command<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutItem.CommandProperty, binding);


 // CommandParameter

/*ValueStyleSetterGenerator*/
public static Style<T> CommandParameter<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutItem.CommandParameterProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CommandParameter<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutItem.CommandParameterProperty, binding);


 // HotKey

/*ValueStyleSetterGenerator*/
public static Style<T> HotKey<T>(this Style<T> style, Avalonia.Input.KeyGesture value) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutItem.HotKeyProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> HotKey<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutItem.HotKeyProperty, binding);


 // InputGesture

/*ValueStyleSetterGenerator*/
public static Style<T> InputGesture<T>(this Style<T> style, Avalonia.Input.KeyGesture value) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutItem.InputGestureProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> InputGesture<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.MenuFlyoutItem 
=> style._addSetter(FluentAvalonia.UI.Controls.MenuFlyoutItem.InputGestureProperty, binding);



}
