using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using ThemeToggleButton = Ursa.Controls.ThemeToggleButton;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class ThemeToggleButtonExtensions
{
public static Style<T> IsThreeState<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.ThemeToggleButton
=> style._addSetter(Ursa.Controls.ThemeToggleButton.IsThreeStateProperty, value);
public static Style<T> IsThreeState<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ThemeToggleButton
=> style._addSetter(Ursa.Controls.ThemeToggleButton.IsThreeStateProperty, binding);
}

