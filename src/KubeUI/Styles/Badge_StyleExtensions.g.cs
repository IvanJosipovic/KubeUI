using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Styling;
using Badge = Ursa.Controls.Badge;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Common;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class BadgeExtensions
{
public static Style<T> BadgeTheme<T>(this Style<T> style, Avalonia.Styling.ControlTheme value) where T : Ursa.Controls.Badge
=> style._addSetter(Ursa.Controls.Badge.BadgeThemeProperty, value);
public static Style<T> BadgeTheme<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Badge
=> style._addSetter(Ursa.Controls.Badge.BadgeThemeProperty, binding);
public static Style<T> Dot<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.Badge
=> style._addSetter(Ursa.Controls.Badge.DotProperty, value);
public static Style<T> Dot<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Badge
=> style._addSetter(Ursa.Controls.Badge.DotProperty, binding);
public static Style<T> CornerPosition<T>(this Style<T> style, Ursa.Common.CornerPosition value) where T : Ursa.Controls.Badge
=> style._addSetter(Ursa.Controls.Badge.CornerPositionProperty, value);
public static Style<T> CornerPosition<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Badge
=> style._addSetter(Ursa.Controls.Badge.CornerPositionProperty, binding);
public static Style<T> OverflowCount<T>(this Style<T> style, System.Int32 value) where T : Ursa.Controls.Badge
=> style._addSetter(Ursa.Controls.Badge.OverflowCountProperty, value);
public static Style<T> OverflowCount<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Badge
=> style._addSetter(Ursa.Controls.Badge.OverflowCountProperty, binding);
public static Style<T> BadgeFontSize<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.Badge
=> style._addSetter(Ursa.Controls.Badge.BadgeFontSizeProperty, value);
public static Style<T> BadgeFontSize<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Badge
=> style._addSetter(Ursa.Controls.Badge.BadgeFontSizeProperty, binding);
}

