#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using ImageIconSource = FluentAvalonia.UI.Controls.ImageIconSource;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class ImageIconSourceExtensions
{
public static T Source<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.ImageIconSource
   => control._set(FluentAvalonia.UI.Controls.ImageIconSource.SourceProperty, binding);
public static T Source<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.ImageIconSource
   => control._set(FluentAvalonia.UI.Controls.ImageIconSource.SourceProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Source<T>(this T control, Func<Avalonia.Media.IImage> func, Action<Avalonia.Media.IImage>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.ImageIconSource
   => control._set(FluentAvalonia.UI.Controls.ImageIconSource.SourceProperty, func, onChanged, expression);
public static T Source<T>(this T control, Avalonia.Media.IImage value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ImageIconSource
=> control._setEx(FluentAvalonia.UI.Controls.ImageIconSource.SourceProperty, ps, () => control.Source = value, bindingMode, converter, bindingSource);
public static T Source<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IImage> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.ImageIconSource
=> control._setEx(FluentAvalonia.UI.Controls.ImageIconSource.SourceProperty, ps, () => control.Source = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

