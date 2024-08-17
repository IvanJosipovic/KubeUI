using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using ClockTicks = Ursa.Controls.ClockTicks;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class ClockTicksExtensions
{
public static Style<T> ShowHourTicks<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.ClockTicks
=> style._addSetter(Ursa.Controls.ClockTicks.ShowHourTicksProperty, value);
public static Style<T> ShowHourTicks<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ClockTicks
=> style._addSetter(Ursa.Controls.ClockTicks.ShowHourTicksProperty, binding);
public static Style<T> ShowMinuteTicks<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.ClockTicks
=> style._addSetter(Ursa.Controls.ClockTicks.ShowMinuteTicksProperty, value);
public static Style<T> ShowMinuteTicks<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ClockTicks
=> style._addSetter(Ursa.Controls.ClockTicks.ShowMinuteTicksProperty, binding);
public static Style<T> HourTickForeground<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Ursa.Controls.ClockTicks
=> style._addSetter(Ursa.Controls.ClockTicks.HourTickForegroundProperty, value);
public static Style<T> HourTickForeground<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ClockTicks
=> style._addSetter(Ursa.Controls.ClockTicks.HourTickForegroundProperty, binding);
public static Style<T> MinuteTickForeground<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Ursa.Controls.ClockTicks
=> style._addSetter(Ursa.Controls.ClockTicks.MinuteTickForegroundProperty, value);
public static Style<T> MinuteTickForeground<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ClockTicks
=> style._addSetter(Ursa.Controls.ClockTicks.MinuteTickForegroundProperty, binding);
public static Style<T> HourTickLength<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.ClockTicks
=> style._addSetter(Ursa.Controls.ClockTicks.HourTickLengthProperty, value);
public static Style<T> HourTickLength<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ClockTicks
=> style._addSetter(Ursa.Controls.ClockTicks.HourTickLengthProperty, binding);
public static Style<T> MinuteTickLength<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.ClockTicks
=> style._addSetter(Ursa.Controls.ClockTicks.MinuteTickLengthProperty, value);
public static Style<T> MinuteTickLength<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ClockTicks
=> style._addSetter(Ursa.Controls.ClockTicks.MinuteTickLengthProperty, binding);
public static Style<T> HourTickWidth<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.ClockTicks
=> style._addSetter(Ursa.Controls.ClockTicks.HourTickWidthProperty, value);
public static Style<T> HourTickWidth<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ClockTicks
=> style._addSetter(Ursa.Controls.ClockTicks.HourTickWidthProperty, binding);
public static Style<T> MinuteTickWidth<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.ClockTicks
=> style._addSetter(Ursa.Controls.ClockTicks.MinuteTickWidthProperty, value);
public static Style<T> MinuteTickWidth<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ClockTicks
=> style._addSetter(Ursa.Controls.ClockTicks.MinuteTickWidthProperty, binding);
}

