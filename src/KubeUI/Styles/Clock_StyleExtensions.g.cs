using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Clock = Ursa.Controls.Clock;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class ClockExtensions
{
public static Style<T> Time<T>(this Style<T> style, System.DateTime value) where T : Ursa.Controls.Clock
=> style._addSetter(Ursa.Controls.Clock.TimeProperty, value);
public static Style<T> Time<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Clock
=> style._addSetter(Ursa.Controls.Clock.TimeProperty, binding);
public static Style<T> ShowHourTicks<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.Clock
=> style._addSetter(Ursa.Controls.Clock.ShowHourTicksProperty, value);
public static Style<T> ShowHourTicks<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Clock
=> style._addSetter(Ursa.Controls.Clock.ShowHourTicksProperty, binding);
public static Style<T> ShowMinuteTicks<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.Clock
=> style._addSetter(Ursa.Controls.Clock.ShowMinuteTicksProperty, value);
public static Style<T> ShowMinuteTicks<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Clock
=> style._addSetter(Ursa.Controls.Clock.ShowMinuteTicksProperty, binding);
public static Style<T> HandBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Ursa.Controls.Clock
=> style._addSetter(Ursa.Controls.Clock.HandBrushProperty, value);
public static Style<T> HandBrush<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Clock
=> style._addSetter(Ursa.Controls.Clock.HandBrushProperty, binding);
public static Style<T> ShowHourHand<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.Clock
=> style._addSetter(Ursa.Controls.Clock.ShowHourHandProperty, value);
public static Style<T> ShowHourHand<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Clock
=> style._addSetter(Ursa.Controls.Clock.ShowHourHandProperty, binding);
public static Style<T> ShowMinuteHand<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.Clock
=> style._addSetter(Ursa.Controls.Clock.ShowMinuteHandProperty, value);
public static Style<T> ShowMinuteHand<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Clock
=> style._addSetter(Ursa.Controls.Clock.ShowMinuteHandProperty, binding);
public static Style<T> ShowSecondHand<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.Clock
=> style._addSetter(Ursa.Controls.Clock.ShowSecondHandProperty, value);
public static Style<T> ShowSecondHand<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Clock
=> style._addSetter(Ursa.Controls.Clock.ShowSecondHandProperty, binding);
public static Style<T> IsSmooth<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.Clock
=> style._addSetter(Ursa.Controls.Clock.IsSmoothProperty, value);
public static Style<T> IsSmooth<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Clock
=> style._addSetter(Ursa.Controls.Clock.IsSmoothProperty, binding);
}

