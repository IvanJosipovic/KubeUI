using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Divider = Ursa.Controls.Divider;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class DividerExtensions
{
public static Style<T> Orientation<T>(this Style<T> style, Avalonia.Layout.Orientation value) where T : Ursa.Controls.Divider
=> style._addSetter(Ursa.Controls.Divider.OrientationProperty, value);
public static Style<T> Orientation<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Divider
=> style._addSetter(Ursa.Controls.Divider.OrientationProperty, binding);
}

