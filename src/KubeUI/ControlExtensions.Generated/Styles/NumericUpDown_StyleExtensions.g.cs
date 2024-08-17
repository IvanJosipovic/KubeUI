using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using NumericUpDown = Ursa.Controls.NumericUpDown;
using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class NumericUpDownExtensions
{
public static Style<T> AllowDrag<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.AllowDragProperty, value);
public static Style<T> AllowDrag<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.AllowDragProperty, binding);
public static Style<T> IsReadOnly<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.IsReadOnlyProperty, value);
public static Style<T> IsReadOnly<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.IsReadOnlyProperty, binding);
public static Style<T> HorizontalContentAlignment<T>(this Style<T> style, Avalonia.Layout.HorizontalAlignment value) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.HorizontalContentAlignmentProperty, value);
public static Style<T> HorizontalContentAlignment<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.HorizontalContentAlignmentProperty, binding);
public static Style<T> InnerLeftContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.InnerLeftContentProperty, value);
public static Style<T> InnerLeftContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.InnerLeftContentProperty, binding);
public static Style<T> Watermark<T>(this Style<T> style, System.String value) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.WatermarkProperty, value);
public static Style<T> Watermark<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.WatermarkProperty, binding);
public static Style<T> NumberFormat<T>(this Style<T> style, System.Globalization.NumberFormatInfo value) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.NumberFormatProperty, value);
public static Style<T> NumberFormat<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.NumberFormatProperty, binding);
public static Style<T> FormatString<T>(this Style<T> style, System.String value) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.FormatStringProperty, value);
public static Style<T> FormatString<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.FormatStringProperty, binding);
public static Style<T> ParsingNumberStyle<T>(this Style<T> style, System.Globalization.NumberStyles value) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.ParsingNumberStyleProperty, value);
public static Style<T> ParsingNumberStyle<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.ParsingNumberStyleProperty, binding);
public static Style<T> TextConverter<T>(this Style<T> style, Avalonia.Data.Converters.IValueConverter value) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.TextConverterProperty, value);
public static Style<T> TextConverter<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.TextConverterProperty, binding);
public static Style<T> AllowSpin<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.AllowSpinProperty, value);
public static Style<T> AllowSpin<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.AllowSpinProperty, binding);
public static Style<T> ShowButtonSpinner<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.ShowButtonSpinnerProperty, value);
public static Style<T> ShowButtonSpinner<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NumericUpDown
=> style._addSetter(Ursa.Controls.NumericUpDown.ShowButtonSpinnerProperty, binding);
}

