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
public static Style<T> CanReorderItems<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.Primitives.TabViewListView
=> style._addSetter(FluentAvalonia.UI.Controls.Primitives.TabViewListView.CanReorderItemsProperty, value);
public static Style<T> CanReorderItems<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.Primitives.TabViewListView
=> style._addSetter(FluentAvalonia.UI.Controls.Primitives.TabViewListView.CanReorderItemsProperty, binding);
public static Style<T> CanDragItems<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.Primitives.TabViewListView
=> style._addSetter(FluentAvalonia.UI.Controls.Primitives.TabViewListView.CanDragItemsProperty, value);
public static Style<T> CanDragItems<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.Primitives.TabViewListView
=> style._addSetter(FluentAvalonia.UI.Controls.Primitives.TabViewListView.CanDragItemsProperty, binding);
}

