using Avalonia.Data;
using Avalonia.Data.Converters;
using DefaultDialogWindow = Ursa.Controls.DefaultDialogWindow;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class DefaultDialogWindowExtensions
{
public static Style<T> Buttons<T>(this Style<T> style, Ursa.Controls.DialogButton value) where T : Ursa.Controls.DefaultDialogWindow
=> style._addSetter(Ursa.Controls.DefaultDialogWindow.ButtonsProperty, value);
public static Style<T> Buttons<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DefaultDialogWindow
=> style._addSetter(Ursa.Controls.DefaultDialogWindow.ButtonsProperty, binding);
public static Style<T> Mode<T>(this Style<T> style, Ursa.Controls.DialogMode value) where T : Ursa.Controls.DefaultDialogWindow
=> style._addSetter(Ursa.Controls.DefaultDialogWindow.ModeProperty, value);
public static Style<T> Mode<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DefaultDialogWindow
=> style._addSetter(Ursa.Controls.DefaultDialogWindow.ModeProperty, binding);
}

