#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avatar = Ursa.Controls.Avatar;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class AvatarExtensions
{
public static T Source<T>(this T control, IBinding binding) where T : Ursa.Controls.Avatar
   => control._set(Ursa.Controls.Avatar.SourceProperty, binding);
public static T Source<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Avatar
   => control._set(Ursa.Controls.Avatar.SourceProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Source<T>(this T control, Func<Avalonia.Media.IImage> func, Action<Avalonia.Media.IImage>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Avatar
   => control._set(Ursa.Controls.Avatar.SourceProperty, func, onChanged, expression);
public static T Source<T>(this T control, Avalonia.Media.IImage value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Avatar
=> control._setEx(Ursa.Controls.Avatar.SourceProperty, ps, () => control.Source = value, bindingMode, converter, bindingSource);
public static T Source<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IImage> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Avatar
=> control._setEx(Ursa.Controls.Avatar.SourceProperty, ps, () => control.Source = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T HoverMask<T>(this T control, IBinding binding) where T : Ursa.Controls.Avatar
   => control._set(Ursa.Controls.Avatar.HoverMaskProperty, binding);
public static T HoverMask<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Avatar
   => control._set(Ursa.Controls.Avatar.HoverMaskProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T HoverMask<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Avatar
   => control._set(Ursa.Controls.Avatar.HoverMaskProperty, func, onChanged, expression);
public static T HoverMask<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Avatar
=> control._setEx(Ursa.Controls.Avatar.HoverMaskProperty, ps, () => control.HoverMask = value, bindingMode, converter, bindingSource);
public static T HoverMask<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Avatar
=> control._setEx(Ursa.Controls.Avatar.HoverMaskProperty, ps, () => control.HoverMask = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

