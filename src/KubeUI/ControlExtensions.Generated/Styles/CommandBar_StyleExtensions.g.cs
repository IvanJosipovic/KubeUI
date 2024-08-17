using Avalonia.Data;
using Avalonia.Data.Converters;
using CommandBar = FluentAvalonia.UI.Controls.CommandBar;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class CommandBarExtensions
{
public static Style<T> IsSticky<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.CommandBar
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBar.IsStickyProperty, value);
public static Style<T> IsSticky<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBar
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBar.IsStickyProperty, binding);
public static Style<T> IsOpen<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.CommandBar
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBar.IsOpenProperty, value);
public static Style<T> IsOpen<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBar
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBar.IsOpenProperty, binding);
public static Style<T> ClosedDisplayMode<T>(this Style<T> style, FluentAvalonia.UI.Controls.CommandBarClosedDisplayMode value) where T : FluentAvalonia.UI.Controls.CommandBar
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBar.ClosedDisplayModeProperty, value);
public static Style<T> ClosedDisplayMode<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBar
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBar.ClosedDisplayModeProperty, binding);
public static Style<T> OverflowButtonVisibility<T>(this Style<T> style, FluentAvalonia.UI.Controls.CommandBarOverflowButtonVisibility value) where T : FluentAvalonia.UI.Controls.CommandBar
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBar.OverflowButtonVisibilityProperty, value);
public static Style<T> OverflowButtonVisibility<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBar
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBar.OverflowButtonVisibilityProperty, binding);
public static Style<T> IsDynamicOverflowEnabled<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.CommandBar
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBar.IsDynamicOverflowEnabledProperty, value);
public static Style<T> IsDynamicOverflowEnabled<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBar
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBar.IsDynamicOverflowEnabledProperty, binding);
public static Style<T> ItemsAlignment<T>(this Style<T> style, FluentAvalonia.UI.Controls.CommandBarItemsAlignment value) where T : FluentAvalonia.UI.Controls.CommandBar
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBar.ItemsAlignmentProperty, value);
public static Style<T> ItemsAlignment<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBar
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBar.ItemsAlignmentProperty, binding);
public static Style<T> DefaultLabelPosition<T>(this Style<T> style, FluentAvalonia.UI.Controls.CommandBarDefaultLabelPosition value) where T : FluentAvalonia.UI.Controls.CommandBar
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBar.DefaultLabelPositionProperty, value);
public static Style<T> DefaultLabelPosition<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBar
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBar.DefaultLabelPositionProperty, binding);
}

