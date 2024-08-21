#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class CommandBar_MarkupExtensions
{
//================= Properties ======================//
 // IsStickyProperty

/*BindFromExpressionSetterGenerator*/
public static T IsSticky<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.IsStickyProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsSticky<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBar
=> control._setEx(FluentAvalonia.UI.Controls.CommandBar.IsStickyProperty, ps, () => control.IsSticky = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsSticky<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.IsStickyProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsSticky<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.IsStickyProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsSticky<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBar
=> control._setEx(FluentAvalonia.UI.Controls.CommandBar.IsStickyProperty, ps, () => control.IsSticky = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsOpenProperty

/*BindFromExpressionSetterGenerator*/
public static T IsOpen<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.IsOpenProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsOpen<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBar
=> control._setEx(FluentAvalonia.UI.Controls.CommandBar.IsOpenProperty, ps, () => control.IsOpen = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsOpen<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.IsOpenProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsOpen<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.IsOpenProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsOpen<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBar
=> control._setEx(FluentAvalonia.UI.Controls.CommandBar.IsOpenProperty, ps, () => control.IsOpen = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ClosedDisplayModeProperty

/*BindFromExpressionSetterGenerator*/
public static T ClosedDisplayMode<T>(this T control, Func<FluentAvalonia.UI.Controls.CommandBarClosedDisplayMode> func, Action<FluentAvalonia.UI.Controls.CommandBarClosedDisplayMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.ClosedDisplayModeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ClosedDisplayMode<T>(this T control, FluentAvalonia.UI.Controls.CommandBarClosedDisplayMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBar
=> control._setEx(FluentAvalonia.UI.Controls.CommandBar.ClosedDisplayModeProperty, ps, () => control.ClosedDisplayMode = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ClosedDisplayMode<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.ClosedDisplayModeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ClosedDisplayMode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.ClosedDisplayModeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ClosedDisplayMode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.CommandBarClosedDisplayMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBar
=> control._setEx(FluentAvalonia.UI.Controls.CommandBar.ClosedDisplayModeProperty, ps, () => control.ClosedDisplayMode = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // OverflowButtonVisibilityProperty

/*BindFromExpressionSetterGenerator*/
public static T OverflowButtonVisibility<T>(this T control, Func<FluentAvalonia.UI.Controls.CommandBarOverflowButtonVisibility> func, Action<FluentAvalonia.UI.Controls.CommandBarOverflowButtonVisibility>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.OverflowButtonVisibilityProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T OverflowButtonVisibility<T>(this T control, FluentAvalonia.UI.Controls.CommandBarOverflowButtonVisibility value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBar
=> control._setEx(FluentAvalonia.UI.Controls.CommandBar.OverflowButtonVisibilityProperty, ps, () => control.OverflowButtonVisibility = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T OverflowButtonVisibility<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.OverflowButtonVisibilityProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T OverflowButtonVisibility<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.OverflowButtonVisibilityProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T OverflowButtonVisibility<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.CommandBarOverflowButtonVisibility> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBar
=> control._setEx(FluentAvalonia.UI.Controls.CommandBar.OverflowButtonVisibilityProperty, ps, () => control.OverflowButtonVisibility = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsDynamicOverflowEnabledProperty

/*BindFromExpressionSetterGenerator*/
public static T IsDynamicOverflowEnabled<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.IsDynamicOverflowEnabledProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsDynamicOverflowEnabled<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBar
=> control._setEx(FluentAvalonia.UI.Controls.CommandBar.IsDynamicOverflowEnabledProperty, ps, () => control.IsDynamicOverflowEnabled = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsDynamicOverflowEnabled<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.IsDynamicOverflowEnabledProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsDynamicOverflowEnabled<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.IsDynamicOverflowEnabledProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsDynamicOverflowEnabled<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBar
=> control._setEx(FluentAvalonia.UI.Controls.CommandBar.IsDynamicOverflowEnabledProperty, ps, () => control.IsDynamicOverflowEnabled = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ItemsAlignmentProperty

/*BindFromExpressionSetterGenerator*/
public static T ItemsAlignment<T>(this T control, Func<FluentAvalonia.UI.Controls.CommandBarItemsAlignment> func, Action<FluentAvalonia.UI.Controls.CommandBarItemsAlignment>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.ItemsAlignmentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ItemsAlignment<T>(this T control, FluentAvalonia.UI.Controls.CommandBarItemsAlignment value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBar
=> control._setEx(FluentAvalonia.UI.Controls.CommandBar.ItemsAlignmentProperty, ps, () => control.ItemsAlignment = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ItemsAlignment<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.ItemsAlignmentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ItemsAlignment<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.ItemsAlignmentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ItemsAlignment<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.CommandBarItemsAlignment> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBar
=> control._setEx(FluentAvalonia.UI.Controls.CommandBar.ItemsAlignmentProperty, ps, () => control.ItemsAlignment = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // DefaultLabelPositionProperty

/*BindFromExpressionSetterGenerator*/
public static T DefaultLabelPosition<T>(this T control, Func<FluentAvalonia.UI.Controls.CommandBarDefaultLabelPosition> func, Action<FluentAvalonia.UI.Controls.CommandBarDefaultLabelPosition>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.DefaultLabelPositionProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T DefaultLabelPosition<T>(this T control, FluentAvalonia.UI.Controls.CommandBarDefaultLabelPosition value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBar
=> control._setEx(FluentAvalonia.UI.Controls.CommandBar.DefaultLabelPositionProperty, ps, () => control.DefaultLabelPosition = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T DefaultLabelPosition<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.DefaultLabelPositionProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T DefaultLabelPosition<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.CommandBar
   => control._set(FluentAvalonia.UI.Controls.CommandBar.DefaultLabelPositionProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T DefaultLabelPosition<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.CommandBarDefaultLabelPosition> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.CommandBar
=> control._setEx(FluentAvalonia.UI.Controls.CommandBar.DefaultLabelPositionProperty, ps, () => control.DefaultLabelPosition = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // Opened

/*ActionToEventGenerator*/
    public static T OnOpened<T>(this T control, Action<FluentAvalonia.UI.Controls.CommandBar, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.CommandBar => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.CommandBar,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Opened += h);


 // Opening

/*ActionToEventGenerator*/
    public static T OnOpening<T>(this T control, Action<FluentAvalonia.UI.Controls.CommandBar, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.CommandBar => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.CommandBar,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Opening += h);


 // Closed

/*ActionToEventGenerator*/
    public static T OnClosed<T>(this T control, Action<FluentAvalonia.UI.Controls.CommandBar, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.CommandBar => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.CommandBar,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Closed += h);


 // Closing

/*ActionToEventGenerator*/
    public static T OnClosing<T>(this T control, Action<FluentAvalonia.UI.Controls.CommandBar, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.CommandBar => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.CommandBar,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Closing += h);



//================= Styles ======================//
 // IsStickyProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsSticky<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.CommandBar
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBar.IsStickyProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsSticky<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBar
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBar.IsStickyProperty, binding);


 // IsOpenProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsOpen<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.CommandBar
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBar.IsOpenProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsOpen<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBar
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBar.IsOpenProperty, binding);


 // ClosedDisplayModeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ClosedDisplayMode<T>(this Style<T> style, FluentAvalonia.UI.Controls.CommandBarClosedDisplayMode value) where T : FluentAvalonia.UI.Controls.CommandBar
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBar.ClosedDisplayModeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ClosedDisplayMode<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBar
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBar.ClosedDisplayModeProperty, binding);


 // OverflowButtonVisibilityProperty

/*ValueStyleSetterGenerator*/
public static Style<T> OverflowButtonVisibility<T>(this Style<T> style, FluentAvalonia.UI.Controls.CommandBarOverflowButtonVisibility value) where T : FluentAvalonia.UI.Controls.CommandBar
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBar.OverflowButtonVisibilityProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> OverflowButtonVisibility<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBar
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBar.OverflowButtonVisibilityProperty, binding);


 // IsDynamicOverflowEnabledProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsDynamicOverflowEnabled<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.CommandBar
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBar.IsDynamicOverflowEnabledProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsDynamicOverflowEnabled<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBar
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBar.IsDynamicOverflowEnabledProperty, binding);


 // ItemsAlignmentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ItemsAlignment<T>(this Style<T> style, FluentAvalonia.UI.Controls.CommandBarItemsAlignment value) where T : FluentAvalonia.UI.Controls.CommandBar
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBar.ItemsAlignmentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ItemsAlignment<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBar
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBar.ItemsAlignmentProperty, binding);


 // DefaultLabelPositionProperty

/*ValueStyleSetterGenerator*/
public static Style<T> DefaultLabelPosition<T>(this Style<T> style, FluentAvalonia.UI.Controls.CommandBarDefaultLabelPosition value) where T : FluentAvalonia.UI.Controls.CommandBar
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBar.DefaultLabelPositionProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> DefaultLabelPosition<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBar
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBar.DefaultLabelPositionProperty, binding);



}
