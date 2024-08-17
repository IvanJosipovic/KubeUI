using Avalonia.Data;
using Avalonia.Data.Converters;
using Dock.Avalonia.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using ToolChromeControl = Dock.Avalonia.Controls.ToolChromeControl;

namespace Avalonia.Markup.Declarative;
public static partial class ToolChromeControlExtensions
{
public static Style<T> IsActive<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.ToolChromeControl
=> style._addSetter(Dock.Avalonia.Controls.ToolChromeControl.IsActiveProperty, value);
public static Style<T> IsActive<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.ToolChromeControl
=> style._addSetter(Dock.Avalonia.Controls.ToolChromeControl.IsActiveProperty, binding);
public static Style<T> IsPinned<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.ToolChromeControl
=> style._addSetter(Dock.Avalonia.Controls.ToolChromeControl.IsPinnedProperty, value);
public static Style<T> IsPinned<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.ToolChromeControl
=> style._addSetter(Dock.Avalonia.Controls.ToolChromeControl.IsPinnedProperty, binding);
public static Style<T> IsFloating<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.ToolChromeControl
=> style._addSetter(Dock.Avalonia.Controls.ToolChromeControl.IsFloatingProperty, value);
public static Style<T> IsFloating<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.ToolChromeControl
=> style._addSetter(Dock.Avalonia.Controls.ToolChromeControl.IsFloatingProperty, binding);
public static Style<T> IsMaximized<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.ToolChromeControl
=> style._addSetter(Dock.Avalonia.Controls.ToolChromeControl.IsMaximizedProperty, value);
public static Style<T> IsMaximized<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.ToolChromeControl
=> style._addSetter(Dock.Avalonia.Controls.ToolChromeControl.IsMaximizedProperty, binding);
}

