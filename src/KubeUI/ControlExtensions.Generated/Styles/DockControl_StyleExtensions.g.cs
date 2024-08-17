using Avalonia.Data;
using Avalonia.Data.Converters;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using DockControl = Dock.Avalonia.Controls.DockControl;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class DockControlExtensions
{
public static Style<T> Layout<T>(this Style<T> style, Dock.Model.Core.IDock value) where T : Dock.Avalonia.Controls.DockControl
=> style._addSetter(Dock.Avalonia.Controls.DockControl.LayoutProperty, value);
public static Style<T> Layout<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DockControl
=> style._addSetter(Dock.Avalonia.Controls.DockControl.LayoutProperty, binding);
public static Style<T> DefaultContext<T>(this Style<T> style, System.Object value) where T : Dock.Avalonia.Controls.DockControl
=> style._addSetter(Dock.Avalonia.Controls.DockControl.DefaultContextProperty, value);
public static Style<T> DefaultContext<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DockControl
=> style._addSetter(Dock.Avalonia.Controls.DockControl.DefaultContextProperty, binding);
public static Style<T> InitializeLayout<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.DockControl
=> style._addSetter(Dock.Avalonia.Controls.DockControl.InitializeLayoutProperty, value);
public static Style<T> InitializeLayout<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DockControl
=> style._addSetter(Dock.Avalonia.Controls.DockControl.InitializeLayoutProperty, binding);
public static Style<T> InitializeFactory<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.DockControl
=> style._addSetter(Dock.Avalonia.Controls.DockControl.InitializeFactoryProperty, value);
public static Style<T> InitializeFactory<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DockControl
=> style._addSetter(Dock.Avalonia.Controls.DockControl.InitializeFactoryProperty, binding);
public static Style<T> Factory<T>(this Style<T> style, Dock.Model.Core.IFactory value) where T : Dock.Avalonia.Controls.DockControl
=> style._addSetter(Dock.Avalonia.Controls.DockControl.FactoryProperty, value);
public static Style<T> Factory<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DockControl
=> style._addSetter(Dock.Avalonia.Controls.DockControl.FactoryProperty, binding);
public static Style<T> IsDraggingDock<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.DockControl
=> style._addSetter(Dock.Avalonia.Controls.DockControl.IsDraggingDockProperty, value);
public static Style<T> IsDraggingDock<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DockControl
=> style._addSetter(Dock.Avalonia.Controls.DockControl.IsDraggingDockProperty, binding);
}

