using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using ItemsRepeater = FluentAvalonia.UI.Controls.ItemsRepeater;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class ItemsRepeaterExtensions
{
public static Style<T> VerticalCacheLength<T>(this Style<T> style, System.Double value) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> style._addSetter(FluentAvalonia.UI.Controls.ItemsRepeater.VerticalCacheLengthProperty, value);
public static Style<T> VerticalCacheLength<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> style._addSetter(FluentAvalonia.UI.Controls.ItemsRepeater.VerticalCacheLengthProperty, binding);
public static Style<T> HorizontalCacheLength<T>(this Style<T> style, System.Double value) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> style._addSetter(FluentAvalonia.UI.Controls.ItemsRepeater.HorizontalCacheLengthProperty, value);
public static Style<T> HorizontalCacheLength<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> style._addSetter(FluentAvalonia.UI.Controls.ItemsRepeater.HorizontalCacheLengthProperty, binding);
public static Style<T> Layout<T>(this Style<T> style, FluentAvalonia.UI.Controls.Layout value) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> style._addSetter(FluentAvalonia.UI.Controls.ItemsRepeater.LayoutProperty, value);
public static Style<T> Layout<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> style._addSetter(FluentAvalonia.UI.Controls.ItemsRepeater.LayoutProperty, binding);
public static Style<T> ItemsSource<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> style._addSetter(FluentAvalonia.UI.Controls.ItemsRepeater.ItemsSourceProperty, value);
public static Style<T> ItemsSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> style._addSetter(FluentAvalonia.UI.Controls.ItemsRepeater.ItemsSourceProperty, binding);
public static Style<T> ItemTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> style._addSetter(FluentAvalonia.UI.Controls.ItemsRepeater.ItemTemplateProperty, value);
public static Style<T> ItemTemplate<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> style._addSetter(FluentAvalonia.UI.Controls.ItemsRepeater.ItemTemplateProperty, binding);
public static Style<T> ItemTransitionProvider<T>(this Style<T> style, FluentAvalonia.UI.Controls.ItemCollectionTransitionProvider value) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> style._addSetter(FluentAvalonia.UI.Controls.ItemsRepeater.ItemTransitionProviderProperty, value);
public static Style<T> ItemTransitionProvider<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ItemsRepeater
=> style._addSetter(FluentAvalonia.UI.Controls.ItemsRepeater.ItemTransitionProviderProperty, binding);
}

