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
public static partial class DockTargetBase_MarkupExtensions
{
//================= Properties ======================//
 // ShowIndicatorsOnly

/*ValueSetterGenerator*/
public static T ShowIndicatorsOnly<T>(this T control, System.Boolean value) where T : Dock.Avalonia.Controls.DockTargetBase 
=> control._set(() => control.ShowIndicatorsOnly = value!);

/*BindFromExpressionSetterGenerator*/
public static T ShowIndicatorsOnly<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : Dock.Avalonia.Controls.DockTargetBase 
   => control._set(Dock.Avalonia.Controls.DockTargetBase.ShowIndicatorsOnlyProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T ShowIndicatorsOnly<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.DockTargetBase 
=> control._setEx(Dock.Avalonia.Controls.DockTargetBase.ShowIndicatorsOnlyProperty, ps, () => control.ShowIndicatorsOnly = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ShowIndicatorsOnly<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.DockTargetBase 
   => control._set(Dock.Avalonia.Controls.DockTargetBase.ShowIndicatorsOnlyProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ShowIndicatorsOnly<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.DockTargetBase 
   => control._set(Dock.Avalonia.Controls.DockTargetBase.ShowIndicatorsOnlyProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T ShowIndicatorsOnly<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.DockTargetBase 
=> control._setEx(Dock.Avalonia.Controls.DockTargetBase.ShowIndicatorsOnlyProperty, ps, () => control.ShowIndicatorsOnly = converter.TryConvert(value)!, bindingMode, converter, bindingSource);


 // ShowHorizontalTargets

/*ValueSetterGenerator*/
public static T ShowHorizontalTargets<T>(this T control, System.Boolean value) where T : Dock.Avalonia.Controls.DockTargetBase 
=> control._set(() => control.ShowHorizontalTargets = value!);

/*BindFromExpressionSetterGenerator*/
public static T ShowHorizontalTargets<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : Dock.Avalonia.Controls.DockTargetBase 
   => control._set(Dock.Avalonia.Controls.DockTargetBase.ShowHorizontalTargetsProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T ShowHorizontalTargets<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.DockTargetBase 
=> control._setEx(Dock.Avalonia.Controls.DockTargetBase.ShowHorizontalTargetsProperty, ps, () => control.ShowHorizontalTargets = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ShowHorizontalTargets<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.DockTargetBase 
   => control._set(Dock.Avalonia.Controls.DockTargetBase.ShowHorizontalTargetsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ShowHorizontalTargets<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.DockTargetBase 
   => control._set(Dock.Avalonia.Controls.DockTargetBase.ShowHorizontalTargetsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T ShowHorizontalTargets<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.DockTargetBase 
=> control._setEx(Dock.Avalonia.Controls.DockTargetBase.ShowHorizontalTargetsProperty, ps, () => control.ShowHorizontalTargets = converter.TryConvert(value)!, bindingMode, converter, bindingSource);


 // ShowVerticalTargets

/*ValueSetterGenerator*/
public static T ShowVerticalTargets<T>(this T control, System.Boolean value) where T : Dock.Avalonia.Controls.DockTargetBase 
=> control._set(() => control.ShowVerticalTargets = value!);

/*BindFromExpressionSetterGenerator*/
public static T ShowVerticalTargets<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : Dock.Avalonia.Controls.DockTargetBase 
   => control._set(Dock.Avalonia.Controls.DockTargetBase.ShowVerticalTargetsProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T ShowVerticalTargets<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.DockTargetBase 
=> control._setEx(Dock.Avalonia.Controls.DockTargetBase.ShowVerticalTargetsProperty, ps, () => control.ShowVerticalTargets = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ShowVerticalTargets<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.DockTargetBase 
   => control._set(Dock.Avalonia.Controls.DockTargetBase.ShowVerticalTargetsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ShowVerticalTargets<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.DockTargetBase 
   => control._set(Dock.Avalonia.Controls.DockTargetBase.ShowVerticalTargetsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T ShowVerticalTargets<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.DockTargetBase 
=> control._setEx(Dock.Avalonia.Controls.DockTargetBase.ShowVerticalTargetsProperty, ps, () => control.ShowVerticalTargets = converter.TryConvert(value)!, bindingMode, converter, bindingSource);



//================= Styles ======================//
 // ShowIndicatorsOnly

/*ValueStyleSetterGenerator*/
public static Style<T> ShowIndicatorsOnly<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.DockTargetBase 
=> style._addSetter(Dock.Avalonia.Controls.DockTargetBase.ShowIndicatorsOnlyProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> ShowIndicatorsOnly<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DockTargetBase 
=> style._addSetter(Dock.Avalonia.Controls.DockTargetBase.ShowIndicatorsOnlyProperty, binding);


 // ShowHorizontalTargets

/*ValueStyleSetterGenerator*/
public static Style<T> ShowHorizontalTargets<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.DockTargetBase 
=> style._addSetter(Dock.Avalonia.Controls.DockTargetBase.ShowHorizontalTargetsProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> ShowHorizontalTargets<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DockTargetBase 
=> style._addSetter(Dock.Avalonia.Controls.DockTargetBase.ShowHorizontalTargetsProperty, binding);


 // ShowVerticalTargets

/*ValueStyleSetterGenerator*/
public static Style<T> ShowVerticalTargets<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.DockTargetBase 
=> style._addSetter(Dock.Avalonia.Controls.DockTargetBase.ShowVerticalTargetsProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> ShowVerticalTargets<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DockTargetBase 
=> style._addSetter(Dock.Avalonia.Controls.DockTargetBase.ShowVerticalTargetsProperty, binding);



}
