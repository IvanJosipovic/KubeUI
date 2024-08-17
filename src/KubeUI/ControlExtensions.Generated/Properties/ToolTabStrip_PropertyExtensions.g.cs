#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Dock.Avalonia.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using ToolTabStrip = Dock.Avalonia.Controls.ToolTabStrip;

namespace Avalonia.Markup.Declarative;
public static partial class ToolTabStripExtensions
{
public static T CanCreateItem<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.ToolTabStrip
   => control._set(Dock.Avalonia.Controls.ToolTabStrip.CanCreateItemProperty, binding);
public static T CanCreateItem<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.ToolTabStrip
   => control._set(Dock.Avalonia.Controls.ToolTabStrip.CanCreateItemProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T CanCreateItem<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.ToolTabStrip
   => control._set(Dock.Avalonia.Controls.ToolTabStrip.CanCreateItemProperty, func, onChanged, expression);
public static T CanCreateItem<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.ToolTabStrip
=> control._setEx(Dock.Avalonia.Controls.ToolTabStrip.CanCreateItemProperty, ps, () => control.CanCreateItem = value, bindingMode, converter, bindingSource);
public static T CanCreateItem<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.ToolTabStrip
=> control._setEx(Dock.Avalonia.Controls.ToolTabStrip.CanCreateItemProperty, ps, () => control.CanCreateItem = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

