using Avalonia.Data;
using Avalonia.Data.Converters;
using CommandBarButton = FluentAvalonia.UI.Controls.CommandBarButton;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class CommandBarButtonExtensions
{
public static Style<T> IconSource<T>(this Style<T> style, FluentAvalonia.UI.Controls.IconSource value) where T : FluentAvalonia.UI.Controls.CommandBarButton
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBarButton.IconSourceProperty, value);
public static Style<T> IconSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBarButton
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBarButton.IconSourceProperty, binding);
public static Style<T> Label<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.CommandBarButton
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBarButton.LabelProperty, value);
public static Style<T> Label<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBarButton
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBarButton.LabelProperty, binding);
public static Style<T> IsCompact<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.CommandBarButton
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBarButton.IsCompactProperty, value);
public static Style<T> IsCompact<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBarButton
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBarButton.IsCompactProperty, binding);
}

