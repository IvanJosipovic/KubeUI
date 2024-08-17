using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TimelinePanel = Ursa.Controls.TimelinePanel;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class TimelinePanelExtensions
{
public static Style<T> Mode<T>(this Style<T> style, Ursa.Controls.TimelineDisplayMode value) where T : Ursa.Controls.TimelinePanel
=> style._addSetter(Ursa.Controls.TimelinePanel.ModeProperty, value);
public static Style<T> Mode<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimelinePanel
=> style._addSetter(Ursa.Controls.TimelinePanel.ModeProperty, binding);
}

