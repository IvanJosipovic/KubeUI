using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using ImageIcon = FluentAvalonia.UI.Controls.ImageIcon;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class ImageIconExtensions
{
public static Style<T> Source<T>(this Style<T> style, Avalonia.Media.IImage value) where T : FluentAvalonia.UI.Controls.ImageIcon
=> style._addSetter(FluentAvalonia.UI.Controls.ImageIcon.SourceProperty, value);
public static Style<T> Source<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ImageIcon
=> style._addSetter(FluentAvalonia.UI.Controls.ImageIcon.SourceProperty, binding);
}

