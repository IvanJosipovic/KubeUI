using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Controls.Primitives;
using NavigationViewItemPresenter = FluentAvalonia.UI.Controls.Primitives.NavigationViewItemPresenter;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class NavigationViewItemPresenterExtensions
{
public static Style<T> IconSource<T>(this Style<T> style, FluentAvalonia.UI.Controls.IconSource value) where T : FluentAvalonia.UI.Controls.Primitives.NavigationViewItemPresenter
=> style._addSetter(FluentAvalonia.UI.Controls.Primitives.NavigationViewItemPresenter.IconSourceProperty, value);
public static Style<T> IconSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.Primitives.NavigationViewItemPresenter
=> style._addSetter(FluentAvalonia.UI.Controls.Primitives.NavigationViewItemPresenter.IconSourceProperty, binding);
public static Style<T> InfoBadge<T>(this Style<T> style, FluentAvalonia.UI.Controls.InfoBadge value) where T : FluentAvalonia.UI.Controls.Primitives.NavigationViewItemPresenter
=> style._addSetter(FluentAvalonia.UI.Controls.Primitives.NavigationViewItemPresenter.InfoBadgeProperty, value);
public static Style<T> InfoBadge<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.Primitives.NavigationViewItemPresenter
=> style._addSetter(FluentAvalonia.UI.Controls.Primitives.NavigationViewItemPresenter.InfoBadgeProperty, binding);
}

