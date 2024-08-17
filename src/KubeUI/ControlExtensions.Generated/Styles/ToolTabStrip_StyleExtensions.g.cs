using Avalonia.Data;
using Avalonia.Data.Converters;
using Dock.Avalonia.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using ToolTabStrip = Dock.Avalonia.Controls.ToolTabStrip;

namespace Avalonia.Markup.Declarative;
public static partial class ToolTabStripExtensions
{
public static Style<T> CanCreateItem<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.ToolTabStrip
=> style._addSetter(Dock.Avalonia.Controls.ToolTabStrip.CanCreateItemProperty, value);
public static Style<T> CanCreateItem<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.ToolTabStrip
=> style._addSetter(Dock.Avalonia.Controls.ToolTabStrip.CanCreateItemProperty, binding);
}

