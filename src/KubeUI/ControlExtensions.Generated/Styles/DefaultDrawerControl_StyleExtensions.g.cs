using Avalonia.Data;
using Avalonia.Data.Converters;
using DefaultDrawerControl = Ursa.Controls.DefaultDrawerControl;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class DefaultDrawerControlExtensions
{
public static Style<T> Buttons<T>(this Style<T> style, Ursa.Controls.DialogButton value) where T : Ursa.Controls.DefaultDrawerControl
=> style._addSetter(Ursa.Controls.DefaultDrawerControl.ButtonsProperty, value);
public static Style<T> Buttons<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DefaultDrawerControl
=> style._addSetter(Ursa.Controls.DefaultDrawerControl.ButtonsProperty, binding);
public static Style<T> Mode<T>(this Style<T> style, Ursa.Controls.DialogMode value) where T : Ursa.Controls.DefaultDrawerControl
=> style._addSetter(Ursa.Controls.DefaultDrawerControl.ModeProperty, value);
public static Style<T> Mode<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DefaultDrawerControl
=> style._addSetter(Ursa.Controls.DefaultDrawerControl.ModeProperty, binding);
public static Style<T> Title<T>(this Style<T> style, System.String value) where T : Ursa.Controls.DefaultDrawerControl
=> style._addSetter(Ursa.Controls.DefaultDrawerControl.TitleProperty, value);
public static Style<T> Title<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DefaultDrawerControl
=> style._addSetter(Ursa.Controls.DefaultDrawerControl.TitleProperty, binding);
}

