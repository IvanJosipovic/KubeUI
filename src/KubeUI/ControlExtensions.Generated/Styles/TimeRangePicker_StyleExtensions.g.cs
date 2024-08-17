using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TimeRangePicker = Ursa.Controls.TimeRangePicker;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class TimeRangePickerExtensions
{
public static Style<T> StartTime<T>(this Style<T> style, System.Nullable<System.TimeSpan> value) where T : Ursa.Controls.TimeRangePicker
=> style._addSetter(Ursa.Controls.TimeRangePicker.StartTimeProperty, value);
public static Style<T> StartTime<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimeRangePicker
=> style._addSetter(Ursa.Controls.TimeRangePicker.StartTimeProperty, binding);
public static Style<T> EndTime<T>(this Style<T> style, System.Nullable<System.TimeSpan> value) where T : Ursa.Controls.TimeRangePicker
=> style._addSetter(Ursa.Controls.TimeRangePicker.EndTimeProperty, value);
public static Style<T> EndTime<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimeRangePicker
=> style._addSetter(Ursa.Controls.TimeRangePicker.EndTimeProperty, binding);
public static Style<T> StartWatermark<T>(this Style<T> style, System.String value) where T : Ursa.Controls.TimeRangePicker
=> style._addSetter(Ursa.Controls.TimeRangePicker.StartWatermarkProperty, value);
public static Style<T> StartWatermark<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimeRangePicker
=> style._addSetter(Ursa.Controls.TimeRangePicker.StartWatermarkProperty, binding);
public static Style<T> EndWatermark<T>(this Style<T> style, System.String value) where T : Ursa.Controls.TimeRangePicker
=> style._addSetter(Ursa.Controls.TimeRangePicker.EndWatermarkProperty, value);
public static Style<T> EndWatermark<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimeRangePicker
=> style._addSetter(Ursa.Controls.TimeRangePicker.EndWatermarkProperty, binding);
}

