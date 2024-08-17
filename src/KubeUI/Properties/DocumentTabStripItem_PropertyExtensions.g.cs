#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Dock.Avalonia.Controls;
using DocumentTabStripItem = Dock.Avalonia.Controls.DocumentTabStripItem;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class DocumentTabStripItemExtensions
{
public static T IsActive<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.DocumentTabStripItem
   => control._set(Dock.Avalonia.Controls.DocumentTabStripItem.IsActiveProperty, binding);
public static T IsActive<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.DocumentTabStripItem
   => control._set(Dock.Avalonia.Controls.DocumentTabStripItem.IsActiveProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsActive<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.DocumentTabStripItem
   => control._set(Dock.Avalonia.Controls.DocumentTabStripItem.IsActiveProperty, func, onChanged, expression);
public static T IsActive<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DocumentTabStripItem
=> control._setEx(Dock.Avalonia.Controls.DocumentTabStripItem.IsActiveProperty, ps, () => control.IsActive = value, bindingMode, converter, bindingSource);
public static T IsActive<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DocumentTabStripItem
=> control._setEx(Dock.Avalonia.Controls.DocumentTabStripItem.IsActiveProperty, ps, () => control.IsActive = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

