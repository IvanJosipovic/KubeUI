using Avalonia.Data;
using Avalonia.Data.Converters;
using PaginationButton = Ursa.Controls.PaginationButton;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class PaginationButtonExtensions
{
public static Style<T> Page<T>(this Style<T> style, System.Int32 value) where T : Ursa.Controls.PaginationButton
=> style._addSetter(Ursa.Controls.PaginationButton.PageProperty, value);
public static Style<T> Page<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.PaginationButton
=> style._addSetter(Ursa.Controls.PaginationButton.PageProperty, binding);
}

