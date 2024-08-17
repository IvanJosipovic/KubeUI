using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TabViewItem = FluentAvalonia.UI.Controls.TabViewItem;

namespace Avalonia.Markup.Declarative;
public static partial class TabViewItemExtensions
{
public static Style<T> Header<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.TabViewItem
=> style._addSetter(FluentAvalonia.UI.Controls.TabViewItem.HeaderProperty, value);
public static Style<T> Header<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TabViewItem
=> style._addSetter(FluentAvalonia.UI.Controls.TabViewItem.HeaderProperty, binding);
public static Style<T> HeaderTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : FluentAvalonia.UI.Controls.TabViewItem
=> style._addSetter(FluentAvalonia.UI.Controls.TabViewItem.HeaderTemplateProperty, value);
public static Style<T> HeaderTemplate<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TabViewItem
=> style._addSetter(FluentAvalonia.UI.Controls.TabViewItem.HeaderTemplateProperty, binding);
public static Style<T> IconSource<T>(this Style<T> style, FluentAvalonia.UI.Controls.IconSource value) where T : FluentAvalonia.UI.Controls.TabViewItem
=> style._addSetter(FluentAvalonia.UI.Controls.TabViewItem.IconSourceProperty, value);
public static Style<T> IconSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TabViewItem
=> style._addSetter(FluentAvalonia.UI.Controls.TabViewItem.IconSourceProperty, binding);
public static Style<T> IsClosable<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.TabViewItem
=> style._addSetter(FluentAvalonia.UI.Controls.TabViewItem.IsClosableProperty, value);
public static Style<T> IsClosable<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TabViewItem
=> style._addSetter(FluentAvalonia.UI.Controls.TabViewItem.IsClosableProperty, binding);
}

