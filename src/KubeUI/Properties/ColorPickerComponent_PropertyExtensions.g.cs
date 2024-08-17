#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using ColorPickerComponent = FluentAvalonia.UI.Controls.ColorPickerComponent;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class ColorPickerComponentExtensions
{
public static T Color<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerComponent
   => control._set(FluentAvalonia.UI.Controls.ColorPickerComponent.ColorProperty, binding);
public static T Color<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPickerComponent
   => control._set(FluentAvalonia.UI.Controls.ColorPickerComponent.ColorProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Color<T>(this T control, Func<FluentAvalonia.UI.Media.Color2> func, Action<FluentAvalonia.UI.Media.Color2>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPickerComponent
   => control._set(FluentAvalonia.UI.Controls.ColorPickerComponent.ColorProperty, func, onChanged, expression);
public static T Color<T>(this T control, FluentAvalonia.UI.Media.Color2 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerComponent
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerComponent.ColorProperty, ps, () => control.Color = value, bindingMode, converter, bindingSource);
public static T Color<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Media.Color2> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerComponent
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerComponent.ColorProperty, ps, () => control.Color = converter.TryConvert(value), bindingMode, converter, bindingSource);

public static T Color<T>(this T control, Byte r = default, Byte g = default, Byte b = default, Byte a = default) where T : FluentAvalonia.UI.Controls.ColorPickerComponent
   => control._set(() => control.Color = new FluentAvalonia.UI.Media.Color2(r, g, b, a));
public static T Color<T>(this T control, Color avColor = default) where T : FluentAvalonia.UI.Controls.ColorPickerComponent
   => control._set(() => control.Color = new FluentAvalonia.UI.Media.Color2(avColor));
public static T Component<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ColorPickerComponent
   => control._set(FluentAvalonia.UI.Controls.ColorPickerComponent.ComponentProperty, binding);
public static T Component<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ColorPickerComponent
   => control._set(FluentAvalonia.UI.Controls.ColorPickerComponent.ComponentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Component<T>(this T control, Func<FluentAvalonia.UI.Controls.ColorComponent> func, Action<FluentAvalonia.UI.Controls.ColorComponent>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ColorPickerComponent
   => control._set(FluentAvalonia.UI.Controls.ColorPickerComponent.ComponentProperty, func, onChanged, expression);
public static T Component<T>(this T control, FluentAvalonia.UI.Controls.ColorComponent value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerComponent
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerComponent.ComponentProperty, ps, () => control.Component = value, bindingMode, converter, bindingSource);
public static T Component<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.ColorComponent> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ColorPickerComponent
=> control._setEx(FluentAvalonia.UI.Controls.ColorPickerComponent.ComponentProperty, ps, () => control.Component = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

