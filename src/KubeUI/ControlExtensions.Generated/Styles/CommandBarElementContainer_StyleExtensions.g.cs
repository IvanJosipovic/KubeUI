using Avalonia.Data;
using Avalonia.Data.Converters;
using CommandBarElementContainer = FluentAvalonia.UI.Controls.CommandBarElementContainer;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class CommandBarElementContainerExtensions
{
public static Style<T> IsCompact<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.CommandBarElementContainer
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBarElementContainer.IsCompactProperty, value);
public static Style<T> IsCompact<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBarElementContainer
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBarElementContainer.IsCompactProperty, binding);
}

