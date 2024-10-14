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
public static partial class PinnedDockControl_MarkupExtensions
{
//================= Properties ======================//
 // PinnedDockAlignment

/*BindFromExpressionSetterGenerator*/
public static T PinnedDockAlignment<T>(this T control, Func<Dock.Model.Core.Alignment> func, Action<Dock.Model.Core.Alignment>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.PinnedDockControl 
   => control._set(Dock.Avalonia.Controls.PinnedDockControl.PinnedDockAlignmentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T PinnedDockAlignment<T>(this T control,Dock.Model.Core.Alignment value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.PinnedDockControl 
=> control._setEx(Dock.Avalonia.Controls.PinnedDockControl.PinnedDockAlignmentProperty, ps, () => control.PinnedDockAlignment = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T PinnedDockAlignment<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.PinnedDockControl 
   => control._set(Dock.Avalonia.Controls.PinnedDockControl.PinnedDockAlignmentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T PinnedDockAlignment<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.PinnedDockControl 
   => control._set(Dock.Avalonia.Controls.PinnedDockControl.PinnedDockAlignmentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T PinnedDockAlignment<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, Dock.Model.Core.Alignment> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.PinnedDockControl 
=> control._setEx(Dock.Avalonia.Controls.PinnedDockControl.PinnedDockAlignmentProperty, ps, () => control.PinnedDockAlignment = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Styles ======================//
 // PinnedDockAlignment

/*ValueStyleSetterGenerator*/
public static Style<T> PinnedDockAlignment<T>(this Style<T> style, Dock.Model.Core.Alignment value) where T : Dock.Avalonia.Controls.PinnedDockControl 
=> style._addSetter(Dock.Avalonia.Controls.PinnedDockControl.PinnedDockAlignmentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> PinnedDockAlignment<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.PinnedDockControl 
=> style._addSetter(Dock.Avalonia.Controls.PinnedDockControl.PinnedDockAlignmentProperty, binding);



}
