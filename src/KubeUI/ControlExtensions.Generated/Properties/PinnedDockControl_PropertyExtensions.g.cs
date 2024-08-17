#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using PinnedDockControl = Dock.Avalonia.Controls.PinnedDockControl;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class PinnedDockControlExtensions
{
public static T PinnedDockAlignment<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.PinnedDockControl
   => control._set(Dock.Avalonia.Controls.PinnedDockControl.PinnedDockAlignmentProperty, binding);
public static T PinnedDockAlignment<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.PinnedDockControl
   => control._set(Dock.Avalonia.Controls.PinnedDockControl.PinnedDockAlignmentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T PinnedDockAlignment<T>(this T control, Func<Dock.Model.Core.Alignment> func, Action<Dock.Model.Core.Alignment>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.PinnedDockControl
   => control._set(Dock.Avalonia.Controls.PinnedDockControl.PinnedDockAlignmentProperty, func, onChanged, expression);
public static T PinnedDockAlignment<T>(this T control, Dock.Model.Core.Alignment value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.PinnedDockControl
=> control._setEx(Dock.Avalonia.Controls.PinnedDockControl.PinnedDockAlignmentProperty, ps, () => control.PinnedDockAlignment = value, bindingMode, converter, bindingSource);
public static T PinnedDockAlignment<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Dock.Model.Core.Alignment> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.PinnedDockControl
=> control._setEx(Dock.Avalonia.Controls.PinnedDockControl.PinnedDockAlignmentProperty, ps, () => control.PinnedDockAlignment = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

