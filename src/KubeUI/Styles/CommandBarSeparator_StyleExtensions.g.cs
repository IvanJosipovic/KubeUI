using Avalonia.Data;
using Avalonia.Data.Converters;
using CommandBarSeparator = FluentAvalonia.UI.Controls.CommandBarSeparator;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class CommandBarSeparatorExtensions
{
public static Style<T> IsCompact<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.CommandBarSeparator
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBarSeparator.IsCompactProperty, value);
public static Style<T> IsCompact<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBarSeparator
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBarSeparator.IsCompactProperty, binding);
}

