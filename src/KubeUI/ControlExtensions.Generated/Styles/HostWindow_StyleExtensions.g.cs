using Avalonia.Data;
using Avalonia.Data.Converters;
using Dock.Avalonia.Controls;
using HostWindow = Dock.Avalonia.Controls.HostWindow;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class HostWindowExtensions
{
public static Style<T> IsToolWindow<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.HostWindow
=> style._addSetter(Dock.Avalonia.Controls.HostWindow.IsToolWindowProperty, value);
public static Style<T> IsToolWindow<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.HostWindow
=> style._addSetter(Dock.Avalonia.Controls.HostWindow.IsToolWindowProperty, binding);
public static Style<T> ToolChromeControlsWholeWindow<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.HostWindow
=> style._addSetter(Dock.Avalonia.Controls.HostWindow.ToolChromeControlsWholeWindowProperty, value);
public static Style<T> ToolChromeControlsWholeWindow<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.HostWindow
=> style._addSetter(Dock.Avalonia.Controls.HostWindow.ToolChromeControlsWholeWindowProperty, binding);
}

