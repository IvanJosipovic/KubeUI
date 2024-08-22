#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class TabViewListView_MarkupExtensions
{
//================= Properties ======================//
 // CanReorderItemsProperty

/*BindFromExpressionSetterGenerator*/
public static T CanReorderItems<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.Primitives.TabViewListView
   => control._set(FluentAvalonia.UI.Controls.Primitives.TabViewListView.CanReorderItemsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CanReorderItems<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Primitives.TabViewListView
=> control._setEx(FluentAvalonia.UI.Controls.Primitives.TabViewListView.CanReorderItemsProperty, ps, () => control.CanReorderItems = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CanReorderItems<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.Primitives.TabViewListView
   => control._set(FluentAvalonia.UI.Controls.Primitives.TabViewListView.CanReorderItemsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CanReorderItems<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.Primitives.TabViewListView
   => control._set(FluentAvalonia.UI.Controls.Primitives.TabViewListView.CanReorderItemsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CanReorderItems<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Primitives.TabViewListView
=> control._setEx(FluentAvalonia.UI.Controls.Primitives.TabViewListView.CanReorderItemsProperty, ps, () => control.CanReorderItems = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CanDragItemsProperty

/*BindFromExpressionSetterGenerator*/
public static T CanDragItems<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.Primitives.TabViewListView
   => control._set(FluentAvalonia.UI.Controls.Primitives.TabViewListView.CanDragItemsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CanDragItems<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Primitives.TabViewListView
=> control._setEx(FluentAvalonia.UI.Controls.Primitives.TabViewListView.CanDragItemsProperty, ps, () => control.CanDragItems = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CanDragItems<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.Primitives.TabViewListView
   => control._set(FluentAvalonia.UI.Controls.Primitives.TabViewListView.CanDragItemsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CanDragItems<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.Primitives.TabViewListView
   => control._set(FluentAvalonia.UI.Controls.Primitives.TabViewListView.CanDragItemsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CanDragItems<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Primitives.TabViewListView
=> control._setEx(FluentAvalonia.UI.Controls.Primitives.TabViewListView.CanDragItemsProperty, ps, () => control.CanDragItems = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // DragOver

/*ActionToEventGenerator*/
    public static T OnDragOver<T>(this T control, Action<Avalonia.Input.DragEventArgs> action) where T : FluentAvalonia.UI.Controls.Primitives.TabViewListView => 
        control._setEvent((System.EventHandler<Avalonia.Input.DragEventArgs>) ((arg0, arg1) => action(arg1)), h => control.DragOver += h);


 // Drop

/*ActionToEventGenerator*/
    public static T OnDrop<T>(this T control, Action<Avalonia.Input.DragEventArgs> action) where T : FluentAvalonia.UI.Controls.Primitives.TabViewListView => 
        control._setEvent((System.EventHandler<Avalonia.Input.DragEventArgs>) ((arg0, arg1) => action(arg1)), h => control.Drop += h);


 // DragItemsStarting

/*ActionToEventGenerator*/
    public static T OnDragItemsStarting<T>(this T control, Action<DragItemsStartingEventArgs> action) where T : FluentAvalonia.UI.Controls.Primitives.TabViewListView => 
        control._setEvent((DragItemsStartingEventHandler) ((arg0, arg1) => action(arg1)), h => control.DragItemsStarting += h);


 // DragItemsCompleted

/*ActionToEventGenerator*/
    public static T OnDragItemsCompleted<T>(this T control, Action<FluentAvalonia.UI.Controls.Primitives.TabViewListView, DragItemsCompletedEventArgs> action) where T : FluentAvalonia.UI.Controls.Primitives.TabViewListView => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.Primitives.TabViewListView,DragItemsCompletedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.DragItemsCompleted += h);



//================= Styles ======================//
 // CanReorderItemsProperty

/*ValueStyleSetterGenerator*/
public static Style<T> CanReorderItems<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.Primitives.TabViewListView
=> style._addSetter(FluentAvalonia.UI.Controls.Primitives.TabViewListView.CanReorderItemsProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CanReorderItems<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.Primitives.TabViewListView
=> style._addSetter(FluentAvalonia.UI.Controls.Primitives.TabViewListView.CanReorderItemsProperty, binding);


 // CanDragItemsProperty

/*ValueStyleSetterGenerator*/
public static Style<T> CanDragItems<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.Primitives.TabViewListView
=> style._addSetter(FluentAvalonia.UI.Controls.Primitives.TabViewListView.CanDragItemsProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CanDragItems<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.Primitives.TabViewListView
=> style._addSetter(FluentAvalonia.UI.Controls.Primitives.TabViewListView.CanDragItemsProperty, binding);



}
