using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using ToolBar = Ursa.Controls.ToolBar;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class ToolBarExtensions
{
public static Style<T> Orientation<T>(this Style<T> style, Avalonia.Layout.Orientation value) where T : Ursa.Controls.ToolBar
=> style._addSetter(Ursa.Controls.ToolBar.OrientationProperty, value);
public static Style<T> Orientation<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ToolBar
=> style._addSetter(Ursa.Controls.ToolBar.OrientationProperty, binding);
public static Style<T> PopupPlacement<T>(this Style<T> style, Avalonia.Controls.PlacementMode value) where T : Ursa.Controls.ToolBar
=> style._addSetter(Ursa.Controls.ToolBar.PopupPlacementProperty, value);
public static Style<T> PopupPlacement<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ToolBar
=> style._addSetter(Ursa.Controls.ToolBar.PopupPlacementProperty, binding);
}

