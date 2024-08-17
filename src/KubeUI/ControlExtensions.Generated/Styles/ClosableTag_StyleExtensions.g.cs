using Avalonia.Data;
using Avalonia.Data.Converters;
using ClosableTag = Ursa.Controls.ClosableTag;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class ClosableTagExtensions
{
public static Style<T> Command<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : Ursa.Controls.ClosableTag
=> style._addSetter(Ursa.Controls.ClosableTag.CommandProperty, value);
public static Style<T> Command<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ClosableTag
=> style._addSetter(Ursa.Controls.ClosableTag.CommandProperty, binding);
}

