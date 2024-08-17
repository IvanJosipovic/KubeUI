using Avalonia.Data;
using Avalonia.Data.Converters;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using PinnedDockControl = Dock.Avalonia.Controls.PinnedDockControl;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class PinnedDockControlExtensions
{
public static Style<T> PinnedDockAlignment<T>(this Style<T> style, Dock.Model.Core.Alignment value) where T : Dock.Avalonia.Controls.PinnedDockControl
=> style._addSetter(Dock.Avalonia.Controls.PinnedDockControl.PinnedDockAlignmentProperty, value);
public static Style<T> PinnedDockAlignment<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.PinnedDockControl
=> style._addSetter(Dock.Avalonia.Controls.PinnedDockControl.PinnedDockAlignmentProperty, binding);
}

