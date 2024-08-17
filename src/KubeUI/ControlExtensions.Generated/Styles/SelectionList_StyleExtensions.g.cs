using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using SelectionList = Ursa.Controls.SelectionList;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class SelectionListExtensions
{
public static Style<T> Indicator<T>(this Style<T> style, Avalonia.Controls.Control value) where T : Ursa.Controls.SelectionList
=> style._addSetter(Ursa.Controls.SelectionList.IndicatorProperty, value);
public static Style<T> Indicator<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.SelectionList
=> style._addSetter(Ursa.Controls.SelectionList.IndicatorProperty, binding);
}

