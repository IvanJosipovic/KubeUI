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
public static Style<T> Source<T>(this Style<T> style, Avalonia.Media.IImage value) where T : Ursa.Controls.Avatar
=> style._addSetter(Ursa.Controls.Avatar.SourceProperty, value);
public static Style<T> Source<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Avatar
=> style._addSetter(Ursa.Controls.Avatar.SourceProperty, binding);
public static Style<T> HoverMask<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.Avatar
=> style._addSetter(Ursa.Controls.Avatar.HoverMaskProperty, value);
public static Style<T> HoverMask<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Avatar
=> style._addSetter(Ursa.Controls.Avatar.HoverMaskProperty, binding);
}

