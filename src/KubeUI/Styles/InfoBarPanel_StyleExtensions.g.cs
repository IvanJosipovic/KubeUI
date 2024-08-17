using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls.Primitives;
using InfoBarPanel = FluentAvalonia.UI.Controls.Primitives.InfoBarPanel;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class InfoBarPanelExtensions
{
public static Style<T> HorizontalOrientationPadding<T>(this Style<T> style, Avalonia.Thickness value) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
=> style._addSetter(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.HorizontalOrientationPaddingProperty, value);
public static Style<T> HorizontalOrientationPadding<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
=> style._addSetter(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.HorizontalOrientationPaddingProperty, binding);

public static Style<T> HorizontalOrientationPadding<T>(this Style<T> style, Double uniformLength) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
   => style._addSetter(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.HorizontalOrientationPaddingProperty, new Avalonia.Thickness(uniformLength));
public static Style<T> HorizontalOrientationPadding<T>(this Style<T> style, Double horizontal, Double vertical) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
   => style._addSetter(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.HorizontalOrientationPaddingProperty, new Avalonia.Thickness(horizontal, vertical));
public static Style<T> HorizontalOrientationPadding<T>(this Style<T> style, Double left, Double top, Double right, Double bottom) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
   => style._addSetter(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.HorizontalOrientationPaddingProperty, new Avalonia.Thickness(left, top, right, bottom));
public static Style<T> VerticalOrientationPadding<T>(this Style<T> style, Avalonia.Thickness value) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
=> style._addSetter(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.VerticalOrientationPaddingProperty, value);
public static Style<T> VerticalOrientationPadding<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
=> style._addSetter(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.VerticalOrientationPaddingProperty, binding);

public static Style<T> VerticalOrientationPadding<T>(this Style<T> style, Double uniformLength) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
   => style._addSetter(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.VerticalOrientationPaddingProperty, new Avalonia.Thickness(uniformLength));
public static Style<T> VerticalOrientationPadding<T>(this Style<T> style, Double horizontal, Double vertical) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
   => style._addSetter(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.VerticalOrientationPaddingProperty, new Avalonia.Thickness(horizontal, vertical));
public static Style<T> VerticalOrientationPadding<T>(this Style<T> style, Double left, Double top, Double right, Double bottom) where T : FluentAvalonia.UI.Controls.Primitives.InfoBarPanel
   => style._addSetter(FluentAvalonia.UI.Controls.Primitives.InfoBarPanel.VerticalOrientationPaddingProperty, new Avalonia.Thickness(left, top, right, bottom));
}

