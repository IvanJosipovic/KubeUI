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
public static partial class DocumentControl_MarkupExtensions
{
//================= Properties ======================//
 // IconTemplate

/*BindFromExpressionSetterGenerator*/
public static T IconTemplate<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.DocumentControl 
   => control._set(Dock.Avalonia.Controls.DocumentControl.IconTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IconTemplate<T>(this T control,System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DocumentControl 
=> control._setEx(Dock.Avalonia.Controls.DocumentControl.IconTemplateProperty, ps, () => control.IconTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IconTemplate<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.DocumentControl 
   => control._set(Dock.Avalonia.Controls.DocumentControl.IconTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IconTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.DocumentControl 
   => control._set(Dock.Avalonia.Controls.DocumentControl.IconTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IconTemplate<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DocumentControl 
=> control._setEx(Dock.Avalonia.Controls.DocumentControl.IconTemplateProperty, ps, () => control.IconTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // HeaderTemplate

/*BindFromExpressionSetterGenerator*/
public static T HeaderTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.DocumentControl 
   => control._set(Dock.Avalonia.Controls.DocumentControl.HeaderTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T HeaderTemplate<T>(this T control,Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DocumentControl 
=> control._setEx(Dock.Avalonia.Controls.DocumentControl.HeaderTemplateProperty, ps, () => control.HeaderTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T HeaderTemplate<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.DocumentControl 
   => control._set(Dock.Avalonia.Controls.DocumentControl.HeaderTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T HeaderTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.DocumentControl 
   => control._set(Dock.Avalonia.Controls.DocumentControl.HeaderTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T HeaderTemplate<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DocumentControl 
=> control._setEx(Dock.Avalonia.Controls.DocumentControl.HeaderTemplateProperty, ps, () => control.HeaderTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ModifiedTemplate

/*BindFromExpressionSetterGenerator*/
public static T ModifiedTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.DocumentControl 
   => control._set(Dock.Avalonia.Controls.DocumentControl.ModifiedTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ModifiedTemplate<T>(this T control,Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DocumentControl 
=> control._setEx(Dock.Avalonia.Controls.DocumentControl.ModifiedTemplateProperty, ps, () => control.ModifiedTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ModifiedTemplate<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.DocumentControl 
   => control._set(Dock.Avalonia.Controls.DocumentControl.ModifiedTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ModifiedTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.DocumentControl 
   => control._set(Dock.Avalonia.Controls.DocumentControl.ModifiedTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ModifiedTemplate<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DocumentControl 
=> control._setEx(Dock.Avalonia.Controls.DocumentControl.ModifiedTemplateProperty, ps, () => control.ModifiedTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CloseTemplate

/*BindFromExpressionSetterGenerator*/
public static T CloseTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.DocumentControl 
   => control._set(Dock.Avalonia.Controls.DocumentControl.CloseTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CloseTemplate<T>(this T control,Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DocumentControl 
=> control._setEx(Dock.Avalonia.Controls.DocumentControl.CloseTemplateProperty, ps, () => control.CloseTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CloseTemplate<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.DocumentControl 
   => control._set(Dock.Avalonia.Controls.DocumentControl.CloseTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CloseTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.DocumentControl 
   => control._set(Dock.Avalonia.Controls.DocumentControl.CloseTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CloseTemplate<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DocumentControl 
=> control._setEx(Dock.Avalonia.Controls.DocumentControl.CloseTemplateProperty, ps, () => control.CloseTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CloseButtonTheme

/*BindFromExpressionSetterGenerator*/
public static T CloseButtonTheme<T>(this T control, Func<Avalonia.Styling.ControlTheme> func, Action<Avalonia.Styling.ControlTheme>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.DocumentControl 
   => control._set(Dock.Avalonia.Controls.DocumentControl.CloseButtonThemeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CloseButtonTheme<T>(this T control,Avalonia.Styling.ControlTheme value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DocumentControl 
=> control._setEx(Dock.Avalonia.Controls.DocumentControl.CloseButtonThemeProperty, ps, () => control.CloseButtonTheme = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CloseButtonTheme<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.DocumentControl 
   => control._set(Dock.Avalonia.Controls.DocumentControl.CloseButtonThemeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CloseButtonTheme<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.DocumentControl 
   => control._set(Dock.Avalonia.Controls.DocumentControl.CloseButtonThemeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CloseButtonTheme<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ControlTheme> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DocumentControl 
=> control._setEx(Dock.Avalonia.Controls.DocumentControl.CloseButtonThemeProperty, ps, () => control.CloseButtonTheme = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsActive

/*BindFromExpressionSetterGenerator*/
public static T IsActive<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.DocumentControl 
   => control._set(Dock.Avalonia.Controls.DocumentControl.IsActiveProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsActive<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DocumentControl 
=> control._setEx(Dock.Avalonia.Controls.DocumentControl.IsActiveProperty, ps, () => control.IsActive = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsActive<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.DocumentControl 
   => control._set(Dock.Avalonia.Controls.DocumentControl.IsActiveProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsActive<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.DocumentControl 
   => control._set(Dock.Avalonia.Controls.DocumentControl.IsActiveProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsActive<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DocumentControl 
=> control._setEx(Dock.Avalonia.Controls.DocumentControl.IsActiveProperty, ps, () => control.IsActive = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TabsLayout

/*BindFromExpressionSetterGenerator*/
public static T TabsLayout<T>(this T control, Func<Dock.Model.Core.DocumentTabLayout> func, Action<Dock.Model.Core.DocumentTabLayout>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.DocumentControl 
   => control._set(Dock.Avalonia.Controls.DocumentControl.TabsLayoutProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T TabsLayout<T>(this T control,Dock.Model.Core.DocumentTabLayout value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DocumentControl 
=> control._setEx(Dock.Avalonia.Controls.DocumentControl.TabsLayoutProperty, ps, () => control.TabsLayout = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TabsLayout<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.DocumentControl 
   => control._set(Dock.Avalonia.Controls.DocumentControl.TabsLayoutProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TabsLayout<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.DocumentControl 
   => control._set(Dock.Avalonia.Controls.DocumentControl.TabsLayoutProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T TabsLayout<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Dock.Model.Core.DocumentTabLayout> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DocumentControl 
=> control._setEx(Dock.Avalonia.Controls.DocumentControl.TabsLayoutProperty, ps, () => control.TabsLayout = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Styles ======================//
 // IconTemplate

/*ValueStyleSetterGenerator*/
public static Style<T> IconTemplate<T>(this Style<T> style, System.Object value) where T : Dock.Avalonia.Controls.DocumentControl 
=> style._addSetter(Dock.Avalonia.Controls.DocumentControl.IconTemplateProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IconTemplate<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DocumentControl 
=> style._addSetter(Dock.Avalonia.Controls.DocumentControl.IconTemplateProperty, binding);


 // HeaderTemplate

/*ValueStyleSetterGenerator*/
public static Style<T> HeaderTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : Dock.Avalonia.Controls.DocumentControl 
=> style._addSetter(Dock.Avalonia.Controls.DocumentControl.HeaderTemplateProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> HeaderTemplate<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DocumentControl 
=> style._addSetter(Dock.Avalonia.Controls.DocumentControl.HeaderTemplateProperty, binding);


 // ModifiedTemplate

/*ValueStyleSetterGenerator*/
public static Style<T> ModifiedTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : Dock.Avalonia.Controls.DocumentControl 
=> style._addSetter(Dock.Avalonia.Controls.DocumentControl.ModifiedTemplateProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ModifiedTemplate<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DocumentControl 
=> style._addSetter(Dock.Avalonia.Controls.DocumentControl.ModifiedTemplateProperty, binding);


 // CloseTemplate

/*ValueStyleSetterGenerator*/
public static Style<T> CloseTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : Dock.Avalonia.Controls.DocumentControl 
=> style._addSetter(Dock.Avalonia.Controls.DocumentControl.CloseTemplateProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CloseTemplate<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DocumentControl 
=> style._addSetter(Dock.Avalonia.Controls.DocumentControl.CloseTemplateProperty, binding);


 // CloseButtonTheme

/*ValueStyleSetterGenerator*/
public static Style<T> CloseButtonTheme<T>(this Style<T> style, Avalonia.Styling.ControlTheme value) where T : Dock.Avalonia.Controls.DocumentControl 
=> style._addSetter(Dock.Avalonia.Controls.DocumentControl.CloseButtonThemeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CloseButtonTheme<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DocumentControl 
=> style._addSetter(Dock.Avalonia.Controls.DocumentControl.CloseButtonThemeProperty, binding);


 // IsActive

/*ValueStyleSetterGenerator*/
public static Style<T> IsActive<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.DocumentControl 
=> style._addSetter(Dock.Avalonia.Controls.DocumentControl.IsActiveProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsActive<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DocumentControl 
=> style._addSetter(Dock.Avalonia.Controls.DocumentControl.IsActiveProperty, binding);


 // TabsLayout

/*ValueStyleSetterGenerator*/
public static Style<T> TabsLayout<T>(this Style<T> style, Dock.Model.Core.DocumentTabLayout value) where T : Dock.Avalonia.Controls.DocumentControl 
=> style._addSetter(Dock.Avalonia.Controls.DocumentControl.TabsLayoutProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> TabsLayout<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DocumentControl 
=> style._addSetter(Dock.Avalonia.Controls.DocumentControl.TabsLayoutProperty, binding);



}
