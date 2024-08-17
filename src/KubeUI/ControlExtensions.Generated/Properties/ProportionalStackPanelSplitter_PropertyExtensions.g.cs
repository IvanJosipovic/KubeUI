#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Dock.Avalonia.Controls;
using ProportionalStackPanelSplitter = Dock.Avalonia.Controls.ProportionalStackPanelSplitter;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class ProportionalStackPanelSplitterExtensions
{
public static T Thickness<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.ProportionalStackPanelSplitter
   => control._set(Dock.Avalonia.Controls.ProportionalStackPanelSplitter.ThicknessProperty, binding);
public static T Thickness<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.ProportionalStackPanelSplitter
   => control._set(Dock.Avalonia.Controls.ProportionalStackPanelSplitter.ThicknessProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Thickness<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.ProportionalStackPanelSplitter
   => control._set(Dock.Avalonia.Controls.ProportionalStackPanelSplitter.ThicknessProperty, func, onChanged, expression);
public static T Thickness<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.ProportionalStackPanelSplitter
=> control._setEx(Dock.Avalonia.Controls.ProportionalStackPanelSplitter.ThicknessProperty, ps, () => control.Thickness = value, bindingMode, converter, bindingSource);
public static T Thickness<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.ProportionalStackPanelSplitter
=> control._setEx(Dock.Avalonia.Controls.ProportionalStackPanelSplitter.ThicknessProperty, ps, () => control.Thickness = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

