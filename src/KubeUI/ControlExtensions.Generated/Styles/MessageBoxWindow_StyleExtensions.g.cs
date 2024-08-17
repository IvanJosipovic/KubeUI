using Avalonia.Data;
using Avalonia.Data.Converters;
using MessageBoxWindow = Ursa.Controls.MessageBoxWindow;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class MessageBoxWindowExtensions
{
public static Style<T> MessageIcon<T>(this Style<T> style, Ursa.Controls.MessageBoxIcon value) where T : Ursa.Controls.MessageBoxWindow
=> style._addSetter(Ursa.Controls.MessageBoxWindow.MessageIconProperty, value);
public static Style<T> MessageIcon<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.MessageBoxWindow
=> style._addSetter(Ursa.Controls.MessageBoxWindow.MessageIconProperty, binding);
}

