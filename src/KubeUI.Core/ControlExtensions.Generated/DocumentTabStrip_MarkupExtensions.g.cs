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
public static partial class DocumentTabStrip_MarkupExtensions
{
//================= Properties ======================//
 // CanCreateItem

/*BindFromExpressionSetterGenerator*/
public static T CanCreateItem<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.DocumentTabStrip 
   => control._set(Dock.Avalonia.Controls.DocumentTabStrip.CanCreateItemProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CanCreateItem<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DocumentTabStrip 
=> control._setEx(Dock.Avalonia.Controls.DocumentTabStrip.CanCreateItemProperty, ps, () => control.CanCreateItem = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CanCreateItem<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.DocumentTabStrip 
   => control._set(Dock.Avalonia.Controls.DocumentTabStrip.CanCreateItemProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CanCreateItem<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.DocumentTabStrip 
   => control._set(Dock.Avalonia.Controls.DocumentTabStrip.CanCreateItemProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CanCreateItem<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DocumentTabStrip 
=> control._setEx(Dock.Avalonia.Controls.DocumentTabStrip.CanCreateItemProperty, ps, () => control.CanCreateItem = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsActive

/*BindFromExpressionSetterGenerator*/
public static T IsActive<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.DocumentTabStrip 
   => control._set(Dock.Avalonia.Controls.DocumentTabStrip.IsActiveProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsActive<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DocumentTabStrip 
=> control._setEx(Dock.Avalonia.Controls.DocumentTabStrip.IsActiveProperty, ps, () => control.IsActive = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsActive<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.DocumentTabStrip 
   => control._set(Dock.Avalonia.Controls.DocumentTabStrip.IsActiveProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsActive<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.DocumentTabStrip 
   => control._set(Dock.Avalonia.Controls.DocumentTabStrip.IsActiveProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsActive<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DocumentTabStrip 
=> control._setEx(Dock.Avalonia.Controls.DocumentTabStrip.IsActiveProperty, ps, () => control.IsActive = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // EnableWindowDrag

/*ValueSetterGenerator*/
public static T EnableWindowDrag<T>(this T control, System.Boolean value) where T : Dock.Avalonia.Controls.DocumentTabStrip 
=> control._set(() => control.EnableWindowDrag = value!);

/*BindFromExpressionSetterGenerator*/
public static T EnableWindowDrag<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : Dock.Avalonia.Controls.DocumentTabStrip 
   => control._set(Dock.Avalonia.Controls.DocumentTabStrip.EnableWindowDragProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T EnableWindowDrag<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.DocumentTabStrip 
=> control._setEx(Dock.Avalonia.Controls.DocumentTabStrip.EnableWindowDragProperty, ps, () => control.EnableWindowDrag = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T EnableWindowDrag<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.DocumentTabStrip 
   => control._set(Dock.Avalonia.Controls.DocumentTabStrip.EnableWindowDragProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T EnableWindowDrag<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.DocumentTabStrip 
   => control._set(Dock.Avalonia.Controls.DocumentTabStrip.EnableWindowDragProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T EnableWindowDrag<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.DocumentTabStrip 
=> control._setEx(Dock.Avalonia.Controls.DocumentTabStrip.EnableWindowDragProperty, ps, () => control.EnableWindowDrag = converter.TryConvert(value)!, bindingMode, converter, bindingSource);


 // Orientation

/*ValueSetterGenerator*/
public static T Orientation<T>(this T control, Avalonia.Layout.Orientation value) where T : Dock.Avalonia.Controls.DocumentTabStrip 
=> control._set(() => control.Orientation = value!);

/*BindFromExpressionSetterGenerator*/
public static T Orientation<T>(this T control, Func<Avalonia.Layout.Orientation> func, Action<Avalonia.Layout.Orientation>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : Dock.Avalonia.Controls.DocumentTabStrip 
   => control._set(Dock.Avalonia.Controls.DocumentTabStrip.OrientationProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T Orientation<T>(this T control,Avalonia.Layout.Orientation value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.DocumentTabStrip 
=> control._setEx(Dock.Avalonia.Controls.DocumentTabStrip.OrientationProperty, ps, () => control.Orientation = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Orientation<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.DocumentTabStrip 
   => control._set(Dock.Avalonia.Controls.DocumentTabStrip.OrientationProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Orientation<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.DocumentTabStrip 
   => control._set(Dock.Avalonia.Controls.DocumentTabStrip.OrientationProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T Orientation<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Layout.Orientation> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.DocumentTabStrip 
=> control._setEx(Dock.Avalonia.Controls.DocumentTabStrip.OrientationProperty, ps, () => control.Orientation = converter.TryConvert(value)!, bindingMode, converter, bindingSource);


 // CreateButtonTheme

/*ValueSetterGenerator*/
public static T CreateButtonTheme<T>(this T control, Avalonia.Styling.ControlTheme value) where T : Dock.Avalonia.Controls.DocumentTabStrip 
=> control._set(() => control.CreateButtonTheme = value!);

/*BindFromExpressionSetterGenerator*/
public static T CreateButtonTheme<T>(this T control, Func<Avalonia.Styling.ControlTheme> func, Action<Avalonia.Styling.ControlTheme>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : Dock.Avalonia.Controls.DocumentTabStrip 
   => control._set(Dock.Avalonia.Controls.DocumentTabStrip.CreateButtonThemeProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T CreateButtonTheme<T>(this T control,Avalonia.Styling.ControlTheme value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.DocumentTabStrip 
=> control._setEx(Dock.Avalonia.Controls.DocumentTabStrip.CreateButtonThemeProperty, ps, () => control.CreateButtonTheme = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CreateButtonTheme<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.DocumentTabStrip 
   => control._set(Dock.Avalonia.Controls.DocumentTabStrip.CreateButtonThemeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CreateButtonTheme<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.DocumentTabStrip 
   => control._set(Dock.Avalonia.Controls.DocumentTabStrip.CreateButtonThemeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T CreateButtonTheme<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ControlTheme> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.DocumentTabStrip 
=> control._setEx(Dock.Avalonia.Controls.DocumentTabStrip.CreateButtonThemeProperty, ps, () => control.CreateButtonTheme = converter.TryConvert(value)!, bindingMode, converter, bindingSource);



//================= Styles ======================//
 // CanCreateItem

/*ValueStyleSetterGenerator*/
public static Style<T> CanCreateItem<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.DocumentTabStrip 
=> style._addSetter(Dock.Avalonia.Controls.DocumentTabStrip.CanCreateItemProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CanCreateItem<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DocumentTabStrip 
=> style._addSetter(Dock.Avalonia.Controls.DocumentTabStrip.CanCreateItemProperty, binding);


 // IsActive

/*ValueStyleSetterGenerator*/
public static Style<T> IsActive<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.DocumentTabStrip 
=> style._addSetter(Dock.Avalonia.Controls.DocumentTabStrip.IsActiveProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsActive<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DocumentTabStrip 
=> style._addSetter(Dock.Avalonia.Controls.DocumentTabStrip.IsActiveProperty, binding);


 // EnableWindowDrag

/*ValueStyleSetterGenerator*/
public static Style<T> EnableWindowDrag<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.DocumentTabStrip 
=> style._addSetter(Dock.Avalonia.Controls.DocumentTabStrip.EnableWindowDragProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> EnableWindowDrag<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DocumentTabStrip 
=> style._addSetter(Dock.Avalonia.Controls.DocumentTabStrip.EnableWindowDragProperty, binding);


 // Orientation

/*ValueStyleSetterGenerator*/
public static Style<T> Orientation<T>(this Style<T> style, Avalonia.Layout.Orientation value) where T : Dock.Avalonia.Controls.DocumentTabStrip 
=> style._addSetter(Dock.Avalonia.Controls.DocumentTabStrip.OrientationProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> Orientation<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DocumentTabStrip 
=> style._addSetter(Dock.Avalonia.Controls.DocumentTabStrip.OrientationProperty, binding);


 // CreateButtonTheme

/*ValueStyleSetterGenerator*/
public static Style<T> CreateButtonTheme<T>(this Style<T> style, Avalonia.Styling.ControlTheme value) where T : Dock.Avalonia.Controls.DocumentTabStrip 
=> style._addSetter(Dock.Avalonia.Controls.DocumentTabStrip.CreateButtonThemeProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> CreateButtonTheme<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DocumentTabStrip 
=> style._addSetter(Dock.Avalonia.Controls.DocumentTabStrip.CreateButtonThemeProperty, binding);



}
