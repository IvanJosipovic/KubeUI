using Avalonia.Data;
using Avalonia.Data.Converters;
using Dock.Avalonia.Controls;
using DocumentTabStripItem = Dock.Avalonia.Controls.DocumentTabStripItem;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class DocumentTabStripItemExtensions
{
public static Style<T> IsActive<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.DocumentTabStripItem
=> style._addSetter(Dock.Avalonia.Controls.DocumentTabStripItem.IsActiveProperty, value);
public static Style<T> IsActive<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DocumentTabStripItem
=> style._addSetter(Dock.Avalonia.Controls.DocumentTabStripItem.IsActiveProperty, binding);
}

