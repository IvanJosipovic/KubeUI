using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TimePicker = Ursa.Controls.TimePicker;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class TimePickerExtensions
{
public static Style<T> SelectedTime<T>(this Style<T> style, System.Nullable<System.TimeSpan> value) where T : Ursa.Controls.TimePicker
=> style._addSetter(Ursa.Controls.TimePicker.SelectedTimeProperty, value);
public static Style<T> SelectedTime<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimePicker
=> style._addSetter(Ursa.Controls.TimePicker.SelectedTimeProperty, binding);
public static Style<T> Watermark<T>(this Style<T> style, System.String value) where T : Ursa.Controls.TimePicker
=> style._addSetter(Ursa.Controls.TimePicker.WatermarkProperty, value);
public static Style<T> Watermark<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimePicker
=> style._addSetter(Ursa.Controls.TimePicker.WatermarkProperty, binding);
}

