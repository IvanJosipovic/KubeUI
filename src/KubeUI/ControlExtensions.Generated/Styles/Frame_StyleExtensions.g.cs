using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using Frame = FluentAvalonia.UI.Controls.Frame;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class FrameExtensions
{
public static Style<T> SourcePageType<T>(this Style<T> style, System.Type value) where T : FluentAvalonia.UI.Controls.Frame
=> style._addSetter(FluentAvalonia.UI.Controls.Frame.SourcePageTypeProperty, value);
public static Style<T> SourcePageType<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.Frame
=> style._addSetter(FluentAvalonia.UI.Controls.Frame.SourcePageTypeProperty, binding);
public static Style<T> CacheSize<T>(this Style<T> style, System.Int32 value) where T : FluentAvalonia.UI.Controls.Frame
=> style._addSetter(FluentAvalonia.UI.Controls.Frame.CacheSizeProperty, value);
public static Style<T> CacheSize<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.Frame
=> style._addSetter(FluentAvalonia.UI.Controls.Frame.CacheSizeProperty, binding);
public static Style<T> IsNavigationStackEnabled<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.Frame
=> style._addSetter(FluentAvalonia.UI.Controls.Frame.IsNavigationStackEnabledProperty, value);
public static Style<T> IsNavigationStackEnabled<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.Frame
=> style._addSetter(FluentAvalonia.UI.Controls.Frame.IsNavigationStackEnabledProperty, binding);
}

