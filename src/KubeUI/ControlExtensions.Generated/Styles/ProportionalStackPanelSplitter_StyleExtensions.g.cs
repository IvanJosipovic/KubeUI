using Avalonia.Data;
using Avalonia.Data.Converters;
using Dock.Avalonia.Controls;
using ProportionalStackPanelSplitter = Dock.Avalonia.Controls.ProportionalStackPanelSplitter;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class ProportionalStackPanelSplitterExtensions
{
public static Style<T> Thickness<T>(this Style<T> style, System.Double value) where T : Dock.Avalonia.Controls.ProportionalStackPanelSplitter
=> style._addSetter(Dock.Avalonia.Controls.ProportionalStackPanelSplitter.ThicknessProperty, value);
public static Style<T> Thickness<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.ProportionalStackPanelSplitter
=> style._addSetter(Dock.Avalonia.Controls.ProportionalStackPanelSplitter.ThicknessProperty, binding);
}

