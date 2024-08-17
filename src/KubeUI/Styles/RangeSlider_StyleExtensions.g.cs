using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using RangeSlider = Ursa.Controls.RangeSlider;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class RangeSliderExtensions
{
public static Style<T> Minimum<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.MinimumProperty, value);
public static Style<T> Minimum<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.MinimumProperty, binding);
public static Style<T> Maximum<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.MaximumProperty, value);
public static Style<T> Maximum<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.MaximumProperty, binding);
public static Style<T> LowerValue<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.LowerValueProperty, value);
public static Style<T> LowerValue<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.LowerValueProperty, binding);
public static Style<T> UpperValue<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.UpperValueProperty, value);
public static Style<T> UpperValue<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.UpperValueProperty, binding);
public static Style<T> TrackWidth<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.TrackWidthProperty, value);
public static Style<T> TrackWidth<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.TrackWidthProperty, binding);
public static Style<T> Orientation<T>(this Style<T> style, Avalonia.Layout.Orientation value) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.OrientationProperty, value);
public static Style<T> Orientation<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.OrientationProperty, binding);
public static Style<T> IsDirectionReversed<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.IsDirectionReversedProperty, value);
public static Style<T> IsDirectionReversed<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.IsDirectionReversedProperty, binding);
public static Style<T> TickFrequency<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.TickFrequencyProperty, value);
public static Style<T> TickFrequency<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.TickFrequencyProperty, binding);
public static Style<T> Ticks<T>(this Style<T> style, Avalonia.Collections.AvaloniaList<System.Double> value) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.TicksProperty, value);
public static Style<T> Ticks<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.TicksProperty, binding);
public static Style<T> TickPlacement<T>(this Style<T> style, Avalonia.Controls.TickPlacement value) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.TickPlacementProperty, value);
public static Style<T> TickPlacement<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.TickPlacementProperty, binding);
public static Style<T> IsSnapToTick<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.IsSnapToTickProperty, value);
public static Style<T> IsSnapToTick<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.RangeSlider
=> style._addSetter(Ursa.Controls.RangeSlider.IsSnapToTickProperty, binding);
}

