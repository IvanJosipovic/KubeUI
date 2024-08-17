#nullable enable
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using ControlRecyclingDataTemplate = Dock.Avalonia.Controls.Recycling.ControlRecyclingDataTemplate;
using Dock.Avalonia.Controls.Recycling;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class ControlRecyclingDataTemplateExtensions
{
public static T Parent<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.Recycling.ControlRecyclingDataTemplate
   => control._set(Dock.Avalonia.Controls.Recycling.ControlRecyclingDataTemplate.ParentProperty, binding);
public static T Parent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.Recycling.ControlRecyclingDataTemplate
   => control._set(Dock.Avalonia.Controls.Recycling.ControlRecyclingDataTemplate.ParentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Parent<T>(this T control, Func<Avalonia.Controls.Control> func, Action<Avalonia.Controls.Control>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.Recycling.ControlRecyclingDataTemplate
   => control._set(Dock.Avalonia.Controls.Recycling.ControlRecyclingDataTemplate.ParentProperty, func, onChanged, expression);
public static T Parent<T>(this T control, Avalonia.Controls.Control value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.Recycling.ControlRecyclingDataTemplate
=> control._setEx(Dock.Avalonia.Controls.Recycling.ControlRecyclingDataTemplate.ParentProperty, ps, () => control.Parent = value, bindingMode, converter, bindingSource);
public static T Parent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Control> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.Recycling.ControlRecyclingDataTemplate
=> control._setEx(Dock.Avalonia.Controls.Recycling.ControlRecyclingDataTemplate.ParentProperty, ps, () => control.Parent = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

