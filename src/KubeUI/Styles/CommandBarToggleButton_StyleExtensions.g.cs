using Avalonia.Data;
using Avalonia.Data.Converters;
using CommandBarToggleButton = FluentAvalonia.UI.Controls.CommandBarToggleButton;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class CommandBarToggleButtonExtensions
{
public static Style<T> IconSource<T>(this Style<T> style, FluentAvalonia.UI.Controls.IconSource value) where T : FluentAvalonia.UI.Controls.CommandBarToggleButton
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBarToggleButton.IconSourceProperty, value);
public static Style<T> IconSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBarToggleButton
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBarToggleButton.IconSourceProperty, binding);
public static Style<T> Label<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.CommandBarToggleButton
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBarToggleButton.LabelProperty, value);
public static Style<T> Label<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBarToggleButton
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBarToggleButton.LabelProperty, binding);
public static Style<T> IsCompact<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.CommandBarToggleButton
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBarToggleButton.IsCompactProperty, value);
public static Style<T> IsCompact<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.CommandBarToggleButton
=> style._addSetter(FluentAvalonia.UI.Controls.CommandBarToggleButton.IsCompactProperty, binding);
}

