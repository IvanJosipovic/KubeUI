using Avalonia.Data;
using Avalonia.Data.Converters;
using NumberDisplayerBase = Ursa.Controls.NumberDisplayerBase;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class NumberDisplayerBaseExtensions
{
public static Style<T> Duration<T>(this Style<T> style, System.TimeSpan value) where T : Ursa.Controls.NumberDisplayerBase
=> style._addSetter(Ursa.Controls.NumberDisplayerBase.DurationProperty, value);
public static Style<T> Duration<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumberDisplayerBase
=> style._addSetter(Ursa.Controls.NumberDisplayerBase.DurationProperty, binding);
public static Style<T> StringFormat<T>(this Style<T> style, System.String value) where T : Ursa.Controls.NumberDisplayerBase
=> style._addSetter(Ursa.Controls.NumberDisplayerBase.StringFormatProperty, value);
public static Style<T> StringFormat<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumberDisplayerBase
=> style._addSetter(Ursa.Controls.NumberDisplayerBase.StringFormatProperty, binding);
public static Style<T> IsSelectable<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.NumberDisplayerBase
=> style._addSetter(Ursa.Controls.NumberDisplayerBase.IsSelectableProperty, value);
public static Style<T> IsSelectable<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumberDisplayerBase
=> style._addSetter(Ursa.Controls.NumberDisplayerBase.IsSelectableProperty, binding);
}

