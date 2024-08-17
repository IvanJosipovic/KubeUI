#nullable enable
using AbstractMargin = AvaloniaEdit.Editing.AbstractMargin;
using Avalonia.Data;
using Avalonia.Data.Converters;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class AbstractMarginExtensions
{
public static T TextView<T>(this T control, IBinding binding) where T : AvaloniaEdit.Editing.AbstractMargin
   => control._set(AvaloniaEdit.Editing.AbstractMargin.TextViewProperty, binding);
public static T TextView<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.Editing.AbstractMargin
   => control._set(AvaloniaEdit.Editing.AbstractMargin.TextViewProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T TextView<T>(this T control, Func<AvaloniaEdit.Rendering.TextView> func, Action<AvaloniaEdit.Rendering.TextView>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.Editing.AbstractMargin
   => control._set(AvaloniaEdit.Editing.AbstractMargin.TextViewProperty, func, onChanged, expression);
public static T TextView<T>(this T control, AvaloniaEdit.Rendering.TextView value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Editing.AbstractMargin
=> control._setEx(AvaloniaEdit.Editing.AbstractMargin.TextViewProperty, ps, () => control.TextView = value, bindingMode, converter, bindingSource);
public static T TextView<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, AvaloniaEdit.Rendering.TextView> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.Editing.AbstractMargin
=> control._setEx(AvaloniaEdit.Editing.AbstractMargin.TextViewProperty, ps, () => control.TextView = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

