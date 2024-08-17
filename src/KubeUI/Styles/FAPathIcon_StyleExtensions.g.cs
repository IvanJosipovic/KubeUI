using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using FAPathIcon = FluentAvalonia.UI.Controls.FAPathIcon;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class FAPathIconExtensions
{
public static Style<T> Data<T>(this Style<T> style, Avalonia.Media.Geometry value) where T : FluentAvalonia.UI.Controls.FAPathIcon
=> style._addSetter(FluentAvalonia.UI.Controls.FAPathIcon.DataProperty, value);
public static Style<T> Data<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAPathIcon
=> style._addSetter(FluentAvalonia.UI.Controls.FAPathIcon.DataProperty, binding);
public static Style<T> Stretch<T>(this Style<T> style, Avalonia.Media.Stretch value) where T : FluentAvalonia.UI.Controls.FAPathIcon
=> style._addSetter(FluentAvalonia.UI.Controls.FAPathIcon.StretchProperty, value);
public static Style<T> Stretch<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAPathIcon
=> style._addSetter(FluentAvalonia.UI.Controls.FAPathIcon.StretchProperty, binding);
public static Style<T> StretchDirection<T>(this Style<T> style, Avalonia.Media.StretchDirection value) where T : FluentAvalonia.UI.Controls.FAPathIcon
=> style._addSetter(FluentAvalonia.UI.Controls.FAPathIcon.StretchDirectionProperty, value);
public static Style<T> StretchDirection<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAPathIcon
=> style._addSetter(FluentAvalonia.UI.Controls.FAPathIcon.StretchDirectionProperty, binding);
}

