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
public static partial class DockControl_MarkupExtensions
{
//================= Properties ======================//
 // Layout

/*BindFromExpressionSetterGenerator*/
public static T Layout<T>(this T control, Func<Dock.Model.Core.IDock> func, Action<Dock.Model.Core.IDock>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.DockControl 
   => control._set(Dock.Avalonia.Controls.DockControl.LayoutProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Layout<T>(this T control,Dock.Model.Core.IDock value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DockControl 
=> control._setEx(Dock.Avalonia.Controls.DockControl.LayoutProperty, ps, () => control.Layout = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Layout<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.DockControl 
   => control._set(Dock.Avalonia.Controls.DockControl.LayoutProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Layout<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.DockControl 
   => control._set(Dock.Avalonia.Controls.DockControl.LayoutProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Layout<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Dock.Model.Core.IDock> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DockControl 
=> control._setEx(Dock.Avalonia.Controls.DockControl.LayoutProperty, ps, () => control.Layout = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // DefaultContext

/*BindFromExpressionSetterGenerator*/
public static T DefaultContext<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.DockControl 
   => control._set(Dock.Avalonia.Controls.DockControl.DefaultContextProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T DefaultContext<T>(this T control,System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DockControl 
=> control._setEx(Dock.Avalonia.Controls.DockControl.DefaultContextProperty, ps, () => control.DefaultContext = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T DefaultContext<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.DockControl 
   => control._set(Dock.Avalonia.Controls.DockControl.DefaultContextProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T DefaultContext<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.DockControl 
   => control._set(Dock.Avalonia.Controls.DockControl.DefaultContextProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T DefaultContext<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DockControl 
=> control._setEx(Dock.Avalonia.Controls.DockControl.DefaultContextProperty, ps, () => control.DefaultContext = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // InitializeLayout

/*BindFromExpressionSetterGenerator*/
public static T InitializeLayout<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.DockControl 
   => control._set(Dock.Avalonia.Controls.DockControl.InitializeLayoutProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T InitializeLayout<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DockControl 
=> control._setEx(Dock.Avalonia.Controls.DockControl.InitializeLayoutProperty, ps, () => control.InitializeLayout = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T InitializeLayout<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.DockControl 
   => control._set(Dock.Avalonia.Controls.DockControl.InitializeLayoutProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T InitializeLayout<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.DockControl 
   => control._set(Dock.Avalonia.Controls.DockControl.InitializeLayoutProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T InitializeLayout<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DockControl 
=> control._setEx(Dock.Avalonia.Controls.DockControl.InitializeLayoutProperty, ps, () => control.InitializeLayout = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // InitializeFactory

/*BindFromExpressionSetterGenerator*/
public static T InitializeFactory<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.DockControl 
   => control._set(Dock.Avalonia.Controls.DockControl.InitializeFactoryProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T InitializeFactory<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DockControl 
=> control._setEx(Dock.Avalonia.Controls.DockControl.InitializeFactoryProperty, ps, () => control.InitializeFactory = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T InitializeFactory<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.DockControl 
   => control._set(Dock.Avalonia.Controls.DockControl.InitializeFactoryProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T InitializeFactory<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.DockControl 
   => control._set(Dock.Avalonia.Controls.DockControl.InitializeFactoryProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T InitializeFactory<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DockControl 
=> control._setEx(Dock.Avalonia.Controls.DockControl.InitializeFactoryProperty, ps, () => control.InitializeFactory = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // Factory

/*BindFromExpressionSetterGenerator*/
public static T Factory<T>(this T control, Func<Dock.Model.Core.IFactory> func, Action<Dock.Model.Core.IFactory>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.DockControl 
   => control._set(Dock.Avalonia.Controls.DockControl.FactoryProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Factory<T>(this T control,Dock.Model.Core.IFactory value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DockControl 
=> control._setEx(Dock.Avalonia.Controls.DockControl.FactoryProperty, ps, () => control.Factory = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Factory<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.DockControl 
   => control._set(Dock.Avalonia.Controls.DockControl.FactoryProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Factory<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.DockControl 
   => control._set(Dock.Avalonia.Controls.DockControl.FactoryProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Factory<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Dock.Model.Core.IFactory> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DockControl 
=> control._setEx(Dock.Avalonia.Controls.DockControl.FactoryProperty, ps, () => control.Factory = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsDraggingDock

/*BindFromExpressionSetterGenerator*/
public static T IsDraggingDock<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.DockControl 
   => control._set(Dock.Avalonia.Controls.DockControl.IsDraggingDockProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsDraggingDock<T>(this T control,System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DockControl 
=> control._setEx(Dock.Avalonia.Controls.DockControl.IsDraggingDockProperty, ps, () => control.IsDraggingDock = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsDraggingDock<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.DockControl 
   => control._set(Dock.Avalonia.Controls.DockControl.IsDraggingDockProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsDraggingDock<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.DockControl 
   => control._set(Dock.Avalonia.Controls.DockControl.IsDraggingDockProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsDraggingDock<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DockControl 
=> control._setEx(Dock.Avalonia.Controls.DockControl.IsDraggingDockProperty, ps, () => control.IsDraggingDock = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Styles ======================//
 // Layout

/*ValueStyleSetterGenerator*/
public static Style<T> Layout<T>(this Style<T> style, Dock.Model.Core.IDock value) where T : Dock.Avalonia.Controls.DockControl 
=> style._addSetter(Dock.Avalonia.Controls.DockControl.LayoutProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Layout<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DockControl 
=> style._addSetter(Dock.Avalonia.Controls.DockControl.LayoutProperty, binding);


 // DefaultContext

/*ValueStyleSetterGenerator*/
public static Style<T> DefaultContext<T>(this Style<T> style, System.Object value) where T : Dock.Avalonia.Controls.DockControl 
=> style._addSetter(Dock.Avalonia.Controls.DockControl.DefaultContextProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> DefaultContext<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DockControl 
=> style._addSetter(Dock.Avalonia.Controls.DockControl.DefaultContextProperty, binding);


 // InitializeLayout

/*ValueStyleSetterGenerator*/
public static Style<T> InitializeLayout<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.DockControl 
=> style._addSetter(Dock.Avalonia.Controls.DockControl.InitializeLayoutProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> InitializeLayout<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DockControl 
=> style._addSetter(Dock.Avalonia.Controls.DockControl.InitializeLayoutProperty, binding);


 // InitializeFactory

/*ValueStyleSetterGenerator*/
public static Style<T> InitializeFactory<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.DockControl 
=> style._addSetter(Dock.Avalonia.Controls.DockControl.InitializeFactoryProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> InitializeFactory<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DockControl 
=> style._addSetter(Dock.Avalonia.Controls.DockControl.InitializeFactoryProperty, binding);


 // Factory

/*ValueStyleSetterGenerator*/
public static Style<T> Factory<T>(this Style<T> style, Dock.Model.Core.IFactory value) where T : Dock.Avalonia.Controls.DockControl 
=> style._addSetter(Dock.Avalonia.Controls.DockControl.FactoryProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Factory<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DockControl 
=> style._addSetter(Dock.Avalonia.Controls.DockControl.FactoryProperty, binding);


 // IsDraggingDock

/*ValueStyleSetterGenerator*/
public static Style<T> IsDraggingDock<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.DockControl 
=> style._addSetter(Dock.Avalonia.Controls.DockControl.IsDraggingDockProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsDraggingDock<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DockControl 
=> style._addSetter(Dock.Avalonia.Controls.DockControl.IsDraggingDockProperty, binding);



}
