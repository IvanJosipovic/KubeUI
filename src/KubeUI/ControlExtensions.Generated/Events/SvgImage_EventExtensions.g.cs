using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Svg.Skia;
using SvgImage = Avalonia.Svg.Skia.SvgImage;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class SvgImageEventsExtensions
{
    public static T OnInvalidated<T>(this T control, Action<System.Object, System.EventArgs> action) where T : Avalonia.Svg.Skia.SvgImage => 
        control._setEvent((System.EventHandler) ((arg0, arg1) => action(arg0, arg1)), h => control.Invalidated += h);
}

