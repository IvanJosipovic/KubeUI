using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Form = Ursa.Controls.Form;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Common;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class FormExtensions
{
public static Style<T> LabelWidth<T>(this Style<T> style, Avalonia.Controls.GridLength value) where T : Ursa.Controls.Form
=> style._addSetter(Ursa.Controls.Form.LabelWidthProperty, value);
public static Style<T> LabelWidth<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Form
=> style._addSetter(Ursa.Controls.Form.LabelWidthProperty, binding);

public static Style<T> LabelWidth<T>(this Style<T> style, Double value) where T : Ursa.Controls.Form
   => style._addSetter(Ursa.Controls.Form.LabelWidthProperty, new Avalonia.Controls.GridLength(value));
public static Style<T> LabelWidth<T>(this Style<T> style, Double value, GridUnitType type) where T : Ursa.Controls.Form
   => style._addSetter(Ursa.Controls.Form.LabelWidthProperty, new Avalonia.Controls.GridLength(value, type));
public static Style<T> LabelPosition<T>(this Style<T> style, Ursa.Common.Position value) where T : Ursa.Controls.Form
=> style._addSetter(Ursa.Controls.Form.LabelPositionProperty, value);
public static Style<T> LabelPosition<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Form
=> style._addSetter(Ursa.Controls.Form.LabelPositionProperty, binding);
public static Style<T> LabelAlignment<T>(this Style<T> style, Avalonia.Layout.HorizontalAlignment value) where T : Ursa.Controls.Form
=> style._addSetter(Ursa.Controls.Form.LabelAlignmentProperty, value);
public static Style<T> LabelAlignment<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Form
=> style._addSetter(Ursa.Controls.Form.LabelAlignmentProperty, binding);
}

