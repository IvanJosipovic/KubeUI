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
public static partial class TeachingTip_MarkupExtensions
{
//================= Properties ======================//
 // TitleProperty

/*BindFromExpressionSetterGenerator*/
public static T Title<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.TitleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Title<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.TitleProperty, ps, () => control.Title = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Title<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.TitleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Title<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.TitleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Title<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.TitleProperty, ps, () => control.Title = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SubtitleProperty

/*BindFromExpressionSetterGenerator*/
public static T Subtitle<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.SubtitleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Subtitle<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.SubtitleProperty, ps, () => control.Subtitle = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Subtitle<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.SubtitleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Subtitle<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.SubtitleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Subtitle<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.SubtitleProperty, ps, () => control.Subtitle = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsOpenProperty

/*BindFromExpressionSetterGenerator*/
public static T IsOpen<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.IsOpenProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsOpen<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.IsOpenProperty, ps, () => control.IsOpen = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsOpen<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.IsOpenProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsOpen<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.IsOpenProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsOpen<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.IsOpenProperty, ps, () => control.IsOpen = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TargetProperty

/*BindFromExpressionSetterGenerator*/
public static T Target<T>(this T control, Func<Avalonia.Controls.Control> func, Action<Avalonia.Controls.Control>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.TargetProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Target<T>(this T control, Avalonia.Controls.Control value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.TargetProperty, ps, () => control.Target = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Target<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.TargetProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Target<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.TargetProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Target<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Control> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.TargetProperty, ps, () => control.Target = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TailVisibilityProperty

/*BindFromExpressionSetterGenerator*/
public static T TailVisibility<T>(this T control, Func<FluentAvalonia.UI.Controls.TeachingTipTailVisibility> func, Action<FluentAvalonia.UI.Controls.TeachingTipTailVisibility>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.TailVisibilityProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T TailVisibility<T>(this T control, FluentAvalonia.UI.Controls.TeachingTipTailVisibility value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.TailVisibilityProperty, ps, () => control.TailVisibility = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TailVisibility<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.TailVisibilityProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TailVisibility<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.TailVisibilityProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T TailVisibility<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.TeachingTipTailVisibility> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.TailVisibilityProperty, ps, () => control.TailVisibility = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ActionButtonContentProperty

/*BindFromExpressionSetterGenerator*/
public static T ActionButtonContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonContentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ActionButtonContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonContentProperty, ps, () => control.ActionButtonContent = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ActionButtonContent<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonContentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ActionButtonContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonContentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ActionButtonContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonContentProperty, ps, () => control.ActionButtonContent = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ActionButtonStyleProperty

/*BindFromExpressionSetterGenerator*/
public static T ActionButtonStyle<T>(this T control, Func<Avalonia.Styling.ControlTheme> func, Action<Avalonia.Styling.ControlTheme>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonStyleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ActionButtonStyle<T>(this T control, Avalonia.Styling.ControlTheme value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonStyleProperty, ps, () => control.ActionButtonStyle = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ActionButtonStyle<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonStyleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ActionButtonStyle<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonStyleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ActionButtonStyle<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ControlTheme> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonStyleProperty, ps, () => control.ActionButtonStyle = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ActionButtonCommandProperty

/*BindFromExpressionSetterGenerator*/
public static T ActionButtonCommand<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonCommandProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ActionButtonCommand<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonCommandProperty, ps, () => control.ActionButtonCommand = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ActionButtonCommand<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonCommandProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ActionButtonCommand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonCommandProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ActionButtonCommand<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonCommandProperty, ps, () => control.ActionButtonCommand = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ActionButtonCommandParameterProperty

/*BindFromExpressionSetterGenerator*/
public static T ActionButtonCommandParameter<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonCommandParameterProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ActionButtonCommandParameter<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonCommandParameterProperty, ps, () => control.ActionButtonCommandParameter = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ActionButtonCommandParameter<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonCommandParameterProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ActionButtonCommandParameter<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonCommandParameterProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ActionButtonCommandParameter<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonCommandParameterProperty, ps, () => control.ActionButtonCommandParameter = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CloseButtonContentProperty

/*BindFromExpressionSetterGenerator*/
public static T CloseButtonContent<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonContentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CloseButtonContent<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonContentProperty, ps, () => control.CloseButtonContent = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CloseButtonContent<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonContentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CloseButtonContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonContentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CloseButtonContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonContentProperty, ps, () => control.CloseButtonContent = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CloseButtonStyleProperty

/*BindFromExpressionSetterGenerator*/
public static T CloseButtonStyle<T>(this T control, Func<Avalonia.Styling.ControlTheme> func, Action<Avalonia.Styling.ControlTheme>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonStyleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CloseButtonStyle<T>(this T control, Avalonia.Styling.ControlTheme value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonStyleProperty, ps, () => control.CloseButtonStyle = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CloseButtonStyle<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonStyleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CloseButtonStyle<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonStyleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CloseButtonStyle<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ControlTheme> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonStyleProperty, ps, () => control.CloseButtonStyle = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CloseButtonCommandProperty

/*BindFromExpressionSetterGenerator*/
public static T CloseButtonCommand<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonCommandProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CloseButtonCommand<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonCommandProperty, ps, () => control.CloseButtonCommand = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CloseButtonCommand<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonCommandProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CloseButtonCommand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonCommandProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CloseButtonCommand<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonCommandProperty, ps, () => control.CloseButtonCommand = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CloseButtonCommandParameterProperty

/*BindFromExpressionSetterGenerator*/
public static T CloseButtonCommandParameter<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonCommandParameterProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CloseButtonCommandParameter<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonCommandParameterProperty, ps, () => control.CloseButtonCommandParameter = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CloseButtonCommandParameter<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonCommandParameterProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CloseButtonCommandParameter<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonCommandParameterProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CloseButtonCommandParameter<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonCommandParameterProperty, ps, () => control.CloseButtonCommandParameter = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // PlacementMarginProperty

/*BindFromExpressionSetterGenerator*/
public static T PlacementMargin<T>(this T control, Func<Avalonia.Thickness> func, Action<Avalonia.Thickness>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.PlacementMarginProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T PlacementMargin<T>(this T control, Avalonia.Thickness value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.PlacementMarginProperty, ps, () => control.PlacementMargin = value, bindingMode, converter, bindingSource);

/*ValueOverloadsSetterGenerator*/

public static T PlacementMargin<T>(this T control, System.Double uniformLength = default) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(() => control.PlacementMargin = new Avalonia.Thickness(uniformLength));
public static T PlacementMargin<T>(this T control, System.Double horizontal = default, System.Double vertical = default) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(() => control.PlacementMargin = new Avalonia.Thickness(horizontal, vertical));
public static T PlacementMargin<T>(this T control, System.Double left = default, System.Double top = default, System.Double right = default, System.Double bottom = default) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(() => control.PlacementMargin = new Avalonia.Thickness(left, top, right, bottom));

/*BindSetterGenerator*/
public static T PlacementMargin<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.PlacementMarginProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T PlacementMargin<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.PlacementMarginProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T PlacementMargin<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Thickness> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.PlacementMarginProperty, ps, () => control.PlacementMargin = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ShouldConstrainToRootBoundsProperty

/*BindFromExpressionSetterGenerator*/
public static T ShouldConstrainToRootBounds<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.ShouldConstrainToRootBoundsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ShouldConstrainToRootBounds<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.ShouldConstrainToRootBoundsProperty, ps, () => control.ShouldConstrainToRootBounds = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ShouldConstrainToRootBounds<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.ShouldConstrainToRootBoundsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ShouldConstrainToRootBounds<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.ShouldConstrainToRootBoundsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ShouldConstrainToRootBounds<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.ShouldConstrainToRootBoundsProperty, ps, () => control.ShouldConstrainToRootBounds = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsLightDismissEnabledProperty

/*BindFromExpressionSetterGenerator*/
public static T IsLightDismissEnabled<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.IsLightDismissEnabledProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsLightDismissEnabled<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.IsLightDismissEnabledProperty, ps, () => control.IsLightDismissEnabled = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsLightDismissEnabled<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.IsLightDismissEnabledProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsLightDismissEnabled<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.IsLightDismissEnabledProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsLightDismissEnabled<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.IsLightDismissEnabledProperty, ps, () => control.IsLightDismissEnabled = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // PreferredPlacementProperty

/*BindFromExpressionSetterGenerator*/
public static T PreferredPlacement<T>(this T control, Func<FluentAvalonia.UI.Controls.TeachingTipPlacementMode> func, Action<FluentAvalonia.UI.Controls.TeachingTipPlacementMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.PreferredPlacementProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T PreferredPlacement<T>(this T control, FluentAvalonia.UI.Controls.TeachingTipPlacementMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.PreferredPlacementProperty, ps, () => control.PreferredPlacement = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T PreferredPlacement<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.PreferredPlacementProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T PreferredPlacement<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.PreferredPlacementProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T PreferredPlacement<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.TeachingTipPlacementMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.PreferredPlacementProperty, ps, () => control.PreferredPlacement = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // HeroContentPlacementProperty

/*BindFromExpressionSetterGenerator*/
public static T HeroContentPlacement<T>(this T control, Func<FluentAvalonia.UI.Controls.TeachingTipHeroContentPlacementMode> func, Action<FluentAvalonia.UI.Controls.TeachingTipHeroContentPlacementMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.HeroContentPlacementProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T HeroContentPlacement<T>(this T control, FluentAvalonia.UI.Controls.TeachingTipHeroContentPlacementMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.HeroContentPlacementProperty, ps, () => control.HeroContentPlacement = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T HeroContentPlacement<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.HeroContentPlacementProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T HeroContentPlacement<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.HeroContentPlacementProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T HeroContentPlacement<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.TeachingTipHeroContentPlacementMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.HeroContentPlacementProperty, ps, () => control.HeroContentPlacement = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // HeroContentProperty

/*BindFromExpressionSetterGenerator*/
public static T HeroContent<T>(this T control, Func<Avalonia.Controls.Control> func, Action<Avalonia.Controls.Control>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.HeroContentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T HeroContent<T>(this T control, Avalonia.Controls.Control value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.HeroContentProperty, ps, () => control.HeroContent = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T HeroContent<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.HeroContentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T HeroContent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.HeroContentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T HeroContent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Control> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.HeroContentProperty, ps, () => control.HeroContent = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IconSourceProperty

/*BindFromExpressionSetterGenerator*/
public static T IconSource<T>(this T control, Func<FluentAvalonia.UI.Controls.IconSource> func, Action<FluentAvalonia.UI.Controls.IconSource>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.IconSourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IconSource<T>(this T control, FluentAvalonia.UI.Controls.IconSource value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.IconSourceProperty, ps, () => control.IconSource = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IconSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.IconSourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IconSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.TeachingTip
   => control._set(FluentAvalonia.UI.Controls.TeachingTip.IconSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IconSource<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.IconSource> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.TeachingTip
=> control._setEx(FluentAvalonia.UI.Controls.TeachingTip.IconSourceProperty, ps, () => control.IconSource = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // ActionButtonClick

/*ActionToEventGenerator*/
    public static T OnActionButtonClick<T>(this T control, Action<FluentAvalonia.UI.Controls.TeachingTip, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.TeachingTip => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.TeachingTip,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.ActionButtonClick += h);


 // CloseButtonClick

/*ActionToEventGenerator*/
    public static T OnCloseButtonClick<T>(this T control, Action<FluentAvalonia.UI.Controls.TeachingTip, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.TeachingTip => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.TeachingTip,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.CloseButtonClick += h);


 // Closing

/*ActionToEventGenerator*/
    public static T OnClosing<T>(this T control, Action<FluentAvalonia.UI.Controls.TeachingTip, FluentAvalonia.UI.Controls.TeachingTipClosingEventArgs> action) where T : FluentAvalonia.UI.Controls.TeachingTip => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.TeachingTip,FluentAvalonia.UI.Controls.TeachingTipClosingEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Closing += h);


 // Closed

/*ActionToEventGenerator*/
    public static T OnClosed<T>(this T control, Action<FluentAvalonia.UI.Controls.TeachingTip, FluentAvalonia.UI.Controls.TeachingTipClosedEventArgs> action) where T : FluentAvalonia.UI.Controls.TeachingTip => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.TeachingTip,FluentAvalonia.UI.Controls.TeachingTipClosedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Closed += h);



//================= Styles ======================//
 // TitleProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Title<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.TitleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Title<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.TitleProperty, binding);


 // SubtitleProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Subtitle<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.SubtitleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Subtitle<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.SubtitleProperty, binding);


 // IsOpenProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsOpen<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.IsOpenProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsOpen<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.IsOpenProperty, binding);


 // TargetProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Target<T>(this Style<T> style, Avalonia.Controls.Control value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.TargetProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Target<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.TargetProperty, binding);


 // TailVisibilityProperty

/*ValueStyleSetterGenerator*/
public static Style<T> TailVisibility<T>(this Style<T> style, FluentAvalonia.UI.Controls.TeachingTipTailVisibility value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.TailVisibilityProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> TailVisibility<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.TailVisibilityProperty, binding);


 // ActionButtonContentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ActionButtonContent<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonContentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ActionButtonContent<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonContentProperty, binding);


 // ActionButtonStyleProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ActionButtonStyle<T>(this Style<T> style, Avalonia.Styling.ControlTheme value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonStyleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ActionButtonStyle<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonStyleProperty, binding);


 // ActionButtonCommandProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ActionButtonCommand<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonCommandProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ActionButtonCommand<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonCommandProperty, binding);


 // ActionButtonCommandParameterProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ActionButtonCommandParameter<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonCommandParameterProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ActionButtonCommandParameter<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonCommandParameterProperty, binding);


 // CloseButtonContentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> CloseButtonContent<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonContentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CloseButtonContent<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonContentProperty, binding);


 // CloseButtonStyleProperty

/*ValueStyleSetterGenerator*/
public static Style<T> CloseButtonStyle<T>(this Style<T> style, Avalonia.Styling.ControlTheme value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonStyleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CloseButtonStyle<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonStyleProperty, binding);


 // CloseButtonCommandProperty

/*ValueStyleSetterGenerator*/
public static Style<T> CloseButtonCommand<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonCommandProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CloseButtonCommand<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonCommandProperty, binding);


 // CloseButtonCommandParameterProperty

/*ValueStyleSetterGenerator*/
public static Style<T> CloseButtonCommandParameter<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonCommandParameterProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CloseButtonCommandParameter<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonCommandParameterProperty, binding);


 // PlacementMarginProperty

/*ValueStyleSetterGenerator*/
public static Style<T> PlacementMargin<T>(this Style<T> style, Avalonia.Thickness value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.PlacementMarginProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> PlacementMargin<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.PlacementMarginProperty, binding);

/*ValueOverloadsStyleSetterGenerator*/
public static Style<T> PlacementMargin<T>(this Style<T> style, System.Double uniformLength) where T : FluentAvalonia.UI.Controls.TeachingTip
   => style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.PlacementMarginProperty, new Avalonia.Thickness(uniformLength));public static Style<T> PlacementMargin<T>(this Style<T> style, System.Double horizontal, System.Double vertical) where T : FluentAvalonia.UI.Controls.TeachingTip
   => style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.PlacementMarginProperty, new Avalonia.Thickness(horizontal, vertical));public static Style<T> PlacementMargin<T>(this Style<T> style, System.Double left, System.Double top, System.Double right, System.Double bottom) where T : FluentAvalonia.UI.Controls.TeachingTip
   => style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.PlacementMarginProperty, new Avalonia.Thickness(left, top, right, bottom));


 // ShouldConstrainToRootBoundsProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ShouldConstrainToRootBounds<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.ShouldConstrainToRootBoundsProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ShouldConstrainToRootBounds<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.ShouldConstrainToRootBoundsProperty, binding);


 // IsLightDismissEnabledProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsLightDismissEnabled<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.IsLightDismissEnabledProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsLightDismissEnabled<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.IsLightDismissEnabledProperty, binding);


 // PreferredPlacementProperty

/*ValueStyleSetterGenerator*/
public static Style<T> PreferredPlacement<T>(this Style<T> style, FluentAvalonia.UI.Controls.TeachingTipPlacementMode value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.PreferredPlacementProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> PreferredPlacement<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.PreferredPlacementProperty, binding);


 // HeroContentPlacementProperty

/*ValueStyleSetterGenerator*/
public static Style<T> HeroContentPlacement<T>(this Style<T> style, FluentAvalonia.UI.Controls.TeachingTipHeroContentPlacementMode value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.HeroContentPlacementProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> HeroContentPlacement<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.HeroContentPlacementProperty, binding);


 // HeroContentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> HeroContent<T>(this Style<T> style, Avalonia.Controls.Control value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.HeroContentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> HeroContent<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.HeroContentProperty, binding);


 // IconSourceProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IconSource<T>(this Style<T> style, FluentAvalonia.UI.Controls.IconSource value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.IconSourceProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IconSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.IconSourceProperty, binding);



}
