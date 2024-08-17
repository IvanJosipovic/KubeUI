using Avalonia.Data;
using Avalonia.Data.Converters;
using BitmapIcon = FluentAvalonia.UI.Controls.BitmapIcon;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class BitmapIconExtensions
{
public static Style<T> UriSource<T>(this Style<T> style, System.Uri value) where T : FluentAvalonia.UI.Controls.BitmapIcon
=> style._addSetter(FluentAvalonia.UI.Controls.BitmapIcon.UriSourceProperty, value);
public static Style<T> UriSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.BitmapIcon
=> style._addSetter(FluentAvalonia.UI.Controls.BitmapIcon.UriSourceProperty, binding);
public static Style<T> ShowAsMonochrome<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.BitmapIcon
=> style._addSetter(FluentAvalonia.UI.Controls.BitmapIcon.ShowAsMonochromeProperty, value);
public static Style<T> ShowAsMonochrome<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.BitmapIcon
=> style._addSetter(FluentAvalonia.UI.Controls.BitmapIcon.ShowAsMonochromeProperty, binding);
}

