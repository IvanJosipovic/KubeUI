using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using ImageViewer = Ursa.Controls.ImageViewer;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class ImageViewerExtensions
{
public static Style<T> Overlayer<T>(this Style<T> style, Avalonia.Controls.Control value) where T : Ursa.Controls.ImageViewer
=> style._addSetter(Ursa.Controls.ImageViewer.OverlayerProperty, value);
public static Style<T> Overlayer<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ImageViewer
=> style._addSetter(Ursa.Controls.ImageViewer.OverlayerProperty, binding);
public static Style<T> Source<T>(this Style<T> style, Avalonia.Media.IImage value) where T : Ursa.Controls.ImageViewer
=> style._addSetter(Ursa.Controls.ImageViewer.SourceProperty, value);
public static Style<T> Source<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ImageViewer
=> style._addSetter(Ursa.Controls.ImageViewer.SourceProperty, binding);
public static Style<T> SmallChange<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.ImageViewer
=> style._addSetter(Ursa.Controls.ImageViewer.SmallChangeProperty, value);
public static Style<T> SmallChange<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ImageViewer
=> style._addSetter(Ursa.Controls.ImageViewer.SmallChangeProperty, binding);
public static Style<T> LargeChange<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.ImageViewer
=> style._addSetter(Ursa.Controls.ImageViewer.LargeChangeProperty, value);
public static Style<T> LargeChange<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ImageViewer
=> style._addSetter(Ursa.Controls.ImageViewer.LargeChangeProperty, binding);
public static Style<T> Stretch<T>(this Style<T> style, Avalonia.Media.Stretch value) where T : Ursa.Controls.ImageViewer
=> style._addSetter(Ursa.Controls.ImageViewer.StretchProperty, value);
public static Style<T> Stretch<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ImageViewer
=> style._addSetter(Ursa.Controls.ImageViewer.StretchProperty, binding);
}

