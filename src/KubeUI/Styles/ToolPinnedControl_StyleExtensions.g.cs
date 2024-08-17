using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Dock.Avalonia.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using ToolPinnedControl = Dock.Avalonia.Controls.ToolPinnedControl;

namespace Avalonia.Markup.Declarative;
public static partial class ToolPinnedControlExtensions
{
public static Style<T> Orientation<T>(this Style<T> style, Avalonia.Layout.Orientation value) where T : Dock.Avalonia.Controls.ToolPinnedControl
=> style._addSetter(Dock.Avalonia.Controls.ToolPinnedControl.OrientationProperty, value);
public static Style<T> Orientation<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.ToolPinnedControl
=> style._addSetter(Dock.Avalonia.Controls.ToolPinnedControl.OrientationProperty, binding);
}

