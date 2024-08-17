using Avalonia.Data;
using Avalonia.Data.Converters;
using PinCode = Ursa.Controls.PinCode;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class PinCodeExtensions
{
public static Style<T> CompleteCommand<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : Ursa.Controls.PinCode
=> style._addSetter(Ursa.Controls.PinCode.CompleteCommandProperty, value);
public static Style<T> CompleteCommand<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.PinCode
=> style._addSetter(Ursa.Controls.PinCode.CompleteCommandProperty, binding);
public static Style<T> Count<T>(this Style<T> style, System.Int32 value) where T : Ursa.Controls.PinCode
=> style._addSetter(Ursa.Controls.PinCode.CountProperty, value);
public static Style<T> Count<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.PinCode
=> style._addSetter(Ursa.Controls.PinCode.CountProperty, binding);
public static Style<T> PasswordChar<T>(this Style<T> style, System.Char value) where T : Ursa.Controls.PinCode
=> style._addSetter(Ursa.Controls.PinCode.PasswordCharProperty, value);
public static Style<T> PasswordChar<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.PinCode
=> style._addSetter(Ursa.Controls.PinCode.PasswordCharProperty, binding);
public static Style<T> Mode<T>(this Style<T> style, Ursa.Controls.PinCodeMode value) where T : Ursa.Controls.PinCode
=> style._addSetter(Ursa.Controls.PinCode.ModeProperty, value);
public static Style<T> Mode<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.PinCode
=> style._addSetter(Ursa.Controls.PinCode.ModeProperty, binding);
}

