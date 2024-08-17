using Avalonia.Data;
using Avalonia.Data.Converters;
using PinCodeItem = Ursa.Controls.PinCodeItem;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class PinCodeItemExtensions
{
public static Style<T> Text<T>(this Style<T> style, System.String value) where T : Ursa.Controls.PinCodeItem
=> style._addSetter(Ursa.Controls.PinCodeItem.TextProperty, value);
public static Style<T> Text<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.PinCodeItem
=> style._addSetter(Ursa.Controls.PinCodeItem.TextProperty, binding);
public static Style<T> PasswordChar<T>(this Style<T> style, System.Char value) where T : Ursa.Controls.PinCodeItem
=> style._addSetter(Ursa.Controls.PinCodeItem.PasswordCharProperty, value);
public static Style<T> PasswordChar<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.PinCodeItem
=> style._addSetter(Ursa.Controls.PinCodeItem.PasswordCharProperty, binding);
}

