#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Input;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using XamlUICommand = FluentAvalonia.UI.Input.XamlUICommand;

namespace Avalonia.Markup.Declarative;
public static partial class XamlUICommandExtensions
{
public static T Command<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Input.XamlUICommand
   => control._set(FluentAvalonia.UI.Input.XamlUICommand.CommandProperty, binding);
public static T Command<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Input.XamlUICommand
   => control._set(FluentAvalonia.UI.Input.XamlUICommand.CommandProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Command<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Input.XamlUICommand
   => control._set(FluentAvalonia.UI.Input.XamlUICommand.CommandProperty, func, onChanged, expression);
public static T Command<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Input.XamlUICommand
=> control._setEx(FluentAvalonia.UI.Input.XamlUICommand.CommandProperty, ps, () => control.Command = value, bindingMode, converter, bindingSource);
public static T Command<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Input.XamlUICommand
=> control._setEx(FluentAvalonia.UI.Input.XamlUICommand.CommandProperty, ps, () => control.Command = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Description<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Input.XamlUICommand
   => control._set(FluentAvalonia.UI.Input.XamlUICommand.DescriptionProperty, binding);
public static T Description<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Input.XamlUICommand
   => control._set(FluentAvalonia.UI.Input.XamlUICommand.DescriptionProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Description<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Input.XamlUICommand
   => control._set(FluentAvalonia.UI.Input.XamlUICommand.DescriptionProperty, func, onChanged, expression);
public static T Description<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Input.XamlUICommand
=> control._setEx(FluentAvalonia.UI.Input.XamlUICommand.DescriptionProperty, ps, () => control.Description = value, bindingMode, converter, bindingSource);
public static T Description<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Input.XamlUICommand
=> control._setEx(FluentAvalonia.UI.Input.XamlUICommand.DescriptionProperty, ps, () => control.Description = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IconSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Input.XamlUICommand
   => control._set(FluentAvalonia.UI.Input.XamlUICommand.IconSourceProperty, binding);
public static T IconSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Input.XamlUICommand
   => control._set(FluentAvalonia.UI.Input.XamlUICommand.IconSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IconSource<T>(this T control, Func<FluentAvalonia.UI.Controls.IconSource> func, Action<FluentAvalonia.UI.Controls.IconSource>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Input.XamlUICommand
   => control._set(FluentAvalonia.UI.Input.XamlUICommand.IconSourceProperty, func, onChanged, expression);
public static T IconSource<T>(this T control, FluentAvalonia.UI.Controls.IconSource value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Input.XamlUICommand
=> control._setEx(FluentAvalonia.UI.Input.XamlUICommand.IconSourceProperty, ps, () => control.IconSource = value, bindingMode, converter, bindingSource);
public static T IconSource<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.IconSource> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Input.XamlUICommand
=> control._setEx(FluentAvalonia.UI.Input.XamlUICommand.IconSourceProperty, ps, () => control.IconSource = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T HotKey<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Input.XamlUICommand
   => control._set(FluentAvalonia.UI.Input.XamlUICommand.HotKeyProperty, binding);
public static T HotKey<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Input.XamlUICommand
   => control._set(FluentAvalonia.UI.Input.XamlUICommand.HotKeyProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T HotKey<T>(this T control, Func<Avalonia.Input.KeyGesture> func, Action<Avalonia.Input.KeyGesture>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Input.XamlUICommand
   => control._set(FluentAvalonia.UI.Input.XamlUICommand.HotKeyProperty, func, onChanged, expression);
public static T HotKey<T>(this T control, Avalonia.Input.KeyGesture value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Input.XamlUICommand
=> control._setEx(FluentAvalonia.UI.Input.XamlUICommand.HotKeyProperty, ps, () => control.HotKey = value, bindingMode, converter, bindingSource);
public static T HotKey<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Input.KeyGesture> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Input.XamlUICommand
=> control._setEx(FluentAvalonia.UI.Input.XamlUICommand.HotKeyProperty, ps, () => control.HotKey = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Label<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Input.XamlUICommand
   => control._set(FluentAvalonia.UI.Input.XamlUICommand.LabelProperty, binding);
public static T Label<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Input.XamlUICommand
   => control._set(FluentAvalonia.UI.Input.XamlUICommand.LabelProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Label<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Input.XamlUICommand
   => control._set(FluentAvalonia.UI.Input.XamlUICommand.LabelProperty, func, onChanged, expression);
public static T Label<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Input.XamlUICommand
=> control._setEx(FluentAvalonia.UI.Input.XamlUICommand.LabelProperty, ps, () => control.Label = value, bindingMode, converter, bindingSource);
public static T Label<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Input.XamlUICommand
=> control._setEx(FluentAvalonia.UI.Input.XamlUICommand.LabelProperty, ps, () => control.Label = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

