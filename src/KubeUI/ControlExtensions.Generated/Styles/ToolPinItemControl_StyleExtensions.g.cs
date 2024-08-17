using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Dock.Avalonia.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using ToolPinItemControl = Dock.Avalonia.Controls.ToolPinItemControl;

namespace Avalonia.Markup.Declarative;
public static partial class ToolPinItemControlExtensions
{
public static Style<T> Orientation<T>(this Style<T> style, Avalonia.Layout.Orientation value) where T : Dock.Avalonia.Controls.ToolPinItemControl
=> style._addSetter(Dock.Avalonia.Controls.ToolPinItemControl.OrientationProperty, value);
public static Style<T> Orientation<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.ToolPinItemControl
=> style._addSetter(Dock.Avalonia.Controls.ToolPinItemControl.OrientationProperty, binding);
}

