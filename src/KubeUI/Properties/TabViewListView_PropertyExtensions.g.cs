#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls.Primitives;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TabViewListView = FluentAvalonia.UI.Controls.Primitives.TabViewListView;

namespace Avalonia.Markup.Declarative;
public static partial class TabViewListViewExtensions
{
public static T CanReorderItems<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.Primitives.TabViewListView
   => control._set(FluentAvalonia.UI.Controls.Primitives.TabViewListView.CanReorderItemsProperty, binding);
public static T CanReorderItems<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.Primitives.TabViewListView
   => control._set(FluentAvalonia.UI.Controls.Primitives.TabViewListView.CanReorderItemsProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CanReorderItems<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.Primitives.TabViewListView
   => control._set(FluentAvalonia.UI.Controls.Primitives.TabViewListView.CanReorderItemsProperty, func, onChanged, expression);
public static T CanReorderItems<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Primitives.TabViewListView
=> control._setEx(FluentAvalonia.UI.Controls.Primitives.TabViewListView.CanReorderItemsProperty, ps, () => control.CanReorderItems = value, bindingMode, converter, bindingSource);
public static T CanReorderItems<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Primitives.TabViewListView
=> control._setEx(FluentAvalonia.UI.Controls.Primitives.TabViewListView.CanReorderItemsProperty, ps, () => control.CanReorderItems = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T CanDragItems<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.Primitives.TabViewListView
   => control._set(FluentAvalonia.UI.Controls.Primitives.TabViewListView.CanDragItemsProperty, binding);
public static T CanDragItems<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.Primitives.TabViewListView
   => control._set(FluentAvalonia.UI.Controls.Primitives.TabViewListView.CanDragItemsProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CanDragItems<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.Primitives.TabViewListView
   => control._set(FluentAvalonia.UI.Controls.Primitives.TabViewListView.CanDragItemsProperty, func, onChanged, expression);
public static T CanDragItems<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Primitives.TabViewListView
=> control._setEx(FluentAvalonia.UI.Controls.Primitives.TabViewListView.CanDragItemsProperty, ps, () => control.CanDragItems = value, bindingMode, converter, bindingSource);
public static T CanDragItems<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Primitives.TabViewListView
=> control._setEx(FluentAvalonia.UI.Controls.Primitives.TabViewListView.CanDragItemsProperty, ps, () => control.CanDragItems = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

