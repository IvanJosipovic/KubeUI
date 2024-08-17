#nullable enable
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Styling;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TeachingTip = FluentAvalonia.UI.Controls.TeachingTip;

namespace Avalonia.Markup.Declarative;
public static partial class TeachingTipExtensions
{
public static T Title<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.TitleProperty, binding);
public static T Title<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.TitleProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Title<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.TitleProperty, func, onChanged, expression);
public static T Title<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.TitleProperty, ps, () => control.Title = value, bindingMode, converter, bindingSource);
public static T Title<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.TitleProperty, ps, () => control.Title = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Subtitle<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.SubtitleProperty, binding);
public static T Subtitle<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.SubtitleProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Subtitle<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.SubtitleProperty, func, onChanged, expression);
public static T Subtitle<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.SubtitleProperty, ps, () => control.Subtitle = value, bindingMode, converter, bindingSource);
public static T Subtitle<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.SubtitleProperty, ps, () => control.Subtitle = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsOpen<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.IsOpenProperty, binding);
public static T IsOpen<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.IsOpenProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsOpen<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.IsOpenProperty, func, onChanged, expression);
public static T IsOpen<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.IsOpenProperty, ps, () => control.IsOpen = value, bindingMode, converter, bindingSource);
public static T IsOpen<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.IsOpenProperty, ps, () => control.IsOpen = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Target<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.TargetProperty, binding);
public static T Target<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.TargetProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Target<T>(this T control, Func<Avalonia.Controls.Control> func, Action<Avalonia.Controls.Control>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.TargetProperty, func, onChanged, expression);
public static T Target<T>(this T control, Avalonia.Controls.Control value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.TargetProperty, ps, () => control.Target = value, bindingMode, converter, bindingSource);
public static T Target<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Control> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.TargetProperty, ps, () => control.Target = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T TailVisibility<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.TailVisibilityProperty, binding);
public static T TailVisibility<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.TailVisibilityProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T TailVisibility<T>(this T control, Func<FluentAvalonia.UI.Controls.TeachingTipTailVisibility> func, Action<FluentAvalonia.UI.Controls.TeachingTipTailVisibility>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.TailVisibilityProperty, func, onChanged, expression);
public static T TailVisibility<T>(this T control, FluentAvalonia.UI.Controls.TeachingTipTailVisibility value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.TailVisibilityProperty, ps, () => control.TailVisibility = value, bindingMode, converter, bindingSource);
public static T TailVisibility<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.TeachingTipTailVisibility> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.TailVisibilityProperty, ps, () => control.TailVisibility = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ActionButtonContent<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonContentProperty, binding);
public static T ActionButtonContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonContentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ActionButtonContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonContentProperty, func, onChanged, expression);
public static T ActionButtonContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonContentProperty, ps, () => control.ActionButtonContent = value, bindingMode, converter, bindingSource);
public static T ActionButtonContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonContentProperty, ps, () => control.ActionButtonContent = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ActionButtonStyle<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonStyleProperty, binding);
public static T ActionButtonStyle<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonStyleProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ActionButtonStyle<T>(this T control, Func<Avalonia.Styling.ControlTheme> func, Action<Avalonia.Styling.ControlTheme>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonStyleProperty, func, onChanged, expression);
public static T ActionButtonStyle<T>(this T control, Avalonia.Styling.ControlTheme value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonStyleProperty, ps, () => control.ActionButtonStyle = value, bindingMode, converter, bindingSource);
public static T ActionButtonStyle<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ControlTheme> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonStyleProperty, ps, () => control.ActionButtonStyle = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ActionButtonCommand<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonCommandProperty, binding);
public static T ActionButtonCommand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonCommandProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ActionButtonCommand<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonCommandProperty, func, onChanged, expression);
public static T ActionButtonCommand<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonCommandProperty, ps, () => control.ActionButtonCommand = value, bindingMode, converter, bindingSource);
public static T ActionButtonCommand<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonCommandProperty, ps, () => control.ActionButtonCommand = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ActionButtonCommandParameter<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonCommandParameterProperty, binding);
public static T ActionButtonCommandParameter<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonCommandParameterProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ActionButtonCommandParameter<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonCommandParameterProperty, func, onChanged, expression);
public static T ActionButtonCommandParameter<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonCommandParameterProperty, ps, () => control.ActionButtonCommandParameter = value, bindingMode, converter, bindingSource);
public static T ActionButtonCommandParameter<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonCommandParameterProperty, ps, () => control.ActionButtonCommandParameter = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T CloseButtonContent<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonContentProperty, binding);
public static T CloseButtonContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonContentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CloseButtonContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonContentProperty, func, onChanged, expression);
public static T CloseButtonContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonContentProperty, ps, () => control.CloseButtonContent = value, bindingMode, converter, bindingSource);
public static T CloseButtonContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonContentProperty, ps, () => control.CloseButtonContent = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T CloseButtonStyle<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonStyleProperty, binding);
public static T CloseButtonStyle<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonStyleProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CloseButtonStyle<T>(this T control, Func<Avalonia.Styling.ControlTheme> func, Action<Avalonia.Styling.ControlTheme>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonStyleProperty, func, onChanged, expression);
public static T CloseButtonStyle<T>(this T control, Avalonia.Styling.ControlTheme value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonStyleProperty, ps, () => control.CloseButtonStyle = value, bindingMode, converter, bindingSource);
public static T CloseButtonStyle<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ControlTheme> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonStyleProperty, ps, () => control.CloseButtonStyle = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T CloseButtonCommand<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonCommandProperty, binding);
public static T CloseButtonCommand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonCommandProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CloseButtonCommand<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonCommandProperty, func, onChanged, expression);
public static T CloseButtonCommand<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonCommandProperty, ps, () => control.CloseButtonCommand = value, bindingMode, converter, bindingSource);
public static T CloseButtonCommand<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonCommandProperty, ps, () => control.CloseButtonCommand = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T CloseButtonCommandParameter<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonCommandParameterProperty, binding);
public static T CloseButtonCommandParameter<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonCommandParameterProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CloseButtonCommandParameter<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonCommandParameterProperty, func, onChanged, expression);
public static T CloseButtonCommandParameter<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonCommandParameterProperty, ps, () => control.CloseButtonCommandParameter = value, bindingMode, converter, bindingSource);
public static T CloseButtonCommandParameter<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonCommandParameterProperty, ps, () => control.CloseButtonCommandParameter = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T PlacementMargin<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.PlacementMarginProperty, binding);
public static T PlacementMargin<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.PlacementMarginProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T PlacementMargin<T>(this T control, Func<Avalonia.Thickness> func, Action<Avalonia.Thickness>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.PlacementMarginProperty, func, onChanged, expression);
public static T PlacementMargin<T>(this T control, Avalonia.Thickness value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.PlacementMarginProperty, ps, () => control.PlacementMargin = value, bindingMode, converter, bindingSource);
public static T PlacementMargin<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Thickness> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.PlacementMarginProperty, ps, () => control.PlacementMargin = converter.TryConvert(value), bindingMode, converter, bindingSource);

public static T PlacementMargin<T>(this T control, Double uniformLength = default) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(() => control.PlacementMargin = new Avalonia.Thickness(uniformLength));
public static T PlacementMargin<T>(this T control, Double horizontal = default, Double vertical = default) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(() => control.PlacementMargin = new Avalonia.Thickness(horizontal, vertical));
public static T PlacementMargin<T>(this T control, Double left = default, Double top = default, Double right = default, Double bottom = default) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(() => control.PlacementMargin = new Avalonia.Thickness(left, top, right, bottom));
public static T ShouldConstrainToRootBounds<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.ShouldConstrainToRootBoundsProperty, binding);
public static T ShouldConstrainToRootBounds<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.ShouldConstrainToRootBoundsProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ShouldConstrainToRootBounds<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.ShouldConstrainToRootBoundsProperty, func, onChanged, expression);
public static T ShouldConstrainToRootBounds<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.ShouldConstrainToRootBoundsProperty, ps, () => control.ShouldConstrainToRootBounds = value, bindingMode, converter, bindingSource);
public static T ShouldConstrainToRootBounds<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.ShouldConstrainToRootBoundsProperty, ps, () => control.ShouldConstrainToRootBounds = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsLightDismissEnabled<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.IsLightDismissEnabledProperty, binding);
public static T IsLightDismissEnabled<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.IsLightDismissEnabledProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsLightDismissEnabled<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.IsLightDismissEnabledProperty, func, onChanged, expression);
public static T IsLightDismissEnabled<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.IsLightDismissEnabledProperty, ps, () => control.IsLightDismissEnabled = value, bindingMode, converter, bindingSource);
public static T IsLightDismissEnabled<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.IsLightDismissEnabledProperty, ps, () => control.IsLightDismissEnabled = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T PreferredPlacement<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.PreferredPlacementProperty, binding);
public static T PreferredPlacement<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.PreferredPlacementProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T PreferredPlacement<T>(this T control, Func<FluentAvalonia.UI.Controls.TeachingTipPlacementMode> func, Action<FluentAvalonia.UI.Controls.TeachingTipPlacementMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.PreferredPlacementProperty, func, onChanged, expression);
public static T PreferredPlacement<T>(this T control, FluentAvalonia.UI.Controls.TeachingTipPlacementMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.PreferredPlacementProperty, ps, () => control.PreferredPlacement = value, bindingMode, converter, bindingSource);
public static T PreferredPlacement<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.TeachingTipPlacementMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.PreferredPlacementProperty, ps, () => control.PreferredPlacement = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T HeroContentPlacement<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.HeroContentPlacementProperty, binding);
public static T HeroContentPlacement<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.HeroContentPlacementProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T HeroContentPlacement<T>(this T control, Func<FluentAvalonia.UI.Controls.TeachingTipHeroContentPlacementMode> func, Action<FluentAvalonia.UI.Controls.TeachingTipHeroContentPlacementMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.HeroContentPlacementProperty, func, onChanged, expression);
public static T HeroContentPlacement<T>(this T control, FluentAvalonia.UI.Controls.TeachingTipHeroContentPlacementMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.HeroContentPlacementProperty, ps, () => control.HeroContentPlacement = value, bindingMode, converter, bindingSource);
public static T HeroContentPlacement<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.TeachingTipHeroContentPlacementMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.HeroContentPlacementProperty, ps, () => control.HeroContentPlacement = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T HeroContent<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.HeroContentProperty, binding);
public static T HeroContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.HeroContentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T HeroContent<T>(this T control, Func<Avalonia.Controls.Control> func, Action<Avalonia.Controls.Control>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.HeroContentProperty, func, onChanged, expression);
public static T HeroContent<T>(this T control, Avalonia.Controls.Control value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.HeroContentProperty, ps, () => control.HeroContent = value, bindingMode, converter, bindingSource);
public static T HeroContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Control> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.HeroContentProperty, ps, () => control.HeroContent = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IconSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.IconSourceProperty, binding);
public static T IconSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.IconSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IconSource<T>(this T control, Func<FluentAvalonia.UI.Controls.IconSource> func, Action<FluentAvalonia.UI.Controls.IconSource>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.IconSourceProperty, func, onChanged, expression);
public static T IconSource<T>(this T control, FluentAvalonia.UI.Controls.IconSource value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.IconSourceProperty, ps, () => control.IconSource = value, bindingMode, converter, bindingSource);
public static T IconSource<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.IconSource> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.IconSourceProperty, ps, () => control.IconSource = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

