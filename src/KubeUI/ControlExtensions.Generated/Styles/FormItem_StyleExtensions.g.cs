using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using FormItem = Ursa.Controls.FormItem;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class FormItemExtensions
{
public static Style<T> LabelWidth<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.FormItem
=> style._addSetter(Ursa.Controls.FormItem.LabelWidthProperty, value);
public static Style<T> LabelWidth<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.FormItem
=> style._addSetter(Ursa.Controls.FormItem.LabelWidthProperty, binding);
public static Style<T> LabelAlignment<T>(this Style<T> style, Avalonia.Layout.HorizontalAlignment value) where T : Ursa.Controls.FormItem
=> style._addSetter(Ursa.Controls.FormItem.LabelAlignmentProperty, value);
public static Style<T> LabelAlignment<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.FormItem
=> style._addSetter(Ursa.Controls.FormItem.LabelAlignmentProperty, binding);
}

