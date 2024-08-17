using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Dock.Avalonia.Controls;
using ProportionalStackPanel = Dock.Avalonia.Controls.ProportionalStackPanel;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class ProportionalStackPanelExtensions
{
public static Style<T> Orientation<T>(this Style<T> style, Avalonia.Layout.Orientation value) where T : Dock.Avalonia.Controls.ProportionalStackPanel
=> style._addSetter(Dock.Avalonia.Controls.ProportionalStackPanel.OrientationProperty, value);
public static Style<T> Orientation<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.ProportionalStackPanel
=> style._addSetter(Dock.Avalonia.Controls.ProportionalStackPanel.OrientationProperty, binding);
}

