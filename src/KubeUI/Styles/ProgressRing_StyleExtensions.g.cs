using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using ProgressRing = FluentAvalonia.UI.Controls.ProgressRing;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class ProgressRingExtensions
{
public static Style<T> IsActive<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.ProgressRing
=> style._addSetter(FluentAvalonia.UI.Controls.ProgressRing.IsActiveProperty, value);
public static Style<T> IsActive<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ProgressRing
=> style._addSetter(FluentAvalonia.UI.Controls.ProgressRing.IsActiveProperty, binding);
public static Style<T> IsIndeterminate<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.ProgressRing
=> style._addSetter(FluentAvalonia.UI.Controls.ProgressRing.IsIndeterminateProperty, value);
public static Style<T> IsIndeterminate<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ProgressRing
=> style._addSetter(FluentAvalonia.UI.Controls.ProgressRing.IsIndeterminateProperty, binding);
}

