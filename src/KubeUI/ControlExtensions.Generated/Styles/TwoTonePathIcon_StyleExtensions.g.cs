using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TwoTonePathIcon = Ursa.Controls.TwoTonePathIcon;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class TwoTonePathIconExtensions
{
public static Style<T> StrokeBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Ursa.Controls.TwoTonePathIcon
=> style._addSetter(Ursa.Controls.TwoTonePathIcon.StrokeBrushProperty, value);
public static Style<T> StrokeBrush<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TwoTonePathIcon
=> style._addSetter(Ursa.Controls.TwoTonePathIcon.StrokeBrushProperty, binding);
public static Style<T> Data<T>(this Style<T> style, Avalonia.Media.Geometry value) where T : Ursa.Controls.TwoTonePathIcon
=> style._addSetter(Ursa.Controls.TwoTonePathIcon.DataProperty, value);
public static Style<T> Data<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TwoTonePathIcon
=> style._addSetter(Ursa.Controls.TwoTonePathIcon.DataProperty, binding);
public static Style<T> IsActive<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.TwoTonePathIcon
=> style._addSetter(Ursa.Controls.TwoTonePathIcon.IsActiveProperty, value);
public static Style<T> IsActive<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TwoTonePathIcon
=> style._addSetter(Ursa.Controls.TwoTonePathIcon.IsActiveProperty, binding);
public static Style<T> ActiveForeground<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Ursa.Controls.TwoTonePathIcon
=> style._addSetter(Ursa.Controls.TwoTonePathIcon.ActiveForegroundProperty, value);
public static Style<T> ActiveForeground<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TwoTonePathIcon
=> style._addSetter(Ursa.Controls.TwoTonePathIcon.ActiveForegroundProperty, binding);
public static Style<T> ActiveStrokeBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Ursa.Controls.TwoTonePathIcon
=> style._addSetter(Ursa.Controls.TwoTonePathIcon.ActiveStrokeBrushProperty, value);
public static Style<T> ActiveStrokeBrush<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TwoTonePathIcon
=> style._addSetter(Ursa.Controls.TwoTonePathIcon.ActiveStrokeBrushProperty, binding);
public static Style<T> StrokeThickness<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.TwoTonePathIcon
=> style._addSetter(Ursa.Controls.TwoTonePathIcon.StrokeThicknessProperty, value);
public static Style<T> StrokeThickness<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TwoTonePathIcon
=> style._addSetter(Ursa.Controls.TwoTonePathIcon.StrokeThicknessProperty, binding);
}

