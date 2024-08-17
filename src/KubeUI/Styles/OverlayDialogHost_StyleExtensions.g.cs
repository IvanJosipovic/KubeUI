using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using OverlayDialogHost = Ursa.Controls.OverlayDialogHost;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class OverlayDialogHostExtensions
{
public static Style<T> IsModalStatusReporter<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.OverlayDialogHost
=> style._addSetter(Ursa.Controls.OverlayDialogHost.IsModalStatusReporterProperty, value);
public static Style<T> IsModalStatusReporter<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.OverlayDialogHost
=> style._addSetter(Ursa.Controls.OverlayDialogHost.IsModalStatusReporterProperty, binding);
public static Style<T> OverlayMaskBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Ursa.Controls.OverlayDialogHost
=> style._addSetter(Ursa.Controls.OverlayDialogHost.OverlayMaskBrushProperty, value);
public static Style<T> OverlayMaskBrush<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.OverlayDialogHost
=> style._addSetter(Ursa.Controls.OverlayDialogHost.OverlayMaskBrushProperty, binding);
}

