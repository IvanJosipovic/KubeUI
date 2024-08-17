#nullable enable
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
public static T Mode<T>(this T control, IBinding binding) where T : Ursa.Controls.TimelinePanel
   => control._set(Ursa.Controls.TimelinePanel.ModeProperty, binding);
public static T Mode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.TimelinePanel
   => control._set(Ursa.Controls.TimelinePanel.ModeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Mode<T>(this T control, Func<Ursa.Controls.TimelineDisplayMode> func, Action<Ursa.Controls.TimelineDisplayMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.TimelinePanel
   => control._set(Ursa.Controls.TimelinePanel.ModeProperty, func, onChanged, expression);
public static T Mode<T>(this T control, Ursa.Controls.TimelineDisplayMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelinePanel
=> control._setEx(Ursa.Controls.TimelinePanel.ModeProperty, ps, () => control.Mode = value, bindingMode, converter, bindingSource);
public static T Mode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Ursa.Controls.TimelineDisplayMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.TimelinePanel
=> control._setEx(Ursa.Controls.TimelinePanel.ModeProperty, ps, () => control.Mode = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

