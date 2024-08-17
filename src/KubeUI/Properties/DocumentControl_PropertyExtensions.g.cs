#nullable enable
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Dock.Avalonia.Controls;
using DocumentControl = Dock.Avalonia.Controls.DocumentControl;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class DocumentControlExtensions
{
public static T HeaderTemplate<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.DocumentControl
   => control._set(Dock.Avalonia.Controls.DocumentControl.HeaderTemplateProperty, binding);
public static T HeaderTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.DocumentControl
   => control._set(Dock.Avalonia.Controls.DocumentControl.HeaderTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T HeaderTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.DocumentControl
   => control._set(Dock.Avalonia.Controls.DocumentControl.HeaderTemplateProperty, func, onChanged, expression);
public static T HeaderTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DocumentControl
=> control._setEx(Dock.Avalonia.Controls.DocumentControl.HeaderTemplateProperty, ps, () => control.HeaderTemplate = value, bindingMode, converter, bindingSource);
public static T HeaderTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DocumentControl
=> control._setEx(Dock.Avalonia.Controls.DocumentControl.HeaderTemplateProperty, ps, () => control.HeaderTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsActive<T>(this T control, IBinding binding) where T : Dock.Avalonia.Controls.DocumentControl
   => control._set(Dock.Avalonia.Controls.DocumentControl.IsActiveProperty, binding);
public static T IsActive<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Dock.Avalonia.Controls.DocumentControl
   => control._set(Dock.Avalonia.Controls.DocumentControl.IsActiveProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsActive<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Dock.Avalonia.Controls.DocumentControl
   => control._set(Dock.Avalonia.Controls.DocumentControl.IsActiveProperty, func, onChanged, expression);
public static T IsActive<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DocumentControl
=> control._setEx(Dock.Avalonia.Controls.DocumentControl.IsActiveProperty, ps, () => control.IsActive = value, bindingMode, converter, bindingSource);
public static T IsActive<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Dock.Avalonia.Controls.DocumentControl
=> control._setEx(Dock.Avalonia.Controls.DocumentControl.IsActiveProperty, ps, () => control.IsActive = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

