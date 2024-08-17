using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TimeBox = Ursa.Controls.TimeBox;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class TimeBoxExtensions
{
public static Style<T> Time<T>(this Style<T> style, System.Nullable<System.TimeSpan> value) where T : Ursa.Controls.TimeBox
=> style._addSetter(Ursa.Controls.TimeBox.TimeProperty, value);
public static Style<T> Time<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimeBox
=> style._addSetter(Ursa.Controls.TimeBox.TimeProperty, binding);
public static Style<T> TextAlignment<T>(this Style<T> style, Avalonia.Media.TextAlignment value) where T : Ursa.Controls.TimeBox
=> style._addSetter(Ursa.Controls.TimeBox.TextAlignmentProperty, value);
public static Style<T> TextAlignment<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimeBox
=> style._addSetter(Ursa.Controls.TimeBox.TextAlignmentProperty, binding);
public static Style<T> SelectionBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Ursa.Controls.TimeBox
=> style._addSetter(Ursa.Controls.TimeBox.SelectionBrushProperty, value);
public static Style<T> SelectionBrush<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimeBox
=> style._addSetter(Ursa.Controls.TimeBox.SelectionBrushProperty, binding);
public static Style<T> SelectionForegroundBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Ursa.Controls.TimeBox
=> style._addSetter(Ursa.Controls.TimeBox.SelectionForegroundBrushProperty, value);
public static Style<T> SelectionForegroundBrush<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimeBox
=> style._addSetter(Ursa.Controls.TimeBox.SelectionForegroundBrushProperty, binding);
public static Style<T> CaretBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Ursa.Controls.TimeBox
=> style._addSetter(Ursa.Controls.TimeBox.CaretBrushProperty, value);
public static Style<T> CaretBrush<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimeBox
=> style._addSetter(Ursa.Controls.TimeBox.CaretBrushProperty, binding);
public static Style<T> ShowLeadingZero<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.TimeBox
=> style._addSetter(Ursa.Controls.TimeBox.ShowLeadingZeroProperty, value);
public static Style<T> ShowLeadingZero<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimeBox
=> style._addSetter(Ursa.Controls.TimeBox.ShowLeadingZeroProperty, binding);
public static Style<T> InputMode<T>(this Style<T> style, Ursa.Controls.TimeBoxInputMode value) where T : Ursa.Controls.TimeBox
=> style._addSetter(Ursa.Controls.TimeBox.InputModeProperty, value);
public static Style<T> InputMode<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimeBox
=> style._addSetter(Ursa.Controls.TimeBox.InputModeProperty, binding);
public static Style<T> AllowDrag<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.TimeBox
=> style._addSetter(Ursa.Controls.TimeBox.AllowDragProperty, value);
public static Style<T> AllowDrag<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimeBox
=> style._addSetter(Ursa.Controls.TimeBox.AllowDragProperty, binding);
public static Style<T> DragOrientation<T>(this Style<T> style, Ursa.Controls.TimeBoxDragOrientation value) where T : Ursa.Controls.TimeBox
=> style._addSetter(Ursa.Controls.TimeBox.DragOrientationProperty, value);
public static Style<T> DragOrientation<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimeBox
=> style._addSetter(Ursa.Controls.TimeBox.DragOrientationProperty, binding);
public static Style<T> IsTimeLoop<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.TimeBox
=> style._addSetter(Ursa.Controls.TimeBox.IsTimeLoopProperty, value);
public static Style<T> IsTimeLoop<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimeBox
=> style._addSetter(Ursa.Controls.TimeBox.IsTimeLoopProperty, binding);
}

