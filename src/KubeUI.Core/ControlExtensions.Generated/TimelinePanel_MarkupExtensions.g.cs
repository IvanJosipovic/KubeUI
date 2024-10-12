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
public static partial class TimelinePanel_MarkupExtensions
{
//================= Properties ======================//
 // ModeProperty

/*BindFromExpressionSetterGenerator*/
public static T Mode<T>(this T control, Func<Ursa.Controls.TimelineDisplayMode> func, Action<Ursa.Controls.TimelineDisplayMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimelinePanel
   => control._set(Ursa.Controls.TimelinePanel.ModeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Mode<T>(this T control, Ursa.Controls.TimelineDisplayMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelinePanel
=> control._setEx(Ursa.Controls.TimelinePanel.ModeProperty, ps, () => control.Mode = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Mode<T>(this T control, IBinding binding) where T : Ursa.Controls.TimelinePanel
   => control._set(Ursa.Controls.TimelinePanel.ModeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Mode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimelinePanel
   => control._set(Ursa.Controls.TimelinePanel.ModeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Mode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Ursa.Controls.TimelineDisplayMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelinePanel
=> control._setEx(Ursa.Controls.TimelinePanel.ModeProperty, ps, () => control.Mode = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // ModeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Mode<T>(this Style<T> style, Ursa.Controls.TimelineDisplayMode value) where T : Ursa.Controls.TimelinePanel
=> style._addSetter(Ursa.Controls.TimelinePanel.ModeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Mode<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimelinePanel
=> style._addSetter(Ursa.Controls.TimelinePanel.ModeProperty, binding);



}
