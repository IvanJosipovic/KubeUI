#nullable enable
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using ContentDialog = FluentAvalonia.UI.Controls.ContentDialog;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Avalonia.Markup.Declarative;
public static partial class ContentDialogExtensions
{
public static T CloseButtonCommand<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonCommandProperty, binding);
public static T CloseButtonCommand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonCommandProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CloseButtonCommand<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonCommandProperty, func, onChanged, expression);
public static T CloseButtonCommand<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonCommandProperty, ps, () => control.CloseButtonCommand = value, bindingMode, converter, bindingSource);
public static T CloseButtonCommand<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonCommandProperty, ps, () => control.CloseButtonCommand = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T CloseButtonCommandParameter<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonCommandParameterProperty, binding);
public static T CloseButtonCommandParameter<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonCommandParameterProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CloseButtonCommandParameter<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonCommandParameterProperty, func, onChanged, expression);
public static T CloseButtonCommandParameter<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonCommandParameterProperty, ps, () => control.CloseButtonCommandParameter = value, bindingMode, converter, bindingSource);
public static T CloseButtonCommandParameter<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonCommandParameterProperty, ps, () => control.CloseButtonCommandParameter = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T CloseButtonText<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonTextProperty, binding);
public static T CloseButtonText<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonTextProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CloseButtonText<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonTextProperty, func, onChanged, expression);
public static T CloseButtonText<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonTextProperty, ps, () => control.CloseButtonText = value, bindingMode, converter, bindingSource);
public static T CloseButtonText<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonTextProperty, ps, () => control.CloseButtonText = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T DefaultButton<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.DefaultButtonProperty, binding);
public static T DefaultButton<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.DefaultButtonProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T DefaultButton<T>(this T control, Func<FluentAvalonia.UI.Controls.ContentDialogButton> func, Action<FluentAvalonia.UI.Controls.ContentDialogButton>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.DefaultButtonProperty, func, onChanged, expression);
public static T DefaultButton<T>(this T control, FluentAvalonia.UI.Controls.ContentDialogButton value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.DefaultButtonProperty, ps, () => control.DefaultButton = value, bindingMode, converter, bindingSource);
public static T DefaultButton<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.ContentDialogButton> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.DefaultButtonProperty, ps, () => control.DefaultButton = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsPrimaryButtonEnabled<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.IsPrimaryButtonEnabledProperty, binding);
public static T IsPrimaryButtonEnabled<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.IsPrimaryButtonEnabledProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsPrimaryButtonEnabled<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.IsPrimaryButtonEnabledProperty, func, onChanged, expression);
public static T IsPrimaryButtonEnabled<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.IsPrimaryButtonEnabledProperty, ps, () => control.IsPrimaryButtonEnabled = value, bindingMode, converter, bindingSource);
public static T IsPrimaryButtonEnabled<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.IsPrimaryButtonEnabledProperty, ps, () => control.IsPrimaryButtonEnabled = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsSecondaryButtonEnabled<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.IsSecondaryButtonEnabledProperty, binding);
public static T IsSecondaryButtonEnabled<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.IsSecondaryButtonEnabledProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsSecondaryButtonEnabled<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.IsSecondaryButtonEnabledProperty, func, onChanged, expression);
public static T IsSecondaryButtonEnabled<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.IsSecondaryButtonEnabledProperty, ps, () => control.IsSecondaryButtonEnabled = value, bindingMode, converter, bindingSource);
public static T IsSecondaryButtonEnabled<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.IsSecondaryButtonEnabledProperty, ps, () => control.IsSecondaryButtonEnabled = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T PrimaryButtonCommand<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonCommandProperty, binding);
public static T PrimaryButtonCommand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonCommandProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T PrimaryButtonCommand<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonCommandProperty, func, onChanged, expression);
public static T PrimaryButtonCommand<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonCommandProperty, ps, () => control.PrimaryButtonCommand = value, bindingMode, converter, bindingSource);
public static T PrimaryButtonCommand<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonCommandProperty, ps, () => control.PrimaryButtonCommand = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T PrimaryButtonCommandParameter<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonCommandParameterProperty, binding);
public static T PrimaryButtonCommandParameter<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonCommandParameterProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T PrimaryButtonCommandParameter<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonCommandParameterProperty, func, onChanged, expression);
public static T PrimaryButtonCommandParameter<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonCommandParameterProperty, ps, () => control.PrimaryButtonCommandParameter = value, bindingMode, converter, bindingSource);
public static T PrimaryButtonCommandParameter<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonCommandParameterProperty, ps, () => control.PrimaryButtonCommandParameter = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T PrimaryButtonText<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonTextProperty, binding);
public static T PrimaryButtonText<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonTextProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T PrimaryButtonText<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonTextProperty, func, onChanged, expression);
public static T PrimaryButtonText<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonTextProperty, ps, () => control.PrimaryButtonText = value, bindingMode, converter, bindingSource);
public static T PrimaryButtonText<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonTextProperty, ps, () => control.PrimaryButtonText = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SecondaryButtonCommand<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonCommandProperty, binding);
public static T SecondaryButtonCommand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonCommandProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SecondaryButtonCommand<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonCommandProperty, func, onChanged, expression);
public static T SecondaryButtonCommand<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonCommandProperty, ps, () => control.SecondaryButtonCommand = value, bindingMode, converter, bindingSource);
public static T SecondaryButtonCommand<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonCommandProperty, ps, () => control.SecondaryButtonCommand = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SecondaryButtonCommandParameter<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonCommandParameterProperty, binding);
public static T SecondaryButtonCommandParameter<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonCommandParameterProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SecondaryButtonCommandParameter<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonCommandParameterProperty, func, onChanged, expression);
public static T SecondaryButtonCommandParameter<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonCommandParameterProperty, ps, () => control.SecondaryButtonCommandParameter = value, bindingMode, converter, bindingSource);
public static T SecondaryButtonCommandParameter<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonCommandParameterProperty, ps, () => control.SecondaryButtonCommandParameter = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SecondaryButtonText<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonTextProperty, binding);
public static T SecondaryButtonText<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonTextProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SecondaryButtonText<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonTextProperty, func, onChanged, expression);
public static T SecondaryButtonText<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonTextProperty, ps, () => control.SecondaryButtonText = value, bindingMode, converter, bindingSource);
public static T SecondaryButtonText<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonTextProperty, ps, () => control.SecondaryButtonText = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Title<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.TitleProperty, binding);
public static T Title<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.TitleProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Title<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.TitleProperty, func, onChanged, expression);
public static T Title<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.TitleProperty, ps, () => control.Title = value, bindingMode, converter, bindingSource);
public static T Title<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.TitleProperty, ps, () => control.Title = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T TitleTemplate<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.TitleTemplateProperty, binding);
public static T TitleTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.TitleTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T TitleTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.TitleTemplateProperty, func, onChanged, expression);
public static T TitleTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.TitleTemplateProperty, ps, () => control.TitleTemplate = value, bindingMode, converter, bindingSource);
public static T TitleTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.TitleTemplateProperty, ps, () => control.TitleTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T FullSizeDesired<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.FullSizeDesiredProperty, binding);
public static T FullSizeDesired<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.FullSizeDesiredProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T FullSizeDesired<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ContentDialog
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.FullSizeDesiredProperty, func, onChanged, expression);
public static T FullSizeDesired<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.FullSizeDesiredProperty, ps, () => control.FullSizeDesired = value, bindingMode, converter, bindingSource);
public static T FullSizeDesired<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.FullSizeDesiredProperty, ps, () => control.FullSizeDesired = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

