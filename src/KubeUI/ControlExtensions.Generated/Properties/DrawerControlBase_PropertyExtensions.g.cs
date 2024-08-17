#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using DrawerControlBase = Ursa.Controls.DrawerControlBase;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Common;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class DrawerControlBaseExtensions
{
public static T Position<T>(this T control, IBinding binding) where T : Ursa.Controls.DrawerControlBase
   => control._set(Ursa.Controls.DrawerControlBase.PositionProperty, binding);
public static T Position<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DrawerControlBase
   => control._set(Ursa.Controls.DrawerControlBase.PositionProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Position<T>(this T control, Func<Ursa.Common.Position> func, Action<Ursa.Common.Position>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DrawerControlBase
   => control._set(Ursa.Controls.DrawerControlBase.PositionProperty, func, onChanged, expression);
public static T Position<T>(this T control, Ursa.Common.Position value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DrawerControlBase
=> control._setEx(Ursa.Controls.DrawerControlBase.PositionProperty, ps, () => control.Position = value, bindingMode, converter, bindingSource);
public static T Position<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Ursa.Common.Position> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DrawerControlBase
=> control._setEx(Ursa.Controls.DrawerControlBase.PositionProperty, ps, () => control.Position = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsOpen<T>(this T control, IBinding binding) where T : Ursa.Controls.DrawerControlBase
   => control._set(Ursa.Controls.DrawerControlBase.IsOpenProperty, binding);
public static T IsOpen<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DrawerControlBase
   => control._set(Ursa.Controls.DrawerControlBase.IsOpenProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsOpen<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DrawerControlBase
   => control._set(Ursa.Controls.DrawerControlBase.IsOpenProperty, func, onChanged, expression);
public static T IsOpen<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DrawerControlBase
=> control._setEx(Ursa.Controls.DrawerControlBase.IsOpenProperty, ps, () => control.IsOpen = value, bindingMode, converter, bindingSource);
public static T IsOpen<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DrawerControlBase
=> control._setEx(Ursa.Controls.DrawerControlBase.IsOpenProperty, ps, () => control.IsOpen = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsCloseButtonVisible<T>(this T control, IBinding binding) where T : Ursa.Controls.DrawerControlBase
   => control._set(Ursa.Controls.DrawerControlBase.IsCloseButtonVisibleProperty, binding);
public static T IsCloseButtonVisible<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.DrawerControlBase
   => control._set(Ursa.Controls.DrawerControlBase.IsCloseButtonVisibleProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsCloseButtonVisible<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.DrawerControlBase
   => control._set(Ursa.Controls.DrawerControlBase.IsCloseButtonVisibleProperty, func, onChanged, expression);
public static T IsCloseButtonVisible<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DrawerControlBase
=> control._setEx(Ursa.Controls.DrawerControlBase.IsCloseButtonVisibleProperty, ps, () => control.IsCloseButtonVisible = value, bindingMode, converter, bindingSource);
public static T IsCloseButtonVisible<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.DrawerControlBase
=> control._setEx(Ursa.Controls.DrawerControlBase.IsCloseButtonVisibleProperty, ps, () => control.IsCloseButtonVisible = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

