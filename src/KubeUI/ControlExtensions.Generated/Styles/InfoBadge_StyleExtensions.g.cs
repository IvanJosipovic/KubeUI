using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using InfoBadge = FluentAvalonia.UI.Controls.InfoBadge;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class InfoBadgeExtensions
{
public static Style<T> Value<T>(this Style<T> style, System.Int32 value) where T : FluentAvalonia.UI.Controls.InfoBadge
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBadge.ValueProperty, value);
public static Style<T> Value<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBadge
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBadge.ValueProperty, binding);
public static Style<T> IconSource<T>(this Style<T> style, FluentAvalonia.UI.Controls.IconSource value) where T : FluentAvalonia.UI.Controls.InfoBadge
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBadge.IconSourceProperty, value);
public static Style<T> IconSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBadge
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBadge.IconSourceProperty, binding);
}

