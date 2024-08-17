using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Dock.Avalonia.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using ToolControl = Dock.Avalonia.Controls.ToolControl;

namespace Avalonia.Markup.Declarative;
public static partial class ToolControlExtensions
{
public static Style<T> HeaderTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : Dock.Avalonia.Controls.ToolControl
=> style._addSetter(Dock.Avalonia.Controls.ToolControl.HeaderTemplateProperty, value);
public static Style<T> HeaderTemplate<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.ToolControl
=> style._addSetter(Dock.Avalonia.Controls.ToolControl.HeaderTemplateProperty, binding);
}

