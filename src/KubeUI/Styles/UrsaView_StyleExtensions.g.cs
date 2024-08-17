using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;
using UrsaView = Ursa.Controls.UrsaView;

namespace Avalonia.Markup.Declarative;
public static partial class UrsaViewExtensions
{
public static Style<T> IsTitleBarVisible<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.UrsaView
=> style._addSetter(Ursa.Controls.UrsaView.IsTitleBarVisibleProperty, value);
public static Style<T> IsTitleBarVisible<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.UrsaView
=> style._addSetter(Ursa.Controls.UrsaView.IsTitleBarVisibleProperty, binding);
public static Style<T> LeftContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.UrsaView
=> style._addSetter(Ursa.Controls.UrsaView.LeftContentProperty, value);
public static Style<T> LeftContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.UrsaView
=> style._addSetter(Ursa.Controls.UrsaView.LeftContentProperty, binding);
public static Style<T> RightContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.UrsaView
=> style._addSetter(Ursa.Controls.UrsaView.RightContentProperty, value);
public static Style<T> RightContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.UrsaView
=> style._addSetter(Ursa.Controls.UrsaView.RightContentProperty, binding);
public static Style<T> TitleBarContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.UrsaView
=> style._addSetter(Ursa.Controls.UrsaView.TitleBarContentProperty, value);
public static Style<T> TitleBarContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.UrsaView
=> style._addSetter(Ursa.Controls.UrsaView.TitleBarContentProperty, binding);
public static Style<T> TitleBarMargin<T>(this Style<T> style, Avalonia.Thickness value) where T : Ursa.Controls.UrsaView
=> style._addSetter(Ursa.Controls.UrsaView.TitleBarMarginProperty, value);
public static Style<T> TitleBarMargin<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.UrsaView
=> style._addSetter(Ursa.Controls.UrsaView.TitleBarMarginProperty, binding);

public static Style<T> TitleBarMargin<T>(this Style<T> style, Double uniformLength) where T : Ursa.Controls.UrsaView
   => style._addSetter(Ursa.Controls.UrsaView.TitleBarMarginProperty, new Avalonia.Thickness(uniformLength));
public static Style<T> TitleBarMargin<T>(this Style<T> style, Double horizontal, Double vertical) where T : Ursa.Controls.UrsaView
   => style._addSetter(Ursa.Controls.UrsaView.TitleBarMarginProperty, new Avalonia.Thickness(horizontal, vertical));
public static Style<T> TitleBarMargin<T>(this Style<T> style, Double left, Double top, Double right, Double bottom) where T : Ursa.Controls.UrsaView
   => style._addSetter(Ursa.Controls.UrsaView.TitleBarMarginProperty, new Avalonia.Thickness(left, top, right, bottom));
}

