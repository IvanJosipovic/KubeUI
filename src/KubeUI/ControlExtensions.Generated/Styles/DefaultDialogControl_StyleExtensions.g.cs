using Avalonia.Data;
using Avalonia.Data.Converters;
using DefaultDialogControl = Ursa.Controls.DefaultDialogControl;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class DefaultDialogControlExtensions
{
public static Style<T> Title<T>(this Style<T> style, System.String value) where T : Ursa.Controls.DefaultDialogControl
=> style._addSetter(Ursa.Controls.DefaultDialogControl.TitleProperty, value);
public static Style<T> Title<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DefaultDialogControl
=> style._addSetter(Ursa.Controls.DefaultDialogControl.TitleProperty, binding);
public static Style<T> Buttons<T>(this Style<T> style, Ursa.Controls.DialogButton value) where T : Ursa.Controls.DefaultDialogControl
=> style._addSetter(Ursa.Controls.DefaultDialogControl.ButtonsProperty, value);
public static Style<T> Buttons<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DefaultDialogControl
=> style._addSetter(Ursa.Controls.DefaultDialogControl.ButtonsProperty, binding);
public static Style<T> Mode<T>(this Style<T> style, Ursa.Controls.DialogMode value) where T : Ursa.Controls.DefaultDialogControl
=> style._addSetter(Ursa.Controls.DefaultDialogControl.ModeProperty, value);
public static Style<T> Mode<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DefaultDialogControl
=> style._addSetter(Ursa.Controls.DefaultDialogControl.ModeProperty, binding);
}

