using Avalonia.Data;
using Avalonia.Data.Converters;
using DatePicker = Ursa.Controls.DatePicker;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class DatePickerExtensions
{
public static Style<T> SelectedDate<T>(this Style<T> style, System.Nullable<System.DateTime> value) where T : Ursa.Controls.DatePicker
=> style._addSetter(Ursa.Controls.DatePicker.SelectedDateProperty, value);
public static Style<T> SelectedDate<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DatePicker
=> style._addSetter(Ursa.Controls.DatePicker.SelectedDateProperty, binding);
public static Style<T> Watermark<T>(this Style<T> style, System.String value) where T : Ursa.Controls.DatePicker
=> style._addSetter(Ursa.Controls.DatePicker.WatermarkProperty, value);
public static Style<T> Watermark<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DatePicker
=> style._addSetter(Ursa.Controls.DatePicker.WatermarkProperty, binding);
}

