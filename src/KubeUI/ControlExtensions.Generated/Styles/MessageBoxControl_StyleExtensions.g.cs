using Avalonia.Data;
using Avalonia.Data.Converters;
using MessageBoxControl = Ursa.Controls.MessageBoxControl;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class MessageBoxControlExtensions
{
public static Style<T> MessageIcon<T>(this Style<T> style, Ursa.Controls.MessageBoxIcon value) where T : Ursa.Controls.MessageBoxControl
=> style._addSetter(Ursa.Controls.MessageBoxControl.MessageIconProperty, value);
public static Style<T> MessageIcon<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.MessageBoxControl
=> style._addSetter(Ursa.Controls.MessageBoxControl.MessageIconProperty, binding);
public static Style<T> Buttons<T>(this Style<T> style, Ursa.Controls.MessageBoxButton value) where T : Ursa.Controls.MessageBoxControl
=> style._addSetter(Ursa.Controls.MessageBoxControl.ButtonsProperty, value);
public static Style<T> Buttons<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.MessageBoxControl
=> style._addSetter(Ursa.Controls.MessageBoxControl.ButtonsProperty, binding);
public static Style<T> Title<T>(this Style<T> style, System.String value) where T : Ursa.Controls.MessageBoxControl
=> style._addSetter(Ursa.Controls.MessageBoxControl.TitleProperty, value);
public static Style<T> Title<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.MessageBoxControl
=> style._addSetter(Ursa.Controls.MessageBoxControl.TitleProperty, binding);
}

