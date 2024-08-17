using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using NumPadButton = Ursa.Controls.NumPadButton;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class NumPadButtonExtensions
{
public static Style<T> NumKey<T>(this Style<T> style, System.Nullable<Avalonia.Input.Key> value) where T : Ursa.Controls.NumPadButton
=> style._addSetter(Ursa.Controls.NumPadButton.NumKeyProperty, value);
public static Style<T> NumKey<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumPadButton
=> style._addSetter(Ursa.Controls.NumPadButton.NumKeyProperty, binding);
public static Style<T> FunctionKey<T>(this Style<T> style, System.Nullable<Avalonia.Input.Key> value) where T : Ursa.Controls.NumPadButton
=> style._addSetter(Ursa.Controls.NumPadButton.FunctionKeyProperty, value);
public static Style<T> FunctionKey<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumPadButton
=> style._addSetter(Ursa.Controls.NumPadButton.FunctionKeyProperty, binding);
public static Style<T> NumMode<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.NumPadButton
=> style._addSetter(Ursa.Controls.NumPadButton.NumModeProperty, value);
public static Style<T> NumMode<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumPadButton
=> style._addSetter(Ursa.Controls.NumPadButton.NumModeProperty, binding);
public static Style<T> NumContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.NumPadButton
=> style._addSetter(Ursa.Controls.NumPadButton.NumContentProperty, value);
public static Style<T> NumContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumPadButton
=> style._addSetter(Ursa.Controls.NumPadButton.NumContentProperty, binding);
public static Style<T> FunctionContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.NumPadButton
=> style._addSetter(Ursa.Controls.NumPadButton.FunctionContentProperty, value);
public static Style<T> FunctionContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumPadButton
=> style._addSetter(Ursa.Controls.NumPadButton.FunctionContentProperty, binding);
}

