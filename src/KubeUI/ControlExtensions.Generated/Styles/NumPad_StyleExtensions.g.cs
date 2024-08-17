using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using NumPad = Ursa.Controls.NumPad;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class NumPadExtensions
{
public static Style<T> Target<T>(this Style<T> style, Avalonia.Input.InputElement value) where T : Ursa.Controls.NumPad
=> style._addSetter(Ursa.Controls.NumPad.TargetProperty, value);
public static Style<T> Target<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumPad
=> style._addSetter(Ursa.Controls.NumPad.TargetProperty, binding);
public static Style<T> NumMode<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.NumPad
=> style._addSetter(Ursa.Controls.NumPad.NumModeProperty, value);
public static Style<T> NumMode<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumPad
=> style._addSetter(Ursa.Controls.NumPad.NumModeProperty, binding);
}

