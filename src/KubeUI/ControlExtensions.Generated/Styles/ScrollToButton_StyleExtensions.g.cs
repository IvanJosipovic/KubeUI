using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using ScrollToButton = Ursa.Controls.ScrollToButton;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Common;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class ScrollToButtonExtensions
{
public static Style<T> Target<T>(this Style<T> style, Avalonia.Controls.Control value) where T : Ursa.Controls.ScrollToButton
=> style._addSetter(Ursa.Controls.ScrollToButton.TargetProperty, value);
public static Style<T> Target<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ScrollToButton
=> style._addSetter(Ursa.Controls.ScrollToButton.TargetProperty, binding);
public static Style<T> Direction<T>(this Style<T> style, Ursa.Common.Position value) where T : Ursa.Controls.ScrollToButton
=> style._addSetter(Ursa.Controls.ScrollToButton.DirectionProperty, value);
public static Style<T> Direction<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ScrollToButton
=> style._addSetter(Ursa.Controls.ScrollToButton.DirectionProperty, binding);
}

