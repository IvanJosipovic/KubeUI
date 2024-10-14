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
public static partial class XamlUICommand_MarkupExtensions
{
//================= Properties ======================//
 // CommandProperty

/*BindFromExpressionSetterGenerator*/
public static T Command<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Input.XamlUICommand
   => control._set(FluentAvalonia.UI.Input.XamlUICommand.CommandProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Command<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Input.XamlUICommand
=> control._setEx(FluentAvalonia.UI.Input.XamlUICommand.CommandProperty, ps, () => control.Command = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Command<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Input.XamlUICommand
   => control._set(FluentAvalonia.UI.Input.XamlUICommand.CommandProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Command<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Input.XamlUICommand
   => control._set(FluentAvalonia.UI.Input.XamlUICommand.CommandProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Command<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Input.XamlUICommand
=> control._setEx(FluentAvalonia.UI.Input.XamlUICommand.CommandProperty, ps, () => control.Command = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // DescriptionProperty

/*BindFromExpressionSetterGenerator*/
public static T Description<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Input.XamlUICommand
   => control._set(FluentAvalonia.UI.Input.XamlUICommand.DescriptionProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Description<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Input.XamlUICommand
=> control._setEx(FluentAvalonia.UI.Input.XamlUICommand.DescriptionProperty, ps, () => control.Description = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Description<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Input.XamlUICommand
   => control._set(FluentAvalonia.UI.Input.XamlUICommand.DescriptionProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Description<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Input.XamlUICommand
   => control._set(FluentAvalonia.UI.Input.XamlUICommand.DescriptionProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Description<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Input.XamlUICommand
=> control._setEx(FluentAvalonia.UI.Input.XamlUICommand.DescriptionProperty, ps, () => control.Description = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IconSourceProperty

/*BindFromExpressionSetterGenerator*/
public static T IconSource<T>(this T control, Func<FluentAvalonia.UI.Controls.IconSource> func, Action<FluentAvalonia.UI.Controls.IconSource>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Input.XamlUICommand
   => control._set(FluentAvalonia.UI.Input.XamlUICommand.IconSourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IconSource<T>(this T control, FluentAvalonia.UI.Controls.IconSource value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Input.XamlUICommand
=> control._setEx(FluentAvalonia.UI.Input.XamlUICommand.IconSourceProperty, ps, () => control.IconSource = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IconSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Input.XamlUICommand
   => control._set(FluentAvalonia.UI.Input.XamlUICommand.IconSourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IconSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Input.XamlUICommand
   => control._set(FluentAvalonia.UI.Input.XamlUICommand.IconSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IconSource<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.IconSource> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Input.XamlUICommand
=> control._setEx(FluentAvalonia.UI.Input.XamlUICommand.IconSourceProperty, ps, () => control.IconSource = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // HotKeyProperty

/*BindFromExpressionSetterGenerator*/
public static T HotKey<T>(this T control, Func<Avalonia.Input.KeyGesture> func, Action<Avalonia.Input.KeyGesture>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Input.XamlUICommand
   => control._set(FluentAvalonia.UI.Input.XamlUICommand.HotKeyProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T HotKey<T>(this T control, Avalonia.Input.KeyGesture value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Input.XamlUICommand
=> control._setEx(FluentAvalonia.UI.Input.XamlUICommand.HotKeyProperty, ps, () => control.HotKey = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T HotKey<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Input.XamlUICommand
   => control._set(FluentAvalonia.UI.Input.XamlUICommand.HotKeyProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T HotKey<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Input.XamlUICommand
   => control._set(FluentAvalonia.UI.Input.XamlUICommand.HotKeyProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T HotKey<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Input.KeyGesture> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Input.XamlUICommand
=> control._setEx(FluentAvalonia.UI.Input.XamlUICommand.HotKeyProperty, ps, () => control.HotKey = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // LabelProperty

/*BindFromExpressionSetterGenerator*/
public static T Label<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Input.XamlUICommand
   => control._set(FluentAvalonia.UI.Input.XamlUICommand.LabelProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Label<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Input.XamlUICommand
=> control._setEx(FluentAvalonia.UI.Input.XamlUICommand.LabelProperty, ps, () => control.Label = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Label<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Input.XamlUICommand
   => control._set(FluentAvalonia.UI.Input.XamlUICommand.LabelProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Label<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Input.XamlUICommand
   => control._set(FluentAvalonia.UI.Input.XamlUICommand.LabelProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Label<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Input.XamlUICommand
=> control._setEx(FluentAvalonia.UI.Input.XamlUICommand.LabelProperty, ps, () => control.Label = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // CanExecuteChanged

/*ActionToEventGenerator*/
    public static T OnCanExecuteChanged<T>(this T control, Action<System.EventArgs> action) where T : FluentAvalonia.UI.Input.XamlUICommand => 
        control._setEvent((System.EventHandler) ((arg0, arg1) => action(arg1)), h => control.CanExecuteChanged += h);


 // CanExecuteRequested

/*ActionToEventGenerator*/
    public static T OnCanExecuteRequested<T>(this T control, Action<FluentAvalonia.UI.Input.XamlUICommand, FluentAvalonia.UI.Input.CanExecuteRequestedEventArgs> action) where T : FluentAvalonia.UI.Input.XamlUICommand => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Input.XamlUICommand,FluentAvalonia.UI.Input.CanExecuteRequestedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.CanExecuteRequested += h);


 // ExecuteRequested

/*ActionToEventGenerator*/
    public static T OnExecuteRequested<T>(this T control, Action<FluentAvalonia.UI.Input.XamlUICommand, FluentAvalonia.UI.Input.ExecuteRequestedEventArgs> action) where T : FluentAvalonia.UI.Input.XamlUICommand => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Input.XamlUICommand,FluentAvalonia.UI.Input.ExecuteRequestedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.ExecuteRequested += h);



//================= Styles ======================//

}
