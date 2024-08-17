using AppWindow = FluentAvalonia.UI.Windowing.AppWindow;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using FluentAvalonia.UI.Windowing;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class AppWindowExtensions
{
public static Style<T> Icon<T>(this Style<T> style, Avalonia.Media.IImage value) where T : FluentAvalonia.UI.Windowing.AppWindow
=> style._addSetter(FluentAvalonia.UI.Windowing.AppWindow.IconProperty, value);
public static Style<T> Icon<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Windowing.AppWindow
=> style._addSetter(FluentAvalonia.UI.Windowing.AppWindow.IconProperty, binding);
}

