using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;
using UrsaWindow = Ursa.Controls.UrsaWindow;

namespace Avalonia.Markup.Declarative;
public static partial class UrsaWindowExtensions
{
public static Style<T> IsFullScreenButtonVisible<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.UrsaWindow
=> style._addSetter(Ursa.Controls.UrsaWindow.IsFullScreenButtonVisibleProperty, value);
public static Style<T> IsFullScreenButtonVisible<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.UrsaWindow
=> style._addSetter(Ursa.Controls.UrsaWindow.IsFullScreenButtonVisibleProperty, binding);
public static Style<T> IsMinimizeButtonVisible<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.UrsaWindow
=> style._addSetter(Ursa.Controls.UrsaWindow.IsMinimizeButtonVisibleProperty, value);
public static Style<T> IsMinimizeButtonVisible<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.UrsaWindow
=> style._addSetter(Ursa.Controls.UrsaWindow.IsMinimizeButtonVisibleProperty, binding);
public static Style<T> IsRestoreButtonVisible<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.UrsaWindow
=> style._addSetter(Ursa.Controls.UrsaWindow.IsRestoreButtonVisibleProperty, value);
public static Style<T> IsRestoreButtonVisible<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.UrsaWindow
=> style._addSetter(Ursa.Controls.UrsaWindow.IsRestoreButtonVisibleProperty, binding);
public static Style<T> IsCloseButtonVisible<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.UrsaWindow
=> style._addSetter(Ursa.Controls.UrsaWindow.IsCloseButtonVisibleProperty, value);
public static Style<T> IsCloseButtonVisible<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.UrsaWindow
=> style._addSetter(Ursa.Controls.UrsaWindow.IsCloseButtonVisibleProperty, binding);
public static Style<T> IsTitleBarVisible<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.UrsaWindow
=> style._addSetter(Ursa.Controls.UrsaWindow.IsTitleBarVisibleProperty, value);
public static Style<T> IsTitleBarVisible<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.UrsaWindow
=> style._addSetter(Ursa.Controls.UrsaWindow.IsTitleBarVisibleProperty, binding);
public static Style<T> TitleBarContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.UrsaWindow
=> style._addSetter(Ursa.Controls.UrsaWindow.TitleBarContentProperty, value);
public static Style<T> TitleBarContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.UrsaWindow
=> style._addSetter(Ursa.Controls.UrsaWindow.TitleBarContentProperty, binding);
public static Style<T> LeftContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.UrsaWindow
=> style._addSetter(Ursa.Controls.UrsaWindow.LeftContentProperty, value);
public static Style<T> LeftContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.UrsaWindow
=> style._addSetter(Ursa.Controls.UrsaWindow.LeftContentProperty, binding);
public static Style<T> RightContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.UrsaWindow
=> style._addSetter(Ursa.Controls.UrsaWindow.RightContentProperty, value);
public static Style<T> RightContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.UrsaWindow
=> style._addSetter(Ursa.Controls.UrsaWindow.RightContentProperty, binding);
public static Style<T> TitleBarMargin<T>(this Style<T> style, Avalonia.Thickness value) where T : Ursa.Controls.UrsaWindow
=> style._addSetter(Ursa.Controls.UrsaWindow.TitleBarMarginProperty, value);
public static Style<T> TitleBarMargin<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.UrsaWindow
=> style._addSetter(Ursa.Controls.UrsaWindow.TitleBarMarginProperty, binding);

public static Style<T> TitleBarMargin<T>(this Style<T> style, Double uniformLength) where T : Ursa.Controls.UrsaWindow
   => style._addSetter(Ursa.Controls.UrsaWindow.TitleBarMarginProperty, new Avalonia.Thickness(uniformLength));
public static Style<T> TitleBarMargin<T>(this Style<T> style, Double horizontal, Double vertical) where T : Ursa.Controls.UrsaWindow
   => style._addSetter(Ursa.Controls.UrsaWindow.TitleBarMarginProperty, new Avalonia.Thickness(horizontal, vertical));
public static Style<T> TitleBarMargin<T>(this Style<T> style, Double left, Double top, Double right, Double bottom) where T : Ursa.Controls.UrsaWindow
   => style._addSetter(Ursa.Controls.UrsaWindow.TitleBarMarginProperty, new Avalonia.Thickness(left, top, right, bottom));
}

