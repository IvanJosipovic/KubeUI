using AbstractMargin = AvaloniaEdit.Editing.AbstractMargin;
using Avalonia.Data;
using Avalonia.Data.Converters;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class AbstractMarginExtensions
{
public static Style<T> TextView<T>(this Style<T> style, AvaloniaEdit.Rendering.TextView value) where T : AvaloniaEdit.Editing.AbstractMargin
=> style._addSetter(AvaloniaEdit.Editing.AbstractMargin.TextViewProperty, value);
public static Style<T> TextView<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.Editing.AbstractMargin
=> style._addSetter(AvaloniaEdit.Editing.AbstractMargin.TextViewProperty, binding);
}

