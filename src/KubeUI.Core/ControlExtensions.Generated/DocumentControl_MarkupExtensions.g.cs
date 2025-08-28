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
 // HeaderTemplate

/*ValueSetterGenerator*/
public static T HeaderTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value) where T : Dock.Avalonia.Controls.DocumentControl 
=> control._set(() => control.HeaderTemplate = value!);

/*BindFromExpressionSetterGenerator*/
public static T HeaderTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : Dock.Avalonia.Controls.DocumentControl 
   => control._set(Dock.Avalonia.Controls.DocumentControl.HeaderTemplateProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T HeaderTemplate<T>(this T control,Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.DocumentControl 
=> control._setEx(Dock.Avalonia.Controls.DocumentControl.HeaderTemplateProperty, ps, () => control.HeaderTemplate = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T HeaderTemplate<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.DocumentControl 
   => control._set(Dock.Avalonia.Controls.DocumentControl.HeaderTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T HeaderTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.DocumentControl 
   => control._set(Dock.Avalonia.Controls.DocumentControl.HeaderTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T HeaderTemplate<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.DocumentControl 
=> control._setEx(Dock.Avalonia.Controls.DocumentControl.HeaderTemplateProperty, ps, () => control.HeaderTemplate = converter.TryConvert(value)!, bindingMode, converter, bindingSource);


 // IsActive

/*ValueSetterGenerator*/
public static T IsActive<T>(this T control, System.Boolean value) where T : Dock.Avalonia.Controls.DocumentControl 
=> control._set(() => control.IsActive = value!);

/*BindFromExpressionSetterGenerator*/
public static T IsActive<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : Dock.Avalonia.Controls.DocumentControl 
   => control._set(Dock.Avalonia.Controls.DocumentControl.IsActiveProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T IsActive<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.DocumentControl 
=> control._setEx(Dock.Avalonia.Controls.DocumentControl.IsActiveProperty, ps, () => control.IsActive = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsActive<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.DocumentControl 
   => control._set(Dock.Avalonia.Controls.DocumentControl.IsActiveProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsActive<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.DocumentControl 
   => control._set(Dock.Avalonia.Controls.DocumentControl.IsActiveProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T IsActive<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.DocumentControl 
=> control._setEx(Dock.Avalonia.Controls.DocumentControl.IsActiveProperty, ps, () => control.IsActive = converter.TryConvert(value)!, bindingMode, converter, bindingSource);


 // TabsLayout

/*ValueSetterGenerator*/
public static T TabsLayout<T>(this T control, Dock.Model.Core.DocumentTabLayout value) where T : Dock.Avalonia.Controls.DocumentControl 
=> control._set(() => control.TabsLayout = value!);

/*BindFromExpressionSetterGenerator*/
public static T TabsLayout<T>(this T control, Func<Dock.Model.Core.DocumentTabLayout> func, Action<Dock.Model.Core.DocumentTabLayout>? onChanged = null, [CallerArgumentExpression(nameof(func))] string? expression = null) where T : Dock.Avalonia.Controls.DocumentControl 
   => control._set(Dock.Avalonia.Controls.DocumentControl.TabsLayoutProperty!, func, onChanged, expression);

/*MagicalSetterGenerator*/
[Obsolete]
public static T TabsLayout<T>(this T control,Dock.Model.Core.DocumentTabLayout value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.DocumentControl 
=> control._setEx(Dock.Avalonia.Controls.DocumentControl.TabsLayoutProperty, ps, () => control.TabsLayout = value!, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TabsLayout<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.DocumentControl 
   => control._set(Dock.Avalonia.Controls.DocumentControl.TabsLayoutProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TabsLayout<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.DocumentControl 
   => control._set(Dock.Avalonia.Controls.DocumentControl.TabsLayoutProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
[Obsolete]
public static T TabsLayout<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Dock.Model.Core.DocumentTabLayout> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression(nameof(value))] string? ps = null) where T : Dock.Avalonia.Controls.DocumentControl 
=> control._setEx(Dock.Avalonia.Controls.DocumentControl.TabsLayoutProperty, ps, () => control.TabsLayout = converter.TryConvert(value)!, bindingMode, converter, bindingSource);



//================= Styles ======================//
 // HeaderTemplate

/*ValueStyleSetterGenerator*/
public static Style<T> HeaderTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : Dock.Avalonia.Controls.DocumentControl 
=> style._addSetter(Dock.Avalonia.Controls.DocumentControl.HeaderTemplateProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> HeaderTemplate<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DocumentControl 
=> style._addSetter(Dock.Avalonia.Controls.DocumentControl.HeaderTemplateProperty, binding);


 // IsActive

/*ValueStyleSetterGenerator*/
public static Style<T> IsActive<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.DocumentControl 
=> style._addSetter(Dock.Avalonia.Controls.DocumentControl.IsActiveProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> IsActive<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DocumentControl 
=> style._addSetter(Dock.Avalonia.Controls.DocumentControl.IsActiveProperty, binding);


 // TabsLayout

/*ValueStyleSetterGenerator*/
public static Style<T> TabsLayout<T>(this Style<T> style, Dock.Model.Core.DocumentTabLayout value) where T : Dock.Avalonia.Controls.DocumentControl 
=> style._addSetter(Dock.Avalonia.Controls.DocumentControl.TabsLayoutProperty!, value!);

/*BindingStyleSetterGenerator*/
public static Style<T> TabsLayout<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DocumentControl 
=> style._addSetter(Dock.Avalonia.Controls.DocumentControl.TabsLayoutProperty, binding);



}
