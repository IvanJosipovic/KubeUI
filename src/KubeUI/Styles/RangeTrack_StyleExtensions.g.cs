using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using RangeTrack = Ursa.Controls.RangeTrack;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class RangeTrackExtensions
{
public static Style<T> Minimum<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.RangeTrack
=> style._addSetter(Ursa.Controls.RangeTrack.MinimumProperty, value);
public static Style<T> Minimum<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.RangeTrack
=> style._addSetter(Ursa.Controls.RangeTrack.MinimumProperty, binding);
public static Style<T> Maximum<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.RangeTrack
=> style._addSetter(Ursa.Controls.RangeTrack.MaximumProperty, value);
public static Style<T> Maximum<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.RangeTrack
=> style._addSetter(Ursa.Controls.RangeTrack.MaximumProperty, binding);
public static Style<T> LowerValue<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.RangeTrack
=> style._addSetter(Ursa.Controls.RangeTrack.LowerValueProperty, value);
public static Style<T> LowerValue<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.RangeTrack
=> style._addSetter(Ursa.Controls.RangeTrack.LowerValueProperty, binding);
public static Style<T> UpperValue<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.RangeTrack
=> style._addSetter(Ursa.Controls.RangeTrack.UpperValueProperty, value);
public static Style<T> UpperValue<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.RangeTrack
=> style._addSetter(Ursa.Controls.RangeTrack.UpperValueProperty, binding);
public static Style<T> Orientation<T>(this Style<T> style, Avalonia.Layout.Orientation value) where T : Ursa.Controls.RangeTrack
=> style._addSetter(Ursa.Controls.RangeTrack.OrientationProperty, value);
public static Style<T> Orientation<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.RangeTrack
=> style._addSetter(Ursa.Controls.RangeTrack.OrientationProperty, binding);
public static Style<T> UpperSection<T>(this Style<T> style, Avalonia.Controls.Control value) where T : Ursa.Controls.RangeTrack
=> style._addSetter(Ursa.Controls.RangeTrack.UpperSectionProperty, value);
public static Style<T> UpperSection<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.RangeTrack
=> style._addSetter(Ursa.Controls.RangeTrack.UpperSectionProperty, binding);
public static Style<T> LowerSection<T>(this Style<T> style, Avalonia.Controls.Control value) where T : Ursa.Controls.RangeTrack
=> style._addSetter(Ursa.Controls.RangeTrack.LowerSectionProperty, value);
public static Style<T> LowerSection<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.RangeTrack
=> style._addSetter(Ursa.Controls.RangeTrack.LowerSectionProperty, binding);
public static Style<T> InnerSection<T>(this Style<T> style, Avalonia.Controls.Control value) where T : Ursa.Controls.RangeTrack
=> style._addSetter(Ursa.Controls.RangeTrack.InnerSectionProperty, value);
public static Style<T> InnerSection<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.RangeTrack
=> style._addSetter(Ursa.Controls.RangeTrack.InnerSectionProperty, binding);
public static Style<T> TrackBackground<T>(this Style<T> style, Avalonia.Controls.Control value) where T : Ursa.Controls.RangeTrack
=> style._addSetter(Ursa.Controls.RangeTrack.TrackBackgroundProperty, value);
public static Style<T> TrackBackground<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.RangeTrack
=> style._addSetter(Ursa.Controls.RangeTrack.TrackBackgroundProperty, binding);
public static Style<T> UpperThumb<T>(this Style<T> style, Avalonia.Controls.Primitives.Thumb value) where T : Ursa.Controls.RangeTrack
=> style._addSetter(Ursa.Controls.RangeTrack.UpperThumbProperty, value);
public static Style<T> UpperThumb<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.RangeTrack
=> style._addSetter(Ursa.Controls.RangeTrack.UpperThumbProperty, binding);
public static Style<T> LowerThumb<T>(this Style<T> style, Avalonia.Controls.Primitives.Thumb value) where T : Ursa.Controls.RangeTrack
=> style._addSetter(Ursa.Controls.RangeTrack.LowerThumbProperty, value);
public static Style<T> LowerThumb<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.RangeTrack
=> style._addSetter(Ursa.Controls.RangeTrack.LowerThumbProperty, binding);
public static Style<T> IsDirectionReversed<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.RangeTrack
=> style._addSetter(Ursa.Controls.RangeTrack.IsDirectionReversedProperty, value);
public static Style<T> IsDirectionReversed<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.RangeTrack
=> style._addSetter(Ursa.Controls.RangeTrack.IsDirectionReversedProperty, binding);
}

