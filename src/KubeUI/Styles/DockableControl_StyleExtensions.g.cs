using Avalonia.Data;
using Avalonia.Data.Converters;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using DockableControl = Dock.Avalonia.Controls.DockableControl;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class DockableControlExtensions
{
public static Style<T> TrackingMode<T>(this Style<T> style, Dock.Model.Core.TrackingMode value) where T : Dock.Avalonia.Controls.DockableControl
=> style._addSetter(Dock.Avalonia.Controls.DockableControl.TrackingModeProperty, value);
public static Style<T> TrackingMode<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DockableControl
=> style._addSetter(Dock.Avalonia.Controls.DockableControl.TrackingModeProperty, binding);
}

