using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using IconSourceElement = FluentAvalonia.UI.Controls.IconSourceElement;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class IconSourceElementExtensions
{
public static Style<T> IconSource<T>(this Style<T> style, FluentAvalonia.UI.Controls.IconSource value) where T : FluentAvalonia.UI.Controls.IconSourceElement
=> style._addSetter(FluentAvalonia.UI.Controls.IconSourceElement.IconSourceProperty, value);
public static Style<T> IconSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.IconSourceElement
=> style._addSetter(FluentAvalonia.UI.Controls.IconSourceElement.IconSourceProperty, binding);
}

