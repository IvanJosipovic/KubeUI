using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TitleBar = Ursa.Controls.TitleBar;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class TitleBarExtensions
{
public static Style<T> LeftContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.TitleBar
=> style._addSetter(Ursa.Controls.TitleBar.LeftContentProperty, value);
public static Style<T> LeftContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TitleBar
=> style._addSetter(Ursa.Controls.TitleBar.LeftContentProperty, binding);
public static Style<T> RightContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.TitleBar
=> style._addSetter(Ursa.Controls.TitleBar.RightContentProperty, value);
public static Style<T> RightContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TitleBar
=> style._addSetter(Ursa.Controls.TitleBar.RightContentProperty, binding);
public static Style<T> IsTitleVisible<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.TitleBar
=> style._addSetter(Ursa.Controls.TitleBar.IsTitleVisibleProperty, value);
public static Style<T> IsTitleVisible<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TitleBar
=> style._addSetter(Ursa.Controls.TitleBar.IsTitleVisibleProperty, binding);
}

