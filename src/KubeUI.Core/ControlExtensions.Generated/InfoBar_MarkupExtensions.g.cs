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
public static partial class InfoBar_MarkupExtensions
{
//================= Properties ======================//
 // IsOpen

/*BindFromExpressionSetterGenerator*/
public static T IsOpen<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.InfoBar 
   => control._set(FluentAvalonia.UI.Controls.InfoBar.IsOpenProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsOpen<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar 
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.IsOpenProperty, ps, () => control.IsOpen = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsOpen<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar 
   => control._set(FluentAvalonia.UI.Controls.InfoBar.IsOpenProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsOpen<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.InfoBar 
   => control._set(FluentAvalonia.UI.Controls.InfoBar.IsOpenProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsOpen<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar 
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.IsOpenProperty, ps, () => control.IsOpen = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // Title

/*BindFromExpressionSetterGenerator*/
public static T Title<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.InfoBar 
   => control._set(FluentAvalonia.UI.Controls.InfoBar.TitleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Title<T>(this T control,System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar 
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.TitleProperty, ps, () => control.Title = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Title<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar 
   => control._set(FluentAvalonia.UI.Controls.InfoBar.TitleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Title<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.InfoBar 
   => control._set(FluentAvalonia.UI.Controls.InfoBar.TitleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Title<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar 
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.TitleProperty, ps, () => control.Title = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // Message

/*BindFromExpressionSetterGenerator*/
public static T Message<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.InfoBar 
   => control._set(FluentAvalonia.UI.Controls.InfoBar.MessageProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Message<T>(this T control,System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar 
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.MessageProperty, ps, () => control.Message = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Message<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar 
   => control._set(FluentAvalonia.UI.Controls.InfoBar.MessageProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Message<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.InfoBar 
   => control._set(FluentAvalonia.UI.Controls.InfoBar.MessageProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Message<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar 
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.MessageProperty, ps, () => control.Message = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // Severity

/*BindFromExpressionSetterGenerator*/
public static T Severity<T>(this T control, Func<FluentAvalonia.UI.Controls.InfoBarSeverity> func, Action<FluentAvalonia.UI.Controls.InfoBarSeverity>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.InfoBar 
   => control._set(FluentAvalonia.UI.Controls.InfoBar.SeverityProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Severity<T>(this T control,FluentAvalonia.UI.Controls.InfoBarSeverity value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar 
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.SeverityProperty, ps, () => control.Severity = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Severity<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar 
   => control._set(FluentAvalonia.UI.Controls.InfoBar.SeverityProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Severity<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.InfoBar 
   => control._set(FluentAvalonia.UI.Controls.InfoBar.SeverityProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Severity<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.InfoBarSeverity> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar 
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.SeverityProperty, ps, () => control.Severity = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IconSource

/*BindFromExpressionSetterGenerator*/
public static T IconSource<T>(this T control, Func<FluentAvalonia.UI.Controls.IconSource> func, Action<FluentAvalonia.UI.Controls.IconSource>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.InfoBar 
   => control._set(FluentAvalonia.UI.Controls.InfoBar.IconSourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IconSource<T>(this T control,FluentAvalonia.UI.Controls.IconSource value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar 
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.IconSourceProperty, ps, () => control.IconSource = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IconSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar 
   => control._set(FluentAvalonia.UI.Controls.InfoBar.IconSourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IconSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.InfoBar 
   => control._set(FluentAvalonia.UI.Controls.InfoBar.IconSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IconSource<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.IconSource> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar 
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.IconSourceProperty, ps, () => control.IconSource = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsIconVisible

/*BindFromExpressionSetterGenerator*/
public static T IsIconVisible<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.InfoBar 
   => control._set(FluentAvalonia.UI.Controls.InfoBar.IsIconVisibleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsIconVisible<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar 
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.IsIconVisibleProperty, ps, () => control.IsIconVisible = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsIconVisible<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar 
   => control._set(FluentAvalonia.UI.Controls.InfoBar.IsIconVisibleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsIconVisible<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.InfoBar 
   => control._set(FluentAvalonia.UI.Controls.InfoBar.IsIconVisibleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsIconVisible<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar 
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.IsIconVisibleProperty, ps, () => control.IsIconVisible = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsClosable

/*BindFromExpressionSetterGenerator*/
public static T IsClosable<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.InfoBar 
   => control._set(FluentAvalonia.UI.Controls.InfoBar.IsClosableProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsClosable<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar 
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.IsClosableProperty, ps, () => control.IsClosable = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsClosable<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar 
   => control._set(FluentAvalonia.UI.Controls.InfoBar.IsClosableProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsClosable<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.InfoBar 
   => control._set(FluentAvalonia.UI.Controls.InfoBar.IsClosableProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsClosable<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar 
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.IsClosableProperty, ps, () => control.IsClosable = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CloseButtonCommand

/*BindFromExpressionSetterGenerator*/
public static T CloseButtonCommand<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.InfoBar 
   => control._set(FluentAvalonia.UI.Controls.InfoBar.CloseButtonCommandProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CloseButtonCommand<T>(this T control,System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar 
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.CloseButtonCommandProperty, ps, () => control.CloseButtonCommand = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CloseButtonCommand<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar 
   => control._set(FluentAvalonia.UI.Controls.InfoBar.CloseButtonCommandProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CloseButtonCommand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.InfoBar 
   => control._set(FluentAvalonia.UI.Controls.InfoBar.CloseButtonCommandProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CloseButtonCommand<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar 
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.CloseButtonCommandProperty, ps, () => control.CloseButtonCommand = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CloseButtonCommandParameter

/*BindFromExpressionSetterGenerator*/
public static T CloseButtonCommandParameter<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.InfoBar 
   => control._set(FluentAvalonia.UI.Controls.InfoBar.CloseButtonCommandParameterProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CloseButtonCommandParameter<T>(this T control,System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar 
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.CloseButtonCommandParameterProperty, ps, () => control.CloseButtonCommandParameter = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CloseButtonCommandParameter<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar 
   => control._set(FluentAvalonia.UI.Controls.InfoBar.CloseButtonCommandParameterProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CloseButtonCommandParameter<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.InfoBar 
   => control._set(FluentAvalonia.UI.Controls.InfoBar.CloseButtonCommandParameterProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CloseButtonCommandParameter<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar 
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.CloseButtonCommandParameterProperty, ps, () => control.CloseButtonCommandParameter = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ActionButton

/*BindFromExpressionSetterGenerator*/
public static T ActionButton<T>(this T control, Func<Avalonia.Controls.Control> func, Action<Avalonia.Controls.Control>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.InfoBar 
   => control._set(FluentAvalonia.UI.Controls.InfoBar.ActionButtonProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ActionButton<T>(this T control,Avalonia.Controls.Control value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar 
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.ActionButtonProperty, ps, () => control.ActionButton = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ActionButton<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar 
   => control._set(FluentAvalonia.UI.Controls.InfoBar.ActionButtonProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ActionButton<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.InfoBar 
   => control._set(FluentAvalonia.UI.Controls.InfoBar.ActionButtonProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ActionButton<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Control> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.InfoBar 
=> control._setEx(FluentAvalonia.UI.Controls.InfoBar.ActionButtonProperty, ps, () => control.ActionButton = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // CloseButtonClick

/*ActionToEventGenerator*/
public static T OnCloseButtonClick<T>(this T control, Action<FluentAvalonia.UI.Controls.InfoBar, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.InfoBar  => 
 control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.InfoBar,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.CloseButtonClick += h);


 // Closing

/*ActionToEventGenerator*/
public static T OnClosing<T>(this T control, Action<FluentAvalonia.UI.Controls.InfoBar, FluentAvalonia.UI.Controls.InfoBarClosingEventArgs> action) where T : FluentAvalonia.UI.Controls.InfoBar  => 
 control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.InfoBar,FluentAvalonia.UI.Controls.InfoBarClosingEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Closing += h);


 // Closed

/*ActionToEventGenerator*/
public static T OnClosed<T>(this T control, Action<FluentAvalonia.UI.Controls.InfoBar, FluentAvalonia.UI.Controls.InfoBarClosedEventArgs> action) where T : FluentAvalonia.UI.Controls.InfoBar  => 
 control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.InfoBar,FluentAvalonia.UI.Controls.InfoBarClosedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Closed += h);



//================= Styles ======================//
 // IsOpen

/*ValueStyleSetterGenerator*/
public static Style<T> IsOpen<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.InfoBar 
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.IsOpenProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsOpen<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar 
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.IsOpenProperty, binding);


 // Title

/*ValueStyleSetterGenerator*/
public static Style<T> Title<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.InfoBar 
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.TitleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Title<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar 
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.TitleProperty, binding);


 // Message

/*ValueStyleSetterGenerator*/
public static Style<T> Message<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.InfoBar 
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.MessageProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Message<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar 
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.MessageProperty, binding);


 // Severity

/*ValueStyleSetterGenerator*/
public static Style<T> Severity<T>(this Style<T> style, FluentAvalonia.UI.Controls.InfoBarSeverity value) where T : FluentAvalonia.UI.Controls.InfoBar 
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.SeverityProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Severity<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar 
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.SeverityProperty, binding);


 // IconSource

/*ValueStyleSetterGenerator*/
public static Style<T> IconSource<T>(this Style<T> style, FluentAvalonia.UI.Controls.IconSource value) where T : FluentAvalonia.UI.Controls.InfoBar 
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.IconSourceProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IconSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar 
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.IconSourceProperty, binding);


 // IsIconVisible

/*ValueStyleSetterGenerator*/
public static Style<T> IsIconVisible<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.InfoBar 
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.IsIconVisibleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsIconVisible<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar 
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.IsIconVisibleProperty, binding);


 // IsClosable

/*ValueStyleSetterGenerator*/
public static Style<T> IsClosable<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.InfoBar 
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.IsClosableProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsClosable<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar 
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.IsClosableProperty, binding);


 // CloseButtonCommand

/*ValueStyleSetterGenerator*/
public static Style<T> CloseButtonCommand<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : FluentAvalonia.UI.Controls.InfoBar 
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.CloseButtonCommandProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CloseButtonCommand<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar 
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.CloseButtonCommandProperty, binding);


 // CloseButtonCommandParameter

/*ValueStyleSetterGenerator*/
public static Style<T> CloseButtonCommandParameter<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.InfoBar 
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.CloseButtonCommandParameterProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CloseButtonCommandParameter<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar 
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.CloseButtonCommandParameterProperty, binding);


 // ActionButton

/*ValueStyleSetterGenerator*/
public static Style<T> ActionButton<T>(this Style<T> style, Avalonia.Controls.Control value) where T : FluentAvalonia.UI.Controls.InfoBar 
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.ActionButtonProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ActionButton<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar 
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.ActionButtonProperty, binding);



}
