using Avalonia.Data;
using Avalonia.Data.Converters;
using Dock.Avalonia.Controls;
using DocumentTabStrip = Dock.Avalonia.Controls.DocumentTabStrip;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class DocumentTabStripExtensions
{
public static Style<T> CanCreateItem<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.DocumentTabStrip
=> style._addSetter(Dock.Avalonia.Controls.DocumentTabStrip.CanCreateItemProperty, value);
public static Style<T> CanCreateItem<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DocumentTabStrip
=> style._addSetter(Dock.Avalonia.Controls.DocumentTabStrip.CanCreateItemProperty, binding);
public static Style<T> IsActive<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.DocumentTabStrip
=> style._addSetter(Dock.Avalonia.Controls.DocumentTabStrip.IsActiveProperty, value);
public static Style<T> IsActive<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DocumentTabStrip
=> style._addSetter(Dock.Avalonia.Controls.DocumentTabStrip.IsActiveProperty, binding);
}

