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
public static partial class ContentDialog_MarkupExtensions
{
//================= Properties ======================//
 // CloseButtonCommand

/*BindFromExpressionSetterGenerator*/
public static T CloseButtonCommand<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonCommandProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CloseButtonCommand<T>(this T control,System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonCommandProperty, ps, () => control.CloseButtonCommand = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CloseButtonCommand<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonCommandProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CloseButtonCommand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonCommandProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CloseButtonCommand<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonCommandProperty, ps, () => control.CloseButtonCommand = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CloseButtonCommandParameter

/*BindFromExpressionSetterGenerator*/
public static T CloseButtonCommandParameter<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonCommandParameterProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CloseButtonCommandParameter<T>(this T control,System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonCommandParameterProperty, ps, () => control.CloseButtonCommandParameter = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CloseButtonCommandParameter<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonCommandParameterProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CloseButtonCommandParameter<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonCommandParameterProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CloseButtonCommandParameter<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonCommandParameterProperty, ps, () => control.CloseButtonCommandParameter = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CloseButtonText

/*BindFromExpressionSetterGenerator*/
public static T CloseButtonText<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonTextProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CloseButtonText<T>(this T control,System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonTextProperty, ps, () => control.CloseButtonText = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CloseButtonText<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonTextProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CloseButtonText<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonTextProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CloseButtonText<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonTextProperty, ps, () => control.CloseButtonText = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // DefaultButton

/*BindFromExpressionSetterGenerator*/
public static T DefaultButton<T>(this T control, Func<FluentAvalonia.UI.Controls.ContentDialogButton> func, Action<FluentAvalonia.UI.Controls.ContentDialogButton>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.DefaultButtonProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T DefaultButton<T>(this T control,FluentAvalonia.UI.Controls.ContentDialogButton value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.DefaultButtonProperty, ps, () => control.DefaultButton = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T DefaultButton<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.DefaultButtonProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T DefaultButton<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.DefaultButtonProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T DefaultButton<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.ContentDialogButton> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.DefaultButtonProperty, ps, () => control.DefaultButton = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsPrimaryButtonEnabled

/*BindFromExpressionSetterGenerator*/
public static T IsPrimaryButtonEnabled<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.IsPrimaryButtonEnabledProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsPrimaryButtonEnabled<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.IsPrimaryButtonEnabledProperty, ps, () => control.IsPrimaryButtonEnabled = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsPrimaryButtonEnabled<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.IsPrimaryButtonEnabledProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsPrimaryButtonEnabled<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.IsPrimaryButtonEnabledProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsPrimaryButtonEnabled<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.IsPrimaryButtonEnabledProperty, ps, () => control.IsPrimaryButtonEnabled = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsSecondaryButtonEnabled

/*BindFromExpressionSetterGenerator*/
public static T IsSecondaryButtonEnabled<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.IsSecondaryButtonEnabledProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsSecondaryButtonEnabled<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.IsSecondaryButtonEnabledProperty, ps, () => control.IsSecondaryButtonEnabled = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsSecondaryButtonEnabled<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.IsSecondaryButtonEnabledProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsSecondaryButtonEnabled<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.IsSecondaryButtonEnabledProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsSecondaryButtonEnabled<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.IsSecondaryButtonEnabledProperty, ps, () => control.IsSecondaryButtonEnabled = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // PrimaryButtonCommand

/*BindFromExpressionSetterGenerator*/
public static T PrimaryButtonCommand<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonCommandProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T PrimaryButtonCommand<T>(this T control,System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonCommandProperty, ps, () => control.PrimaryButtonCommand = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T PrimaryButtonCommand<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonCommandProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T PrimaryButtonCommand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonCommandProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T PrimaryButtonCommand<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonCommandProperty, ps, () => control.PrimaryButtonCommand = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // PrimaryButtonCommandParameter

/*BindFromExpressionSetterGenerator*/
public static T PrimaryButtonCommandParameter<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonCommandParameterProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T PrimaryButtonCommandParameter<T>(this T control,System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonCommandParameterProperty, ps, () => control.PrimaryButtonCommandParameter = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T PrimaryButtonCommandParameter<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonCommandParameterProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T PrimaryButtonCommandParameter<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonCommandParameterProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T PrimaryButtonCommandParameter<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonCommandParameterProperty, ps, () => control.PrimaryButtonCommandParameter = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // PrimaryButtonText

/*BindFromExpressionSetterGenerator*/
public static T PrimaryButtonText<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonTextProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T PrimaryButtonText<T>(this T control,System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonTextProperty, ps, () => control.PrimaryButtonText = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T PrimaryButtonText<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonTextProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T PrimaryButtonText<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonTextProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T PrimaryButtonText<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonTextProperty, ps, () => control.PrimaryButtonText = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SecondaryButtonCommand

/*BindFromExpressionSetterGenerator*/
public static T SecondaryButtonCommand<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonCommandProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SecondaryButtonCommand<T>(this T control,System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonCommandProperty, ps, () => control.SecondaryButtonCommand = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SecondaryButtonCommand<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonCommandProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SecondaryButtonCommand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonCommandProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SecondaryButtonCommand<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonCommandProperty, ps, () => control.SecondaryButtonCommand = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SecondaryButtonCommandParameter

/*BindFromExpressionSetterGenerator*/
public static T SecondaryButtonCommandParameter<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonCommandParameterProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SecondaryButtonCommandParameter<T>(this T control,System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonCommandParameterProperty, ps, () => control.SecondaryButtonCommandParameter = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SecondaryButtonCommandParameter<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonCommandParameterProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SecondaryButtonCommandParameter<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonCommandParameterProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SecondaryButtonCommandParameter<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonCommandParameterProperty, ps, () => control.SecondaryButtonCommandParameter = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SecondaryButtonText

/*BindFromExpressionSetterGenerator*/
public static T SecondaryButtonText<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonTextProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SecondaryButtonText<T>(this T control,System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonTextProperty, ps, () => control.SecondaryButtonText = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SecondaryButtonText<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonTextProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SecondaryButtonText<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonTextProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SecondaryButtonText<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonTextProperty, ps, () => control.SecondaryButtonText = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // Title

/*BindFromExpressionSetterGenerator*/
public static T Title<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.TitleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Title<T>(this T control,System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.TitleProperty, ps, () => control.Title = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Title<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.TitleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Title<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.TitleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Title<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.TitleProperty, ps, () => control.Title = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TitleTemplate

/*BindFromExpressionSetterGenerator*/
public static T TitleTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.TitleTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T TitleTemplate<T>(this T control,Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.TitleTemplateProperty, ps, () => control.TitleTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TitleTemplate<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.TitleTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TitleTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.TitleTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T TitleTemplate<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.TitleTemplateProperty, ps, () => control.TitleTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // FullSizeDesired

/*BindFromExpressionSetterGenerator*/
public static T FullSizeDesired<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.FullSizeDesiredProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T FullSizeDesired<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.FullSizeDesiredProperty, ps, () => control.FullSizeDesired = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T FullSizeDesired<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.FullSizeDesiredProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T FullSizeDesired<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
   => control._set(FluentAvalonia.UI.Controls.ContentDialog.FullSizeDesiredProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T FullSizeDesired<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> control._setEx(FluentAvalonia.UI.Controls.ContentDialog.FullSizeDesiredProperty, ps, () => control.FullSizeDesired = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // Opening

/*ActionToEventGenerator*/
public static T OnOpening<T>(this T control, Action<FluentAvalonia.UI.Controls.ContentDialog, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.ContentDialog  => 
 control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.ContentDialog,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Opening += h);


 // Opened

/*ActionToEventGenerator*/
public static T OnOpened<T>(this T control, Action<FluentAvalonia.UI.Controls.ContentDialog, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.ContentDialog  => 
 control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.ContentDialog,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Opened += h);


 // Closing

/*ActionToEventGenerator*/
public static T OnClosing<T>(this T control, Action<FluentAvalonia.UI.Controls.ContentDialog, FluentAvalonia.UI.Controls.ContentDialogClosingEventArgs> action) where T : FluentAvalonia.UI.Controls.ContentDialog  => 
 control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.ContentDialog,FluentAvalonia.UI.Controls.ContentDialogClosingEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Closing += h);


 // Closed

/*ActionToEventGenerator*/
public static T OnClosed<T>(this T control, Action<FluentAvalonia.UI.Controls.ContentDialog, FluentAvalonia.UI.Controls.ContentDialogClosedEventArgs> action) where T : FluentAvalonia.UI.Controls.ContentDialog  => 
 control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.ContentDialog,FluentAvalonia.UI.Controls.ContentDialogClosedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Closed += h);


 // PrimaryButtonClick

/*ActionToEventGenerator*/
public static T OnPrimaryButtonClick<T>(this T control, Action<FluentAvalonia.UI.Controls.ContentDialog, FluentAvalonia.UI.Controls.ContentDialogButtonClickEventArgs> action) where T : FluentAvalonia.UI.Controls.ContentDialog  => 
 control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.ContentDialog,FluentAvalonia.UI.Controls.ContentDialogButtonClickEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.PrimaryButtonClick += h);


 // SecondaryButtonClick

/*ActionToEventGenerator*/
public static T OnSecondaryButtonClick<T>(this T control, Action<FluentAvalonia.UI.Controls.ContentDialog, FluentAvalonia.UI.Controls.ContentDialogButtonClickEventArgs> action) where T : FluentAvalonia.UI.Controls.ContentDialog  => 
 control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.ContentDialog,FluentAvalonia.UI.Controls.ContentDialogButtonClickEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.SecondaryButtonClick += h);


 // CloseButtonClick

/*ActionToEventGenerator*/
public static T OnCloseButtonClick<T>(this T control, Action<FluentAvalonia.UI.Controls.ContentDialog, FluentAvalonia.UI.Controls.ContentDialogButtonClickEventArgs> action) where T : FluentAvalonia.UI.Controls.ContentDialog  => 
 control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.ContentDialog,FluentAvalonia.UI.Controls.ContentDialogButtonClickEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.CloseButtonClick += h);



//================= Styles ======================//
 // CloseButtonCommand

/*ValueStyleSetterGenerator*/
public static Style<T> CloseButtonCommand<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonCommandProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CloseButtonCommand<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonCommandProperty, binding);


 // CloseButtonCommandParameter

/*ValueStyleSetterGenerator*/
public static Style<T> CloseButtonCommandParameter<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonCommandParameterProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CloseButtonCommandParameter<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonCommandParameterProperty, binding);


 // CloseButtonText

/*ValueStyleSetterGenerator*/
public static Style<T> CloseButtonText<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonTextProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CloseButtonText<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonTextProperty, binding);


 // DefaultButton

/*ValueStyleSetterGenerator*/
public static Style<T> DefaultButton<T>(this Style<T> style, FluentAvalonia.UI.Controls.ContentDialogButton value) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.DefaultButtonProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> DefaultButton<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.DefaultButtonProperty, binding);


 // IsPrimaryButtonEnabled

/*ValueStyleSetterGenerator*/
public static Style<T> IsPrimaryButtonEnabled<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.IsPrimaryButtonEnabledProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsPrimaryButtonEnabled<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.IsPrimaryButtonEnabledProperty, binding);


 // IsSecondaryButtonEnabled

/*ValueStyleSetterGenerator*/
public static Style<T> IsSecondaryButtonEnabled<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.IsSecondaryButtonEnabledProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsSecondaryButtonEnabled<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.IsSecondaryButtonEnabledProperty, binding);


 // PrimaryButtonCommand

/*ValueStyleSetterGenerator*/
public static Style<T> PrimaryButtonCommand<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonCommandProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> PrimaryButtonCommand<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonCommandProperty, binding);


 // PrimaryButtonCommandParameter

/*ValueStyleSetterGenerator*/
public static Style<T> PrimaryButtonCommandParameter<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonCommandParameterProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> PrimaryButtonCommandParameter<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonCommandParameterProperty, binding);


 // PrimaryButtonText

/*ValueStyleSetterGenerator*/
public static Style<T> PrimaryButtonText<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonTextProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> PrimaryButtonText<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonTextProperty, binding);


 // SecondaryButtonCommand

/*ValueStyleSetterGenerator*/
public static Style<T> SecondaryButtonCommand<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonCommandProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SecondaryButtonCommand<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonCommandProperty, binding);


 // SecondaryButtonCommandParameter

/*ValueStyleSetterGenerator*/
public static Style<T> SecondaryButtonCommandParameter<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonCommandParameterProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SecondaryButtonCommandParameter<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonCommandParameterProperty, binding);


 // SecondaryButtonText

/*ValueStyleSetterGenerator*/
public static Style<T> SecondaryButtonText<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonTextProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SecondaryButtonText<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonTextProperty, binding);


 // Title

/*ValueStyleSetterGenerator*/
public static Style<T> Title<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.TitleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Title<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.TitleProperty, binding);


 // TitleTemplate

/*ValueStyleSetterGenerator*/
public static Style<T> TitleTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.TitleTemplateProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> TitleTemplate<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.TitleTemplateProperty, binding);


 // FullSizeDesired

/*ValueStyleSetterGenerator*/
public static Style<T> FullSizeDesired<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.FullSizeDesiredProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> FullSizeDesired<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog 
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.FullSizeDesiredProperty, binding);



}
