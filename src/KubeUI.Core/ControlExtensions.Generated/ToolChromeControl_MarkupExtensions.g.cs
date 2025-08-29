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
public static partial class ToolChromeControl_MarkupExtensions
{
//================= Properties ======================//
 // IsActive

/*BindFromExpressionSetterGenerator*/
public static T IsActive<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.ToolChromeControl 
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.IsActiveProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsActive<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> control._setEx(Dock.Avalonia.Controls.ToolChromeControl.IsActiveProperty, ps, () => control.IsActive = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsActive<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.ToolChromeControl 
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.IsActiveProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsActive<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.ToolChromeControl 
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.IsActiveProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsActive<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> control._setEx(Dock.Avalonia.Controls.ToolChromeControl.IsActiveProperty, ps, () => control.IsActive = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsPinned

/*BindFromExpressionSetterGenerator*/
public static T IsPinned<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.ToolChromeControl 
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.IsPinnedProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsPinned<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> control._setEx(Dock.Avalonia.Controls.ToolChromeControl.IsPinnedProperty, ps, () => control.IsPinned = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsPinned<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.ToolChromeControl 
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.IsPinnedProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsPinned<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.ToolChromeControl 
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.IsPinnedProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsPinned<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> control._setEx(Dock.Avalonia.Controls.ToolChromeControl.IsPinnedProperty, ps, () => control.IsPinned = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsFloating

/*BindFromExpressionSetterGenerator*/
public static T IsFloating<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.ToolChromeControl 
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.IsFloatingProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsFloating<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> control._setEx(Dock.Avalonia.Controls.ToolChromeControl.IsFloatingProperty, ps, () => control.IsFloating = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsFloating<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.ToolChromeControl 
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.IsFloatingProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsFloating<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.ToolChromeControl 
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.IsFloatingProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsFloating<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> control._setEx(Dock.Avalonia.Controls.ToolChromeControl.IsFloatingProperty, ps, () => control.IsFloating = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsMaximized

/*BindFromExpressionSetterGenerator*/
public static T IsMaximized<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.ToolChromeControl 
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.IsMaximizedProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsMaximized<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> control._setEx(Dock.Avalonia.Controls.ToolChromeControl.IsMaximizedProperty, ps, () => control.IsMaximized = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsMaximized<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.ToolChromeControl 
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.IsMaximizedProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsMaximized<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.ToolChromeControl 
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.IsMaximizedProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsMaximized<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> control._setEx(Dock.Avalonia.Controls.ToolChromeControl.IsMaximizedProperty, ps, () => control.IsMaximized = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ToolFlyout

/*ValueSetterGenerator*/
public static T ToolFlyout<T>(this T control, Avalonia.Controls.Primitives.FlyoutBase value) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> control._set(() => control.ToolFlyout = value!);

/*BindFromExpressionSetterGenerator*/
public static T ToolFlyout<T>(this T control, Func<Avalonia.Controls.Primitives.FlyoutBase> func, Action<Avalonia.Controls.Primitives.FlyoutBase>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : Dock.Avalonia.Controls.ToolChromeControl 
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.ToolFlyoutProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T ToolFlyout<T>(this T control,Avalonia.Controls.Primitives.FlyoutBase value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> control._setEx(Dock.Avalonia.Controls.ToolChromeControl.ToolFlyoutProperty, ps, () => control.ToolFlyout = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ToolFlyout<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.ToolChromeControl 
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.ToolFlyoutProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ToolFlyout<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.ToolChromeControl 
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.ToolFlyoutProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T ToolFlyout<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Primitives.FlyoutBase> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> control._setEx(Dock.Avalonia.Controls.ToolChromeControl.ToolFlyoutProperty, ps, () => control.ToolFlyout = converter.TryConvert(value)!, bindingMode, converter, bindingSource);


 // CloseButtonTheme

/*ValueSetterGenerator*/
public static T CloseButtonTheme<T>(this T control, Avalonia.Styling.ControlTheme value) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> control._set(() => control.CloseButtonTheme = value!);

/*BindFromExpressionSetterGenerator*/
public static T CloseButtonTheme<T>(this T control, Func<Avalonia.Styling.ControlTheme> func, Action<Avalonia.Styling.ControlTheme>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : Dock.Avalonia.Controls.ToolChromeControl 
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.CloseButtonThemeProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T CloseButtonTheme<T>(this T control,Avalonia.Styling.ControlTheme value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> control._setEx(Dock.Avalonia.Controls.ToolChromeControl.CloseButtonThemeProperty, ps, () => control.CloseButtonTheme = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CloseButtonTheme<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.ToolChromeControl 
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.CloseButtonThemeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CloseButtonTheme<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.ToolChromeControl 
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.CloseButtonThemeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T CloseButtonTheme<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ControlTheme> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> control._setEx(Dock.Avalonia.Controls.ToolChromeControl.CloseButtonThemeProperty, ps, () => control.CloseButtonTheme = converter.TryConvert(value)!, bindingMode, converter, bindingSource);


 // MaximizeButtonTheme

/*ValueSetterGenerator*/
public static T MaximizeButtonTheme<T>(this T control, Avalonia.Styling.ControlTheme value) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> control._set(() => control.MaximizeButtonTheme = value!);

/*BindFromExpressionSetterGenerator*/
public static T MaximizeButtonTheme<T>(this T control, Func<Avalonia.Styling.ControlTheme> func, Action<Avalonia.Styling.ControlTheme>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : Dock.Avalonia.Controls.ToolChromeControl 
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.MaximizeButtonThemeProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T MaximizeButtonTheme<T>(this T control,Avalonia.Styling.ControlTheme value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> control._setEx(Dock.Avalonia.Controls.ToolChromeControl.MaximizeButtonThemeProperty, ps, () => control.MaximizeButtonTheme = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T MaximizeButtonTheme<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.ToolChromeControl 
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.MaximizeButtonThemeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T MaximizeButtonTheme<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.ToolChromeControl 
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.MaximizeButtonThemeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T MaximizeButtonTheme<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ControlTheme> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> control._setEx(Dock.Avalonia.Controls.ToolChromeControl.MaximizeButtonThemeProperty, ps, () => control.MaximizeButtonTheme = converter.TryConvert(value)!, bindingMode, converter, bindingSource);


 // PinButtonTheme

/*ValueSetterGenerator*/
public static T PinButtonTheme<T>(this T control, Avalonia.Styling.ControlTheme value) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> control._set(() => control.PinButtonTheme = value!);

/*BindFromExpressionSetterGenerator*/
public static T PinButtonTheme<T>(this T control, Func<Avalonia.Styling.ControlTheme> func, Action<Avalonia.Styling.ControlTheme>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : Dock.Avalonia.Controls.ToolChromeControl 
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.PinButtonThemeProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T PinButtonTheme<T>(this T control,Avalonia.Styling.ControlTheme value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> control._setEx(Dock.Avalonia.Controls.ToolChromeControl.PinButtonThemeProperty, ps, () => control.PinButtonTheme = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T PinButtonTheme<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.ToolChromeControl 
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.PinButtonThemeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T PinButtonTheme<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.ToolChromeControl 
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.PinButtonThemeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T PinButtonTheme<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ControlTheme> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> control._setEx(Dock.Avalonia.Controls.ToolChromeControl.PinButtonThemeProperty, ps, () => control.PinButtonTheme = converter.TryConvert(value)!, bindingMode, converter, bindingSource);


 // MenuButtonTheme

/*ValueSetterGenerator*/
public static T MenuButtonTheme<T>(this T control, Avalonia.Styling.ControlTheme value) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> control._set(() => control.MenuButtonTheme = value!);

/*BindFromExpressionSetterGenerator*/
public static T MenuButtonTheme<T>(this T control, Func<Avalonia.Styling.ControlTheme> func, Action<Avalonia.Styling.ControlTheme>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : Dock.Avalonia.Controls.ToolChromeControl 
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.MenuButtonThemeProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T MenuButtonTheme<T>(this T control,Avalonia.Styling.ControlTheme value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> control._setEx(Dock.Avalonia.Controls.ToolChromeControl.MenuButtonThemeProperty, ps, () => control.MenuButtonTheme = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T MenuButtonTheme<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.ToolChromeControl 
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.MenuButtonThemeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T MenuButtonTheme<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.ToolChromeControl 
   => control._set(Dock.Avalonia.Controls.ToolChromeControl.MenuButtonThemeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T MenuButtonTheme<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ControlTheme> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> control._setEx(Dock.Avalonia.Controls.ToolChromeControl.MenuButtonThemeProperty, ps, () => control.MenuButtonTheme = converter.TryConvert(value)!, bindingMode, converter, bindingSource);



//================= Styles ======================//
 // IsActive

/*ValueStyleSetterGenerator*/
public static Style<T> IsActive<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> style._addSetter(Dock.Avalonia.Controls.ToolChromeControl.IsActiveProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsActive<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> style._addSetter(Dock.Avalonia.Controls.ToolChromeControl.IsActiveProperty, binding);


 // IsPinned

/*ValueStyleSetterGenerator*/
public static Style<T> IsPinned<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> style._addSetter(Dock.Avalonia.Controls.ToolChromeControl.IsPinnedProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsPinned<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> style._addSetter(Dock.Avalonia.Controls.ToolChromeControl.IsPinnedProperty, binding);


 // IsFloating

/*ValueStyleSetterGenerator*/
public static Style<T> IsFloating<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> style._addSetter(Dock.Avalonia.Controls.ToolChromeControl.IsFloatingProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsFloating<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> style._addSetter(Dock.Avalonia.Controls.ToolChromeControl.IsFloatingProperty, binding);


 // IsMaximized

/*ValueStyleSetterGenerator*/
public static Style<T> IsMaximized<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> style._addSetter(Dock.Avalonia.Controls.ToolChromeControl.IsMaximizedProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsMaximized<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> style._addSetter(Dock.Avalonia.Controls.ToolChromeControl.IsMaximizedProperty, binding);


 // ToolFlyout

/*ValueStyleSetterGenerator*/
public static Style<T> ToolFlyout<T>(this Style<T> style, Avalonia.Controls.Primitives.FlyoutBase value) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> style._addSetter(Dock.Avalonia.Controls.ToolChromeControl.ToolFlyoutProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> ToolFlyout<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> style._addSetter(Dock.Avalonia.Controls.ToolChromeControl.ToolFlyoutProperty, binding);


 // CloseButtonTheme

/*ValueStyleSetterGenerator*/
public static Style<T> CloseButtonTheme<T>(this Style<T> style, Avalonia.Styling.ControlTheme value) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> style._addSetter(Dock.Avalonia.Controls.ToolChromeControl.CloseButtonThemeProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> CloseButtonTheme<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> style._addSetter(Dock.Avalonia.Controls.ToolChromeControl.CloseButtonThemeProperty, binding);


 // MaximizeButtonTheme

/*ValueStyleSetterGenerator*/
public static Style<T> MaximizeButtonTheme<T>(this Style<T> style, Avalonia.Styling.ControlTheme value) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> style._addSetter(Dock.Avalonia.Controls.ToolChromeControl.MaximizeButtonThemeProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> MaximizeButtonTheme<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> style._addSetter(Dock.Avalonia.Controls.ToolChromeControl.MaximizeButtonThemeProperty, binding);


 // PinButtonTheme

/*ValueStyleSetterGenerator*/
public static Style<T> PinButtonTheme<T>(this Style<T> style, Avalonia.Styling.ControlTheme value) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> style._addSetter(Dock.Avalonia.Controls.ToolChromeControl.PinButtonThemeProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> PinButtonTheme<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> style._addSetter(Dock.Avalonia.Controls.ToolChromeControl.PinButtonThemeProperty, binding);


 // MenuButtonTheme

/*ValueStyleSetterGenerator*/
public static Style<T> MenuButtonTheme<T>(this Style<T> style, Avalonia.Styling.ControlTheme value) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> style._addSetter(Dock.Avalonia.Controls.ToolChromeControl.MenuButtonThemeProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> MenuButtonTheme<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.ToolChromeControl 
=> style._addSetter(Dock.Avalonia.Controls.ToolChromeControl.MenuButtonThemeProperty, binding);



}
